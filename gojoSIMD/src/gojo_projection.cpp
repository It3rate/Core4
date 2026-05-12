#include "gojo_internal.hpp"

namespace gojo::simd {

bool World::tryGetCarrierLocations(RelationRef relation, CarrierLocationRef& left, CarrierLocationRef& right) const
{
    const auto& row = relations_.at(relation.index);
    if (row.left.store != ElementStore::carrier_location || row.right.store != ElementStore::carrier_location)
    {
        return false;
    }

    left = CarrierLocationRef{row.left.index};
    right = CarrierLocationRef{row.right.index};
    return true;
}

bool World::tryProject(RelationRef relation, CarrierLocationRef location, CarrierRef reader, RelationRef& projected)
{
    const auto& row = relations_.at(relation.index);
    if (tryProjectBetweenSurfaces(row.left, row.right, location, reader, projected))
    {
        return true;
    }

    projected = projectionResult(this->location(reader, 0), location);
    return false;
}

RelationRef World::project(RelationRef relation, CarrierLocationRef location, CarrierRef reader)
{
    RelationRef projected;
    (void)tryProject(relation, location, reader, projected);
    return projected;
}


bool World::tryReadPoint(ElementRef ref, CarrierLocationRef& location, CarrierRef& carrier) const
{
    if (ref.store != ElementStore::relation)
    {
        return false;
    }

    const auto& relation = relations_.at(ref.index);
    if (relation.left.store != ElementStore::carrier_location || relation.right.store != ElementStore::carrier)
    {
        return false;
    }

    const auto candidateLocation = CarrierLocationRef{relation.left.index};
    const auto candidateCarrier = CarrierRef{relation.right.bits, relation.right.index};
    if (!sameCarrier(locationView(candidateLocation).carrier, candidateCarrier))
    {
        return false;
    }

    location = candidateLocation;
    carrier = candidateCarrier;
    return true;
}

bool World::tryReadFrame(ElementRef ref, CarrierFrame& frame) const
{
    if (ref.store == ElementStore::carrier_location)
    {
        const auto view = locationView(CarrierLocationRef{ref.index});
        frame = CarrierFrame{view.carrier, 0, view.ticks};
        return true;
    }

    if (ref.store != ElementStore::relation)
    {
        return false;
    }

    CarrierLocationRef pointLocation;
    CarrierRef pointCarrier;
    if (tryReadPoint(ref, pointLocation, pointCarrier))
    {
        frame = CarrierFrame{pointCarrier, 0, locationView(pointLocation).ticks};
        return true;
    }

    const auto& relation = relations_.at(ref.index);
    if (relation.left.store == ElementStore::carrier_location && relation.right.store == ElementStore::carrier_location)
    {
        const auto left = locationView(CarrierLocationRef{relation.left.index});
        const auto right = locationView(CarrierLocationRef{relation.right.index});
        if (sameCarrier(left.carrier, right.carrier))
        {
            frame = CarrierFrame{left.carrier, left.ticks, right.ticks};
            return true;
        }
    }

    CarrierLocationRef leftPoint;
    CarrierRef leftCarrier;
    CarrierLocationRef rightPoint;
    CarrierRef rightCarrier;
    if (tryReadPoint(relation.left, leftPoint, leftCarrier) &&
        tryReadPoint(relation.right, rightPoint, rightCarrier) &&
        sameCarrier(leftCarrier, rightCarrier))
    {
        frame = CarrierFrame{leftCarrier, locationView(leftPoint).ticks, locationView(rightPoint).ticks};
        return true;
    }

    return false;
}

bool World::tryReadPointFrame(ElementRef ref, CarrierFrame& frame) const
{
    CarrierLocationRef pointLocation;
    CarrierRef pointCarrier;
    if (!tryReadPoint(ref, pointLocation, pointCarrier))
    {
        return false;
    }

    frame = CarrierFrame{pointCarrier, 0, locationView(pointLocation).ticks};
    return true;
}

bool World::tryReadBasis(ElementRef ref, CarrierLocationRef& basis)
{
    CarrierFrame frame;
    if (!tryReadFrame(ref, frame))
    {
        return false;
    }

    basis = location(frame.carrier, frame.extent());
    return true;
}

bool World::frameAccepts(const CarrierFrame& frame, CarrierRef carrier) const
{
    return isUnknown(frame.carrier) || sameCarrier(frame.carrier, carrier);
}

bool World::frameAcceptsProjectionSource(const CarrierFrame& frame, CarrierRef carrier) const
{
    return frameAccepts(frame, carrier) ||
        (frame.start == 0 &&
            !isUnknown(frame.carrier) &&
            compatibleUnitShape(unitShapeOf(frame.carrier), unitShapeOf(carrier)));
}

World::CarrierFrame World::resolveFrameCarrier(const CarrierFrame& frame, CarrierRef carrier) const
{
    return isUnknown(frame.carrier)
        ? CarrierFrame{carrier, frame.start, frame.end}
        : frame;
}

World::CarrierFrame World::resolveProjectionSourceFrame(const CarrierFrame& frame, CarrierRef carrier) const
{
    return isUnknown(frame.carrier) || !sameCarrier(frame.carrier, carrier)
        ? CarrierFrame{carrier, frame.start, frame.end}
        : frame;
}

bool World::tryProjectBetweenSurfaces(ElementRef left, ElementRef right, CarrierLocationRef input, CarrierRef reader, RelationRef& projected)
{
    CarrierFrame leftFrame;
    CarrierFrame rightFrame;
    if (!tryReadFrame(left, leftFrame) || !tryReadFrame(right, rightFrame))
    {
        projected = projectionResult(location(reader, 0), input);
        return false;
    }

    const auto inputCarrier = locationView(input).carrier;
    if (frameAcceptsProjectionSource(leftFrame, inputCarrier) && frameAccepts(rightFrame, reader))
    {
        projected = projectBetween(
            input,
            reader,
            resolveProjectionSourceFrame(leftFrame, inputCarrier),
            resolveFrameCarrier(rightFrame, reader));
        return true;
    }

    if (frameAcceptsProjectionSource(rightFrame, inputCarrier) && frameAccepts(leftFrame, reader))
    {
        projected = projectBetween(
            input,
            reader,
            resolveProjectionSourceFrame(rightFrame, inputCarrier),
            resolveFrameCarrier(leftFrame, reader));
        return true;
    }

    projected = projectionResult(location(reader, 0), input);
    return false;
}

RelationRef World::projectBetween(CarrierLocationRef input, CarrierRef reader, const CarrierFrame& source, const CarrierFrame& target)
{
    const auto inputView = locationView(input);
    const auto sourceBasis = source.extent();
    const auto targetBasis = target.extent();
    if (sourceBasis == 0 || targetBasis == 0)
    {
        return projectionResult(location(reader, 0), input);
    }

    const auto relativeTicks = saturatingAdd(inputView.ticks, -source.start);
    if (relativeTicks == 0)
    {
        return projectionResult(location(reader, target.start), location(inputView.carrier, 0));
    }

    const auto sourceMagnitude = magnitudeOf(sourceBasis);
    const auto targetMagnitude = magnitudeOf(targetBasis);
    const auto locationMagnitude = magnitudeOf(relativeTicks);
    const auto scaled = saturatingMultiply(locationMagnitude, targetMagnitude);
    const auto projectedMagnitude = scaled / sourceMagnitude;
    const auto residueMagnitude = scaled % sourceMagnitude;
    const auto sign = signOf(relativeTicks) * signOf(sourceBasis) * signOf(targetBasis);

    return projectionResult(
        location(reader, saturatingAdd(target.start, signedTicks(projectedMagnitude, sign))),
        location(inputView.carrier, signedTicks(residueMagnitude, signOf(relativeTicks))));
}

RelationRef World::projectionResult(CarrierLocationRef projected, CarrierLocationRef residue)
{
    return relate(element(projected), element(residue));
}

bool World::tryProjectToTargetSurface(ElementRef sourceSurface, ElementRef targetSurface, CarrierLocationRef input, SurfaceProjection& projection)
{
    CarrierFrame targetFrame;
    if (!tryReadFrame(targetSurface, targetFrame) || isUnknown(targetFrame.carrier))
    {
        return false;
    }

    return tryProjectThrough(sourceSurface, targetSurface, input, targetFrame.carrier, projection);
}

bool World::tryProjectThrough(ElementRef sourceSurface, ElementRef targetSurface, CarrierLocationRef input, CarrierRef reader, SurfaceProjection& projection)
{
    RelationRef projected;
    if (!tryProjectBetweenSurfaces(sourceSurface, targetSurface, input, reader, projected))
    {
        return false;
    }

    CarrierLocationRef target;
    CarrierLocationRef residue;
    if (!tryGetCarrierLocations(projected, target, residue) || !sameCarrier(locationView(target).carrier, reader))
    {
        return false;
    }

    projection = SurfaceProjection{target, residue};
    return true;
}


} // namespace gojo::simd

