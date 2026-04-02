# Core4 Axiom System — Version 2

A mathematics bootstrapped from uncalibrated extents and 
their ability to react to each other.
No real numbers, no stated axes, no assumed dimensions.
Everything is constructed from these ordered magnitudes 
and a physical interpretation of them.

---

## Geometric Orientation

This is not an axiom — it is a guide to help readers orient themselves
geometrically as the system unfolds. The grades of this system correspond
loosely to familiar geometric objects, but with important differences.

**Pre-grade (uncalibrated extents).** Before any unit is formed, pinned
extents can compare ratios and contain ticks, but there is no agreed way
to read them — you don't know which extent is the denominator, so you
don't know the resolution, and there is no unit another element could use
as a measuring stick. Math is available here (comparison, ratio), but
these are not yet graded elements.

**Grade 0 — the "point."** In conventional geometry, a point is
0-dimensional and has no extent. In this system, grade 0 is an atomic
element: a ratio where one extent has been chosen as the unit. It is a
segment from that unit's zero to the end of the value — calibrated,
directed, and one-dimensional. A "point" here is already a measurement.

**Grade 1 — the "line."** Conventionally, a line has one degree of
freedom. In this system, a grade-1 element is a composite of two
independent grade-0 elements (recessive and dominant). It is a directed
segment with a start and an end — two independent points. This is why
grade-1 elements need both a recessive and dominant axis as a unit: the
line's identity is in the relationship between its two endpoints, not in
a single free parameter.

**Grade 2 — "area."** Formed by multiplying two grade-1 elements (lines).
The four-term kernel produces four orthogonal shapes from the unit origin:
`dd` (area creation) and `rr` (anti-area) lie on one diagonal, forming
area that extends from the origin. `rd` and `dr` lie on the other
diagonal, forming rays from the origin — these are complex numbers. The
two types of complex numbers (dominant-recessive and recessive-dominant)
are orthogonal values that can be thought of geometrically as pointing
"up" and "down" from the area plane.

**Grade 3 — "volume."** Formed by combining one of the rays (complex
numbers) with one of the areas. The only way to break into a new
dimension is to combine a symmetric object (area) with one that breaks
symmetry (a ray). Higher grades continue this pattern indefinitely,
though our intuitive spatial geometry ends at grade 3.

**Beyond geometry.** Geometry is a convenient visualization, but these
grades usually represent non-spatial quantities: weight and density,
temperature and pressure, items and repetitions, signal and noise. An
"orthogonal direction" simply means: the measurement system is receiving
information it cannot account for within its current dimensions. That
orthogonal direction can be *anything* until a different element is
discovered that accounts for it. The geometric picture is a scaffold,
not the territory.

**Note on the Engine:** This assumes an engine that is reacting to 
these numeric structures in some consistant way. The main assumption 
is if we define a direction, the flow tendency will be along that 
direction. There is a natural order defined, and we assume elements
can react to it. It still will be driven by amechanical increment 
force that will be modeled in the system, but that won't be directing.

A key aspect is 
*tension*, which this 'engine' tries to solve. In the real world 
the engine is often physics, causing rocks to roll, or whirlpools
to swirl to reduce their tension. As we move to intelligence 
problem solving becomes more abstract, but the goal is the same - to 
reduce tension. The system here is mathematical, so it uses structure,
values, and laws to solve tension. These are not created in the abstract
however, they are derived from the same forces that create the tension 
in the first place. There is a gradient from entents to numbers to 
dimensions and with data and scale, it starts to resemble behavior.

---


## Part I: Ground

### Axiom 1 — Extents and Chaos

There exist **extents**: ordered, comparable magnitudes. An extent has
no unit, no resolution, no sign. It is simply "some amount" that can be
compared to another extent by size. This is the sole assumption.

Not everything is an extent. Some phenomena are **chaotic** — not orderable,
not comparable. The system can only observe these; it cannot operate on them.
They appear as irreducible tension, background noise within the resolution
of whatever structure is present. One purpose of the system is to discover
frames that render previously chaotic phenomena orderable — but that is
never guaranteed, and some residual chaos is always acceptable.

We write an extent as a bare value: `a`, `b`, `c`.

### Axiom 2 — Signs from Endpoint Attachment

Extents have no sign. Signs arise from **how extents join at their
endpoints.** Each extent has a start and an end. When two extents meet
at a point, there are four possible configurations based on which
endpoints are shared:

```
    ── ──        start-start    both extend away    ALIGNED (++)
    ── ──        end-end        both extend toward   ALIGNED (--)
    ──── ──      end-start      first flows into second  OPPOSED (+-)
    ── ────      start-end      second flows into first  OPPOSED (-+)
```

**Aligned** configurations (start-start or end-end) produce extents
facing the same direction from the join point. There is no tension
between them — they reinforce.

**Opposed** configurations (end-start or start-end) produce extents
facing opposite directions from the join point. This opposition IS
**negation** — one extent undoes the other's direction. This is where
the concept of negative comes from. Not from a number line, but from
the physical fact that two extents can face opposite ways.

There are only **two** physically distinct configurations — aligned and
opposed. The two aligned cases (++ and --) are the same relationship
viewed from opposite sides, as are the two opposed cases (+- and -+).

**Opposition has two modes**, like all elements in this system:
- **Recognition:** you can *read* that two extents are opposed — observe
  their negation without acting on it
- **Action:** you can *cause* negation — attach in opposition to reverse
  a direction

This distinction between aligned and opposed attachment is the foundation
of much that follows: it gives rise to the sign field in atomic elements,
the aspect trichotomy (Axiom 10), the distinction between value and
structure, and ultimately to dimensional emergence.

### Axiom 3 — The Atomic Element
The conversion of the denominator to a unit (Axiom 13) changes the ratio 
into a number. It is a rational number where the denominator determines
the resolution, and the numerator can be positve or negative. They are 
built using 'ticks' which are extents, now with direction. We call them
signed integers here for brevity.

An **atomic element** is a pair `(v, u)` where:
- `v` is a signed integer: the **value** (how many)
- `u` is a signed integer: the **unit** (what "one" is)

The signs come from endpoint attachment (Axiom 2). When an extent is
chosen as the unit, the attachment configuration determines the unit's
sign. A negative value means the measurement opposes the unit's direction.
A negative unit means something structurally different: **orthogonal
tension** — the unit is in opposition to its context, which creates a
force that organizes rather than measures (see Axiom 10).

This distinction — negation in the value vs negation in the denominator
— persists at every grade. At grade 0, a negative value is a reversed
measurement; a negative unit is an orthogonal structural force. At higher
grades, the same principle applies through every level of composition.

