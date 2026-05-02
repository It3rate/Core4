# Gojo Implementation Plan

Gojo is the current Core4 numeric kernel direction. `Core4.Kernel` and older
projects remain preserved for comparison, but new kernel work should happen in
Gojo.

For the current pseudo-code view of carrier input, local accounting, relation
application, and tension routing, see `Gojo Carrier Settlement Algorithm.md`.

The current model is:

- carriers are physical mutable sites with unique IDs
- carriers store positive value and signed unresolved tension
- scalar sign is a read of signed locations on the same carrier; carrier
  value storage itself remains positive
- carrier duality creates an opposed-flow / higher-grade carrier face, not the
  ordinary negative side of the scalar carrier
- relations bind carrier sites into points, segments, boundaries, routes, and
  transforms
- relations have physical identity; structural equality is still useful for
  tests and read comparison, but identity decides shared participation
- settlement happens inside the connected carrier/relation world
- behavior should come from numeric relation shape, not tags, helper objects,
  contribution tables, or external settle readers

Current step boundary:

- ordinary scalar math uses signed locations on one carrier, point/carry
  boundaries, ratio projection, backward ratio reads, and structural
  perturbation
- dual-carrier / inverse-flow behavior is only an isolated foothold for the
  later higher-grade or imaginary layer; it must not be treated as ordinary
  scalar negation

## Phase 1. Carrier Reads

In place:

- carrier identity
- automatic unique carrier IDs, with `Carrier.Unknown` fixed at `Id = 0`
- carrier locations
- shared `TickRef` coordinates for explicit same-unit references across
  carrier faces
- same-carrier positive reads
- same-carrier negative reads through signed `CarrierLocation` ticks
- orthogonal reads as zero
- inverse carrier reads are invisible to the scalar carrier unless an explicit
  relation/crystallized read connects them
- direct carrier-held `Value` and signed `Tension`
- `IElement.Dual`, implemented by carriers, carrier locations, and relations
- structural `Reverse` for relation traversal order
- relation identity separate from shape equality
- anonymous equal locations are value-equal but independent; locations built
  from the same `TickRef` share physical coordinate state
- shape-driven structural perturbation:
  - signed carrier location perturbations translate matching carrier sites
  - a translated shared `TickRef` moves each grounded carrier face that uses it
  - relation-shaped ratio perturbations scale participating sites exactly
  - inexact scale is left unresolved rather than rounded

## Phase 2. Internal Structural Settlement

In place:

- `carrier.Attach(relation)` attaches relation structure to the connected
  carrier world
- `Add` and `Remove` feed signed tension and settle that world automatically
- input enters as signed tension and the carrier absorbs what it can into
  positive value before relations route the residue
- a forward local span such as `L(C,0) -> L(C,10)` gives projection/basis
  structure without hardening the carrier
- a reverse local span such as `L(C,10) -> L(C,0)` gives return/wrap
  accounting
- same-carrier point relations perform reset/clamp accounting
- cross-carrier point receivers route source positive tension

Current stable examples:

- unbounded carrier input keeps its value
- bare carrier addition/subtraction adjusts accounted value, and underflow
  remains signed tension
- `L(C,0) -> L(C,10)` leaves value `11` as value `11` with no tension
- `L(C,10) -> L(C,0)` turns value `13` into value `3` plus tension `10`
- `P(C,10) -> P(C,0)` turns `23` into value `3` plus tension `20`
- `P(C,10) -> P(C,10)` clamps `23` to value `10` plus tension `13`
- `P(C0,10) -> P(C1,0)` turns `37` into `C0 = 7`, `C1 = 3`
- chained point/carry relations express decimal and binary-style positional
  counters by changing only the boundary unit
- `P(C0,2) -> P(C1,3)` is a ratio projection surface, not a carry route; the
  projected read is executable through `Relation.Project(...)`
- exact and inexact ratio projection preserve quotient plus unreduced residue;
  signed locations express scalar subtraction-like reads without inverse
  carriers
- offset and reversed segment frames project from their first endpoint rather
  than sorting endpoints
- `(P(C0,10) -> P(C0,0)) -> P(C1,0)` can absorb positive tension after the
  boundary has already produced it

Deferred dual-carrier foothold:

- inverse-oriented point receivers can route source negative tension
- related opposed carriers can cancel visible values before leaving residue
- these behaviors are tested separately from ordinary scalar math because
  full inverse-flow / imaginary operation semantics are not complete yet

Removed:

- `ContributionSurface`
- `ContributionReads`
- `StructuralTransformReads`
- tests that treated settlement as an external read over a structure surface

## Phase 3. Consolidation

Next cleanup targets:

- review `RelationReads` and keep only projections that remain useful as local
  reads over already-existing relation structure
- keep support/span reads as projections until they can participate in
  internal settlement
- avoid adding replacement helper APIs for old removed route concepts

## Phase 4. Missing Structural Mechanics

Add only when the numeric shape is clear:

- extent/stretch projection inside the carrier/relation world
- refill and borrow
- branch and merge negotiation
- conserved split versus observation fanout
- inverse-flow / imaginary operation behavior beyond the first dual-carrier
  foothold
- opposed loop and cycle behavior
- route hardening
- carrierless floating relation expressions
- support/filter routing through settlement rather than separate boolean logic

For branch/fanout, keep the material/observation distinction structural:
material transfer drains or distributes participation, while observation can
copy a read without consuming the observed carrier. Avoid flags if relation
shape can carry the distinction.

## Migration Rule

Only copy or preserve old Kernel code when it still follows the Gojo model.

Prefer deleting a helper over keeping a second meaning system alive.

If a concept is difficult to express, extend the numeric carrier/relation
format rather than falling back to enums, opaque tags, or ad hoc metadata.
