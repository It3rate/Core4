# Core4 Basis Encounter Plan

This note turns the conclusions in
[Core4 Basis Encounter Note](./Core4%20Basis%20Encounter%20Note.md)
into an implementation plan.

The goal is to make basis encounter a primitive Core4 behavior rather than a
UI-era interpretation layered on top of topology.

This is the path toward:

- reflection
- clamp and stop behavior
- closure and loop entry
- local reinterpretation of motion
- later frame-like local interpretation

without introducing a second symbolic ontology for seams or branch rules.

## 1. Goal

Make the following statement true in core code:

- a trolley carries a basis of understanding and its own traversal state
- a pinned basis encountered at a site can act as a local calibration law
- continuation falls out of the negotiation between:
  - carried basis
  - traversal state
  - encountered basis
  - local topology commitment

The important design target is:

- keep this Core4-native
- keep it local
- keep it numeric
- avoid enums as the source of truth

## 2. Canonical Interpretation

The implementation should assume the following as canonical.

### 2A. Basis

A basis is a frozen calibration law.

It tells a traveler:

- what counts as positive
- what counts as negative
- what resolution is currently intelligible
- what denominator or span is reusable
- what kinds of change are available under this reading

### 2B. Trolley

A trolley carries two different things:

- a basis reference
- a traversal state

The basis reference is the calibration the trolley is currently using.

The traversal state is the mutable part:

- increment or decrement intent
- local modifier or handedness
- step size
- carried amount or tension
- provenance

The trolley should begin from an identity-like local modifier and let
encountered structure act on that modifier.

### 2C. Encounter

When the trolley reaches a site with a pinned basis-like law, the result
should be expressible as:

- retain the carried basis
- replace the carried basis
- compose the traversal state against the encountered basis
- fail to reconcile and produce tension

The important constraint is:

- do not model this as mutating the carried basis itself by default
- composition should usually happen in traversal state

### 2D. Seam

Seam should not be treated as a new first ontology.

Instead:

- seam is a role of basis encounter
- specifically, a pinned basis encounter where continuation is nontrivial

So the code should first try to express seam behavior using:

- basis
- site
- attachment
- continuation agreement

before inventing a separate seam type.

## 3. Non-Goals

This pass should not yet try to finish:

- full generalized periodicity
- full loop solver or oscillator engine
- arbitrary high-grade traversal intelligence
- large symbolic boundary rule libraries
- UI-first behavior that has no core expression

Those are downstream of the primitive encounter law.

## 4. Immediate Primitive Gap

The current code already has useful ingredients:

- [Basis.cs](/C:/Users/Robin/source/repos/Core4/Core4/Structure/Basis.cs)
- [Relation.cs](/C:/Users/Robin/source/repos/Core4/Core4/Structure/Relation.cs)
- [Site.cs](/C:/Users/Robin/source/repos/Core4/Core4/Structure/Site.cs)
- [TraversalEncoding.cs](/C:/Users/Robin/source/repos/Core4/Core4/Dynamics/TraversalEncoding.cs)
- [ConnectedSiteTopology.cs](/C:/Users/Robin/source/repos/Core4/Core4/Topology/ConnectedSiteTopology.cs)

But it is still missing an explicit primitive for:

- basis encounter at a site
- continuation agreement resulting from that encounter

Today traversal mostly compares support continuations.

The next pass should compare:

- support continuation
- basis compatibility
- local calibration agreement

at the same time.

## 5. Planned Core Additions

The smallest plausible additions are:

### 5A. Explicit Traversal State

The current traversal encoding carries:

- placement
- perspective
- carried ratio
- provenance

It should likely be sharpened so the mutable local part is clearer.

Target shape:

- carried basis or calibration reference
- traversal modifier
- carried amount or tension
- provenance
- placement

This does not necessarily require a large new type explosion.

But the distinction between:

- carried basis
- current modifier applied through that basis

