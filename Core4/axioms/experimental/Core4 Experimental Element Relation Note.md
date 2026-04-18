# Core4 Experimental Element Relation Note

This note captures the current experimental direction after backing away from a
separate address-based freeze encoding.

The main conclusion is:

- everything that can participate in freezing should be an element
- freezing itself should be one ordered relation between elements
- a relation must also be an element

This keeps the substrate small and lets roles emerge by inspection rather than
by building a separate primitive type for every concept.

## 1. Experimental Goal

The goal is to reach a simpler substrate behind:

- pin
- join
- unit
- sample attachment
- directed segment
- area / complex
- self-interaction

without introducing bespoke primitives for each one.

The intended rule is:

- one shared relation mechanism
- named structures discovered from relation patterns

## 2. Invariance, Noise, And Freeze

Under this experimental model, the deeper primitive is not "object" but
observed invariance. The default state is uncalibrated variance, not order.

The system first distinguishes:

- things that appear noisy
- things that appear ordered

Order means some relational attribute is observed to remain invariant across
different contexts. That invariance is the stronger claim, because it survives
changes of perspective and interpretation. Big is always larger than small.

Freezing, pinning, or fusing is the next claim layered on top of that:

- something is being treated as invariant over time
- but only to some degree of stickiness

So a freeze is not magic permanence. It is a promise, or an observation, that a
relation holds strongly enough to be reused before time and later tension erode
it. A big mountain may become a small hill, but not by tomorrow.

In that sense:

- conceptual ordering is the deeper invariance
- freezing is temporal invariance with commitment

This is why time can relax a freeze, but should not automatically destroy the
underlying idea of what it means to calibrate variance.

## 3. Elements

At this stage an element is just:

- something that can stand at one end of a relation

The experimental marker is `IElement`.

This is intentionally weak. It does not say what the element *is* yet. It only
says the thing can participate in structural freezing.

In the current Core4 code, obvious candidates include:

- `Measure`
- `Carrier`
- carrier-local `Site`
- live or frozen support aliases like `Relation` and `Basis`
- support-shaped structures like `SupportStructure`, `SampleSet`, `Attachment`,
  and `Placement`

The important part is not that every current type is final. The important part
is that freezing does not require a different endpoint type for every case.

The experimental slice now uses its own self-contained primitives:

- experimental `IElement`
- experimental `Measure`
- experimental `Carrier`
- experimental `Relation`

So the experiment no longer depends on the wider Core4 object set just to
express the primitive freeze graph.

## 4. Relations

The current experimental primitive relation is a small immutable chain.

It starts from one attached element plus merge scope, then grows by `Add(...)`
calls that append:

- `Attached`
- `MergeScope`
- `Commitment`
- optional `Transform`
- optional `Aspect`

So every relation remains ordered, but the order is now simply the order in
which elements were attached to the relation.

There is also a convenience constructor that takes:

- one base element
- then additional attached elements

and defaults those participations to:

- `Node` merge scope
- commitment `1`
- multiplicity `1`

That is only a surface convenience. Internally the relation still grows as an
ordered chain of attachments.

Multiplicity still exists, but it is now treated as construction-time sugar
rather than final stored structure.

This matches the current intended reading:

- start from the existing relation context
- attach one more thing
- optionally repeat that attached participation some number of times

So the current edge defaults are explicitly:

- merge scope = `Node`
- commitment = `1`
- multiplicity = `1`

while explicit whole-merge relations still need to ask for `Whole` directly.
If multiplicity is greater than `1`, the current experimental builder expands
that into repeated relation participations instead of storing a single edge with
a multiplicity count on it.

## 5. Relation To Relation

A relation is itself an element.

That means:

- carrier to carrier is allowed
- carrier to measure is allowed
- measure to measure is allowed
- relation to relation is allowed
- relation to measure is allowed
- relation to carrier is allowed

This is the key simplification for second-order fusion.

Self-interaction should not need to be read as "point collapsed onto point."
It can be read as:

- one relation frozen to another relation

The same simplification also helps sample attachment, support alignment, and
carrier-instance alignment.

## 6. Roles By Inspection

The experimental layer should avoid directly storing labels like:

- origin
- unit
- join
- attachment
- directed segment
- area
- complex
- self-interaction

Instead those roles should come from inspection.

Examples of the intended reading:

- one carrier related to one measure can behave like a landmark-capable point
- that landmark does not become a measurement origin or unit by itself
- a second measure on the same carrier is what gives the relation a unit delta
- so values like `100 -> 200` may define a positive unit just as well as
  `0 -> 100`
- one second-order relation between two carrier-point relations on the same
  carrier can behave like a unit candidate
- one second-order relation between two carrier-point relations on different
  carriers can behave like a join candidate
