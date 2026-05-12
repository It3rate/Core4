#include "gojo_internal.hpp"

namespace gojo::simd {

bool World::tryReadOperationFrom(RelationRef operation, ElementRef input, CarrierRef reader, RelationRef& read)
{
    const auto& row = relations_.at(operation.index);
    std::vector<OperationOperand> operands;
    if (!tryCollectOperationOperands(row.left, input, reader, operands) || operands.empty())
    {
        return false;
    }

    CarrierFrame pointFrame;
    if (tryReadPointFrame(row.right, pointFrame))
    {
        CarrierFrame targetFrame;
        if (!tryResolveOperationTargetFrame(pointFrame, reader, false, targetFrame) || targetFrame.extent() == 0)
        {
            return false;
        }

        const auto result = std::accumulate(
            operands.begin(),
            operands.end(),
            std::int64_t{0},
            [](std::int64_t total, const OperationOperand& operand)
            {
                return saturatingAdd(total, operand.ticks);
            });
        read = operationResult(targetFrame, result, operands);
        return true;
    }

    CarrierFrame segmentFrame;
    if (tryReadFrame(row.right, segmentFrame))
    {
        CarrierFrame targetFrame;
        if (!tryResolveOperationTargetFrame(segmentFrame, reader, false, targetFrame) || targetFrame.extent() == 0)
        {
            return false;
        }

        const auto result = std::accumulate(
            operands.begin(),
            operands.end(),
            std::int64_t{1},
            [](std::int64_t total, const OperationOperand& operand)
            {
                return saturatingMultiply(total, operand.ticks);
            });
        read = operationResult(targetFrame, result, operands);
        return true;
    }

    return false;
}


bool World::tryReadPortSurface(ElementRef ref, RelationRef& port) const
{
    if (ref.store != ElementStore::relation)
    {
        return false;
    }

    CarrierFrame ignored;
    const auto& row = relations_.at(ref.index);
    if (!tryReadFrame(ref, ignored) && tryReadFrame(row.left, ignored) && tryReadFrame(row.right, ignored))
    {
        port = RelationRef{ref.index};
        return true;
    }

    return false;
}

bool World::tryCollectOperationOperands(ElementRef portSurface, ElementRef input, CarrierRef reader, std::vector<OperationOperand>& operands)
{
    RelationRef port;
    if (tryReadPortSurface(portSurface, port))
    {
        return tryCollectOperationOperand(port, input, reader, operands);
    }

    if (input.store == ElementStore::relation && portSurface.store == ElementStore::relation)
    {
        const auto& inputGroup = relations_.at(input.index);
        const auto& portGroup = relations_.at(portSurface.index);
        return tryCollectOperationOperands(portGroup.left, inputGroup.left, reader, operands) &&
            tryCollectOperationOperands(portGroup.right, inputGroup.right, reader, operands);
    }

    return false;
}

bool World::tryCollectOperationOperand(RelationRef port, ElementRef input, CarrierRef reader, std::vector<OperationOperand>& operands)
{
    if (input.store == ElementStore::carrier_location)
    {
        return tryCollectOperationOperand(port, CarrierLocationRef{input.index}, reader, operands);
    }

    CarrierLocationRef atomicInput;
    const auto& portRow = relations_.at(port.index);
    if (input.store == ElementStore::relation && tryReadAtomicFrameInput(input, portRow.left, atomicInput))
    {
        return tryCollectOperationOperand(port, atomicInput, reader, operands);
    }

    return false;
}

bool World::tryCollectOperationOperand(RelationRef port, CarrierLocationRef input, CarrierRef reader, std::vector<OperationOperand>& operands)
{
    const auto& row = relations_.at(port.index);
    SurfaceProjection projection;
    if (!tryProjectOperationOperandThrough(row.left, row.right, input, reader, projection))
    {
        return false;
    }

    operands.push_back(OperationOperand{locationView(projection.target).ticks, projection.residue});
    return true;
}

bool World::tryProjectOperationOperandThrough(ElementRef sourceSurface, ElementRef targetSurface, CarrierLocationRef input, CarrierRef reader, SurfaceProjection& projection)
{
    if (tryProjectThrough(sourceSurface, targetSurface, input, reader, projection))
    {
        return true;
    }

    CarrierFrame sourceFrame;
    CarrierFrame targetFrame;
    if (!tryReadFrame(sourceSurface, sourceFrame) || !tryReadFrame(targetSurface, targetFrame))
    {
        return false;
    }

    const auto inputCarrier = locationView(input).carrier;
    if (frameAcceptsProjectionSource(sourceFrame, inputCarrier))
    {
        projection = projectOperationOperandBetween(
            input,
            reader,
            resolveProjectionSourceFrame(sourceFrame, inputCarrier),
            targetFrame);
        return !projection.target.isNone();
    }

    if (!isUnknown(targetFrame.carrier) && frameAcceptsProjectionSource(targetFrame, inputCarrier))
    {
        projection = projectOperationOperandBetween(
            input,
            reader,
            resolveProjectionSourceFrame(targetFrame, inputCarrier),
            sourceFrame);
        return !projection.target.isNone();
    }

    return false;
}

World::SurfaceProjection World::projectOperationOperandBetween(CarrierLocationRef input, CarrierRef reader, const CarrierFrame& source, const CarrierFrame& target)
{
    const auto projected = projectBetween(input, reader, source, CarrierFrame{reader, target.start, target.end});
    CarrierLocationRef targetLocation;
    CarrierLocationRef residue;
    if (!tryGetCarrierLocations(projected, targetLocation, residue))
    {
        return SurfaceProjection{};
    }

    return SurfaceProjection{targetLocation, residue};
}

bool World::tryReadAtomicFrameInput(ElementRef input, ElementRef sourceSurface, CarrierLocationRef& inputExtent)
{
    CarrierFrame inputFrame;
    CarrierFrame sourceFrame;
    if (!tryReadFrame(input, inputFrame) || !tryReadFrame(sourceSurface, sourceFrame))
    {
        return false;
    }

    inputExtent = location(inputFrame.carrier, saturatingAdd(sourceFrame.start, inputFrame.extent()));
    return true;
}

bool World::tryResolveOperationTargetFrame(const CarrierFrame& frame, CarrierRef reader, bool bindOutputToReader, CarrierFrame& targetFrame) const
{
    if (bindOutputToReader)
    {
        targetFrame = CarrierFrame{reader, frame.start, frame.end};
        return true;
    }

    if (!frameAccepts(frame, reader))
    {
        return false;
    }

    targetFrame = resolveFrameCarrier(frame, reader);
    return true;
}

RelationRef World::operationResult(const CarrierFrame& targetFrame, std::int64_t result, const std::vector<OperationOperand>& operands, bool scaleByTargetFrame)
{
    const auto offset = scaleByTargetFrame ? saturatingMultiply(result, targetFrame.extent()) : result;
    return relate(
        element(location(targetFrame.carrier, saturatingAdd(targetFrame.start, offset))),
        operationResidue(operands));
}

ElementRef World::operationResidue(const std::vector<OperationOperand>& operands)
{
    if (operands.empty())
    {
        return ElementRef{};
    }

    ElementRef residue = element(operands.back().residue);
    for (std::size_t remaining = operands.size() - 1; remaining-- > 0;)
    {
        residue = element(relate(element(operands[remaining].residue), residue));
    }

    return residue;
}


} // namespace gojo::simd

