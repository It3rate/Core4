# Core4 Boundary Overflow Axioms

This note records the target model for carrier-layer boundaries, newly visible
tension, overflow, underflow, route coupling, loop closure, counters, filters,
and directed segment boundaries.

It builds on `Core4 Carrier Read And Resolution Axioms.md`.

The current kernel does not yet implement this whole mechanism. This file is a
design target for the next implementation pass.

## 1. Layer Order Is Dependency Order

Carrier layers should be visualized as dependency layers.

Earlier crystallized layers sit lower. Later interpretation, resolution,
operation, and read layers sit above them.

This vertical order is not size order.

For example, the following is legal:

- meter layer
- centimeter layer
- kilometer layer

The centimeter layer contracts the meter layer. The kilometer layer expands the
centimeter layer. The order says which layer depends on which earlier layer,
not which tick is smaller or larger.

## 2. One Transform Per Layer

One layer relates to its dependency layer through one numeric transform.

A single layer should not simultaneously scale, shift, and change resolution.
Those are separate layers:

- resolution layer
- scale layer
- shift layer

or another dependency order if the physical construction calls for it.

The transform slot contains an ordinary carrier-located value. That value has
the usual Core4 read:

- positive: expansive
- negative: contractive
- zero: additive, translational, or non-mixing, depending on route structure

Nothing special makes that value a transform except its position in the layer
relation. A `CarrierLocation` on the unknown carrier may be used until it is
crystallized or attached more specifically. A host-language signed scalar is
only a projection of that relation.

This keeps every layer inspectable. A complex operation is a stack or network
of simple carrier layers, not one opaque operation tag.

## 3. Operations Are Carrier Layers

Every stable operation can be read as a carrier layer.

Reading is traversal through carrier-layer relations.

Multiplication, division, resolution, approximation, inversion, addition,
branching, filtering, wrapping, clamping, and oscillation differ by:

- the carrier value in the transform slot between layers
- the newly visible tension exposed by the layer
- the routes attached to that tension
- whether routed flow drains, observes, wraps, branches, or becomes tension

This means an equation is not a symbolic expression outside the carrier system.
It is a carrier-layer path that values can be read through in either compatible
direction.

## 4. Resolution And Crystal Layers

A high-resolution carrier layer can feed a crystal layer.

For example, a Fahrenheit reading may arrive with fifty later-layer ticks per
degree. That high-resolution layer can relate to a Fahrenheit crystal layer
whose native tick is one degree Fahrenheit.

Then:

- the high-resolution layer keeps its unoverflowed layer ticks
- the Fahrenheit crystal layer absorbs whole degree overflows
- reading through the high-resolution layer exposes those layer ticks
- reading through the crystal layer exposes whole Fahrenheit ticks

No precision is destroyed unless a read intentionally projects only through the
coarser crystal layer.

## 5. Boundary As A Carrier-Layer Fact

A layer with a transform creates a boundary.

For a contractive ten-partition layer over a carrier tick:

- partition-layer values `0` through `9` remain in the partition layer
- the next forward overflow crosses the boundary
- that overflow can increment the earlier carrier tick and drain the partition
  layer

For an expansive ten-tick approximation layer:

- ten lower carrier ticks fill one approximate tick
- overflow emits one approximate tick
- leftover lower ticks may remain as attached-layer state or tension depending
  on route

The boundary belongs to the layer relation. It is not a separate symbolic
operator, and crossing it is not automatically an event forever. If the
crossing is already accounted for by a route or reservoir, no new event is
needed until that accounting fails or saturates.

## 6. Boundary Encounters As New Tension

An event is the appearance of new tension at a relation site.

This means a boundary crossing, endpoint touch, fusion-point encounter,
orthogonal attachment, support split, route junction, clamp, or filter block
all use the same mechanism:

- incoming participation reaches a relation site
- local structure accounts for some or all of it
- unaccounted participation becomes newly visible tension
- that new tension may route, observe, branch, harden, or remain unresolved

