# Boundary And Flow Codex

This note defines a small boundary/flow vocabulary for concept surfaces. The
goal is to separate two questions that are easy to collapse:

```text
boundary/domain:
  what kind of position surface is addressable?

flow exposure:
  what kind of calibrated tension/change route is present?
```

The two axes are independent. Each axis has two primitive sides and therefore
four combinations. Together they produce sixteen useful boundary/flow states.

This should be implemented as inspectable Gojo structure, not as a word enum.
The labels below are compact design names for local reads over boundary and
flow surfaces.

## Boundary Axis

The boundary axis answers:

```text
does this position surface have a boundary on the left,
on the right, on both sides, or on neither side?
```

Use two structural bits:

```text
left boundary present
right boundary present
```

The four combinations are:

| Label | Left boundary | Right boundary | Meaning |
| --- | --- | --- | --- |
| `PN` | false | false | no boundary on either side; full positive/negative position surface. |
| `P` | true | false | left-bounded positive surface; `uint`-like amount, radius, count, forward distance. |
| `N` | false | true | right-bounded negative surface; deficit, retreat, distance-from-goal, negative-only amount. |
| `S` | true | true | bounded whole segment; whole-instance surface, interval, percent/ratio object. |

The implementation bit order can be:

```text
(leftBoundary, rightBoundary)

FF -> PN
TF -> P
FT -> N
TT -> S
```

`S` means a bounded segment or whole. It is not "nothing." It is the domain
where whole-instance partitioning belongs.

### Boundary Meanings

`P` is the familiar positive-only domain:

```text
0..+
```

It is useful for count, length, radius, accumulated amount, fill level, or any
quantity that has one origin boundary and grows away from it.

`N` is the dual of `P`:

```text
-..0
```

It is useful for deficits, countdowns, retreat distance, remaining distance to
a goal, or negative-only accounting. It should not be treated as a hack around
ordinary positive numbers. It is a valid domain by itself.

`PN` is the ordinary full signed position surface:

```text
-..0..+
```

It has neither a left nor right boundary. Positions may be positive or
negative, but without flow exposure those are just positions. Segment overlap,
union, intersection, and geometry placement can live here without importing
ordinary value-flow arithmetic.

`S` is a bounded whole:

```text
left boundary .. right boundary
```

It is the home of whole-partition concepts: percent, interval, selected range,
valve opening, bridge span, finite field, bounded tolerance, and non-count
whole surfaces. A partition of `S` divides this whole instance; it is not a raw
unit count.

## Flow Axis

The flow axis answers:

```text
which calibrated flow/tension faces are exposed?
```

Use two structural bits:

```text
forward flow present
inverse flow present
```

The four combinations are:

| Label | Forward flow | Inverse flow | Meaning |
| --- | --- | --- | --- |
| `Ø` | false | false | no flow exposure; position, support, read-only, partition, overlap, or placement. |
| `F` | true | false | forward calibrated flow only. |
| `I` | false | true | inverse/opposed calibrated flow only. |
| `FI` | true | true | both calibrated flow faces explicitly present. |

The implementation bit order can be:

```text
(forwardFlow, inverseFlow)

FF -> Ø
TF -> F
FT -> I
TT -> FI
```

`Ø` flow does not mean the structure is meaningless. It means the structure is
not exposing calibrated tension/change direction. It may still expose
positions, partitions, ratios, trolleys, overlaps, boundaries, and reads.

## Why Boundary And Flow Are Separate

A position surface can be addressable without being a calibrated flow surface.
For example:

```text
football field geometry
interval overlap
rectangle coordinates
percent of an opening
selection range
```

These can use positions and partitions without claiming that a positive tick
routes tension in a particular direction.

Conversely, flow is a calibration or tension-route claim. It says how change
enters, leaves, routes, or opposes through the structure.

```text
boundary/domain = where positions can be
flow exposure   = how change/tension is calibrated
```

This distinction lets Gojo express:

- positive-only numbers
- negative-only numbers
- signed position lines
- bounded whole segments
- no-flow position math
- one-way flow
- inverse one-way flow
- explicit two-way flow

without collapsing them into one sign marker.

## Relationship To Carrier Faces

Current `Carrier` exposes a face through `IsInverse`. That means a current
carrier is close to:

```text
axis + one exposed flow face
```

The boundary/flow model suggests a lower-level split:

```text
axis / position surface
  identity, partitions, anchors, topology

flow face
  forward reservoir or inverse reservoir

flow exposure
  Ø, F, I, or FI
```