should become explicit in core.

### 5B. Basis Encounter Read

Add a reusable way to ask:

- what basis-like laws are encounterable at this site?

This should include at least:

- directly pinned basis at the site
- local carrier-aligned basis
- attached basis that can be read through the site
- later boundary-local basis

This encounter read should remain local and numeric.

### 5C. Continuation Agreement

Add a numeric result of comparing:

- carried basis
- traversal modifier
- encountered basis
- local support continuity

The agreement should be rich enough to express:

- preserve
- invert
- clamp
- close
- partial transfer

But these should be represented as ordinary numeric agreement and commitment,
not as the source-of-truth enum.

### 5D. Capability Read

Add a way for basis encounter to report not only direction agreement, but also
capability agreement.

Examples:

- dominant-only unit: one-sided motion is intelligible
- directed segment basis: bilateral bounded change is intelligible
- higher-grade basis: off-axis or compound motion may become intelligible

This should let basis encounter reduce capability as well as expand it.

## 6. First Cases To Support

The first pass should make the following cases explicit and testable.

### 6A. Aligned Identity Continuation

- same carrier
- compatible resolution
- compatible orientation
- no special reinterpretation

Expected result:

- trolley continues
- carried basis may remain unchanged
- traversal modifier remains identity-like

### 6B. Opposed Local Unit

- same carrier
- opposed local orientation
- no richer higher-grade reinterpretation

Expected result:

- trolley can either replace carried basis or invert traversal modifier
- functional result is reflective reversal

### 6C. Dominant-Only Encounter

- same carrier
- one-sided local calibration

Expected result:

- forward continuation may remain intelligible
- reverse continuation may become unresolved
- reverse attempt may produce tension instead of symmetric motion

### 6D. Directed-Segment Encounter

- same carrier
- bilateral basis available

Expected result:

- forward and backward bounded interpretation remain available
- trolley does not lose bilateral capability by passing through

### 6E. Resolution Mismatch On Same Carrier

- same carrier
- denominator mismatch

Expected result:

- do not classify as orthogonal
- read as calibration tension or attachment mismatch
- later allow negotiated hardening into shared denominator

### 6F. Grade Mismatch

- same carrier, but encountered basis is richer

Expected result:

- maybe project down and continue
- maybe expose new orthogonal options
- maybe remain too rich and create tension

This should be handled numerically rather than by a case list.

## 7. Boundary And Loop Cases To Build After The Primitive Pass

Once basis encounter exists, build these in order.

### 7A. Clamp

Interpretation:

- basis encounter offers no admissible onward calibration

Expected result:

- attempted motion stalls
- residual becomes tension

### 7B. Reflective Interval

Interpretation:

- endpoint-local opposed basis encounter

Expected result:

- positive travel reaches an end
- local encounter inverts the usable reading
- motion continues in the opposite geometric direction

This is the football-field-style case.

### 7C. Closure With Preserved Perspective

Interpretation:

- endpoint-local continuation remaps to another endpoint
- handedness preserved

Expected result:

- true loop-like continuation without forced reflection

### 7D. Closure With Inverted Perspective

Interpretation:

- endpoint-local continuation remaps to another endpoint
- handedness inverted

Expected result:

- reflective closure or shuttle-like behavior

## 8. Where This Should Land In Code

### 8A. Structure Layer

Primary files:

- [Basis.cs](/C:/Users/Robin/source/repos/Core4/Core4/Structure/Basis.cs)
- [Relation.cs](/C:/Users/Robin/source/repos/Core4/Core4/Structure/Relation.cs)
- [Site.cs](/C:/Users/Robin/source/repos/Core4/Core4/Structure/Site.cs)
- [Attachment.cs](/C:/Users/Robin/source/repos/Core4/Core4/Structure/Attachment.cs)

Likely changes:

