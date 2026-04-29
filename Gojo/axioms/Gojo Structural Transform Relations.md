# Gojo Structural Transform Relations

This note records the current correction to the Gojo direction:

Relations must carry enough internal numeric structure to embody meaning. If
route or transform behavior exists only because an external method says "read
this relation as transfer", then the structure is still too much like a generic
holder.

The goal is not to add tags. The goal is to make relation shape itself carry
the discoverable affordance.

## 1. Everything Is Relation Of Elements

The target algorithm should treat points, segments, carrier attachments, and
cross-carrier transforms as one family of relations between elements.

In that model:

- a carrier is a low-level position holder
- a carrier location is a measurable position on that carrier
- a point is a relation from a carrier location to its carrier
- a segment is a relation between two points on the same carrier
- a transform is a relation between points, segments, or higher relation
  surfaces

So:

```text
point  = (carrier location -> carrier)
segment = (point -> point)
transform = (point -> point) or (segment -> segment)
```

The aim is not to create separate point and segment wrapper types. The shape of
the relation carries the meaning.

The current executable pass grounds both ends of a same-carrier segment as
points:

```text
S(C,2,5) = P(C,2) -> P(C,5)
```

That is a useful concrete simplification, not the only possible relation
construction. A more literal ordered construction can ground the first site and
relate later values through it:

```text
a = P(C,2)
b = a -> 5
```

In that construction, `2` is related to the carrier, while `5` is related to
the grounded site `a`; `5` is not independently grounded to the carrier until
some later relation supplies that grounding. This matters for metaphor and
reuse. A carrierless relation such as:

```text
2 -> 5
```

is a floating expression. It is not meaningless, but it is unbound. It carries
an ordered relational shape that can later be applied to a carrier, much like
words such as "double", "half", or "between" carry available structure before
they are attached to a particular domain. One way to think about metaphor is:
remove or loosen the original carrier grounding, preserve the relation shape,
then re-ground that shape on a different carrier.

The current code does not yet have a bare carrierless numeric element, so this
is a conceptual direction rather than an executable representation. `Carrier`
and `CarrierLocation` still provide the concrete substrate for this pass.

For a compact list of operation-like shapes that follow from this notation, see
`Gojo Operation Shape Catalog.md`.

## 2. Cross-Carrier Relation Means Transform

A relation between different carriers should generally be read as a transform
or attachment between carrier layers.

The carrier locations still store only positive ticks. The relation tells how
participation on one carrier can be read, accumulated, stretched, reset, or
continued through another carrier.

This gives a physical interpretation:

- a carrier holds position or accumulated participation
- a relation binds positions, points, or segments between carriers
- the read of that relation determines how flow crosses carrier layers

The transform is not a symbolic operator. It is the measurable relation between
carrier structure.

## 3. Origin-To-Origin Attachment

A point-to-point relation from origin to origin is a cross-carrier attachment
with no interior modification:

```text
(lower 0 -> lower carrier) -> (upper 0 -> upper carrier)
```

This is data-like attachment or shared reference. Values on each carrier can be
inspected in relation to the other, but the attachment does not by itself create
automatic accumulation, scaling, or overflow.

It says "these origins are related", not "flow should move through this
boundary."

## 4. Point-To-Origin And Same-Carrier Boundary Transform

A point-to-point relation from a nonzero lower point to an upper origin creates
a threshold/accumulator relation:

```text
(lower k -> lower carrier) -> (upper 0 -> upper carrier)
```

The lower carrier fills until it reaches the related point. When flow crosses
that point, the crossing can move participation into the upper carrier and the
lower carrier resets to its local residue.

The same physical crossing can be expressed locally first:

```text
(lower k -> lower carrier) -> (lower 0 -> lower carrier)
```

This is an active same-carrier reset boundary. If `23` enters a carrier with
`k = 10`, the carrier hardens to `3` and the unaccounted carrier tension is
`20`. That tension is not destroyed. A later relation may absorb it through the
same boundary unit:

