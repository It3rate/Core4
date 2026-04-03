# Core4 Axiom System — Version 3

A mathematics bootstrapped from single measures and an implicit zero.
No real numbers, no stated axes, no assumed dimensions.
Everything is constructed from measures, frozen intervals,
opposing perspectives, and the tension between them.

---

## Geometric Orientation

This is not an axiom — it is a guide to help readers orient themselves
geometrically as the system unfolds. The grades of this system correspond
loosely to familiar geometric objects, but with important differences.

**Pre-grade (raw measures).** Before any unit is formed, measures are
positive values from an implicit zero. They can be compared by size, but
there is no unit, no resolution, and no interval. You can say one
measure is bigger than another, but you cannot say how much bigger in
any reusable way. You cannot measure backwards from zero — there are no
negative values. A light sensor that has never experienced total darkness
still produces measures. A thermometer that has never reached absolute
zero still produces measures.

**Grade 0 — the "calibrated measure."** A measure is frozen and declared
a unit. It is just a regular measure that has been committed to not
changing, and it persists. Any other measure of the same property can now
be expressed as a count of this unit. Grade 0 is a single value with a
single unit, measuring from the implicit zero. It is one-dimensional and
positive only. You can measure any runner's time, but you cannot measure
a runner going backwards.

**Grade 1 — the "interval."** Two measures taken from different positions,
or two units calibrated from different perspectives, create an interval.
The simplest case: one measure becomes the origin, and the difference to a
second becomes the unit. This is the first time negative values exist —
something can be below the chosen origin. A grade-1 element is a pair of
grade-0 elements (recessive and dominant) sharing a dual unit made of two
opposing perspectives. It can measure intervals, directions, and changes.

Alternatively, a dual unit can be formed by combining two single units
that point in opposite directions — one measuring increases (dominant),
the other measuring decreases (recessive). The dual unit captures both
perspectives in a single structure. This is more physical than storing
two values with one unit, because the pairing is inherent in the number
rather than imposed by external rules.

**Grade 2 — "area."** Formed by fusing two grade-1 dual units. Each
dual unit has a recessive and dominant component, giving four possible
pairings. Picking any two of the four gives a snapshot (size measurement):

- Fusing both dominant components (expanding + widening) → **area**:
  larger values mean more, smaller mean less.
- Fusing both recessive components (shrinking + narrowing) → **anti-area**:
  larger values mean less, smaller mean more.
- Fusing one dominant with one recessive (growing + narrowing, or
  shrinking + widening) → **rays**: larger or smaller does not directly
  tell you about area. These are complex numbers — they have length and
  angle, not enclosed space.

Using all four terms together gives change measurement: each axis gets
a directed segment (start and end state), and together they describe
how both axes changed simultaneously.

The two area types (positive and negative) sit on one diagonal. The two
ray types sit on the other. This IS complex multiplication — it falls
out of the grade system rather than being introduced.

**Grade 3 — "volume."** Formed by combining a ray (complex number) with
an area. The only way to break into a new dimension is to combine a
symmetric object (area) with one that breaks symmetry (a ray). Higher
grades continue this pattern indefinitely, though our intuitive spatial
geometry ends at grade 3.

**Beyond geometry.** Geometry is a convenient visualization, but these
grades usually represent non-spatial quantities: weight and density,
temperature and pressure, items and repetitions, signal and noise. An
"orthogonal direction" simply means: the measurement system is receiving
information it cannot account for within its current dimensions. That
orthogonal direction can be *anything* until a different element is
discovered that accounts for it. The geometric picture is a scaffold,
not the territory.

**Note on the Engine:** This assumes an engine that is reacting to
these numeric structures in some consistent way. The main assumption
is if we define a direction, the flow tendency will be along that
direction. There is a natural order defined, and we assume elements
can react to it. It still will be driven by a mechanical increment
force that will be modeled in the system, but that won't be directing.

A key aspect is *tension*, which this engine tries to solve. In the real
world the engine is often physics, causing rocks to roll, or whirlpools
to swirl to reduce their tension. As we move to intelligence, problem
solving becomes more abstract, but the goal is the same — to reduce
tension. Tension is not just a passive record of failure; it is actively
attractive, creating a demand that pulls compatible information toward it.
The system here is mathematical, so it uses structure, values, and laws
to solve tension. These are not created in the abstract however, they are
derived from the same forces that create the tension in the first place.
There is a gradient from measures to numbers to dimensions and with data
and scale, it starts to resemble behavior.

A stable state is not zero tension. It is the equilibrium where
confusion and available information cannot help each other further.
The system does not seek to eliminate all tension — it seeks the point
where remaining tension cannot be reduced by any signal currently
available. New information can always disturb this equilibrium and
restart the resolution process.

---


## Part I: Ground — From Measures to Numbers

### Axiom 1 — Measures and Chaos

There exist **measures**: positive comparable magnitudes from an implicit
zero. A measure has no unit, no resolution, no sign. It is simply "some
amount" that can be compared to another measure by size. This is the sole
assumption.

A measure is not an extent with two endpoints. It is a single reading — a
light level, a voltage, a duration, a weight. We think of extents as
primitive because we are accustomed to measuring from a convenient zero
(the height of something, the time of a race), but the zero is implicit
in the measuring system. A light sensor may never experience total
darkness. A thermometer may never reach absolute zero. The measure is
what the sensor reports, and it starts from whatever the sensor's
natural floor is.

Because the zero is implicit and the measure is always from that zero
upward, **there are no negative values** at this stage. Temperature
does not go below absolute zero. Absolute darkness does not get darker.
A runner in a race does not run backwards. Every measure is positive by
default.

Not everything is a measure. Some phenomena are **chaotic** — not
orderable, not comparable. The system can only observe these; it cannot
operate on them. They appear as irreducible tension, background noise
within the resolution of whatever structure is present. One purpose of
the system is to discover frames that render previously chaotic phenomena
orderable — but that is never guaranteed, and some residual chaos is
always acceptable.

We write a measure as a bare value: `a`, `b`, `c`. All are non-negative.

### Axiom 2 — The Unit (Freezing a Measure)

A **unit** is a measure that has been frozen: its value is fixed and it
persists.

