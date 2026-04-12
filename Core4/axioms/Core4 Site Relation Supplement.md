# Core4 Site Relation Supplement

This note sharpens five terms that were starting to blur together in the
engine and the axioms:

- site
- pin
- relation
- freeze
- attachment

It does not replace the main axioms. It clarifies the construction order they
already imply.

## 0. Carrier And Measure

Core4 should only reason over what is orderable.

That means **carrier** is structurally prior:

- a carrier is the ordered attribute or support on which relation may occur
- it may exist before any actual reading has yet been taken

A **measure** is then the first detected reading of some site on that carrier.

So there are two useful primitive readings:

- **structural**: carrier comes first, because it supplies the order
- **epistemic**: measure comes first, because it is the first thing observed

Both are valid, but for glossary and construction purposes it is often clearer
to read support-first:

- carrier
- site
- measure

This also exposes two related triples:

- pin
- origin
- relation

and

- freeze
- basis
- attachment

The first triple is the primitive live case.

The second triple is the richer reusable case.

They should not be treated as unrelated ontologies. They are the same general
pattern at different levels of commitment.

## 1. Site

A **site** is a referencable place where relation may occur.

For the current concrete engine pass, a site is expressed on one carrier by:

- a carrier identity
- a location measure
- a relation state

The relation state is encoded as a Core4 ratio:

- `0/0` = open site
- `m/0` = unresolved occupied site, or tension-bearing attachment potential
- `m/u` = resolved relation state

So a site is not just a point. It is a point together with its current
relational openness or commitment.

## 2. Pin

**Pin** should be treated primarily as a verb, not a noun.

To **pin** is to choose a site as stable enough to reference. Pinning does not
by itself create a basis. It only creates a privileged site.

This matters because the earlier engine collapsed:

1. selecting a site
2. relating it to another site
3. freezing that relation

into one object too early.

## 3. Relation

A **relation** is an ordered coupling between sites.

At the current engine level, the most primitive useful relation is:

- one pinned origin site
- one second site on the same carrier

This gives:

- a directed interval
- a delta
- an orientation
- a candidate denominator for later reuse

Relations are live. They can still be focused, compared, attached to, or left
unresolved.

## 4. Freeze

To **freeze** is to commit a live relation for reuse.

Freezing does not invent new content. It stabilizes an already encountered
relation so later values can be expressed against it.

This yields a **basis**.

So:

- site = where relation may happen
- pinning = selecting a stable site
- relation = coupling sites
- freeze = stabilizing a relation
- basis = frozen relation

## 4A. Resolution, Finitude, And Ticks

Core4 should only reason over what is both:

- orderable
- finite enough to inspect

So:

- infinity is not a legal calculable value
- unordered content is not a legal calculable value
- randomness or noise may still contribute measurable **amount**, but not an
  ordered internal structure unless that structure is itself made orderable

This implies every practical reading lives inside a finite window:

- a minimum extent worth distinguishing
- a maximum extent worth carrying

Those limits may come from physical hardware, from the surrounding system, or
from an explicit observer choice.

This matters immediately once a relation is frozen into a unit.

A frozen unit is not only a magnitude. It also implies a finite resolution for
inspection and reuse.

We can call those resolution steps **ticks**.

The simplest case is:

- one unit
- one tick per unit

But if finer inspection is available, the same unit may be divided into several
ticks. Then the denominator carries not just "which unit" is being used, but
also "at what committed resolution" it is being read.

So Core4 should generally not treat fraction reduction as a harmless default.

- `5/10` and `1/2` may agree in proportion
- but they do not necessarily express the same committed resolution

This gives a useful base case for normalized iteration:

- freeze one unit
- choose a resolution of `n` ticks
- the unit becomes traversable from `0/n` through `n/n`

For example, one unit at ten ticks yields:

- `0/10`
- `1/10`
- `2/10`
- ...
- `10/10`

