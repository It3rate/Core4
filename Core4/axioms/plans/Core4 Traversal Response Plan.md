# Core4 Traversal Response Plan

This note captures the next consolidation pass after the current topology and
trolley experiments.

It is intentionally a planning note, not a final axiom.

## 1. Goal

Unify traversal, reaction, repetition, and path choice under the same Core4
numeric structure already used for:

- measurement
- projection
- attachment
- operation
- settlement

The main question is:

- how much of current traversal behavior is already inherent in Core4
  structure
- how much is still temporary scaffolding in the current `Inspection`
  namespace

## 2. Current Suspicion

Traversal is probably not a separate ontology.

The current trolley and propagation code are useful target-behavior sketches,
but they likely sit one level above the more primitive law we actually want.

That more primitive law appears to involve three linked pieces:

- **advance**: interpolate on support
- **transfer**: let structure or boundary alter carried state
- **re-entry**: compare the new state against prior state and determine
  whether the result is:
  - new continuation
  - reflection
  - oscillation
  - wrap or self-feed
  - merge
  - exact periodic return

## 3. Working Structural Table

The next pass should explicitly map Core4 structures to:

- what they mean as actions on structure
- what they mean as actions on a traveler or trolley

This is partly to expose wrong assumptions if the table fails to cohere.

| Core4 structure | Structural reading | Action on structure | Action on traveler |
| --- | --- | --- | --- |
| primitive measure | raw amount without calibration | contributes magnitude | contributes sensed amount |
| resolved ratio | ordered state against a denominator | places, scales, compares, or filters | positions, phases, gates, or weights traversal |
| tension ratio `m/0` | unresolved attachment or explanatory mismatch | requests accommodation or new support | perturbs, diverts, or stalls traversal |
| carrier | primitive support axis | defines one family of ordered change | defines local interpolation direction and resolution |
| relation | live ordered coupling on a support | carries current span, orientation, and negotiability | offers a traversable leg with direction and potential |
| frozen basis / unit | committed reusable support | stabilizes denominator and reusable law | gives local resolution, admissible interpolation, and reusable response |
| inverted unit or opposed basis | same support under opposite perspective | reverses aligned reading or span interpretation | reverses local traversal perspective or re-entry reading |
| placement | where something is on a support | locates present structure | locates the traveler and gives the next interpolation frame |
| site | pinned relation place | couples, transfers, branches, and joins | acts as a transfer, choice, or reaction point |
| attachment | live reading of sites through frozen support | binds present state into reusable structure | lets the traveler inspect present structure through a basis |
| provenance | self / other weighting | biases publication and reception | biases candidate path choice and handedness of response |
| flow | live directional support influence | transmits pressure or explanation | offers incoming, outgoing, or oscillating continuation |
| zero unit | unresolved orthogonal explanatory need | spawns or seeks attachment and new support | pushes traversal off default continuation into new explanatory dimension |
| directed segment | bounded opposed support | defines span and admissible boundary relation | admits continuation, overflow, reflection, or closure response |
| boundary / closure seam | admissibility limit or identified return edge | constrains, wraps, reflects, absorbs, or emits | causes wrap, reflect, stall, absorb, or emit behavior |
| grade | compositional depth and fused richness | changes how much structure is internalized | changes what kinds of re-entry or equivalence are meaningful |
| repeated carrier relation / self-interaction | accumulated or self-referential support law | creates repeated action, power-like response, or non-linearity | creates repeated transfer under one law or repeated local reaction |
| orthogonal independence | a second explanatory axis rather than same-carrier continuation | enables area, ray, branch, and cross-support response | enables turning, branching, and non-collinear inspection paths |

This table is not final.

Its purpose is to test whether these meanings are numerically natural and
perspective-consistent.

The more semantic categories behind this table are probably:

- magnitude or state
- support
- placement
- perspective
- commitment
- mismatch
- composition
- boundary and re-entry

## 4. Dual Perspective Hypothesis

One strong hypothesis to test is:

- action on a support
- inspection from within a support

behave as local duals.

For example:

- if a carrier acts on a traveler by a positive scale, the traveler may read
  the carrier as compressing toward it
- if a carrier reverses perspective, the traveler should read that as a local
  inversion or reflection

This is similar to the way:

- claim and measurement
- generation and inspection

often look like dual readings of the same relation under changed perspective.

## 5. Same Value, Different Stance

One thing the next pass should test explicitly is whether the same Core4 value
can carry different but coherent meanings under different stances without
changing ontology.

For example, `3` should not need a different symbolic species in order to
mean:

- a measured amount
- a position on a unit
- a coefficient acting on another support
- a local traversal weighting
- a repeated local action recipe

Which reading appears should depend on:

- what support the value is attached to
- whether it is being claimed, measured, inspected, or generated
- whether it is being read from the structure side or the traveler side

For example:

- a positive `3` on a unit can be read as location or amount
- a positive `3` acting on a second support can stretch, repeat, or shift by
  three
- a negative value should negate or reflect under the same law rather than
  requiring a special traversal directive
- an inverted unit should naturally reverse perspective

So the aim is not just to catalog objects.

It is to see whether the same numeric structure already implies coherent
behavior across several modalities.

## 6. Repetition Family To Separate Carefully

The next pass should keep these distinct:

- **sequence**: unrolled ordered events with no meaningful re-entry
- **repeated action**: repeated transfer under the same law
- **reflection**: perspective inversion at a boundary or site
- **oscillation**: repeated reflection or bidirectional exchange
- **graph loop**: return through connected support
- **self-feed**: direct re-entry without traveling a graph loop
- **orbit**: repeated family in projection without full closure
- **periodicity**: exact repeated re-entry under a stable law

These should not be collapsed too early, but they also should not require a
separate symbolic mechanism for each.

## 7. Likely Core Mechanism

The likely next primitive is not another trolley class.

It is more likely a reusable structural signature for carried state, such as:

- current support
- current placement
- current perspective
- carried value or tension
- provenance weighting
- frozen local constraints
- possibly retained phase or memory

Then traversal can compare signatures to determine:

- whether a state is novel
- whether it has reflected
- whether it has merged
- whether it has wrapped
- whether it has truly re-entered

## 8. Immediate Next Pass

Before rewriting core code, inspect current mechanisms and map where they
already align:

- `Placement` / `ISupport` interpolation
- `Ratio` sign and tension
- `Relation` orientation and projection
- `Flow` / `Provenance` directional weighting
- `Site.Settle()` feedback and remainder
- topology incidence and continuation
- current trolley forward / reverse / merge behavior

Then identify which parts of current traversal are:

- already Core4-native
- thin but acceptable scaffolding
- hard-coded policy that should be re-expressed numerically

## 9. Design Constraint

When possible:

- do not add traversal enums as the source of truth
- do not add a large command list of path behaviors
- prefer numeric response encoded in structure
- allow explicit overrides later as attached equations or local constraints

The desired end state is:

- a unified system of reactions to values, numbers, and structure
- contextual behavior that is mostly implicit in the structure
- only a few explicit choices where the default numeric reading needs bending