To create a unit, pick any measure and declare it permanent. That is all.
The unit is not attached to any particular value at this point. If you
inspected it, you would find it to be just a regular measure like the
others, except that it does not change value and it persists through time.

    Freeze(m) → u    where u = m, permanently

The unit can now be used to express any other measure of the same property
as a count: "this measure is 3.2 of that unit." This is calibration. The
unit's magnitude becomes the **resolution** — how fine the measurement
grid is.

We did not create a numerator/denominator relation. We simply took a value
and froze it, declaring it a denominator. The numerator is whatever measure
we subsequently compare to it. The unit exists independently, waiting for
values to attach.

**Freezing is the universal prerequisite for creating extents.** A raw
measure is just a magnitude. To turn measures into extents — spans that
can be compared, combined, and structured — you must first freeze a
reference point. At grade 0, the frozen measure is the unit (what "one"
is). When the implicit zero is not available, a frozen measure serves as
the origin (what "zero" is). At higher grades the same pattern applies:
freeze an area before measuring larger and smaller areas, freeze a rate
before measuring faster and slower rates. The act of freezing is what
turns the current grade's raw material into a reference frame that
subsequent material can be expressed against.

**What a single unit cannot do:** A single unit starts from the implicit
zero and extends in one direction. You can use it to measure any runner's
time. You cannot use it to measure a runner going in the opposite direction.
That would be error and tension — a real runner, really being timed, but
producing values the system cannot express. This is not a flaw; it is the
honest limitation of a system with one perspective.

### Axiom 3 — The Atomic Element

An **atomic element** is a pair `(v, u)` where:
- `v` is a non-negative integer: the **value** (how many units)
- `u` is a frozen measure: the **unit** (what "one" is)

At this stage, `v` is always non-negative because there is only one
direction: from the implicit zero toward larger values. The unit
magnitude `|u|` is the **resolution**: how many ticks compose one unit.

An atomic element is **grade 0**: the leaf of all structure.

### Axiom 4 — Perspective and Direction

Direction does not come from extents having endpoints. It comes from
**perspective** — the choice of where to start measuring and which way
to call "increasing."

Consider calibrating a temperature sensor. You take a reading and
**freeze it** as the origin — just as Axiom 2 freezes a measure into a
unit, here you freeze a measure into a zero point. Only after the origin
is frozen can subsequent readings become extents measured from it. If
you freeze a morning reading (cool) and subsequent readings are warmer,
larger numbers mean hotter. If someone else freezes an evening reading
(warm) and subsequent readings are cooler, larger numbers mean colder.

Both systems measure temperature correctly. Both have the same physical
resolution if their intervals are the same size. They can even share
the same zero point. But they are **incompatible** because their units
point in opposite directions. A formula could convert between them —
they measure the same underlying physical thing — but that conversion
does not happen automatically.

This is the origin of **sign**. A measure in one system is positive; the
same physical quantity in the opposite system is also positive but means
the reverse. The concept of negative does not come from a number line
extending below zero. It comes from the existence of a second
perspective that opposes the first.

Two perspectives on the same property, pointing in opposite directions,
are the foundation for everything that follows. We call them:
- **Dominant (!direction):** the perspective treated as the primary
  increasing direction
- **Recessive (~direction):** the opposing perspective

These labels are relative. A diver thinks in depth (dominant); a
mountaineer thinks in height (dominant). Neither is inherently primary.
The terms simply distinguish the two directions so we can reason about
their interaction. All composite objects in this system are composed of
these opposing perspectives, because any quantity that can increase can
also decrease, and the system needs both views.

### Axiom 5 — The Interval and Negative Values

An **interval** is formed when two measures create a span that includes
a meaningful zero point between them:

**Simple case (implicit zero):** When you measure many runners in a race,
they all start from zero. You only need one unit and one value per runner.
The zero is implicit — you do not record it. This is the grade-0 case:
one unit, one value, always non-negative.

**General case (explicit origin):** You take a reading from a temperature
sensor and **freeze it** as the origin — a permanent zero point. This is
the same act as Axiom 2 (freezing a measure), but here the frozen measure
serves as a reference position rather than a unit size. Only after the
origin is frozen can subsequent readings become extents measured from it.
The difference between a new reading and the frozen origin is an extent,
and for the first time, **negative values exist** — a reading below the
frozen origin is negative relative to it.

This is a universal pattern: **you always freeze before you can create
extents.** At grade 0, you freeze a measure to create a unit, and other
measures become counts of that unit. At grade 0 with a non-default zero,
you freeze a measure to create an origin, and other measures become
extents from it. At grade 2, you would freeze an area before you could
measure larger and smaller areas. The act of freezing is what turns raw
measures into a reference frame that subsequent measures can be expressed
against.

Both cases require an interval with two numbers. In the simple case, the
first number is always zero (the implicit floor), so we omit it. In the
general case, both numbers matter.

There is an assumption hidden here: we assume subsequent measures are
larger than the frozen origin, establishing which direction is "positive."
If someone freezes a different reading and measures in the opposite
direction, they get an equally valid system that points the other way
(Axiom 4). Both work, but they are incompatible without explicit
conversion.

Negative values are not a fundamental feature of measures. They are an
artifact of perspective — the consequence of having chosen an origin that
is not at the implicit zero.

---

## Part II: Dual Units and Grade 1

### Axiom 6 — The Dual Unit

A **dual unit** is formed by combining two single units that point in
opposite directions into a single structure:

    DualUnit(recessive_unit, dominant_unit)

where the recessive and dominant units measure the same property from
opposing perspectives.

**Why dual units exist — the data problem.** You can measure temperature
ranges with a single unit and two values: record the low, record the
high. But this approach is fragile:
- You need an external rule to know the values are pairs, not triplets
- If you miss a single recording, every subsequent pair is misaligned
- The pairing is imposed by convention, not inherent in the data
- The range (high minus low) requires a separate calculation
- The range alone (e.g., "7 degrees") discards the zero point —
  you know the span but not the temperature

A dual unit solves these problems by making the pairing inherent:

**Example:** A temperature range of -2°C to +5°C is recorded as `2r + 5d`
(2 recessive units + 5 dominant units). This may seem strange at first —
it is like measuring the height of an iceberg by saying "20 deep and 6
high." But it is not too different from measuring a rectangle by saying
"2 by 5 inches = 10 square inches."

