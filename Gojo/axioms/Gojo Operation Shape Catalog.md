# Gojo Operation Shape Catalog

This note gives a compact notation for operation-like behavior that falls out
of relation shape. It is not a symbolic operator table. It is a checklist of
structural affordances we have seen or expect to see.

Notation:

- `P(C,n)` means a point: `(C.n -> C)`
- `S(C,a,b)` means a segment: `P(C,a) -> P(C,b)`
- `S0.P0` and `S0.P1` mean the first and second point of segment `S0`
- `A -> B` means an ordinary `Relation(A, B)`
- `^n` means repeated relation shape, not a primitive exponent operator
- "read layer" means the carrier layer inspected after the structure settles

## Basic Shapes

| Shape | Read |
|---|---|
| `P(C0,0) -> P(C1,0)` | origin attachment / shared reference; no automatic accumulation |
| `P(C0,1) -> P(C1,0)` | one-for-one accumulator; every tick crosses upward |
| `P(C0,k) -> P(C1,0)` | base/counter carry; lower keeps `value % k`, upper receives `value / k` |
| `P(C0,k) -> P(C1,m)` | offset-point candidate; current internal settling treats offset as existing `C1` position rather than consuming `m` from this endpoint |
| `P(C0,k) -> P(C0,0)` | same-carrier reset/counter shape; needs point identity before it is fully safe |

## Segment And Scaling Shapes

| Shape | Read |
|---|---|
| `S(C0,0,1) -> S(C1,0,1)` | segment-to-segment stretch/projection; also a unit duplicate/copy basis shape |
| `S(C0,0,k) -> S(C1,0,m)` | ratio stretch from one basis segment to another |
| `S0 -> S1.P1` | multiplication-like basis scaling; source segment flow moves the endpoint of `S1` while `S1.P0 -> S1.P1` remains intact |
| `S0 -> P(C1,n)` | move a point from source segment flow; if that point is an endpoint of a segment, the segment stretches |
| `S(C,5,5)` | zero-length segment; point-like now, but still two related points that can later separate |

## Repetition And Powers

| Shape | Read |
|---|---|
| `(P(C0,1) -> P(C1,0))^n` | additive repetition; every tick crosses each point layer without scaling the amount |
| `(P(C0,k) -> P(C1,0))^n` | repeated base/counter carry; positional positive integer stack with `n + 1` carrier layers |
| `(P(C0,10) -> P(C1,0))^7` | eight decimal digit carrier stack, such as a fixed-width positive calculator read |
| `(S0 -> S1.P1)^2` | square-like repeated basis scaling |
| `(S0 -> S1.P1)^3` | cube-like repeated basis scaling |
| `S0 -> S1`, `S1 -> S2`, `S2 -> S3` | repeated segment relation; power-like behavior when each layer reads the previous stretched basis |

The point-chain and segment-chain repeats are both "powers" in the loose
structural sense that the same relation shape is repeated. They are not the
same operation. Repeated point layers preserve count through carry/reset.
Repeated segment-to-point or segment-to-segment layers alter basis length and
therefore create multiplication-like or power-like reads.

## Residue, Modulo, And Shift Shapes

| Shape | Read |
|---|---|
| `P(C0,k) -> P(C1,0)`, then read `C0` | modulo/residue read; lower carrier reports the settled `value % k` layer |
| `P(C0,k) -> P(C1,0)`, then read final carried layer | distributed quotient/counter read |
| current `P(C1,n)` plus `P(C0,1) -> P(C1,0)` | additive shift; incoming ticks accumulate from an offset carrier position |
| `S(C,a,b) -> S(C,a+n,b+n)` | segment shift/translation candidate; endpoints move together while segment length is preserved |
| `S0 -> S1`, then read `S1` | duplicate/projection read of one basis through another |

The current executable settling already supports modulo by reading the lower
carry layer after settlement, and offset-style addition when the receiving
carrier already carries the offset as local position. General same-carrier
segment shift still needs point identity so coincident points can move without
accidentally moving every matching point in the surface.

## Routing-Like Shapes

| Shape | Read |
|---|---|
| `P(C0,k) -> P(counter,0)` | crossing counter |
| `P(C0,k) -> P(C0_inverse,0)` | opposed/refill or reflection foothold; forward crossing settles as positive ticks on the opposed carrier |
| `P(C0,k) -> P(C0_inverse,0)` plus `P(C0_inverse,k) -> P(C0,0)` | opposed return loop/ping-pong candidate; lossy or thresholded return can settle without signed storage |
| `P(C,k) -> P(C,0)` | same-oriented cycle/reset candidate; needs point/site identity before it should be treated as modulo or restart |
| `S(C,a,b).P1 -> S(C,a,b).P0` | segment end-to-start cycle candidate; restart without traversing a return path |
| `S(C,a,b).P1 -> S(C_inverse,0)` | loop/ping-pong candidate; flow traverses an opposed return path before it can come forward again |
| `S0 -> (P(C1,n) -> P(C2,m))` | segment flow into a relation surface; possible coupled fanout/general receiver surface |
| `(source relation) -> (open 0/0 site)` | unresolved gather / attachment potential |

Cycling and looping are intentionally separated here. A cycle restarts at a
related origin or point. A loop has a return journey, usually through an
opposed carrier or explicit return relation, so the return path can carry its
own tension, residue, or blockage.

Same-carrier cycles are not yet executable as true cycles. A relation such as
`P(C,k) -> P(C,0)` currently cannot distinguish residue storage from the return
site on the same carrier. Treating it as modulo would hide the missing
site/point identity. Using `C_inverse` gives a structural return path that can
be inspected now.

Lossless point loops are allowed as structures, but settling stops when a full
pass returns to the same carrier positions. The remaining load is visible
participation for a later frame or a different relation surface, not an error.

## Boolean And Filter Footholds

These are not yet structural-settle operations, but the expected shapes are:

| Shape | Read |
|---|---|
| `travel segment -> support segment` | pass where spans overlap |
| `travel segment -> gap in support surface` | block / unpassed tension |
| `support A + support B` | support union/readable coverage |
| `support A opposed support B` | tension/shared opposed participation |

## Possible But Not Fully Listed Yet

The same notation should extend to:

- relation-to-relation transforms where both sides are multi-carrier structures
- segment-to-surface fanout
- point-to-surface carry into several related carrier points
- looped point carry, modulo, and periodic counters
- inverse point and segment relations for subtraction/division/refill
- orthogonal relation surfaces for area, complex rays, and multi-axis support
- filter surfaces that split travel into passed and blocked spans

The guardrail is simple: if a behavior needs a new action name, first try to
express it as points, segments, and relation surfaces whose points move while
their relations are preserved.
