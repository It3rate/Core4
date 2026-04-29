# Gojo Route Generalization Notes

This note used to describe transitional helper APIs such as
`TransferThrough`, `ObserveThrough`, `GatherThrough`, and the later external
structural-settle reader.

Those APIs have now been removed from Gojo. Route-like behavior should live in
carrier/relation structure and settle inside the connected carrier world.

## Current Executable Route Shapes

The current executable shapes are intentionally small:

- `L(source,0) -> L(source,k)`
  - local capacity
  - hardens value to the capacity
  - leaves excess as carrier overflow

- `P(source,k) -> P(source,0)`
  - same-carrier reset boundary
  - hardens local value to residue
  - exposes unabsorbed crossing energy as carrier overflow

- `P(source,k) -> P(receiver,0)`
  - compact counter route
  - creates source overflow through the boundary unit
  - routes quotient-like overflow into the receiver carrier
  - leaves local residue on the source carrier

- `(P(source,k) -> P(source,0)) -> P(receiver,0)`
  - explicit counter route
  - can absorb carrier overflow produced before the route existed

- `P(source,k) -> P(receiver_inverse,0)`
  - underflow route
  - routes source underflow into an inverse-oriented receiver

- `P(source,1) -> P(receiver_inverse,0)`
  - opposed visible-value balancing
  - cancels related positive values before leaving residue

These are relation shapes, not tags. A carrier settles them because they are
attached to its world.

## Removed Surface Machinery

The following helper layers were deliberately removed:

- route sequence helpers
- residual route helpers
- direct carrier-location transfer helpers
- direct carrier-location observation helpers
- direct carrier-location gather helpers
- contribution surfaces
- external structural-settle readers
- boundary projection helpers that returned quotient/residue outside the
  carrier world

The removed tests mostly tested helper APIs against themselves. New tests
should attach relation structure to carriers, perturb carrier values, and then
inspect carrier value/overflow/underflow.

## Remaining Gap

Extent/stretch projection, refill/borrow, loop hardening, same-carrier cycles,
branch/merge, and dynamic balancing are not solved by the removed helpers. They
should come back only as structural relation shapes that carriers can settle
internally.

The pressure to watch for is API shape: if a behavior needs another named
method before it can be discovered, try to encode the behavior in carrier
locations, points, segments, and relation surfaces first.
