# Gojo Implementation Plan

Gojo is the next kernel pass after `Core4.Kernel`.

`Core4.Kernel` remains preserved for comparison. Gojo starts from the newer
carrier-layer reading:

- carrier-local storage is positive-flow storage
- sign is a cross-carrier read
- operations are carrier layers
- each layer has one carrier value in its transform slot
- cross-carrier relations are transform/attachment structure, not generic
  holders waiting for external method names
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

- point-to-point cross-carrier transforms
- origin-to-origin attachment without automatic modification
- point-to-origin carry/reset accumulation
- segment-to-segment stretch/scale transforms
- repeated segment-to-segment projection through explicit layer lengths

This phase should cover multiplication-like and division-like reads without
ordinary algebraic operator tags.

See `Gojo Structural Transform Relations.md`.

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
- `Gojo.Reads` holds consequential read passes over the same core structures.
  These helpers are not a second ontology; they keep traversal, support,
  residual, and structural settlement projections from crowding the primitive
  carrier/relation types.
- `Gojo.Diagnostics` holds inspection surfaces that are useful for tests and
  numeric exploration but are not action machinery by themselves.
- `Gojo.Interpretation` holds named lower-projection vocabulary, such as
  boolean-style aliases, when the name is useful but not primitive.
- `Gojo.Reads.StructuralTransformReads` is the first relation-shape transform
  reader. It treats a point as `(carrier location -> carrier)`, a segment as
  `(point -> point)`, and transform as relation-to-relation. It currently
  distinguishes origin attachment, internal add-and-settle point carry/reset,
  internal segment stretch, base-like carry chains, segment-produced point
  carry, segment-to-point endpoint scaling, and repeated segment projection
  through explicit layer lengths without route tags.
- The older one-shot structural transform calculator has been removed. Current
  transform tests now build a structure, add incoming participation, settle,
  and read the resulting carrier surfaces.
- Resolved transfer, open gather, and observation fanout now have structural
  settlement footholds:
  `L(source,k) -> receiver`, `L(source,m) -> L(unknown,0)`, and
  `(L(source,a) -> L(source,b)) -> observer`.
  These are tested against saturating arithmetic, receiver addition, and
  non-draining fanout rather than only against older route helper shapes.
- `Gojo Operation Shape Catalog.md` keeps a compact list of operation-like
  relation shapes such as `P(C0,k) -> P(C1,0)` for carry and `S0 -> S1.P1`
  for multiplication-like endpoint scaling.
- The first structural settling shape is `position surface -> relation surface`.
  Adding a carrier location or incoming relation surface updates a fresh frame
  accounting surface and repeatedly applies internal point transforms until no
  carrier can carry further. A relation `(lower 10 -> lower carrier) -> (upper
  0 -> upper carrier)` therefore settles lower `11` as lower `1` and upper
  `1`; the value `11` does not remain as an externally meaningful carrier
  value.
- The settled position surface is now a contribution/tension accounting
  surface rather than a flattened carrier map. `Gojo.Reads.ContributionReads`
  exposes raw contribution relations and lower total/scale/tension
  projections, so provenance, post-accounting scale marks, and unresolved
  carrier tension remain inspectable as relation structure. This is a frame
  accounting surface, not an optimized runtime cache; future dynamic passes
  may discard and recompute it whenever relation structure is perturbed.
- `CarrierLocation` has been reduced back toward substrate storage. It now
  carries only carrier/tick position plus its direct `ReadFrom` projection.
  Boundary, transfer, observation, and gather behaviors are no longer public
  location methods; they are relation-surface shapes read by
  `Gojo.Reads.StructuralTransformReads`.
- Boundary-style quotient and residue can be expressed either as the compact
  absorber `P(partition,k) -> P(whole,0)` or as the more explicit
  same-carrier boundary plus counter route:
  `(P(partition,k) -> P(partition,0)) -> P(whole,0)`. Both settle the local
  carrier before reads inspect it; the compact form now does this by creating
  carrier tension and immediately routing it. The explicit form exposes
  unrouted carrier tension when no counter absorbs it.
- Active same-carrier accounting is now executable:
  `P(carrier,k) -> P(carrier,0)` hardens local value to `value % k` and leaves
  `value - value % k` as visible carrier tension. `P(carrier,k) ->
  P(carrier,k)` clamps local value to `k` and leaves the excess as tension.
  A relation from that boundary relation to a point on another carrier can
  absorb carrier tension through the boundary unit.
- Segment-to-segment settlement is checked against ordinary multiplication by
  layer lengths: `S(C0,0,1) -> S(C1,0,m)` maps `x` on `C0` to `x*m` on `C1`.
  Repeating unit segment copies now copies participation through layers; powers
  should come from an explicit repeated basis-scaling structure, not from
  reusing contribution bookkeeping as hidden state.
