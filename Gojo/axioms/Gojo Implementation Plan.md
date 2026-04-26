# Gojo Implementation Plan

Gojo is the next kernel pass after `Core4.Kernel`.

`Core4.Kernel` remains preserved for comparison. Gojo starts from the newer
carrier-layer reading:

- carrier-local storage is positive-flow storage
- sign is a cross-carrier read
- operations are carrier layers
- each layer has one carrier value in its transform slot
- an event is the appearance of newly visible tension
- boundary overflow leaves attached-layer state or new tension unless routed

## Phase 1. Positive Carrier Reads

Implement the smallest executable model:

- positive carrier identity
- positive carrier location
- same-carrier reads
- orthogonal reads returning zero
- opposed carrier reads projecting as negative

This phase proves that location and read are separate.

## Phase 2. Transform Paths

Add one-transform-per-layer paths:

- bare `CarrierLocation` values in transform slots
- expansive reads
- contractive reads
- reverse reads with attached-layer state or tension
- zero transform as additive or non-mixing attachment

This phase should cover multiplication-like and division-like reads without
ordinary algebraic operator tags.

## Phase 3. Layered Reads And New Tension

Add carrier-layer boundaries:

- carrier tick overflow
- attached carrier-layer ticks
- four directional side/travel carrier-location reads
- newly visible tension when continuation is not already accounted for
- routed or self-absorbed overflow producing no repeated event

This phase should produce reads such as `23 4|10 C`.

Current executable foothold:

- `IElement` is the minimal recursive participation surface. `CarrierLocation`
  and `Relation` both participate as elements.
- `Relation` is now a minimal ordered pair of elements. It carries no route,
  event, support, or endpoint policy by itself; context gives its two slots
  their current read.
- `CarrierLocation.ReadThroughBoundary(earlierCarrier, boundary)` returns a
  `Relation` whose left slot is the earlier carrier-layer ticks and whose right
  slot is the later carrier-layer ticks that did not cross the boundary. This
  replaces the former dedicated layered-read wrapper.
- Boundary overflow routing composes from existing reads: read through a
  boundary, then transfer the left overflow carrier ticks through an ordinary
  route relation. A nested receiving side can update both a wrapped whole
  carrier and a counter, while any route saturation remains visible as overflow
  tension beside the residue.
- `CarrierLocation.ReadRefillThroughBoundary(earlierCarrier, boundary)` is the
  refill/underflow mirror of `ReadThroughBoundary`. Reverse partition flow
  reads as borrowed ticks on an earlier inverse carrier plus positive refill
  residue on the partition carrier. Those borrowed ticks route through the same
  transfer/observe/gather machinery as overflow.
- `CarrierLocation.NewTensionAfter(accounted, reader)` returns the carrier
  ticks that remain newly visible after the current read frame subtracts what
  local structure already accounts for.
- `CarrierLocation.NewTensionAfter(reader, accounted...)` accumulates multiple
  compatible accounting surfaces before reporting new tension, giving route
  capacity a numeric foothold without adding a route wrapper yet.
- `CarrierLocation.AccountedBy(reader, accounted...)` returns a `Relation`
  whose left slot is the participation accounted by the current read and whose
  right slot is the newly visible tension.
- A `Relation` can be used as accounting input by reading both slots as carrier
  locations in the current context. This lets route-like capacity, split reads,
  and prior relation results feed the same mechanism without a route class.
- `Relation.CarrierLocations()` now traverses nested relation surfaces.
  `LeftCarrierLocations()` and `RightCarrierLocations()` keep the ordered side
  distinction when context needs it.
- `CarrierLocation.TransferThrough(reader, route)` is the first resolved-route
  projection. The route's left side is read as source-side capacity; the
  route's right side is read as the receiving carrier surface. The returned
  relation is `source accounting split -> received surface`, so partial
  transfer leaves excess visible as tension. If the receiving side is nested,
  the transfer preserves that nested relation shape, giving branch-like
  receiver updates without introducing a graph object.
- `CarrierLocation.TransferThroughRoutes(reader, routes...)` reads an ordered
  route sequence. Each route receives only the source tension left by earlier
  routes, and traversal stops once no visible tension remains.
- Transfer into an opposed carrier already gives the first reflection shape.
  The opposed carrier stores positive ticks in its own direction, and those
  ticks can route back through a later relation when read from the original
  carrier frame.
- `CarrierLocation.ObserveThrough(reader, route)` is the first zero-unit
  observation projection. The route's left side sizes what can be observed, but
  the source is read with no drain, so the full visible incoming participation
  remains new tension. The right side receives the observed amount as a marker,
  counter, or claim.
- `CarrierLocation.GatherThrough(reader, route)` is the first open-route
  projection. The source is read with no drain, and the incoming visible
  tension gathers into the route's left side while the route's right side
  remains open. This turns `0/0` into `m/0`; the gathered `m` can later harden
  into transfer capacity by pairing it with a resolved receiving side.
- `Relation.BoundariesOn(reader)` reads the same ordered pair as carrier-local
  segment boundaries when context asks for endpoint behavior. If the reader
  cannot see both endpoints as aligned participation, the boundary read is zero.
  A zero-magnitude endpoint can still be a real boundary site; endpoint reads
  test carrier alignment, not nonzero value visibility.
- `Relation.SpanOn(reader)` reads the carrier-local span between those ordered
  boundaries. This is the first foothold for support-span splitting without a
  dedicated span type.
