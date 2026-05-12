#pragma once

#include <array>
#include <cstdint>
#include <limits>
#include <optional>
#include <string>
#include <string_view>
#include <tuple>
#include <vector>

namespace gojo::simd {

using Id = std::uint64_t;

enum class ElementStore : std::uint8_t
{
    none = 0,
    carrier = 1,
    tick_ref = 2,
    carrier_location = 3,
    relation = 4,
    unit_shape = 5
};

struct ElementRef
{
    static constexpr std::uint32_t invalid_index = std::numeric_limits<std::uint32_t>::max();

    ElementStore store{ElementStore::none};
    std::uint8_t bits{0};
    std::uint32_t index{invalid_index};

    [[nodiscard]] bool isNone() const noexcept
    {
        return store == ElementStore::none || index == invalid_index;
    }

    friend bool operator==(const ElementRef&, const ElementRef&) = default;
};

struct CarrierRef
{
    std::uint8_t bits{0};
    std::uint32_t index{ElementRef::invalid_index};

    [[nodiscard]] bool isNone() const noexcept
    {
        return bits == 0 || index == ElementRef::invalid_index;
    }

    friend bool operator==(const CarrierRef&, const CarrierRef&) = default;
};

struct TickRefRef
{
    std::uint8_t bits{0};
    std::uint32_t index{ElementRef::invalid_index};

    [[nodiscard]] bool isNone() const noexcept
    {
        return bits == 0 || index == ElementRef::invalid_index;
    }

    friend bool operator==(const TickRefRef&, const TickRefRef&) = default;
};

struct CarrierLocationRef
{
    std::uint32_t index{ElementRef::invalid_index};

    [[nodiscard]] bool isNone() const noexcept
    {
        return index == ElementRef::invalid_index;
    }

    friend bool operator==(const CarrierLocationRef&, const CarrierLocationRef&) = default;
};

struct RelationRef
{
    std::uint32_t index{ElementRef::invalid_index};

    [[nodiscard]] bool isNone() const noexcept
    {
        return index == ElementRef::invalid_index;
    }

    friend bool operator==(const RelationRef&, const RelationRef&) = default;
};

struct UnitShapeRef
{
    std::uint32_t index{ElementRef::invalid_index};

    [[nodiscard]] bool isNone() const noexcept
    {
        return index == ElementRef::invalid_index;
    }

    friend bool operator==(const UnitShapeRef&, const UnitShapeRef&) = default;
};

struct CarrierLocationView
{
    CarrierRef carrier;
    TickRefRef tick;
    std::int64_t ticks{0};
};

struct CarrierState
{
    Id id{0};
    bool isInverse{false};
    bool isUnknown{false};
    std::uint8_t bits{0};
    std::uint64_t value{0};
    std::int64_t tension{0};
    UnitShapeRef unitShape;
    std::optional<CarrierRef> inverse;
};

template <class T, std::size_t Count>
struct Lane
{
    std::array<T, Count> values{};

    [[nodiscard]] T& operator[](std::size_t index) noexcept
    {
        return values[index];
    }

    [[nodiscard]] const T& operator[](std::size_t index) const noexcept
    {
        return values[index];
    }
};

class World
{
public:
    World();

    [[nodiscard]] CarrierRef unknownCarrier() const noexcept;

    [[nodiscard]] CarrierRef createCarrier(std::uint8_t bits = 64, bool inverse = false);
    [[nodiscard]] CarrierRef createCarrier(UnitShapeRef unitShape, std::uint8_t bits = 64, bool inverse = false);
    [[nodiscard]] CarrierRef inverse(CarrierRef carrier);

    [[nodiscard]] TickRefRef createTickRef(std::int64_t ticks, std::uint8_t bits = 0);
    [[nodiscard]] CarrierLocationRef location(CarrierRef carrier, std::int64_t ticks, std::uint8_t bits = 0);
    [[nodiscard]] CarrierLocationRef location(CarrierRef carrier, TickRefRef tick);

    [[nodiscard]] RelationRef relate(ElementRef left, ElementRef right);
    [[nodiscard]] RelationRef point(CarrierRef carrier, std::int64_t ticks);
    [[nodiscard]] RelationRef segment(CarrierRef carrier, std::int64_t start, std::int64_t end);
    [[nodiscard]] RelationRef orthogonalOutlet(ElementRef source);
    [[nodiscard]] RelationRef liveOutput(ElementRef input, RelationRef outputPort);
    [[nodiscard]] RelationRef additiveAttachment(CarrierRef holder, ElementRef participant);
    [[nodiscard]] RelationRef additiveGroup(CarrierRef holder, const std::vector<ElementRef>& participants);

