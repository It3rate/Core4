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
- `Core4.Experimental.RelationFold`

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

### Signature As Local Surface View

A signature should be read as the exposed local participation view of a
structure at one boundary.

It is not the whole structure. It is what that structure presents to a
neighboring participant from the current level.

So a signature should contain:

- what is directly visible at that boundary
- what degrees of freedom are exposed there
- what lineage or unit behavior is available there
- what inputs and outputs can sensibly couple at that level

and it should intentionally hide:

- deeper internal construction details
- lower-level reused substructure
- hidden carrier snaps or basis choices that have already been resolved below
  the current boundary

This means a structure may have many valid signatures at different depths or
from different local positions. A top-level signature is not "the" signature of
the whole object in every possible sense. It is one surface reading.

So for example:

- one level may see `pressure`
- another may see `force` and `area`
- another may see width and height or radius
- another may see reused units, aspects, or carrier lineage

Each of those is a signature from a different local structural view.

### Signature Versus Structure And Runtime

This experiment now treats three related layers separately:

- `Structure` = the full relation content
- `Signature` = the exposed local view of that structure at one boundary
- `Runtime` = parameterized or tension-bearing structure in motion

So fold is better understood as a structure-to-structure transform whose input
and output structures each have their own signatures. The signature is a useful
surface reading of that transform, but it is not the transform substrate by
itself.

### Fold As Ordered Structural Transform

The next layer above primitive relation is `RelationFold`.

A fold is not:

- a bigger signature
- a runtime rebalance rule
- a path or trolley API

It is one ordered `Relation` whose attached elements are themselves full
`Relation` structures.

So a fold currently preserves:

- first stage = input structure
- later stage(s) = output or intermediate structures
- per-stage `Whole` / `Node` participation, transform, commitment, and aspect
- object-reference reuse when the same structure is repeated

This means normalization should not be introduced as a second primitive
mechanism. It is one use of fold where a later relation stage is being treated
as the reusable normalized structure.

This keeps the layers separate:

- `Relation` = primitive structure
- `RelationSignature` = local surface reading
- `RelationFold` = structure-to-structure transform
- runtime rebalance = later tension layer

### Fold Output Versus Linked Output

Fold always produces a new output stage.

That does not mean every fold is only a one-shot calculation.

The current distinction is more like:

- snapshot fold = produce an output stage without preserving a live reuse link
- linked fold = produce an output stage while preserving the fold relation so
  later input changes can retune the output

So a fold may be:

- input to output directly, when read as a boundary-only or one-shot result
- input to fusion to output, when the fused scaffold is preserved strongly
  enough to keep the output linked

This means the output is still new structure, but it may continue to inherit
its behavior from preserved fold lineage.

That looks very similar to reused units:

- one-shot use gives a result without preserving live dependency
- reused live structure keeps later changes coupled through the preserved bond

So the current suspicion is that operational linkage and reused unit linkage
are not two different mechanisms. They are likely the same reuse mechanism seen
at different structural grades.

### Trolley View

It is fair to say that a trolley, at any one local position, sees a signature.

More precisely:

- a single trolley instance sees the signature exposed at its current local
  boundary
- if it moves across, inward, outward, or along a branch, it may arrive at a
  different local signature
- deeper inspection is therefore not "more of the same signature," but a move
  to another structural surface with its own exposed local view

At runtime the trolley also carries values, tension, and continuation state, so
what it experiences is a parameterized signature rather than a purely static
one. But structurally the right mental model is still:

- one trolley position
- one currently exposed local signature

with motion required to see something else.

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

### Merge Scope As Local Bond Mode

`Node` and `Whole` should not be read as permanent labels for the whole
structure.

They are better read as local participation modes describing how one side is
meeting the current bond.

So the same lineage or same unit may participate as:

- boundary-only
- extent-wide
- jump-like
- continuous

depending on the current local relation or fold stage.