```text
((lower 10 -> lower carrier) -> (lower 0 -> lower carrier))
->
(upper 0 -> upper carrier)
```

In that case the carrier tension `20` reads through the `10:1` boundary and the
upper carrier receives `2`.

A same-carrier clamp is the degenerate target-at-boundary form:

```text
(lower k -> lower carrier) -> (lower k -> lower carrier)
```

The local carrier hardens to `min(value,k)` and the excess remains carrier
tension.

Because this is point-to-point, there is no segment length to stretch. The
natural mechanical behavior is overflow-and-clear rather than resize.

Examples:

- `lower 1 -> upper 0` means every lower tick crosses the point, so a chain of
  such relations behaves like additive accumulation upward.
- `lower 2 -> upper 0` creates binary-style grouping: every two lower ticks
  produce one upper tick.
- `lower 10 -> upper 0` creates decimal-style grouping: lower ticks `0..9`
  remain local, crossing `10` moves one tick upward and leaves residue.
- `forward 10 -> forward_inverse 0` stores the crossing as positive
  participation on the opposed carrier rather than as a negative number.

A chain of point-to-origin transforms gives positional carrier layering:

```text
ones 10 -> tens 0
tens 10 -> hundreds 0
hundreds 10 -> thousands 0
```

The same mechanism also supports other bases, counters, and grouped
discernment.

Repeated point layers are additive powers rather than multiplicative powers.
For example:

```text
a 1 -> b 0
b 1 -> c 0
c 1 -> d 0
```

Every tick crosses every point layer, but the amount does not stretch. A value
of `15` entering `a` settles as `15` on `d`, not as `15 * 15 * 15`. This is
still a repeated relation shape; it is just the point/carry family rather than
the segment/stretch family.

A finite positive integer read is the same point/carry shape repeated at the
chosen base. An eight-digit decimal calculator stack is:

```text
digit0 10 -> digit1 0
digit1 10 -> digit2 0
digit2 10 -> digit3 0
digit3 10 -> digit4 0
digit4 10 -> digit5 0
digit5 10 -> digit6 0
digit6 10 -> digit7 0
```

Reading an earlier digit layer after settlement gives the residue/modulo for
that layer. Reading the final carried layer gives the distributed quotient that
made it through the stack.

Opposed point layers can also be related back to the original carrier:

```text
forward 10 -> forward_inverse 0
forward_inverse 10 -> forward 0
```

This is the first executable loop/ping-pong foothold. Flow can make a return
journey through the opposed carrier, and both sides still store only positive
ticks in their own direction. If the return has unresolved residue, that
residue remains visible on the opposed carrier.

Point-loop settlement only lowers while the carrier positions actually change.
A lossless loop such as `a 1 -> b 0` and `b 1 -> a 0` does not silently solve
itself. If one full settle pass returns to the same visible positions, the
load remains visible in that frame as unresolved participation.

This is deliberately different from a full path loop. Same-carrier reset now
performs local accounting and exposes crossing tension, but it does not by
itself claim that the tension has completed a return journey. A loop still
needs an absorber, counter, opposed carrier, or other relation to account for
the emitted tension.

## 5. Point Transform Direction

Increment and decrement have different natural starting sides.

For point-to-origin transforms:

- increments approach from the zero side
- crossing the point emits upward and clears local overflow to residue
- decrements or refill approach from the positive side
- a borrow from above can set the lower carrier to the maximum local residue
  before continuing downward

This is not signed storage. It is the same positive carrier storage read through
opposed or reverse flow.

The current refill/underflow reads are early executable versions of this idea,
but they are still called externally rather than being discovered from relation
shape.

## 6. Segment-To-Segment Transform

When a relation binds segments, the transform is between continuous extents:

```text
((lower 0 -> lower carrier) -> (lower 1 -> lower carrier))
->
((upper 0 -> upper carrier) -> (upper 1 -> upper carrier))
```

This is not an overflow-and-clear point boundary. It is a stretch/compression
relation. The segment gives a mechanical length that can expand or contract.