With a dual unit:
- The pairing of low and high is structural, not imposed
- Missing data loses one number, not the alignment of all subsequent pairs
- The range is computed by addition: `2r + 5d` spans 7 total units
- The zero point is embedded in the structure (the boundary between
  recessive and dominant)

**What a dual unit is physically:** it is one concept (temperature range,
change in light, depth-to-height) captured as a single number with two
components. The recessive component measures how far below the boundary,
the dominant measures how far above. The boundary itself is the zero of
the dual unit.

### Axiom 7 — The Composite Element (Grade 1)

A **composite element** is a pair of two grade-0 elements measured in
opposing units:

    Composite(recessive, dominant)

where the recessive and dominant are each atomic `(v, u)` pairs whose
units point in opposite directions. The composite's grade is
`children's grade + 1`.

- **Recessive**: the reference side, the context, the "before"
- **Dominant**: the applied side, the measured, the "after"

This is the structure that dual units measure. A grade-1 element has two
independent atomic children, each with its own value and unit. The
identity of the composite is in the relationship between its two
endpoints — a directed interval from recessive to dominant.

**Why opposition is required:** Two elements with same-direction units
are in the same space. Combining them adds within that space but does
not create a new grade of structure. Same-unit, same-direction values
are just data — a list of measures sharing a calibration. To build a
composite that represents a genuinely new structural level, the two
children must carry opposing perspectives. The recessive provides the
reference frame and the dominant provides the measurement against it.

This is the only way to build higher-grade structure. A grade-1 element
has two atomic children. A grade-2 element has two grade-1 children
(four atomic leaves). Grade N has 2^N atomic leaves.

### Axiom 8 — Ordering and Direction

Every composite has inherent direction: recessive → dominant.
This is not a convention. The recessive provides context for the dominant.
Swapping them changes the physical meaning.

Direction gives every composite a natural "before" and "after."
Traversal from recessive to dominant is the most primitive form of time
(see Part VIII). No external clock is needed, just the tendency to flow
from beginning to end.

---

## Part III: The Lifecycle

Every event in this system — from the first unit bootstrapping itself
out of raw measures, to dimensional emergence, to a pin firing during
traversal — follows a universal pattern. The pattern has four phases,
visible from two complementary perspectives: the **structural** perspective
(what happens to the structure) and the **observer** perspective (what
happens to the participant element moving through the structure).

Any event in the system is one of:
1. **A single phase** within a larger lifecycle
2. **A complete lifecycle** instance
3. **A composite event** built from multiple lifecycles

### Axiom 9 — The Four Phases

Each phase is described from both perspectives. The structural view
names the mechanical event; the observer view names the experiential
state. They are the same event seen from outside and inside.

**Phase 1 — Observe / Attach**

*Structural:* A connection forms. This has two sub-steps, and the first
is meaningful on its own:

> **Pin (to a single measure):** A pin attaches to one measure. This
> alone gives the pin a **location** (here), a **direction** (one way
> to go — from the implicit zero upward along this measure), and the
> **potential to interpolate** along that direction.
>
> At this stage — pinned to one measure — the observer already has
> meaningful capabilities: a position, a direction, the potential to
> traverse, and a perspective (the world as seen from this location).
>
> **Join (pin to a second measure):** The pin attaches to a second
> measure without releasing the first. Now there are **two direction
> opinions** from a single location. These opinions are either aligned
> (same direction) or opposed (opposite directions). This is comparison —
> not yet measured, but structurally present as a ratio.

*Observer:* This phase is entirely **passive**. The element observes
measures, sees direction, recognizes that interpolation is possible.
It can attach more closely to see more — but it is not acting on
anything, only receiving information. Joining a second measure enriches
what can be observed (now there is comparison), but does not change the
passive character of this phase.

**Phase 2 — Experience / Traverse**

*Structural:* The interpolation potential is exercised. The element moves
along the attached measure, producing an ordered sequence of positions.
This is traversal — the structure is being read, generating feedback at
each position.

*Observer:* The element **experiences** the measure by moving through it.
This is like running an equation — at each position, the structure
provides feedback (what is here, how does this compare to where I was).
The experience is informational, not transformative: the element is
learning about the structure, not changing it.

The drive to interpolate is not innate in the observer. It requires
**structure** to achieve — either external structure pushing the element
through (a mover advancing along a route), or internal structure within
the element itself. Either way, the element experiences traversal;
whether the impulse is external or internal does not change what is
experienced.

If the element interpolates along the measure, it gains **time** — an
ordered sequence of positions. If it interpolates past the end, it gains
**tension** — the structure has been exceeded.

**Phase 3 — Manipulate / Negotiate**

*Structural:* The ability to compare (from Observe) and the feedback
from traversal (from Experience) together imply the potential for
**change**. Laws emerge from abilities already present:

- **Track:** Use one measure to measure changes in the other
- **Repeat:** Re-attach to the start when you reach the end
- **Stretch:** Adjust the rate of interpolation (change the ratio)
- **Negate:** Reverse direction
- **Halt:** Recognize when combined potential is exceeded; create tension

*Observer:* The element can now **manipulate** — actually modify measures,
adjust directions, negotiate changes to be more in line with its needs.
Where Experience was reading the structure, Manipulate is writing to it.

Every ability has two modes: **recognition** (you can see direction, see
rate) and **action** (you can change direction, adjust rate). You had
recognition from Phase 2; you gain action in Phase 3.

**Phase 4 — Resolve / Commit + Independence**

*Structural:* Two things happen in sequence:

> **Commit:** The negotiated structure is locked. Specifically, the
> **unit** is locked — the frozen measure, the "what is one," the tick
> structure. The values that participated in negotiation become
> measurements *within* the new unit, not parts of the unit itself.
>
> This is always a unit lock. The unit commits to not changing size even
> if the values that created it later adjust themselves. Units are local —
> they are bound to the place and structure where they were committed.
>
> **Independence:** The committed unit separates from the values that
> created it. It becomes a **symbol**: a portable, self-sufficient entity
> that can calibrate other things. The unit transcends its construction —
> area is more than two segments, speed is more than distance and time.
> The values remain as a remainder or reattach elsewhere; the unit is free.

