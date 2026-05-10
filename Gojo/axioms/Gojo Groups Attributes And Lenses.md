# Gojo Groups, Attributes, And Lenses

This note captures the current working model for additive participation,
attribute attachment, low-resolution fill, and observer/read lenses. It is a
conceptual bridge between current `UnitShape`/`Relation` behavior and future
group, fill, and graphics-style structures.

## 1. Additive Participation

An additive relation has two natural outcomes depending on unit compatibility.

```text
same compatible unit shapes      calculated sum
different unit shapes            additive participation / group
different units cast to one unit  calculated sum at the shared unit
```

Examples:

```text
3 apples + 1 apple              -> 4 apples
3 apples + 2 oranges            -> apple/orange group
3 apples-as-fruit + 2 oranges   -> 5 fruit
```

This means a group does not need a separate symbolic mode. A group can be an
additive holder whose participants do not collapse to one unit shape. If later
structure projects them into a shared unit, the same additive relation can be
read as a sum at that shared type.

## 2. Group Holders

A group holder is a carrier or relation site that gathers participants around a
shared zero or junction. It may have no useful scalar value. Its job is to make
participation inspectable.

```text
group holder at 0
  + boundary segment A
  + boundary segment B
  + boundary segment C
  + boundary segment D
```

The holder can be durable structure, such as a named boundary group, or a
temporary selection created for inspection and discarded after use. Either way,
the grouping should be real Gojo structure, not an external list that holds the
only meaning.

Order, side, winding, adjacency, and selection membership are extra structure.
An unordered additive group and an ordered contour are not the same read, even
when they contain the same segments. Those distinctions should be carried by
relations among the participants or by the lens used to inspect them.

## 3. Boundary, Fill, And Low Resolution

A boundary group does not automatically contain fill. It exposes potential
uses. A fill exists when there is a region or interior participation carrier
attached to the boundary/group.

Area does not have to be calculated in order for a fill to exist. At low
resolution, fill may be only an interior marker:

```text
0 ticks   no filled interior at this resolution
1 tick    filled/interior participation
inverse   anti-fill, emptying pressure, exterior-side participation
```

This lets a region carry color, texture, hit participation, clipping, or other
attributes without first computing its exact area. Area is one possible read of
the fill. It is not the fill itself.

## 4. Attribute Carriers

Attributes are carriers attached by relation structure, not metadata fields.

```text
region/fill holder
  + color carrier
  + stroke/line-weight carrier
  + texture carrier
  + opacity carrier
```

These attachments are additive participation. Color does not multiply area to
become a new spatial fill. A region and a color participate together, and a
renderer or observer can later read that participation.

Attributes can themselves be built through carrier ladders:

```text
color carrier
  <- red component
  <- green component
  <- blue component

same color carrier
  <- hue
  <- saturation
  <- lightness

same color carrier
  <- palette index
```

RGB, HSL, palette index, and sampled texture are not hidden properties. They
are alternate relation paths into color participation. The color carrier is the
thing that can attach to a fill; the component ladders explain or produce it.

## 5. Lenses And Observer Reads

A lens is relation structure that reads a group, boundary, fill, or attribute
into some result carrier. A single boundary or region can expose many lenses:

```text
boundary group -> perimeter length lens
boundary group -> bounding box lens
fill region    -> area lens
fill region    -> hit/membership lens
fill region    -> centroid lens
fill region    -> render/tessellation lens
fill + mapping -> color sample lens
```

Each lens has a result unit shape and a mechanical read. The read mechanism may
eventually be exact, approximate, sampled, or unresolved. It should still be
encoded as Gojo-readable structure rather than as an enum value that only an
external interpreter understands.

Examples of future mechanics:

```text
bounding box      project boundary points onto axes, read min/max
perimeter length  traverse ordered segments and add compatible lengths
area              read fill through square-unit resolution or count samples
centroid          read balance of filled participation
hit area          read interior marker at chosen point/resolution
```

The lens result can be live, reading from the current source structure, or
crystallized as an independent carrier copy. Crystallization copies a read; it
does not make the source boundary or fill stop existing.

## 6. Metaphorical Attachment

Gojo should allow useful metaphorical units when the relation is explicit.

For example, line weight is not physical mass, but "heavy line" and "light
line" behave enough like weight to be useful. This should be represented by a
stroke-weight carrier attached to a line or contour, possibly with projection
relations to other weight-like structures.

The rule is not "forbid category mistakes." The rule is "make the projection or
attachment visible." A metaphor becomes meaningful when there is relation
structure that carries how the metaphor participates.

## 7. Compatibility With Current Code

Current Gojo code is already close in several places:

```text
UnitShape.Additive(...)   collapses compatible units; preserves unlike units
UnitShape.Product(...)    represents multiplicative/product participation
Relation point output     additive operation read
Relation segment output   multiplicative/product operation read
TryReadFillMeasureOn      deferred interval-fill measure
TryCrystallizeFill        independent interval-fill carrier copy
OrthogonalOutlet          generic source-to-orthogonal unit-shape foothold
AttachLiveOutput          live read into a concrete result carrier
```

The missing pieces are mostly semantic and inspection footholds:

```text
first-class additive group holder reads
public inspection of additive unit-shape operands
ordered contour/group structure for boundaries
low-resolution fill carriers that do not require area measurement
attribute carrier ladders such as RGB/HSL -> color
named lens structures for area, bbox, perimeter, hit, centroid, render
stable live result carriers with provenance
unit projection/cast structure for apples/oranges -> fruit-like reads
```

The current code should therefore be treated as compatible but incomplete. It
already has the local mechanics for unit compatibility, additive/product shape,
orthogonal surfaces, and crystallized reads. It does not yet make groups,
attributes, and lenses explicit enough to use them as ordinary inspectable
Gojo objects.