This is the simplest finite normalized ladder. It provides a reusable way to
iterate within one frozen extent without appealing to infinity or to an
externally imposed continuous line.

## 5. Attachment

An **attachment** is a live binding of current sites into a basis.

An attachment is not a second kind of number system. It is a present relation
state being read through frozen structure.

For this reason, attachments should degrade into tension rather than error when
the basis is too weak to interpret them cleanly.

## 6. Zero-Dominant As Attachment Potential

Anything with a dominant side of `0` should be read as unresolved attachment
potential rather than invalid math.

This gives a useful ladder:

- `0/0` = open attachment site
- `m/0` = site carrying unresolved energy
- `m/u` = resolved reading through a basis

This same mechanism should remain meaningful at higher grades.

## 7. Structural Hardening And Relaxation

Open sites may repeatedly accept data without yet preferring any stable
structure.

Over time:

1. repeated tension gathers at a site
2. a relation begins to prefer some values
3. the dominant side moves away from `0`
4. a carrier-local structure hardens
5. later the relation may relax back toward `0/0`

So structure is not a separate ontology layered on top of attachment. It is
attachment potential that has stabilized enough to be frozen and reused.

## 8. Current Engine Direction

The current coding direction should therefore be:

- `Site` = location plus relation state
- `Relation` = ordered coupling between sites
- `Basis` = frozen relation or frozen fusion of relations
- `Attachment` = live sites read through a basis

This is the conceptual target for the next axiom rewrite, where site, relation,
and freeze should be interleaved directly into the main text rather than
remaining an external supplement.

## 9. Aligned And Orthogonal Are Not Symmetric

For any one carrier, the aligned structure is intrinsically binary:

- recessive
- dominant

These are the two known directed interpretations of that carrier.

If more values gather on one aligned side, they should not create new aligned
slots. Instead they deepen or repeat the same carrier. This is the beginning of:

- self-interaction
- accumulation
- power progression
- non-linearity

Orthogonal structure is different.

Orthogonal means "not yet explained by the current carrier's aligned pair."
So orthogonal should not be thought of as one more fixed attribute with only two
positions. It is better understood as unresolved remainder that may later split
into several newly resolved carriers.

So the asymmetry is:

- aligned multiplicity deepens
- orthogonal multiplicity differentiates

This is important because it suggests a carrier has a fixed aligned polarity but
an expandable orthogonal frontier.

## 10. The Orthogonal Frontier

At the local site level, any carrier may leave unresolved orthogonal remainder.

That orthogonal remainder is still meaningful. It is not noise. It is tension
that has value but not yet a settled denominator in the current carrier's
language.

Other carriers may later attach to this remainder and account for part of it.
More than one carrier may do so.

Therefore unresolved orthogonal tension must be able to fission into several
child supports without changing ontology. It should remain the same kind of
thing before and after articulation:

- before articulation: unresolved orthogonal tension
- during articulation: partially distributed orthogonal tension
- after articulation: several child carrier supports plus any residual remainder

So the current four-slot engine picture should be treated as a provisional
grade-1 window:

- aligned recessive
- aligned dominant
- orthogonal recessive
- orthogonal dominant

This is useful for the present engine pass, but it is not the final full
topology of a site. Long-term the aligned pair remains fixed, while the
orthogonal side must be numerically extensible.

## 10A. Local Interpretation: Orthogonal Evidence Into Aligned Meaning

One useful way to read a site is:

- **orthogonal input**: incoming evidence or energy not yet characterized in the
  local carrier's language
- **aligned recessive**: the part locally retained, absorbed, or sunk into the
  current structure
- **aligned dominant**: the part locally asserted, published, or transmitted
  onward on that carrier
- **orthogonal output**: what remains real and influential but is still not
  explained by the current carrier

This should be treated as a strong local interpretation, not as the only legal
meaning of recessive, dominant, and orthogonal.

The point is not that orthogonal content is globally random or meaningless.