The `FI` case should usually be two connected faces, not one value collapsed
into a net scalar. If both forward and inverse participation exist at once,
Gojo should preserve opposed participation and tension until an explicit read
cancels, projects, or settles it.

Useful coordinate notation:

```text
 3   forward face, positive site
-3   forward face, negative site
 3i  inverse face, positive site
-3i  inverse face, negative site
```

The `i` notation is only a thinking aid. The graph should represent this by
carrier face plus signed carrier location.

## Boundary Plus Flow Matrix

The sixteen combinations are:

| State | Meaning | Example uses |
| --- | --- | --- |
| `PN:Ø` | full signed position surface with no flow exposure | geometry coordinates, interval union/intersection, graph placement, segment boolean operations. |
| `PN:F` | signed position surface with forward calibrated flow | directed displacement, affine read, time-like axis, one-drive direction over signed positions. |
| `PN:I` | signed position surface with inverse calibrated flow | mirrored axis, opposed drive basis, reverse read over signed positions. |
| `PN:FI` | signed surface with both flow faces exposed | reversible mechanics on signed coordinates, exchange/cancellation systems, two opposed drives. |
| `P:Ø` | left-bounded positive position with no flow exposure | static length, radius as placement magnitude, read-only count, nonnegative support extent. |
| `P:F` | positive-only domain with forward flow | `uint`, accumulated amount, odometer, fill level increasing, forward-only capacity. |
| `P:I` | positive-only domain with inverse flow | draining positive capacity, spending budget, reverse gear over a nonnegative amount. |
| `P:FI` | positive-only domain with both fill and drain | tank level, queue length, inventory, adjustable preload, nonnegative reservoir. |
| `N:Ø` | right-bounded negative position with no flow exposure | left-of-anchor placement, deficit position, back margin, countdown region as geometry. |
| `N:F` | negative-only domain with forward flow | recovering deficit toward zero, countdown progress, moving from behind toward goal. |
| `N:I` | negative-only domain with inverse flow | accumulating deficit, retreat, increasing distance from goal. |
| `N:FI` | negative-only domain with both routes | debt/error that can deepen or be paid down, backlog below target. |
| `S:Ø` | bounded whole with no flow exposure | percent, interval, bounded selected range, bridge span as a whole, football field geometry. |
| `S:F` | bounded whole with forward flow | one-way finite capacity, progress through a bounded span, valve opening read in one direction. |
| `S:I` | bounded whole with inverse flow | reverse finite capacity, countdown over a bounded span, drain/read from far end. |
| `S:FI` | bounded whole with both flow faces exposed | explicit two-way bridge, bidirectional slot/track, reversible conduit, two-lane exchange through a finite whole. |

## Low Resolution: Calibration Versus Partition

Low resolution can mean two different things:

```text
calibrated estimate
  measure in feet instead of inches
  one tick means a coarser calibrated unit

whole partition
  divide this particular whole into 3 or 100 parts
  one part means a fraction of this instance
```

This difference matters for concept surfaces.

Example:

```text
3 ticks
  three units of participation

3 partitions of S
  one bounded whole divided into three parts
```

If a bridge opening is represented by `3 ticks`, that is a count or amount. If
it is represented by `3 partitions`, it is a coarse control surface over the
whole opening, such as closed/partial/open or low/medium/high.

`S` is the natural domain for whole partitioning.

## Unspecified, None, And Open

Do not encode "unspecified" as a primitive separate from absence unless there
is structure that makes it visible.

The useful distinctions are:

```text
absent
  no boundary or flow surface is present

Ø
  the surface explicitly reads as no boundary/no flow for the axis in question

open
  an attachment site or placeholder exists for future boundary/flow structure

blocked
  no-flow or no-passage is witnessed by capacity, boundary, residue, or tension
```

For example:

```text
bridge with no directed flow read
  S:Ø or related no-flow passage read

bridge with two explicit directions
  S:FI

bridge with open flow site
  passage + open attachment/controller slot

bridge with blocked crossing
  passage + capacity/residue/tension witness
```

## Football Field Example

Football-field math is not a primitive boundary/flow state. It is a composed
structure over the primitives.

Base field:

```text
field whole:
  S:Ø
  bounded whole partitioned into yard marks
```

The field itself has two endpoints and partitions, but no single intrinsic
flow. It is a bounded whole used for position and partition.

Team reads:

```text
team A read:
  origin = A goal line
  positive flow toward B goal line

team B read:
  origin = B goal line
  positive flow toward A goal line
```

These are two opposed calibration reads over the same bounded whole:

```text
ARead = Segment(field, AGoal, BGoal) -> Segment(AProgress, 0, 100)
BRead = Segment(field, BGoal, AGoal) -> Segment(BProgress, 0, 100)
```

