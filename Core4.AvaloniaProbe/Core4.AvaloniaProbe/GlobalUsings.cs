// The Probe remains a consumer of the reorganized Core4 API rather than
// reintroducing compatibility types into the kernel.
global using Core4.Aliases;
global using Core4.Encoding;
global using Core4.Interpretation;
global using Core4.State;
global using Core4.Topology;
global using CoreMeasure = Core4.Encoding.Measure;

// These empty namespaces keep the Probe's historical using directives valid
// while the types themselves resolve through the current namespaces above.
namespace Core4.Numerics { }

namespace Core4.Structure { }