This is important because the current intuition is not:

- "this object is a Node object forever"
- "this object is a Whole object forever"

but rather:

- this attachment is meeting the current bond at its boundary
- this attachment is meeting the current bond through its whole extent

The current primitive still stores merge scope per participation entry.

The richer meaning likely appears one layer up when two participations are read
together through a local bond or fold:

- `Node` to `Node`
- `Node` to `Whole`
- `Whole` to `Node`
- `Whole` to `Whole`

Those should not be collapsed too early because they likely distinguish:

- jump
- span or segment hardening
- point shifted by span
- span shifted or re-anchored to point
- additive accumulation
- multiplicative co-integration

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

### Relate Versus Fuse At Each Grade

The same structural distinction appears at multiple grades.

At a low grade, two points can be:

- related boundary to boundary, which reads like a jump or delta
- fused strongly enough that the space between them hardens into a span or
  segment candidate

At a higher grade, two spans can be:

- related while remaining externally distinct
- fused whole to whole

This gives the current intended ladder:

- point related to point = jump-like or teleport-like offset
- point fused with point = span or segment candidate
- span related to span = additive or unfused family
- span fused with span = multiplicative or compositional family

This should not be read as four unrelated primitives. It is one repeating bond
distinction:

- relate = keep the sides distinct while coupling them
- fuse = let the sides co-integrate through the bond

The important correction is that sequential and simultaneous are probably not
different mechanisms in the system.

Instead, the current intuition is:

- all execution is alternating local integration
- the two sides take turns
- a point-like participation only has one available value or tick to integrate
- a span-like participation has many local ticks to integrate

So the felt difference between "teleport" and "continuous" is not a second
execution engine. It is the difference between:

- one local value collapsing the integration immediately
- many local values making the same alternating process appear continuous

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

### Whole To Whole

If the participation is taken as `Whole`, the reading changes.

The relation is no longer only a jump between endpoints. It is a full transform
over the extent being coupled.

Repeating that kind of whole participation is compositional rather than merely
additive. This is the seed of multiplication, powers, scaling, and growth.

So the same primitive relation tools can produce two very different behaviors:

- `Node` repetition behaves like repeated deltas or additive accumulation
- `Whole` repetition behaves like repeated transforms or generative composition

This still does not mean every whole-to-whole fusion is scalar multiplication.
It means multiplication should live in this fused family rather than in the
boundary-only family.

At the execution level, this can still be read as alternating turns rather than
true simultaneous update.

For example, if two extents are fused:

- one side contributes a local tick
- the other side contributes the next local tick
- each local piece is stretched or compressed through the bond
- the result appends into the growing total

So "fused" should not be read as a different clock. It is a different local
bond rule applied during the same alternating integration process.

This also helps with the apparent tension between `Node` and `Whole`.

- whole exposure means expose the full available extent to the bond
- node exposure means expose only the boundary participation to the bond

Neither one says whether the fold must update "all at once."

If the exposed structure has only one available piece, then even a whole
exposure may collapse in one step.

If the exposed structure has many local pieces, then whole exposure naturally
appears piecewise under the same fold semantics.

### Mixed Local Bonds

The mixed cases should remain meaningful as well.

Current intuition:

- `Node` to `Whole` can read like a point being shifted by a span
- `Whole` to `Node` can read like a span being shifted or re-anchored to a
  point

These mixed cases are important because they let the same substrate express:

- pure jump
- pure continuous span
- jump into span
- span into point

without introducing a second primitive mechanism.

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

### Orthogonality Versus Bond Mode

Orthogonality still matters independently of relate versus fuse.

Orthogonality answers questions like:

- which freedoms are independent
- which participations are aligned versus orthogonal
- which aspect or lineage a participation belongs to

Bond mode answers different questions:

- are the sides meeting boundary to boundary
- are they meeting through their whole extents
- are they only appending
- are they also stretching or compressing through fusion

