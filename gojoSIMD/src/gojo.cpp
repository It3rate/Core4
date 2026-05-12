#include "gojoSIMD/gojo.hpp"

#include <algorithm>
#include <cstdlib>
#include <limits>
#include <numeric>
#include <queue>
#include <sstream>
#include <type_traits>

namespace gojo::simd {
namespace {

template <class Unsigned>
constexpr std::size_t laneCount()
{
    if constexpr (sizeof(Unsigned) == 1)
    {
        return 32;
    }
    else if constexpr (sizeof(Unsigned) == 2)
    {
        return 16;
    }
    else if constexpr (sizeof(Unsigned) == 4)
    {
        return 8;
    }
    else
    {
        return 4;
    }
}

template <class Signed>
Signed clampSigned(std::int64_t value)
{
    const auto lo = static_cast<std::int64_t>(std::numeric_limits<Signed>::min());
    const auto hi = static_cast<std::int64_t>(std::numeric_limits<Signed>::max());
    return static_cast<Signed>(std::clamp(value, lo, hi));
}

template <>
std::int64_t clampSigned<std::int64_t>(std::int64_t value)
{
    return value;
}

template <class Unsigned>
Unsigned clampUnsigned(std::uint64_t value)
{
    const auto hi = static_cast<std::uint64_t>(std::numeric_limits<Unsigned>::max());
    return static_cast<Unsigned>(std::min(value, hi));
}

std::int64_t clampToInt64(std::uint64_t value)
{
    const auto hi = static_cast<std::uint64_t>(std::numeric_limits<std::int64_t>::max());
    return static_cast<std::int64_t>(std::min(value, hi));
}

int signOf(std::int64_t value)
{
    return (value > 0) - (value < 0);
}

std::int64_t saturatingAdd(std::int64_t left, std::int64_t right)
{
    if (right > 0 && left > std::numeric_limits<std::int64_t>::max() - right)
    {
        return std::numeric_limits<std::int64_t>::max();
    }

    if (right < 0 && left < std::numeric_limits<std::int64_t>::min() - right)
    {
        return std::numeric_limits<std::int64_t>::min();
    }

    return left + right;
}

std::int64_t saturatingMultiply(std::int64_t left, std::int64_t right)
{
    if (left == 0 || right == 0)
    {
        return 0;
    }

    const auto max = std::numeric_limits<std::int64_t>::max();
    const auto min = std::numeric_limits<std::int64_t>::min();

    if (left == -1 && right == min)
    {
        return max;
    }

    if (right == -1 && left == min)
    {
        return max;
    }

    const auto product = left * right;
    if (product / right == left)
    {
        return product;
    }

    return ((left < 0) ^ (right < 0)) ? min : max;
}

std::uint64_t saturatingMultiply(std::uint64_t left, std::uint64_t right)
{
    if (left != 0 && right > std::numeric_limits<std::uint64_t>::max() / left)
    {
        return std::numeric_limits<std::uint64_t>::max();
    }

    return left * right;
}

bool containsRelation(const std::vector<std::uint32_t>& relations, std::uint32_t relation)
{
    return std::find(relations.begin(), relations.end(), relation) != relations.end();
}

std::string bitsLabel(std::uint8_t bits)
{
    return std::to_string(static_cast<int>(bits));
}

} // namespace

World::World()
{
    unknown_ = createCarrierRaw(64, 0, false, UnitShapeRef{});
    setUnitShape(unknown_, atom(unknown_));
}

CarrierRef World::unknownCarrier() const noexcept
{
    return unknown_;
}

CarrierRef World::createCarrier(std::uint8_t bits, bool inverse)
{
    auto carrier = createCarrierRaw(bits, nextCarrierId_++, inverse, UnitShapeRef{});
    setUnitShape(carrier, atom(carrier));
    return carrier;
}

CarrierRef World::createCarrier(UnitShapeRef unitShape, std::uint8_t bits, bool inverse)
{
    auto carrier = createCarrierRaw(bits, nextCarrierId_++, inverse, unitShape);
    if (unitShape.isNone())
    {
        setUnitShape(carrier, atom(carrier));
    }

    return carrier;
}

CarrierRef World::inverse(CarrierRef carrier)
{
    if (auto existing = inverseOf(carrier); existing.has_value())
    {
        return *existing;
    }

    auto other = createCarrier(unitShapeOf(carrier), carrier.bits, !isInverse(carrier));
    setInverse(carrier, other);
    setInverse(other, carrier);
    return other;
}

TickRefRef World::createTickRef(std::int64_t ticks, std::uint8_t bits)
{
    bits = bits == 0 ? bitsForSigned(ticks) : bits;
    if (!isSupportedSignedBits(bits))
    {
        bits = 64;
    }

    const auto id = nextTickId_++;
    switch (bits)
    {
    case 8:
    {
        const auto index = static_cast<std::uint32_t>(ticks8_.ticks.size());
        ticks8_.ids.push_back(id);
        ticks8_.ticks.push_back(clampSigned<std::int8_t>(ticks));
        return TickRefRef{8, index};
    }
    case 16:
    {
        const auto index = static_cast<std::uint32_t>(ticks16_.ticks.size());
        ticks16_.ids.push_back(id);
        ticks16_.ticks.push_back(clampSigned<std::int16_t>(ticks));
        return TickRefRef{16, index};
    }
    case 32:
    {
        const auto index = static_cast<std::uint32_t>(ticks32_.ticks.size());
        ticks32_.ids.push_back(id);
        ticks32_.ticks.push_back(clampSigned<std::int32_t>(ticks));
        return TickRefRef{32, index};
    }
    default:
    {
        const auto index = static_cast<std::uint32_t>(ticks64_.ticks.size());
        ticks64_.ids.push_back(id);
        ticks64_.ticks.push_back(ticks);
        return TickRefRef{64, index};
    }
    }
}

CarrierLocationRef World::location(CarrierRef carrier, std::int64_t ticks, std::uint8_t bits)
{
    return location(carrier, createTickRef(ticks, bits));
}

CarrierLocationRef World::location(CarrierRef carrier, TickRefRef tick)
{
    const auto index = static_cast<std::uint32_t>(locations_.size());
    locations_.push_back(LocationRow{carrier, tick});
    return CarrierLocationRef{index};
}

RelationRef World::relate(ElementRef left, ElementRef right)
{
    const auto index = static_cast<std::uint32_t>(relations_.size());
    relations_.push_back(RelationRow{nextRelationId_++, left, right});
    return RelationRef{index};
}

RelationRef World::point(CarrierRef carrier, std::int64_t ticks)
{
    return relate(element(location(carrier, ticks)), element(carrier));
}

RelationRef World::segment(CarrierRef carrier, std::int64_t start, std::int64_t end)
{
    return relate(element(point(carrier, start)), element(point(carrier, end)));
}

RelationRef World::orthogonalOutlet(ElementRef source)
{
    return relate(source, element(segment(unknown_, 0, 0)));
}

RelationRef World::liveOutput(ElementRef input, RelationRef outputPort)
{
    return relate(input, element(outputPort));
}

RelationRef World::additiveAttachment(CarrierRef holder, ElementRef participant)
{
    return relate(element(point(holder, 0)), participant);
}

RelationRef World::additiveGroup(CarrierRef holder, const std::vector<ElementRef>& participants)
{
    ElementRef chain = element(point(holder, 0));
    if (!participants.empty())
    {
        chain = element(additiveAttachment(holder, participants.front()));
        for (std::size_t index = 1; index < participants.size(); ++index)
        {
            chain = element(relate(chain, element(additiveAttachment(holder, participants[index]))));
        }
    }

    return relate(chain, element(holder));
}

UnitShapeRef World::atom(CarrierRef carrier)
{
    return createUnitShape(element(point(carrier, 1)));
}

UnitShapeRef World::orthogonalOf(ElementRef source)
{
    return createUnitShape(element(orthogonalOutlet(source)));
}

UnitShapeRef World::circularFrom(RelationRef pointSource)
{
    return isUnitPoint(pointSource) ? orthogonalOf(element(pointSource)) : UnitShapeRef{};
}

UnitShapeRef World::linearFrom(RelationRef zeroLengthSegment)
{
    CarrierLocationRef left;
    CarrierLocationRef right;
    if (!tryReadSegment(zeroLengthSegment, left, right))
    {
        return UnitShapeRef{};
    }

    const auto leftView = locationView(left);
    const auto rightView = locationView(right);
    if (!sameCarrier(leftView.carrier, rightView.carrier) || leftView.ticks != rightView.ticks)
    {
        return UnitShapeRef{};
    }

    return orthogonalOf(element(zeroLengthSegment));
}

UnitShapeRef World::sheetFrom(RelationRef nonzeroSegment)
{
    CarrierLocationRef left;
    CarrierLocationRef right;
    if (!tryReadSegment(nonzeroSegment, left, right))
    {
        return UnitShapeRef{};
    }

    const auto leftView = locationView(left);
    const auto rightView = locationView(right);
    if (!sameCarrier(leftView.carrier, rightView.carrier) || leftView.ticks == rightView.ticks)
    {
        return UnitShapeRef{};
    }

    return orthogonalOf(element(nonzeroSegment));
}

RelationRef World::attach(CarrierRef anchor, RelationRef relation)
{
    auto carriers = carriersOf(element(relation));
    std::vector<CarrierRef> attached;
    for (auto carrier : carriers)
    {
        if (isUnknown(carrier))
        {
            continue;
        }

        const auto already = std::find(attached.begin(), attached.end(), carrier) != attached.end();
        if (already)
        {
            continue;
        }

        auto& relations = attachedRelationsOf(carrier);
        if (!containsRelation(relations, relation.index))
        {
            relations.push_back(relation.index);
        }

        attached.push_back(carrier);
    }

    settleWorld(anchor);
    return relation;
}

void World::add(CarrierRef carrier, std::uint64_t ticks)
{
    receive(carrier, clampToInt64(ticks));
    settleWorld(carrier);
}

void World::remove(CarrierRef carrier, std::uint64_t ticks)
{
    receive(carrier, -clampToInt64(ticks));
    settleWorld(carrier);
}

void World::set(CarrierRef carrier, std::int64_t ticks)
{
    receive(carrier, saturatingAdd(ticks, -static_cast<std::int64_t>(std::min(value(carrier), static_cast<std::uint64_t>(std::numeric_limits<std::int64_t>::max())))));
    settleWorld(carrier);
}

CarrierLocationRef World::valueSlot(CarrierRef carrier)
{
    const auto tick = valueTickOf(carrier);
    const auto index = static_cast<std::uint32_t>(locations_.size());
    locations_.push_back(LocationRow{carrier, tick});
    return CarrierLocationRef{index};
}

CarrierLocationRef World::tensionSlot(CarrierRef carrier)
{
    return location(carrier, tension(carrier));
}

std::uint64_t World::value(CarrierRef carrier) const
{
    switch (carrier.bits)
    {
    case 8:
        return carriers8_.values.at(carrier.index);
    case 16:
        return carriers16_.values.at(carrier.index);
    case 32:
        return carriers32_.values.at(carrier.index);
    default:
        return carriers64_.values.at(carrier.index);
    }
}

std::int64_t World::tension(CarrierRef carrier) const
{
    switch (carrier.bits)
    {
    case 8:
        return carriers8_.tensions.at(carrier.index);
    case 16:
        return carriers16_.tensions.at(carrier.index);
    case 32:
        return carriers32_.tensions.at(carrier.index);
    default:
        return carriers64_.tensions.at(carrier.index);
    }
}

CarrierState World::carrierState(CarrierRef carrier) const
{
    CarrierState state;
    state.bits = carrier.bits;
    state.value = value(carrier);
    state.tension = tension(carrier);
    state.unitShape = unitShapeOf(carrier);
    state.inverse = inverseOf(carrier);

    switch (carrier.bits)
    {
    case 8:
        state.id = carriers8_.ids.at(carrier.index);
        break;
    case 16:
        state.id = carriers16_.ids.at(carrier.index);
        break;
    case 32:
        state.id = carriers32_.ids.at(carrier.index);
        break;
    default:
        state.id = carriers64_.ids.at(carrier.index);
        break;
    }

    state.isUnknown = state.id == 0;
    state.isInverse = isInverse(carrier);
    return state;
}

CarrierLocationView World::locationView(CarrierLocationRef location) const
{
    const auto& row = locations_.at(location.index);
    return CarrierLocationView{row.carrier, row.tick, tickValue(row.tick)};
}

ElementRef World::left(RelationRef relation) const
{
    return relations_.at(relation.index).left;
}

ElementRef World::right(RelationRef relation) const
{
    return relations_.at(relation.index).right;
}

Id World::relationId(RelationRef relation) const
{
    return relations_.at(relation.index).id;
}

std::uint32_t World::carrierCount(std::uint8_t bits) const
{
    switch (bits)
    {
    case 8:
        return static_cast<std::uint32_t>(carriers8_.ids.size());
    case 16:
        return static_cast<std::uint32_t>(carriers16_.ids.size());
    case 32:
        return static_cast<std::uint32_t>(carriers32_.ids.size());
    default:
        return static_cast<std::uint32_t>(carriers64_.ids.size());
    }
}

std::uint32_t World::tickRefCount(std::uint8_t bits) const
{
    switch (bits)
    {
    case 8:
        return static_cast<std::uint32_t>(ticks8_.ids.size());
    case 16:
        return static_cast<std::uint32_t>(ticks16_.ids.size());
    case 32:
        return static_cast<std::uint32_t>(ticks32_.ids.size());
    default:
        return static_cast<std::uint32_t>(ticks64_.ids.size());
    }
}

ElementRef World::element(CarrierRef carrier) const noexcept
{
    return ElementRef{ElementStore::carrier, carrier.bits, carrier.index};
}

ElementRef World::element(TickRefRef tick) const noexcept
{
    return ElementRef{ElementStore::tick_ref, tick.bits, tick.index};
}

ElementRef World::element(CarrierLocationRef location) const noexcept
{
    return ElementRef{ElementStore::carrier_location, 0, location.index};
}

ElementRef World::element(RelationRef relation) const noexcept
{
    return ElementRef{ElementStore::relation, 0, relation.index};
}

ElementRef World::element(UnitShapeRef shape) const noexcept
{
    return ElementRef{ElementStore::unit_shape, 0, shape.index};
}

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

bool World::tryCrystallize(RelationRef relation, CarrierRef& carrier)
{
    ElementRef source;
    if (tryReadOrthogonalSource(element(relation), source))
    {
        carrier = createCarrier(orthogonalOf(source), 64);
        return true;
    }

    CarrierFrame frame;
    if (tryReadFrame(element(relation), frame))
    {
        carrier = createCarrier(unitShapeOf(frame.carrier), frame.carrier.bits);
        return true;
    }

    return false;
}

bool World::tryReadCircularOrigin(CarrierRef carrier, CarrierLocationRef& origin) const
{
    const auto shape = unitShapeOf(carrier);
    if (shape.isNone())
    {
        return false;
    }

    ElementRef source;
    if (!tryReadOrthogonalSource(unitShapes_.at(shape.index).surface, source))
    {
        return false;
    }

    CarrierRef sourceCarrier;
    return tryReadPoint(source, origin, sourceCarrier);
}

bool World::tryReadLinearOrigin(CarrierRef carrier, CarrierLocationRef& origin) const
{
    const auto shape = unitShapeOf(carrier);
    if (shape.isNone())
    {
        return false;
    }

    ElementRef source;
    if (!tryReadOrthogonalSource(unitShapes_.at(shape.index).surface, source) ||
        source.store != ElementStore::relation)
    {
        return false;
    }

    CarrierLocationRef left;
    CarrierLocationRef right;
    if (!tryReadSegment(RelationRef{source.index}, left, right))
    {
        return false;
    }

    const auto leftView = locationView(left);
    const auto rightView = locationView(right);
    if (!sameCarrier(leftView.carrier, rightView.carrier) || leftView.ticks != rightView.ticks)
    {
        return false;
    }

    origin = left;
    return true;
}

bool World::compatibleUnitShape(UnitShapeRef left, UnitShapeRef right) const
{
    if (left.isNone() || right.isNone())
    {
        return false;
    }

    if (left == right)
    {
        return true;
    }

    return sameElementValue(unitShapes_.at(left.index).surface, unitShapes_.at(right.index).surface);
}

bool World::sameElementValue(ElementRef left, ElementRef right) const
{
    if (left.store != right.store)
    {
        return false;
    }

    switch (left.store)
    {
    case ElementStore::none:
        return true;
    case ElementStore::carrier:
        return sameCarrier(CarrierRef{left.bits, left.index}, CarrierRef{right.bits, right.index});
    case ElementStore::tick_ref:
        return tickValue(TickRefRef{left.bits, left.index}) == tickValue(TickRefRef{right.bits, right.index});
    case ElementStore::carrier_location:
    {
        const auto leftView = locationView(CarrierLocationRef{left.index});
        const auto rightView = locationView(CarrierLocationRef{right.index});
        return sameCarrier(leftView.carrier, rightView.carrier) && leftView.ticks == rightView.ticks;
    }
    case ElementStore::relation:
    {
        const auto& leftRelation = relations_.at(left.index);
        const auto& rightRelation = relations_.at(right.index);
        return sameElementValue(leftRelation.left, rightRelation.left) &&
            sameElementValue(leftRelation.right, rightRelation.right);
    }
    case ElementStore::unit_shape:
        return compatibleUnitShape(UnitShapeRef{left.index}, UnitShapeRef{right.index});
    }

    return false;
}

std::string World::describe(ElementRef ref) const
{
    switch (ref.store)
    {
    case ElementStore::carrier:
        return describe(CarrierRef{ref.bits, ref.index});
    case ElementStore::tick_ref:
        return "tick" + bitsLabel(ref.bits) + "(" + std::to_string(tickValue(TickRefRef{ref.bits, ref.index})) + ")";
    case ElementStore::carrier_location:
    {
        const auto view = locationView(CarrierLocationRef{ref.index});
        return describe(view.carrier) + ":" + std::to_string(view.ticks);
    }
    case ElementStore::relation:
        return describe(RelationRef{ref.index});
    case ElementStore::unit_shape:
        return "unit#" + std::to_string(unitShapes_.at(ref.index).id);
    default:
        return "none";
    }
}

std::string World::describe(CarrierRef carrier) const
{
    const auto state = carrierState(carrier);
    std::ostringstream text;
    text << state.id << (state.isInverse ? "-" : "+") << "/u" << static_cast<int>(state.bits);
    if (state.value != 0 || state.tension != 0)
    {
        text << "[" << state.value << "|" << state.tension << "]";
    }

    return text.str();
}

std::string World::describe(RelationRef relation) const
{
    const auto& row = relations_.at(relation.index);
    return describe(row.left) + " -> " + describe(row.right);
}

CarrierRef World::createCarrierRaw(std::uint8_t bits, Id id, bool inverse, UnitShapeRef unitShape)
{
    if (!isSupportedCarrierBits(bits))
    {
        bits = 64;
    }

    const auto valueTick = createTickRef(0, coordinateBitsForCarrier(bits));
    switch (bits)
    {
    case 8:
    {
        const auto index = static_cast<std::uint32_t>(carriers8_.ids.size());
        carriers8_.ids.push_back(id);
        carriers8_.inverseFlags.push_back(inverse);
        carriers8_.unitShapes.push_back(unitShape);
        carriers8_.inverses.push_back(std::nullopt);
        carriers8_.valueTicks.push_back(valueTick);
        carriers8_.values.push_back(0);
        carriers8_.tensions.push_back(0);
        carriers8_.attachedRelations.emplace_back();
        return CarrierRef{8, index};
    }
    case 16:
    {
        const auto index = static_cast<std::uint32_t>(carriers16_.ids.size());
        carriers16_.ids.push_back(id);
        carriers16_.inverseFlags.push_back(inverse);
        carriers16_.unitShapes.push_back(unitShape);
        carriers16_.inverses.push_back(std::nullopt);
        carriers16_.valueTicks.push_back(valueTick);
        carriers16_.values.push_back(0);
        carriers16_.tensions.push_back(0);
        carriers16_.attachedRelations.emplace_back();
        return CarrierRef{16, index};
    }
    case 32:
    {
        const auto index = static_cast<std::uint32_t>(carriers32_.ids.size());
        carriers32_.ids.push_back(id);
        carriers32_.inverseFlags.push_back(inverse);
        carriers32_.unitShapes.push_back(unitShape);
        carriers32_.inverses.push_back(std::nullopt);
        carriers32_.valueTicks.push_back(valueTick);
        carriers32_.values.push_back(0);
        carriers32_.tensions.push_back(0);
        carriers32_.attachedRelations.emplace_back();
        return CarrierRef{32, index};
    }
    default:
    {
        const auto index = static_cast<std::uint32_t>(carriers64_.ids.size());
        carriers64_.ids.push_back(id);
        carriers64_.inverseFlags.push_back(inverse);
        carriers64_.unitShapes.push_back(unitShape);
        carriers64_.inverses.push_back(std::nullopt);
        carriers64_.valueTicks.push_back(valueTick);
        carriers64_.values.push_back(0);
        carriers64_.tensions.push_back(0);
        carriers64_.attachedRelations.emplace_back();
        return CarrierRef{64, index};
    }
    }
}

UnitShapeRef World::createUnitShape(ElementRef surface)
{
    const auto index = static_cast<std::uint32_t>(unitShapes_.size());
    unitShapes_.push_back(UnitShapeRow{nextUnitShapeId_++, surface});
    return UnitShapeRef{index};
}

bool World::isSupportedCarrierBits(std::uint8_t bits) const noexcept
{
    return bits == 8 || bits == 16 || bits == 32 || bits == 64;
}

bool World::isSupportedSignedBits(std::uint8_t bits) const noexcept
{
    return bits == 8 || bits == 16 || bits == 32 || bits == 64;
}

bool World::sameCarrier(CarrierRef left, CarrierRef right) const noexcept
{
    return left.bits == right.bits && left.index == right.index;
}

bool World::isUnknown(CarrierRef carrier) const
{
    return carrierState(carrier).id == 0;
}

bool World::isInverse(CarrierRef carrier) const
{
    switch (carrier.bits)
    {
    case 8:
        return carriers8_.inverseFlags.at(carrier.index);
    case 16:
        return carriers16_.inverseFlags.at(carrier.index);
    case 32:
        return carriers32_.inverseFlags.at(carrier.index);
    default:
        return carriers64_.inverseFlags.at(carrier.index);
    }
}

UnitShapeRef World::unitShapeOf(CarrierRef carrier) const
{
    switch (carrier.bits)
    {
    case 8:
        return carriers8_.unitShapes.at(carrier.index);
    case 16:
        return carriers16_.unitShapes.at(carrier.index);
    case 32:
        return carriers32_.unitShapes.at(carrier.index);
    default:
        return carriers64_.unitShapes.at(carrier.index);
    }
}

std::optional<CarrierRef> World::inverseOf(CarrierRef carrier) const
{
    switch (carrier.bits)
    {
    case 8:
        return carriers8_.inverses.at(carrier.index);
    case 16:
        return carriers16_.inverses.at(carrier.index);
    case 32:
        return carriers32_.inverses.at(carrier.index);
    default:
        return carriers64_.inverses.at(carrier.index);
    }
}

TickRefRef World::valueTickOf(CarrierRef carrier) const
{
    switch (carrier.bits)
    {
    case 8:
        return carriers8_.valueTicks.at(carrier.index);
    case 16:
        return carriers16_.valueTicks.at(carrier.index);
    case 32:
        return carriers32_.valueTicks.at(carrier.index);
    default:
        return carriers64_.valueTicks.at(carrier.index);
    }
}

std::vector<std::uint32_t>& World::attachedRelationsOf(CarrierRef carrier)
{
    switch (carrier.bits)
    {
    case 8:
        return carriers8_.attachedRelations.at(carrier.index);
    case 16:
        return carriers16_.attachedRelations.at(carrier.index);
    case 32:
        return carriers32_.attachedRelations.at(carrier.index);
    default:
        return carriers64_.attachedRelations.at(carrier.index);
    }
}

const std::vector<std::uint32_t>& World::attachedRelationsOf(CarrierRef carrier) const
{
    switch (carrier.bits)
    {
    case 8:
        return carriers8_.attachedRelations.at(carrier.index);
    case 16:
        return carriers16_.attachedRelations.at(carrier.index);
    case 32:
        return carriers32_.attachedRelations.at(carrier.index);
    default:
        return carriers64_.attachedRelations.at(carrier.index);
    }
}

void World::setUnitShape(CarrierRef carrier, UnitShapeRef unitShape)
{
    switch (carrier.bits)
    {
    case 8:
        carriers8_.unitShapes.at(carrier.index) = unitShape;
        break;
    case 16:
        carriers16_.unitShapes.at(carrier.index) = unitShape;
        break;
    case 32:
        carriers32_.unitShapes.at(carrier.index) = unitShape;
        break;
    default:
        carriers64_.unitShapes.at(carrier.index) = unitShape;
        break;
    }
}

void World::setInverse(CarrierRef carrier, CarrierRef inverse)
{
    switch (carrier.bits)
    {
    case 8:
        carriers8_.inverses.at(carrier.index) = inverse;
        break;
    case 16:
        carriers16_.inverses.at(carrier.index) = inverse;
        break;
    case 32:
        carriers32_.inverses.at(carrier.index) = inverse;
        break;
    default:
        carriers64_.inverses.at(carrier.index) = inverse;
        break;
    }
}

void World::setValue(CarrierRef carrier, std::uint64_t nextValue)
{
    std::uint64_t visible = nextValue;
    switch (carrier.bits)
    {
    case 8:
        visible = std::min<std::uint64_t>(visible, std::numeric_limits<std::uint8_t>::max());
        carriers8_.values.at(carrier.index) = static_cast<std::uint8_t>(visible);
        break;
    case 16:
        visible = std::min<std::uint64_t>(visible, std::numeric_limits<std::uint16_t>::max());
        carriers16_.values.at(carrier.index) = static_cast<std::uint16_t>(visible);
        break;
    case 32:
        visible = std::min<std::uint64_t>(visible, std::numeric_limits<std::uint32_t>::max());
        carriers32_.values.at(carrier.index) = static_cast<std::uint32_t>(visible);
        break;
    default:
        carriers64_.values.at(carrier.index) = visible;
        break;
    }

    setTick(valueTickOf(carrier), clampToInt64(visible));
}

void World::setTick(TickRefRef tick, std::int64_t ticks)
{
    switch (tick.bits)
    {
    case 8:
        ticks8_.ticks.at(tick.index) = clampSigned<std::int8_t>(ticks);
        break;
    case 16:
        ticks16_.ticks.at(tick.index) = clampSigned<std::int16_t>(ticks);
        break;
    case 32:
        ticks32_.ticks.at(tick.index) = clampSigned<std::int32_t>(ticks);
        break;
    default:
        ticks64_.ticks.at(tick.index) = ticks;
        break;
    }
}

void World::addResidualTension(CarrierRef carrier, std::int64_t ticks)
{
    if (ticks == 0)
    {
        return;
    }

    switch (carrier.bits)
    {
    case 8:
        carriers8_.tensions.at(carrier.index) = saturatingAdd(carriers8_.tensions.at(carrier.index), ticks);
        break;
    case 16:
        carriers16_.tensions.at(carrier.index) = saturatingAdd(carriers16_.tensions.at(carrier.index), ticks);
        break;
    case 32:
        carriers32_.tensions.at(carrier.index) = saturatingAdd(carriers32_.tensions.at(carrier.index), ticks);
        break;
    default:
        carriers64_.tensions.at(carrier.index) = saturatingAdd(carriers64_.tensions.at(carrier.index), ticks);
        break;
    }
}

bool World::keepTensionRemainder(CarrierRef carrier, std::int64_t ticks)
{
    if (tension(carrier) == ticks)
    {
        return false;
    }

    switch (carrier.bits)
    {
    case 8:
        carriers8_.tensions.at(carrier.index) = ticks;
        break;
    case 16:
        carriers16_.tensions.at(carrier.index) = ticks;
        break;
    case 32:
        carriers32_.tensions.at(carrier.index) = ticks;
        break;
    default:
        carriers64_.tensions.at(carrier.index) = ticks;
        break;
    }

    return true;
}

bool World::leaveTension(CarrierRef carrier, std::uint64_t nextValue, std::int64_t nextTension)
{
    if (value(carrier) == nextValue && nextTension == 0)
    {
        return false;
    }

    setValue(carrier, nextValue);
    addResidualTension(carrier, nextTension);
    return true;
}

bool World::balanceOpposedValueWith(CarrierRef left, CarrierRef right)
{
    const auto shared = std::min(value(left), value(right));
    if (shared == 0)
    {
        return false;
    }

    setValue(left, value(left) - shared);
    setValue(right, value(right) - shared);
    return true;
}

void World::receive(CarrierRef carrier, std::int64_t ticks)
{
    if (ticks == 0)
    {
        return;
    }

    addResidualTension(carrier, ticks);
    absorbCarrier(carrier);
}

bool World::receiveTo(CarrierRef carrier, std::int64_t ticks)
{
    const auto beforeValue = value(carrier);
    const auto beforeTension = tension(carrier);
    const auto current = static_cast<std::int64_t>(std::min(value(carrier), static_cast<std::uint64_t>(std::numeric_limits<std::int64_t>::max())));
    receive(carrier, saturatingAdd(ticks, -current));
    return value(carrier) != beforeValue || tension(carrier) != beforeTension;
}

void World::absorbCarrier(CarrierRef carrier)
{
    auto absorbOne = [this, carrier](auto& block)
    {
        using Unsigned = typename std::remove_reference_t<decltype(block.values)>::value_type;
        constexpr auto lanes = laneCount<Unsigned>();
        const auto maxValue = static_cast<std::uint64_t>(std::numeric_limits<Unsigned>::max());
        const auto base = (carrier.index / lanes) * lanes;
        const auto count = std::min<std::size_t>(lanes, block.values.size() - base);

        Lane<std::uint64_t, lanes> values;
        Lane<std::int64_t, lanes> tensions;
        for (std::size_t lane = 0; lane < count; ++lane)
        {
            values[lane] = static_cast<std::uint64_t>(block.values[base + lane]);
            tensions[lane] = block.tensions[base + lane];
        }

        const auto lane = carrier.index - base;
        auto visible = values[lane];
        auto pending = tensions[lane];
        if (pending > 0)
        {
            const auto incoming = static_cast<std::uint64_t>(pending);
            const auto room = maxValue - visible;
            if (room >= incoming)
            {
                visible += incoming;
                pending = 0;
            }
            else
            {
                visible = maxValue;
                pending = static_cast<std::int64_t>(incoming - room);
            }
        }
        else if (pending < 0)
        {
            const auto draw = magnitudeOf(pending);
            if (visible >= draw)
            {
                visible -= draw;
                pending = 0;
            }
            else
            {
                pending = signedTicks(draw - visible, -1);
                visible = 0;
            }
        }

        values[lane] = visible;
        tensions[lane] = pending;
        block.values[carrier.index] = clampUnsigned<Unsigned>(visible);
        block.tensions[carrier.index] = pending;
        setTick(block.valueTicks[carrier.index], clampToInt64(visible));
    };

    switch (carrier.bits)
    {
    case 8:
        absorbOne(carriers8_);
        break;
    case 16:
        absorbOne(carriers16_);
        break;
    case 32:
        absorbOne(carriers32_);
        break;
    default:
        absorbOne(carriers64_);
        break;
    }
}

void World::absorbAllCarrierBlocks()
{
    auto absorb = [this](auto& block)
    {
        using Unsigned = typename std::remove_reference_t<decltype(block.values)>::value_type;
        constexpr auto lanes = laneCount<Unsigned>();
        const auto maxValue = static_cast<std::uint64_t>(std::numeric_limits<Unsigned>::max());

        for (std::size_t base = 0; base < block.values.size(); base += lanes)
        {
            Lane<std::uint64_t, lanes> values;
            Lane<std::int64_t, lanes> tensions;
            const auto count = std::min<std::size_t>(lanes, block.values.size() - base);
            for (std::size_t lane = 0; lane < count; ++lane)
            {
                values[lane] = static_cast<std::uint64_t>(block.values[base + lane]);
                tensions[lane] = block.tensions[base + lane];
            }

            for (std::size_t lane = 0; lane < count; ++lane)
            {
                auto visible = values[lane];
                auto pending = tensions[lane];
                if (pending > 0)
                {
                    const auto incoming = static_cast<std::uint64_t>(pending);
                    const auto room = maxValue - visible;
                    if (room >= incoming)
                    {
                        visible += incoming;
                        pending = 0;
                    }
                    else
                    {
                        visible = maxValue;
                        pending = static_cast<std::int64_t>(incoming - room);
                    }
                }
                else if (pending < 0)
                {
                    const auto draw = magnitudeOf(pending);
                    if (visible >= draw)
                    {
                        visible -= draw;
                        pending = 0;
                    }
                    else
                    {
                        pending = signedTicks(draw - visible, -1);
                        visible = 0;
                    }
                }

                values[lane] = visible;
                tensions[lane] = pending;
            }

            for (std::size_t lane = 0; lane < count; ++lane)
            {
                block.values[base + lane] = clampUnsigned<Unsigned>(values[lane]);
                block.tensions[base + lane] = tensions[lane];
                setTick(block.valueTicks[base + lane], clampToInt64(values[lane]));
            }
        }
    };

    absorb(carriers8_);
    absorb(carriers16_);
    absorb(carriers32_);
    absorb(carriers64_);
}

void World::settleWorld(CarrierRef anchor)
{
    if (settling_)
    {
        return;
    }

    settling_ = true;
    for (std::size_t pass = 0; pass < 256; ++pass)
    {
        auto carriers = connectedCarriers(anchor);
        const auto before = snapshot(carriers);

        std::vector<std::uint32_t> relationIndexes;
        for (const auto carrier : carriers)
        {
            for (const auto relation : attachedRelationsOf(carrier))
            {
                if (!containsRelation(relationIndexes, relation))
                {
                    relationIndexes.push_back(relation);
                }
            }
        }

        for (const auto relationIndex : relationIndexes)
        {
            (void)applyRelation(RelationRef{relationIndex});
        }

        carriers = connectedCarriers(anchor);
        if (snapshot(carriers) == before)
        {
            settling_ = false;
            return;
        }
    }

    settling_ = false;
}

std::vector<CarrierRef> World::connectedCarriers(CarrierRef anchor) const
{
    std::vector<CarrierRef> found;
    std::queue<CarrierRef> pending;
    pending.push(anchor);

    while (!pending.empty())
    {
        const auto carrier = pending.front();
        pending.pop();
        if (std::find(found.begin(), found.end(), carrier) != found.end())
        {
            continue;
        }

        found.push_back(carrier);
        for (const auto relation : attachedRelationsOf(carrier))
        {
            for (const auto related : carriersOf(element(RelationRef{relation})))
            {
                if (!isUnknown(related))
                {
                    pending.push(related);
                }
            }
        }
    }

    std::sort(found.begin(), found.end(), [](CarrierRef left, CarrierRef right)
    {
        return std::tie(left.bits, left.index) < std::tie(right.bits, right.index);
    });
    return found;
}

std::vector<CarrierRef> World::carriersOf(ElementRef ref) const
{
    switch (ref.store)
    {
    case ElementStore::carrier:
        return {CarrierRef{ref.bits, ref.index}};
    case ElementStore::carrier_location:
        return {locations_.at(ref.index).carrier};
    case ElementStore::relation:
    {
        const auto& relation = relations_.at(ref.index);
        auto left = carriersOf(relation.left);
        auto right = carriersOf(relation.right);
        left.insert(left.end(), right.begin(), right.end());
        return left;
    }
    case ElementStore::unit_shape:
        return carriersOf(unitShapes_.at(ref.index).surface);
    default:
        return {};
    }
}

std::vector<CarrierLocationRef> World::carrierSitesOf(ElementRef ref) const
{
    switch (ref.store)
    {
    case ElementStore::carrier_location:
        return {CarrierLocationRef{ref.index}};
    case ElementStore::relation:
    {
        const auto& relation = relations_.at(ref.index);
        auto left = carrierSitesOf(relation.left);
        auto right = carrierSitesOf(relation.right);
        left.insert(left.end(), right.begin(), right.end());
        return left;
    }
    default:
        return {};
    }
}

bool World::tryReadOneCarrier(const std::vector<CarrierLocationRef>& sites, CarrierSites& carrierSites) const
{
    if (sites.empty())
    {
        return false;
    }

    const auto first = locationView(sites.front()).carrier;
    std::int64_t minTicks = locationView(sites.front()).ticks;
    std::int64_t maxTicks = minTicks;
    for (const auto site : sites)
    {
        const auto view = locationView(site);
        if (!sameCarrier(first, view.carrier))
        {
            return false;
        }

        minTicks = std::min(minTicks, view.ticks);
        maxTicks = std::max(maxTicks, view.ticks);
    }

    carrierSites = CarrierSites{first, minTicks, maxTicks};
    return true;
}

std::vector<std::tuple<std::uint8_t, std::uint32_t, std::uint64_t, std::int64_t>> World::snapshot(const std::vector<CarrierRef>& carriers) const
{
    std::vector<std::tuple<std::uint8_t, std::uint32_t, std::uint64_t, std::int64_t>> rows;
    for (const auto carrier : carriers)
    {
        rows.emplace_back(carrier.bits, carrier.index, value(carrier), tension(carrier));
    }

    std::sort(rows.begin(), rows.end());
    return rows;
}

bool World::applyRelation(RelationRef relation)
{
    if (tryApplyLiveOutput(relation))
    {
        return true;
    }

    const auto& row = relations_.at(relation.index);
    const auto leftSites = carrierSitesOf(row.left);
    const auto rightSites = carrierSitesOf(row.right);

    CarrierSites left;
    CarrierSites right;
    if (!tryReadOneCarrier(leftSites, left) || !tryReadOneCarrier(rightSites, right))
    {
        return false;
    }

    if (sameCarrier(left.carrier, right.carrier))
    {
        return accountLocal(left.carrier, left.max, right.min);
    }

    if (isInverse(left.carrier) != isInverse(right.carrier) && right.isOrigin())
    {
        return balanceOpposedValueWith(left.carrier, right.carrier) |
            routeNegativeTension(left, right);
    }

    return right.isOrigin() && routePositiveTension(left, right);
}

bool World::accountLocal(CarrierRef carrier, std::int64_t source, std::int64_t target)
{
    if (source <= 0 || target < 0 || source < target)
    {
        return false;
    }

    const auto sourceTicks = static_cast<std::uint64_t>(source);
    const auto targetTicks = static_cast<std::uint64_t>(target);
    const auto current = value(carrier);
    if (current <= targetTicks)
    {
        return false;
    }

    const auto local = sourceTicks == targetTicks
        ? std::min(current, sourceTicks)
        : targetTicks + ((current - targetTicks) % (sourceTicks - targetTicks));

    return local != current && leaveTension(carrier, local, clampToInt64(current - local));
}

bool World::routePositiveTension(const CarrierSites& source, const CarrierSites& receiver)
{
    const auto unit = source.width() == 0 ? source.max : source.width();
    if (unit <= 0)
    {
        return false;
    }

    const auto target = source.width() == 0 ? 0 : source.min;
    auto changed = accountLocal(source.carrier, source.max, target);
    if (tension(source.carrier) <= 0)
    {
        return changed;
    }

    const auto pending = static_cast<std::uint64_t>(tension(source.carrier));
    const auto unitTicks = static_cast<std::uint64_t>(unit);
    const auto received = pending / unitTicks;
    if (received == 0)
    {
        return changed;
    }

    changed |= keepTensionRemainder(source.carrier, static_cast<std::int64_t>(pending % unitTicks));
    receive(receiver.carrier, clampToInt64(received));
    return true;
}

bool World::routeNegativeTension(const CarrierSites& source, const CarrierSites& receiver)
{
    const auto unit = source.width() == 0 ? source.max : source.width();
    if (unit <= 0 || tension(source.carrier) >= 0)
    {
        return false;
    }

    const auto pending = magnitudeOf(tension(source.carrier));
    const auto unitTicks = static_cast<std::uint64_t>(unit);
    const auto received = pending / unitTicks;
    if (received == 0)
    {
        return false;
    }

    const auto changed = keepTensionRemainder(source.carrier, -static_cast<std::int64_t>(pending % unitTicks));
    receive(receiver.carrier, clampToInt64(received));
    return changed || received != 0;
}

bool World::tryApplyLiveOutput(RelationRef relation)
{
    const auto& row = relations_.at(relation.index);
    if (row.right.store != ElementStore::relation)
    {
        return false;
    }

    const auto port = RelationRef{row.right.index};
    return tryApplyLiveProjectionOutput(row.left, port) ||
        tryApplyLiveOperationOutput(row.left, port);
}

bool World::tryApplyLiveProjectionOutput(ElementRef input, RelationRef port)
{
    if (input.store != ElementStore::carrier_location)
    {
        return false;
    }

    const auto inputLocation = CarrierLocationRef{input.index};
    const auto& portRow = relations_.at(port.index);
    SurfaceProjection projection;
    if (tryProjectToTargetSurface(portRow.left, portRow.right, inputLocation, projection) ||
        tryProjectToTargetSurface(portRow.right, portRow.left, inputLocation, projection))
    {
        return applyLiveOutputTargets({{locationView(projection.target).carrier, locationView(projection.target).ticks}});
    }

    return false;
}

bool World::tryApplyLiveOperationOutput(ElementRef input, RelationRef operation)
{
    const auto& row = relations_.at(operation.index);
    CarrierFrame targetFrame;
    if (!tryReadFrame(row.right, targetFrame) || isUnknown(targetFrame.carrier))
    {
        return false;
    }

    RelationRef read;
    if (!tryReadOperationFrom(operation, input, targetFrame.carrier, read))
    {
        return false;
    }

    const auto targetRef = left(read);
    if (targetRef.store != ElementStore::carrier_location)
    {
        return false;
    }

    const auto target = CarrierLocationRef{targetRef.index};
    const auto targetView = locationView(target);
    if (!sameCarrier(targetView.carrier, targetFrame.carrier))
    {
        return false;
    }

    return applyLiveOutputTargets({{targetView.carrier, targetView.ticks}});
}

bool World::applyLiveOutputTargets(const std::vector<std::pair<CarrierRef, std::int64_t>>& targets)
{
    bool changed = false;
    std::vector<std::pair<CarrierRef, std::int64_t>> grouped;
    for (const auto& target : targets)
    {
        auto existing = std::find_if(
            grouped.begin(),
            grouped.end(),
            [&](const auto& row)
            {
                return sameCarrier(row.first, target.first);
            });
        if (existing == grouped.end())
        {
            grouped.push_back(target);
        }
        else
        {
            existing->second = saturatingAdd(existing->second, target.second);
        }
    }

    for (const auto& target : grouped)
    {
        changed |= receiveTo(target.first, target.second);
    }

    return changed;
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

bool World::tryReadOrthogonalSource(ElementRef ref, ElementRef& source) const
{
    if (ref.store != ElementStore::relation)
    {
        return false;
    }

    const auto& row = relations_.at(ref.index);
    if (isZeroSegment(row.right))
    {
        source = row.left;
        return true;
    }

    return false;
}

bool World::tryReadSegment(RelationRef relation, CarrierLocationRef& left, CarrierLocationRef& right) const
{
    const auto& row = relations_.at(relation.index);
    CarrierRef leftCarrier;
    CarrierRef rightCarrier;
    if (!tryReadPoint(row.left, left, leftCarrier) ||
        !tryReadPoint(row.right, right, rightCarrier) ||
        !sameCarrier(leftCarrier, rightCarrier))
    {
        return false;
    }

    return true;
}

bool World::isUnitPoint(RelationRef relation) const
{
    CarrierLocationRef locationRef;
    CarrierRef carrier;
    return tryReadPoint(element(relation), locationRef, carrier);
}

bool World::isZeroSegment(ElementRef ref) const
{
    if (ref.store != ElementStore::relation)
    {
        return false;
    }

    CarrierLocationRef left;
    CarrierLocationRef right;
    if (!tryReadSegment(RelationRef{ref.index}, left, right))
    {
        return false;
    }

    const auto leftView = locationView(left);
    const auto rightView = locationView(right);
    return isUnknown(leftView.carrier) &&
        isUnknown(rightView.carrier) &&
        leftView.ticks == 0 &&
        rightView.ticks == 0;
}

std::int64_t World::tickValue(TickRefRef tick) const
{
    switch (tick.bits)
    {
    case 8:
        return ticks8_.ticks.at(tick.index);
    case 16:
        return ticks16_.ticks.at(tick.index);
    case 32:
        return ticks32_.ticks.at(tick.index);
    default:
        return ticks64_.ticks.at(tick.index);
    }
}

std::uint64_t World::magnitudeOf(std::int64_t ticks) const noexcept
{
    if (ticks >= 0)
    {
        return static_cast<std::uint64_t>(ticks);
    }

    if (ticks == std::numeric_limits<std::int64_t>::min())
    {
        return std::uint64_t{1} << 63;
    }

    return static_cast<std::uint64_t>(-ticks);
}

std::int64_t World::signedTicks(std::uint64_t ticks, int sign) const
{
    const auto max = static_cast<std::uint64_t>(std::numeric_limits<std::int64_t>::max());
    if (sign < 0)
    {
        if (ticks > max)
        {
            return std::numeric_limits<std::int64_t>::min();
        }

        return -static_cast<std::int64_t>(ticks);
    }

    return static_cast<std::int64_t>(std::min(ticks, max));
}

std::uint8_t World::bitsForSigned(std::int64_t ticks) const noexcept
{
    if (ticks >= std::numeric_limits<std::int8_t>::min() && ticks <= std::numeric_limits<std::int8_t>::max())
    {
        return 8;
    }

    if (ticks >= std::numeric_limits<std::int16_t>::min() && ticks <= std::numeric_limits<std::int16_t>::max())
    {
        return 16;
    }

    if (ticks >= std::numeric_limits<std::int32_t>::min() && ticks <= std::numeric_limits<std::int32_t>::max())
    {
        return 32;
    }

    return 64;
}

std::uint8_t World::coordinateBitsForCarrier(std::uint8_t carrierBits) const noexcept
{
    switch (carrierBits)
    {
    case 8:
        return 16;
    case 16:
        return 32;
    default:
        return 64;
    }
}

} // namespace gojo::simd