If a lower value of `7` flows into the lower unit segment, the related upper
unit segment can be read as expanded to `7`. A value of `3` on the upper side
then reads through that transformed segment as `21`.

This is multiplication-like behavior, but it is not an operator tag. It is the
physical result of relating one unit segment to another unit segment and then
reading participation through the stretched relation.

## 7. Repeated Segment Relations Project Through Layer Lengths

Repeated segment-to-segment relations copy or scale participation through the
explicit segment lengths in each layer.

For example:

```text
(a 0 -> a 1) -> (b 0 -> b 1)
(b 0 -> b 1) -> (c 0 -> c 1)
(c 0 -> c 1) -> (d 0 -> d 1)
```

Reading through these unit segment relations copies visible participation from
`a` to `b` to `c` to `d`. If a layer relates `0..1` to `0..7`, that layer
multiplies by `7`; if a later layer relates `0..1` to `0..3`, it then
multiplies by `3`.

Power-like behavior is still expected to emerge from repeated basis-scaling
structure, especially segment-to-point endpoint motion that changes the basis
used by later relations. It should not come from hidden contribution
bookkeeping that multiplies the same scale mark more than once.

## 8. Point Transform Versus Segment Transform

The distinction is structural:

- point-to-point relation: threshold, overflow, carry, reset, borrow, positional
  accumulation
- segment-to-segment relation: stretch, scale, multiply, divide, projection by
  repeated explicit layer lengths
- segment-to-point relation: move a point while preserving topology; candidate
  basis-scaling foothold for powers
- origin-to-origin point relation: attachment/reference without automatic
  modification

This is a numeric distinction in relation shape, not an enum.

## 9. Same Carrier And Cross Carrier Relations

There should not be a huge algorithmic divide between several relations on one
carrier and relations across several carriers.

The main constraint is that one carrier shares one basis unit definition. A
same-carrier segment therefore uses one carrier-local basis, while orthogonal
or cross-carrier relations may bind separate basis definitions together.

The natural boundary points of a carrier are the boundary points exposed by the
relations it contains. Because relations are ordered, those boundary points are
naturally interpretable without a separate action list.

If a carrier has no connection that can account for overflow or underflow,
that unaccounted participation remains as tension. It can be inspected later
and distributed when some relation structure can account for it.

## 10. Current Executable Pass

The first executable structural transform pass lives in
`Gojo.Reads.StructuralTransformReads`.

It recognizes relation shape rather than method-specific route declarations:

- point: `(carrier location -> carrier)`
- segment: `(point -> point)`
- point transform: `point -> point`
- segment transform: `segment -> segment`

The current behavior is intentionally small:

- a structure can be represented as current carrier-position surface related to
  an internal relation surface
- structural transform behavior enters through settlement of a structure; the
  older one-shot transform calculator has been removed so tests and callers
  must put participation into relation structure and inspect the settled result
- adding a carrier location or incoming relation surface into that structure
  returns a new settled structure
- one settle frame snapshots the relation nodes present at frame start and then
  repeatedly applies that relation surface until its contribution surface and
  relation surface stop changing
- incoming relation surfaces are decomposed into fresh frame contributions:
  a bare carrier location can enter as carrier participation when it is the
  incoming surface itself, points enter as carrier-position contributions, and
  existing contribution relations enter as already-accounted frame
  contributions
- bare carrier locations nested inside arbitrary relation structure do not
  automatically become position contributions; the relation context may be a
  span, route, boundary, or support surface rather than local load
- carrier position is no longer only a flattened `carrier -> ticks` map inside
  the settle frame. It is a contribution surface: each visible contribution has
  a source element, a carrier location, and a visible carrier-local scale
  location
- carrier tension is separate from value provenance. Each carrier has one
  positive-flow tension total in the settle frame. The persisted structure for
  unresolved carrier tension is numeric relation state:

```text
P(C,0) -> L(C,tension)
```

  This says the carrier has unaccounted participation in its own direction. It
  is not a signed scalar, and it is not a per-relation side table.
- settled structures keep that contribution surface on their left side; lower
  reads such as `PositionOn` collapse it to carrier totals when a total is what
  the caller wants
