# ConceptUnits: Gojo-Centric Concept Encoding

## Purpose

`ConceptUnits` is a low-resolution concept grammar for representing ideas as
physical, traversable, mathematically computable Gojo structures.

The central goal is:

- start from low-resolution concept seeds,
- preserve physical interpretability,
- progressively refine toward higher-resolution design,
- keep all meanings inspectable through carriers, relations, unit shapes,
  flow contracts, and settlement behavior.

Mechanical examples are used as a rich concept space (bridge, funnel, corner,
fastener, nozzle, dock), but this grammar is intended for language in general:
product concepts, social concepts, financial concepts, and metaphorical reuse.

## Why low resolution first

Concept work behaves like solving a polynomial in stages:

1. identify coarse structural terms,
2. preserve ambiguity where it is meaningful,
3. add constraints and calibration only when needed,
4. resolve into higher-order coherent structure.

A "bridge" at low resolution is not yet a CAD bridge. It is a crossable neck
between regions. Later, constraints can transform it into filter-like,
one-way conduit-like, or dock-like forms. Later still, it can become a
specific engineering design (or even a finance concept such as a bridge round)
through descriptor grounding and additional relation structure.

## Gojo grounding assumptions

Concept encoding should remain Gojo-native:

- meaning exists in connected structure, not metadata tags,
- orthogonal structure expresses unaccounted participation,
- tension is meaningful intermediate state,
- resolution and calibration are local read choices,
- descriptors ground provenance when needed without destroying algebraic
  transfer.

## Primitive classes

### 1) Carrier and directional primitives

- positive/opposed carrier participation,
- aligned vs inverse traversal reads,
- explicit directional segment participation,
- orthogonal outlet participation.

These are the minimum ingredients for directional semantics without overfitting
an ontology.

### 2) Shape seed primitives

- point seed,
- linear seed,
- circular/contour seed,
- sheet seed,
- opposed face variant.

These are low-resolution unit-shape seeds that can be refined through relation
structure and calibration.

### 3) Composition primitives

- additive coexistence,
- product (orthogonal) participation,
- repeated participation,
- aligned composition,
- crossed composition.

Composition should stay visible in structure to support inspection and
metaphorical transfer.

### 4) Topology primitives

- area,
- line route,
- neck,
- bridge neck,
- H-link,
- stem join,
- split node,
- merge node,
- loop closure,
- dead-end sink,
- bypass branch,
- fixed anchor site.

These form the minimal topological vocabulary for many mechanical concepts.

### 5) Flow contract primitives

- unspecified traversal,
- bidirectional declared,
- forward constrained,
- inverse constrained,
- radial convergent,
- radial divergent.

Important distinction: unspecified traversal is not equivalent to explicit
bidirectionality.

### 6) Parameter policy primitives

- none (unparameterized concept seed),
- derived (computed from structure),
- tunable (explicitly exposed).

Adding tunable parameters often changes concept meaning (e.g., bridge -> filter
like behavior), even when topology is unchanged.

### 7) Settlement policy primitives

- pass,
- retain,
- latch,
- release,
- dissipate,
- sacrifice.

Settlement policy should be physically interpretable. "Friction" should emerge
from explicit structure that retards traversal, not from opaque concept labels.

### 8) Focus policy primitives

- neck,
- join,
- boundary,
- gradient,
- contour,
- novelty.

Focus selects the structural window being read. Many concept differences are
focus+flow differences over similar topology.

## Concept unit schema

A `ConceptUnit` is represented by:

- topology primitive set,
- flow contract,
- parameter policy,
- settlement policy,
- focus policy,
- optional notes.

This schema allows low-resolution concept storage and systematic refinement.

## Example progression

1. `Bridge`: bridge-neck + unspecified traversal + no parameters.
2. `Conduit`: same topology + forward constrained flow.
3. `Filter`: same topology + tunable bound parameter.
4. `Funnel`: convergent radial flow contract onto neck.
5. `Nozzle`: funnel + aligned outlet route.
6. `Diffuser/Sprayer`: diverging radial contract on output side.

All are structural refinements of related seeds rather than unrelated class
hierarchies.

## Language and metaphor extension

Descriptors can re-ground a structural concept without changing its algebraic
core:

- mechanical bridge,
- bridge round (finance),
- bridge sentence (rhetoric).

This enables metaphor as structural transfer with changed provenance.

## Missing pieces to implement next

- canonical relation constructors for focus windows,
- constraint relation helpers for bounded-parameter semantics,
- novelty reads comparing baseline vs refined concept contracts,
- explicit examples in tests showing concept refinement across resolutions.


## Concrete implementation in Gojo

This section describes how each ConceptUnits field maps to concrete Gojo
structures using existing kernel primitives.

### 1) Topology primitive -> Relation skeleton

Topology is encoded as relation structure rooted in points, segments, and
orthogonal outlets.

Core constructors:

- `Relation.Point(carrier, ticks)`
- `Relation.Segment(carrier, start, end)`
- `Relation.DirectedSegment(carrier, inverseStart, positiveEnd)`
- `Relation.OrthogonalOutlet(source)`

