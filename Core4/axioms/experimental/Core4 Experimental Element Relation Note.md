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
- `Multiplicity`

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

Multiplicity still lives on the relation edge and applies to the attached side.
This matches the current intended reading:

- start from the existing relation context
- attach one more thing
- repeat that attached participation some number of times

So the current edge defaults are explicitly:

- merge scope = `Node`
- commitment = `1`
- multiplicity = `1`

while explicit whole-merge relations still need to ask for `Whole` directly.

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
- that landmark does not become a measurement origin until another measure
  relates through it
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
- `MergeScope`
- `CarrierNovelty`
- `DomainNovelty`
- `Multiplicity`

So the signature does not try to summarize the whole relation into one named
pattern yet. It preserves the ordered participation history directly.

### Polarity

Every experimental element exposes a polarity, even if it is neutral.

The signature records the polarity of each attached element directly. This keeps
room for later directional patterns like:

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

`Multiplicity` remains on the relation edge and applies to the attached side.
It is stored per added element, not once for the whole relation.

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
