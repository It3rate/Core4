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
- `Relation.SupportStatesOn(reader, relations...)` returns nested relations:
  the left slot is a carrier-local interval and the right slot is the
  reverse/forward participation pair for that interval.
- `Relation.SupportOn(reader)`, `Relation.TensionOn(reader)`, and
  `Relation.ExclusiveOn(reader)` project a reverse/forward pair without leaving
  the relation system. They can read either a bare reverse/forward pair or a
  nested support state.
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

- resolved transfer routes
- zero-unit observation routes
- open tension routes
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
