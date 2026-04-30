# Gojo Duality And Relation Settlement Table

This note names the current distinction between orientation duality, traversal
reversal, and contextual inversion. It also starts the table we need for the
single carrier/relation settlement algorithm.

## 1. Three Moves, Not One Flag

Gojo should not hide sign, inverse flow, subtraction, division, or reverse
travel in separate flags.

The smaller structural moves are:

```text
Dual(element)      = same magnitude or shape on the opposed orientation face
Reverse(relation)  = same relation surface traversed in the opposite order
Invert(read/use)   = contextual use of Dual, Reverse, or both
```

`Dual` is part of the `IElement` contract. Each element constructs its own
dual; no outside helper owns that meaning. `Reverse` is ordered and should
always be constructible for a relation. A contextual algebraic inverse is not
guaranteed to settle cleanly. It may leave tension.

This keeps `-5` from being a primitive host-language sign:

- value claim: `5` on the dual face
- operation: a transform slot read through its dual face
- trolley movement: reverse traversal by magnitude `5`
- measurement: a carrier-local read whose origin/basis decides whether the
  opposite site is a dual face, a backward same-carrier segment, or unresolved
  tension

Those are perspectives over the same small structure, not separate operator
taxonomies.

## 2. Element Duality Table

| Element | Dual | Reverse | Notes |
|---|---|---|---|
| `C` | `C*` | none | Opposed carrier face with the same unit claim and opposite native flow. |
| `L(C,k)` | `L(C*,k)` | none | Magnitude stays positive; opposition comes from the carrier face. |
| `P(C,k) = L(C,k) -> C` | `P(C*,k)` | `C -> L(C,k)` | Point dual is useful. Point reverse is constructible but not usually a local point. |
| `S(C,a,b) = P(C,a) -> P(C,b)` | `S(C*,a,b)` | `S(C,b,a)` | Dual changes face; reverse changes traversal/order. |
| `A -> B` | `Dual(A) -> Dual(B)` | `B -> A` | These commute structurally, but a read through them may settle differently. |

`C*` means the stable dual carrier returned by `C.Dual()`. It is a physical
carrier face, not a negative flag on `C`.

## 3. Site Magnitude Table

For point-like source shapes:

| Source | Meaning |
|---|---|
| `P(C,0)` | origin/reference site; attachment or zero-site read, no material unit by itself |
| `P(C,1)` | one tick boundary; one-for-one carry/accumulation when routed |
| `P(C,k)` | grouped boundary; local accounting emits signed tension in groups of `k` |

For segment-like source shapes:

| Source | Meaning |
|---|---|
| `S(C,a,a)` | zero extent; point-like now, but still two coincident points that may separate later |
| `S(C,0,1)` | unit basis segment |
| `S(C,a,b)` | ordered extent; `a < b` follows native carrier order, `a > b` traverses backward on the same face |

A backward same-carrier segment is not the same thing as a dual carrier. It is
reverse traversal inside one carrier face.

## 4. Current Settlement Table

This table names what Gojo currently executes and what is still only a target.

| Shape | Current settlement |
|---|---|
| bare `C` | input tension is absorbed into `C.Value`; no boundary tension remains |
| `L(C,0) -> L(C,k)` | local capacity: `Value > k` hardens to `k` and leaves positive `Tension` |
| `P(C,k) -> P(C,0)` | reset/carry boundary: value hardens to residue and grouped excess remains as positive `Tension` |
| `P(C,k) -> P(C,k)` | clamp boundary: value hardens to `k` and excess remains as positive `Tension` |
| `P(C0,k) -> P(C1,0)` | same-orientation route: positive source tension routes into `C1` by `k:1` |
| `P(C0,k) -> P(C1*,0)` | opposed route: negative source tension routes into the dual face by `k:1` |
| related opposed visible values | shared visible value cancels before residue remains |

The current code treats `P(C0,k) -> P(C1,m)` mostly like the `m = 0` route.
The meaning of nonzero receiver offset still needs to be made structural
instead of quietly ignored.

## 5. Target Relation Shape Table

The likely general algorithm should not ask "which operation is this?" It
should inspect the relation shape:

| Left shape | Right shape | Same face | Different same-oriented face | Opposed face |
|---|---|---|---|---|
| point | point | local boundary/reset/clamp depending on right site | route positive tension by source unit | route negative tension by source unit |
| point | segment | move or seed receiver endpoint if the segment exposes that point | shift/stretch target surface from a site | opposed shift/refill target surface |
| segment | point | source extent moves receiver point; if receiver point anchors a segment, that segment scales | multiplication-like endpoint movement | division/refill-like endpoint movement, possibly tension |
| segment | segment | preserve local extent relation; do not collapse to point carry | basis projection, copy, ratio scale | inverse basis projection, division/root-like read |
| relation surface | relation surface | internal structure preserved while sites move | coupled fanout/merge by visible sites | opposed fanout/merge with tension if not reversible |

The target behavior should fall out from:

- carrier-local accounting capacity
- signed carrier tension
- relation order
- site duality
- traversal reversal
- preservation of relation structure while points move

If a row needs a named helper forever, the shape is probably not primitive
enough yet.

## 6. Crystallized Reads

A crystallized carrier is a local interface over a settled relation assembly.
It may expose a single usable tick while hiding lower construction from local
control.

Examples:

- positive and dual faces crystallize into a signed-axis read
- ones/tens/hundreds crystallize into an integer read
- whole plus partition layers crystallize into a mixed-resolution read
- length and width can crystallize into an area carrier
- distance and time can crystallize into a speed carrier

Crystallization should be one general operation: if a relation assembly has a
stable repeatable unit, it can expose that unit as a new carrier tick. The
lower assembly remains provenance and may still affect settlement, but the
local crystal read does not inspect every lower carrier by hand.
