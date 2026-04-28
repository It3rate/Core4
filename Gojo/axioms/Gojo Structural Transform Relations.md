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

## 4. Point-To-Origin Transform

A point-to-point relation from a nonzero lower point to an upper origin creates
a threshold/accumulator relation:

```text
(lower k -> lower carrier) -> (upper 0 -> upper carrier)
```

The lower carrier fills until it reaches the related point. When flow crosses
that point, the crossing can move participation into the upper carrier and the
lower carrier resets to its local residue.

Because this is point-to-point, there is no segment length to stretch. The
natural mechanical behavior is overflow-and-clear rather than resize.

Examples:

- `lower 1 -> upper 0` means every lower tick crosses the point, so a chain of
  such relations behaves like additive accumulation upward.
- `lower 2 -> upper 0` creates binary-style grouping: every two lower ticks
  produce one upper tick.
- `lower 10 -> upper 0` creates decimal-style grouping: lower ticks `0..9`
  remain local, crossing `10` moves one tick upward and leaves residue.

A chain of point-to-origin transforms gives positional carrier layering:

```text
ones 10 -> tens 0
tens 10 -> hundreds 0
hundreds 10 -> thousands 0
```

The same mechanism also supports other bases, counters, and grouped
discernment.

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

## 7. Repeated Segment Relations Produce Powers

Repeated segment-to-segment relations create power-like behavior.

For example:

```text
(a 0 -> a 1) -> (b 0 -> b 1)
(b 0 -> b 1) -> (c 0 -> c 1)
(c 0 -> c 1) -> (d 0 -> d 1)
```

Reading through three repeated segment relations gives cubed behavior. Adding
another repeated layer gives fourth-power behavior.

This preserves the earlier Core4 idea that powers emerge from repeated
self-fusion or repeated unit relation, not from a separate exponent operator.

## 8. Point Transform Versus Segment Transform

The distinction is structural:

- point-to-point relation: threshold, overflow, carry, reset, borrow, positional
  accumulation
- segment-to-segment relation: stretch, scale, multiply, divide, power by
  repeated relation
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
- adding a carrier location into that structure returns a new settled structure
- origin-to-origin point transform preserves the incoming local value and does
  not accumulate
- nonzero-point-to-origin transform carries by threshold and leaves local
  residue
- chains of point transforms carry residue upward through base-like layers
- segment-to-segment transform stretches the target segment by the incoming
  value and both segment lengths
- repeated segment transform chains carry the previous stretch forward, giving
  the first power-like behavior

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

This does not preserve the old route API as the preferred model. It gives a
new structural foothold so route, overflow, observation, and gather behavior can
be re-expressed through relation shape.

## 11. Consequence For Routes

The current route methods are executable scaffolding, but the long-term model
should not depend on external parameterization.

Instead of:

```text
TransferThrough(reader, relation)
ObserveThrough(reader, relation)
GatherThrough(reader, relation)
```

the system should be able to inspect relation shape:

- is this a point transform?
- is this a segment transform?
- does crossing expose overflow?
- does the related surface provide continuation?
- does the related surface only observe?
- is the related surface open `m/0` attachment potential?

The answer should come from carrier relation structure, not from a method name
or tag.

## 12. Implementation Direction

The next kernel pass should prioritize structural relation reads over adding
more named route helpers.

Recommended order:

1. Move more behavior from external read-through-transform calls into
   add-and-settle structure reads.
2. Re-express current overflow, transfer, observation, and gather helpers as
   transitional conveniences over structural relation reads.
3. Demote or remove external route parameterization once relation shape carries
   the behavior.
4. Extend structural reads to underflow/refill, opposed carriers, and tension
   surfaces.

Do not delete the current executable model until the structural version covers
the same tests. The current code is useful scaffolding; the correction is that
it should not be the final public mechanism.

## 13. Guardrail

Carriers are low-level position holders.

Relations bind meaning from those positions.

If a behavior cannot be discovered from carrier relation structure, the model
has probably leaked meaning back into external symbolic control.