- one relation between two unit candidates can further behave like a
  directed-segment, area-like, complex-like, or self-interaction candidate
  when the carrier/base/alignment conditions line up
- same-carrier or same-domain comparable point pairs may also read as self,
  aligned, or opposed depending on their attached locations
- second-order relation-to-relation freezes can behave like directed segment,
  area, complex, or self-interaction depending on the underlying elements

For the moment, that inspection layer has been intentionally stepped back.
The current code only preserves primitive relation participation so we can make
sure the substrate is right before naming too many derived patterns.

## 7. Carrier Relations

This model also leaves room for carrier-to-carrier relations.

Examples:

- two carrier instances with the same carrier identity may relate numerically
- two different carriers in the same domain may relate semantically before they
  are numerically pinned
- later additional relations can pin meeting points such as shared landmarks or
  matching unit relations

So carrier alignment does not need a special-case mechanism outside the same
relation substrate.

## 8. Samples

This model matches the current sample intuition well.

- raw sample = weakly or unfrozen element/relation content
- lightly attached sample = low-commitment relation onto support
- calibrated sample = stronger relation patterns around that attachment
- hardened sample = structure strong enough to be reused reliably

This means sample and support can remain one ladder of structure rather than
two disconnected ontologies.

## 9. What This Experimental Layer Is Not Doing Yet

This stage is intentionally *not* trying to solve:

- a canonical graph container
- address allocation
- numeric traversal over the whole experimental graph
- promotion rules from sample to topology
- full inspection taxonomy

For now the goal is only to nail down the primitive shape:

- `IElement`
- ordered `Relation`
- relation also being an `IElement`

## 10. Immediate Open Questions

These remain open for later passes:

- which current Core4 types should permanently remain `IElement`
- whether commitment should stay on the relation directly or later become a
  derived freeze over another layer
- how much alignment/orientation should be readable directly from existing
  Core4 numeric structure versus extra experimental relations
- what the smallest useful inspection layer should look like

## 11. Current Code Shape

The current experimental slice is intentionally tiny:

- `Core4.Experimental.IElement`
- `Core4.Experimental.Measure`
- `Core4.Experimental.Carrier`
- `Core4.Experimental.Relation`
- `Core4.Experimental.RelationSignature`

Everything else should be built outward from that only if inspection proves it
is truly necessary.

## 12. Current Signature Shape

The signature is now literally one participation tuple per attached element in
the relation chain.

Each entry currently carries:

- `Polarity`
- `Transform`
- `MergeScope`
- `CarrierNovelty`
- `DomainNovelty`

So the signature does not try to summarize the whole relation into one named
pattern yet. It preserves the ordered participation history directly.

### Polarity

Every experimental element exposes a polarity, even if it is neutral.

Bare measures and carriers are currently neutral. They do not declare their own
dominant or recessive status.

Polarity is instead meant to arise from calibrated relation direction. In the
current experimental pass, the first concrete example of that is a carrier-local
unit: if two measures on the same carrier define a positive delta, the unit
reads as dominant; if they define a negative delta, it reads as recessive.

That means `0` is not privileged as an origin. A unit can be formed from:

- `100 -> 200`
- `200 -> 100`
- `-40 -> 60`

just as naturally as from `0 -> 1`.

The signature records the attached element polarity directly, while the relation
itself may derive a nonzero polarity from its internal unit structure. This
keeps room for later directional patterns like:

- `DD`
- `DR`
- `RD`
- `RR`

without requiring the relation itself to name those combinations up front.

### Merge Scope

Each attached element must declare whether it is entering the relation as:

- `Whole`
- `Node`

There is currently no `Unknown` merge scope in the primitive relation. It is
always an explicit participation choice for that edge.

### Transform

Each attached element may also carry an optional transform element.

This is the current minimal representation of:

- reuse under inversion
- reuse under orthogonal change
- reuse under some later inspection or generative action

The important point is that transform is not stored as an enum operation. It is
itself another element, usually something small and numeric such as a measure.

At the current experimental stage:

- reusing the same element already preserves source lineage by reference
- `Transform` captures only the extra "reuse under transform" part

So the primitive relation does not need a separate stored source pointer yet.

### Aspect

Each attached element may also carry an optional aspect element.

Aspect is the current minimal way to say:

- this same lineage is participating in a different snapped context
- this same unit is being used through a different directional or structural
  selection
- this participation should keep its shared lineage while still remaining
  distinguishable from another use of that lineage

At the current experimental stage, aspect is intentionally weak:

- it is just another element
- it contributes carrier and domain context
- it does not yet imply a full inspection or path language

This keeps the primitive core small while still giving the same unit or same
carrier a way to participate differently in different structures.

### Carrier And Domain Novelty

For each attached element, the relation calculates whether that element brings:

- a `New` carrier or an `Existing` carrier already seen earlier in the relation
- a `New` domain or an `Existing` domain already seen earlier in the relation

