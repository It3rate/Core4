# Gojo Operation Shape Catalog

This note gives a compact notation for operation-like behavior that falls out
of relation shape. It is not a symbolic operator table. It is a checklist of
structural affordances we have seen or expect to see.

Current Gojo scalar reverse/negative participation is written as signed
locations on the same carrier, for example `L(C,-5)`. Rows that mention
`C_inverse` describe explicit dual-face routes, not the ordinary negative
scalar side of `C`.

Notation:

- `L(C,n)` means a location on a carrier, not a relation: L(C,3)
- `P(C,n)` means a point, a relation of (L->C): `(C.n -> C)`
- `S(C,a,b)` means a segment: `P(C,a) -> P(C,b)`
- `S0.P0` and `S0.P1` mean the first and second point of segment `S0`
- `A -> B` means an ordinary `Relation(A, B)`
- `^n` means repeated relation shape, not a primitive exponent operator
- "read layer" means the carrier layer inspected after the structure settles

## Basic Shapes

| Shape | Read |
|---|---|
| `L(C0,0) -> L(C0,k)` | forward projection/basis surface; values are interpreted through it, not hardened by it |
| `L(C0,k) -> L(C0,0)` | reverse return surface; crossing `k` wraps local residue back toward origin and leaves crossed tension |
| `L(C0,k) -> L(C0,k)` | same-site stop surface; clamp-like hardening at `k` |
| `P(C0,0) -> P(C1,0)` | origin attachment / shared reference; no automatic accumulation |
| `P(C0,1) -> P(C1,0)` | one-for-one accumulator; every tick crosses upward |
| `P(C0,k) -> P(C1,0)` | compact base/counter absorption; lower hardens, emits carrier tension, and routes that tension through `k:1` into `C1` |
| `P(C0,k) -> P(C1,m)` | ratio projection candidate when `m > 0`; `k` ticks on `C0` read as `m` ticks on `C1`, with no source clearing |
| `P(C0,k) -> P(C0,0)` | same-carrier reset boundary; local carrier hardens to `value % k`, unaccounted carrier tension remains as `value - value % k` |
| `P(C0,k) -> P(C0,k)` | same-carrier clamp boundary; local carrier hardens to `min(value,k)`, excess remains as carrier tension |

Same-carrier local accounting is directional. Forward `0 -> k` and reverse
`k -> 0` are different physical shapes even when they mention the same two
locations. Forward `0 -> k` is aligned with carrier flow and does not create
tension by itself.

## Segment And Scaling Shapes

| Shape | Read |
|---|---|
| `S(C0,0,1) -> S(C1,0,1)` | segment-to-segment projection; unit duplicate/copy basis shape |
| `S(C0,0,k) -> S(C1,0,m)` | ratio stretch from one basis segment to another; `x` on `C0` reads as `x*m/k` on `C1` in the exact divisible case |
| `S0 -> S1.P1` | multiplication-like basis scaling; source segment flow moves the endpoint of `S1` while `S1.P0 -> S1.P1` remains intact |
| `S0 -> P(C1,n)` | move a point from source segment flow; if that point is an endpoint of a segment, the segment stretches |
| `S(C,5,5)` | zero-length segment; point-like now, but still two related points that can later separate |

A same-carrier segment such as `S(C,2,5)` remains segment structure. It is not
read as `P(C,2) -> P(C,5)` point carry, because that would collapse local
extent into overflow behavior and make ordinary segments unsafe to carry in a
relation surface.

## Repetition And Powers

| Shape | Read |
|---|---|
| `(P(C0,1) -> P(C1,0))^n` | additive repetition; every tick crosses each point layer without scaling the amount |
| `(P(C0,k) -> P(C1,0))^n` | repeated base/counter carry; positional positive integer stack with `n + 1` carrier layers |
| `(P(C0,10) -> P(C1,0))^7` | eight decimal digit carrier stack, such as a fixed-width positive calculator read |
| `(S0 -> S1.P1)^2` | square-like repeated basis scaling |
| `(S0 -> S1.P1)^3` | cube-like repeated basis scaling |
| `S0 -> S1`, `S1 -> S2`, `S2 -> S3` | repeated segment-to-segment projection; copies/scales by each layer's explicit segment lengths |

