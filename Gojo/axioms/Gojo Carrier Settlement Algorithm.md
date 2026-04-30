# Gojo Carrier Settlement Algorithm

This note writes the current carrier-tension settlement loop as pseudo code.
The goal is not to create a second interpreter. The goal is to see which parts
still exist only because the host number system is weaker than the Gojo number
system we are building.

In an ideal Gojo-native runtime, adding tension to a carrier would already know
how to account through carrier-local boundaries, dual faces, relation order,
and routed tension. The pseudo code below is the current executable shadow of
that behavior.

## 1. Carrier Input

All input enters a carrier as signed tension.

```text
receive(carrier C, signed input):
    C.Tension += input
    absorb-local-tension(C)
    settle-connected-world(C)
```

`input` may come from a user action, a previous unresolved frame, a route from
another carrier, a future sensor, or a trolley-like traversal. The source does
not change the first step.

Direct value assignment is not a primitive operation. A request to move a
carrier from one visible value to another is represented as a perturbation:

```text
request-value(C, desired):
    receive(C, desired - C.Value)
```

A stronger future command such as "make this carrier read 3" should likewise
act by adding/removing tension, adjusting relations, or repeating perturbation
until the structure stabilizes. It should not bypass carrier accounting by
writing value directly.

## 1A. Downward Delta Normalization Target

The current executable pass mostly settles tension upward through return/carry
relations. The expected next extension is a downward normalization pass for
ratio-coupled layers.

The guiding idea is:

```text
stable values do not create tension merely by disagreeing
changes create tension
upper-layer tension can move down through inverse ratio relations
only representable quanta move; the rest remains tension
the lowest reachable non-crystallized layer absorbs what it can
ordinary upward settlement then re-expresses the changed base value
```

For example:

```text
P(C0,2) -> P(C1,3)
C1 receives +1 tension
```

Moving that tension downward through the inverse `3:2` relation may move `2`
ticks toward `C0` while leaving `1` tick of unresolved tension at `C1`. This is
not yet proven, but it is the first candidate rule. A resolution/crystal layer
may later account for smaller quanta.

This is different from ratio-mismatch tension. The relation does not wake up
because `C0` and `C1` have different stable values. It wakes because new
tension appeared somewhere in the connected structure.

## 2. Local Absorption

A primitive carrier face stores positive accounted value. Its signed tension is
unaccounted pressure.

```text
absorb-local-tension(C):
    if C.Tension > 0:
        C.Value += C.Tension
        C.Tension = 0
        return

    if C.Tension < 0:
        draw = abs(C.Tension)

        if C.Value >= draw:
            C.Value -= draw
            C.Tension = 0
            return

        C.Tension = -(draw - C.Value)
        C.Value = 0
```

This step says only what an unconstrained carrier can account for by itself.
It does not know about boundaries yet.

## 3. World Settlement

After local absorption, the connected relation world repeatedly applies its
relation shapes until a pass changes nothing.

```text
settle-connected-world(start):
    world = all carriers connected to start by attached relations

    repeat until quiet:
        snapshot = carrier states in world

        for each attached relation R in world:
            apply-relation(R)

        if carrier states equal snapshot:
            stop

        world = all carriers connected to start by attached relations
```

This is currently a quiet loop. Later dynamic versions may distribute work over
time, but the underlying motion is the same: carrier state changes wake related
structure, and unresolved participation remains as tension.

## 4. Relation Application

A relation is applied by reading the carrier sites visible on its left and
right sides.

```text
apply-relation(R = A -> B):
    leftSites  = carrier locations inside A
    rightSites = carrier locations inside B

    if either side contains no carrier sites:
        no action

    if either side spans more than one carrier:
        no action in the current pass

    if left and right are on the same carrier face:
        apply-local-relation(left, right)

    else if left and right are on opposed carrier faces
         and right is origin point:
        cancel-visible-opposed-values(left.carrier, right.carrier)
        route-negative-tension(left, right)

    else if right is origin point:
        route-positive-tension(left, right)
```

This is still deliberately narrow. Multi-carrier surfaces, segment-to-segment
stretch, fanout, merge, support filtering, and crystallized reads should be
added by strengthening this same shape read, not by introducing unrelated
route objects.

## 5. Local Same-Carrier Relations

Same-carrier local behavior is directional.

```text
apply-local-relation(left, right):
    source = greatest left-side site
    target = smallest right-side site

    if source == 0 and target > 0:
        no mutation; this is a forward projection/basis surface
        return

    apply-local-accounting(C, source, target)
```

Directional local accounting:

```text
apply-local-accounting(C, source, target):
    if source == 0:
        no action

    if source < target:
        no action in the current pass

    if C.Value <= target:
        no action

    if source == target:
        local = min(C.Value, source)
    else:
        width = source - target
        local = target + ((C.Value - target) mod width)

    excess = C.Value - local
    C.Value = local
    C.Tension += excess
```

Concrete same-carrier reads:

```text
L(C,0)  -> L(C,k)  = forward projection/basis surface; no mutation by itself
L(C,k)  -> L(C,0)  = return/wrap to origin
L(C,k)  -> L(C,k)  = stop/clamp at k
P(C,k)  -> P(C,0)  = carrier-bound return/wrap
P(C,k)  -> P(C,k)  = carrier-bound stop/clamp
S(C,a,b) where a < b = forward ordered extent, not point carry by itself
S(C,a,b) where a > b = backward ordered extent, not dual face by itself
```

The important correction is that wrap/reset comes from relation order, not from
whether the sites were written as points instead of locations.

## 6. Positive Tension Routing

Same-oriented origin receivers can absorb positive source tension through the
source unit. A nonzero receiver site is a projection/basis relation in the
current executable pass, not a material carry route.

```text
route-positive-tension(source, receiver):
    if receiver.site != 0:
        no mutation

    unit = source.width if source has width else source.maxSite

    if unit == 0:
        no action

    if source is a local boundary:
        apply-local-accounting(source.carrier, source.maxSite, source.minSite)

    if source.carrier.Tension <= 0:
        no action

    received = source.carrier.Tension / unit
    remainder = source.carrier.Tension mod unit

    if received == 0:
        no action

    source.carrier.Tension = remainder
    receive(receiver.carrier, received)
```

Example:

```text
P(C0,10) -> P(C1,0)
C0 receives 37

C0 local accounting: Value 7, Tension 30
route by 10: C1 receives 3, C0.Tension = 0
```

By contrast:

```text
P(C0,2) -> P(C1,3)
C0 receives 6

C0 remains Value 6, Tension 0
C1 remains Value 0, Tension 0
```

That relation says that `2` ticks on `C0` project as `3` ticks on `C1`.
The projection read is not yet executable as a crystallized carrier value, so
the relation does not mutate either carrier.

## 7. Negative Tension Routing

Opposed origin receivers can absorb negative source tension as positive value
on their own face.

```text
route-negative-tension(source, receiver):
    if receiver.site != 0:
        no mutation

    unit = source.width if source has width else source.maxSite

    if unit == 0:
        no action

    if source.carrier.Tension >= 0:
        no action

    available = abs(source.carrier.Tension)
    received = available / unit
    remainder = available mod unit

    if received == 0:
        no action

    source.carrier.Tension = -remainder
    receive(receiver.carrier, received)
```

Example:

```text
P(C0,10) -> P(C1*,0)
C0 receives -23

C0 local absorption: Value 0, Tension -23
route by 10: C1* receives 2, C0.Tension = -3
```

The receiver does not receive a negative value. It receives positive
participation on the opposed face.

## 8. Opposed Visible Value Cancellation

When related opposed carriers both hold visible value, their shared amount
cancels before residue remains.

```text
cancel-visible-opposed-values(A, B):
    shared = min(A.Value, B.Value)

    A.Value -= shared
    B.Value -= shared
```

This is the current foothold for signed-axis assemblies. It is not yet full
crystallization.

## 9. Current Missing Rows

The current algorithm intentionally leaves these rows unresolved or partial:

- crystallized ratio read for `P(C0,k) -> P(C1,m)` where `m > 0`
- `source < target` same-carrier relations
- relation-to-relation stretch where both sides preserve extent
- segment-to-point endpoint movement
- segment-to-segment basis projection
- refill/borrow from a higher carrier back into a lower carrier
- multi-carrier receiver surfaces
- branch, merge, observation, support, and filter routes
- crystallized reads that expose one tick interface over many carriers
- carrierless floating relation expressions

These are not reasons to add operation tags. They are the next places where the
same carrier-local accounting, relation order, duality, reversal, and tension
routing need to become more innate.

## 10. Desired Direction

The pseudo code should shrink over time.

The target is not a larger dispatcher. The target is that the carrier/relation
number system itself makes these operations natural:

```text
apply input tension
carrier absorbs what its structure can explain
unexplained participation remains as signed tension
relations expose compatible receivers
receivers absorb through their own structure
repeat until quiet or until remaining tension has no route
```

When the algorithm needs a new named branch, first ask what numeric relation
shape would make that branch physically visible.
