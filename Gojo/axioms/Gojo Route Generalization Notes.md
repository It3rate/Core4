# Gojo Route Generalization Notes

This note records a transitional route consolidation target. The current route
reads are useful and executable, but the route law is still partly carried by
method names such as `TransferThrough`, `ObserveThrough`, and `GatherThrough`.

The newer direction in `Gojo Structural Transform Relations.md` is stronger:
relation shape should carry more of the transform/route affordance internally.
The helper-factor direction below is therefore transitional scaffolding. It
should not be protected as a public API if it gets in the way of structural
relation reads.

## 1. Current Executable Pieces

The current route system is already relation-shaped:

- `AccountedBy(reader, accounted...)` splits a source into:
  - left: participation accounted in this reader frame
  - right: newly visible residual tension
- `TransferThrough(reader, route)` reads the route's left side
  as source-side capacity and emits the accounted source participation onto the
  route's right side.
- `ObserveThrough(reader, route)` reads a bounded amount through
  the route's left side, emits that amount to the right side, and leaves the
  source undrained.
- `GatherThrough(reader, route)` leaves the source undrained and
  gathers the visible source tension into the route's left side while the right
  side remains open.
- Nested receiver surfaces update every compatible carrier location by the same
  emitted participation. This gives coupled fanout without a branch flag.
- Ordered route sequences distribute conserved source tension by feeding only
  the residual source tension forward.
- Residual route reads feed visible right-side residuals back into the same
  transfer, observation, and gathering mechanics.

These are all variations of the same physical pass:

1. read incoming participation in a frame
2. split or preserve the source
3. choose an emitted carrier participation
4. apply that participation to a relation surface
5. leave unaccounted participation visible as residual tension

## 2. The Shared Shape

A route read should be understood as:

```text
incoming source + reader + route surface -> source split -> updated route surface
```

The output shape should remain:

```text
(accounted source, residual source) -> updated receiver or route surface
```

That shape is already what the current methods return. The consolidation target
is not a new route object. It is a shared read path that makes the source split,
emitted participation, and updated surface explicit.

## 3. Drain And Observation Without Flags

The system should not add a `drain` enum or boolean.

The apparent drain law comes from which source split is used and which part is
emitted:

- transfer:
  - source split uses the route's source-side capacity
  - emitted participation is the accounted side
  - residual source remains as tension
- observation:
  - source split uses no draining capacity, so source remains residual
  - emitted participation is separately bounded by the route's source-side
    capacity
  - receiver records the reading without consuming source participation
- gathering:
  - source split uses no draining capacity
  - emitted participation is the visible residual source
  - the route's open side accumulates unresolved potential

These are not different ontologies. They are different relation contexts over
the same source-split and emit mechanics.

## 4. Open, Resolved, And Hardened Routes

The current open route idiom is a relation whose right side is an open or
unknown zero surface. Gathering turns:

```text
0/0 -> m/0
```

in carrier-location form:

```text
(source 0, unknown 0) -> (source m, unknown 0)
```

That gathered left side can later harden into transfer capacity by pairing it
with a resolved receiving side:

```text
(source m, unknown 0) -> (source m, target n)
```

No wrapper is needed for this; the relation context supplies the role.

## 5. Branch, Merge, Loop, And Counter As Route Shapes

The existing cases should generalize as follows:

- conserved branch: an ordered relation sequence where each route receives only
  the residual source tension left by earlier routes
- observation fanout: several routes read the same undrained source
- coupled fanout: one route has a nested receiver surface, so one accounted
  source updates several receiver carrier locations
- conserved merge: several sources read through one route surface, accumulating
  only accounted participation into the receiver
- loop: a receiver carrier location from one route becomes the source for a
  later route
- ping-pong: the same loop shape crosses into an opposed carrier and back
- counter: a receiver carrier records crossing participation in its own
  positive direction

These should remain route compositions, not separate tags.

## 6. Transitional Code Direction

Current low-risk transitional status:

- boundary, accounting, transfer, observation, and gathering reads have been
  moved out of `Gojo.Core.CarrierLocation` into
  `Gojo.Reads.CarrierLocationReads`
- `TransferThrough`, `ObserveThrough`, and `GatherThrough` now share one
  private route-read helper:

```text
source split + emitted carrier location + receiver surface -> relation result
```

This helper does not introduce a public route type. It simply makes the common
route mechanics obvious and prevents the three public reads from drifting apart
while structural relation reads are being built.

After that, move outward:

1. route sequence and residual route reads currently call the shared helper
   through the existing public extension reads
2. make route collections relation surfaces rather than arrays once the shape
   is clear
3. add a balancing pass that repeatedly reads visible residuals and routes what
   local structure can account for, without adding a global settled status

## 7. Caution

The main incompatibility with the target model is not storage; the current
storage is compatible. The pressure is API shape.

If every route behavior becomes a new public method, the kernel will drift
toward symbolic branching. If the next pass extracts the shared source-split
and emit/update path, then transfer, observation, gathering, branch, merge,
loop, counter, and filter can stay different relation configurations over one
mechanism.