If a route already accounts for overflow past one carrier tick, then values
inside that routed region do not keep producing events. If that route runs out
of capacity, loses compatibility, reaches another boundary, or exposes a new
site, new tension appears there and the system reports that.

Several compatible routes or reservoirs may account for the same incoming
participation together. Their visible capacities accumulate in the reader
frame. Orthogonal, opposed, invisible, or independent carrier participation does
not silently count toward that capacity unless a relation explicitly makes it
compatible. Same-lineage duplicate carriers may account for one another when
the reader sees their participation as aligned.

A relation can itself be read as accounting capacity by reading its slots as
carrier locations in the current context. This is how route-like capacity,
split reads, and prior relation results can feed the same mechanism without a
separate route ontology.

Boundary tension still has side and travel direction.

Side and travel are not host-language signs. They are carrier-located numeric
participations whose aligned or opposed read gives the familiar `+` or `-`
projection.

Endpoint and support-span reads follow the same rule. A zero-magnitude
endpoint can still be a real boundary site; endpoint visibility depends on
carrier alignment, not on the endpoint having nonzero magnitude. Directed
travel is read as carrier participation: forward travel appears on the reader
carrier, and reverse travel appears on the reader's inverse carrier.

At the current Gojo pass, an event does not need its own object. It is a read of
newly visible tension in context. If routing later needs an explicit value, it
should reuse the same general relation structure as layer relations rather than
introducing a separate event ontology.

For a bilateral or bounded layer, there are four primitive boundary tension
directions:

- `++`: positive side, forward/outward crossing
- `+-`: positive side, backward/inward crossing
- `-+`: negative side, forward/inward crossing
- `--`: negative side, backward/outward crossing

Common discussion may name them more plainly:

- positive overflow
- positive refill or underflow
- negative refill or underflow
- negative overflow

The important point is that overflow is not one event kind. Each side and
travel direction can route independently when new tension appears there.

Symmetric behavior is common, but it is not required.

## 7. Partition Overflow And Refill

A simple ten-partition carrier layer over a whole carrier tick shows the basic
mechanism.

Forward accumulation:

- the partition layer fills from `0|10` toward `9|10`
- one more partition-layer tick exposes positive overflow tension
- whole carrier tick increments
- the partition layer drains or wraps to `0|10`

Backward accumulation:

- subtracting one partition-layer tick from `0|10` exposes positive refill or
  underflow tension
- whole carrier tick decrements if a whole tick is available
- the partition layer refills to `9|10`

So `23 9|10` plus one partition-layer tick becomes `24 0|10`.

And `24 0|10` minus one partition-layer tick becomes `23 9|10`.

This is boundary routing, not primitive signed arithmetic.

The current executable foothold composes this rather than adding a special
boundary-route object. A carrier value first reads through a boundary as
`whole residue|boundary`. The whole-side carrier ticks can then transfer
through an ordinary route. A nested receiving side can update both the wrapped
whole carrier and a counter. If that route has too little source-side capacity,
the untransferred whole-side ticks remain visible as overflow tension while the
residue remains on the partition carrier.

## 8. Tension Routes

New tension may attach to zero, one, or many routes.

Routes can connect the tension to:

- the same carrier layer
- a parent crystal layer
- an opposed carrier layer
- a counter carrier
- a tension reservoir
- a boolean or support filter
- a branch of several carriers
- a merge into one carrier
- an open site that has not yet chosen a route

Unrouted new tension remains visible as tension.

Routing is the main way the system lowers tension dynamically rather than
solving an external equation.

## 9. Drain Versus Observe Without Flags

The system should not need a `drain` flag.

Drain should emerge from relation shape.

A resolved route with unit participation transfers flow:

- routed content enters the target
- the source reservoir updates or drains according to the boundary law
- overflow is consumed to the degree the target absorbs it