- Same-carrier bare spans such as `L(C,2) -> L(C,5)` remain boundary/span
  structure. They are not consumed as transfer routes merely because they are
  location-to-location relations.
- Coupled counter decrement is represented as opposed counter participation.
  A draw routed into `counter.Inverse()` can be compared with existing forward
  counter ticks through `TensionOn` and `ExclusiveOn`; remaining forward count
  or opposed overdraw is visible without signed storage. The current structural
  settle path has only a foothold for opposed point carry; refill/borrow
  routing still needs a structural pass.
- `NewTensionAfter(accounted, reader)` returns the carrier
  ticks that remain newly visible after the current read frame subtracts what
  local structure already accounts for.
- `NewTensionAfter(reader, accounted...)` accumulates multiple
  compatible accounting surfaces before reporting new tension, giving route
  capacity a numeric foothold without adding a route wrapper yet.
- `AccountedBy(reader, accounted...)` returns a `Relation`
  whose left slot is the participation accounted by the current read and whose
  right slot is the newly visible tension.
- Clamp is currently this same accounting read with no continuation route. The
  accepted portion stays on the left of the returned relation, and the excess
  stays visible as tension on the right. That excess may then gather into an
  open route or remain unresolved.
- A carrier can self-absorb larger positive counts only until active structure
  gives it an accounting boundary. For example, `23` ticks on an unbounded
  carrier are just `23`, but `P(partition,10) -> P(partition,0)` settles them
  as local `3` plus carrier tension `20`.
- A `Relation` can be used as accounting input by reading both slots as carrier
  locations in the current context. This lets route-like capacity, split reads,
  and prior relation results feed the same mechanism without a route class.
- `Relation.CarrierLocations()` now traverses nested relation surfaces.
  `LeftCarrierLocations()` and `RightCarrierLocations()` keep the ordered side
  distinction when context needs it.
- Structural transfer is now a relation shape:
  `L(source,k) -> receiver surface`. Settlement moves
  `min(source value,k)` from the source carrier into every carrier location on
  the receiver surface and leaves `max(source value-k,0)` visible on the
  source. Ordered relation surfaces give conserved branch distribution. A
  transfer route fires at most once per settle pass, but it can fire again in a
  later pass if new visible source participation appears.
- The old route-sequence and residual-route helper APIs have been removed from
  Gojo. Their useful behavior is now tested through settled relation surfaces.
  Residual settlement should be added as relation structure when needed rather
  than kept as a primitive `Relation` query.
- Transfer into an opposed carrier already gives the first reflection shape.
  The opposed carrier stores positive ticks in its own direction, and those
  ticks can route back through a later relation when read from the original
  carrier frame.
- Structural observation is now
  `(L(source,a) -> L(source,b)) -> receiver surface`. Settlement reads up to
  `|b-a|` from the source carrier into the receiver surface without draining
  the source. Several observation relations can fan out from the same source
  without splitting it.
- Structural open gather is now `L(source,m) -> L(unknown,0)`. Settlement
  raises `m` to cover visible carrier tension without draining the source, so
  the same persisted tension is not gathered twice. The open side leaves an
  attachment potential that can later be replaced by a resolved receiving
  surface.
- Loop, ping-pong, refill, and route hardening are intentionally not fully
  solved by helper APIs anymore. They should be added as structural relation
  settlement shapes when the numeric form is clear.
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
- `Gojo.Reads.RelationReads.BoundaryPointsOn(reader, relations...)` discovers the ordered
  carrier locations where endpoint participation can change. This mirrors the
  old support span split mechanism without creating a support-specific span
  type.
- `RelationReads.TraversedBoundaryPointsOn(reader, travel, relations...)` reads the
  boundary sites crossed by an ordered travel relation. The start site is not
  repeated as a new encounter, the end site is included, and results are ordered
  by travel direction while preserving opposed carrier surfaces.
- `Relation.TraversalSitesOn(reader)` exposes the travel endpoint gate as
  relation state: the start site is paired with zero new flow, while the end
  site is paired with the travel flow. This makes start-excluded/end-included
  behavior inspectable as carrier participation.
- `RelationReads.TraversedBoundaryStatesOn(reader, travel, relations...)` pairs each
  crossed boundary site with the travel flow that caused the encounter. These
  site-flow relations can observe, gather, or route without introducing an
  event type.
- `RelationReads.TraversedSpansOn(reader, travel, relations...)` splits an ordered
  travel relation into the carrier intervals occupied between crossed boundary
  sites. Same-carrier reverse travel keeps span sites on the original carrier;
  the span flow is what moves onto the inverse carrier.
