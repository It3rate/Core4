# Gojo Route Generalization Notes

This note used to describe the transitional helper API named
`TransferThrough`, `ObserveThrough`, and `GatherThrough`.

That API has now been removed from Gojo. The useful behavior is carried by
relation structure and settled by `Gojo.Reads.StructuralTransformReads`.

## Current Structural Route Shapes

The current executable route-like shapes are:

- `P(source,k) -> P(source,0)`
  - same-carrier reset boundary
  - hardens local position to residue
  - exposes unabsorbed crossing energy as carrier tension

- `(P(source,k) -> P(source,0)) -> P(receiver,0)`
  - explicit counter route
  - absorbs carrier tension through the boundary unit
  - leaves local residue on the source carrier

- `L(source,k) -> receiver surface`
  - conserved transfer
  - drains up to `k` from `source`
  - writes the drained amount into every carrier location in the receiver
    surface
  - ordered relation surfaces distribute only remaining source participation
    forward
  - fires at most once in one settle pass, then may fire again in a later pass
    when new participation appears

- `(L(source,a) -> L(source,b)) -> receiver surface`
  - observation
  - reads up to `|b-a|` from `source`
  - writes the observed amount into the receiver surface
  - does not drain the source

- `L(source,m) -> L(unknown,0)`
  - open gather
  - raises `m` to cover newly visible carrier tension
  - does not drain the source
  - does not gather the same persisted carrier tension twice
  - leaves a numeric attachment potential that may later harden into a
    resolved transfer surface

These are relation shapes, not tags. The code should inspect structure and
settle the relation surface, not require callers to choose a route method.

## What Was Removed

The following helper layers were deliberately removed:

- route sequence helpers
- residual route helpers
- direct carrier-location transfer helpers
- direct carrier-location observation helpers
- direct carrier-location gather helpers
- boundary projection helpers that returned quotient/residue outside a
  relation surface

The removed tests were mostly testing the helper API against itself. New tests
should prefer structural settlement checked against ordinary arithmetic where
that arithmetic is the intended projection, such as division/remainder,
base-carry, conserved distribution, and multiplication by layer lengths.

## Remaining Gap

Underflow/refill, loop hardening, same-carrier cycles, and dynamic balancing are
not solved by the removed helpers. They should come back only as structural
relation shapes.

The pressure to watch for is API shape: if a behavior needs another named
method before it can be discovered, try to encode the behavior in carrier
locations, points, segments, and relation surfaces first.