The unit magnitude `|u|` is the **resolution**: how many ticks compose
one unit. When `u = 0`, the element has magnitude but no established
"one" — it is **unresolved**.

An atomic element is **grade 0**: the leaf of all structure.

### Axiom 4 — The Composite Element

A **composite element** is a pair of two equal-grade elements in
opposition:

    Composite(recessive, dominant)    where recessive.grade == dominant.grade

The composite's grade is `children's grade + 1`.

- **Recessive**: the reference side, the context, the "before"
- **Dominant**: the applied side, the measured, the "after"

**Why opposition is required:** Two elements with aligned, same-sign
denominators are in the same space — combining them adds within that
space but does not create a new grade of structure. To build a composite
that represents a genuinely new structural level, the two children must
carry orthogonal tension between them: the recessive provides the
reference frame, and the dominant provides the measurement against it.
This tension between context and content is what makes a composite more
than the sum of its children.

This is the only way to build higher-grade structure. A grade-1 element
has two atomic children. A grade-2 element has two grade-1 children
(four atomic leaves). Grade N has 2^N atomic leaves.

### Axiom 5 — Ordering and Direction

Every composite has inherent direction: recessive → dominant.
This is not a convention. The recessive provides context for the dominant.
Swapping them (SwapOrder) changes the physical meaning.

Direction gives every composite a natural "before" and "after."
Traversal from recessive to dominant is the most primitive form of time
(see Part VIII). No external clock is needed, just the tendancy to flow
from beginning to end.

---

## Part II: The Lifecycle

Every event in this system — from the first unit bootstrapping itself out
of uncalibrated extents, to dimensional emergence, to a pin firing during
traversal — follows a universal pattern. The pattern has four phases,
visible from two complementary perspectives: the **structural** perspective
(what happens to the structure) and the **observer** perspective (what
happens to the participant element moving through the structure).

Any event in the system is one of:
1. **A single phase** within a larger lifecycle
2. **A complete lifecycle** instance
3. **A composite event** built from multiple lifecycles

### Axiom 6 — The Four Phases

Each phase is described from both perspectives. The structural view
names the mechanical event; the observer view names the experiential
state. They are the same event seen from outside and inside.

**Phase 1 — Observe / Attach**

*Structural:* A connection forms. This has two sub-steps, and the first
is meaningful on its own:

> **Pin (to a single extent):** A pin attaches to one extent at an
> endpoint. This alone gives the pin a **location** (here), a **direction**
> (one way to go along the extent), and the **potential to interpolate**
> along that direction. Endpoint attachment is the primitive — midpoints
> cannot be discovered without calibration, so a midpoint is just somewhere
> along the interpolation from an endpoint, not a directly discoverable
> position.
>
> At this stage — pinned to one extent — the observer already has
> meaningful capabilities: a position, a direction, the potential to
> traverse, and a perspective (the world as seen from this location).
>
> **Join (pin to a second extent):** The pin attaches to a second extent
> without releasing the first. Now there are **two direction opinions**
> from a single location. These opinions are either aligned (same
> direction) or opposed (opposite directions). This is comparison — not
> yet measured, but structurally present as a ratio.

*Observer:* This phase is entirely **passive**. The element observes
endpoints, sees direction, recognizes that interpolation is possible.
It can attach more closely to see more — but it is not acting on
anything, only receiving information. The potential to interpolate exists
from the moment of the first pin, but exercising it is a separate choice
that belongs to the next phase. Joining a second extent enriches what
can be observed (now there is comparison), but does not change the
passive character of this phase.

The observer at this stage has: a position, a view of the world from
that position (perspective), and one or two directions it could go. It
has not yet gone anywhere.

**Phase 2 — Experience / Traverse**

*Structural:* The interpolation potential is exercised. The element moves
along the attached extent, producing an ordered sequence of positions.
This is traversal — the structure is being read, generating feedback at
each position.

*Observer:* The element **experiences** the extent by moving through it.
This is like running an equation — at each position, the structure
provides feedback (what is here, how does this compare to where I was).
The experience is informational, not transformative: the element is
learning about the structure, not changing it.

The drive to interpolate is not innate in the observer. It requires
**structure** to achieve — either external structure pushing the element
through (a mover advancing along a route), or internal structure within
the element itself (an internal mechanism invisible to the element, much
as our cells are invisible to us). Either way, the element experiences
traversal; whether the impulse is external or internal does not change
what is experienced.

If the element interpolates along the extent, it gains **time** — an
ordered sequence of positions. If it interpolates past the end, it gains
**tension** — the structure has been exceeded. If a second extent is
attached (Join), the element can experience both directions: interpolating
along one while using the other as reference, or interpolating through
itself from one extent's endpoint to the other's.

**Phase 3 — Manipulate / Negotiate**

*Structural:* The ability to compare (from Observe) and the feedback
from traversal (from Experience) together imply the potential for
**change**. Laws emerge from abilities already present — they are not
imported from outside the system:

- **Track:** Use one extent to measure changes in the other
- **Repeat:** Re-attach to the start when you reach the end
- **Stretch:** Adjust the rate of interpolation (change the ratio)
- **Negate:** Reverse direction
- **Halt:** Recognize when combined potential is exceeded; create tension

*Observer:* The element can now **manipulate** — actually modify extents,
adjust directions, negotiate changes to be more in line with its needs.
Where Experience was reading the structure, Manipulate is writing to it.

Every ability has two modes that map to Experience and Manipulate:
**recognition** (you can see direction, see extent, see rate) and
**action** (you can change direction, modify extent, adjust rate). You
had recognition from Phase 2; you gain action in Phase 3. You can
attach to things to observe them (Experience) or attach to things to
accumulate them (Manipulate).

The negotiation determines how the joined extents will relate. It has
limits. When combined potential is exceeded — for example, one axis
repeats 3 times and the other stretches to twice its length, giving a
combined reach of 6; beyond that, the shared system produces tension it
cannot absorb — the negotiation records this as structural tension, not
failure.

**Phase 4 — Resolve / Commit + Independence**

*Structural:* Two things happen in sequence:

> **Commit:** The negotiated structure is locked. Specifically, the
> **denominator** is locked — the unit, the "what is one," the tick
> structure. The values that participated in negotiation become
> measurements *within* the new unit, not parts of the unit itself.
>
> This is always a denominator lock, even at higher grades. A vertical 3
> and a horizontal 4 combine into an area of 12, but the unit created is
> 1² (one-squared), not 12. The 3 and 4 become values measured by that
> unit. The unit commits to not changing size even if the values that
> created it later adjust themselves. Units are local — they are bound
> to the place and structure where they were committed.
>
> **Independence:** The committed unit separates from the values that
> created it. It becomes a **symbol**: a portable, self-sufficient entity
> that can calibrate other things. The unit transcends its construction —
> area is more than two segments, speed is more than distance and time.
> The values remain as a remainder or reattach elsewhere; the unit is free.

*Observer:* The element reaches a **resolution** — a stopping point. The
negotiation is complete. The structure is committed. The element is now
*available for others to observe*. Its Independence is the beginning of
other elements' Observe phase — they can attach to this resolved symbol,
experience it, and eventually manipulate in relation to it.

This is the mechanism by which the system creates abstractions. A symbol
carries the memory of its formation (direction, resolution, structure)
but has no ongoing dependency on the specific values that formed it.
A different value can attach to the unit and be measured by it. The symbol
can travel to other parts of the structure and serve as a reference there.

### Axiom 7 — Abilities Accumulate

Abilities unlocked at any phase remain available in all subsequent phases
and in all subsequent lifecycle instances at higher grades. Nothing is lost.
Crucially, abilities gained early may not be *exercisable* until a later
phase provides the environment to use them — you had the concept of
interpolation from Observe, but couldn't use it richly until Experience
provided traversal and Manipulate provided modification.

After **Pin** (to one extent): location, direction, interpolation
potential, perspective (the world seen from this position).

After **Join** (to a second extent): comparison, two direction opinions,
ratio, the distinction between aligned and opposed. All Pin abilities
remain and are now richer — you had one direction, now you have two.

After **Experience/Traverse**: time (ordered positions), feedback (what
is at each position), tension awareness (when the structure is exceeded),
rate (how fast positions change). The earlier abilities (location,
comparison) are now exercisable in a richer environment — you had the
concept of interpolation before, but now you have actual experience of
moving through structure.

After **Manipulate/Negotiate**: change, laws (track, repeat, stretch,
negate, halt), tension boundaries, rate adjustment. All earlier abilities
(location, comparison, traversal feedback) are available as inputs to
manipulation. Recognition becomes action.

After **Resolve/Commit**: unit, measurement, denominator stability.
Comparison now becomes measurable comparison (numbers, not just ratios).
Interpolation now has resolution (ticks). The element is complete and
available for others.

After **Independence**: symbol, portability, calibration of others.
Everything from prior phases is available *and* the symbol carries a
compressed version of all of it.

### Axiom 8 — The Reverse Lifecycle

The lifecycle runs backward for decomposition:

**Capture:** A free-running symbol is bound — attached to a specific
context. Another element observes and attaches to the resolved symbol.
(Reverse of Independence.)

**Cleave:** The committed structure is broken — the denominator lock
is released, exposing the internal negotiation. The manipulation is
undone. (Reverse of Commit.)

**Separate:** Values are distinguished from units by sorting — the
negotiated relationships are unwound into their component experiences.
(Reverse of Negotiate.)

**Disconnect:** The pin releases. Two previously joined extents become
independent again. The observation ends. (Reverse of Attach.)

In mathematics: taking a square root is capturing an area symbol, cleaving
its unit, separating the axis values, and disconnecting them into
independent extents. Factoring is the same pattern applied to multiplication.
Analysis — in the broad sense — is the reverse lifecycle.
Importantly, an area created from a rectanglular area generating machine
can be captured and run in reverse through a circle generating machine,
and result in a the component/s needed to generate a circle. A symbol 
does not retain its provenance, so symbol creation is lossy and decomposing
is context dependent.
### Axiom 9 — Aligned and Opposed Directions

When two extents join at a point, their directions from that point are
either **aligned** (same direction) or **opposed** (opposite directions).
This distinction is structural — it exists before any measurement, and
is observable from the first phase.

**Aligned directions** create parallel flow. Two streams going the same
way. They cannot create a new dimension (there is no tension between them
that demands one), but they create something a single stream cannot:
parallel force, co-travel, the concept of relation between two co-directional
flows. This is the seed of structure within a dimension. An element
experiencing both sees two parallel interpolation paths — it can traverse
either, or both simultaneously, learning about the space between them.

**Opposed directions** create orthogonal potential. From the join point,
there is a *third* interpolation path that didn't exist in either extent
alone: starting at the recessive endpoint (which pulls towards the origin), 
and interpolating through the origin the the dominant endpoint (which is 
pushed away from the origin). 
This path crosses both extents and requires acknowledging both directions.
This is an example orthogonal forces create new dimensions that can explore 
spaces that were previously unavailable. The tension between 
opposed directions generates a path that neither extent contains by itself.

An element at the join point of opposed extents, if it chooses to
experience the third path, is performing the most primitive dimensional
promotion. It is discovering a direction that neither extent alone contained.

---

## Part III: The Three Aspects

### Axiom 10 — Unit Sign Determines Aspect

The sign of an atomic element's unit determines its **aspect** — its
physical role in any encounter:

| Unit sign | Aspect | Meaning |
|-----------|--------|---------|
| `u > 0` (positive) | **Value** | Aligned. Shares a carrier with its context. A calibrated measurement in a known space. "One" is established. |
| `u < 0` (negative) | **Structure** | Orthogonal. A force acting on the context without being the context. Organizes, constrains, imposes a dimension. Does not contribute magnitude — contributes arrangement. |
| `u = 0` | **Generative** | Unresolved. The relationship between this element and its context hasn't settled. Has magnitude but no "one" to measure against. Between unit spaces. Demands resolution through a law. |

These are not labels applied externally. They are readable from the element
itself — observable in Phase 1, before any experience or manipulation.

### Axiom 11 — Aspects Are Relational

An element's aspect is not intrinsic — it depends on the frame through which
it is read. The same element can be value in one context, structure in another,
and generative potential in a third. What determines its role is the
relationship, not the element alone.

A composite frame has leaves with potentially mixed signs. Each leaf
contributes its own aspect independently. A single frame can simultaneously
calibrate values (positive leaves), organize structure (negative leaves),
and demand generation (zero leaves) at different positions in its tree.

### Axiom 12 — The Multiplication Table of Aspects

When two elements interact through multiplication or scaling, their unit
signs multiply. This determines the aspect of the result:

    (+1) × (+1) = +1    Value × Value = Value
    (-1) × (-1) = +1    Structure × Structure = Value
    (+1) × (-1) = -1    Value × Structure = Structure
    ( 0) × ( n) =  0    Generative × Anything = Generative

This is not metaphor. It is the actual arithmetic performed on the unit
field during `Scale` and `Multiply`. The engine computes aspect transitions
through ordinary integer multiplication of unit signs.

Key physical implications:
- Two orthogonal forces meeting produce something aligned (cross product → scalar)
- A value applied through structure produces structural position
- Anything touching the unresolved stays unresolved — generation absorbs

---

## Part IV: Bootstrap — The First Lifecycle Instance

The number system is the first complete run of the lifecycle.

### Axiom 13 — Ratios from Extents (Observe Phase)

Take two uncalibrated extents and **join** them in a composite:

    Composite(a, b)

This is the Observe phase: two extents are pinned together. The result is
a **ratio**: "a relative to b." It is meaningful even though neither `a`
nor `b` has a unit. No measurement has been made. A relationship has been
created. This is the first structural act.

The ratio does not have resolution — neither child knows what "one" is.
But the relationship `a/b` is real and comparable to other ratios.

At this point the abilities from Observe are live: location (within the
ratio), direction (recessive → dominant), the potential to interpolate
between the two extents, and comparison (their relative sizes). An element
sitting at this ratio can see both extents and their relative magnitudes.
It has not yet experienced traversal through either.

### Axiom 14 — Unit Formation (Experience + Manipulate + Resolve)

The ratio can **fold**: the Experience, Manipulate, and Resolve phases
run in sequence.

    Fold(Composite(denominator, numerator)) → AtomicElement(v, u)

**Step 1 — Experience gives direction (Phase 2).** The unit-to-be
connects to the value at one endpoint and experiences it — traversing
enough to discover the value's direction. Before this, the unit-to-be
is directionless — just an extent with magnitude. The attachment point
determines the sign relationship. If the unit-to-be and value face the
same direction from the attachment point, the unit is **aligned**
(positive). If they face opposite directions, the unit is **opposed**
(negative). There are two physically distinct configurations; the other
two apparent configurations are the same situations viewed from the
other side.

The drive to traverse may come from external structure or from internal
structure invisible to the unit-to-be itself. Either way, the experience
happens and direction is discovered.

**Step 2 — Manipulation negotiates "one" (Phase 3).** The value provides
calibration: *where* the attachment point sits, and its own natural
direction. The unit-to-be provides resolution: what "one" will mean, the
tick structure. Together they manipulate — actively adjusting — until they
agree on a shared version of 1. 

>In reality this may be enacted by physics or biology.
>In this system it is future machines made of these values, 
structures and laws. 'Averaging' may be a good first aproximation. 

The laws used in this negotiation are
the abilities already available: comparison, ratio, direction, plus
the feedback gained from experience.

**Step 3 — Resolution commits the unit (Phase 4).** The unit-to-be
commits to **not changing size**. It becomes the fixed denominator. This
is irreversible — the unit now carries direction (from Step 1), magnitude
(from its original extent), and the agreement about what "one" means
(from Step 2). The unit separates. It retains the information it received
from the value (direction, attachment point, resolution), but the value
itself could detach. A different value could join. The unit has become a
measuring stick that remembers its formation but has no ongoing dependency
on the specific value that formed it.

The fold operation `ComposeRatio` performs this entire sequence:
- The value `v` encodes the numerator's magnitude scaled by the denominator's resolution
- The unit `u` encodes the resolution, derived from the denominator's magnitude

The atomic case is special: the full value does not survive into the final
unit. It donates calibration information (position, direction) and some of
that lives on in the unit nondestructively, but the value field is then
free for other use. All units — at every grade — form through these same
steps.

### Axiom 15 — Numbers from Alignment (Others Observe the Resolved Unit)

Once a unit exists and is independent, other elements can be **aligned**
to it:

    element.CommitToCalibration(unit) → calibrated element

Alignment reexpresses an element in terms of the unit's resolution.
The element becomes a **number**: a value expressed in terms of the
self-declared "one." Numbers are born from units, not the reverse.

This is the resolved unit being **observed by other elements**. Its
Resolve/Independence phase is their Observe phase. The unit is a symbol;
alignment is what happens when another element attaches to that symbol
and uses it as a frame of reference. The lifecycle feeds forward.

The complete bootstrap lifecycle:
1. **Observe:** Pin two extents → a ratio (Axiom 13)
2. **Experience:** Traverse to discover direction (Axiom 14, Step 1)
3. **Manipulate:** Negotiate a shared "one" (Axiom 14, Step 2)
4. **Resolve:** Lock denominator, unit becomes independent symbol (Axiom 14, Step 3)
5. Others observe the symbol → numbers (Axiom 15)

From "ordered magnitudes exist" alone, the entire number system is constructed.

---

## Part V: Dimensional Emergence — The Second Lifecycle Instance

Dimensions are the second lifecycle running at grade 1 and above.
The pattern is the same: observe, experience, manipulate, resolve.
But the objects are richer (directed segments, not bare extents),
so the negotiation produces richer structures (area, complex numbers,
higher dimensions).

### Axiom 16 — The Denominator Is "What Is One"

The unit field of an atomic element is literally "what one is."
When two elements sit in a composite with independent units, each retains
its own "one." They are co-located but not fused.

**Co-location** (Observe phase): Two elements in a composite, each with
its own unit. A width AND a height. Two measurements. Independent
denominators. Either side can be reexpressed without affecting the other.
An observer can see both but they are not yet one thing.

**Fusion** (Resolve phase): The two units combine into a single "one"
that spans both. Now you have area. The numerators still vary freely
(measurements within the new space), but the denominator is locked. You
cannot reexpress one side without affecting the other. The fused entity
transcends its construction — area can be circular, not just rectangular.

The transition from co-location to fusion IS a lifecycle run: the two
units are observed together (co-locate), experienced through traversal,
manipulated (compare and adjust their "ones"), and resolved (lock a
shared denominator). The fused entity becomes independent — area as a
concept, not just width-times-height.

### Axiom 17 — The 1×1 Requirement and Degrees of Freedom

For dimensional fusion to produce a coherent space, the unit cell must be
**1×1**: each axis contributes exactly one of its own measure.

**Why:** Predictable scaling under interchangeable degrees of freedom.

Every fused entity has exactly **one degree of freedom** — it is one thing.
Speed is one thing. Area is one thing. But the *tools* used to construct
fused entities may expose more than one adjustable axis (a rectangle has
width and height). The 1×1 requirement governs what those tools may do.

**When the fused axes are interchangeable** (both inches in a square-inch
unit cell): the construction tool gets **multiple degrees of freedom**.
You can scale width or height, and the area changes predictably without
needing to know which axis changed. The axes are symmetric with respect
to scaling — "I tripled one side" fully determines the new area. This
extra freedom in the tool is only possible because the axes are
indistinguishable within the fused unit.

**When the fused axes are not interchangeable** (miles and hours in a
speed unit): the fusion is equally real — speed is a genuine entity, not
two measurements sitting side by side. But the fusion itself **imposes a
restriction**: values may only attach freely to one axis. The tool gets
exactly **one degree of freedom**. You cannot drag the hours around
independently without destroying the concept of speed. The denominator
axis (hours) is locked to one, and only the numerator axis (miles) is
free. This restriction is not external — it is the cost of fusing
non-interchangeable components into a single entity.

**The non-fused alternative** is co-location without fusion: "I drove 80
miles in 2 hours." You can compare such ratios — 20 miles in half an hour
gives an equivalent ratio — but you do not have a *concept* of speed.
There is no fused entity, no single degree of freedom, no unit that
persists independently of the specific values that created it. You are
stuck describing instances. Fusion is what turns a ratio into a thing.

**The 1×1 cell** is the mechanism that makes all of this work. Each axis
contributes exactly "one of itself." When the axes are interchangeable,
this makes them symmetric in the tool. When they are not, this is what
forces one axis to lock (it contributes its one, and that one is fixed).
In both cases, the fused entity has predictable behavior: adjusting any
*available* degree of freedom produces a result you can compute without
additional information about which axis moved.

**Consequence:** Denominators must be settled before fusion can happen.
If either axis has a free denominator (u = 0), you cannot know what "1"
means for that axis, and therefore cannot construct the 1×1 cell.

    Free denominators = structure (not ready for fusion)
    Fixed denominators = ready for fusion
    Combined denominator = fused entity

In the engine: `Scale` multiplies `Unit × factor.Unit`. The product is the
resolution of the fused space. Each input contributes its full resolution
as its "one." The 1×1 cell emerges from ordinary arithmetic.

### Axiom 18 — Tension as Incomplete Fusion

When two elements attempt to fuse (through fold or multiply) and cannot
agree on a shared "one," the result carries **tension**:

    EngineElementOutcome { Result, Tension, Note }

- `Result`: the best lawful projection (what the system can produce)
- `Tension`: the unresolved structure (what couldn't settle)
- When `Tension` is null, the outcome is **exact** (fusion complete)

Tension is not failure. It is a lifecycle that hasn't reached Resolve —
the Manipulate phase produced structure that cannot be committed. From the
observer's perspective, tension is the experience of *almost* resolving
but not quite — the structure remembers what it was trying to become.

Tension is never discarded. It propagates through operations via
`CombineTension`. The unresolved relationship remains inspectable and
available for later resolution.

### Axiom 19 — Directed Segments (Grade 1 Unit Cell)

At grade 0, all elements are atomic. To create grade-1 structure with
genuine **dimensional tension**, you need one aligned (+) and one
orthogonal (-) unit to combine. This is the only configuration that
creates dimensional pressure — two positives just add within the same
space; a positive meeting a negative is a genuine encounter between
incompatible orientations.

There is only **one** orthogonal direction available at grade 0: the
opposite sign. So the unit cell that emerges is:

    Composite(AtomicElement(1, -1), AtomicElement(1, +1))

One step in the negative direction, one step in the positive direction.
This is a **directed segment**: an extent with a start and an end.
Not a rotation — a directed extent. Something with "from" and "to."

A directed segment can measure anything with magnitude and polarity
along a single quality: a time interval, a temperature range, a price
spread, a potential difference, a gradient. The two directions need not
represent the same physical quantity.

**A directed segment is not yet a complex number.** A directed segment has
two positions (start and end). A complex number has magnitude and angle
from a privileged origin. The distinction matters (see Axiom 22).

### Axiom 20 — The Four-Term Kernel (Grade 2)

When two grade-1 elements (directed segments) multiply, the four-term
kernel appears. Each composite has a recessive (r) and dominant (d) child.
The multiplication produces four terms:

    rr = left.Recessive × right.Recessive
    rd = left.Recessive × right.Dominant
    dr = left.Dominant  × right.Recessive
    dd = left.Dominant  × right.Dominant

It's worth reiterating exactly what a unit promises with two axes. They are 
mechanically bound in their formation, so they 'follow' each other by extending
into each other's space. That is most obvious in area, but if area isn't possible
they will still try to align to each other, resulting in a line that extends into
both spaces. They follow both value and structure, which may be positive or negative,
and dominant or recessive.

Each term has a physically distinct character determined by the dominance
pattern of its components:

| Term | Dominance pattern | Physical character |
|------|-------------------|--------------------|
| `dd` | Both expanding (dominant × dominant) | **Area creation.** Two outward pushes meeting orthogonally. The space between opens up. |
| `rr` | Both contracting (recessive × recessive) | **Anti-area.** Two inward pulls. The structural opposite of area creation. |
| `rd` | One contracting, one expanding | **Line.** Expansion and contraction cancel dimensionally. Net dimensional contribution: zero. Pure directed extent to a corner point. |
| `dr` | One expanding, one contracting | **Line.** Same cancellation, different order. |

The engine resolves this kernel into two components:

    squareDifference = rr + dd     → net area (aligned result)
    crossSum         = rd + dr     → net directed extent (orthogonal result)

**Both equations are addition.** The traditional formulation writes
`rr - dd` because conventional mathematics only acknowledges positive
area — it needs an explicit subtraction to account for cancellation.
In this system, `rr` IS anti-area: structurally negative, the opposite
of `dd`. Note: This may seem like a strange concept, but it can be as simple 
as previous area and future area. If you mow a lawn, start at the recessive 
end point, get to present, and plan to finish at the dominant end point.
The total area is the lawn, and it has completed (anti-area) and todo (future area).
Adding anti-area to area cancels naturally, the same way
adding a negative number to a positive one does. The subtraction is
in the structure of `rr` itself, not in the equation. The operation
is always addition; the signs are carried by the terms.

This IS complex multiplication. It was not introduced — it **falls out**
of the grade system. The "real part" (rr + dd) is the net area contribution.
The "imaginary part" (rd + dr) is the net directional residue.

The four-term kernel is what the observer experiences when two directed
segments are multiplied: four distinct interactions, each with its own
physical character, resolving into area and direction.

### Axiom 21 — Dimensional Promotion Through Symmetry Breaking

New dimensions are not stated. They **emerge** from tension that cannot
be expressed at the current dimensionality. The recipe is recursive and
each step is a lifecycle instance:

**0D → 1D:** Two uncalibrated extents are **observed** (joined). Their
directions are opposed — the observer experiences tension between them.
**Manipulation** resolves to a directed extent. **Resolution** locks the
directed segment as a unit cell. The segment is independent — direction
emerges from the incompatibility between the two extents.

**1D → 2D:** Two directed segments are **observed** together (multiply).
The observer **experiences** the four-term kernel — four distinct
interactions. **Manipulation** cannot reduce all terms to 1D lines:
- `dd` and `rr` terms are area-like — they represent enclosed space
  that has no 1D expression. The excess IS area.
- Area emerges specifically when both components are co-directional in
  dominance (both expanding or both contracting)
- `rd` and `dr` terms cancel to lines (the complex number part)
**Resolution** locks the area unit. The area is independent — a fused
entity that transcends the segments that created it.

**2D → 3D:** Area has a natural flow (start size → end size). It is
**observed** alongside a line that **breaks the 2D symmetry**: the line's
dominance pattern is incompatible with the area's natural
expansion/contraction. The observer **experiences** volume — space swept
by an area along a symmetry-breaking line. **Resolution** locks the
volume unit. Volume is independent.

**General rule:** Dimension N+1 = Dimension N entity × symmetry-breaking
orthogonal extent. "Symmetry-breaking" means the new extent has a
different dominance pattern than the existing entity, forcing a genuinely
new dimensional direction.

**The search space collapses.** A machine navigating this system does not
explore arbitrary stated axes. It follows the forced path of tension
resolution. The only new dimensions that appear are those demanded by
unresolvable tension at the current grade. Dimensions are consequences,
not assumptions.

### Axiom 22 — Corner Resolution and Complex Numbers

When an orthogonal value and an aligned value meet at a corner, the corner
carries tension. Resolution depends on the dominance combination:

**Mixed dominance (rd or dr):** One axis expands, the other contracts.
They cancel dimensionally → the result is a **line** from the origin to the
corner point. The origin-anchoring comes from the cancellation of area.
This IS a complex number: a directed extent from a privileged origin.
The transition from directed segment to complex number is precisely this
corner resolution where area cancels to a line.

**Matched dominance (dd):** Both axes expand → **area** emerges.
Both are trying to create enclosed space.

**Matched recessiveness (rr):** Both axes contract → **anti-area** emerges.
Both are trying to destroy enclosed space.

Dominant area and recessive area combine to form an aligned area with
start and end — a directed area, analogous to a directed segment but
one grade higher.

The complex number (line from origin) combined with area can produce
volume — a variable-length line sweeping through a variable area.
The symmetry break (line's dominance vs area's dominance) is what forces
the third dimension into existence.

---

## Part VI: Structure — Lifecycle Composition

Structural topology is what happens when multiple lifecycles compose.
A pin is a lifecycle site. A segment is the span between lifecycle sites.
A network of pins and segments is a network of lifecycle instances
interleaving and feeding into each other — each resolved symbol becoming
the observed extent of the next lifecycle.

### Axiom 23 — The Pin

Every structural transition happens at a **pin**. A pin is a lifecycle
site: the place where observation, experience, manipulation, or resolution
occurs.

A pin on a **host** (a composite span) splits the host at a position:

    Pin(host = Composite(start, end), position)
    → Inbound  = position - start
    → Outbound = end - position

The pin position is a value. The host is structure. The inbound and outbound
are **generated**: new elements that didn't exist in the inputs. The pin is
the site of generation.

A pin at the intermediate (zero-denominator) state is in the Manipulate
phase — the old units have dissolved and the new unit hasn't resolved yet.
This is where the law acts.

### Axiom 24 — Merge and Branch

There are exactly two topological events. Everything else is traversal
along a segment between pins.

**Merge:** Two inbound segments → pin → one outbound. This is a
**forward lifecycle**: two things are observed together, experienced
through the kernel, manipulated through a law, and resolved into one
independent result. Multiplication, addition, fold, and every combining
operation are merges. The zero-denominator intermediate state at the pin
is the Manipulate phase where the old units dissolve and the new unit forms.

**Branch:** One inbound segment → pin → two outbound segments. This is
the **reverse lifecycle**: one thing is captured, cleaved, separated,
and disconnected into two. Division, square roots, factoring, and every
splitting operation are branches.

The bootstrap (Axioms 13-15) is a merge: two uncalibrated extents merge
through a pin to create one unit. Decomposition (e.g., square root)
is the reverse: one value branches into two components at an anti-merge pin.

### Axiom 25 — Segments and Flow

A **segment** is a grade-1 composite: `(start, end)`. It has direction
(recessive → dominant), extent, and is traversable. A segment is the
span between two lifecycle sites (pins) — the interval where the observer
is in the Experience phase, moving through structure between events.

When segments meet at pins, their **flow directions** determine the
physical character of the encounter:

| Configuration | Flow | Physical character |
|---------------|------|--------------------|
| **Convergent** | → ← | Two dominants meet. Maximum tension. Boundary, wall, collision. |
| **Divergent** | ← → | Two recessives meet (shared origin). Branch, fork, source, choice point. |
| **Parallel** | → → | Co-directional. Co-traveling. The space between them is structure (offset, gap, channel). |
| **Anti-parallel** | → over ← | Same line, opposite direction. Loop or standing wave. Reflection or interference. |

These are the same aligned/opposed distinction from Axiom 9, now
manifested at the structural level between segments rather than between
bare extents.

### Axiom 26 — Loops

A **loop** is a segment whose dominant end is identified with a recessive
end earlier in the structure: "this end IS that beginning."

A loop creates:
- A **period**: the number of ticks around the loop
- **Repetition**: traversal naturally recurs (the Repeat ability from
  Manipulate, now structurally embodied)
- **Enclosure**: a loop bounds a region. Area isn't in the 1D loop — it
  emerges from what the loop encloses (topological property)

A loop is a lifecycle that feeds its Resolve phase back into its Observe
phase. The symbol produced by one cycle becomes the extent observed by
the next. This is self-sustaining structure. From the observer's
perspective, a loop means: experience → resolve → re-observe → experience
again, endlessly. The observer never reaches true independence; it
re-enters observation.

### Axiom 27 — Boundary Conditions from Structure

When traversal reaches the end of an axis, it creates tension.
**The orthogonal structure absorbs or fails to absorb that tension.**
The boundary behavior is determined by what exists on the structural axes,
not by an external rule:

| Orthogonal structure | Boundary behavior |
|----------------------|-------------------|
| Repeat | **Wrap.** Tension restarts traversal at the beginning. |
| Mirror | **Reflect.** Tension reverses traversal direction. |
| Expand | **Extend.** The boundary moves outward to accommodate. |
| Nothing | **Clamp.** Tension stays at the boundary. Accumulates. |
| Unresolved (u = 0) | **Promote.** Tension cannot be absorbed. May become a new dimensional direction (Axiom 21). |

This is physical: the mover doesn't check a boundary flag. It creates
tension, and the surrounding structure either absorbs it or doesn't.
The behavior is readable from the structure itself.

Each boundary condition is a specific resolution of a lifecycle at the
edge: the tension is **observed** by the orthogonal structure,
**experienced** as pressure, **manipulated** using whatever law that
structure carries, and either **resolved** into a boundary behavior
(wrap, reflect, extend, clamp) or fails to resolve (**promote** — a new
lifecycle begins at a higher dimension).

### Axiom 28 — Dimensional Transition (Rivers and Lakes)

Structure can vary in dimensionality along traversal. A pin can connect
segments of different structural freedom:

- **Narrowing:** A 2D region (area, "lake") connects to a 1D path
  ("river") through an exit pin. Constraints tighten.
- **Widening:** A 1D path connects to a 2D region through an entry pin.
  Constraints relax.

At a widening pin, multiple exit paths become possible. The flow conditions
and available exit pins determine which path is taken. This models:
- Circuit components (wires → capacitor plates → wires)
- Decision spaces (constrained path → open choice → selected path)
- Impedance mismatch (when structural dimensions don't match at a pin,
  some flow reflects and some transmits)

---

## Part VII: Traversal and Time

### Axiom 29 — Traversal

A **mover** is an atomic element used as a cursor: `(currentTick, endTick)`.
The value is the current position. The unit is the route's extent.

Traversal is the act of advancing the mover from tick 0 to tick `endTick`.
Each step produces a new position — a new element that didn't exist at
the previous tick. Each step is a generative act.

Traversal IS the Experience phase made structural. The mover is an
observer being driven through a route by its internal or external
structure. At each tick, the mover experiences what is at that position.
The drive to advance — the "clock" — is structure, not will. In reality
that may be physics or time, in code it will be a computational tick based
on some kind of increment machine. The key is this push isn't directing 
anything, just like time itself isn't deciding outcomes.

### Axiom 30 — Time Is Traversal

Time is not external. It is what you get when you traverse any ordered
structure.

Every composite has a recessive ("before") and a dominant ("after").
Moving from recessive to dominant IS time passing. The structure IS
the clock:
- Resolution (how many ticks) = temporal granularity
- Ordering (recessive before dominant) = arrow of time
- Extent (recessive to dominant distance) = duration

No seconds, no hours — just positions within a directed structure.
But "before" and "after" are physically real. Causation follows from
ordering: what happens at tick N can depend on what happened at tick N-1,
but not on tick N+1.

### Axiom 31 — Operations as Pinned Events

Operations are **pins on the traversal route**. An operation attachment
specifies:
- A **site** (structural position along the route)
- A **law** (what happens when the mover reaches this position)
- The law produces an **outbound result** (generated value)

The traversal machine is a timeline: structural positions with lifecycle
events that produce values as the mover passes through. The operations
fire in order of their site position — causally ordered by the structure.

Each operation is a complete lifecycle: the mover **observes** the
operation site, **experiences** the law's effect on its state,
**manipulates** the inputs through the law, and **resolves** to a value
that is independent — available downstream as input to later operations,
where it will be observed by the next lifecycle.

---

## Part VIII: Laws

### Axiom 32 — Laws Are Abilities from the Manipulate Phase

Laws are not external rules applied to the system. They are the abilities
that emerge during the Manipulate phase (Axiom 6), formalized and named.
Every law derives from capabilities already present when two extents are
joined and experienced:

| Law | Derived from | Character |
|-----|-------------|-----------|
| **Expand** (multiply) | Stretch ability | Continuous growth. Adjust the rate/ratio of extension. |
| **Accumulate** (add) | Attach ability | General combining. Attach more of the same. |
| **Partition** (divide/ratio) | Comparison | Discrete splitting. Uses ratio to divide. |
| **Boolean** (and/or/overlap) | Location + comparison | Co-presence at a position. |
| **Repeat** | Re-attach ability | Iteration. Return to start and run again. |
| **Move** (translate) | Direction change | Shift position. Other-moving. |
| **Reframe** (perspective) | Perspective from location | Change how values are read. Self-moving. |
| **Fold/Interpolate** (run) | Experience/traversal | Traverse and derive intermediate values. |
| **Split/Merge** | Attach/disconnect | Branch and join. Roots, averages, convergence. |
| **Associate** (tension) | Halt/tension | Relate unlike forces. Preserve without resolving. |

More complex laws are compositions of these primitives, possibly sequenced
across multiple lifecycle instances. But the primitive laws all trace back
to abilities that exist by the Manipulate phase — abilities that were
first *observed* as potential, then *experienced* through traversal, and
finally exercised as *manipulation*.

### Axiom 33 — Two-Level Law System

Laws operate at two levels:

**Implicit law:** Derived from the unit categories of the inbound axes.
What the axes "naturally want" to do. Two continuous-expand axes →
multiply produces area. One continuous-expand + one discrete-repeat →
multiply produces repeated addition. The law falls out from what's arriving.

**Explicit law:** Specified by the pin or operation. Can override the
implicit law. You CAN add areas. You CAN multiply discrete counts.
But when the explicit law conflicts with what the axes naturally support,
**tension arises**. The system produces the best result it can, plus
tension recording the conflict.

### Axiom 34 — The Kernel as Natural Law

At grade 2+, the four-term multiply kernel (Axiom 20) is the **natural
law** for combining two directed structures. The pin computes all four
terms; the dominance pattern determines interpretation:

- `dd` terms → area contribution (aligned)
- `rr` terms → anti-area contribution (aligned, opposite sign)
- `rd + dr` terms → directed extent contribution (orthogonal)
- Net result: `(rr + dd)` aligned + `(rd + dr)` orthogonal

The explicit law (add, multiply, etc.) is a further operation applied to
the kernel's output. The kernel is universal; the explicit law is chosen.

---

## Part IX: Views, Frames, and Collections

### Axiom 35 — A View Is Measurement Without Ownership

A **view** relates a subject to a frame:

    View(frame, subject) → read outcome

The frame is a composite. Its recessive child is the **calibration**
(the reference, the structural context). Its dominant child is the
**existing readout** (the current measurement).

Reading commits the subject to the frame's calibration. The result is
a new element: the subject as seen through the frame. This is generation —
the read outcome belongs to neither the frame nor the subject alone.

A view is a lifecycle instance: the subject **observes** the frame
(attaches), **experiences** the calibration (traverses the frame's
structure), the frame **manipulates** the subject into its terms
(negotiates expression), and the result **resolves** to a measured
value independent of both the original subject and the frame.

### Axiom 36 — Frame-Driven Dispatch

When reading through a composite frame, each atomic leaf in the frame
tree determines its own behavior via unit sign:

- **Positive leaf** → value calibration at that position
- **Negative leaf** → structural organization at that position
- **Zero leaf** → generative demand at that position

A grade-3 frame has 8 atomic leaves. Some may calibrate, some organize,
some demand generation. The view doesn't choose a mode — it reads through
the frame, and the structure determines what happens everywhere.

### Axiom 37 — Families

A **family** is an ordered collection of elements read in one frame:

- **Frame**: the structural context (organizes members)
- **Members**: the values (the data being organized)
- **Ordering**: whether member sequence matters

Family operations map to the trichotomy:
- Reading all members through the frame = projecting values through structure
- Sorting, focusing, reorganizing = structural operations
- Accumulation, boolean, fold = generative operations (produce new elements
  from existing ones)

**Focusing** shifts what counts as structure vs value: a member becomes
the new frame, and the remaining members are reorganized within it.
The same element changes role. Aspects are relational (Axiom 11).

---

## Part X: Completeness

### Theorem 1 — Construction Completeness

From the bootstrap lifecycle (Axioms 13-15), the entire number system
is constructed:
- Extents → ratios → units → numbers
- No real numbers assumed. All arithmetic is exact integer operations on
  value/unit pairs.

### Theorem 2 — Dimensional Completeness

From the dimensional lifecycle (Axioms 19-22), all dimensions emerge:
- Grade 1: directed segments (1D with direction)
- Grade 2: the four-term kernel separates into area (2D) and complex
  lines (1D with rotation)
- Grade 3+: each grade produces the next dimension through symmetry
  breaking of the previous grade's kernel
- No dimensions are assumed. Each is forced by unresolvable tension.

### Theorem 3 — Structural Completeness

From lifecycle composition (Axioms 23-28), all topology is constructed:
- Merge and branch are the only two topological events (forward and
  reverse lifecycle)
- All structures are networks of segments connected at pins (lifecycle
  sites connected by flow)
- Loops, boundaries, and dimensional transitions follow from
  pin topology and tension absorption
- Equations are merge points where tension must reach zero
- Variables are segments with free denominators (incomplete Manipulate phase)
- Recurrence relations are loops (Resolve feeding back to Observe)

### Theorem 4 — Temporal Completeness

From traversal (Axioms 29-31), time emerges:
- Every ordered structure contains "before" and "after"
- Traversal actualizes the Experience phase into sequence
- Causation follows from ordering
- No external clock needed
- Operations fire as lifecycle instances at pinned positions

### Theorem 5 — The Aspect Trichotomy Is Exhaustive

Every element in the system, in every context, plays exactly one of
three roles determined by its unit sign. Every operation is classifiable
as value-work, structural-work, or generative-work based on what it
preserves and what it produces. The multiplication table (Axiom 12)
governs all aspect transitions and is closed under composition.

### Theorem 6 — Lifecycle Universality

Every event in the system is an instance of the four-phase lifecycle
(Axiom 6) or a composition of such instances:
- The bootstrap is a lifecycle (Axioms 13-15)
- Dimensional emergence is a lifecycle per dimension (Axiom 21)
- Every merge is a forward lifecycle, every branch a reverse (Axiom 24)
- Every operation is a lifecycle at a pin (Axiom 31)
- Every view/measurement is a lifecycle (Axiom 35)
- Every boundary resolution is a lifecycle (Axiom 27)
- Loops are self-feeding lifecycles (Axiom 26)

The lifecycle is the universal pattern because its phases correspond
to the only things that *can* happen between an observer and structure:

| Phase | Observer | Structure | Ability mode |
|-------|----------|-----------|--------------|
| 1 | Observe | Attach | Passive — receive |
| 2 | Experience | Traverse | Active — read |
| 3 | Manipulate | Negotiate | Active — write |
| 4 | Resolve | Commit + Independence | Complete — become observable |

These exhaust the structural possibilities. An element can receive
information (Observe), move through structure to gather feedback
(Experience), act on structure using gathered knowledge (Manipulate),
or reach completion and become available for others (Resolve). There
is no fifth option.

### Theorem 7 — Observer-Structure Duality

The observer perspective and the structural perspective describe the
same events. Every structural phase has an observer correlate and vice
versa. No event exists that is purely structural (with no observer) or
purely experiential (with no structure). This duality is not a metaphor —
it is the same information described from inside and outside the lifecycle.

The drive to traverse (Experience) requires structure to achieve, whether
external or internal to the observer. The observer need not know which;
the experience is the same. This is analogous to internal structure being
invisible to the entity it composes (as cells are invisible to the
organism). The system is agnostic about the source of the drive — only
the structural result matters.

---

## Summary: What Is Assumed vs What Is Constructed

**Assumed:**
- Ordered comparable extents exist (Axiom 1)

**Constructed (in order of emergence):**
1. Signs from endpoint attachment (Axiom 2)
2. Atomic elements with value/unit pairs (Axiom 3)
3. Composite structure via opposition (Axiom 4)
4. Direction from ordering (Axiom 5)
5. The four-phase lifecycle from the possibilities between observers
   and structure (Axiom 6)
6. Three aspects from unit sign (Axiom 10)
7. Ratios from observing extents together (Axiom 13)
8. Units from the experience/manipulate/resolve lifecycle (Axiom 14)
9. Numbers from others observing the resolved unit (Axiom 15)
10. Directed segments from orthogonal tension (Axiom 19)
11. Area, complex numbers from the four-term kernel (Axioms 20, 22)
12. Higher dimensions from symmetry breaking (Axiom 21)
13. Topology from pins, merges, branches (Axioms 23-26)
14. Boundary behavior from structural absorption (Axiom 27)
15. Time from traversal of ordered structure (Axiom 30)
16. Laws from Manipulate-phase abilities (Axiom 32)

Nothing is assumed except that ordered extents exist (and that not
everything is ordered). Everything else — numbers, dimensions, topology,
time, area, complex arithmetic, boundary conditions, laws, symbols,
and the lifecycle itself — is constructed through observation, experience,
manipulation, and resolution.

---

## Notes for Future Development

**Pins as machines.** The system constructs many machines at different
scales. A pin can be thought of as a mini-machine, and the extent it
traverses is another machine. Both can hold data, and their data can
affect each other — the pin's state influences how it reads the extent,
and the extent's structure influences how the pin advances. The full
interaction between pin-machines and extent-machines is part of the
data layer design and will be formalized separately.
