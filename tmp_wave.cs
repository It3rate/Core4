using System;
using Core4.AvaloniaProbe.Models;
using Core4.Inspection;
using Core4.Structure;

var state = CarrierChainDemoState.DiamondCross;
var start = Site.Open(new Carrier(1), new Core4.Numerics.Measure(0));
var wave = PropagationWave.Start(state.Topology, start);
var stepIndex = 0;
while (!wave.IsComplete && stepIndex < 20)
{
    var step = wave.Advance();
    Console.WriteLine($"STEP {stepIndex}: before perspective={wave.Perspective.Raw}, after perspective={step.After.Perspective.Raw}, motions={step.Moves.Length}");
    Console.WriteLine("  Active after:");
    foreach (var f in step.After.Active)
    {
        Console.WriteLine($"    {f.Site.Carrier.Id}:{f.Site.Location.Raw} mult={f.Multiplicity.Raw}");
    }
    Console.WriteLine("  Parked after:");
    foreach (var f in step.After.Parked)
    {
        Console.WriteLine($"    {f.Site.Carrier.Id}:{f.Site.Location.Raw} mult={f.Multiplicity.Raw}");
    }
    wave = step.After;
    stepIndex++;
}