The current executable foothold reads a plain relation this way:

- left side: source-side capacity/accounting surface
- right side: receiving carrier surface
- output: a relation from source accounting split to the received carrier
  surface

So a route with source capacity `10` and target site `3` receiving incoming
`15` reads as source `10 + 5 tension` and target `13`. No route wrapper is
introduced; the route role comes from asking to transfer through that relation.

If the receiving side is itself a relation, the same accounted participation is
read through each receiver slot while the source is accounted once. This gives
the first executable branch shape: for example, one boundary crossing can update
a wrapped continuation carrier and a counter carrier as one routed read.

An ordered route sequence reads by feeding only remaining source tension forward
to later routes. If an earlier route fully accounts for the incoming
participation, traversal stops and later routes do not wake. This is an initial
capacity/saturation rule, not a separate scheduler.

A zero-unit route observes or claims the newly visible tension:

- the route can sense that new tension appeared
- it can count, mark, branch a signal, or create a claim
- it does not consume the source reservoir by itself

The current executable foothold reads observation through the same relation
shape but with a different context:

- left side: the observable amount or scope
- right side: marker, counter, or claim surface
- output: a relation from unchanged source tension to the marked receiver
  surface

So an incoming source value of `7` observed through `4/0` into marker `100`
reads as source `0 + 7 tension` and marker `104`. The observation is bounded by
what the reader can see; invisible incoming participation marks nothing.

An open route accepts potential but has no committed transfer law yet:

- newly visible content can gather as unresolved tension
- a later relation may harden and absorb it

The current executable foothold reads open gathering through the same relation
shape again:

- left side: unresolved potential surface
- right side: open commitment surface
- output: unchanged source tension paired with the updated open route

So an incoming source value of `7` gathered through `0/0` reads as source
`0 + 7 tension` and route `7/0`. If that gathered `7` is later paired with a
resolved receiving site, it can act as source-side capacity in a transfer read.

So the numeric ladder is:

- `0/0`: open route
- `m/0`: tension claim, marker, or unresolved tension potential
- `m/u`: resolved transfer route

This preserves the existing Core4 open/unbound/resolved ladder.

## 10. Event Projection As New Tension

An event is the projection of newly visible tension. It is not a primitive
signal separate from the numeric structure.

Tension may appear because overflow or underflow has no compatible route under
the current read, but overflow is only one source. Tension can also appear when:

- a traversal reaches a segment endpoint
- a traversal reaches a fusion point between carrier participations
- an orthogonal carrier attaches to a site
- a support span splits at a new boundary
- a filter blocks participation
- a route reaches its capacity
- a previously open site hardens or loses compatibility

Examples:

- `43` read down through `7` and `3` gives `2` plus overflow tension if no
  compatible intermediate carrier layer exists
- the same value can read without tension at an intermediate resolution layer
- a clamp is a boundary with no continuation route, so overflow becomes tension
- a partial route absorbs some content and leaves the rest as tension
- an overflow reservoir that accepts `10..20` does not emit again at `11`, but
  it emits new tension when the value reaches `21`

Tension is not an exception. It is real content that the current structure has
not explained or cannot silently continue to account for.

## 11. Self Absorption Versus Routed Overflow

When a value overshoots a carrier's ordinary positive range, the first possible
behavior may be self absorption.

Self absorption means:

- the carrier accepts a larger positive count on itself
- no external route is needed
- no new tension is produced

For example, a partition carrier with unit size ten may be able to hold value
`50` and read that as five whole partition cycles if its own boundary permits
self absorption.

Routed overflow is different:

- the carrier crosses a boundary
- newly visible tension leaves or updates the source layer
- another carrier, counter, crystal layer, or tension reservoir receives it

The boundary law decides which behavior is active.

## 12. Underflow And Opposed Carriers

No positive-flow carrier can go below its own origin.

If flow attempts to move below zero and no route exists, underflow becomes
tension.

