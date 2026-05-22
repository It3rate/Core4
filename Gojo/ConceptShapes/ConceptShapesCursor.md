# Concept Shapes Cursor
This document is the consolidated design reference for **concept shapes** in Gojo:
low-resolution, physically inspectable, perturbable structure that represents how
we think about things (corners, passages, collection, delivery, sharpness, absence)
before they become measurements, CAD numbers, or application objects.

It supersedes enum-based concept vocabularies. Meaning lives in **connected Gojo
structure**. Helpers may build and read concept shapes; they must not own concept
identity.

**Companion documents:**

- `Gojo/ConceptSurface/README.md` — example catalog and near-term code direction
- `Gojo/axioms/Gojo Potential Shape Fact Sheet.md` — potential, orthogonal, fill
- `Gojo/axioms/Gojo Unit Shape And Crystallization.md` — tick contracts
- `Gojo/axioms/Gojo Resolution Ladders.md` — multi-level wholes and partial reads
- `Gojo/axioms/Gojo Structural Transform Relations.md` — signed sites vs inverse faces
- `Gojo/axioms/Gojo Carrier Sign And Flow Law.md` — **sign law (P/N/PN/S) and flow law (F/I/FI/Ø), 16 combinations, implementation plan**
- `AGENTS.md` — project rules for Gojo work

**Status:** design codex. Executable `ConceptShape` helpers are a proposed foothold;
this codex defines what they should construct and read.

---

## Table of Contents

