using System.Collections.Immutable;
using Core4.Dynamics;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.Examples;

public static class SiteExamples
{
    public static ImmutableArray<(string Name, Site Site)> Create()
    {
        var x = new Carrier(1);
        var lightSensor = new Carrier(2);
        var switchState = new Carrier(3);
        var navigation = new Carrier(4);
        var open = Site.Open(x, 0);
        var negotiating = open
            .Anchor(new Ratio(0, 1))
            .WithResidual(Ratio.Tension(3))
            .Request(lightSensor, Ratio.Tension(4))
            .Request(switchState, Ratio.Tension(6))
            .Exchange(navigation, Ratio.Tension(2), Provenance.Shared(2));
        var committed = negotiating.Resolve(new Ratio(10, 5));

        return
        [
            ("open", open),
            ("negotiating", negotiating),
            ("committed", committed)
        ];
    }

    public static ImmutableArray<string> Describe()
    {
        var examples = Create();
        var descriptions = ImmutableArray.CreateBuilder<string>(examples.Length);

        foreach (var (name, site) in examples)
        {
            var step = site.Settle();
            var phases = step.Phases;
            var terms = ImmutableArray.CreateBuilder<string>(phases.Samples.Length);
            phases.Reset();

            while (phases.MoveNext())
            {
                terms.Add($"{phases.CurrentIndex.Raw}:{phases.Current}={step.Current.Normalize()}");
            }

            descriptions.Add(
                $"{name}: position={site.Position.Normalize()}, onCarrier={site.On(site.Carrier).Normalize()}, " +
                $"potential={site.Potential.Raw}, kernel=[r={site.Kernel.Recessive.Normalize()}, d={site.Kernel.Dominant.Normalize()}], " +
                $"orthogonal=[residual={site.Orthogonal.Residual.Normalize()}, connections={site.Orthogonal.ConnectionCount.Raw}, " +
                $"incoming={site.Orthogonal.Incoming().Normalize()}, outgoing={site.Orthogonal.Outgoing().Normalize()}, oscillating={site.Orthogonal.Oscillating().Normalize()}], " +
                $"settle=[anchor={step.Anchor.Normalize()}, in={step.Incoming.Normalize()}, provisional={step.Provisional.Normalize()}, " +
                $"published={step.Published.Normalize()}, feedback={step.Feedback.Normalize()}, settled={step.Settled.Normalize()}, remainder={step.Remainder.Normalize()}], " +
                $"terms=[{string.Join(", ", terms)}]");
        }

        return descriptions.MoveToImmutable();
    }
}