The point is that it is not yet explained **here**, by **this** local carrier.

So a site can often be read as a local conversion point:

- orthogonal evidence arrives
- some of it becomes aligned carrier meaning
- some of it remains unresolved and continues onward as orthogonal remainder

This is one of the main ways Core4 can express the transition from:

- locally unordered or uncharacterized evidence

to:

- locally ordered or calibrated structure

### Example 1. Body As Heat Interpreter

Suppose energy arrives at a body and the body is being read through a heat
carrier.

Then one useful local reading is:

- orthogonal input = incoming energy not yet characterized as heat
- aligned recessive = heat absorbed into the body
- aligned dominant = heat transmitted from the body into the surrounding space
- orthogonal output = whatever remains real but still not explained by the heat
  carrier, such as reflected light or some other exported non-heat effect

In this reading:

- if one wants to know about the body heating up, the recessive aligned value is
  the important local read
- if one wants to know about the body heating the surrounding space, the
  dominant aligned value is the important local read

This is a useful interpretation because it lets one local carrier separate:

- retained ordered value
- transmitted ordered value
- leftover unexplained remainder

without introducing a second ontology.

### Example 2. Light Bulb With Coupled Light And Heat Sites

Suppose electricity enters a bulb and the bulb is modeled with at least two
local carrier readings:

- a light site
- a heat site

Then one useful simple reading is:

- electrical input arrives as orthogonal evidence to the light-generating site
- the light site settles some of that into aligned light behavior
- some light is retained or internally reflected
- some light is transmitted outward into the room
- some locally unexplained thermal remainder is exported orthogonally toward the
  heat site

Then the heat site receives:

- orthogonal input coming from the light site

and may settle that into:

- aligned recessive heat warming the bulb itself
- aligned dominant heat transmitted into the surrounding air

In a simple model, there may be no leftover orthogonal tension after that local
heat settlement. In a richer model, some unresolved remainder may persist and
continue onward.

### Why This Matters

This is one good way to understand how conversion happens locally in Core4:

- unknown or uncharacterized evidence arrives orthogonally
- local carrier structure interprets what it can
- the interpreted portion becomes aligned meaning
- any unaccounted remainder stays as tension or orthogonal export

The same general mechanism appears at several levels of abstraction:

- physical transduction
- local estimation
- carrier-to-carrier conversion
- basis encounter during traversal

So this should not be treated as an isolated sensor story. It is a recurring
Core4 pattern:

- unresolved input
- local interpretation
- aligned settlement
- residual remainder

And importantly, unresolved tension inside the mechanism is not failure.

It is expected and useful, because it preserves what has not yet been locally
explained in a form that other carriers or later structure may still account
for.

## 11. Data And Carrier Formation Use One Mechanism

Data and carrier formation should not be treated as two unrelated mechanisms.

If several values attach to one unresolved slot, each may appear as a local
`m/0` relation. For example:

- a slot holds `10/0`
- five attached values each contribute `2/0`

This is still one unresolved field, not yet a stable carrier.

If the shared denominator hardens away from `0`, the same population becomes a
committed repeated carrier relation instead of loose data. Then the structure is
read as:

- self-interaction
- accumulation
- power
- repeated unit action

depending on the surrounding basis and observer intention.

So the difference between "data" and "carrier" is not ontology but commitment.

## 12. Flow, Export, And Import

What one site reads as dominant or recessive often also appears as outbound or
inbound explanation.

One useful local reading is:

- outbound = this basis explains what it can, but still exports unresolved
  tension or resolved contribution outward
- inbound = this basis is changing in a way it cannot fully explain, so it
  imports explanatory structure

These should not become two separate kinds of relation. They are two local
interpretations of one flow.

So:

- one connection may carry positive flow in one local reading
- the other endpoint reads the same connection as incoming explanation
- if both directions are active, the better model is oscillation or
  bidirectional exchange rather than a new ontology