Implementation rule: a topology primitive is valid only if removing helper
objects still leaves an inspectable relation graph representing the concept.

Examples:

- `BridgeNeck`:
  - area A boundary point -> neck segment -> area B boundary point.
- `HLink`:
  - line route L1 and line route L2 joined by one neck segment at aligned join
    points.
- `StemJoin`:
  - one stem segment and one branch segment sharing a single join point.

### 2) Flow contract -> directed participation read

Flow does not require a separate ontology object. It is read from directed
participation in the topology relation.

- `UnspecifiedTraversal`: crossing relation exists, no directional constraint
  relation attached.
- `BidirectionalDeclared`: explicit paired directional participation in both
  senses.
- `ForwardConstrained` / `InverseConstrained`: attach one-sided directional
  constraint relation so opposite direction accumulates residue/tension.
- `RadialConvergent` / `RadialDivergent`: use orthogonal/circular seed routes
  whose local reads point toward or away from the neck.

### 3) Shape seeds -> UnitShape constructors

Use unit-shape seeds directly and keep them unparameterized unless the concept
requires calibration.

- point seed: `UnitShape.Atom(carrier)`
- linear seed: `UnitShape.LinearFrom(zeroLengthSegment)`
- contour seed: `UnitShape.CircularFrom(point)`
- sheet seed: `UnitShape.SheetFrom(segment)`
- opposed face: `UnitShape.OpposedFaceOf(shape)`

This preserves low-resolution meaning while allowing later refinement.

### 4) Parameters -> descriptor slots and bounded relations

Parameter presence is structural, not metadata.

- no parameter: no descriptor slot bound to the focused primitive.
- derived parameter: computed from existing topology/read.
- tunable parameter: explicit descriptor slot and bound relation.

Mechanism:

1. allocate slot using `UnitShape.DescribedAtom(descriptor)` or
   `UnitShape.DescriptorSlot(index)` in algebra surfaces,
2. bind slot through relation structure connected to the focused primitive,
3. optionally add bounds using carrier-face or segment interval relations.

Example: bridge -> filter refinement

- bridge seed: neck only, unspecified traversal.
- add tunable bound slot on neck traversal extent.
- add directional bound relation; concept now behaves as a constrained filter or
  conduit depending read context.

### 5) Settlement policy -> explicit relation outcomes

Settlement policy is represented as structural outcome relations instead of
opaque tags.

- `Pass`: transfer relation with minimal retained residue.
- `Retain`: route unresolved participation into a retention branch attached to
  the focused primitive.
- `Latch`: two stable settlement basins connected by a threshold relation.
- `Release`: explicit release route from retained/latch basin.
- `Dissipate`: fan-out branch from focus into a larger area topology.
- `Sacrifice`: threshold relation that irreversibly disconnects or blocks a
  branch after overload.

Note on friction: friction should be modeled as physically interpretable
retarding structure (e.g., retention branch and directional resistance), not as
an abstract standalone label.

### 6) Focus policy -> temporary read groups

Focus is represented as a local read scope over existing structure.

Practical encoding approach:

- construct a temporary relation group containing the selected subgraph,
- expose a live output relation from the group to the read port,
- evaluate flow/settlement/parameter reads only within this scope.

This allows the same topology to yield different concepts under different focus
windows (e.g., bridge vs choke vs funnel-like read).

### 7) Recommended build order for code

1. Add concept builders that return relation skeletons for each
   `TopologyPrimitive`.
2. Add flow decorators that attach directional constraints without changing
   topology identity.
3. Add parameter-slot binders and bounded relation helpers.
4. Add settlement outcome decorators (retain/latch/release/dissipate).
5. Add focus-scope helper for localized reads.
6. Add tests in `Gojo.Tests` validating progressive refinement:
   - bridge seed,
   - bridge + flow constraint,
   - bridge + tunable parameter (filter-like),
   - bridge + retention/latch behavior.

## Minimal worked patterns

### Bridge (seed)

- Topology: `BridgeNeck`
- Flow: `UnspecifiedTraversal`
- Parameters: `None`
- Settlement: `Pass`
- Focus: `Neck`

### Funnel (refinement)

- Topology: `BridgeNeck` with convergent branch structure on input side
- Flow: `RadialConvergent`
- Parameters: optional tunable arc/radius/steepness
- Settlement: `Pass` or `Retain` (context dependent)
- Focus: `Neck` + input `Boundary`

### Diffuser/Sprayer (refinement)

- Topology: `BridgeNeck` with divergent branch structure on output side
- Flow: `RadialDivergent`
- Parameters: tunable spread extent
- Settlement: typically `Dissipate`
- Focus: output `Boundary` / `Gradient`

### Fastener-like hold

- Topology: neck plus target anchor branch
- Flow: usually forward constrained during engagement
- Parameters: tunable engagement depth or threshold
- Settlement: `Retain` or `Latch`
- Focus: join/neck near target boundary