- because the contribution surface is returned as ordinary structure, it can
  feed the next settle frame without first being flattened back to points
- a segment relation replaces its own contribution on the upper carrier instead
  of multiplying the carrier total it produced on an earlier pass
- origin-to-origin point transform preserves the incoming local value and does
  not accumulate
- nonzero-point-to-origin transform carries by threshold and leaves local
  residue
- point carry first hardens the source carrier, creates carrier tension for the
  crossing amount, and then routes that tension through the related upper
  point. The compact cross-carrier point form is therefore executable shorthand
  for the same local-boundary-then-route mechanism, not a separate external
  calculation.
- same-carrier reset/clamp boundaries replace the source carrier with its
  locally accountable value and place the unaccounted source energy in the
  carrier's visible tension state
- a relation from that boundary relation to a point on another carrier can
  absorb carrier tension through the boundary unit
- the residual contribution left by point carry is visible as post-accounting
  structure with scale `0`; segment relations do not re-stretch that residual
  in the same settle frame
- same-carrier point-to-point relations are read as local segment structure,
  not as point transforms; this prevents a segment from accidentally behaving
  like a carry/reset relation
- chains of point transforms carry residue upward through base-like layers
- repeated point layers behave as additive powers: they preserve count through
  carry/reset rather than multiplying basis length
- reading a lower carry layer after settlement gives modulo/residue while
  reading higher layers gives the positional distributed value
- point carry can target an opposed carrier, and a second relation can carry
  opposed participation back through the original carrier without signed
  storage
- segment-to-segment transforms now also participate in add-and-settle:
  they do not clear the lower carrier, but stretch or create upper carrier
  participation from the lower carrier position and both segment lengths
- existing upper carrier participation stretches with the segment relation
- segment-produced participation can feed point carries in the same settled
  structure
- segment-to-point transforms move the target point while preserving the
  relation topology around that point; this is the first basis-scaling /
  multiplication-like structural update
- a carrier position produced by segment stretch can activate a
  segment-to-point endpoint move in the same frame
- repeated segment transform chains project through each layer's explicit
  segment length; powers need an explicit repeated basis-scaling structure
- resolved transfer routes now participate in structural settlement when the
  relation surface contains a bare carrier-location capacity:

```text
source capacity -> receiver surface
```

  Settlement transfers at most the visible source capacity, leaves remaining
  source participation as local residual value, and writes the received
  participation into the receiver surface. This gives saturating subtraction on
  the source and addition on the receiver without calling a transfer method.
  A transfer route fires once in a settle pass so capacity is not drained
  repeatedly by the quiet loop; if new source participation appears in a later
  pass, the same route can fire again.
- open gather routes now participate in structural settlement when the relation
  surface contains:

```text
source potential -> unknown 0
```

  Settlement raises the source potential to cover visible carrier tension
  without draining the source. Persisted carrier tension is not gathered twice;
  only newly visible tension beyond the current potential changes the route.
- observation routes now participate in structural settlement when the relation
  surface contains a bare same-carrier span as the left side:

```text
(source start -> source end) -> observer surface
```

  Settlement writes up to the span length into the observer surface while
  leaving source participation undrained. Multiple observation routes can
  therefore fan out the same visible source without splitting it.

Here, independent participation means carrier-local load that arrived from the
outside of the relation being applied, or from a different already-accounted
structure. Derived participation means load produced by the same relation
during the current frame. For example, if `lower 7` stretches `upper` to `7`,
the new `upper 7` is derived from that stretch. Reapplying the same stretch to
that derived `upper 7` as if it were independent would invent `49`. If `upper`
already carried an independent `3`, then stretching that independent `3` by
the lower `7` can produce `21` in the current executable model.

The current executable answer is value provenance by contribution plus
carrier-local tension accounting. A value contribution is ordinary relation
structure:

```text
source -> (carrier location -> carrier-local scale location)
```