    [[nodiscard]] UnitShapeRef atom(CarrierRef carrier);
    [[nodiscard]] UnitShapeRef orthogonalOf(ElementRef source);
    [[nodiscard]] UnitShapeRef circularFrom(RelationRef pointSource);
    [[nodiscard]] UnitShapeRef linearFrom(RelationRef zeroLengthSegment);
    [[nodiscard]] UnitShapeRef sheetFrom(RelationRef nonzeroSegment);

    RelationRef attach(CarrierRef anchor, RelationRef relation);
    void add(CarrierRef carrier, std::uint64_t ticks);
    void remove(CarrierRef carrier, std::uint64_t ticks);
    void set(CarrierRef carrier, std::int64_t ticks);

    [[nodiscard]] CarrierLocationRef valueSlot(CarrierRef carrier);
    [[nodiscard]] CarrierLocationRef tensionSlot(CarrierRef carrier);

    [[nodiscard]] std::uint64_t value(CarrierRef carrier) const;
    [[nodiscard]] std::int64_t tension(CarrierRef carrier) const;
    [[nodiscard]] CarrierState carrierState(CarrierRef carrier) const;
    [[nodiscard]] CarrierLocationView locationView(CarrierLocationRef location) const;
    [[nodiscard]] ElementRef left(RelationRef relation) const;
    [[nodiscard]] ElementRef right(RelationRef relation) const;
    [[nodiscard]] Id relationId(RelationRef relation) const;
    [[nodiscard]] std::uint32_t carrierCount(std::uint8_t bits) const;
    [[nodiscard]] std::uint32_t tickRefCount(std::uint8_t bits) const;

    [[nodiscard]] ElementRef element(CarrierRef carrier) const noexcept;
    [[nodiscard]] ElementRef element(TickRefRef tick) const noexcept;
    [[nodiscard]] ElementRef element(CarrierLocationRef location) const noexcept;
    [[nodiscard]] ElementRef element(RelationRef relation) const noexcept;
    [[nodiscard]] ElementRef element(UnitShapeRef shape) const noexcept;

    [[nodiscard]] bool tryGetCarrierLocations(RelationRef relation, CarrierLocationRef& left, CarrierLocationRef& right) const;
    [[nodiscard]] bool tryProject(RelationRef relation, CarrierLocationRef location, CarrierRef reader, RelationRef& projected);
    [[nodiscard]] RelationRef project(RelationRef relation, CarrierLocationRef location, CarrierRef reader);
    [[nodiscard]] bool tryReadOperationFrom(RelationRef operation, ElementRef input, CarrierRef reader, RelationRef& read);
    [[nodiscard]] bool tryCrystallize(RelationRef relation, CarrierRef& carrier);
    [[nodiscard]] bool tryReadCircularOrigin(CarrierRef carrier, CarrierLocationRef& origin) const;
    [[nodiscard]] bool tryReadLinearOrigin(CarrierRef carrier, CarrierLocationRef& origin) const;
    [[nodiscard]] bool compatibleUnitShape(UnitShapeRef left, UnitShapeRef right) const;
    [[nodiscard]] bool sameElementValue(ElementRef left, ElementRef right) const;

    [[nodiscard]] std::string describe(ElementRef element) const;
    [[nodiscard]] std::string describe(CarrierRef carrier) const;
    [[nodiscard]] std::string describe(RelationRef relation) const;

private:
    struct CarrierFrame
    {
        CarrierRef carrier;
        std::int64_t start{0};
        std::int64_t end{0};

        [[nodiscard]] std::int64_t extent() const noexcept
        {
            return end - start;
        }
    };

    struct CarrierSites
    {
        CarrierRef carrier;
        std::int64_t min{0};
        std::int64_t max{0};

        [[nodiscard]] std::int64_t width() const noexcept
        {
            return max - min;
        }

        [[nodiscard]] bool isOrigin() const noexcept
        {
            return width() == 0 && min == 0;
        }
    };

    struct SurfaceProjection
    {
        CarrierLocationRef target;
        CarrierLocationRef residue;
    };

    struct OperationOperand
    {
        std::int64_t ticks{0};
        CarrierLocationRef residue;
    };

    struct OperationBasis
    {
        std::int64_t extent{0};
        bool dominant{false};
    };

    template <class Unsigned>
    struct CarrierBlock
    {
        std::vector<Id> ids;
        std::vector<bool> inverseFlags;
        std::vector<UnitShapeRef> unitShapes;
        std::vector<std::optional<CarrierRef>> inverses;
        std::vector<TickRefRef> valueTicks;
        std::vector<Unsigned> values;
        std::vector<std::int64_t> tensions;
        std::vector<std::vector<std::uint32_t>> attachedRelations;
    };

