# Gojo Implementation Plan

Gojo is the current Core4 numeric kernel direction. `Core4.Kernel` and older
projects remain preserved for comparison, but new kernel work should happen in
Gojo.

The current model is:

- carriers store positive participation
- sign is a read across related or opposed carriers, not signed storage
- carrier state is held by carrier instances as value and tension
- relations bind carrier sites into points, segments, boundaries, routes, and
  transforms
- settlement lowers visible tension by repeatedly applying relation structure
- behavior should come from numeric relation shape, not tags or helper objects

## Phase 1. Carrier Reads

In place:

- carrier identity
- carrier locations
- same-carrier positive reads
- orthogonal reads as zero
- opposed carrier reads as negative projection without negative storage
- direct carrier-held `Value` and `Tension`

## Phase 2. Structural Settlement

In place as the first small pass in `Gojo.Reads.StructuralTransformReads`:

- `structure = carrier-state surface -> relation surface`
- incoming participation enters the left surface as fresh frame input
- the right surface is traversed for relation nodes
- same-carrier relations perform local accounting
- cross-carrier point receivers route source tension
- cross-carrier extent receivers project fresh participation by extent ratio
- settlement repeats until carrier value/tension state is quiet

Current stable examples:

- unbounded carrier input keeps its value
- `P(C,10) -> P(C,0)` turns `23` into value `3` plus tension `20`
- `P(C,10) -> P(C,10)` clamps `23` to value `10` plus tension `13`
- `P(C0,10) -> P(C1,0)` turns `37` into `C0 = 7`, `C1 = 3`
- `(P(C0,10) -> P(C0,0)) -> P(C1,0)` can absorb tension now or in a later
  settle frame
- `S(C0,0,1) -> S(C1,0,5)` maps fresh `4` on `C0` to fresh `20` on `C1`
- chained extent projections can feed later relations in the same frame
- settled projected values do not re-stretch in the next frame without fresh
  input

The old externally driven structural transform reader has been removed. Tests
now assert settled carrier states rather than calling named transform routes.

## Phase 3. Consolidation

Next cleanup targets:

- rename or reduce `ContributionReads` and `ContributionSurface`; they are now
  transitional carrier-state projections, not provenance storage
- keep carrier-line matching small and local; full carrier equality already
  includes held state
- keep `RelationReads` traversal/support utilities as projections until they
  are absorbed into structural settlement
- delete tests that only protect removed helper APIs

The settlement pass may still use an ephemeral map internally to compute one
frame. That is acceptable as accounting machinery. It should not become a
persisted parallel explanation structure.

## Phase 4. Missing Structural Mechanics

Add only when the numeric shape is clear:

- underflow and refill
- branch and merge negotiation
- conserved split versus observation fanout
- opposed loop and cycle behavior
- route hardening
- carrierless floating relation expressions
- support/filter routing through settlement rather than separate boolean
  logic

For branch/fanout, keep the material/observation distinction structural:
material transfer drains or distributes participation, while observation can
copy a read without consuming the observed carrier. Avoid flags if relation
shape can carry the distinction.

## Phase 5. Support And Boolean-Like Reads

The existing support/span reads are useful projections:

- boundary point discovery
- traversed span splitting
- support, tension, exclusive reads
- scoped pass/block/not projections

These should remain lower projections over carrier relation structure. Do not
promote them into a separate boolean algebra.

When support becomes active settlement, express it as ordinary carrier
relation traversal: split where participation changes, account what can be
accounted, and preserve the rest as carrier tension.

## Migration Rule

Only copy or preserve old Kernel code when it still follows the Gojo model.

Prefer deleting a helper over keeping a second meaning system alive.

If a concept is difficult to express, extend the numeric carrier/relation
format rather than falling back to enums, opaque tags, or ad hoc metadata.