- `RelationReads.TraversedSpanStatesOn(reader, travel, relations...)` pairs each
  traversed span with its local flow, so path occupancy and path flow can be
  inspected separately.
- `RelationReads.TraversedSupportStatesOn(reader, travel, support...)` reads the
  support state on every traversed path span. Unsupported path spans stay
  present as zero reverse/forward support, so blocked traversal remains visible
  instead of disappearing during projection.
- Traversed path projections `TraversedSupportOn`, `TraversedTensionOn`, and
  `TraversedExclusiveOn` apply the same reverse/forward state reads to only the
  path occupied by the travel relation.
- `TraversedPassOn` and `TraversedBlockOn` are path filter projections:
  passing is supported traversal, and blocking is unsupported traversal. They
  are not route objects or truth values.
- `Gojo.Diagnostics.TraversalDiagnostics.Traverse(reader, travel, relations...)`
  returns a `TraversalRead` that groups the same traversal sequence in
  discoverable order: boundary points, boundary states, spans, span states,
  support states, support/tension/exclusive projections, and pass/block
  projections. It is an inspection surface over the existing relation-shaped
  outputs, not a new numeric object replacing those outputs.
- TODO: Revisit whether `TraversalRead` should remain a named projection bundle
  or be replaced by a richer Core4-shaped traversal observation. The current
  shape is useful for tests and inspection, but a mature numeric investigator
  should discover what is present in the traversal surface without probing every
  conceptual projection by name.
- `RelationReads.CoveredSpansOn(reader, relations...)` returns adjacent carrier-local
  `Relation` intervals between those boundary points, skipping gaps where no
  input relation covers the interval. Opposed surfaces stay on the opposed
  carrier rather than being flattened onto the reader carrier.
- `RelationReads.CoveredSpansWithinOn(reader, scope, relations...)` reads coverage only
  inside one explicit scope relation, clipping covered intervals to that scope.
- `RelationReads.SupportStatesOn(reader, relations...)` returns nested relations:
  the left slot is a carrier-local interval and the right slot is the
  reverse/forward participation pair for that interval.
- Multiple compatible relations covering the same interval merge by increasing
  the forward or reverse carrier participation count in that state pair.
- `RelationReads.SupportStatesWithinOn(reader, scope, relations...)` does the same
  inside one explicit scope, preserving clipped intervals and their local
  reverse/forward state.
- `Relation.SupportOn(reader)`, `Relation.TensionOn(reader)`, and
  `Relation.ExclusiveOn(reader)` project a reverse/forward pair without leaving
  the relation system. They can read either a bare reverse/forward pair or a
  nested support state.
- Span projections `RelationReads.SupportOn`, `RelationReads.TensionOn`, and
  `RelationReads.ExclusiveOn` preserve the carrier intervals that carry those
  projected states and omit zero projected states.
- Scoped projections `SupportWithinOn`, `TensionWithinOn`, and
  `ExclusiveWithinOn` apply the same support/tension/exclusive reads inside a
  chosen interval.
- `Gojo.Interpretation.BooleanStyleReads` provides boolean-style vocabulary as
  aliases over those projections: `OrOn` is support, `AndOn` is tension/shared
  opposed participation, and `XorOn` is exclusive unopposed participation.
  These names live outside the primitive relation type because they are lower
  projections, not new structure.
- `RelationReads.UncoveredSpansOn(reader, scope, covered...)` reads scoped absence
  by splitting one visible scope relation around covered intervals. `NotOn` is
  only this scoped absence projection; there is no global negation outside a
  chosen carrier interval.
- `BooleanStyleReads.PassOn` and `BooleanStyleReads.BlockOn` are scoped filter
  projections over the same split: pass reads the covered intervals inside the
  scope, while block reads the uncovered intervals. They are not new truth
  values or route objects.
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

- resolved transfer route shape: `L(source,k) -> receiver surface`
- zero-unit observation shape:
  `(L(source,a) -> L(source,b)) -> receiver surface`
- open tension shape: `L(source,m) -> L(unknown,0)`
- route capacity and saturation
- counters
- wrap
- clamp (already accounted by `AccountedBy` with no continuation route)
- ping-pong
- segment end-to-start closure

This phase should make boundary behavior compositional instead of flag-driven.

Next consolidation target:

- See `Gojo Route Generalization Notes.md`.
- See `Gojo Structural Transform Relations.md`.
- The old named route helpers have been removed. Keep extending relation
  settlement rather than rebuilding those helpers under new names.
- Prefer relation shape and read context over drain/observe/gather flags.

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