So a complex-like or 2D object can be built from:

- two jump-like participations
- two continuous participations
- one jump-like participation and one continuous participation

This is important for examples like:

- race handicap or head-start style offset, where one participation may behave
  like a discrete jump while another remains a continuous running span
- prior work versus next work, where both participations may remain continuous
  while still being distinguished by orientation, aspect, or fold role

So orthogonality should not be collapsed into "jump versus continuous." They
are separate distinctions.

### Multiplication Versus Ratio Projection

One open question remains around multiplication-like fold.

The current Core4 system still often exposes a ratio-like projection shaped as:

- numerator
- denominator

That is useful as a compatibility or measurement read, but it should not be
mistaken for the canonical structure of fused whole-to-whole participation.

The current suspicion is:

- ratio-like division is one later projection of structure
- multiplication-like behavior should arise from fused co-integration in fold

More precisely, the current structural intuition is:

- related operation = one side is expanded by the whole other value in one jump
  and appended to the total
- fused operation = local ticks from each side meet through the bond, stretch or
  compress each other, and append into the total

This gives the current family reading:

- related + normal = additive scaling or addition
- related + inverse = removing or subtraction
- fused + normal = multiplicative stretching
- fused + inverse = divisive or compressive stretching

So addition and multiplication are not different in the sense of one being
"append" and the other being "integration." Both append to a total. The key
difference is whether local ticking also fuses through the bond before that
append.

### Raw Measure Tick Basis

It is fair to say that even a raw measure exposes a primitive tick basis.

So a non-calibrated structure may still participate multiplicatively.

The difference is not that raw measure has no basis at all. The difference is
that raw measure lacks a stronger frozen external calibration such as a
stabilized reusable unit.

So:

- raw measure can still present a tick count or local repetition basis
- calibration makes that basis reusable and interpretable across more contexts
- multiplication-like fold may therefore act on non-calibrated structure, even
  if the result is weaker or more local than calibrated multiplication

### Host Basis And Inserted Structure

The cleanest current reading of multiplication-like fold is:

- the first operand provides the host basis
- the second operand provides the inserted structure

So if a value calibrated as `6` exposes six basis units, then `6 * 2` means:

- take the six host basis units from the first operand
- resolve each one through a whole copy of the second operand

This is why "replace each basis unit with the second operand" is a useful
physical picture.

It may be even safer to say:

- each host basis unit is filled by
- or resolved through

the second operand, because the first operand still contributes the host count,
layout, and lineage.

This same idea should scale beyond simple segment count:

- segment basis units may be filled by another segment
- area basis units may be filled by another structure
- volume basis units may be filled by another structure

The main structural operation is still basis substitution through fold.

### Inverse Compression

The inverse fused case can be read as compression rather than insertion.

For example, if `6` is being resolved through inverse fusion by `2`, the fold
can be read as:

- walk the host extent of `6`
- collapse it in runs of the partner extent `2`
- append one output basis unit per resolved run

This yields `3`.

So division-like fold need not be described primarily as numerator over
denominator. It can be described as inverse basis compression through the
partner extent.

### Equal Result, Different Structure

`6 * 2` and `2 * 6` may collapse to the same visible scalar result while still
remaining different structures.

The difference is not only order. It is host versus inserted role.

Examples:

- six host cells filled by twos
- two host cells filled by sixes

Those are not the same construction even if one projection later collapses them
to the same visible value.

This distinction matters because many later cases will depend on structure:

- aspect or orientation
- orthogonality
- later reuse and retuning
- higher-grade fold behavior

The same caution also applies to addition-like results. Equal visible totals do
not automatically imply identical construction lineage.

So this note does not yet claim that Core4 multiplication has been fully
identified. It only sharpens where it most likely belongs:

- not in boundary-only relation
- not in symbolic operator tagging
- but in fused whole participation whose later projections may still be read in
  several ways, including ratio-like ones

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