If an attached element introduces no carriers at all, its novelty now falls back
to the current relation context:

- first element in an empty relation still reads as `New`
- later carrier-less elements read as `Existing`

This keeps novelty two-state instead of introducing a third "unknown" category.

The relation also caches the carrier/domain sets it has already accumulated, so
adding a new element does not need to recalculate the whole signature from
scratch. When a nested relation is attached as one element, the outer relation
uses the nested relation's cached carrier/domain context instead of rewalking
all of its grandchildren.

### Multiplicity

`Multiplicity` remains available on the builder surface, but it no longer
persists as final structure on each edge.

If multiplicity is greater than `1`, relation construction expands that
participation into repeated entries. This keeps the final structure closer to
the real lineage:

- one repeated relation
- another repeated relation
- another repeated relation

rather than one opaque edge carrying only a count.

### Deferred Inspection

Named readings like:

- point
- unit
- join
- segment
- area
- complex
- self-interaction

are intentionally deferred for now.

The current experiment is focused on:

- getting the primitive elements right
- getting ordered relation growth right
- storing only the participation facts we are confident about

before building a larger derived inspection layer on top.

## 13. Construction, Repetition, And Transform Lineage

The next conceptual layer is not a bigger pattern taxonomy. It is construction
lineage.

Two visible structures may carry the same final numbers while still being
different objects because they were constructed differently.

The important distinction is not only:

- what values are present

but also:

- whether a relation is new
- whether a relation is repeated
- whether a relation is repeated under transform
- whether a relation is built from a prior result

This is the difference between:

- three equal-looking siblings placed side by side
- one invariant relation repeated three times
- one invariant relation repeated after inversion
- one invariant relation repeated after orthogonal rotation
- one relation built recursively from an earlier result

### Node To Node

Relating two nodes gives a jump or delta-like structure.

Example:

- pin a landmark on a carrier
- relate a second landmark to it as `Node`

This gives a discrete offset.

If that participation is repeated, the repetition is additive in character:

- one jump
- another jump
- another jump

Even if the jumps happen to have equal values, they are still side-by-side
unless the construction explicitly says they are the same repeated relation.

### Node To Whole To Node

If the relation to the second landmark is taken as `Whole`, the reading changes.

The relation is no longer only a jump between endpoints. It is a full transform
over the interval.

Repeating that kind of whole participation is compositional rather than merely
additive. This is the seed of multiplication, powers, scaling, and growth.

So the same primitive relation tools can produce two very different behaviors:

- `Node` repetition behaves like repeated deltas or additive accumulation
- `Whole` repetition behaves like repeated transforms or generative composition

### Anchored Interval Versus Sliding Interval

There is also an important distinction between:

- pinning one landmark as a privileged origin and relating another landmark to
  it
- relating one landmark to another without first privileging either one as an
  origin

The first case gives an anchored interval or unit candidate:

- position is stabilized through the chosen origin
- later values can be read relative to that pinned place

The second case gives a sliding interval:

- the length is frozen
- but the absolute position is not
- the pair can move together while preserving the same delta

So not every invariant interval is automatically an origin-based unit. Some are
better read as movable windows, kernels, offsets, or span-preserving couplings.

This distinction does not require a second primitive mechanism. It falls out of
the same relation substrate depending on whether one endpoint has first been
privileged as a reusable pinned site.

The current experimental code does not yet make landmark motion propagate
through those couplings automatically. That kind of "move one end and the other
rebalances to preserve the interval" behavior likely belongs in a later
runtime/tension layer rather than as special-case methods on the immutable
primitive relation itself.

### Reuse Versus Equality

Equal-looking units are not automatically the same unit.

There is an important difference between:

- copying a unit
- repeating the same unit
- deriving a new unit from an existing unit under transform

This difference matters even when the visible values match exactly.

For example:

- `A * B * C` where `A`, `B`, and `C` happen to be equal
- `A^3`

may land on the same visible values in special cases, but they are not the same
lineage. The second one preserves invariant reuse. The first one does not.

### Transform Reuse

Some important structures arise not from mere duplication, but from repeating
an existing relation under transform.

Examples:

- directed segment = unit repeated with inversion
- area-like structure = unit repeated under orthogonal transform
- complex-like structure = unit repeated under orthogonal transform with
  opposed orientation
- self-interaction or power = whole relation repeated against itself

So a directed segment is not merely "two equal and opposite values." It is a
unit repeated with inversion. Likewise an area is not merely "two lengths." It
is a repeated unit under an orthogonal change of participation.

This remains true at higher grades as well. A folded object may behave like one
degree of freedom, but its lineage still depends on whether it came from:

- repeated same basis
- repeated transformed basis
- copied equal-looking basis
- recursive reference to earlier result structure

### Recursive Reuse