The source says where the contribution came from. The carrier location says
what visible participation it contributes. The scale location is an
inspectable carrier-local accounting mark. A scale of `1` is ordinary carrier
participation. A segment-derived contribution carries the segment stretch as
its scale for inspection. A point-carry residue carries scale `0`, which makes
the residue visible as already-accounted participation for this settle frame
instead of fresh stretch input.

Tension is not stored as another contribution source. If a carrier cannot
account for all incoming participation, the unaccounted positive amount is
kept once on that carrier as:

```text
P(C,0) -> L(C,tension)
```

That tension can persist across frames and be absorbed later when a compatible
boundary route is added. If several receivers can absorb the same carrier
tension, the later branch/merge negotiation should also be relation structure;
the current executable pass only covers the single-route case.

This surface is now readable as relation structure. A plain point on the left
surface is lifted into the same contribution shape with the point itself as
source and scale `1`. A settled surface can be inspected in four lower
projections without changing the stored structure:

```text
source -> (carrier location -> carrier-local scale location)
```

- contribution read: returns the raw contribution relations
- total read: sums carrier locations for a requested carrier
- scale read: reads the carrier-local scale marks carried by the contribution
  surface
- tension read: reads the one unresolved positive tension total carried by a
  requested carrier

The total, scale, and tension reads are lower projections over the settled
surface. They are conveniences for action and tests, not a separate state
model.

The contribution surface should not be treated as an optimized runtime cache or
as predictive memory for a future perturbed structure. It is the visible
accounting surface for one settled frame. A later runtime may throw this whole
surface away and recompute it from the underlying carrier/relation structure on
every pass.

For the current executable shape, preserving the settled contribution surface
is still useful when a settled frame is fed through the same relation surface
again. If it were collapsed to only carrier totals, a segment-derived upper
position would look independent on the next pass and could be stretched again.
The contribution surface prevents that double accounting without adding a
separate symbolic flag.

TODO: when Gojo has an explicit runtime/source boundary, revisit whether
`AddAndSettle` should return only canonical source structure plus a disposable
settlement projection. Do not build dynamic optimization policy into this
surface yet.

The implementation should keep pruning named intermediate passes toward this
single relation-application algorithm as the numeric state becomes rich enough
to make every pass idempotent.

For a point transform such as:

```text
(lower 10 -> lower carrier) -> (upper 0 -> upper carrier)
```

adding `11` to the lower carrier does not leave an external value of `11`
waiting to be interpreted. The structure settles to:

```text
lower position = 1
upper position = 1
```

The whole crossing is absorbed by the upper carrier relation, and the lower
carrier keeps only its residue.

This does not preserve the old route API. It gives a structural foothold so
route, overflow, observation, and gather behavior are expressed through
relation shape.

## 11. Consequence For Routes

The named route methods have been removed from Gojo. The system now inspects
relation shape:

- point carry: `P(C0,k) -> P(C1,0)`
- segment stretch/copy: `S(C0,a,b) -> S(C1,c,d)`
- conserved transfer: `L(C0,k) -> receiver surface`
- observation: `(L(C0,a) -> L(C0,b)) -> receiver surface`
- open gather: `L(C0,m) -> L(unknown,0)`

The answer comes from carrier relation structure, not from a method name or
tag. Same-carrier bare spans such as `L(C,2) -> L(C,5)` remain span structure
and are not consumed as transfer routes.

## 12. Implementation Direction

The next kernel pass should prioritize structural relation reads over adding
more named route helpers.

Current order:

1. Point carry, segment stretch/copy, structural transfer, structural
   observation, and structural open gather are executable relation-settle
   shapes.
2. External route parameterization has been removed.
3. Next structural gaps are underflow/refill, opposed loop/cycle behavior,
   route hardening, and tension-surface balancing.

Do not reintroduce a helper API just to make those gaps convenient. Add the
numeric relation shape first, then a small read utility only if it remains a
projection over visible structure.

## 13. Guardrail

Carriers are low-level position holders.

Relations bind meaning from those positions.

If a behavior cannot be discovered from carrier relation structure, the model
has probably leaked meaning back into external symbolic control.