This suggests dominant and recessive are not merely geometric labels. They are
also local directional readings of explanatory flow.

## 12A. Polarity And Provenance Are Distinct

It is useful to distinguish:

- **polarity**: recessive vs dominant
- **provenance**: self vs other

Polarity describes the structural role a quantity plays inside a carrier-local
reading.

Provenance describes where the pressure or information is coming from.

These often align, but they should not be collapsed into one axis.

For example:

- aligned dominant often carries the current local claim of value
- orthogonal recessive often carries evidence from other structures

But there are important crossings:

- a remembered landmark may be self-sourced while still functioning on a
  recessive side
- an externally requested setpoint may act directly on the dominant value

So a better reading is:

- polarity says what role the slot plays
- provenance says who is sourcing influence on that role

This gives a useful binary overlay:

- recessive/self
- recessive/other
- dominant/self
- dominant/other

This should remain extensible later into richer provenance gradients such as:

- trusted vs weakly trusted
- near vs far
- fresh vs stale
- strong source vs weak source

But self/other is a good first structural split.

## 13. Decay And Relaxation

Disconnection should not usually mean the immediate disappearance of a relation.

The more Core4-native reading is:

- strong denominator = strong explanatory commitment
- weak denominator = weak explanatory commitment
- denominator approaching `0` = relation relaxing toward unresolved tension
- denominator at `0` = disconnected or fully unresolved relation

So decay is best understood as denominator relaxation.

This also clarifies why frozen relations resist decay:

- a live relation may relax quickly if no meaningful flow remains
- a frozen relation resists decay because commitment is part of what was frozen
- a frozen relation should generally need an explicit relaxation process before
  it loses denominator and disconnects

This keeps commitment physically meaningful.

## 13A. Recalculation, Relaxation, And Oscillation

Live values should generally be re-derived rather than treated as permanently
stored truth.

What can remain stored is:

- frozen basis structure
- committed denominators
- retained prior state used as local memory or anchor

But current value should be read as a repeated settlement process.

One useful local cycle is:

1. gather recessive anchors and retained commitments
2. gather orthogonal incoming evidence
3. form a provisional aligned value
4. publish that value outward
5. receive confirmation, disagreement, or feedback
6. redistribute unresolved remainder through connected relations
7. repeat until the local structure settles, oscillates, or relaxes

In simple acyclic structures, this may approximate a directional sweep.

In richer structures with loops or mutual dependency, it is better understood as
an iterative relaxation or oscillator process rather than as a single
two-direction pipeline.

This is one reason denominator strength matters so much:

- strong denominator = stiffness, inertia, resistance to perturbation
- weak denominator = easy adjustment
- zero denominator = no settled local commitment, so tension transfers onward

So `0` denominator does not simply fail to explain. It keeps information in play
as transferable tension.

## 14. Higher Grades: Stable Rule, Open Questions

A promising rule is that even higher-grade structures still expose:

- a recessive side
- a dominant side
- unresolved orthogonal remainder

But the character of those numbers changes with grade.

For example:

- a 1D opposed pair yields a directed segment
- a 2D aligned pair yields area
- a 2D opposed pair yields a ray-like complex structure

These are not all "the same quantity," even when they still expose recessive
and dominant orientation.

So a higher-grade basis may still obey the same relation law while changing what
its value means physically.

An open question remains:

- when a basis changes sign or inversion pattern, is it still the same basis in
  a new state, or has it become a different basis-kind?

Another likely rule is that orthogonality is relative to the frozen
interpretation currently chosen. For example:

- a `++` 2D basis may be frozen as area
- a `+-` 2D basis may be frozen as a ray-like complex

Both are legal, but they should likely be treated as different frozen readings
of similar ingredients rather than one basis casually changing what it means.

Current guidance:

- a frozen basis should not silently mutate its defining interpretation
- if inversion changes what the structure fundamentally measures, that should
  likely be expressed as a different frozen basis, not a casual runtime flip

