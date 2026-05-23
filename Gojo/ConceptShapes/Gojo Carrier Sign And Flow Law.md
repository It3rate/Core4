# Gojo Carrier Sign And Flow Law

This note defines **sign/boundary law** and **flow law** for Gojo carriers: what
they mean, how they combine, how they relate to today's `IsInverse` face marker,
and how to implement them without a kernel rewrite.

**Status:** approved design direction. Implementation follows the phased plan
in [Section 8](#8-implementation-plan). Code work starts after this document.

**Companion documents:**

- `Gojo/axioms/Gojo Structural Transform Relations.md` — signed sites vs inverse faces
- `Gojo/axioms/Gojo Unit Shape And Crystallization.md` — tick shape vs resolution
- `Gojo/axioms/Gojo Resolution Ladders.md` — partitions of a whole vs calibrated units
- `Gojo/ConceptShapes/ConceptShapesCursor.md` — concept shapes built on these laws

---

## Table of Contents

1. [Summary](#1-summary)
2. [Two independent axes](#2-two-independent-axes)
3. [Sign / boundary law (P, N, PN, S)](#3-sign--boundary-law-p-n-pn-s)
4. [Flow law (F, I, FI, Ø)](#4-flow-law-f-i-fi-ø)
5. [The 16 combinations](#5-the-16-combinations)
6. [Relation to today's Carrier](#6-relation-to-todays-carrier)
7. [Encoding in Gojo structure](#7-encoding-in-gojo-structure)
8. [Implementation plan](#8-implementation-plan)
9. [Future extensions (not v1)](#9-future-extensions-not-v1)
10. [Inspection habits vs explicit law](#10-inspection-habits-vs-explicit-law)

---

## 1. Summary

Gojo carriers today expose **which face object** they are (`IsInverse`) and store
**unsigned value** with **signed tension** and **signed locations**. That is not
yet a full statement of:

- how **coordinates** on the site should be read (placement vs uint vs int vs
  bounded segment), or
- whether the site may **calibrate** flowing value (meter) or only support
  comparison, partitions, and layout.

This document adds two **four-valued laws**, each built from the same pattern:

```text
primary pole
dual pole
both poles
neither pole
```

| Axis | Primary | Dual | Both | Neither |
|------|---------|------|------|---------|
| **Sign / boundary** | **P** (positive / uint) | **N** (negative / nint) | **PN** (int) | **S** (segment whole) |
| **Flow** | **F** (forward) | **I** (inverse) | **FI** (both faces) | **Ø** (no flow) |

**Default for compatibility:** absent law attachments read as **PN + F** on the
forward face — matching current bare-carrier behavior.

**Not in v1:** football-field dual-origin frame selection (documented only as an
[extension example](#91-dual-origin-bounded-segment-football-field)).

---

## 2. Two independent axes

The system is intentionally **not** one sixteen-way enum baked into `Carrier`.
It is two laws that compose:

```text
CarrierSite (identity + unit shape + attached structure)
  SignLaw   ∈ { P, N, PN, S }
  FlowLaw   ∈ { F, I, FI, Ø }
```

### 2.1 Why two axes

| Question | Axis |
|----------|------|
| How do I interpret **positions** on this ruler? | **Sign / boundary** |
| May this ruler **calibrate** flowing participation (settlement into value)? | **Flow** |

**Calibration** (feet per tick, counting visits) needs a **flow law other than Ø**.
**Percent of a fixed corridor** (thirds of a bridge neck) needs **S** (bounded
whole) and often **Ø** until you also meter throughput.

### 2.2 Dual + both + neither on each axis

On each axis, **dual** means the opposed pole of that axis (not "scalar minus"
confused with inverse **face**):

- **N** is the **sign dual** of **P** (against-goal as first-class, not failed positive).
- **I** is the **flow dual** of **F** (calibration on the inverse face law).

**Both** means both poles of that axis are active for reads/settlement that need them:

- **PN** — toward-goal and against-goal ticks on the active face (int grammar).
- **FI** — forward and inverse **face** may settle (two storages when both materialized).

**Neither** means deliberately **not** using that axis's pole grammar:

- **S** — neither infinite positive nor negative ray; the instance **is** a
  **segment between two anchors** (positions, overlap, partitions of the whole).
- **Ø** — neither forward nor inverse **flow calibration**; pier / union / layout /
  boolean segment compare; partitions OK.

---

## 3. Sign / boundary law (P, N, PN, S)

### 3.1 The four values

| Law | Name | Meaning |
|-----|------|---------|
| **P** | Positive / uint | One goal. Coordinates march **with** goal (0, 1, 2, …). "How far toward the end." |
| **N** | Negative / nint | Dual of P. One goal. Only **against-goal** coordinates (−1, −2, …) are first-class. |
| **PN** | Both signs / int | Full scalar line on the face: toward-goal and against-goal both meaningful. |
| **S** | Neither sign ray / segment | **Both boundaries** anchor a **bounded whole**. Coordinates are **positions in the instance** — overlap, percent, thirds — not an unbounded number line. |

### 3.2 Segment picture (boundary metaphor)

```text
P:   [origin|····················)     one wall, march toward increasing tick
N:   (····················|origin]     one wall, against-goal ticks first-class
PN:  (····························)   no extra walls on the signed line
S:   [anchor A|············|anchor B]  TWO walls — the whole is the segment
```

**S** is where "divide into 100 sections" and "half the distance" live: the
**whole** is defined before numeric units attach.

### 3.3 Relation to low-resolution reads

| Low-resolution style | Typical sign law |
|----------------------|------------------|
| Estimate with units (feet not inches) | **P**, **N**, or **PN** with **flow ≠ Ø** |
| Partitions of the whole (3 buckets, 100%) | **S** (often with **Ø** until metering is added) |
| Boolean overlap of segments | **S** + **Ø** |
| Layout / pier / "not half a water" | **S** + **Ø** |

### 3.4 Not the same as inverse carrier face

| Concept | Role |
|---------|------|
| **N** (sign law) | Scalar **against-goal** grammar on a **face** |
| **I** (flow law) | Settlement/read on **inverse carrier face** |
| **`i` in read notation** | "on inverse **face**" (e.g. `3i`), not "negative number" |

---

## 4. Flow law (F, I, FI, Ø)

### 4.1 The four values

| Law | Name | Meaning |
|-----|------|---------|
| **F** | Forward | Calibrate on the **forward** face law. Native forward gear. |
| **I** | Inverse | Calibrate on the **inverse** face law. Dual of forward gear (one-way filter flip). |
| **FI** | Both flows | **Both** faces may participate in settlement when explicitly wired. |
| **Ø** | Neither flow | **No flowing calibration.** Trolley, compare, union, intersect, layout. Partitions and spans OK; not a metered stream. |

### 4.2 Inspection vs settlement (existing split)

From structural transform axioms:

```text
inspection flow  = read a location through a perspective surface
settlement flow  = tension/value strikes accounting surfaces on carriers
```

Flow law gates **settlement calibration**. **Ø** does not forbid inspection reads
(`BoundariesOn`, overlap, projection as read-only).

### 4.3 FI and two values

**FI** does **not** mean one integer storage. It means **both face carriers** may
be active (`C` and `C.Inverse()`), with exchange relations — as today when dual
faces are wired. Read notation may still show `3`, `−3`, `3i`, `−3i` as **face +
sign + magnitude**, not four separate storages.

### 4.4 Unspecified flow commitment (concepts)

For concept connections (bridge seed), **unspecified** flow is the same encoding
as **Ø** or **free connection without directional law** until refinement:

- topology present (neck exists),
- no one-way or reversible law installed yet.

Refinement to **F**, **I**, or **FI** is a timeline step — not a different primitive.

---

## 5. The 16 combinations

Each cell is **SignLaw + FlowLaw**. Words are **reads**, not stored enums.

### 5.1 Matrix

|  | **P** | **N** | **PN** | **S** |
|--|-------|-------|--------|-------|
| **F** | uint meter → | nint meter → | int meter → | % of span, metered → |
| **I** | uint meter ← | nint meter ← | int meter ← (inv gear) | % of span, inv meter |
| **FI** | two uint lanes | rare | two-way int street | reversible + occupancy % |
| **Ø** | labeled ray | labeled ray | layout line | **pier, ratio, overlap, concept seed** |

Arrow **→** / **←** = forward / inverse **calibration gear**, not "negative numbers flow backward."

### 5.2 Short real-world anchors

| Cell | Example |
|------|---------|
| **P + F** | Odometer, inventory count, queue position |
| **PN + F** | Signed offset along a drive direction |
| **PN + FI** | Two-way traffic with signed net flow |
| **S + Ø** | "At the 40-yard line" as place; segment overlap; non-count layout |
| **S + F** | Metered **share** of a fixed corridor (e.g. 60% of neck capacity) |
| **P + I** | One-way counter on **inverse** face (filter direction) |
| **S + FI** | Fixed bridge span: meter **both** directional throughputs on same section |

### 5.3 Language / non-count nouns

Many concept words sit in **S + Ø**:

- "Half the **distance**" → shorter interval, not half a meter.
- "Middle of the **bridge**" → partition of the whole.
- Mass nouns (water, line) bounded by the **drawn instance**.

Count nouns ("three cars", "five inches") generally need **flow ≠ Ø** and **P**, **N**, or **PN**.

---

## 6. Relation to today's Carrier

### 6.1 What `IsInverse` is today

```text
IsInverse = which carrier *face object* this instance is (forward vs inverse)
```

It is **not**:

- sign law (P/N/PN/S),
- flow law (F/I/FI/Ø),
- "negative numbers" as a line.

### 6.2 What stays the same in v1

- `Carrier`, `CarrierLocation`, `TickRef`, `Relation`, settlement routes.
- `Carrier.Inverse()` for opposed face materialization.
- Unsigned `Value` + signed `Tension` + signed locations.

### 6.3 What gets added

- **SignLaw** and **FlowLaw** as **relation-shaped** attachments on a **holder** carrier.
- **TryReadSignLaw** / **TryReadFlowLaw** (and optional segment bounds for **S**).
- Read gates (Phase 1) and settlement gates (Phase 2).
- **Default:** missing laws ⇒ **PN + F** (no silent behavior change).

### 6.4 What we are not doing in v1

- Replacing `Carrier` with a new `CarrierSite` type (optional later refactor).
- Football dual-origin frame selector (see [Section 9.1](#91-dual-origin-bounded-segment-football-field)).
- Sixteen concept enums or `Carrier.SignFlow` bitfield as source of meaning.

---

## 7. Encoding in Gojo structure

Laws must remain visible when helper types disappear. Prefer **marker relations**
on `Carrier.Unknown`, not opaque carrier fields.

### 7.1 Law markers (proposed)

```text
SignLawSurface(holder):
  Point(Unknown, -3) -> Point(holder, 0)   // P
  Point(Unknown, -3) -> Point(holder, 1)   // N
  Point(Unknown, -3) -> Point(holder, 2)   // PN
  Point(Unknown, -3) -> Point(holder, 3)   // S

FlowLawSurface(holder):
  Point(Unknown, -4) -> Point(holder, 0)   // F
  Point(Unknown, -4) -> Point(holder, 1)   // I
  Point(Unknown, -4) -> Point(holder, 2)   // FI
  Point(Unknown, -4) -> Point(holder, 3)   // Ø
```

Attach to the carrier used as **holder**:

```csharp
holder.Attach(CarrierLaw.SignLaw(holder, SignLawKind.PN));
holder.Attach(CarrierLaw.FlowLaw(holder, FlowLawKind.F));
```

### 7.2 Sign law S — both boundaries

Additionally attach segment bounds on the same holder:

```text
Segment(holder, anchorA, anchorB)
```

Reads use anchors for overlap, wrap partitions, and percent-of-whole. No single
`Point(holder, 0)` goal is required for **S**.

### 7.3 Effective laws (compatibility)

```text
if no SignLaw attached  => treat as PN
if no FlowLaw attached  => treat as F
```

All new checks use **effective** laws until tests explicitly attach markers.

### 7.4 Concept shapes

`ConceptSurface` / `ConceptShape` packaging should record law attachments on
the **flow surface** and **constraint surface**, not English labels:

```text
bridge seed:     topology + FlowLaw unspecified or Ø + SignLaw S or PN
one-way filter:  FlowLaw I (or F) + projection residue
peninsula:       topology + open outlet + Ø
```

---

## 8. Implementation plan

Phased rollout. **Do not** implement football dual-origin in these phases.

### Phase 0 — Law surface only (small, low risk)

**Add:** `Gojo/Core/CarrierLaw.cs` (name may vary)

| Deliverable | Content |
|-------------|---------|
| API | `AttachSignLaw`, `AttachFlowLaw`, `TryReadSignLaw`, `TryReadFlowLaw`, `TryReadSegmentBounds` |
| Encoding | Marker relations per Section 7 |
| Tests | `Gojo.Tests/Core/CarrierLawTests.cs` (~25–35 tests) |
| Docs | This file (done); cross-links only |

**No changes** to settlement, `CarrierRead`, or serialization.

**Size:** ~250–400 lines core, ~300 lines tests.

---

### Phase 1 — Read path respects laws (medium-small)

**Touch:**

| File | Change |
|------|--------|
| `CarrierRead.cs` | Gate visibility by sign law |
| `Relation.Reads.cs` | `FlowOn` / traversal inactive for **Ø** where appropriate |
| `CarrierLaw.cs` | `ReadPolicy(holder, location, reader)` |

**Tests:** ~15–25 new/extended in `CarrierReadTests`, `CarrierLawTests`.

**Size:** ~150–250 lines core, ~200–300 lines tests.

---

### Phase 2 — Settlement respects laws (medium, main churn)

**Touch:**

| File | Change |
|------|--------|
| `Carrier.cs` | Entry to settlement respects **FlowLaw** |
| `Carrier.RouteSettlement.cs` | Route targets skip **Ø**; **I** routes to inverse face |
| `Relation.Settlement.cs` | Face rules interact with **FI** vs **F** |
| `GojoEncodedSnapshot.cs` | Optional persistence of law markers (last) |

**Sign at settlement:**

| Law | Behavior |
|-----|----------|
| **P** | Below-origin remains tension; do not absorb as positive value |
| **N** | Dual: only against-goal participation settles |
| **PN** | Current behavior |
| **S** | Wrap/partition OK; block unit crystallization unless flow allows |

**Flow at settlement:**

| Law | Behavior |
|-----|----------|
| **Ø** | No metered calibration into `Value` |
| **F** | Forward face only |
| **I** | Inverse face only |
| **FI** | Both faces when wired (today's dual foothold) |

**Tests:** ~20–30 new settlement tests; ~5–15 existing tests may need explicit law attachment.

**Size:** ~300–500 lines core, ~400–600 lines tests.

---

### Phase 3 — Optional later (large, defer)

- `CarrierSite` holder type unifying identity + laws + faces.
- Migration away from attaching laws only on forward carrier.
- Full snapshot schema version bump.

---

### Implementation order (recommended)

1. This axiom document (complete).
2. Phase 0: `CarrierLaw.cs` + tests.
3. Phase 1: read gating.
4. Wire `Gojo/ConceptShapes` to attach/read laws on examples.
5. Phase 2: settlement — **Ø** first, then **P/N**, then **I/FI** face routing.
6. Snapshot persistence.

---

### Effort summary

| Scope | Rating |
|-------|--------|
| Phase 0 only | **Small** — assessment + law CRUD |
| Phase 0 + 1 | **Medium-small** — correct reads |
| Phase 0 + 1 + 2 | **Medium** — kernel-complete for laws |
| + `CarrierSite` refactor | **Large** — defer |

| Artifacts | Approximate total |
|-----------|-------------------|
| New tests | ~60–90 |
| Adjusted existing tests | ~5–15 |
| Documentation | this file + codex cross-links |

---

## 9. Future extensions (not v1)

These are **composed patterns** on top of P/N/PN/S and F/I/FI/Ø. No extra
primitives required in the four-by-four grid.

### 9.1 Dual-origin bounded segment (football field)

**Not implemented in v1.** Documented as an extension example.

**Idea:**

```text
One bounded segment (SignLaw S) between two goal-line anchors.
Two frame laws, each Positive (P) with Forward (F) marching inward toward midfield:
  FrameA: origin at goal A, positive toward B
  FrameB: origin at goal B, positive toward A
Exactly one frame active per read — selector (e.g. possession), not FlowLaw FI.
```

| Feature | Encoding |
|---------|----------|
| Field geometry | **S** + often **Ø** |
| Play-by-play yards | **P + F** on **active** frame only |
| Both teams at once | **not FI** — use **selector**, not simultaneous meters |
| Absolute 0–100 chart | Optional second read chart on same **S** |

**Implementation sketch (later):**

```text
DualOriginSegment(holder, anchorA, anchorB)
FrameLaw(holder, FrameA, originA, towardB)
FrameLaw(holder, FrameB, originB, towardA)
ActiveFrame(selector) -> FrameA | FrameB
```

This belongs in `ConceptShapes` or a small `Gojo/Structures/FrameSelector` helper,
not in the sign/flow law enum grid.

### 9.2 Other extensions (notes only)

| Pattern | Composition |
|---------|-------------|
| Open attach / void sink | **Ø** flow + orthogonal outlet + placeholder |
| Valve coarse buckets | **S** wrap partition (n=3) + **F** when metering added |
| Bridge one-way filter | **I** or **F** + projection residue |
| Dual-origin selector | **S** + two **P+F** frames + context switch |

---

## 10. Inspection habits vs explicit law

| Old inspection habit | Explicit encoding |
|----------------------|-------------------|
| "Not focusing on direction" | **Ø** flow or unspecified flow commitment on link |
| "Assuming the whole" | **S** sign law |
| "Uint-like" | **P** |
| "Int-like" | **PN** (one face) or **FI** (two-face exchange) |
| "One-way bridge" | **F** or **I** + filter residue |

Concept and equation reads should stop relying on implicit defaults once laws
are attached in structure.

---

## Document history

- Initial version: sign/flow four-by-four, dual/both/neither framing, implementation
  phases, football field as Section 9.1 extension only (no v1 code).

---

*End of Gojo Carrier Sign And Flow Law*
