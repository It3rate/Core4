# Gojo Structural Transform Relations

Gojo settlement now lives on the physical carrier/relation world.

There is no structural transform reader, contribution surface, route table, or
external balancing surface in the current core. A carrier owns value, signed
tension, and the relations attached to its local world. When carrier tension or
value changes, the connected relation world settles from inside that world.

## 1. Primitive Shapes

Gojo still treats these as relation shapes rather than wrapper types:

```text
point   P(C,k)   = L(C,k) -> C
segment S(C,a,b) = P(C,a) -> P(C,b)
link             = element -> element
```

A point is a carrier location related to its carrier. A segment is two points
related on one carrier. A transform is just another relation whose sides may be
points, segments, or larger relation surfaces.

Carrierless expressions such as `2 -> 5` remain conceptual for now. They are
floating relation shapes that can later be grounded on a carrier.

## 2. Carrier State

Each carrier instance can hold:

```text
Id        = unique physical identity; ordinary construction always allocates it
IsInverse = local orientation marker, not a pairing rule by itself
Value     = positive participation currently accounted on that carrier
Tension   = signed participation the carrier has not locally accounted for
```

`Carrier.Unknown` is the one carrier with `Id = 0`. User-created carriers do
not receive caller-supplied IDs. Two structurally identical carriers with
different IDs are physically different carriers.

Carrier value and tension are not points on the carrier line. They belong to
the carrier instance. Relation structure such as `P(C,10)` tells the carrier
what it can account for.

Input enters a carrier as tension. The carrier absorbs what it can into
`Value`. Any positive residue remains positive tension. Any below-origin
residue remains negative tension. Relations may then route that tension into
other carriers.

## 3. Local Accounting

A bare carrier span from origin to a point is local capacity:

```text
L(C,0) -> L(C,10)
```

If `C.Value` is set to `11`, the carrier settles to:

```text
C.Value    = 10
C.Tension  = 1
```

A point reset relation is grouped carry:

```text
P(C,10) -> P(C,0)
```

If `23` enters `C`, the carrier settles to:

```text
C.Value    = 3
C.Tension  = 20
```

A same-site point relation clamps:

```text
P(C,10) -> P(C,10)
```

If `23` enters `C`, the carrier settles to:

```text
C.Value    = 10
C.Tension  = 13
```

These are ordinary relation shapes attached to the carrier. There is no stored
boundary kind.

## 4. Routing

When a cross-carrier relation ends at a point-like receiver, the source side can
route positive carrier tension into the receiver carrier:

```text
P(C0,10) -> P(C1,0)
```

Adding `37` to `C0` settles as:

```text
C0.Value    = 7
C0.Tension  = 0
C1.Value    = 3
```

The compact form is local source accounting followed by routing through the
source unit.

The explicit boundary route is also valid:

```text
(P(C0,10) -> P(C0,0)) -> P(C1,0)
```

If the boundary exists first, positive tension can remain on `C0`. When the
outer route is later attached, that existing tension can be absorbed by `C1`.

Negative tension mirrors positive tension. If a source carrier has below-origin
tension and is related to an inverse-oriented point receiver, the receiver can
absorb that tension:

```text
P(C0,10) -> P(C1_inverse,0)
```

If `C0.Tension = -23`, this relation settles as:

```text
C0.Tension       = -3
C1_inverse.Value = 2
```

An inverse carrier does not absorb anything merely because `IsInverse = true`.
The relation makes the underflow route active.

## 5. Opposed Values

When opposed carriers are related, visible values cancel through that relation
before residue remains on either side:

```text
P(C0,1) -> P(C1_inverse,0)
```

If `C0.Value = 5` and `C1_inverse` receives `7`, settlement leaves:

```text
C0.Value         = 0
C1_inverse.Value = 2
```

This is the first executable foothold for signed-capable carrier assemblies
without signed carrier storage.

## 6. Removed Surface Machinery

The old `ContributionSurface`, `ContributionReads`, and
`StructuralTransformReads` files have been removed. They were useful while the
model was still immutable, but they kept too much meaning in an external
machine that inspected structure and decided what it meant.

The current rule is simpler:

```text
carrier.Attach(relation)
carrier.SetValue / Add / Remove
carrier settles its connected relation world
```

Known gaps:

- extent/stretch projection needs to be reintroduced internally
- refill and borrow
- branch and merge negotiation
- conserved split versus observation fanout
- opposed loop and cycle behavior
- carrierless floating relation expressions
- support/filter routing through settlement rather than separate boolean logic

These are not contradictions. They are intentionally left out until the same
small carrier/relation/overflow/underflow mechanism can express them from
inside the connected world.

## 7. Guardrail

Carriers hold positive accounted participation and signed unresolved tension.

Relations bind the meaning of carrier sites.

If a behavior needs an external reader to decide what structure means, it does
not belong in the kernel yet.