This is not fully settled and should be revisited when the axioms are rewritten.

## 15. Current Design Consequence

The present engine should therefore move toward:

- a site with fixed aligned polarity for each known carrier
- an orthogonal residual reservoir that can later fission into child supports
- live relations carrying flow between sites
- frozen bases preserving committed explanatory structure
- attachment and data as low-commitment forms of the same relation law

The current finite site-field implementation is a useful first approximation,
but the long-term Core4 target is:

- fixed aligned depth
- extensible orthogonal articulation
- numeric hardening and relaxation of denominators
- no separate ontology for data, carrier, branch, or flow

## 16. Sensor, Estimator, And Generator

It is useful to reserve the word **sensor** for a physical transducer role, but
that does not place it outside Core4. A sensor is still a full carrier-local
Core4 object. It is simply a simpler one whose flow is mostly feed-forward.

A physical light sensor, for example, has its own internal mechanism. Light rays
arrive as external influence, affect that mechanism, and through some local
transformation produce a stabilized carrier value. That device may then export a
signal that other structures use.

So in Core4 terms:

- a **sensor** is a physical transduction arrangement
- an **estimator** is a carrier-local structure negotiating evidence into a
  value
- a **generator** is a carrier-local structure pushing value outward into the
  world

These are not three unrelated ontologies. They are different flow balances of
the same relation law, ranging from low-negotiation transduction to richer
multi-input calibration and outward drive.

One useful reading is:

- orthogonal recessive = incoming evidence, disturbance, or explanatory input
- aligned recessive/dominant = the carrier's own negotiated value on its known
  polarity
- orthogonal dominant = exported signal, published expectation, or outward
  influence available to other structures

Under this reading:

- a light **sensor** is a mostly one-way transducer supplying evidence
- a light **estimator** gathers many orthogonal inputs, such as sensor data,
  switch state, time of day, prior expectation, and feedback from other systems
- a light **generator** mainly sets or drives the aligned value and exports its
  influence outward, while still potentially accepting feedback

For a simple sensor, listeners may disagree with the exported value, but that
usually changes their commitment rather than the sensor's own value. In the
simplest case they disconnect. A more adaptive or self-healing sensor is still
the same kind of Core4 object, but with stronger feedback participation and more
estimator-like behavior.

So a night light with an ambient detector, a manual switch, and a burn-out
condition can still be modeled as one carrier-local construct. It is
simultaneously:

- estimator, because it negotiates multiple inputs
- generator, because it drives a light value outward
- sensor-coupled, because it uses transduced evidence from other mechanisms

This supports a very physical reading of flow:

- orthogonal input is what the carrier must account for
- orthogonal output is what other structures can read, react to, and return
  feedback against

Disagreement is then natural rather than exceptional. If the switch says
"light," the ambient detector says "dark," and collision feedback says
"navigation failed," the estimator does not crash. It negotiates among those
tensions.

## 17. Resolution, Centering, And Rounding

Resolution should ideally fall out of basis structure rather than from a
separate rounding rule.

If a carrier has only one directed unit, the lowest tick of resolution behaves
like a one-sided detection threshold. Then:

- values smaller than one tick may collapse toward `0`
- `0.6` of a tick and `0.999` of a tick can both still read as `0`

This is physically meaningful when the system is only detecting outward
departure from an origin.

But sometimes the desired interpretation is nearest-value rather than
origin-biased thresholding.

This also clarifies the apparent "double zero" problem.

If one imagines ordinary signed rounding while still using only one directed
unit, zero seems to become twice as wide as every other bucket:

- positive sub-tick values collapse to `0`
- negative sub-tick values also collapse to `0`

Then the interval around zero would be artificially doubled, which is not a
physically satisfying calibration.

The better Core4 reading is that this doubling only appears if one assumes the
inverse side is visible when it is not.

With a dominant-only unit, the basis does not merely prefer the dominant
perspective. It only exposes that side of the origin at all. In that case:

- the dominant side is visible
- the recessive side is not yet accessible
- negative readings do not meaningfully exist for that basis

So a one-sided unit should not be treated as a full signed number line with a
bad rounding rule. It is a half-line.

Core4 can explain this structurally by adding the inverse unit.

Even if the inverse side currently carries zero value, the inverse unit still
changes calibration. It pulls resolution back toward centered intervals rather
than leaving all uncertainty on one outward side.

So:

- one-sided unit -> threshold-like quantization
- paired direct/inverse unit -> centered quantization

In that centered case, tick intervals naturally lock around the whole values,
which gives nearest-integer style behavior without needing a separate ad hoc
rule.

This suggests the inverse unit does two things at once:

- it introduces the opposite perspective
- it opens the opposite side of the origin for readable placement

So the inverse unit is not merely a second opinion about already-visible values.
It also reveals values that were structurally inaccessible before.

So the difference between truncation and nearest rounding is not merely a later
numeric choice. It can be read as a difference in available basis structure:

- no inverse partner means purely outward resolution
- inverse partner present, even at zero value, means centered resolution

This should be treated as a promising structural explanation, though the exact
quantization law still needs to be worked through when the axioms are rewritten.

## 18. Locked Invariants And Revealed Properties

Freezing should not be limited to origins, units, and directly encountered
relations.

Once a fusion has revealed a new stable structure, that revealed structure may
itself become the thing an observer chooses to hold fixed.

This gives a second important use of freeze:

- first freeze stabilizes ingredients into a reusable structure
- later freeze stabilizes some property revealed by that structure

This is important because many operational behaviors are not fully expressed by
the forward construction alone.

For example:

- if a directed span is frozen and one endpoint moves, the other must respond
  to preserve total extent
- if a product-like self-interaction is frozen and one factor moves, the other
  responds non-linearly
- if area is frozen and one axis moves, the other responds reciprocally
- if a complex-like ray length is frozen and one axis moves, the endpoint traces
  a circular family

So a useful Core4 reading is:

- a fusion does not only create a new quantity
- it also creates new properties that can later be frozen
- freezing those properties reveals lawful compatible motions in the ingredients

Those compatible motions should be treated as part of the structure, not as a
separate symbolic inverse procedure.

In this sense, operation is not only:

- expand ingredients into a result

It is also:

- freeze some structurally revealed invariant
- perturb one participant
- observe the compatible response of the rest

This suggests a useful distinction:

- **directly frozen structure**: the relation, basis, or fusion that was
  explicitly committed
- **revealed property**: a stable magnitude or geometric feature that only
  becomes available after that structure exists

Examples of revealed properties include:

- total directed span
- product-like magnitude
- bounded area
- ray length
- orientation or turning family

These should not require a second ontology.

They are still Core4-native because:

- the invariant is numeric
- the perturbation is numeric
- the resulting path is numeric
- intermediate states remain inspectable as live attachment or tension states

So "solving" should often be re-read as constrained structural motion under a
frozen invariant.

This is likely one of the main bridges from static fused structure into
operation, traversal, and later tension-lowering processes.

## 19. Orbit, Rotation, And True Periodicity

Circular-looking behavior should not all be collapsed into one claim.

There is an important distinction between:

- an **orbit**: a family of equal-magnitude positions around an origin
- a **rotation**: ordered traversal through that family
- a **loop**: true re-entry of the same path or state
- **periodicity**: repeated re-entry under a stable turning law

This matters because a projected path may look circular without actually
closing.

For example:

- a spiral may look like repeated looping from one perspective
- but if it is still advancing on another carrier, it is not truly rejoining
  the same position in full structure

So visual overlap is weaker than structural re-entry.

One good Core4 reading is:

- fix an origin
- freeze a ray-like magnitude
- allow ordered change in the position of the endpoint