*Observer:* The element reaches a **resolution** — a stopping point. The
negotiation is complete. The structure is committed. The element is now
*available for others to observe*. Its Independence is the beginning of
other elements' Observe phase.

This is the mechanism by which the system creates abstractions. A symbol
carries the memory of its formation (direction, resolution, structure)
but has no ongoing dependency on the specific values that formed it.
A different value can attach to the unit and be measured by it.

### Axiom 10 — Abilities Accumulate

Abilities unlocked at any phase remain available in all subsequent phases
and in all subsequent lifecycle instances at higher grades. Nothing is lost.

After **Pin** (to one measure): location, direction, interpolation
potential, perspective.

After **Join** (to a second measure): comparison, two direction opinions,
ratio.

After **Experience/Traverse**: time (ordered positions), feedback,
tension awareness, rate.

After **Manipulate/Negotiate**: change, laws (track, repeat, stretch,
negate, halt), tension boundaries.

After **Resolve/Commit**: unit, measurement, resolution.

After **Independence**: symbol, portability, calibration of others.

### Axiom 11 — The Reverse Lifecycle

The lifecycle runs backward for decomposition:

**Capture:** A free-running symbol is bound — attached to a specific
context. (Reverse of Independence.)

**Cleave:** The committed structure is broken — the unit lock is released,
exposing the internal negotiation. (Reverse of Commit.)

**Separate:** Values are distinguished from units by sorting — the
negotiated relationships are unwound into their component experiences.
(Reverse of Negotiate.)

**Disconnect:** The pin releases. Two previously joined measures become
independent again. (Reverse of Attach.)

Importantly, a symbol does not retain its provenance. An area created
from a rectangular area-generating machine can be captured and run in
reverse through a circle-generating machine, yielding the components
needed to generate a circle. Symbol creation is lossy and decomposing
is context dependent.

---

## Part IV: The Three Aspects

### Axiom 12 — Unit Sign Determines Aspect

The sign of an atomic element's unit determines its **aspect** — its
physical role in any encounter:

| Unit sign | Aspect | Meaning |
|-----------|--------|---------|
| `u > 0` (positive) | **Value** | Aligned. A calibrated measurement in a known space. "One" is established. |
| `u < 0` (negative) | **Structure** | Orthogonal. A force acting on the context without being the context. Organizes, constrains, imposes a dimension. Does not contribute magnitude — contributes arrangement. |
| `u = 0` | **Generative** | Unresolved. Has magnitude but no "one" to measure against. Between unit spaces. Demands resolution through a law. |

At grade 0, a unit is always positive (measuring from the implicit zero
upward). Negative units first appear at grade 1, when opposing
perspectives create the possibility of structural opposition. Generative
(zero) units appear whenever the system encounters magnitude it cannot
yet calibrate.

### Axiom 13 — Aspects Are Relational

An element's aspect is not intrinsic — it depends on the frame through
which it is read. The same element can be value in one context, structure
in another, and generative potential in a third.

### Axiom 14 — The Multiplication Table of Aspects

When two elements interact through multiplication or scaling, their unit
signs multiply:

    (+1) x (+1) = +1    Value x Value = Value
    (-1) x (-1) = +1    Structure x Structure = Value
    (+1) x (-1) = -1    Value x Structure = Structure
    ( 0) x ( n) =  0    Generative x Anything = Generative

Key physical implications:
- Two orthogonal forces meeting produce something aligned
- A value applied through structure produces structural position
- Anything touching the unresolved stays unresolved

---

## Part V: Bootstrap — The First Lifecycle Instance

The number system is the first complete run of the lifecycle.

### Axiom 15 — Measures as the Starting Point (Observe Phase)

A system begins with raw measures — positive values from an implicit
zero. These are the sensor readings, the durations, the magnitudes. They
can be compared (this one is bigger than that one) but not calibrated
(we cannot say how much bigger in reusable terms).

This is the Observe phase: the system is receiving information passively.
A light sensor detects some amount of light. It can use the energy from
the light it detects, but it cannot understand whether the reading is
large or small because it has nothing to compare against in a lasting way.

Multiple measures can be observed. Many runners can be timed. Many
temperatures can be recorded. But without a unit, these are just raw
magnitudes — comparable in the moment but not systematically.

### Axiom 16 — Unit Formation (Experience + Manipulate + Resolve)

A measure can be **frozen** into a unit. This is the Experience,
Manipulate, and Resolve phases running in sequence.

**Step 1 — Experience provides context (Phase 2).** Multiple measures
are observed over time or across instances. The system experiences
variation — some readings are higher, some lower. This traversal through
different values is the experiential basis for wanting a reference point.

**Step 2 — Manipulation selects a reference (Phase 3).** Some measure is
chosen as the reference. This may be driven by physics (the most common
reading, the first reading, the reading at some notable event) or by
engineering (an arbitrary but convenient value). The selection is an act
of manipulation — the system is writing to its own structure.

> In reality this may be enacted by physics or biology. In this system
> it is future machines made of these values, structures and laws.
> 'Averaging' may be a good first approximation.

**Step 3 — Resolution commits the unit (Phase 4).** The chosen measure
is frozen — it commits to **not changing value**. It becomes the unit.
This is irreversible in the sense that the unit now persists independently
of the values that motivated its creation.

The frozen measure IS the unit. It is not a ratio of two things. It is
a single value that has been declared permanent. The numerator is whatever
measure you subsequently compare to it.

    Freeze(m) → u
    Compare(reading, u) → v    (v = reading / u)

Once a unit exists and is independent, other measures can be **calibrated**
to it:

    measure.CalibrateWith(unit) → calibrated value

Calibration reexpresses a measure in terms of the unit's resolution.
The measure becomes a **number**: a value expressed in terms of the
self-declared "one." Numbers are born from units, not the reverse.

The complete bootstrap lifecycle:
1. **Observe:** Receive raw measures (Axiom 15)
2. **Experience:** Encounter variation across measures (Axiom 16, Step 1)
3. **Manipulate:** Select a reference measure (Axiom 16, Step 2)
4. **Resolve:** Freeze the reference into a permanent unit (Axiom 16, Step 3)
5. Others calibrate to the unit → numbers