1. [Purpose and mental model](#1-purpose-and-mental-model)
2. [Three layers: tick shape, concept shape, concept word](#2-three-layers-tick-shape-concept-shape-concept-word)
3. [Design rules](#3-design-rules)
4. [Four participations and flow goals](#4-four-participations-and-flow-goals)
5. [Gojo math primitives (exhaustive inventory)](#5-gojo-math-primitives-exhaustive-inventory)
6. [Concept shape generators](#6-concept-shape-generators)
7. [Construction: literal relation signatures](#7-construction-literal-relation-signatures)
8. [Unordered versus ordered structure](#8-unordered-versus-ordered-structure)
9. [Absence: omitted, open attach, void sink](#9-absence-omitted-open-attach-void-sink)
10. [Partitions, ticks, and resolution](#10-partitions-ticks-and-resolution)
11. [Radial and contour calibration](#11-radial-and-contour-calibration)
12. [Angles without angle primitives](#12-angles-without-angle-primitives)
13. [Group fan versus radial field](#13-group-fan-versus-radial-field)
14. [Assembly: the concept shape record](#14-assembly-the-concept-shape-record)
15. [Extension and refinement](#15-extension-and-refinement)
16. [Reads as focus (exposure, not a focus primitive)](#16-reads-as-focus-exposure-not-a-focus-primitive)
17. [Settlement, residue, and perturbation](#17-settlement-residue-and-perturbation)
18. [Probability, certainty, and presence](#18-probability-certainty-and-presence)
19. [Composition catalog](#19-composition-catalog)
20. [Refinement pairs (seed → one tweak → new word)](#20-refinement-pairs-seed--one-tweak--new-word)
21. [Linguistic and metaphorical re-grounding](#21-linguistic-and-metaphorical-re-grounding)
22. [Missing core Gojo structure](#22-missing-core-gojo-structure)
23. [Anti-patterns](#23-anti-patterns)
24. [Appendix: quick reference tables](#24-appendix-quick-reference-tables)
25. [Near-term implementation checklist](#25-near-term-implementation-checklist)

---

## 1. Purpose and mental model

### 1.1 What a concept shape is

A **concept shape** is a connected subgraph of ordinary Gojo elements that
expresses an **affordance promise** at low resolution:

```text
this local structure can be joined, crossed, narrowed, widened, blocked,
passed through, accumulated, repeated, opposed, left open, or refined
```

without yet committing to numeric size, material, tolerance, tool path, or a
named mechanical part number.

Concept shapes are meant to mirror **structure in our heads when we think** about
something:

- a **corner** is an orthogonal meeting before we know chamfer, fillet, or sharpness;
- a **bridge** is a passage between two regions before we know capacity or filter mesh;
- **sharp corner** is not a separate universe — it is a corner whose transition
  parameter is driven toward zero or omitted;
- a **peninsula** is like a funnel collector whose **output is intentionally not
  implemented**, not forgotten.

### 1.2 What a concept shape is not

| Not this | Why |
|----------|-----|
| A `UnitShape` on a value carrier | Unit shape crystallizes **tick meaning** for measurement-grade carriers |
| An enum catalog (`Bridge`, `Funnel`, …) | Words are **reads** over structure, not source of meaning |
| A wrapper class that hides the graph | If the helper vanishes, the graph must still speak |
| A simulation of physics | Forces/materials appear only as **settlement witnesses** when needed |
| A CAD solid | Solids are late refinement; early shape is relation topology |

### 1.3 Relationship to `ConceptSurface`

`ConceptSurface` (README) is the **host pattern** for packaging a concept shape
for inspection, refinement, and solver seeds. This codex defines the **primitive
algebra** that fills those surfaces.

```text
Concept shape     = the relation-world (topology + laws + slots + witnesses)
ConceptSurface    = OrderedGroup packaging + metadata surfaces + timeline
Concept word      = local read at a resolution (bridge, sharp corner, deliver, …)
```

---

## 2. Three layers: tick shape, concept shape, concept word

### 2.1 UnitShape (tick participation)

```text
UnitShape  →  one tick of this carrier participates like this
```

Examples: linear atom, circular contour tick, sheet tick, product of spans,
`ResolutionOf(shape, N)` coarse ticks.

Unit shape is **immutable** on a carrier after crystallization. It answers
**measurement grain**, not **concept role**.

### 2.2 Concept shape (affordance participation)

```text
Concept shape  →  this connected structure affords these joins, passages,
                  potentials, constraints, open attaches, and settlement responses
```

Built only from Gojo math primitives (Section 5–7). May use carriers as **role
sites** (neck width, angle slot, phase partition) without assigning shop-floor
units.

### 2.3 Concept word (interpretive read)

```text
Concept word  →  local read over concept shape + exposed reads + witnesses
                + what was omitted vs left open
```

The same concept shape can read as **bridge**, **choke**, or **connector** depending
on which element carries an **exposed read** (Section 16), not because the word
was stored in structure.

### 2.4 Crystallization analogy (corrected)

| Layer | Crystallizes | Host |
|-------|--------------|------|
| UnitShape | tick kind | `Carrier.UnitShape` |
| Concept shape | affordance pattern | relation subgraph / `ConceptSurface` group |
| Design detail | evidence, bounds, timelines | witnesses inside surface |

Do **not** add `Carrier.ConceptShape` parallel to `UnitShape` unless a later design
proves a carrier must permanently promise concept role. Prefer quoted subgraphs.

---

## 3. Design rules

1. **Structure is the source of truth.** Enums may classify; they must not create meaning.
2. **Primitives are relation signatures**, not mechanical nouns.
3. **Compose before you name.** Friction, filter, choke are compositions.
4. **Separate participation kinds** (Section 4): scalar signed site ≠ inverse face.
5. **Unordered is explicit** (Section 8): do not confuse syntax order with semantic direction.
6. **Absence is three-way** (Section 9): omitted ≠ open attach ≠ void sink.
7. **Partitions imply wholes** (Section 10): three ticks ≠ three-tenths without a wrap.
8. **Radial ≠ group fan** (Section 13): field potential vs attachment list.
9. **Focus = exposed read** (Section 16): wiring, not a phenomenon primitive.
10. **Perturbation preserves ghosts** (Section 17): tension/residue are information.
11. **Extend via slots and timelines** (Section 15), not reclassification.
12. **Metaphor rebinds descriptors**, not algebra (Section 21).

---

## 4. Four participations and flow goals

Gojo distinguishes **signed locations on one carrier** from **inverse carrier faces**.
Concept shapes must keep them separate.

### 4.1 The four corners

Treat these as **inspectable participation corners**, not as a second integer number line:

| Corner | Notation | Meaning |
|--------|----------|---------|
| **P+** | forward carrier, location ≥ 0 (convention) | march **with** native forward goal |
| **P−** | forward carrier, location < 0 or below-origin tension | **against** forward goal on same face; grows farther from “end of flow” |
| **I+** | inverse carrier face, positive site | opposed-flow participation, forward on that face |
| **I−** | inverse face, against-flow site | against goal on opposed face |

```text
Goal of a directed number line  =  reach the end of encoded forward flow
P−  =  farther from that goal on the SAME face (not “opposite line”)
I+  =  different face / opposed participation (not scalar negation)
```

### 4.2 Uint-like carriers (concept)

A **uint concept** uses only **P+** participation on the forward face:

- visible `Value` stays non-negative in current Gojo storage;
- below-origin participation is **signed tension**, not a second visible line;
- routes and claims **reject** P− reads or leave residue.

Construction pattern:

```text
parameter carrier C
constraint: AtLeast(Point(C,0), floor) or read convention n ≥ 0
routes use only Point(C, n) with n ≥ 0
```

### 4.3 Flow restriction (concept)

Restrict traversal by using **one corner** or **one face**:

| Policy | Structure sketch |
|--------|------------------|
| Forward only | `DirectedSegment(C, invStart, posEnd)` or one-way projection |
| Inverse receiver only | route to `C.Inverse()` explicit point |
| Block P− | `OperationClaim` + residue on negative attempt |
| Scalar only, no inverse face | never materialize `C.Inverse()` in concept graph |

### 4.4 Directed segment as oriented span

`Relation.DirectedSegment(C, inverseStart, positiveEnd)` fuses opposed-face
endpoints into one **oriented** extent — the workhorse for flow-bearing concepts.

### 4.5 Complex-like direction (concept)

**Direction + extent** without a world angle:

```text
forward span  ×  inverse-face span  →  directed participation read
```

This is product/participation structure, not a symbolic `complex` type.

---

## 5. Gojo math primitives (exhaustive inventory)

Everything in Sections 6–21 must compose from this list.

### 5.1 Ordered connection

| Primitive | API / form | Role |
|-----------|------------|------|
| **Arrow** | `Relation(A, B)` | Universal ordered glue |
| **Reverse** | `relation.Reverse()` | Swap ends; not dual |
| **Dual** | `element.Dual()` | Structural inversion |

Chains nest arrows: `A → B → C`.

### 5.2 Sites

| Primitive | Form | Role |
|-----------|------|------|
| **Point** | `Relation.Point(C, n)` → `L(C,n) → C` | Site, marker, endpoint |
| **Signed site** | `L(C, n)` with `n < 0` | Against-flow on forward face |
| **Shared coordinate** | `TickRef` grounded in multiple `CarrierLocation`s | Same physical site |
| **Wildcard** | `Carrier.Unknown` | Unbound frame / discovery |

### 5.3 Extent on one carrier

| Primitive | Form | Role |
|-----------|------|------|
| **Directed segment** | `Segment(C, a, b)` | Oriented interval; projection frame |
| **Directed face segment** | `DirectedSegment(C, invStart, posEnd)` | Oriented across faces |
| **Collapsed segment** | `Segment(C, t, t)` | Undirected basis; zero-length orthogonal seed |
| **Symmetric span** (idiom) | pair `Segment(C,a,b)` + `Segment(C,b,a)` or future helper | Unordered extent a↔b |

### 5.4 Potential / orthogonal grade change

| Primitive | Form | Role |
|-----------|------|------|
| **Orthogonal outlet** | `OrthogonalOutlet(source)` | Uncalibrated potential |
| **Point radial** | `OrthogonalOutlet(Point(C,n))` | Contour / radial field seed |
| **Ray potential** | `OrthogonalOutlet(Segment(C,t,t))` | Linear potential from collapsed basis |
| **Sheet potential** | `OrthogonalOutlet(Segment(C,a,b), a≠b)` | Side / area potential |

Unit-shape crystallization (`CircularFrom`, `LinearFrom`, `SheetFrom`) **hardens**
potential into tick contracts — use at measurement boundary, not as concept identity.

### 5.5 Gathering and packaging

| Primitive | Form | Role |
|-----------|------|------|
| **Additive attachment** | `AdditiveAttachment(holder, X)` | `Point(holder,0) → X` |
| **Ordered group** | `OrderedGroup(holder, …)` | Sequence, surface host, branch list |
| **Additive gather** | chained attachments | Coexistence at holder |

### 5.6 Faces

| Primitive | Form | Role |
|-----------|------|------|
| **Forward carrier** | `Carrier` | Positive-flow face |
| **Inverse face** | `carrier.Inverse()` | Opposed-flow face |
| **Opposed unit face** | `UnitShape.OpposedFaceOf` | Tick-grade opposed wrapper |
| **Fused faces** | `DirectedSegment` / fused-face reads | Contact, opposed endpoints |

### 5.7 Transfer, accounting, ports

| Primitive | Form | Role |
|-----------|------|------|
| **Projection** | frame `Segment → Segment` (or point frames) | Map, scale, converge/diverge read |
| **Projection residue** | failed / partial `TryProject` | Block, filter, obstruction witness |
| **Port import** | `TrySetFrom` | Admit value into frame |
| **Live output** | `LiveOutput(input, port)` | Continuous scoped feed |
| **Wrap partition** | `Point(C,n) → Point(C,0)` | Whole divided into n parts |
| **Clamp** | `Point(C,k) → Point(C,k)` | Fixed ceiling/floor |
| **Cycle counter** | `Point(phase,span) → Point(cycles,0)` | Closure with count |

### 5.8 Open structure

| Primitive | Form | Role |
|-----------|------|------|
| **Placeholder** | `Placeholder` | Unbound concept slot |
| **Descriptor slot** | `DescriptorSlot(i)` + bind | Provenance / naming |
| **Pattern apply** | `StructuralPattern.Apply` | Instantiate template |

### 5.9 Witness layers

| Primitive | Form | Role |
|-----------|------|------|
| **Tension / value** | on `Carrier` | Perturbation state |
| **Operation claim** | `Equality`, `AtMost`, `Between`, … | Threshold, bound |
| **Operation follow** | claim + response route | Step, latch-like behavior |
| **Repeat surface** | selected + controller + … + timeline | Repeated feature |
| **Time step sequence** | ordered steps with residue | Refinement history |
| **Equation fanout** | one input → many branches | Branching read structure |

### 5.10 Interpretive reads (not generators)

| Layer | Examples |
|-------|----------|
| Boolean-style | `AndOn`, `OrOn`, `XorOn`, `NotOn` over support/tension |
| Boundary / traversal | `BoundaryPointsOn`, `TraversedSpansOn` |
| Fill reads | `CreateFillBasis`, fill measure |

These classify existing structure; they do not replace concept generators unless
promoted to relation-shaped surfaces (Section 22).

---

## 6. Concept shape generators

Concept primitives are **composition forms** built from Section 5. Target set: **22 generators**.

| ID | Generator | Built from (Section 5) |
|----|-----------|-------------------------|
| G1 | **Site** | Point |
| G2 | **Co-site** | TickRef / same location |
| G3 | **Directed extent** | Segment |
| G4 | **Symmetric extent** | collapsed segment or span pair |
| G5 | **Arrow chain** | nested Relation |
| G6 | **Dual / Reverse** | Dual, Reverse |
| G7 | **Radial potential** | OrthogonalOutlet(Point) |
| G8 | **Ray potential** | OrthogonalOutlet(zero segment) |
| G9 | **Sheet potential** | OrthogonalOutlet(nonzero segment) |
| G10 | **Gather** | AdditiveGroup / attachments |
| G11 | **Fan-out** | OrderedGroup branches / EquationFanout |
| G12 | **Opposed pair** | DirectedSegment / inverse+forward |
| G13 | **Projection frame** | Segment → Segment |
| G14 | **Wrap partition** | Point(n)→Point(0) |
| G15 | **Clamp boundary** | Point(k)→Point(k) |
| G16 | **Open slot** | Placeholder / DescriptorSlot |
| G17 | **Claim** | OperationClaim |
| G18 | **Open attach** | outlet + Placeholder / Unknown port |
| G19 | **Void sink** | residue route to non-consumer |
| G20 | **Exposed read** | port / LiveOutput / projection witness |
| G21 | **Repeat / timeline** | RepeatSurface / TimeStepSequence |
| G22 | **Ordering commitment** | attach DirectedSegment (refinement) |

**Demoted (not generators):** bridge, funnel, friction, passage, convergence,
focus phenomenon, sharp corner, presence, stable site, allow/block nouns.

---

## 7. Construction: literal relation signatures

### 7.1 Site and co-site

```text
Site(C, n):
  Relation.Point(C, n)

CoSite(C, n) via TickRef:
  t = TickRef(n)
  L(C, t) grounded on forward face
  optional: second carrier face grounds same t
```

### 7.2 Directed and symmetric extent

```text
DirectedExtent(C, a, b):
  Relation.Segment(C, a, b)

SymmetricExtent(C, a, b):
  Segment(C, a, b)  and  Segment(C, b, a)   // no privileged traverse
  // OR collapsed:
SymmetricExtent(C, t):
  Segment(C, t, t)
```

### 7.3 Junction (H-meet)

```text
Junction(C, t):
  Segment(C, a, t)  +  Segment(C, b, t)  +  CoSite at t
```

### 7.4 Passage (neck between regions)

```text
Passage(regionA, regionB, neckCarrier, neckSpan):
  Gather(holder, regionA, regionB)
  DirectedExtent(neckCarrier, 0, neckSpan)   // neck
  Attach regionA → neck start, neck end → regionB
  // flow law attached separately (Section 8)
```

### 7.5 Radial potential (uncalibrated)

```text
RadialPotential(centerCarrier):
  Relation.OrthogonalOutlet(Relation.Point(centerCarrier, 0))
```

### 7.6 Calibrated contour (radius + perimeter direction)

```text
CalibratedContour(center, radiusCarrier, perimeterCarrier):
  RadialPotential(center)
  Relation.Point(radiusCarrier, r) → Relation.Point(perimeterCarrier, p)
  // direction: second marker on perimeter OR oriented route (Section 11)
```

### 7.7 Wrap partition (whole into n parts)

```text
WrapPartition(phaseCarrier, n):
  Relation.Point(phaseCarrier, n) → Relation.Point(phaseCarrier, 0)
```

Valve at low resolution: `n = 3` without numeric ranges — three coarse buckets on
one whole (Section 10).

### 7.8 Open attach

```text
OpenAttach(sourceElement):
  Relation.OrthogonalOutlet(sourceElement)
  port = Point(Unknown,0) → Placeholder or Unknown
  LiveOutput optional from outlet to port
  // witness: tension accumulates on outlet until bound
```

### 7.9 Void sink

```text
VoidSink(sourceElement, residueCarrier):
  route from source to Point(residueCarrier, 0)
  no consumer binding on residue branch
  // read: participation exits concept world as tension
```

### 7.10 Exposed read (focus by wiring)

```text
ExposeRead(selectedElement, readPort, targetCarrier):
  Relation.LiveOutput(selectedElement, readPort)
  readPort projects into targetCarrier slot
```

Reading neck **capacity** on a bridge shape is what makes **choke** a natural word
(Section 16).

### 7.11 Claim-bound parameter

```text
BoundParameter(valueElement, min, max, residual):
  OperationClaim.Between(holder, valueElement, min, max, residual)
```

### 7.12 Corner (orthogonal meeting)

```text
Corner(C, t):
  Segment(C, t, t_a)  +  Segment(C, t, t_b)  with orthogonal meeting at t
  // symmetric extents from co-site t
```

**Sharp corner refinement:** omit transition orthogonal OR bind radius slot → 0 OR
`AtMost(radius, ε)`.

**Fillet refinement:** `RadialPotential` at t + `CalibratedContour` transition.

### 7.13 Funnel versus peninsula (absence)

```text
Funnel:
  InverseRadialPotential → neck → DirectedExtent outlet (forward)

Peninsula (output omitted):
  InverseRadialPotential → neck
  // no outlet in ConceptSurface group

Peninsula (output open):
  InverseRadialPotential → neck → OpenAttach(neckEnd)

Collector:
  InverseRadialPotential → neck → Gather(retainBasin)
```

---

## 8. Unordered versus ordered structure

### 8.1 Syntax versus semantics

Every `Relation` is **ordered** (`Left → Right`). **Semantic** unorderedness is
a convention on top:

| Semantic | Structure |
|----------|-----------|
| Unordered pair of sites | Co-site + no privileged arrow between regions |
| Unordered extent | symmetric span or collapsed segment |
| Unordered bag | `AdditiveGroup` — coexistence without sequence |
| Ordered route | single `Segment` used as frame |
| Ordered list | `OrderedGroup` — sequence matters |

### 8.2 Ordering as a decision (refinement)

Moving from unordered → ordered is a **concept refinement** recorded on timeline:

```text
Step: bind DirectedExtent(C, a, b)  // chooses traverse direction
Step: attach DirectedSegment         // chooses face orientation
Step: install paired reverse routes  // declares reversible
```

Conditional ordering:

```text
claim threshold on parameter
OperationFollow routes to alternate directed attachment
```

### 8.3 Allowable both directions

```text
forward route Segment(C, a, b)
inverse face route on C.Inverse()
// OR explicit reverse Segment(C, b, a) with declared reversible law
```

### 8.4 Fan-out kinds

| Kind | Structure | Inspectability |
|------|-----------|----------------|
| **Group fan** | `OrderedGroup(holder, branchᵢ…)` | list of attachments |
| **Equation fanout** | `EquationFanout.Create(…)` | multi-branch read |
| **Radial fan** | `OrthogonalOutlet(Point)` | field potential, not a list |

Do not call group fan “radial.”

---

## 9. Absence: omitted, open attach, void sink

### 9.1 Three meanings

| State | Meaning | How to document |
|-------|---------|-----------------|
| **Omitted** | not part of this concept at this resolution | element absent from `OrderedGroup`; no phantom port |
| **Open attach** | port exists; flow may leave; binding TBD | `OpenAttach` + Placeholder + tension witness |
| **Void sink** | flow intentionally unreceived | explicit residue route, no consumer |

### 9.2 Why omission ≠ open attach

If funnel **output is omitted** without convention, reads may confuse **peninsula**
(collector) with incomplete drawing. Convention:

- **Omitted:** no right-hand branch in packaged surface.
- **Open:** `Placeholder` / `Unknown` port present in structure.
- **Void:** explicit sink carrier with routing policy.

### 9.3 Orthogonal tension at open attach

`OrthogonalOutlet(source)` with unbound leg explores participation until bound.
Open attach makes that **explicit** for concept work:

```text
tension at outlet  =  “flow arrived, no receiver yet”
bind Placeholder   =  refinement
attach consumer    =  concept becomes funnel/nozzle/etc.
```

Input-side open attach (stable site before use) mirrors output-side: potential on
**input** orthogonal awaiting source.

### 9.4 Crossing without exchange

Two structures at co-site **without** connecting arrow:

```text
CoSite at t
regionA touches t
regionB touches t
no Relation between A and B regions
```

Reads as overlap / co-location until exchange relation added.

---

## 10. Partitions, ticks, and resolution

This section exists because **ticks**, **partitions**, and **resolution** are
often confused. They answer different questions.

### 10.1 Raw ticks (magnitude only)

```text
Carrier.Value = 3   // three steps on this carrier's tick shape
```

Without a **whole** definition, “3” is **not** “3 out of 10.” It is not proportional.
It is not probability. It is **count along the carrier's native tick**.

### 10.2 Partitions of a whole (proportional slots)

```text
WrapPartition(C, n):  Point(C, n) → Point(C, 0)
```

`n` says: **one whole** on this phase/contour is divided into **n buckets**.
“Half” lives in the middle bucket — proportional meaning exists because the **wrap
defines the whole**.

| Concept | Partition use |
|---------|----------------|
| **Switch** | 1 or 2 buckets — on/off |
| **Valve (coarse)** | 3 buckets — low / mid / high without numeric ranges |
| **Detent** | repeat + wrap — stable sites |
| **Contour tooth** | perimeter partition — countable closure |

### 10.3 Resolution ladder (nested wholes)

```text
ones:   Segment(ones,0,10) → tens
tens:   Segment(tens,0,10) → hundreds
```

Each level is a **span of a whole** at that grade. Reading only tens and hundreds
yields **390-ish** — partial resolution is honest uncertainty (axioms).

**Certainty** can track how many levels were read (Section 18).

### 10.4 UnitShape.ResolutionOf (measurement grain)

```text
UnitShape.ResolutionOf(source, N)
```

One tick of coarse shape = **N** ticks of source shape. Example: one tick = ⅛ inch.
This is **measurement refinement**, not concept partition.

### 10.5 Basis segment (read frame, not unit shape)

```text
Segment(C, 0, 10) → port
```

Exposes `2 7|10` style reads. The `|10` is **basis/resolution**, not a new unit
(axioms: resolution is not unit shape).

### 10.6 Concept partition rules

When building concept shapes:

| Need | Use |
|------|-----|
| Variable without precision | 2–3 **wrap partitions** on phase carrier |
| Part of named whole | wrap + contour calibration |
| Physical units | `ResolutionOf` / ladder on **measurement** carriers only |
| Presence | single level 0/1 — logical, not probabilistic |
| Angle as proportion of turn | partition perimeter into 360 (Section 12) |

### 10.7 Crystallized unit versus partition

Once a carrier is **crystallized** with a unit shape, partitioning **that** carrier
partitions the **entire tick meaning** of that grade — you are not mixing unrelated
units. If you need width×height area, preserve source structure until final read
(axioms: crystallized resolution).

---

## 11. Radial and contour calibration

### 11.1 Calibration ladder (point count)

| Points at origin | Structural read |
|------------------|-----------------|
| **1** | `RadialPotential` — outlet only, no radius, no spin |
| **2** (co-site) | collapsed segment — **pinch** to linear ray potential |
| **2** (radius + center) | `Point(r)→Point(p)` — radius without direction |
| **3** | radius + perimeter marker — **radius + rotation direction** |
| **+** wrap on perimeter | countable contour, detents, gear teeth |

### 11.2 Inflow and outflow (concept)

Inflow/outflow are **reads** on directed contour + projection:

```text
Inverse radial potential → neck   // inflow read
neck → forward outlet             // outflow read
```

### 11.3 Arc versus full perimeter

| Structure | Read |
|-----------|------|
| partial span on contour carrier | arc |
| wrap + partition on full contour | perimeter / closed loop |
| reset route on contour | countable closure |

### 11.4 Two-point pinch (segment from origin)

Two points at origin (co-located) force orthogonal of **zero segment** — linear
potential, not radial disk. This matches Potential Shape Fact Sheet: **point ≠
zero segment**.

---

## 12. Angles without angle primitives

### 12.1 Complex-like direction + length

```text
forward span  ×  inverse-face span
```

Directed participation without embedding θ. Length and direction live in **faces**.

### 12.2 Angle from partitioned circumference

```text
contour carrier C
WrapPartition(C, 360)   // or 2^n for binary refinement
marker Point(C, k)
angle between k1 and k2  =  difference read on partition indices
```

**No world zero required** — difference-only angles for relative geometry.

### 12.3 World-relative angle

```text
TickRef binds contour origin to world feature F
same partition read
```

### 12.4 Corner angle (unresolved)

```text
Corner with open angle slot:
  two SymmetricExtent from co-site
  claim Between(angleParameter, min, max)
```

**Sharp corner:** drive transition radius slot to 0 or omit fillet orthogonal.

---

## 13. Group fan versus radial field

| | Radial field | Group fan |
|--|--------------|-----------|
| Generator | G7 OrthogonalOutlet(Point) | G10/G11 Gather / Fan-out |
| Geometry | continuum around site | discrete branches |
| Multiplicity | potential | list |
| Typical words | funnel collector field, spray divergence | split, merge, bus, attachment hub |
| Mistake to avoid | calling OrderedGroup “radial” | calling radial outlet “a list” |

**Attach hub** (concept): `AdditiveGroup(holder, branchᵢ…)` — data at a point, not
isotropic potential.

---

## 14. Assembly: the concept shape record

### 14.1 Recommended package (ConceptSurface host)

```text
ConceptSurface(root) = OrderedGroup(
  root,
  selectedStructure,      // pointer into graph — read entry
  topologySurface,        // G1–G12 compositions
  flowSurface,            // participation laws + directed attachments
  parameterSurface,       // G16 slots
  constraintSurface,      // G17 claims
  exposedReadSurface,     // G20 — replaces “focus phenomenon”
  settlementWitness,      // G19 tension/residue/violation
  refinementTimeline      // G21 steps
)
```

### 14.2 What moved from older sketches

| Old field | Codex field |
|-----------|-------------|
| focus surface (phenomenon) | **exposedReadSurface** + `selectedStructure` |
| flow surface | participation laws + directed structure (not tags) |

### 14.3 Helpers (proposed API)

```csharp
ConceptShape.Create(...)          // build OrderedGroup
ConceptShape.TryReadTopology(...)
ConceptShape.TryReadExposedReads(...)
ConceptShape.TryReadOpenAttaches(...)
ConceptShape.TryReadWitnesses(...)
ConceptShape.TryReadTimeline(...)
// classifiers return suggested words — never own meaning
ConceptShape.TrySuggestWords(...) // optional interpretive layer
```

All fields **`IElement`**. No concept enums (per README near-term direction).

---

## 15. Extension and refinement

### 15.1 Refinement pipeline

```text
1. Seed concept shape      (generators only, many Placeholders)
2. Bind slots              StructuralPattern.Apply
3. Attach flow law         DirectedSegment / projection / claims
4. Expose reads            LiveOutput / ports — “focus”
5. Perturb                 Add/Remove tension, test settlement
6. Record witnesses        residue, violations
7. Timeline step           TimeStepSequence.Step
8. Harden optional         crystallize measurement carriers when needed
```

### 15.2 One-tweak refinement law

| Seed | One structural tweak | New read |
|------|----------------------|----------|
| Passage + free | bound claim on neck | filter / choke |
| Passage + free | directed law | conduit |
| Corner | transition orthogonal | fillet |
| Corner | radius → 0 | sharp corner |
| Funnel topology | omit outlet | peninsula |
| Funnel topology | open attach | dock / awaiting outlet |
| Bridge + read on neck | capacity claim | choke-capable |

### 15.3 Ordering commitment as refinement

```text
unordered SymmetricExtent
  → Step: attach DirectedExtent(C, a, b)
```

Recorded on timeline — it is a **decision**, not a new primitive object.

### 15.4 Valve versus switch

```text
Switch:   presence level 0/1 OR WrapPartition(C, 2)
Valve:    WrapPartition(C, 3)   // coarse variable, extend later
```

Extend by increasing partition count or adding `ResolutionLadder` levels — not by
renaming the concept enum.

---

## 16. Reads as focus (exposure, not a focus primitive)

### 16.1 Rule

**Focus is not a phenomenon primitive.** Focus is which subgraph is wired to an
**exposed read** or writable port.

```text
same topology + ExposeRead(neck, capacityPort)  →  choke-capable bridge
same topology + no exposed read on neck          →  mere connector
same topology + writable LiveOutput on neck    →  controlled flow device
```

### 16.2 Read-only versus writable depth

| Depth | Structure |
|-------|-----------|
| Notice | projection read / claim witness |
| Measure | bound parameter + repeated read |
| Control | `LiveOutput` + settlement route affecting neck |

### 16.3 Suggested word inference (optional layer)

Word suggestion inspects:

- topology class (junction, passage, corner, …)
- exposed read targets
- open attaches / omissions
- witnesses after perturbation
- timeline latest step

Returns **hints**, not authoritative labels.

---

## 17. Settlement, residue, and perturbation

### 17.1 Settlement laws (concept, not nouns)

| Law | Witness |
|-----|---------|
| **Pass** | clean projection; low residue |
| **Store** | tension retained at site |
| **Route** | tension forwarded to branch |
| **Cancel** | opposed faces fold before residue |
| **Latch** | claim + stable basins |

**Friction (composition):** contact + directed pass + **Store** on tangential attempt.

### 17.2 Perturbation contract

Concept shapes must be **mathematically perturbable**:

```text
carrier.Add(delta)
bind slot
move TickRef
tighten claim
```

Observe `Tension`, `TryProject` residue, claim violations. Incomplete states are
**information**, not exceptions (AGENTS.md).

### 17.3 Block versus filter

| Read | Structure |
|------|-----------|
| **Block** | projection fails; residue on source |
| **Filter** | matching basis passes; nonmatching remains residue |

Both are **projection + residue** patterns; no `BlockRelation` primitive.

---

## 18. Probability, certainty, and presence

### 18.1 Resolution depth as certainty

```text
more ladder levels read  →  higher numeric certainty
shallower read           →  coarser value, honest ambiguity
equation resolution search  →  evidence accumulation over branches
```

### 18.2 Partitions and probability

Partition **refinement** (3 → 12 → 360) narrows **which bucket** — useful as
evidence, not as Bayesian probability unless you add interpretive layer.

### 18.3 Presence basis (not probability)

```text
one level marker 0/1
filled / not filled
```

Logical presence — do not treat as 50% unless you add explicit probabilistic structure
(Section 22).

### 18.4 Global all / none

Quantifiers over a scope (“all branches”, “no branch”) need **scope structure** —
not in core today. List under missing structure.

---

## 19. Composition catalog

Words are **reads**. Generators are in Section 6.

### 19.1 Incidence family

| Word read | Generators |
|-----------|------------|
| site, anchor, pin | G1 |
| beam, link, route | G3, G5 |
| H-link, corner, T-junction | G2, G3, G10 |
| loop, orbit | G3, G14, co-site return |
| crossing (no exchange) | G2, no arrow between regions |
| crossing (exchange) | G2 + arrow |

### 19.2 Passage family

| Word read | Generators + laws |
|-----------|-------------------|
| neck | G3 between regions |
| bridge, connector | passage + G20 optional |
| choke | passage + ExposeRead(neck) + claim |
| filter | passage + projection residue |
| funnel | inverse G7 → neck → outlet G3 |
| peninsula | funnel − outlet (omitted) or G18 only |
| conduit | passage + directed law |

### 19.3 Radial family

| Word read | Generators |
|-----------|------------|
| radial field | G7 |
| pinched ray | G8 (two co-site points) |
| sheet, panel | G9 |
| fillet | corner + G7 calibrated |
| sharp corner | corner + radius slot → 0 |
| sprayer | funnel + divergent projection read |

### 19.4 Contact family

| Word read | Generators |
|-----------|------------|
| gap | G3 on clearance carrier |
| contact | G12 + gap |
| seat, stop | G12 + claim |
| clamp | G12 + G17 latch |
| bearing | contact + directed pass + store |

### 19.5 Accounting family

| Word read | Generators |
|-----------|------------|
| counter, wrap | G14 |
| detent | G14 + G17 latch |
| valve (coarse) | G14 with n=3 |
| switch | presence or n=2 |

### 19.6 Transfer family

| Word read | Generators |
|-----------|------------|
| scale, reducer | G13 + claim |
| expander | G13 inverted ratio |
| deliver | directed passage + G20 |
| accept | deliver + G17 store at receiver |

---

## 20. Refinement pairs (seed → one tweak → new word)

| # | Seed | One tweak | New read |
|---|------|-----------|----------|
| 1 | Passage free | directed law | conduit |
| 2 | Passage free | bound claim | filter |
| 3 | Passage free | ExposeRead neck | choke-capable |
| 4 | Corner | transition orthogonal | fillet |
| 5 | Corner | radius → 0 | sharp corner |
| 6 | Funnel | omit outlet | peninsula |
| 7 | Funnel | open attach | awaiting outlet |
| 8 | Junction | projection fan-in | merge read |
| 9 | Junction | projection fan-out | split read |
| 10 | Contact | latch claim | clamp |
| 11 | Wrap n=2 | third partition | valve coarse |
| 12 | Symmetric extent | ordering commitment | directed route |
| 13 | Radial potential | calibrated contour | countable disk |
| 14 | Open attach input | bind source | committed inlet |
| 15 | Void sink output | bind consumer | completed funnel |

---

## 21. Linguistic and metaphorical re-grounding

### 21.1 Descriptor rule

Algebra surface stays stable. **Descriptors** change context:

```text
bridge (mechanical)     descriptor: structure / steel / …
bridge round (finance)  descriptor: capital flow
bridge sentence         descriptor: rhetoric
filter (data)           descriptor: record basis
```

Rebind with `UnitShape.WithDescriptors` / `StructuralPattern.BindDescriptor`.

### 21.2 Metaphor must not hide shape

Metaphor replaces or augments **descriptor slots**, not silently retargets topology.
Failed metaphor transfer should leave **residue/tension** visible.

---

## 22. Missing core Gojo structure

This section lists gaps between the codex and comfortable expression in current
Gojo. Each item includes: **need**, **proposed implementation**, **usage**, **replaces**.

### 22.1 `UnorientedSpan(C, a, b)`

**Need:** Semantic unordered extent for `a ≠ b` without privileging `Segment(C,a,b)`
as “start at a”.

**Implementation:**

```csharp
// Gojo.Core or Gojo.ConceptSurface
static Relation UnorientedSpan(Carrier c, long a, long b) =>
  new Relation(Segment(c, a, b), Segment(c, b, a));
static bool TryReadUnorientedSpan(Relation r, out Carrier c, out long a, out long b);
```

**Usage:** corners, beams “between” points, symmetric links.

**Replaces:** ad hoc double segments; mistaken direction reads from single `Segment`.

**Extends:** G4 Symmetric extent generator.

---

### 22.2 `CoLocated(a, b)` without exchange

**Need:** Positive assertion of overlap **without** connection.

**Implementation:**

```csharp
static Relation CoLocated(IElement a, IElement b, TickRef site) =>
  OrderedGroup(holder, BindSite(a,site), BindSite(b,site));
// NO Relation between a and b regions
static bool TryReadCoLocated(...);
```

**Usage:** crossing without exchange; overlap; pre-connection alignment.

**Replaces:** accidental inference from shared numeric ticks alone.

**Extends:** G2 co-site.

---

### 22.3 `OpenAttach` and `VoidSink` helpers

**Need:** Distinguish open port, void sink, omission in concept docs and builders.

**Implementation:**

```csharp
static Relation OpenAttach(IElement source) =>
  OrderedGroup(holder,
    OrthogonalOutlet(source),
    new Relation(DescriptorSlot(0), new Placeholder()));

static Relation VoidSink(IElement source, Carrier residue) =>
  new Relation(source, Point(residue, 0)); // + policy: no consumer bind
```

**Usage:** peninsula, dock awaiting ship, unplugged outlet tension.

**Replaces:** ambiguous absence; confusing `Unknown` with “forgot to model”.

**Extends:** G18, G19.

---

### 22.4 `ConceptShape` OrderedGroup builder/reader

**Need:** Standard package from Section 14.

**Implementation:** `Gojo.ConceptSurface.ConceptShape` static class mirroring
`RepeatSurface` / `TimeStepSequence` patterns.

**Usage:** all concept work; tests for inspectability without helper names.

**Replaces:** hand-rolled groups per example.

**Extends:** ConceptSurface README catalog.

---

### 22.5 `UnorderedGroup` (optional, low priority)

**Need:** Bag semantics without sequence order — today `AdditiveGroup` is close but
reads as attachments to holder.

**Implementation:**

```csharp
// Marker relation: UnorderedBag(holder, elements...) 
// read: set equality, no order index
```

**Usage:** unordered parts list; symmetric tool palettes.

**Replaces:** misusing `OrderedGroup` where sequence is meaningless.

**Extends:** G10 gather.

---

### 22.6 Fill claim versus boundary-only

**Need:** Enclosed segment loop should not auto-imply area (Potential Shape §5).

**Implementation:**

```csharp
static Relation FillClaim(IElement boundary, Carrier fillCarrier) =>
  OrderedGroup(holder,
    boundary,
    OrthogonalOutlet(boundary),
    Point(fillCarrier,0));
static bool TryReadFillClaim(...);
```

**Usage:** plate with hole vs outline only; interior material optional.

**Replaces:** accidental “closure implies fill” readings.

**Extends:** G9 sheet potential + settlement witness.

---

### 22.7 Material transfer versus observation branch

**Need:** Fan-out where one branch **drains** and another **copies read** (Implementation Plan Phase 4).

**Implementation:**

```csharp
enum-less: tag via relation shape
  MaterialBranch:  route + settlement drain
  ObservationBranch: LiveOutput without draining source
static bool TryReadBranchKind(Relation branch, out BranchKind kind);
```

**Usage:** sensor tap vs splitter; broadcast vs consumption.

**Replaces:** one fan-out shape for two behaviors.

**Extends:** G11 fan-out.

---

### 22.8 Relation-shaped boolean surfaces (optional)

**Need:** AND/OR as **structure**, not only `BooleanStyleReads`.

**Implementation:**

```csharp
static Relation AndSurface(params Relation[] supports) =>
  OrderedGroup(holder, ...supports, tensionMergeRule);
```

**Usage:** guard conditions inside concept graph without external interpreter.

**Replaces:** interpretive-only boolean layer for concepts that must live in graph.

**Extends:** Section 5.10 reads — **does not remove** reads; adds optional structure.

---

### 22.9 Scope quantifier (all / none)

**Need:** “All branches satisfy claim” over a fan-out.

**Implementation:**

```csharp
static Relation AllBranches(Relation fanout, Relation claim) =>
  OrderedGroup(holder, fanout, claim, AggregateViolationCarrier);
```

**Usage:** global guards; safety interlocks.

**Replaces:** ad hoc loops in application code over branches.

**Extends:** G17 claims + G11 fan-out.

---

### 22.10 Explicit disconnectedness

**Need:** Assert two subgraphs are **not** connected (harder than omission).

**Implementation:**

```csharp
static Relation Separated(IElement worldA, IElement worldB, Carrier barrier) =>
  OrderedGroup(holder, worldA, barrier, worldB);
// barrier: claim that no route exists except through barrier port
```

**Usage:** islands, firewall, sealed regions.

**Replaces:** hoping absence of arrow is noticed.

**Extends:** G5 chains + claims.

---

### 22.11 Participation quadrant filter helper

**Need:** Enforce P+/P−/I+/I− policies without repeating claims.

**Implementation:**

```csharp
static Relation AllowQuadrants(Carrier c, ParticipationMask mask) =>
  claim composition on Point(c,n) attempts;
```

**Usage:** uint-only parameters; inverse-only receivers.

**Replaces:** duplicated `OperationClaim` patterns.

**Extends:** Section 4 four corners.

---

### 22.12 Suggested word read (optional, non-authoritative)

**Need:** Optional classifier for UX/docs/tests.

**Implementation:**

```csharp
static bool TrySuggestWords(ConceptShape surface, out string[] hints);
// pattern match topology + exposed reads + open attaches — NO enums stored
```

**Usage:** IDE labels; test fixtures.

**Replaces:** `ConceptUnitPrimitives` enums (deleted).

**Extends:** interpretive layer only.

---

### 22.13 Summary table

| ID | Structure | Priority | Replaces |
|----|-----------|----------|----------|
| 22.1 | UnorientedSpan | high | direction bias |
| 22.2 | CoLocated | medium | implicit overlap |
| 22.3 | OpenAttach / VoidSink | high | ambiguous absence |
| 22.4 | ConceptShape helper | high | hand-built groups |
| 22.5 | UnorderedGroup | low | OrderedGroup misuse |
| 22.6 | FillClaim | medium | auto-fill confusion |
| 22.7 | Branch kind | medium | fan-out ambiguity |
| 22.8 | Boolean surface | low | read-only booleans |
| 22.9 | Scope quantifier | low | app-side loops |
| 22.10 | Separated | low | silent disconnect |
| 22.11 | Quadrant mask | medium | repeated claims |
| 22.12 | TrySuggestWords | low | deleted enums |

---

## 23. Anti-patterns

1. **Concept enums** as source of meaning.
2. **`Carrier.ConceptShape`** field parallel to `UnitShape` without proof.
3. **Calling group fan “radial.”**
4. **Single `Segment` for unordered beam** — use symmetric or UnorientedSpan.
5. **Merging inverse face with scalar negative** in documentation or APIs.
6. **Treating three ticks as three-tenths** without wrap whole.
7. **Friction / bridge / sharp corner** as Tier-1 generators.
8. **Focus surface** as phenomenon without exposed read wiring.
9. **Omitting output** without omission convention — peninsula vs mistake.
10. **Crystallizing measurement too early** — losing width×height structure (axioms).

---

## 24. Appendix: quick reference tables

### 24.1 Generator → Gojo only

| Generator | Minimal Gojo |
|-----------|--------------|
| Site | `Point` |
| Co-site | `TickRef` |
| Directed extent | `Segment` |
| Symmetric extent | `Segment(t,t)` or pair |
| Radial potential | `OrthogonalOutlet(Point)` |
| Gather | `AdditiveGroup` |
| Fan-out | `OrderedGroup` / `EquationFanout` |
| Projection | `Segment→Segment` |
| Wrap | `Point(n)→Point(0)` |
| Open slot | `Placeholder` |
| Claim | `OperationClaim` |
| Exposed read | `LiveOutput` / port |
| Timeline | `TimeStepSequence` |

### 24.2 Participation corners

| Corner | Carrier | Site sign |
|--------|---------|-----------|
| P+ | forward | ≥ 0 |
| P− | forward | < 0 / tension |
| I+ | inverse | ≥ 0 on inverse |
| I− | inverse | < 0 / tension |

### 24.3 Absence

| State | In group? | Port? | Sink? |
|-------|-----------|-------|-------|
| Omitted | no | no | no |
| Open | optional marker | yes Placeholder | no |
| Void | route present | optional | yes |

### 24.4 Partition vs tick vs resolution

| Question | Answer mechanism |
|----------|------------------|
| How many steps? | tick count on carrier |
| What fraction of whole? | wrap partition n |
| How many digits? | resolution ladder |
| How fine per tick? | `ResolutionOf` |

---

## 25. Near-term implementation checklist

1. Add `ConceptShapesCodex.md` cross-link from `ConceptSurface/README.md` (one line).
2. Implement `ConceptShape.Create` / `TryRead` for Section 14 skeleton.
3. Implement **Corner**, **Passage**, **OpenAttach** as first composition tests.
4. Implement **UnorientedSpan** (22.1) before extensive catalog examples.
5. Test **sharp corner** = corner + radius slot perturbation → 0.
6. Test **peninsula** = funnel topology with omitted vs open output distinction.
7. Test **choke** = passage + `ExposeRead` on neck + claim.
8. Document participation corners in tests (P− vs I+ settlement footholds).
9. Defer **TrySuggestWords** until topology + exposed read patterns stabilize.
10. Keep all tests asserting structure remains inspectable when helper type removed.

---

## Document history

- Initial codex: consolidates ConceptSurface design sessions (math-only primitives,
  four participations, unordered structure, absence taxonomy, partitions vs ticks,
  reads-as-focus, missing core structure). Replaces enum-based ConceptUnits approach.

---

*End of Concept Shapes Codex*