- `Relation.FlowOn(reader)` reads travel direction as carrier participation:
  forward travel appears on the reader carrier, reverse travel appears on the
  reader's inverse carrier.
- `Relation.BoundaryPointsOn(reader, relations...)` discovers the ordered
  carrier locations where endpoint participation can change. This mirrors the
  old support span split mechanism without creating a support-specific span
  type.
- `Relation.CoveredSpansOn(reader, relations...)` returns adjacent carrier-local
  `Relation` intervals between those boundary points, skipping gaps where no
  input relation covers the interval. Opposed surfaces stay on the opposed
  carrier rather than being flattened onto the reader carrier.
- `Relation.CoveredSpansWithinOn(reader, scope, relations...)` reads coverage only
  inside one explicit scope relation, clipping covered intervals to that scope.
- `Relation.SupportStatesOn(reader, relations...)` returns nested relations:
  the left slot is a carrier-local interval and the right slot is the
  reverse/forward participation pair for that interval.
- Multiple compatible relations covering the same interval merge by increasing
  the forward or reverse carrier participation count in that state pair.
- `Relation.SupportStatesWithinOn(reader, scope, relations...)` does the same
  inside one explicit scope, preserving clipped intervals and their local
  reverse/forward state.
- `Relation.SupportOn(reader)`, `Relation.TensionOn(reader)`, and
  `Relation.ExclusiveOn(reader)` project a reverse/forward pair without leaving
  the relation system. They can read either a bare reverse/forward pair or a
  nested support state.
- Static span projections `Relation.SupportOn`, `Relation.TensionOn`, and
  `Relation.ExclusiveOn` preserve the carrier intervals that carry those
  projected states and omit zero projected states.
- Scoped projections `SupportWithinOn`, `TensionWithinOn`, and
  `ExclusiveWithinOn` apply the same support/tension/exclusive reads inside a
  chosen interval.
- Boolean-style span reads are aliases over those projections: `OrOn` is
  support, `AndOn` is tension/shared opposed participation, and `XorOn` is
  exclusive unopposed participation.
- `Relation.UncoveredSpansOn(reader, scope, covered...)` reads scoped absence
  by splitting one visible scope relation around covered intervals. `NotOn` is
  only this scoped absence projection; there is no global negation outside a
  chosen carrier interval.
- `PassOn` and `BlockOn` are scoped filter projections over the same split:
  pass reads the covered intervals inside the scope, while block reads the
  uncovered intervals. They are not new truth values or route objects.
- A support filter can route by treating an incoming value as a scoped interval
  from origin to that value, reading pass/block spans inside it, and then using
  span widths as transfer capacity or open gathered tension. Selection is
  therefore span splitting plus ordinary route reads.
- With no compatible accounting surface, the full visible participation remains
  new tension. A zero-size accounting surface does not consume participation by
  itself; observation and claim behavior must come from relation context.
- If the current reader cannot see the incoming carrier, no tension is visible
  in that frame.
- If the accounting carrier is orthogonal or opposed in this frame, it does not
  silently absorb the incoming participation.
- Same-lineage duplicate carriers may account for one another when the reader
  sees them as aligned; coincident but independent carriers do not.
- Opposed carrier locations can be accounted within their own opposed flow,
  because they are still positive storage on that opposed carrier.
- If accounting covers the incoming ticks, no new event appears; if it
  saturates, only the excess remains as new tension.

## Phase 4. Routes

Add route structures:

- resolved transfer routes (started as `TransferThrough`)
- zero-unit observation routes (started as `ObserveThrough`)
- open tension routes (started as `GatherThrough`)
- route capacity and saturation
- counters
- wrap
- clamp
- ping-pong
- segment end-to-start closure

This phase should make boundary behavior compositional instead of flag-driven.

## Phase 5. Filters And Support

Rebuild support and boolean-like reads on top of routes:

- support span boundary splitting as new tension discovery
- pass
- block
- branch
- merge
- unpassed carrier-layer state
- support/tension/exclusive reads

This phase should replace boolean branching with carrier-native route support.

## Existing Code Pressure

The preserved bootstrap/kernel code already contains partial versions of this
mechanism:

- `SupportSpanUtilities.BuildStateSpans` splits support at span start/end
  positions. In Gojo terms, those split points are places where carrier
  participation changes and new tension may become visible.
- `SupportReadEquations.Tension` reads overlap between reverse and forward
  slots as tension. In Gojo terms, boolean-like cancellation is a projection of
  carrier tension, not a separate boolean operator.
- `SupportReadEquations.Emit` currently clamps incompatible slot emissions with
  a TODO for future boundary containers. In Gojo terms, those rejected emissions
  should become newly visible tension and then route, observe, or remain
  unresolved.

Do not port these as special support-only mechanisms. Port them as ordinary
carrier relation traversal: split where tension becomes visible, route what can
be accounted for, and preserve the rest as tension.

## Migration Rule

Only copy code from `Core4.Kernel` when it still preserves the Gojo direction.
Do not carry over signed `long` fold behavior as canonical storage. It may stay
as a projection only.

Prefer removing role wrappers when relation context already supplies the role.
A value in a transform slot is a transform; a side/travel pair in boundary
context is boundary tension direction. Dedicated types should appear only when
they carry new numeric structure.

Likewise, avoid introducing an event system as a separate ontology. Kernel
events are projections of newly visible tension: a relation site has encountered
participation that cannot be silently accounted for under the current read.
C# events may be useful in a runtime layer, but the numeric kernel should return
the tension/read structure directly.
