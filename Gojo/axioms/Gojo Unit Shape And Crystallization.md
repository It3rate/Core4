# Gojo Unit Shape And Crystallization

This note records the current direction for unit shape in Gojo, especially as
it relates to crystallization.

The central split is:

```text
unit shape       what one tick scales as
resolution       what distinctions a carrier or port exposes
accounting       how a surface wraps, carries, clamps, or leaves tension
orientation      how a frame is ordered for read/settlement
```

Wrapping behavior, fractional residue, and reverse frame direction are not
unit shape. They are ways a unit is used through a particular carrier/relation
surface.

## 1. Carrier Unit Shape

Every carrier has an inherent unit shape for its native tick.

The default carrier unit shape is a linear one-tick atom:

```text
P(U,1)
```

where `U` is the carrier's unit atom. This atom does not need a human label
such as inch, meter, or lumen. Labels can be attached later, but the kernel
meaning should remain numeric and relation-shaped.

A carrier may also be created with a known unit shape:

```text
lengthCarrier.UnitShape = P(LengthUnit,1)
areaCarrier.UnitShape   = P(LengthUnit,1) -> P(LengthUnit,1)
```

Two carriers can share the same unit shape without being the same carrier.
This is useful for separate value holders, alternate resolutions, and copied or
crystallized surfaces.

## 2. Relation Unit Shape Is Calculated

Do not store unit shape on ordinary relations by default.

A relation's exposed unit shape should be calculated from:

- the unit shapes of carrier sites it mentions
- the relation surface shape
- the read or operation context

For example:

```text
S(Inch,0,10)
```

does not create `inch^10`. It is an inch-shaped surface with a ten-tick basis
or resolution. Its unit shape remains the carrier's native inch shape.

A port uses the same unit-shape rules as any other relation surface. Being a
port means the surface is exposed, not that it has a separate unit system.

## 3. Unit Shape Is Relation-Shaped

Unit shape should use the same relation structure as other Gojo structure, but
abstracted to unit tick `1`.

Examples:

```text
linear unit          P(u,1)
square unit          P(u,1) -> P(u,1)
man-hour             P(man,1) -> P(hour,1)
meters per second^2  P(m,1) -> (P(s,1)^-1 -> P(s,1)^-1)
```

The notation `^-1` is explanatory. The implementation should eventually use
unit-surface dual/inverse participation, not a string exponent.

Repeated participation expresses powers:

```text
u^2 = u -> u
u^3 = u -> u -> u
```

The repeated relation is visible structure. A compact calculated scale
signature such as `u:+2` may be useful as a read, but it should not replace the
relation-shaped unit surface as the source of truth.

## 4. Addition And Compatibility

Addition can collapse only when unit shapes are compatible:

```text
3u + 4u -> 7u
```

Incompatible addition is still valid. It remains relation-shaped rather than
collapsing:

```text
3u + 4v -> L(u,3) -> L(v,4)
```

This is like complex addition: real and imaginary parts can sit side by side
without reducing to one scalar tick.

Compatibility should not be confused with identical carrier identity. Carriers
may differ while sharing compatible unit shape. Conversely, two relation
surfaces on the same carrier may expose different higher structures depending
on operation context.

## 5. Transfer, Fusion, And Operation Results

Transfer/calibration does not multiply units.

For example:

```text
Light -> Heat
```

does not produce `light*heat`. The target unit shape wins for the received
value. Source residue remains source-unit tension.

Fusion/operation can produce composite unit shape:

```text
length * length       -> length^2
man * hour            -> man-hour
distance / time^2     -> acceleration shape
```

The operation surface decides which unit shape is exposed. A port group:

```text
S(X,0,3) -> S(Y,0,4)
```

is only paired extents until an operation surface reads it. A multiply-like
operation may expose area shape. An add-like operation may expose compatible
length shape or an unreduced relation-shaped sum.

## 6. Resolution, Basis Direction, And Unit Inverse

Resolution is not unit shape.

```text
S(C,0,10) -> P(U,1)
```

may expose whole/residue reads such as `2 7|10`, but the `|10` is a resolution
or basis feature, not a new unit shape.

Reverse orientation is also not automatically unit inverse:

```text
S(C,10,0)
```

has the same carrier unit shape as `S(C,0,10)`, but it reads through an
opposite ordered frame.

Unit inverse is a scale-law denominator, such as `s^-1` or `s^-2`. It should
be represented by inverse participation in the unit surface. It should not be
silently equated with every reverse frame or every opposed-flow carrier face.

## 7. Crystallization Split

Crystallization creates a reusable unit shape and may optionally project a
current value onto a carrier using that shape.

These are separate actions:

```text
derive unit shape
project current value
```

For example:

```text
width inch port -> height inch port -> multiply-like operation
```

can derive:

```text
square inch unit shape = inch -> inch
```

Then a carrier can be created with that unit shape:

```text
squareInchCarrier.UnitShape = inch -> inch
```

That carrier can later measure circles, triangles, or any other area-like
structure compatible with square inches. It is not permanently tied to the
rectangle or operation that first derived it.

Projecting a current operation value onto that carrier is optional and should
not be conflated with creating the unit shape. Projection feeds the crystal
carrier by delta, like other port-style value input; it does not directly
overwrite carrier state.

## 8. Current Code Foothold

In current Gojo code:

- `Carrier.UnitShape` is the native shape of one tick on that carrier.
- a default carrier gets a fresh linear atom, so two unrelated carriers are
  not unit-compatible by coincidence.
- carriers can be created with an existing `UnitShape`, giving them compatible
  ticks without sharing carrier identity.
- an inverse carrier shares the forward carrier's unit shape; inverse flow is
  not itself a new unit kind.
- `Relation.TryReadUnitShape(...)` calculates unit shape from existing
  relation surfaces:
  - same-carrier point/segment frames expose that carrier's unit shape
  - transfer/calibration ports expose the concrete target unit shape
  - `Unknown` output ports inherit the source unit shape for discovery reads
  - point-output operation surfaces fold operand shapes additively
  - segment-output operation surfaces fold operand shapes multiplicatively
- `Relation.TryCrystallize(out carrier)` creates a new carrier with the
  derived unit shape and does not project any current operation value onto it.
- `Relation.TryCrystallize(input, out carrier, out read)` creates the crystal
  carrier and projects the current operation read onto it.
- `Relation.TryProjectCrystallizedValueFrom(input, crystal, out read)` projects
  the current operation read onto an existing compatible crystal carrier.

The projection read preserves operand residue as visible relation-shaped
residue. It does not treat residue as fresh source tension merely because a
crystal read occurred.

The current value projection is intentionally conservative. Product-like
operation surfaces can project into one crystal scalar. Additive point-output
surfaces can project only when operand unit shapes are compatible. Incompatible
additive surfaces can still derive and crystallize an unreduced unit shape, but
their current value is not collapsed into one scalar until Gojo has a
multi-component crystal value surface.

## 9. Open Questions

The initial implementation can stay simple:

- multi-component crystal value surfaces for unreduced additive shapes
- richer compatibility reads for structured and inverse unit participation

Later work must clarify:

- inverse/dual unit participation in code
- compact scale-signature reads for fast scaling behavior
- deeper structured units such as radians, arc-length/radius, and geometric or
  dynamic units whose net scale signature is not the whole meaning