    template <class Signed>
    struct TickBlock
    {
        std::vector<Id> ids;
        std::vector<Signed> ticks;
    };

    struct LocationRow
    {
        CarrierRef carrier;
        TickRefRef tick;
    };

    struct RelationRow
    {
        Id id{0};
        ElementRef left;
        ElementRef right;
    };

    struct UnitShapeRow
    {
        Id id{0};
        ElementRef surface;
    };

    CarrierRef createCarrierRaw(std::uint8_t bits, Id id, bool inverse, UnitShapeRef unitShape);
    UnitShapeRef createUnitShape(ElementRef surface);

    [[nodiscard]] bool isSupportedCarrierBits(std::uint8_t bits) const noexcept;
    [[nodiscard]] bool isSupportedSignedBits(std::uint8_t bits) const noexcept;
    [[nodiscard]] bool sameCarrier(CarrierRef left, CarrierRef right) const noexcept;
    [[nodiscard]] bool isUnknown(CarrierRef carrier) const;
    [[nodiscard]] bool isInverse(CarrierRef carrier) const;
    [[nodiscard]] UnitShapeRef unitShapeOf(CarrierRef carrier) const;
    [[nodiscard]] std::optional<CarrierRef> inverseOf(CarrierRef carrier) const;
    [[nodiscard]] TickRefRef valueTickOf(CarrierRef carrier) const;
    [[nodiscard]] std::vector<std::uint32_t>& attachedRelationsOf(CarrierRef carrier);
    [[nodiscard]] const std::vector<std::uint32_t>& attachedRelationsOf(CarrierRef carrier) const;

    void setUnitShape(CarrierRef carrier, UnitShapeRef unitShape);
    void setInverse(CarrierRef carrier, CarrierRef inverse);
    void setValue(CarrierRef carrier, std::uint64_t value);
    void setTick(TickRefRef tick, std::int64_t ticks);
    void addResidualTension(CarrierRef carrier, std::int64_t ticks);
    bool keepTensionRemainder(CarrierRef carrier, std::int64_t ticks);
    bool leaveTension(CarrierRef carrier, std::uint64_t value, std::int64_t tension);
    bool balanceOpposedValueWith(CarrierRef left, CarrierRef right);
    void receive(CarrierRef carrier, std::int64_t ticks);
    bool receiveTo(CarrierRef carrier, std::int64_t ticks);
    void absorbCarrier(CarrierRef carrier);
    void absorbAllCarrierBlocks();
    void settleWorld(CarrierRef anchor);

    [[nodiscard]] std::vector<CarrierRef> connectedCarriers(CarrierRef anchor) const;
    [[nodiscard]] std::vector<CarrierRef> carriersOf(ElementRef element) const;
    [[nodiscard]] std::vector<CarrierLocationRef> carrierSitesOf(ElementRef element) const;
    [[nodiscard]] bool tryReadOneCarrier(const std::vector<CarrierLocationRef>& sites, CarrierSites& carrierSites) const;
    [[nodiscard]] std::vector<std::tuple<std::uint8_t, std::uint32_t, std::uint64_t, std::int64_t>> snapshot(const std::vector<CarrierRef>& carriers) const;
    [[nodiscard]] bool applyRelation(RelationRef relation);
    [[nodiscard]] bool accountLocal(CarrierRef carrier, std::int64_t source, std::int64_t target);
    [[nodiscard]] bool routePositiveTension(const CarrierSites& source, const CarrierSites& receiver);
    [[nodiscard]] bool routeNegativeTension(const CarrierSites& source, const CarrierSites& receiver);

    [[nodiscard]] bool tryApplyLiveOutput(RelationRef relation);
    [[nodiscard]] bool tryApplyLiveProjectionOutput(ElementRef input, RelationRef port);
    [[nodiscard]] bool tryApplyLiveOperationOutput(ElementRef input, RelationRef operation);
    [[nodiscard]] bool applyLiveOutputTargets(const std::vector<std::pair<CarrierRef, std::int64_t>>& targets);

    [[nodiscard]] bool tryReadPoint(ElementRef element, CarrierLocationRef& location, CarrierRef& carrier) const;
    [[nodiscard]] bool tryReadFrame(ElementRef element, CarrierFrame& frame) const;
    [[nodiscard]] bool tryReadPointFrame(ElementRef element, CarrierFrame& frame) const;
    [[nodiscard]] bool tryReadBasis(ElementRef element, CarrierLocationRef& basis);
    [[nodiscard]] bool frameAccepts(const CarrierFrame& frame, CarrierRef carrier) const;
    [[nodiscard]] bool frameAcceptsProjectionSource(const CarrierFrame& frame, CarrierRef carrier) const;
    [[nodiscard]] CarrierFrame resolveFrameCarrier(const CarrierFrame& frame, CarrierRef carrier) const;
    [[nodiscard]] CarrierFrame resolveProjectionSourceFrame(const CarrierFrame& frame, CarrierRef carrier) const;