- basis should expose more explicit calibration characteristics
- site should be able to surface encounterable basis-like laws
- attachment should remain the place where unresolved same-carrier mismatch can
  degrade into tension

### 8B. Dynamics Layer

Primary file:

- [TraversalEncoding.cs](/C:/Users/Robin/source/repos/Core4/Core4/Dynamics/TraversalEncoding.cs)

Likely changes:

- make carried basis vs traversal modifier more explicit
- add basis-encounter agreement
- add capability agreement alongside direction agreement
- keep result numeric

### 8C. Topology Layer

Primary files:

- [SiteIncidence.cs](/C:/Users/Robin/source/repos/Core4/Core4/Topology/SiteIncidence.cs)
- [ConnectedSiteTopology.cs](/C:/Users/Robin/source/repos/Core4/Core4/Topology/ConnectedSiteTopology.cs)

Likely changes:

- topology should expose what basis-like laws are local to a site
- later boundary-local closure points should still appear as site-local
  encounter facts

### 8D. Inspection Layer

Primary files:

- [TopologyTrolley.cs](/C:/Users/Robin/source/repos/Core4/Core4/Inspection/TopologyTrolley.cs)
- [PropagationWave.cs](/C:/Users/Robin/source/repos/Core4/Core4/Inspection/PropagationWave.cs)

Expected role:

- increasingly thin consumer of basis encounter and continuation agreement
- not the place where reflection, clamp, and closure are invented

## 9. Tests To Add

Add core tests when each behavior stabilizes.

### 9A. Basis Encounter Core

- same-basis continuation preserves modifier
- opposed basis encounter can invert effective motion
- dominant-only basis removes bilateral capability
- same-carrier resolution mismatch produces tension rather than orthogonality

### 9B. Boundary Cases

- clamp accumulates tension
- reflective interval reverses motion under endpoint-local basis encounter
- closure preserves perspective when specified
- closure inverts perspective when specified

### 9C. Grade Cases

- richer encountered basis may expose extra options
- too-rich basis can degrade into unresolved tension instead of fake precision

## 10. Demo Expectations

Once the primitive core is in place, the trolley/circuit demos should stop
hardcoding boundary behavior.

Useful visual checkpoints:

- a reflective interval demo
- a clamp demo
- a simple preserved-perspective loop
- a mixed-capability demo where basis replacement visibly changes what the
  trolley can interpret

These demos should verify that:

- the UI is only rendering core decisions
- the same core encounter law works in more than one shape

## 11. Relationship To Frames And Stored Context

This pass also supports later work on:

- carrier-local interpreters
- frames
- data held inside a calibration context
- multi-basis readings on one carrier

If traversal can already distinguish:

- carried basis
- local traversal state
- encountered basis
- unresolved calibration mismatch

then the same pattern can later describe:

- a carrier holding a reference interpreter
- a present state being read through that interpreter
- another encountered interpreter altering or resisting that read

So this pass is not only about loops.

It is one of the primitive bridges from:

- carrier + basis

to:

- frame + interpretation + live state

## 12. Suggested Order Of Work

1. Refine traversal encoding so carried basis and traversal modifier are
   clearly distinct.
2. Add explicit basis-encounter reads at a site.
3. Add numeric continuation agreement from basis encounter.
4. Add tests for identity, inversion, one-sided basis, and resolution mismatch.
5. Re-express trolley continuation through that agreement.
6. Add clamp and reflective interval as the first nontrivial boundary cases.
7. Add closure with preserved perspective after reflection is stable.
8. Revisit full loop / periodic re-entry only after the above is solid.

## 13. Guiding Constraint

If a new loop or boundary behavior appears to require:

- a dedicated symbolic boundary enum
- a new seam ontology
- a large list of traversal directives

then first check whether the behavior can instead be expressed as:

- basis encounter
- traversal-state modification
- continuation agreement
- attachment/tension when reconciliation fails

That should remain the default Core4-native direction unless it clearly breaks.
