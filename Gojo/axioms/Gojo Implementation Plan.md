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
- `CarrierLocation.ReadThroughSegment(reader, segment, lapCarrier)` lets a
  directed segment act as a boundary. It reads whole segment crossings on the
  lap carrier and preserves residue as a carrier location on the segment
  surface. Reading exactly at the segment end returns one lap and the segment
  start, so end-to-start closure is ordinary segment-boundary reading.
- Coupled counter decrement is represented as opposed counter participation.
  A refill draw routed into `counter.Inverse()` can be compared with existing
  forward counter ticks through `TensionOn` and `ExclusiveOn`; remaining forward
  count or opposed overdraw is visible without signed storage.
- `CarrierLocation.NewTensionAfter(accounted, reader)` returns the carrier
  ticks that remain newly visible after the current read frame subtracts what
  local structure already accounts for.
- `CarrierLocation.NewTensionAfter(reader, accounted...)` accumulates multiple
  compatible accounting surfaces before reporting new tension, giving route
  capacity a numeric foothold without adding a route wrapper yet.
- `CarrierLocation.AccountedBy(reader, accounted...)` returns a `Relation`
  whose left slot is the participation accounted by the current read and whose
  right slot is the newly visible tension.
- Clamp is currently this same accounting read with no continuation route. The
  accepted portion stays on the left of the returned relation, and the excess
  stays visible as tension on the right. That excess may then gather into an
  open route or remain unresolved.
- A carrier can self-absorb larger positive counts until a boundary context
  asks for overflow. For example, `23` partition ticks are just `23` ticks on
  that carrier until `ReadThroughBoundary` projects them as `2 3|10`.
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
- `CarrierLocation.TransferThrough(reader, route, receiverReader,
  receiverCapacity)` composes a receiver-side accounting read after source
  transfer. The returned shape is `source split -> receiver split`, so source
  excess and receiver excess remain distinct tensions that can settle in later
  route passes. If the receiver reader cannot see the received carrier, no
  receiver tension is visible in that frame.
- `CarrierLocation.TransferThroughRoutes(reader, routes...)` reads an ordered
  route sequence. Each route receives only the source tension left by earlier
  routes, and traversal stops once no visible tension remains.
- Conserved branch behavior is currently this same ordered route read. If
  source `8` meets route capacities `5` and `5`, the first route can account
  for `5` and the second can account only for the remaining `3`.
- `CarrierLocation.TransferSourcesThrough(reader, route, sources...)` is the
  first conserved merge projection. Each source is accounted independently by
  the route's source side, and the receiver surface accumulates the accounted
  participation from each source in order. Source tension remains attached to
  the source that could not transfer.
- Transfer into an opposed carrier already gives the first reflection shape.
  The opposed carrier stores positive ticks in its own direction, and those
  ticks can route back through a later relation when read from the original
  carrier frame.
- Loop closure is currently plain composition: feed the receiving carrier
  location from one transfer into the next route. If the return route saturates,
  its source-side split exposes the unreturned tension. Opposed loop closure is
  the same composition through an inverse carrier, giving ping-pong without a
  reflection token.
- `CarrierLocation.ObserveThrough(reader, route)` is the first zero-unit
  observation projection. The route's left side sizes what can be observed, but
  the source is read with no drain, so the full visible incoming participation
  remains new tension. The right side receives the observed amount as a marker,
  counter, or claim.
- `CarrierLocation.ObserveThroughRoutes(reader, routes...)` reads each
  observation route against the same undrained source. This is the first
  observation fanout shape: several displays, sensors, or claims may each read
  the same visible participation without splitting the source.
- `CarrierLocation.ObserveSourcesThrough(reader, route, sources...)` is the
  observation-merge mirror. Several sources can update one receiver surface
  without being drained; each source still reports its own unchanged visible
  tension.
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
- `Relation.TraversedBoundaryPointsOn(reader, travel, relations...)` reads the
  boundary sites crossed by an ordered travel relation. The start site is not
  repeated as a new encounter, the end site is included, and results are ordered
  by travel direction while preserving opposed carrier surfaces.
- `Relation.TraversalSitesOn(reader)` exposes the travel endpoint gate as
  relation state: the start site is paired with zero new flow, while the end
  site is paired with the travel flow. This makes start-excluded/end-included
  behavior inspectable as carrier participation.
- `Relation.TraversedBoundaryStatesOn(reader, travel, relations...)` pairs each
  crossed boundary site with the travel flow that caused the encounter. These
  site-flow relations can observe, gather, or route without introducing an
  event type.
- `Relation.TraversedSpansOn(reader, travel, relations...)` splits an ordered
  travel relation into the carrier intervals occupied between crossed boundary
  sites. Same-carrier reverse travel keeps span sites on the original carrier;
  the span flow is what moves onto the inverse carrier.
- `Relation.TraversedSpanStatesOn(reader, travel, relations...)` pairs each
  traversed span with its local flow, so path occupancy and path flow can be
  inspected separately.
- `Relation.TraversedSupportStatesOn(reader, travel, support...)` reads the
  support state on every traversed path span. Unsupported path spans stay
  present as zero reverse/forward support, so blocked traversal remains visible
  instead of disappearing during projection.
- Traversed path projections `TraversedSupportOn`, `TraversedTensionOn`, and
  `TraversedExclusiveOn` apply the same reverse/forward state reads to only the
  path occupied by the travel relation.
- `TraversedPassOn` and `TraversedBlockOn` are path filter projections:
  passing is supported traversal, and blocking is unsupported traversal. They
  are not route objects or truth values.
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
- A traversed support filter composes the same way. `TraversedPassOn` returns
  `span -> state` relations, so the path span remains inspectable separately
  from its support state. The span width can become route capacity, and for
  reverse travel that capacity is projected onto the inverse path-flow carrier
  before accounting.
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
- clamp (already accounted by `AccountedBy` with no continuation route)
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
