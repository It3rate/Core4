# Gojo Structural Transform Relations

This note records the current Gojo transform pass after the cut away from
externally named route/read shapes.

The executable rule is intentionally small:

```text
structure = carrier-state surface -> relation surface
settled   = add incoming participation, then repeatedly apply relation surface
```

The pass does not ask for a point-transform object, segment-transform object,
route object, boundary event, or operation tag. It walks relation structure,
finds carrier sites, and lets the related sites determine what a carrier can
account for and where tension can flow.

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

The current C# code uses `Carrier` and `CarrierLocation` as the concrete
substrate. Carrierless expressions such as `2 -> 5` are still conceptual: they
represent a floating relation shape that could later be grounded on a carrier,
but Gojo does not yet have a bare numeric element for them.

## 2. Carrier State

Each carrier instance can hold:

```text
Value   = positive participation currently accounted on that carrier
Tension = positive participation the carrier could not locally account for
```

The carrier line is still identified by `Id`, `Lineage`, and `Flow`. Full
carrier equality includes `Value` and `Tension`, because two carriers with the
same line and different held state are different carrier instances in the
settled surface.

Carrier value and tension are not points on the carrier line. They belong to
the carrier instance. Relation structure such as `P(C,10)` remains the
carrier-local shape that tells the settlement pass what can be accounted.

## 3. Local Accounting

When a relation has carrier sites on one carrier on both sides, it is read as a
carrier-local accounting relation.

```text
P(C,k) -> P(C,0)
```

This is a reset boundary. If `23` enters `C` with `k = 10`, the carrier settles
to:

```text
C.Value   = 3
C.Tension = 20
```

The value `23` no longer remains as the carrier value. The carrier has
hardened what it can locally hold, and the crossing amount is visible tension.

```text
P(C,k) -> P(C,k)
```

This is a clamp boundary. If `23` enters with `k = 10`, the carrier settles to:

```text
C.Value   = 10
C.Tension = 13
```

Both forms are ordinary same-carrier relations. The pass does not store a
boundary kind.

## 4. Tension Routing

When a cross-carrier relation ends at a point-like receiver, the source side can
route carrier tension into the receiver carrier.

Compact carry:

```text
P(C0,10) -> P(C1,0)
```

Adding `37` to `C0` settles as:

```text
C0.Value   = 7
C0.Tension = 0
C1.Value   = 3
```

The compact form is treated as local source accounting followed by routing
through the source unit.

Explicit carry:

```text
(P(C0,10) -> P(C0,0)) -> P(C1,0)
```

This exposes the local boundary as a relation in its own right. If the outer
route is absent, the tension remains on `C0`. If the route is present now or in
a later frame, compatible tension can be absorbed by `C1`.

The routed amount is quotient-like, but the underlying participation is not
destroyed. The source tension is read through the source boundary unit, and any
remainder stays as source tension.

## 5. Fresh Extent Projection

When a cross-carrier relation has extent on both sides, it projects fresh
participation by the ratio of the two extents.

```text
S(C0,0,1) -> S(C1,0,5)
```

Adding `4` fresh ticks to `C0` creates `20` fresh ticks on `C1`.

The source carrier is not drained by extent projection. This is stretch/copy
behavior, not boundary accounting. A value created by extent projection can
feed later relations in the same settle frame, but in the next frame it is just
settled carrier state and does not stretch itself again.

This is the current foothold for multiplication-like structure. Powers and
basis scaling should come from explicit repeated relation structure, not from
hidden scale provenance.

## 6. Settlement Loop

`Gojo.Reads.StructuralTransformReads.AddAndSettle` currently does this:

1. Read the left side of `structure` into an ephemeral carrier-state surface.
2. Add incoming participation as fresh frame input.
3. Collect relation nodes from the right side of `structure`.
4. Repeatedly apply the relation nodes until carrier values and tensions stop
   changing.
5. Return a new structure whose left side is carrier instances with held state
   and whose right side is the original relation surface.

The in-memory settlement surface is accounting machinery for one pass. It is
not intended to be a second ontology or a persistent explanation graph.

## 7. What Was Removed

The previous pass had named helpers for transfer, observation, open gather,
boundary routes, residual settlement, and several specific transform reads.
Those were useful scaffolding, but they kept too much meaning outside the
relation structure.

After this cut, those behaviors should be reintroduced only when their numeric
relation shape is clear. Do not rebuild them as route enums, wrapper classes,
or method-specific meanings.

Current executable behavior is limited to:

- unbounded carrier accumulation
- same-carrier reset/clamp accounting
- cross-carrier point tension routing
- explicit boundary relation routing
- fresh extent-to-extent projection
- chained fresh projection inside one settle frame

Known gaps:

- underflow and refill
- branch and merge negotiation
- material split versus observation fanout
- opposed loops and cycle draining
- carrierless floating expressions
- structural route hardening beyond the simple point receiver

These are not contradictions. They are intentionally left out until the same
small carrier/relation/tension mechanism can express them.

## 8. Guardrail

Carriers hold positive participation and positive unresolved tension.

Relations bind the meaning of carrier sites.

If a behavior cannot be discovered from carrier relation structure, it does not
belong in the kernel yet.