The point-chain and segment-chain repeats are both repeated relation shapes,
but they are not the same operation. Repeated point layers preserve count
through carry/reset. Repeated segment-to-segment layers copy or scale by the
explicit layer lengths and are tested against ordinary multiplication by those
lengths. Power-like reads still need an explicit repeated basis-scaling shape;
they should not fall out of hidden contribution bookkeeping.

## Residue, Modulo, And Shift Shapes

| Shape | Read |
|---|---|
| `P(C0,k) -> P(C0,0)`, then read `C0` | modulo/residue read; carrier reports the settled `value % k` layer |
| `(P(C0,k) -> P(C0,0)) -> P(C1,0)` | explicit counter route; carrier tension on `C0` is absorbed through `k:1` into `C1` |
| `P(C0,k) -> P(C1,0)`, then read final carried layer | compact distributed quotient/counter read; executable as carrier-tension emission plus immediate route |
| current `P(C1,n)` plus `P(C0,1) -> P(C1,0)` | additive shift; incoming ticks accumulate from an offset carrier position |
| `S(C,a,b) -> S(C,a+n,b+n)` | segment shift/translation candidate; endpoints move together while segment length is preserved |
| `S0 -> S1`, then read `S1` | duplicate/projection read of one basis through another |

The current executable settling supports modulo through active same-carrier
reset boundaries. The carrier value is not left as an uninterpreted larger
number; it hardens to the local residue and leaves unabsorbed carrier tension.
General same-carrier segment shift still needs point identity so coincident
points can move without accidentally moving every matching point in the
surface.

## Routing-Like Shapes

| Shape | Read |
|---|---|
| `L(C0,k) -> L(C1,n)` | structural transfer route; moves `min(value,k)` from `C0` into `C1+n`, leaves `max(value-k,0)` on `C0` |
| `L(C0,k) -> receiver surface` | coupled structural transfer; same accounted source updates each carrier location in the receiver surface |
| `L(C0,m) -> L(unknown,0)` | structural open gather; raises `m` to cover visible `C0` tension without draining `C0` |
| `(L(C0,a) -> L(C0,b)) -> L(C1,n)` | structural observation window; reads up to `|b-a|` from `C0` into `C1+n` without draining `C0` |
| `P(C0,k) -> P(counter,0)` | crossing counter |
| `P(C0,k) -> P(C0_inverse,0)` | opposed/refill or reflection foothold; forward crossing settles as positive ticks on the opposed carrier |
| `P(C0,k) -> P(C0_inverse,0)` plus `P(C0_inverse,k) -> P(C0,0)` | opposed return loop/ping-pong candidate; lossy or thresholded return can settle without signed storage |
| `P(C,k) -> P(C,0)` | same-oriented reset boundary; modulo locally, tension externally |
| `S(C,a,b).P1 -> S(C,a,b).P0` | segment end-to-start cycle candidate; restart without traversing a return path |
| `S(C,a,b).P1 -> S(C_inverse,0)` | loop/ping-pong candidate; flow traverses an opposed return path before it can come forward again |
| `S0 -> (P(C1,n) -> P(C2,m))` | segment flow into a relation surface; possible coupled fanout/general receiver surface |
| `(source relation) -> (open 0/0 site)` | unresolved gather / attachment potential |

Cycling and looping are intentionally separated here. A cycle restarts at a
related origin or point. A loop has a return journey, usually through an
opposed carrier or explicit return relation, so the return path can carry its
own tension, residue, or blockage.

Same-carrier reset is executable as local accounting, not as a full path loop:
`P(C,k) -> P(C,0)` hardens the carrier and exposes crossing tension. A true
cycle or loop still needs an absorber, counter, opposed carrier, or other
relation to account for that tension.

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