From "positive comparable measures exist" alone, the number system is
constructed. At this stage all numbers are non-negative.

---

## Part VI: Dimensional Emergence — Perspectives and Tension

Dimensions are not stated. They **emerge** from tension that cannot
be expressed at the current dimensionality. Each dimensional increase
follows the same lifecycle pattern, driven by the system encountering
information it cannot account for within its current structure.

### Axiom 17 — Tension as Demand

When the system's predictions do not match its inputs — when feedback
disagrees with output — **tension** arises. This tension is not passive.
It is a structural vacancy, a demand that attracts compatible information.

    EngineElementOutcome { Result, Tension, Note }

- `Result`: the best lawful projection (what the system can produce)
- `Tension`: the unresolved structure (what couldn't settle)
- When `Tension` is null, the outcome is **exact** (resolution complete)

Tension is never discarded. It propagates through operations. But
critically, it does not just sit as a record of failure. It acts as a
socket — an open slot that any signal of compatible shape can fill.
Initially it does not discriminate what kind of information fills it.
A light sensor with unexplained extra readings and a food system with
unexplained extra energy can resolve each other's tension if the signals
are routed together.

**Equilibrium, not zero.** The system does not seek to eliminate all
tension. It seeks the point where remaining tension cannot be reduced by
any currently available signal. This is a stable state — the loads
between confusion and information cannot help each other further. New
information can always disturb this equilibrium and restart the process.

### Axiom 18 — Opposing Perspectives Create Direction (Grade 0 → 1)

A single unit measures from the implicit zero in one direction. Direction
and negative values emerge when a **second perspective** creates an
opposing unit.

**The temperature example.** A morning calibration and an evening
calibration of the same sensor may produce units pointing in opposite
directions. Both measure temperature correctly. Both have valid zeros.
But they are incompatible without conversion, because what one calls
"increasing" the other calls "decreasing."

This opposition is the origin of sign. It is not abstract. It comes from
the physical fact that two observers can calibrate the same property in
opposite directions.

When these two perspectives are combined into a **dual unit** (Axiom 6),
the result is a grade-1 element: a directed interval with a meaningful
zero between the recessive and dominant components. For the first time,
the system can express negative values — not as a mathematical extension,
but as the natural consequence of having two opposing viewpoints.

**The sensory example.** A creature with a light sensor and a frozen unit
can say "it's lighter than normal" or "darker than normal." If the light
suddenly changes in a way the system's feedback cannot explain — say,
extra light reflecting off food — the feedback system carries tension.
This tension is a vacancy (Axiom 17). If the current light reading is
routed to fill that vacancy, the system now has two values (previous
light and current light) with one dual unit (the original perspective
and its recessive counterpart). It can now say not just "lighter than
normal" but "light has increased." The second value is recessive to the
first — it is "before" to the first's "after."

**The mechanism:** unresolved tension from feedback disagreement creates
the demand. A new signal dimension fills the demand. The grade increases
from 0 to 1. This is not invention — it is the environment forcing the
system to expand its dimensional structure.

### Axiom 19 — Dual Units vs Data

There are two ways to track intervals and changes: **data** (multiple
values sharing one unit) and **dual units** (one number with two
components in opposing units). Both work. They have different properties.

**Data approach (single unit, multiple values):**
- Store pairs of readings: (low, high), (before, after)
- Requires external rules to delineate pairs vs triplets vs sequences
- Missing a single recording misaligns all subsequent pairs
- Range requires subtraction: high - low
- Range alone (e.g., "7 degrees") discards the zero point
- Flexible — can store any number of values with the same unit

**Dual unit approach (two opposing units, one number):**
- Each number inherently contains both components: `2r + 5d`
- Pairing is structural, not imposed by convention
- Missing data loses one number, not the alignment of all subsequent ones
- Range is computed by addition: recessive + dominant = total span
- The zero point is embedded (the boundary between recessive and dominant)
- Less flexible — each number is exactly two components

The dual unit approach is more **physical** — it represents the
underlying reality more directly because the structure of the number
mirrors the structure of the phenomenon. The data approach is more
**computational** — it is easier to manipulate, sort, and process, but
requires rules that are external to the data itself.

Neither is better in all situations. The system supports both. But grade
increases specifically require the dual unit approach, because grade
increase is about creating new structural levels, not accumulating data
at the current level.

### Axiom 20 — The 1x1 Requirement and Degrees of Freedom

For dimensional fusion to produce a coherent space, the unit cell must be
**1x1**: each axis contributes exactly one of its own measure.

Every fused entity has exactly **one degree of freedom** — it is one thing.
Speed is one thing. Area is one thing. But the *tools* used to construct
fused entities may expose more than one adjustable axis.

**When the fused axes are interchangeable** (both inches in a square-inch
unit cell): the construction tool gets **multiple degrees of freedom**.
You can scale width or height, and the area changes predictably.

**When the fused axes are not interchangeable** (miles and hours in a
speed unit): the fusion imposes a **restriction**: values may only attach
freely to one axis. The denominator axis (hours) is locked to one.

**The non-fused alternative** is co-location without fusion: "I drove 80
miles in 2 hours." You can compare such ratios, but you do not have a
*concept* of speed. There is no fused entity, no unit that persists
independently. You are stuck describing instances. Fusion is what turns
a ratio into a thing.

**Consequence:** Units must be settled before fusion can happen. If either
axis has no committed unit, you cannot construct the 1x1 cell.

### Axiom 21 — Grade 1 → Grade 2: The Four-Term Kernel

When two grade-1 elements (directed intervals with dual units) are
combined, each has a recessive (r) and dominant (d) component. Their
combination produces four terms:

    rr = left.Recessive x right.Recessive
    rd = left.Recessive x right.Dominant
    dr = left.Dominant  x right.Recessive
    dd = left.Dominant  x right.Dominant

Each term fuses a unit from each side. The physical character depends on
which perspectives are being combined:

**The iceberg example.** Suppose we measure an iceberg along two axes:
vertical (growing up / shrinking down) and horizontal (widening /
narrowing). Each axis is a grade-1 dual unit with a recessive and
dominant perspective.

The four terms of the kernel represent every combination of perspectives
across the two axes:

| Term | Units fused | Physical character |
|------|-------------|--------------------|
| `dd` | growing + widening | **Area creation.** Both dominant. Larger values mean more ice. Smaller mean less. Both components push outward together. |
| `rr` | shrinking + narrowing | **Anti-area.** Both recessive. Larger values mean less ice. Smaller mean more. Both components pull inward together. |
| `rd` | shrinking + widening | **Ray.** Mixed perspectives. One component pulls in while the other pushes out. |
| `dr` | growing + narrowing | **Ray.** Mixed perspectives, opposite combination. Same dimensional cancellation, different order. |

**Snapshot vs change — the same data-vs-fused-unit question.** Picking
any two of these four terms gives you a **snapshot**: a measurement of
size at one moment. For example, `dd` (growing + widening) measures how
much ice there is right now. Complex number or area calculations give
you a snapshot of area or direction and length.

But a snapshot cannot measure change. To measure how the iceberg is
*changing*, you need all four terms — the full kernel. This is because
each axis needs a directed segment (a start state and an end state) to
describe how it changed. The vertical axis needs both growing and
shrinking to form a directed segment describing whether the iceberg got
taller or shorter. The horizontal axis needs both widening and narrowing
to describe whether it got wider or thinner. Together, the four terms
give you two directed segments forming an area that describes the
*change* on both axes simultaneously.

This is the same data-vs-dual-unit distinction from Axiom 19, applied
one grade higher. You could measure change with multiple snapshots (data
approach: record the area now, record the area later, compare). Or you
could use the full four-term kernel as a fused unit that inherently
captures change on both axes (dual unit approach). Both work. The fused
version is more physical — the structure of the number mirrors the
structure of the phenomenon.

**Why rays are not area:** When one component is expanding and the other
is contracting, they work against each other dimensionally. The expansion
and contraction cancel to produce a directed extent — a line from the
origin to a point — rather than enclosed space. This is why these terms
behave as **complex numbers**: they have length and angle, not area.
Volume can only be recovered from complex numbers through a formula that
accounts for all four quadrants, precisely because the terms act against
each other.

The engine resolves this kernel into two components:

    areaResult  = rr + dd     → net area (aligned result)
    rayResult   = rd + dr     → net directed extent (orthogonal result)

**Both equations are addition.** In this system, `rr` IS anti-area:
structurally the opposite of `dd`. Adding anti-area to area cancels
naturally, the same way adding a negative number to a positive one does.
The subtraction is in the structure of `rr` itself, not in the equation.

This IS complex multiplication. It was not introduced — it **falls out**
of the grade system. The "real part" (rr + dd) is the net area. The
"imaginary part" (rd + dr) is the net directional residue.

**An important subtlety:** regardless of which unit pair you choose for
fusion, the values attached to them can be positive or negative. So even
if you have chosen growing + widening units (the area case), a negative
value on one side creates the same uncertainty as the ray case — you
have to subtract one from the other. The unit tells you the *kind* of
measurement; the value tells you the *direction* of the reading. Both
matter for determining whether the result is genuine area or a residue.

### Axiom 22 — Higher Dimensional Promotion

New dimensions emerge from tension that cannot be expressed at the
current dimensionality. The recipe is recursive:

**0D → 1D:** A single measure frozen as a unit can only measure in one
direction. When the system encounters information that opposes this
direction (a second perspective, a feedback disagreement), it creates
a dual unit. The grade increases to 1. This is the transition from
"positive values only" to "directed intervals with sign."

**1D → 2D:** Two directed intervals are combined. The four-term kernel
(Axiom 21) produces area and rays. Area emerges specifically when both
components share the same dominance pattern (both expanding or both
contracting). Rays emerge when the patterns are mixed. The grade
increases to 2.

**2D → 3D:** Area has a natural flow (start size → end size). A line
whose dominance pattern is incompatible with the area's natural
expansion/contraction **breaks the symmetry**. The observer experiences
volume — space swept by an area along a symmetry-breaking line. The
grade increases to 3.

**General rule:** Dimension N+1 = Dimension N entity x symmetry-breaking
orthogonal extent. "Symmetry-breaking" means the new component has a
different dominance pattern than the existing entity.

**The search space collapses.** A machine navigating this system does not
explore arbitrary axes. It follows the forced path of tension resolution.
The only new dimensions that appear are those demanded by unresolvable
tension at the current grade.

### Axiom 23 — Cross-Modal Bonds and Weak Units

Tension does not discriminate by source. A light sensor with unexplained
extra readings and a food system with unexplained extra energy can form a
**weak bond** — a proto-unit that associates the two signals.

If a light turns on every time an alarm rings, a weak unit forms
associating light-increase with sound. This unit strengthens each time
the correlation holds and weakens each time light changes without sound
or sound occurs without light.

Over time, through multiple examples, these unit bonds strengthen or
weaken until there is an acceptable amount of meaning and the tension
stabilizes — or, if the correlation was spurious, the bond dissolves
entirely and the signals disconnect.

This mechanism explains how the system discovers which signals are
genuinely related and which are coincidental. The tension created through
prediction error is essentially confusion about where something is coming
from, or having extra unaccounted-for information. This confusion is
exported to any area that needs imported energy to explain something.
The bond is not a committed unit (Axiom 2) — it is a lifecycle that
remains in the Manipulate phase, repeatedly negotiating. If the evidence
supports it, the bond eventually reaches Resolve and becomes a true unit.
If not, it weakens through the reverse lifecycle (Axiom 11) and
disconnects.

---

## Part VII: Structure — Lifecycle Composition

Structural topology is what happens when multiple lifecycles compose.
A pin is a lifecycle site. A segment is the span between lifecycle sites.
A network of pins and segments is a network of lifecycle instances
interleaving and feeding into each other — each resolved symbol becoming
the observed measure of the next lifecycle.

### Axiom 24 — The Pin

Every structural transition happens at a **pin**. A pin is a lifecycle
site: the place where observation, experience, manipulation, or resolution
occurs.

A pin on a **host** (a composite span) splits the host at a position:

    Pin(host = Composite(start, end), position)
    → Inbound  = position - start
    → Outbound = end - position

The pin position is a value. The host is structure. The inbound and
outbound are **generated**: new elements that didn't exist in the inputs.
The pin is the site of generation.

### Axiom 25 — Merge and Branch

There are exactly two topological events. Everything else is traversal
along a segment between pins.

**Merge:** Two inbound segments → pin → one outbound. This is a
**forward lifecycle**: two things are observed together, experienced
through the kernel, manipulated through a law, and resolved into one
independent result. The zero-denominator intermediate state at the pin
is the Manipulate phase where the old units dissolve and the new unit
forms.

**Branch:** One inbound segment → pin → two outbound segments. This is
the **reverse lifecycle**: one thing is captured, cleaved, separated,
and disconnected into two.

### Axiom 26 — Segments and Flow

A **segment** is a grade-1 composite: `(start, end)`. It has direction
(recessive → dominant), extent, and is traversable.

When segments meet at pins, their **flow directions** determine the
physical character of the encounter:

| Configuration | Flow | Physical character |
|---------------|------|--------------------|
| **Convergent** | → ← | Two dominants meet. Maximum tension. Boundary, wall, collision. |
| **Divergent** | ← → | Two recessives meet. Branch, fork, source, choice point. |
| **Parallel** | → → | Co-directional. The space between is structure. |
| **Anti-parallel** | → over ← | Same line, opposite direction. Loop or standing wave. |

### Axiom 27 — Loops

A **loop** is a segment whose dominant end is identified with a recessive
end earlier in the structure: "this end IS that beginning."

A loop creates:
- A **period**: the number of ticks around the loop
- **Repetition**: traversal naturally recurs
- **Enclosure**: a loop bounds a region

A loop is a lifecycle that feeds its Resolve phase back into its Observe
phase. This is self-sustaining structure.

### Axiom 28 — Boundary Conditions from Structure

When traversal reaches the end of an axis, it creates tension. **The
orthogonal structure absorbs or fails to absorb that tension.** The
boundary behavior is determined by what exists on the structural axes:

| Orthogonal structure | Boundary behavior |
|----------------------|-------------------|
| Repeat | **Wrap.** Tension restarts traversal at the beginning. |
| Mirror | **Reflect.** Tension reverses traversal direction. |
| Expand | **Extend.** The boundary moves outward to accommodate. |
| Nothing | **Clamp.** Tension stays at the boundary. Accumulates. |
| Unresolved (u = 0) | **Promote.** Tension cannot be absorbed. May become a new dimensional direction (Axiom 22). |

### Axiom 29 — Dimensional Transition (Rivers and Lakes)

Structure can vary in dimensionality along traversal. A pin can connect
segments of different structural freedom:

- **Narrowing:** A 2D region ("lake") connects to a 1D path ("river")
  through an exit pin.
- **Widening:** A 1D path connects to a 2D region through an entry pin.

---

## Part VIII: Traversal and Time

### Axiom 30 — Traversal

A **mover** is an atomic element used as a cursor: `(currentTick, endTick)`.
The value is the current position. The unit is the route's extent.

Traversal is the act of advancing the mover from tick 0 to tick `endTick`.
Each step produces a new position — a new element that didn't exist at
the previous tick. Each step is a generative act.

Traversal IS the Experience phase made structural. The mover is an
observer being driven through a route by its internal or external
structure. The drive to advance — the "clock" — is structure, not will.

### Axiom 31 — Time Is Traversal

Time is not external. It is what you get when you traverse any ordered
structure.

Every composite has a recessive ("before") and a dominant ("after").
Moving from recessive to dominant IS time passing. The structure IS
the clock:
- Resolution (how many ticks) = temporal granularity
- Ordering (recessive before dominant) = arrow of time
- Extent (recessive to dominant distance) = duration

No seconds, no hours — just positions within a directed structure.

### Axiom 32 — Operations as Pinned Events

Operations are **pins on the traversal route**. An operation attachment
specifies:
- A **site** (structural position along the route)
- A **law** (what happens when the mover reaches this position)
- The law produces an **outbound result** (generated value)

The traversal machine is a timeline: structural positions with lifecycle
events that produce values as the mover passes through.

---

## Part IX: Laws

### Axiom 33 — Laws Are Abilities from the Manipulate Phase

Laws are not external rules. They are the abilities that emerge during
the Manipulate phase (Axiom 9), formalized and named:

| Law | Derived from | Character |
|-----|-------------|-----------|
| **Expand** (multiply) | Stretch ability | Continuous growth |
| **Accumulate** (add) | Attach ability | General combining |
| **Partition** (divide/ratio) | Comparison | Discrete splitting |
| **Boolean** (and/or/overlap) | Location + comparison | Co-presence |
| **Repeat** | Re-attach ability | Iteration |
| **Move** (translate) | Direction change | Shift position |
| **Reframe** (perspective) | Perspective from location | Change how values are read |
| **Fold/Interpolate** (run) | Experience/traversal | Traverse and derive intermediate values |
| **Split/Merge** | Attach/disconnect | Branch and join |
| **Associate** (tension) | Halt/tension | Relate unlike forces. Preserve without resolving. |

### Axiom 34 — Two-Level Law System

Laws operate at two levels:

**Implicit law:** Derived from the unit categories of the inbound axes.
What the axes "naturally want" to do.

**Explicit law:** Specified by the pin or operation. Can override the
implicit law. When the explicit law conflicts with what the axes
naturally support, **tension arises**.

### Axiom 35 — The Kernel as Natural Law

At grade 2+, the four-term multiply kernel (Axiom 21) is the **natural
law** for combining two directed structures.

- `dd` terms → area contribution (aligned)
- `rr` terms → anti-area contribution (aligned, opposite sign)
- `rd + dr` terms → directed extent contribution (orthogonal)
- Net result: `(rr + dd)` aligned + `(rd + dr)` orthogonal

---

## Part X: Views, Frames, and Collections

### Axiom 36 — A View Is Measurement Without Ownership

A **view** relates a subject to a frame:

    View(frame, subject) → read outcome

The frame is a composite. Its recessive child is the **calibration**
(the reference). Its dominant child is the **existing readout** (the
current measurement).

Reading commits the subject to the frame's calibration. The result is a
new element: the subject as seen through the frame.

### Axiom 37 — Frame-Driven Dispatch

When reading through a composite frame, each atomic leaf in the frame
tree determines its own behavior via unit sign:

- **Positive leaf** → value calibration at that position
- **Negative leaf** → structural organization at that position
- **Zero leaf** → generative demand at that position

### Axiom 38 — Families

A **family** is an ordered collection of elements read in one frame:

- **Frame**: the structural context
- **Members**: the values
- **Ordering**: whether member sequence matters

**Focusing** shifts what counts as structure vs value: a member becomes
the new frame, and the remaining members are reorganized within it.
Aspects are relational (Axiom 13).

---

## Part XI: Completeness

### Theorem 1 — Construction Completeness

From the bootstrap lifecycle (Axioms 15-16), the entire number system
is constructed:
- Measures → frozen units → calibrated numbers
- No real numbers assumed. All arithmetic is exact integer operations on
  value/unit pairs.
- Negative values emerge at grade 1, not grade 0.

### Theorem 2 — Dimensional Completeness

From the dimensional lifecycle (Axioms 18-22), all dimensions emerge:
- Grade 0: positive measures with a single unit (no direction)
- Grade 1: directed intervals with dual units (sign exists)
- Grade 2: the four-term kernel separates into area (2D) and complex
  lines (1D with rotation)
- Grade 3+: each grade produces the next dimension through symmetry
  breaking of the previous grade's kernel
- No dimensions are assumed. Each is forced by unresolvable tension.

### Theorem 3 — Structural Completeness

From lifecycle composition (Axioms 24-29), all topology is constructed:
- Merge and branch are the only two topological events
- All structures are networks of segments connected at pins
- Loops, boundaries, and dimensional transitions follow from
  pin topology and tension absorption

### Theorem 4 — Temporal Completeness

From traversal (Axioms 30-32), time emerges:
- Every ordered structure contains "before" and "after"
- Traversal actualizes the Experience phase into sequence
- Causation follows from ordering

### Theorem 5 — The Aspect Trichotomy Is Exhaustive

Every element in the system plays exactly one of three roles determined
by its unit sign. The multiplication table (Axiom 14) governs all aspect
transitions and is closed under composition.

### Theorem 6 — Lifecycle Universality

Every event in the system is an instance of the four-phase lifecycle
(Axiom 9) or a composition of such instances:
- The bootstrap is a lifecycle (Axioms 15-16)
- Dimensional emergence is a lifecycle per dimension (Axiom 22)
- Every merge is a forward lifecycle, every branch a reverse (Axiom 25)
- Every operation is a lifecycle at a pin (Axiom 32)
- Every view/measurement is a lifecycle (Axiom 36)
- Every boundary resolution is a lifecycle (Axiom 28)
- Loops are self-feeding lifecycles (Axiom 27)
- Cross-modal bonds are lifecycles in the Manipulate phase (Axiom 23)

### Theorem 7 — Observer-Structure Duality

The observer perspective and the structural perspective describe the same
events. Every structural phase has an observer correlate and vice versa.
No event exists that is purely structural or purely experiential.

---

## Summary: What Is Assumed vs What Is Constructed

**Assumed:**
- Positive comparable measures from an implicit zero exist (Axiom 1)

**Constructed (in order of emergence):**
1. Units from freezing measures (Axiom 2)
2. Atomic elements with value/unit pairs, non-negative (Axiom 3)
3. Perspective and direction from opposing calibrations (Axiom 4)
4. Intervals and negative values from perspective (Axiom 5)
5. The four-phase lifecycle (Axiom 9)
6. Dual units from opposing perspectives (Axiom 6)
7. Grade-1 composite elements (Axiom 7)
8. Three aspects from unit sign (Axiom 12)
9. Numbers from calibration to frozen units (Axiom 16)
10. Tension as active demand for information (Axiom 17)
11. Dimensional promotion through tension and opposing perspectives (Axiom 18)
12. Data vs dual unit representations (Axiom 19)
13. The four-term kernel: area, anti-area, and complex numbers (Axiom 21)
14. Higher dimensions from symmetry breaking (Axiom 22)
15. Cross-modal bonds and weak units (Axiom 23)
16. Topology from pins, merges, branches (Axioms 24-27)
17. Boundary behavior from structural absorption (Axiom 28)
18. Time from traversal of ordered structure (Axiom 31)
19. Laws from Manipulate-phase abilities (Axiom 33)

Nothing is assumed except that positive measures from an implicit zero
exist (and that not everything is orderable). Everything else — units,
numbers, direction, sign, dimensions, topology, time, area, complex
arithmetic, boundary conditions, laws, symbols, and the lifecycle
itself — is constructed through observation, experience, manipulation,
and resolution.

---

## Notes for Future Development

**Pins as machines.** The system constructs many machines at different
scales. A pin can be thought of as a mini-machine, and the extent it
traverses is another machine. Both can hold data, and their data can
affect each other — the pin's state influences how it reads the extent,
and the extent's structure influences how the pin advances.

**The sensory grounding.** The bootstrap can be understood through a
minimal sensory system: a creature with a single light sensor. A raw
reading is a measure (Axiom 1). Freezing a typical reading creates a
unit (Axiom 2). Feedback disagreement creates tension (Axiom 17).
Routing a second signal to fill the tension creates a dual unit and
grade-1 structure (Axiom 18). Calibrating "normal change" from two
grade-1 intervals creates grade-2 structure (Axiom 21). Cross-modal
associations (light correlating with food) form weak bonds that
strengthen or dissolve through evidence (Axiom 23). The entire
dimensional hierarchy emerges from a single sensor interacting with
its environment, not from stated axes or assumed spaces.

**Grade 2 objects and lossy compression.** A grade-2 unit of change
(e.g., "normal amount of increase") is composed of two grade-0 values
but is its own thing. It could theoretically measure the change of
anything within its resolution — change of pressure, hunger, position.
But decomposing it does not recover the original measurements: a large
amount of light with a small change looks the same as a small amount of
light with a large change, and you might decompose it into pressure or
position instead of light. Symbol creation is lossy (Axiom 11).