Then a circular orbit appears as the locus of positions with equal frozen
magnitude, while rotation is the ordered traversal of those endpoint
positions.

This matters because it lets turning emerge from constrained geometric motion
rather than assuming orientation in advance.

Under this reading:

- the orbit is the geometric family
- rotation is motion through that family
- loop closure only appears when the path truly rejoins itself

So a drawn circle in a plane is weaker than a genuinely closed support.

A stronger periodic claim means the system returns not only to a projected
location, but to an equivalent structural state under the chosen reading.

This suggests that modulo full turn should be treated as a later commitment,
not as an automatic consequence of every circular-looking motion.

It is also useful to note that circular or oscillatory behavior may emerge from
carrier response itself.

If an origin or frozen landmark affects traversal along a carrier, then motion
need not remain uniform.

For example:

- motion toward an origin may accelerate
- motion away from it may decelerate
- the support being traversed can therefore perturb the traversal itself

This is important because it means action is not only one structure affecting
another. The traversed structure may also react back on the traversal.

That makes oscillation, orbital bias, and periodic return feel like natural
extensions of the same relation law rather than like a separate mechanics
system.

So the promising long-term Core4 reading is:

- a traveler perturbs structure
- structure perturbs the traveler
- some frozen invariant may constrain the family of allowed positions
- if those constraints and responses truly rejoin, the result is loop or
  periodicity
- if they only repeat in projection, the result is orbit-like appearance
  without full closure

## 20. Traversal, Response, And Dual Perspective

Traversal should probably not remain a separate algorithmic ontology.

It looks increasingly like it should fall out of the same Core4 structures
already used for:

- measurement
- attachment
- projection
- operation
- settlement

One useful way to frame this is:

- a structure may be read as something being **acted on**
- the same structure may be read as something being **inspected from within**

These are not unrelated readings.

They often behave like local duals.

For example:

- acting on a support may move or scale what is attached to it
- inspecting from the traveler perspective makes the support appear to move
  relative to the traveler
- a claim and a measurement are similarly dual under changed perspective

So a traversal law should likely be expressed in the same Core4-native terms
as structure itself.

The earlier short list of nouns is useful, but it is probably not the best
final division of meaning.

A better first grouping is:

- **magnitude or state**:
  - primitive measure
  - resolved or unresolved ratio
- **support**:
  - carrier
  - live relation
  - frozen basis or unit
- **placement**:
  - where something is on a support
  - how interpolation proceeds there
- **perspective**:
  - polarity
  - provenance
  - observer stance
- **commitment**:
  - open
  - live
  - frozen
  - graded or compounded
- **mismatch**:
  - tension
  - zero-unit style unresolved explanatory need
- **composition**:
  - fusion
  - branching
  - joining
  - repetition
  - self-interaction
- **boundary and re-entry**:
  - admissible range
  - closure or non-closure
  - exact or inexact return

These are probably closer to the main divisions of meaning in Core4 than a
flat list of specific object names.

This suggests a common decomposition:

- **advance**: interpolate on a support
- **transfer**: allow site, boundary, or attached structure to alter the
  carried state
- **re-entry**: compare the new state against prior state and decide whether
  the result is:
  - novel continuation
  - reflection
  - oscillation
  - wrap or self-feed
  - merge
  - exact periodic return

Under this reading:

- sequence is just continued advance without meaningful re-entry
- repeated action is repeated transfer under the same law
- reflection is a response that inverts perspective
- oscillation is repeated reflection or bidirectional exchange under a stable
  local constraint
- looping is true re-entry into equivalent structure
- periodicity is stable repeated re-entry

This is still incomplete and should not be frozen too early.

But the likely long-term rule is that traversal should be driven by numeric
response encoded in structure, not by a large menu of special traversal
directives.

So later pathfinding should likely depend on:

- local continuity
- boundary response
- provenance alignment
- available denominator or resolution
- tension lowering
- explicit frozen constraints

rather than on hard-coded branch behavior alone.
