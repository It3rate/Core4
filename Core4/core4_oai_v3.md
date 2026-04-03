# Core4 OAI V3

A rewrite of the Core4 starting axioms with one major change:

The primitive is not an extent.
The primitive is a measure on a common raw substrate.

An extent already has two ends, so it is not primitive enough. It is a later
construction from a pinned origin and at least one later measure, even when the
origin is an implicit zero.

This document takes that change as primary, while preserving the recent Core4
direction around:

- portable units
- shared-unit families
- sequential versus fused structure
- tension as preserved mismatch
- grade as structural composition
- degrees of freedom as a separate question

---

## Geometric And Physical Orientation

We often talk as if we begin with line segments, extents, or distances from
zero. That is convenient, but it hides a structural assumption.

A race timer, a light sensor, or a simple counter may behave as if zero is
already given. In that local system, readings are naturally nonnegative:

- elapsed time does not go below zero
- counted objects do not go below zero
- detected light does not go below the sensor floor

This does not mean signed space is primitive. It means the measuring system
already has a local origin, whether explicit or implicit.

To create a true interval, or a signed reading that can go both ways, the
system must first pin an origin measure, then compare later measures to it.
So the minimal constructive pattern is:

1. receive raw measures on one substrate
2. pin an origin
3. measure outward from that pinned origin
4. freeze one such outward span as a reusable unit

So what looks like a one-ended number is often shorthand for a hidden
two-point structure.

This changes the foundation:

- primitive systems begin with measures
- intervals are constructed
- signs are constructed
- units are frozen intervals
- higher grades are built from committed relations between such units

---

## Part I. Ground

### Axiom 1 - Primitive Measures

A **measure** is a single raw reading on one carrier within one local measuring
system.

Examples:

- one `long` value in the engine substrate
- one timer reading
- one light reading
- one pressure reading
- one sensor voltage

At the primitive level, a measure is not yet a two-ended extent. It is simply
"where the system currently reads."

The important point is that all such readings may share one low-level
representation. We do not need to bootstrap the storage type itself. What needs
bootstrapping is calibration.

The substrate may already contain finer ticks, but before calibration those
ticks are not yet usable public resolution. The system may know only:

- this reading is larger
- this reading is smaller
- this reading is the same

It does not yet know which distinctions are lawfully reusable as units.

The simplest local system has a default origin and therefore produces
nonnegative readings only. This is the first useful case:

    m >= 0

Negative values do not exist yet. There is no "below zero side" until an
interval with orientation has been constructed.

### Axiom 2 - Extents Are Derived From A Pinned Origin

An **extent** is not primitive. It is a derived interval:

    Extent = (origin, endpoint)

This is true even when the origin is omitted in writing.

But the origin is not just "there." It must be pinned first.

If the origin is fixed at local zero, then:

    Extent(0, a)

may be written simply as:

    a

But the hidden two-point structure still exists. The extent has:

- a starting point
- an ending point
- a direction from start to end
- a length

So whenever earlier Core4 language spoke of primitive extents, it is more
accurate to read:

- primitive measure when only one reading is present
- interval or extent only after an origin and a later measure are held together

### Axiom 3 - The First Unit Is A Frozen Outward Interval

A **unit** is formed when one outward interval from a pinned origin is preserved
for reuse.

The simplest case starts from a local zero:

    origin = 0    (already pinned)
    unit   = Freeze(Span(origin, a))

This produces a reusable interval from origin to anchor.

The more general case starts from two observed readings:

    origin = Pin(a)
    unit   = Freeze(Span(origin, b))

Now the first reading is preserved as origin, and the interval from that pinned
origin to `b` becomes the reusable calibration.

This is the correct pattern for carriers such as temperature, where the useful
calibration may not come from a metaphysically meaningful zero encountered by
the system itself.

This is also the first declaration of usable precision. If the committed unit
has size `10` in the raw substrate, the system is declaring that this interval
supports ten reusable divisions of precision for this unit family. The raw
substrate may contain finer distinctions, but until calibration the system has
not yet exposed them as lawful measurement.

Once frozen, the unit becomes portable:

- it persists
- it no longer changes size during ordinary use
- later values can attach to it
- it can outlive the original readings that formed it

If inspected while unattached, the unit is still just a preserved interval. It
is not actively measuring anything until a value binds to it.

### Axiom 4 - Storage May Stay Atomic While Meaning Is Relational

The engine may still store a calibrated reading as:

    Atomic(v, u)

but the bootstrap meaning is relational rather than solitary.

At minimum, every calibrated reading implies:

- a pinned origin
- a frozen unit interval from that origin
- a current reading expressed against that interval

So the storage may remain atomic, but the meaning is already composite in the
larger structural sense.