    [[nodiscard]] bool tryProjectBetweenSurfaces(ElementRef left, ElementRef right, CarrierLocationRef location, CarrierRef reader, RelationRef& projected);
    [[nodiscard]] RelationRef projectBetween(CarrierLocationRef location, CarrierRef reader, const CarrierFrame& source, const CarrierFrame& target);
    [[nodiscard]] RelationRef projectionResult(CarrierLocationRef projected, CarrierLocationRef residue);
    [[nodiscard]] bool tryProjectToTargetSurface(ElementRef sourceSurface, ElementRef targetSurface, CarrierLocationRef input, SurfaceProjection& projection);
    [[nodiscard]] bool tryProjectThrough(ElementRef sourceSurface, ElementRef targetSurface, CarrierLocationRef input, CarrierRef reader, SurfaceProjection& projection);

    [[nodiscard]] bool tryReadPortSurface(ElementRef element, RelationRef& port) const;
    [[nodiscard]] bool tryCollectOperationOperands(ElementRef portSurface, ElementRef input, CarrierRef reader, std::vector<OperationOperand>& operands);
    [[nodiscard]] bool tryCollectOperationOperand(RelationRef port, ElementRef input, CarrierRef reader, std::vector<OperationOperand>& operands);
    [[nodiscard]] bool tryCollectOperationOperand(RelationRef port, CarrierLocationRef input, CarrierRef reader, std::vector<OperationOperand>& operands);
    [[nodiscard]] bool tryProjectOperationOperandThrough(ElementRef sourceSurface, ElementRef targetSurface, CarrierLocationRef input, CarrierRef reader, SurfaceProjection& projection);
    [[nodiscard]] SurfaceProjection projectOperationOperandBetween(CarrierLocationRef input, CarrierRef reader, const CarrierFrame& source, const CarrierFrame& target);
    [[nodiscard]] bool tryReadAtomicFrameInput(ElementRef input, ElementRef sourceSurface, CarrierLocationRef& inputExtent);
    [[nodiscard]] bool tryResolveOperationTargetFrame(const CarrierFrame& frame, CarrierRef reader, bool bindOutputToReader, CarrierFrame& targetFrame) const;
    [[nodiscard]] RelationRef operationResult(const CarrierFrame& targetFrame, std::int64_t value, const std::vector<OperationOperand>& operands, bool scaleByTargetFrame = true);
    [[nodiscard]] ElementRef operationResidue(const std::vector<OperationOperand>& operands);

    [[nodiscard]] bool tryReadOrthogonalSource(ElementRef element, ElementRef& source) const;
    [[nodiscard]] bool tryReadSegment(RelationRef relation, CarrierLocationRef& left, CarrierLocationRef& right) const;
    [[nodiscard]] bool isUnitPoint(RelationRef relation) const;
    [[nodiscard]] bool isZeroSegment(ElementRef element) const;

    [[nodiscard]] std::int64_t tickValue(TickRefRef tick) const;
    [[nodiscard]] std::uint64_t magnitudeOf(std::int64_t ticks) const noexcept;
    [[nodiscard]] std::int64_t signedTicks(std::uint64_t ticks, int sign) const;
    [[nodiscard]] std::uint8_t bitsForSigned(std::int64_t ticks) const noexcept;
    [[nodiscard]] std::uint8_t coordinateBitsForCarrier(std::uint8_t carrierBits) const noexcept;

    CarrierBlock<std::uint8_t> carriers8_;
    CarrierBlock<std::uint16_t> carriers16_;
    CarrierBlock<std::uint32_t> carriers32_;
    CarrierBlock<std::uint64_t> carriers64_;

    TickBlock<std::int8_t> ticks8_;
    TickBlock<std::int16_t> ticks16_;
    TickBlock<std::int32_t> ticks32_;
    TickBlock<std::int64_t> ticks64_;

    std::vector<LocationRow> locations_;
    std::vector<RelationRow> relations_;
    std::vector<UnitShapeRow> unitShapes_;

    CarrierRef unknown_;
    Id nextCarrierId_{1};
    Id nextTickId_{1};
    Id nextRelationId_{1};
    Id nextUnitShapeId_{1};
    bool settling_{false};
};

} // namespace gojo::simd