Possession is a controller that selects the active read:

```text
Possession
  selected team read
  ball position
  predicted progress / yards-to-goal
  residue if unresolved
```

The same physical ball position can read differently:

```text
ball at one point on field
  through ARead -> A progress
  through BRead -> B progress
```

So the implementation should not force football fields into primitive `S:FI`.
The cleaner structure is:

```text
S:Ø bounded whole
+ two opposed calibration reads
+ possession/controller surface
```

## Implementation Plan

This section is a plan, not a code change.

### 1. Add Compact Value Types Or Helpers

The boundary axis can be represented by two booleans:

```csharp
public readonly record struct BoundaryExposure(
    bool LeftBoundary,
    bool RightBoundary);
```

The flow axis can also be represented by two booleans:

```csharp
public readonly record struct FlowExposure(
    bool ForwardFlow,
    bool InverseFlow);
```

These should be helper/read values, not the meaning-owning source. The source
should be relation structure.

Useful derived reads:

```csharp
BoundaryExposure.Label  // PN, P, N, S
FlowExposure.Label      // Ø, F, I, FI
```

### 2. Add Relation Builders

Add builders under `Gojo/ConceptSurface` first:

```csharp
BoundarySurface.Create(root, leftBoundary, rightBoundary, wholeOrAxis)
FlowSurface.Create(root, forwardFlow, inverseFlow, selected)
BoundaryFlowSurface.Create(root, boundarySurface, flowSurface)
```

These should return ordinary `Relation` groups.

### 3. Add Local Readers

Readers should inspect the relation structure and return helper values:

```csharp
BoundarySurface.TryRead(surface, out BoundaryExposure exposure, out IElement wholeOrAxis)
FlowSurface.TryRead(surface, out FlowExposure exposure, out IElement selected)
BoundaryFlowSurface.TryRead(surface, out BoundaryExposure boundary, out FlowExposure flow)
```

If the structure is absent, the reader can fail. If the structure explicitly
stores `false,false`, the reader returns `PN` for boundary or `Ø` for flow.

### 4. Keep Open Separate

Open/unresolved flow should not be encoded as a fifth flow bit. Use existing
open structure:

```text
Placeholder
ConceptAttachment.OpenOutlet
ConceptAttachment.OpenInlet
ConceptGate
ConceptCapacity
```

This preserves:

```text
Ø flow
  explicitly no exposed flow

open flow
  unresolved future flow site
```

### 5. Preserve Carrier Compatibility

Do not immediately rewrite `Carrier.IsInverse`.

First pass:

- represent boundary/flow states in ConceptSurface helpers
- continue to use current `Carrier` and `carrier.Inverse()`
- treat `FI` as a surface containing both faces or both routes
- treat `Ø` flow as no exposed flow relation

Later core exploration:

```text
split carrier axis from carrier face
```

Current `Carrier` can remain the default forward face until that deeper split
is justified by tests.

### 6. Add Tests Around Structural Reads

Tests should prove:

- `BoundaryExposure(false,false)` reads as `PN`
- `BoundaryExposure(true,false)` reads as `P`
- `BoundaryExposure(false,true)` reads as `N`
- `BoundaryExposure(true,true)` reads as `S`
- `FlowExposure(false,false)` reads as `Ø`
- `FlowExposure(true,false)` reads as `F`
- `FlowExposure(false,true)` reads as `I`
- `FlowExposure(true,true)` reads as `FI`
- `S:Ø` can be partitioned as a bounded whole
- `S:FI` contains explicit forward and inverse flow surfaces
- football-field math is built as `S:Ø` plus two opposed reads and a controller

### 7. Avoid Word Classification

Do not implement:

```csharp
IsBridge()
IsFunnel()
IsFootballField()
```

Instead, expose local reads:

```csharp
TryReadBoundaryExposure(...)
TryReadFlowExposure(...)
TryReadPartition(...)
TryReadReadFocus(...)
TryReadGate(...)
```

Words can remain documentation-level interpretations over those structures.

## Summary

The proposed primitive system is:

```text
Boundary:
  P, N, PN, S
  encoded by left/right boundary presence

Flow:
  F, I, FI, Ø
  encoded by forward/inverse flow exposure

Combined:
  16 boundary/flow states
```

The two axes should remain independent. Boundary describes the position domain.
Flow describes calibrated tension/change exposure. More advanced concepts such
as bridge traffic, football fields, filters, valves, and gauges are composed
structures built from these primitives plus reads, partitions, gates,
controllers, residue, and timelines.