The unit may be free or bound:

- **Free unit:** the interval persists without a current value attached
- **Bound measurement:** a value is currently being read through the unit

This keeps the engine leaf small while allowing the bootstrapping story to stay
origin-first and interval-first.

### Axiom 5 - Sign Is Constructed, Not Primitive

Sign appears only after an oriented interval exists.

If the system pins `a` as origin and calibrates toward `b`, then "positive"
means "in the orientation from `a` toward `b`."

If another system calibrates from `b` to `a`, then the same physical carrier is
measured through the opposite perspective.

Both may be valid. They are not identical.

This matters for temperature-like carriers:

- one calibration may make larger raw readings mean hotter
- another may make larger raw readings mean colder

Even if the zero point and interval size are later aligned, the units still
carry opposite orientation. They are not automatically interchangeable.

A conversion law may relate them, but the conversion is extra structure. It is
not built into the units merely because they address the same world.

### Axiom 6 - Natural-Floor Measures And Bipolar Measures

Some carriers are naturally used with a default local zero:

- elapsed time
- counted quantity
- distance from a start point

These begin as positive-only measure systems.

Other carriers become most useful only once an interval is preserved and used
as a signed frame:

- temperature around a chosen baseline
- gain versus loss
- brightening versus darkening
- depth versus height

So the primitive system is positive-only by default, while signed measurement is
a later achievement of calibration.

---

## Part II. Measurement Reuse

### Axiom 7 - A Portable Unit Is The First Frame-Capable Object

A resolved unit is the first object that can experience many values across its
lifetime.

This does not mean the unit measures all of them at once. It means:

- the unit persists
- many values can separately bind to it over time
- those values become comparable because they share calibration

This is the smallest real frame in the system.

More complex objects may also serve as frames, but the portable unit is the
first one that does so lawfully.

### Axiom 8 - Shared-Unit Families Are Real But They Are Not Promotion

If many values attach to one unit, we get a **shared-unit family**:

    { v1/u, v2/u, v3/u, ... }

This family may be treated operationally as a collection, a column, or a data
stream. Under the hood, however, it remains a set of individual bindings:

    Bind(v1, u)
    Bind(v2, u)
    Bind(v3, u)

The family is real because the values are comparable through one denominator.
But the family does not automatically become a higher-grade object.

Reuse is not the same as fusion.

### Axiom 9 - Data With One Unit Is More Fragile Than A Committed Structure

Suppose we record daily low and high temperatures using one shared unit.

We may write:

    low1/u, high1/u, low2/u, high2/u, ...

This is workable, but the pairing rule is external. The measuring system must
impose:

- which values belong together
- whether the grouping is pairs or triplets
- what order the values are listed in

If one reading is missed, later pairings may all shift.

So shared-unit data is useful, but structurally fragile. It needs external
bookkeeping.

---

## Part III. Dual Units On One Carrier

### Axiom 10 - Opposed Units On One Carrier Can Form A Dual Unit

On one carrier, two opposite perspectives may be committed together into a
single paired unit.

For temperature, we may have:

- a cold-facing unit
- a hot-facing unit

These are not just two stored values. They are two oriented calibrations over
the same carrier.

When preserved together, they form a **dual unit**:

    DualUnit(recessive, dominant)

Here:

- recessive = the below-origin or opposite-facing side
- dominant = the above-origin or forward-facing side

These names do not imply better or worse. They identify the ordered roles of
the two sides.

### Axiom 11 - A Dual Unit Can Bind Two Partial Values Into One Number

Using a dual unit, a range can be written as one structured number instead of
two separate data values.

Example:

    2r + 5d

This means:

- 2 recessive partial units below the pivot
- 5 dominant partial units above the pivot

For a temperature range from `-2` to `+5`, the dual-unit reading stores the
whole interval in one object.

This differs from storing two ordinary values under one shared unit:

    -2/u, +5/u

because the dual-unit form commits the pairing into the number itself.

The result is less fragile:

- the two parts belong together intrinsically
- no external "pair these columns" rule is needed
- missing one later reading does not shift the internal structure of the others

### Axiom 12 - Addition Recovers Span In A Dual-Unit Range

In a committed dual-unit range:

    2r + 5d

ordinary addition recovers the total span:

    2 + 5 = 7

This is physically meaningful because the number already contains:

- the below-origin part
- the above-origin part
- their shared pivot

By contrast, two ordinary same-unit values:

    -2/u, +5/u

do not yield the span merely by being present together. To get the range, we
must add extra structure:

- an external rule
- or an extra stored value
- or a subtraction step applied afterward

So the dual-unit number is more physical in the Core4 sense. More of the needed
structure lives in the object, and less in external bookkeeping.

