#include "gojoSIMD/gojo.hpp"

#include <cstdlib>
#include <iostream>
#include <string>

using gojo::simd::CarrierLocationRef;
using gojo::simd::CarrierRef;
using gojo::simd::ElementStore;
using gojo::simd::RelationRef;
using gojo::simd::World;

namespace {

void expect(bool condition, const std::string& message)
{
    if (!condition)
    {
        std::cerr << "FAILED: " << message << '\n';
        std::exit(1);
    }
}

void expectValue(World& world, CarrierRef carrier, std::uint64_t value, std::int64_t tension, const std::string& label)
{
    expect(world.value(carrier) == value, label + " value");
    expect(world.tension(carrier) == tension, label + " tension");
}

CarrierLocationRef leftLocation(World& world, RelationRef relation)
{
    const auto left = world.left(relation);
    expect(left.store == ElementStore::carrier_location, "relation left is a carrier location");
    return CarrierLocationRef{left.index};
}

CarrierLocationRef rightLocation(World& world, RelationRef relation)
{
    const auto right = world.right(relation);
    expect(right.store == ElementStore::carrier_location, "relation right is a carrier location");
    return CarrierLocationRef{right.index};
}

void bareCarrierAndResolutionPressure()
{
    World world;
    const auto carrier = world.createCarrier(16);

    world.add(carrier, 17);
    world.remove(carrier, 5);

    expectValue(world, carrier, 12, 0, "bare carrier");

    const auto byteCarrier = world.createCarrier(8);
    world.add(byteCarrier, 300);

    expectValue(world, byteCarrier, 255, 45, "u8 carrier exposes overflow as tension");
    expect(world.carrierCount(8) >= 1, "u8 carrier lives in the 8-bit carrier block");
}

void wrapAndCarry()
{
    World world;
    const auto residue = world.createCarrier(8);
    const auto whole = world.createCarrier(16);
    const auto route = world.relate(
        world.element(world.point(residue, 10)),
        world.element(world.point(whole, 0)));

    world.attach(residue, route);
    world.add(residue, 234);

    expectValue(world, residue, 4, 0, "residue route");
    expectValue(world, whole, 23, 0, "whole route");
}

void binaryCarryUsesSameShape()
{
    World world;
    const auto ones = world.createCarrier(8);
    const auto twos = world.createCarrier(8);
    const auto fours = world.createCarrier(8);
    const auto eights = world.createCarrier(8);

    world.attach(ones, world.relate(world.element(world.point(ones, 2)), world.element(world.point(twos, 0))));
    world.attach(twos, world.relate(world.element(world.point(twos, 2)), world.element(world.point(fours, 0))));
    world.attach(fours, world.relate(world.element(world.point(fours, 2)), world.element(world.point(eights, 0))));

    world.add(ones, 13);

    expectValue(world, ones, 1, 0, "ones");
    expectValue(world, twos, 0, 0, "twos");
    expectValue(world, fours, 1, 0, "fours");
    expectValue(world, eights, 1, 0, "eights");
}

void projectionReadsKeepResidue()
{
    World world;
    const auto source = world.createCarrier(16);
    const auto target = world.createCarrier(16);
    const auto ratio = world.relate(
        world.element(world.point(source, 2)),
        world.element(world.point(target, 3)));

    auto projected = world.project(ratio, world.location(source, 8), target);
    expect(world.locationView(leftLocation(world, projected)).ticks == 12, "exact projection target");
    expect(world.locationView(rightLocation(world, projected)).ticks == 0, "exact projection residue");

    projected = world.project(ratio, world.location(target, 10), source);
    expect(world.locationView(leftLocation(world, projected)).ticks == 6, "backward projection target");
    expect(world.locationView(rightLocation(world, projected)).ticks == 2, "backward projection residue");

    const auto subtract = world.relate(
        world.element(world.point(source, 1)),
        world.element(world.point(target, -1)));
    projected = world.project(subtract, world.location(source, 9), target);
    expect(world.locationView(leftLocation(world, projected)).ticks == -9, "negative projection target");
}

void unitShapesRemainRelationSurfaces()
{
    World world;
    const auto origin = world.createCarrier(16);
    const auto perimeterShape = world.circularFrom(world.point(origin, 5));
    const auto radiusShape = world.linearFrom(world.segment(origin, 5, 5));
    const auto perimeter = world.createCarrier(perimeterShape, 16);
    const auto radius = world.createCarrier(radiusShape, 16);

    CarrierLocationRef perimeterOrigin;
    CarrierLocationRef radiusOrigin;
    expect(world.tryReadCircularOrigin(perimeter, perimeterOrigin), "perimeter reads circular origin");
    expect(world.tryReadLinearOrigin(radius, radiusOrigin), "radius reads linear origin");
    expect(world.locationView(perimeterOrigin).carrier == origin, "circular origin carrier");
    expect(world.locationView(radiusOrigin).carrier == origin, "linear origin carrier");
    expect(world.locationView(perimeterOrigin).ticks == 5, "circular origin ticks");
    expect(world.locationView(radiusOrigin).ticks == 5, "linear origin ticks");
}

void operationSurfacesReadFromPortStructure()
{
    World world;
    const auto x = world.createCarrier(16);
    const auto y = world.createCarrier(16);
    const auto result = world.createCarrier(16);
    const auto xPort = world.relate(world.element(world.point(x, 1)), world.element(world.point(world.unknownCarrier(), 1)));
    const auto yPort = world.relate(world.element(world.point(y, 1)), world.element(world.point(world.unknownCarrier(), 1)));

    world.set(x, 3);
    world.set(y, 4);

    const auto input = world.relate(world.element(world.valueSlot(x)), world.element(world.valueSlot(y)));
    const auto addOperation = world.relate(world.element(world.relate(world.element(xPort), world.element(yPort))), world.element(world.point(result, 1)));

    RelationRef read;
    expect(world.tryReadOperationFrom(addOperation, world.element(input), result, read), "add operation reads");
    expect(world.locationView(leftLocation(world, read)).ticks == 7, "add operation result");

    const auto multiplyOperation = world.relate(
        world.element(world.relate(world.element(xPort), world.element(yPort))),
        world.element(world.segment(result, 0, 1)));

    expect(world.tryReadOperationFrom(multiplyOperation, world.element(input), result, read), "multiply operation reads");
    expect(world.locationView(leftLocation(world, read)).ticks == 12, "multiply operation result");
}

void liveOutputFeedsTargetCarrier()
{
    World projectionWorld;
    const auto source = projectionWorld.createCarrier(16);
    const auto target = projectionWorld.createCarrier(16);
    const auto port = projectionWorld.relate(
        projectionWorld.element(projectionWorld.segment(source, 0, 2)),
        projectionWorld.element(projectionWorld.segment(target, 0, 1)));

    projectionWorld.attach(source, projectionWorld.liveOutput(projectionWorld.element(projectionWorld.valueSlot(source)), port));
    projectionWorld.set(source, 8);
    expectValue(projectionWorld, target, 4, 0, "live target after first set");

    projectionWorld.set(source, 10);
    expectValue(projectionWorld, target, 5, 0, "live target after second set");

    World operationWorld;
    const auto x = operationWorld.createCarrier(16);
    const auto y = operationWorld.createCarrier(16);
    const auto result = operationWorld.createCarrier(16);
    const auto xPort = operationWorld.relate(
        operationWorld.element(operationWorld.point(x, 1)),
        operationWorld.element(operationWorld.point(operationWorld.unknownCarrier(), 1)));
    const auto yPort = operationWorld.relate(
        operationWorld.element(operationWorld.point(y, 1)),
        operationWorld.element(operationWorld.point(operationWorld.unknownCarrier(), 1)));
    const auto operation = operationWorld.relate(
        operationWorld.element(operationWorld.relate(operationWorld.element(xPort), operationWorld.element(yPort))),
        operationWorld.element(operationWorld.segment(result, 0, 1)));
    const auto input = operationWorld.relate(operationWorld.element(operationWorld.valueSlot(x)), operationWorld.element(operationWorld.valueSlot(y)));

    operationWorld.attach(x, operationWorld.liveOutput(operationWorld.element(input), operation));
    operationWorld.set(x, 3);
    operationWorld.set(y, 4);
    expectValue(operationWorld, result, 12, 0, "live operation result");

    operationWorld.set(x, 5);
    expectValue(operationWorld, result, 20, 0, "live operation updated result");
}

} // namespace

int main()
{
    bareCarrierAndResolutionPressure();
    wrapAndCarry();
    binaryCarryUsesSameShape();
    projectionReadsKeepResidue();
    unitShapesRemainRelationSurfaces();
    operationSurfacesReadFromPortStructure();
    liveOutputFeedsTargetCarrier();

    std::cout << "gojoSIMD tests passed\n";
    return 0;
}
