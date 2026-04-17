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

## 2. Elements

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

## 3. Relations

The experimental primitive relation is always:

- ordered
- `Base`
- `Attached`
- `Commitment`
- `Multiplicity`

So every relation has a direction.

Multiplicity lives on the relation and applies to the `Attached` side. This matches
the current intended reading:

- start from origin/reference
- relate toward the second thing
- repeat the second-side relation some number of times

## 4. Relation To Relation

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

## 5. Roles By Inspection

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

## 6. Carrier Relations

This model also leaves room for carrier-to-carrier relations.

Examples:

- two carrier instances with the same carrier identity may relate numerically
- two different carriers in the same domain may relate semantically before they
  are numerically pinned
- later additional relations can pin meeting points such as shared landmarks or
  matching unit relations

So carrier alignment does not need a special-case mechanism outside the same
relation substrate.

## 7. Samples

This model matches the current sample intuition well.

- raw sample = weakly or unfrozen element/relation content
- lightly attached sample = low-commitment relation onto support
- calibrated sample = stronger relation patterns around that attachment
- hardened sample = structure strong enough to be reused reliably

This means sample and support can remain one ladder of structure rather than
two disconnected ontologies.

## 8. What This Experimental Layer Is Not Doing Yet

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

## 9. Immediate Open Questions

These remain open for later passes:

- which current Core4 types should permanently remain `IElement`
- whether commitment should stay on the relation directly or later become a
  derived freeze over another layer
- how much alignment/orientation should be readable directly from existing
  Core4 numeric structure versus extra experimental relations
- what the smallest useful inspection layer should look like

## 10. Current Code Shape

The current experimental slice is intentionally tiny:

- root `IElement`
- `Core4.Experimental.Relation`
- `Core4.Experimental.RelationInspection`
- `Core4.Experimental.RelationSignature`

Everything else should be built outward from that only if inspection proves it
is truly necessary.

## 11. Current Signature Shape

The first numeric signature is tuple-shaped rather than packed into one code.

It currently carries:

- `ElementOrder`
  - `BaseOrder`
  - `AttachedOrder`
- `FreezeOrder`
- `CarrierPattern`
- `BasePattern`
- `ReadPattern`
- `AlignmentPattern`
- `FusionPattern`
- `Multiplicity`

This preserves direction while still giving one ordered structural summary.

`ReadPattern` is the current bridge between raw extraction and higher
classification. It is where the experimental layer records whether the raw
relation currently reads as:

- a carrier-point
- a carrier-alignment
- a point-pair
- a unit candidate
- a join candidate
- a unit-candidate pair

The intent is that classification helpers increasingly read through this
signature, while raw extraction helpers still return the underlying carriers,
locations, and nested relations when that detail is actually needed.

For now only three element families have canonical order:

- `Carrier`
- `Measure`
- experimental `Relation`

Other current `IElement` implementations are still treated as unknown-order
until we intentionally fold them down into this structural substrate.