If an opposed carrier is fused into the structure, underflow can route into
that opposed carrier. The opposed carrier still receives positive flow in its
own direction.

So reverse movement is not negative storage. It is routed positive storage on
an inverse participation.

The current executable foothold confirms this directly: a resolved transfer
route can receive forward flow into the inverse carrier, and a later route can
read that inverse-carrier participation from the original frame and transfer it
back into the forward carrier. This is the first reflection/ping-pong shape.

## 13. Looping And Circular Boundaries

Looping is boundary routing, not a separate loop primitive.

A circular two-boundary relation may be:

- positive overflow routes to the origin of the opposed carrier
- opposed overflow routes back to the origin of the positive carrier

If routed content drains and re-enters the opposite side, the value circulates.

If no route can absorb the content, the boundary produces tension.

If a counter route observes or absorbs each newly visible crossing tension, the
system can count periods without changing the circulating content unless the
route is coupled.

This gives modulo, wrap, periodicity, and loop counting as different route
structures.

## 14. Drainable And Non-Drainable Loops

A circular flow may or may not drain on boundary crossing.

Drainable loop:

- boundary crossing transfers content to the next carrier
- source reservoir wraps or clears
- flow continues around the circuit

Non-drainable observer loop:

- boundary crossing is detected
- source reservoir does not clear
- tension may accumulate if the value has crossed a boundary but no coupled
  route absorbs it

The distinction comes from whether the route is resolved transfer or zero-unit
observation.

## 15. Counters

A counter is a carrier route attached to newly visible boundary tension.

It may be coupled:

- absorbs one overflow quantum
- source drains or wraps
- counter increments in its own positive direction

It may be observational:

- sees the newly visible tension
- records a marker or claim
- source is not consumed

Refill can draw from a coupled counter if the inverse route exists:

- underflow asks the counter for one quantum
- counter decrements by routing through its opposed participation or by tension
  if decrement is not supported
- the partition layer refills

If no compatible counter or tension source can supply refill, negative-side
tension appears.

## 16. Directed Segment Boundaries

A directed segment can be built from fused carrier participations, but its
boundaries do not have to be only the native units.

A segment boundary may be:

- a carrier origin
- a carrier tick
- a value read through one side of the fused pair
- a value read through the other side
- a pair of values, one read through each carrier
- a higher crystallized segment carrier

This means a directed segment itself can become a boundary object.

For example:

- define a segment by two values
- route the segment end back to its beginning
- if the route transfers flow, the segment drains and wraps
- if no counter or absorber receives the routed tension, the drain may produce
  tension

A segment used as a boundary is not special. It is a carrier-layer structure
whose endpoints expose new tension when continuation is not already accounted
for.

## 17. Segment Loop Closure

The end of a segment can route to its own beginning.

This creates loop closure:

- crossing the end boundary maps to the start boundary
- the source segment may drain, wrap, or only signal depending on route
- a counter can attach to count laps
- another route can absorb the full value
- an unrouted excess becomes tension

If the segment also has an opposed participation, the start and end can support
ping-pong behavior:

- positive end overflow routes into inverse flow
- inverse end overflow routes back into positive flow

This gives reflection and oscillation without separate reflection tokens.

## 18. Branch And Merge

Branching is one newly visible tension site with multiple routes.

For example, one overflow tension can route to:

- a counter carrier
- a wrapped continuation carrier
- a tension reservoir
- an observational marker

Merging is several newly visible tension sites routing to one carrier.

Neither branch nor merge needs to be a primitive symbolic graph operation.
They are route multiplicity patterns over tension sites.

## 19. Boolean Filters

Boolean-like operations should be expressible as boundary and support filters.

A filter is a layer or route that can:

- pass supported flow
- block unsupported flow into tension
- split flow into passed and unpassed carrier-layer state
- observe a crossing without draining it
- merge several support conditions into one route

This makes boolean reads lower projections over flow support:

- `and`: multiple supports must route into one pass condition
- `or`: any compatible support may route into pass
- `not`: absence becomes visible only inside an explicit scoped carrier
  interval, or opposed participation becomes visible through an opposed route
- branch: support routes to multiple continuations
- selection: only supported routes transfer flow

The current `SupportReadEquations` already points this way by representing
boolean-like reads as relation-shaped support projections rather than as a
separate boolean algebra.

In the Gojo pass, support states should stay inside the same recursive relation
structure:

- a carrier interval is a relation between endpoint locations
- a support state is a relation between that interval and a reverse/forward
  participation pair
- support, tension, and exclusive-support reads are projections over that
  reverse/forward pair
- scoped support is the same read clipped to an explicit carrier interval

So support splitting and boolean-like filtering should not introduce a separate
span or truth-table ontology unless new numeric structure genuinely requires it.
Boolean-style names can exist as lower projections: `or` reads support, `and`
reads shared opposed participation/tension, and `xor` reads exclusive unopposed
participation.

Scoped absence is the current safe form of `not`: a scope relation is read as
carrier-local boundaries, covered intervals are clipped to that scope, and the
remaining visible intervals are returned as ordinary relations. This keeps
absence numerically grounded. There is no meaningful global complement without
first naming the carrier surface that absence is being read through.

The first executable filter projection follows the same rule. A pass read is
the covered carrier intervals inside the scope. A block read is the uncovered
carrier intervals inside the same scope. The difference is projection context,
not a separate boolean value or a symbolic branch table.

The first executable merge projection is equally small: when several relations
cover the same carrier interval, their compatible flow participations add into
the same reverse/forward state pair. Merge is therefore visible as larger
carrier participation before it needs any separate route graph object.

## 20. Oscillation And Field-Like Response

Oscillation can emerge from routed boundary response.

Example pattern:

- position carrier has positive and inverse fused sides
- velocity carrier is incremented or decremented depending on which side is
  active
- overflow routes switch or oppose velocity at boundaries
- repeated route response produces oscillation

A gravitational or spring-like pull can be read similarly:

- side of position determines route direction
- distance or later-layer magnitude affects how strongly velocity changes
- opposing carrier flow absorbs attempted underflow
- tension remains where route capacity is insufficient

This is not a hard-coded sine or gravity function. It is routed response in a
carrier-layer network.

## 21. Implementation Shape

The implementation should likely introduce an explicit boundary/read layer
before trying to encode all of this in `RelationFold`.

Useful concepts to preserve numerically:

- source carrier layer
- dependent carrier layer
- one carrier value in the transform slot
- boundary size
- boundary side
- travel direction
- overflow amount
- unoverflowed attached-layer amount
- route target
- route coupling state: open, zero-unit observation, resolved transfer
- whether the route absorbed all, some, or none of the new tension

The route coupling state should be encoded through existing Core4 numeric
structure rather than through enum tags where possible.

## 22. Current Kernel Mismatch

The current kernel still collapses many of these ideas into:

- signed `long` measures
- relation folds
- partition widths
- support projections

That is acceptable for the bootstrap layer, but the boundary implementation
should move toward:

- positive carrier-local storage
- sign as cross-read or opposed-carrier projection
- attached-layer ticks as live relation state
- tension as unrouted boundary content
- branch, merge, loop, clamp, and filter as route structures

## 23. Guiding Rule

A boundary is a site where flow asks for continuation.

Continuation is not a symbolic control-flow instruction. It is a numeric route.

If the route is resolved, flow transfers.

If the route is zero-unit, the new tension is observed or claimed without
consuming the source.

If the route is open, tension gathers.

If multiple routes attach, the new tension branches.

If multiple tension sites share a route, the new tension merges.

If the route returns to an earlier boundary, the structure loops.

If the route crosses into an opposed carrier, the structure reflects or
ping-pongs.

If no route can accept the content, the boundary clamps and tension remains.