The relation substrate should also leave room for structures that reference
results, not only inputs.

This is where richer growth systems come from:

- powers
- chained expansions
- logarithmic inspection of repeated whole transforms
- additive recurrences like Fibonacci-style `(n-1) + n`

Again the important distinction is structural:

- direct sibling participation
- exact repetition
- transformed repetition
- result-referential repetition

These are not the same object, even when they can collapse to the same visible
numbers after folding.

### Shared Origin Versus Football-Field Construction

A related distinction is origin lineage.

Two opposed units on the same carrier do not automatically form a true directed
segment. If they do not share the same origin lineage, they are closer to a
football-field style dual-coordinate structure than to one shared-origin
segment.

So there is a real difference between:

- same reused origin
- distinct origins explicitly joined
- distinct origins merely equal in value

The current experimental code does not fully encode this lineage yet. The note
captures it here because this distinction will matter once derived inspection
and fold behavior grow more precise.

### What The Current Code Captures Versus What It Does Not

The current primitive experimental relation already captures:

- ordered participation
- `Node` versus `Whole`
- multiplicity
- carrier and domain novelty
- relation-local polarity
- optional transform lineage
- optional aspect participation

What it does not yet fully capture is the stronger lineage question:

- whether an equal-looking relation is a true repeat
- whether it is a transformed repeat
- whether it is a fresh sibling
- whether it is built from prior result structure

That is likely the next major conceptual seam after the current primitive pass.

## 14. Lineage, Participation, And Snapped Landmarks

The current experiment now needs one more distinction:

- lineage
- participation instance
- snapped landmark

These are not the same thing.

### Lineage

Lineage is the broad kind of freedom:

- distance
- time
- direction
- temperature

It is the thing that persists even when structures reuse it in many contexts.

### Participation Instance

A participation instance is one use of a lineage inside a structure.

Examples:

- house width
- store-trip distance
- horizontal distance inside one area
- vertical distance inside that same area

These may all draw from the same lineage while still being different uses.

The current minimal experimental hook for this is `Aspect` on the relation
entry. The aspect does not replace the carrier lineage. It qualifies how that
carrier or unit is participating in the current relation.

### Snapped Landmarks

A snapped landmark is a common frozen point on some lineage.

The important correction is that a "4-way direction" is not best modeled as a
carrier with only four ticks. It is better modeled as:

- one real directional lineage
- with whatever fine resolution we want
- plus common frozen landmarks at places like `0`, `90`, `180`, and `270`

Then a structure can snap to those landmarks while still leaving room for finer
directional resolution later.

This is similar to common bolt sizes:

- the measurable space can be finer
- but some standard landmarks are common and heavily reused

### Snaps Versus Categories

Snapping should not be read as loss of the underlying carrier resolution.

A carrier may remain high-resolution while later structures classify ranges of
that carrier into broader frozen regions.

So there is a useful distinction between:

- a precise landmark on a rich substrate
- a common snapped landmark that many structures reuse
- a broad category whose whole interior maps to one classification tick

This means a category like:

- `horizontal`
- `vertical`
- wrench size families
- even broad cases like `apple` / `not apple`

does not require the underlying carrier itself to become coarse.

Instead:

- the carrier can stay fine-grained
- broad categories can still be pinned over it
- these categories can be accessed by a low resolution, four tick enumerator
- confidence is usually strongest near the center of a category
- tension or uncertainty tends to rise near category boundaries

So "low resolution category" should usually mean broad frozen regions over a
rich substrate, not a poor carrier with too few expressible values.

### Width And Height

Under this model, width and height can be described as:

- the same distance lineage
- reused through different directional snaps

So a pure same-metric area does not require two unrelated distance carriers.
It can be expressed as one distance unit reused with different directional
aspects.

That still leaves room for a broader fused model where two independent distance
units are combined. The difference is structural:

- same lineage reused under different snaps
- versus distinct lineages or independently built units that only happen to be
  compatible

### Miles Per Hour

`mph` is a different case.

It still involves multiple lineages:

- distance lineage
- time lineage

Those are not merely different snaps of one shared lineage. They are different
lineages participating together before fold and normalization.

So:

- width/height often means same lineage plus different aspect
- `mph` means distinct lineages with different fold roles

### Access By Path

The current note also leaves room for later path-like access such as:

- second distance participation
- vertical distance participation
- nested relation path traversal

That is probably an inspection concern rather than a primitive storage concern.
The current minimal structure only needs to preserve the participation context
so that a later inspection layer can walk it.

In practice that likely means:

- keep lineage on `Carrier`
- keep participation context on relation entries
- let later inspection expose path-like selection over those entries and nested
  relations

This is the main reason `Aspect` is the minimal useful addition right now. It
preserves contextual participation without forcing a path system into the
primitive substrate too early.
