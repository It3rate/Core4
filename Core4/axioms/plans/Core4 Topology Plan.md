# Core4 Topology Plan

This note captures the current direction for moving from isolated carrier and
basis diagrams toward connected site and relation structures.

It does not replace the main axioms. It translates them into an implementation
path.

## 1. Canonical Stance

Topology in Core4 should remain Core4-native.

- no connector enums as the source of truth
- no UI-only shape labels as the source of truth
- no second ontology for "diagram elements"

The canonical structure should be built from:

- sites
- live relations
- frozen bases
- attachments
- orthogonal flows

## 2. Two Layers

The current best model is a two-layer structure.

### Layer 1: Site Incidence Topology

This is the canonical topological layer.

A site is described by the supports incident to it:

- which carriers meet at the site
- which supports are incoming
- which supports are outgoing
- which carriers continue through the site
- which carriers terminate at the site

This layer should be enough to express:

- continuation
- terminal attachment
- branch
- join
- later loop closure

These should be derived numerically from incident supports, not assigned by
enum.

### Layer 2: Aligned Placement And Geometry

This is the geometric layer built on top of incidence.

It answers questions such as:

- is this site an endpoint or an interior point on a support
- where along a support is the site pinned
- is an attachment centered, offset, mirrored, or asymmetric
- what span lies between two sites on the same support

This layer is necessary because coarse graph topology collapses meaningful
Core4 distinctions.

For example:

- a `T` and an `H` both contain continuation plus terminal attachment
- an `A` and an `H` may look similar under a stripped graph view
- midpoint and endpoint attachments only remain meaningful if aligned placement
  is preserved

So the graph-like view is useful, but it must be derived from richer Core4
structure rather than replacing it.

## 3. Supports, Strands, And Local Carriers

There is an important role distinction, but it should not become a new
ontology.

- some supports act like stable local value carriers
- some supports act like negotiated strands between sites
- some negotiated strands later harden into reusable supports

These should remain one mechanism.

A strand is not a second kind of carrier. It is the same support machinery used
in a more relational role:

- carrying unresolved tension
- carrying resolved ratio through another denominator
- carrying projection, coefficient, or negotiated access
- exposing a support another site can inspect

This fits the current engine better than introducing a separate symbolic type
for strand versus carrier.

## 4. Midpoints, Pins, And Hardening

A midpoint on an intermediate strand is meaningful.

At first it may be only an inspection position.

If that position is pinned strongly enough, it becomes a real site. Then one
support relation becomes two incident relations around that site.

So:

- landmarks on a strand are allowed
- a midpoint becomes canonical only when pinned as a site
- later hardening may promote that local site into a stable reusable support

This is how crossbars, hinges, and interior attachment points should emerge.

## 5. Shape Reading

The first shape vocabulary should stay descriptive rather than symbolic.

- `T`: one continuing carrier plus one terminal carrier at a site
- `H`: two such sites connected by another support
- `A`: similar incidence in places, but not the same once apex meeting and span
  geometry are preserved

So early topology work should focus on site incidence and support continuity,
not on letter-shape labels.

## 6. Stage Plan

### Stage 1: Site Incidence

Add a reusable core topology layer for one site:

- incident support legs
- per-carrier incoming and outgoing reading
- continuation versus termination as derived numeric facts

This stage should work directly with real `Site`, `Relation`, and `Flow`
objects.

### Stage 2: Support Placement

Add the aligned placement layer:

- pin position on support
- endpoint versus interior reading
- relative span and centeredness

This is the target state for the current pass.

### Stage 3: Connected Multi-Site Structures

Once site incidence and aligned placement both exist, connect them into larger
site and relation topologies:

- connector primitives
- chains
- forks
- joins
- later cycles and boundary behavior

### Stage 4: External Inspection

Traversal should ultimately live in a general external inspector rather than in
site-specific or enumerable-unit-specific cursors.

That inspector should be able to:

- move along supports
- split and merge perspective at branches and joins
- collect readings at pins and spans
- perturb or compare structure without changing ontology

## 7. Immediate Rule

When deciding whether a new concept belongs in topology:

- if it is about what meets what, it belongs in site incidence
- if it is about where on a support something is pinned, it belongs in aligned
  placement
- if it is only a simplified diagram rendering, it should stay derived
