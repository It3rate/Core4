#include "gojo_internal.hpp"

namespace gojo::simd {

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


} // namespace gojo::simd

