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
| resolved ratio | ordered relation against a denominator | places or scales structure | positions or phases traveler |
| tension ratio `m/0` | unresolved attachment potential | requests accommodation | perturbs or diverts traversal |
| unit / frozen basis | reusable denominator or support | stabilizes interpretation | gives local resolution and admissible interpolation |
| inverted unit | same magnitude under opposite perspective | reverses aligned reading | reverses local traversal perspective |
| directed segment | bounded opposed support | defines span and boundary | admits continuation, overflow, reflection, or closure response |
| site | pinned relation place | couples supports | decision / transfer point |
| boundary | admissibility limit or closure seam | constrains structure | causes wrap, reflect, stall, absorb, or emit |
| provenance | self / other weighting | biases publication and reception | biases candidate path choice |
| flow | live directional support influence | transmits pressure | offers outgoing, incoming, or oscillating legs |
| zero unit | unknown orthogonal explanatory need | spawns or seeks attachment | pushes traversal off default continuation |
| grade | compositional depth | expresses fused richness | changes what kinds of re-entry count as equivalent |
| repeated carrier relation | accumulated / self-interacting support | creates power-like or repeated action | creates repeated transfer under one law |

This table is not final.

Its purpose is to test whether these meanings are numerically natural and
perspective-consistent.

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

## 5. Repetition Family To Separate Carefully

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

## 6. Likely Core Mechanism

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

## 7. Immediate Next Pass

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

## 8. Design Constraint

When possible:

- do not add traversal enums as the source of truth
- do not add a large command list of path behaviors
- prefer numeric response encoded in structure
- allow explicit overrides later as attached equations or local constraints

The desired end state is:

- a unified system of reactions to values, numbers, and structure
- contextual behavior that is mostly implicit in the structure
- only a few explicit choices where the default numeric reading needs bending
