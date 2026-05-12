#include "gojo_internal.hpp"

namespace gojo::simd {

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

