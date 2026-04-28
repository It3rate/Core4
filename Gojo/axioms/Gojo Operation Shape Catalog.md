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

## Basic Shapes

| Shape | Read |
|---|---|
| `P(C0,0) -> P(C1,0)` | origin attachment / shared reference; no automatic accumulation |
| `P(C0,1) -> P(C1,0)` | one-for-one accumulator; every tick crosses upward |
| `P(C0,k) -> P(C1,0)` | base/counter carry; lower keeps `value % k`, upper receives `value / k` |
| `P(C0,k) -> P(C1,m)` | offset carry or carry into an already displaced point; exact offset behavior still needs more tests |
| `P(C0,k) -> P(C0,0)` | same-carrier reset/counter shape; needs point identity before it is fully safe |

## Segment And Scaling Shapes

| Shape | Read |
|---|---|
| `S(C0,0,1) -> S(C1,0,1)` | segment-to-segment stretch/projection |
| `S(C0,0,k) -> S(C1,0,m)` | ratio stretch from one basis segment to another |
| `S0 -> S1.P1` | multiplication-like basis scaling; source segment flow moves the endpoint of `S1` while `S1.P0 -> S1.P1` remains intact |
| `S0 -> P(C1,n)` | move a point from source segment flow; if that point is an endpoint of a segment, the segment stretches |
| `S(C,5,5)` | zero-length segment; point-like now, but still two related points that can later separate |

## Repetition And Powers

| Shape | Read |
|---|---|
| `(S0 -> S1.P1)^2` | square-like repeated basis scaling |
| `(S0 -> S1.P1)^3` | cube-like repeated basis scaling |
| `S0 -> S1`, `S1 -> S2`, `S2 -> S3` | repeated segment relation; power-like behavior when each layer reads the previous stretched basis |

## Routing-Like Shapes

| Shape | Read |
|---|---|
| `P(C0,k) -> P(counter,0)` | crossing counter |
| `P(C0,k) -> P(C0_inverse,0)` | opposed/refill or reflection foothold; inverse handling still needs structural tests |
| `S0 -> (P(C1,n) -> P(C2,m))` | segment flow into a relation surface; possible coupled fanout/general receiver surface |
| `(source relation) -> (open 0/0 site)` | unresolved gather / attachment potential |

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
