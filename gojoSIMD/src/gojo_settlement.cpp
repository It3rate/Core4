#include "gojo_internal.hpp"

namespace gojo::simd {

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


} // namespace gojo::simd

