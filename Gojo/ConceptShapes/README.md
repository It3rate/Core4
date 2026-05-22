# Gojo ConceptShapes

First-pass helpers that build and read **concept shapes** as ordinary Gojo
relation structure. Design reference: `ConceptShapesCursor.md`.

## Layout

| File | Role |
|------|------|
| `ConceptPrimitives.cs` | Codex generators (site, span, radial potential, wrap, …) |
| `ConceptCompositions.cs` | Corner, passage, funnel, peninsula, open attach, void sink |
| `ConceptShape.cs` | Ordered-group package `Create` / `TryRead` |
| `ConceptShapeExamples.cs` | Sample assemblies for exploration |
| `ConceptShapeRoot.cs` | Internal ordered-group reads |

## Usage

```csharp
using Gojo.ConceptShapes;

var shape = ConceptShapeExamples.CornerSeed();
ConceptShape.TryRead(shape, out _, out var selected, out var topology, ...);

var openSlots = ConceptShape.ReadOpenSlotBindings(parameterSurface);
```

## Not implemented yet

- `CoLocated` without exchange marker
- `FillClaim` boundary vs interior
- Material vs observation branch kinds
- `TrySuggestWords` interpretive layer
- Participation quadrant mask helper
- Full concept shape tests (add under `Gojo.Tests` when ready)
