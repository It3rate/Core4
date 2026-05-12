#include "gojo_internal.hpp"

namespace gojo::simd {

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


} // namespace gojo::simd