### Axiom 13 - Grade 1 Is The First Committed Dual Relation

A useful updated reading is:

- grade 0 = one calibrated value against one portable unit
- grade 1 = one committed opposition between two grade-0 sides on one carrier

This makes grade 1 the natural home of:

- intervals
- ranges
- directed changes
- low/high envelopes
- below/above pivot structure

The shared theme is not "there are simply two values."
The shared theme is:

- two opposed sides
- one committed relation
- one carrier
- one structured object

So a range written with a dual unit is closer to a genuine grade-1 object than a
pair of raw data values is.

---

## Part IV. Sequence, Change, And History

### Axiom 14 - Sequence Reuses Similar Arithmetic Without Becoming Fusion

If one carrier is read over time:

- previous light
- current light
- next light

the structure is sequential.

This can be stored as shared-unit data. It can also be committed into grade-1
forms such as:

- recent decrease
- recent increase
- low/high interval
- expected/current deviation

But sequence remains different from multi-axis fusion.

Sequence gives:

- before and after
- route-like order
- history
- directed change

Sequence does not automatically give:

- a co-present interior
- interchangeable axes
- a fused higher-dimensional object

So the arithmetic may resemble a two-axis system, but the topology is still
ordered encounter.

### Axiom 15 - History With One Unit And Grade 1 With A Dual Unit Are Distinct

There are two lawful ways to handle two readings on one carrier.

**Data form:**

    v1/u, v2/u

This is history or paired data under one unit.

**Committed form:**

    ar + bd

This is one structured number under a dual unit.

Both are valid. The second is more physical because the relation between the
parts has been committed into the object itself.

This is the same distinction as:

- a list of measurements that must later be interpreted
- versus a measurement form that already carries its own interior relation

---

## Part V. Tension, Association, And New Axes

### Axiom 16 - Tension Is Structured Mismatch

If a system cannot absorb some reading into its current frame, the result is
tension.

Examples:

- a runner measured against a unit that assumes progress toward a finish line,
  while the runner is moving away from it
- a light-driven behavior whose feedback return is consistently larger or
  smaller than expected
- a temperature model that keeps receiving residual variation from some other
  physical influence

Tension is not meaningless failure. It is structured non-settlement.

### Axiom 17 - Residual Tension Can Become Association

Not every mismatch deserves a new dimension. The useful ladder is:

1. residual tension
2. repeated correlation
3. weak association
4. committed association
5. portable reuse

If a residual repeatedly helps explain future mismatch, the system has found a
candidate relation worth preserving.

If the relation remains useful, it may commit into a new unit or axis.

If it stops helping, the bond should weaken or release.

This gives a physical explanation for learning, separation, and dimension
growth without requiring arbitrary invention.

### Axiom 18 - Orthogonality Means Preserved Non-Assimilation

Orthogonality should not be read only as another geometric direction.

The deeper rule is:

- some structured variation is real
- the current unit family cannot absorb it honestly
- the mismatch persists
- the mismatch is preserved instead of discarded

That preserved mismatch is orthogonal in the Core4 sense.

This includes:

- light mixed with heat
- signal mixed with danger
- pressure mixed with vibration
- expected reward mixed with unexplained excess

New axes appear when such persistent mismatch becomes committed explanatory
structure.

---

## Part VI. Grade Increase To Area

### Axiom 19 - Two Grade-1 Axes Must Exist Before Grade 2 Can Be Fused

To build grade 2, one interval-like axis is not enough.

We need two grade-1 structures, each already capable of opposition on its own.

For an iceberg-like example, suppose we have:

**Vertical dual unit:**

- shrinking
- growing

**Horizontal dual unit:**

- narrowing
- widening

Each of these is already a grade-1 way to express change or interval on its own
axis.

These support two related but distinct uses:

- a **snapshot basis**, where one vertical reading and one horizontal reading
  describe current size or position in two axes
- a **change basis**, where each axis carries a start and end state, producing
  two directed segments that can then be fused

### Axiom 20 - Two Orthogonal Axis Readings Give A Snapshot, Not Change

Choosing one vertical reading and one horizontal reading gives a two-axis
snapshot.

This is enough to describe:

- current width and height
- current size relative to a basis
- a present area-like state
- a present ray-like state

But it is not yet enough to measure widening, narrowing, growth, or shrinkage
amounts on its own. Those require at least two states per axis.

So any one vertical side plus one horizontal side can measure size, but not
change.

### Axiom 21 - Change On Both Axes Requires Four Underlying Values

To measure change on both axes, each axis must carry a directed segment:

- vertical start -> vertical end
- horizontal start -> horizontal end

This gives four underlying values.

Now the vertical axis can describe:

- growth amount
- shrinkage amount

and the horizontal axis can describe:

- widening amount
- narrowing amount

Fusing these two directed segments yields a grade-2 structure describing change
across both axes.

So picking any two orthogonal axis readings gives a size snapshot, while picking
all four start/end values gives change on each axis.

### Axiom 22 - Matched Segment Interactions Are Area-Capable

When the two directed segments interact, matched combinations are area-capable.

Examples:

- growing with widening
- shrinking with narrowing

In these cases the axes agree about the whole object's expansion or
contraction.

These are the matched cases:

- positive area
- negative area, or anti-area

### Axiom 23 - Diagonal Segment Interactions Are Ray-Capable

When the two directed segments interact diagonally, the axes act against each
other.

Examples:

- growing with narrowing
- shrinking with widening

Here larger magnitude does not tell us monotonically that there is more or less
ice overall. One axis enlarges while the other reduces.

These diagonal cases behave like rays rather than enclosed areas:

- they carry directed residue
- they do not directly provide enclosed content
- they align with the complex-like part of the kernel

This is why complex numbers are naturally read as direction and length rather
than as straightforward area.

### Axiom 24 - The Kernel Resolves The Whole, But The Segments Carry Axis Change

The full interaction of two grade-1 segments still resolves by the four-term
kernel:

    rr
    rd
    dr
    dd

The unit pairing determines the kind of contribution the fused basis can
support:

- matched terms are area-capable or anti-area-capable
- diagonal terms are ray-capable

The attached values determine the realized sign and magnitude of those
contributions.

But area and complex readings are summary views of the fusion. They give:

- a snapshot of enclosed content
- or a snapshot of net direction and length

They do not by themselves tell the explicit widening or shrinking amount on
each axis. That information lives in the component directed segments.

So the full structure is:

- the segments carry axis-specific change
- the kernel resolves their joint interaction
- the area and complex parts are lawful summaries of that interaction

---

## Part VII. Symmetric And Asymmetric Fusion

### Axiom 25 - Grade And Degrees Of Freedom Must Stay Separate

Grade tells us how many layers of committed opposition and composition were
required to build the object.

Degrees of freedom tell us how many independent adjustments remain available in
ordinary use after commitment.

These are related but not the same.

### Axiom 26 - Symmetric Fusion

A fusion is **symmetric** when its contributors are interchangeable in normal
use.

Area is the model case:

- width and height both contribute one of themselves
- either may be adjusted in ordinary use
- the fused object still behaves lawfully

### Axiom 27 - Asymmetric Fusion

A fusion is **asymmetric** when one contributor is more strongly locked into the
resolved unit than the other.

Examples:

- miles per hour
- pounds per square inch

These are still real fusions. They are not "half grades."
But they expose fewer free adjustments in ordinary use.

So the better distinction is not grade 1.5.
The better distinction is:

- structural grade
- symmetric versus asymmetric fusion
- surviving degrees of freedom

---

## Part VIII. Revised Construction Ladder

### Axiom 28 - The Updated Ladder

A compact statement of the revised growth process is:

1. primitive raw measure
2. pinned origin
3. first frozen interval / portable unit
4. calibrated value/unit pair
5. shared-unit family or history
6. committed dual-unit relation on one carrier
7. preserved tension across repeated mismatch
8. committed new association or axis
9. fusion of independent grade-1 axes into grade 2

This replaces the earlier habit of treating primitive extents as the true
starting point.

### Axiom 29 - What The System Now Assumes

The current minimal assumptions are:

- some local measuring act can produce a reading
- readings can be preserved
- preserved readings can be compared
- useful preserved relations may be committed for reuse

Everything else is constructed:

- extents
- intervals
- sign
- units
- ranges
- grade-1 dual structure
- higher-grade fusion
- area and anti-area
- ray and complex residue

---

## Part IX. Current Direction

The updated Core4 picture is now:

- the primitive is a measure on a common raw substrate, not an extent
- an extent is already an origin-plus-endpoint construction
- a unit is the first frozen outward interval from a pinned origin
- negative values are constructed from oriented calibration, not given at the
  bottom
- many values sharing one unit remain data unless their relation is committed
- dual units on one carrier give a more physical representation of ranges and
  intervals than raw paired data does
- sequential history and true fusion reuse similar math but are structurally
  different
- orthogonality means preserved explanatory mismatch
- grade 2 begins when independent grade-1 structures are fused, not merely when
  many values have been recorded
- the four-term kernel remains the right resolution law, but matched and
  diagonal terms are now explained through the physical meaning of the chosen
  unit orientations

This keeps the Core4 emphasis on lawful structural emergence, but starts from a
more primitive and more physical base.
