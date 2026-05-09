# Core4 / Gojo Agent Instructions

## Active Scope

- Gojo is the current Core4 kernel model. New engine work belongs in `Gojo`.
- Use `Gojo.slnx` for builds and tests unless the user explicitly asks for historical comparison.
- Core code lives in `Gojo/Core`.
- Tests live in `Gojo.Tests`.
- The current code-only Avalonia/Skia demo lives in `Gojo.GearDemo`, with desktop and browser hosts wrapping that shared project.
- Older Core4, Kernel, Runtime, Probe, and historical projects are comparison material only. Do not use the old Core4 engine for new Gojo behavior.

## Gojo Structure Rules

- Use Gojo carriers, locations, relations, unit shapes, and tension as the canonical internal representation.
- A concept exists only when its parts are connected by Gojo relation structure. Code may inspect that structure, but code objects must not become the source of meaning.
- Before adding a class, record, enum, metadata field, or wrapper, ask: if this code artifact disappeared, would the Gojo structure still represent the concept? If not, build the relation/carrier structure first.
- Prefer local discovery from a carrier, relation, or unit shape over static all-knowing classifiers that receive unrelated pieces and decide globally what they mean.
- Meaning should be physically traversable through connected structure. Avoid standalone bags of related values.
- If a concept is hard to express, extend the Gojo numeric/structural format rather than falling back to symbolic operation enums, opaque tags, ad hoc metadata, or case-by-case dispatch.
- Keep representations physically interpretable:
  - larger things should generally be represented by larger numbers
  - intermediate states should remain inspectable as numeric, structural, or tension-bearing states
  - structure should be traversable, perturbable, and discoverable numerically
- Encode branches, loops, joins, continuation, composition, calibration, and routes as relation structure where possible.
- Prefer calculated and locally read values to stored duplicates. Assume structures are dynamic.
- Avoid global snapshots and avalanche-style read models as sources of behavior. Inspection helpers are fine when they read local structure on demand.
- Stable crystallized structures such as `UnitShape` are valid when stability is the concept being represented. Do not confuse that with app-style immutable state models.
- Incomplete or incompatible states are information. For Gojo-domain behavior, prefer tension, failed reads, or explicit unresolved structure over exceptions and guard-heavy invalid-value checks.

## Tension, Residue, And Reads

- Tension is unresolved transferred change that must remain available to settlement.
- Read residue is the remainder from interpreting an existing source through a requested basis or resolution. If the source still exists, do not persist read residue just to work around the resolution system.
- Preserve residue as tension when transfer/import/settlement would otherwise discard incoming change.
- A lower-resolution read is not an error; it is the requested resolution. If another resolution is needed, expose and read through that structure.

## Circular And Orthogonal Mechanics

- `Relation.Point(C,n)`, `Relation.Segment(C,a,b)`, and `Relation.OrthogonalOutlet(A)` are official relation constructors.
- `UnitShape.CircularFrom(point)` creates a circular/level-contour carrier from a point orthogonal. It does not by itself know radius, direction, tooth geometry, or physical mesh behavior.
- `UnitShape.LinearFrom(zeroLengthSegment)` creates a radial/linear carrier from a zero-length segment orthogonal. It does not by itself know external direction.
- A radius calibration is ordinary connected structure, for example `Point(radius, r) -> Point(perimeter, p)`, between a radial endpoint and a perimeter marker whose source origins align.
- Do not create circle/calibration wrapper classes that merely collect origin, radius, marker, and direction. The circle must remain inspectable from carrier/unit-shape/relation structure after any helper code is removed.
- Forward perimeter faces are the normal contour face; inverse perimeter faces are the opposed contour face. Richer handedness requires explicit structure.

## UI And Demo Code

- Use Avalonia for UI code, but never use XAML.
- Use native Skia rendering and render ticks. Redraw the areas that need updating.
- Keep UI logic separate from Gojo core unless a missing kernel capability is clearly discovered.
- Reusable UI pieces can live in component libraries, but they should not own Gojo meaning.
- Convert Gojo structures to `int`, `double`, pixels, strings, or display DTOs only at adapter/render boundaries.
- Avoid React-style state snapshots and broad global read records. Prefer local observers/inspectors over the structures being displayed.
- Use `Core4.AvaloniaProbe` only as structural guidance for code-only Avalonia/Skia patterns.

## Testing

- Write tests for stable Gojo concepts, especially numeric core behavior, calibration, structural composition, unit-shape changes, and tension/settlement behavior.
- UI, glue code, and rendering wrappers do not need tests unless they contain real logic, risk, or useful behavioral guarantees.
- Run:

```powershell
dotnet test Gojo.slnx
```

- For browser-target safety after demo/core changes, also run:

```powershell
dotnet build Gojo.GearDemo.Browser\Gojo.GearDemo.Browser.csproj
```

## Axioms And Source Of Truth

- Treat `Gojo/axioms` and the current Gojo code as the active source of truth.
- Older axioms and historical projects are useful comparison material, but Gojo wins when there is conflict.
- If the axioms are incomplete, unclear, or conflict with the code or the user's current direction, call that out so the model can be refined.
