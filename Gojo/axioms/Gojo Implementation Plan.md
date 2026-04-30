# Gojo Implementation Plan

Gojo is the current Core4 numeric kernel direction. `Core4.Kernel` and older
projects remain preserved for comparison, but new kernel work should happen in
Gojo.

The current model is:

- carriers are physical mutable sites with unique IDs
- carriers store positive value and signed unresolved tension
- sign is a read across related or opposed carriers; carrier value itself
  remains positive
- relations bind carrier sites into points, segments, boundaries, routes, and
  transforms
- settlement happens inside the connected carrier/relation world
- behavior should come from numeric relation shape, not tags, helper objects,
  contribution tables, or external settle readers

## Phase 1. Carrier Reads

In place:

- carrier identity
- automatic unique carrier IDs, with `Carrier.Unknown` fixed at `Id = 0`
- carrier locations
- same-carrier positive reads
- orthogonal reads as zero
- opposed carrier reads as negative projection without negative storage
- direct carrier-held `Value` and signed `Tension`
- `IElement.Dual`, implemented by carriers, carrier locations, and relations
- structural `Reverse` for relation traversal order

## Phase 2. Internal Structural Settlement

In place:

- `carrier.Attach(relation)` attaches relation structure to the connected
  carrier world
- `SetValue`, `Add`, and `Remove` settle that world automatically
- input enters as signed tension and the carrier absorbs what it can into
  positive value before relations route the residue
- a forward local span such as `L(C,0) -> L(C,10)` gives local capacity
- a reverse local span such as `L(C,10) -> L(C,0)` gives return/wrap
  accounting
- same-carrier point relations perform reset/clamp accounting
- cross-carrier point receivers route source positive tension
- inverse-oriented point receivers can route source negative tension
- related opposed carriers cancel visible values before leaving residue

Current stable examples:

- unbounded carrier input keeps its value
- `L(C,0) -> L(C,10)` turns value `11` into value `10` plus tension `1`
- `L(C,10) -> L(C,0)` turns value `13` into value `3` plus tension `10`
- `P(C,10) -> P(C,0)` turns `23` into value `3` plus tension `20`
- `P(C,10) -> P(C,10)` clamps `23` to value `10` plus tension `13`
- `P(C0,10) -> P(C1,0)` turns `37` into `C0 = 7`, `C1 = 3`
- `(P(C0,10) -> P(C0,0)) -> P(C1,0)` can absorb positive tension after the
  boundary has already produced it
- `P(C0,10) -> P(C1_inverse,0)` can absorb source negative tension through the same
  relation-shaped unit

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
