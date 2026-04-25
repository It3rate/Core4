# Core4 Basis Encounter Note

This note captures a clarification that emerged while trying to express:

- loops
- reflection
- clamps
- closure
- traversal perspective
- local basis changes along one carrier

It does not replace the main axioms.

It sharpens how a frozen basis behaves when it is not merely being measured,
but is being encountered by something already traversing structure.

The main conclusion is:

- a basis is not only a static measuring statement
- the same frozen basis can also act as a transfer law when encountered during
  traversal

This should remain one Core4-native mechanism rather than splitting into:

- one ontology for units
- another ontology for seams
- a third ontology for traversal directives

## 1. Basis As Frozen Calibration

The current engine already treats a basis as more than a label.

A basis is a frozen calibration law.

It carries at least:

- an orientation
- a resolution
- a denominator or reusable extent
- a pinned reference

So a basis does not merely say "this amount exists."

It says something closer to:

- this is how change is interpreted here
- this is what counts as positive and negative under this reading
- this is the minimum or committed granularity worth distinguishing
- this is the reusable denominator for comparison and interpolation

This is why a basis already behaves as both:

- a statement when frozen
- a generative law when acted through

That dual role is already familiar elsewhere in Core4:

- claim vs measure
- generation vs inspection
- structure being acted on vs structure being inspected from within

So basis behavior should not become exceptional once traversal is involved.

## 2. Trolley Carries Two Things

A trolley should not be thought of as carrying only "a direction."

It carries at least two distinct ingredients:

- a carried basis or calibration law
- its own current traversal state

The carried basis gives the trolley:

- what counts as positive
- what counts as negative
- what resolution it currently understands
- what kinds of change it can express cleanly

The trolley state gives:

- intended direction of travel
- current step size
- current carried value or tension
- provenance or local intent
- any local modifier currently being applied to the carried basis

These are related, but they are not the same object.

For example:

- a trolley may carry a positive-oriented basis
- yet choose to travel in a negative direction on that support

So the trolley's own signed motion is not identical to the sign of the basis
it currently carries.

One useful way to say this is:

- the trolley carries a basis of understanding
- plus its own traversal state

The basis supplies calibration.

The traversal state supplies intent and any local modification being applied to
that calibration.

## 3. Basis Encounter

When a trolley reaches a pinned basis, that basis is no longer only a frozen
statement.

It becomes an encountered local calibration law.

At that point, the trolley may:

- retain its current basis
- replace its current basis with the encountered basis
- compose its current traversal state against the encountered basis
- fail to reconcile them and generate tension

So the key event is not "the trolley hit a seam object."

The key event is:

- the trolley encountered a pinned basis while already carrying another basis

Traversal then becomes a negotiation between:

- carried calibration
- trolley-local traversal state
- encountered local calibration
- present topology and commitment

This gives a much more Core4-native reading than introducing a second symbolic
class of boundary laws.

The important refinement is that composition should not be read as mutating the
carried basis itself.

The better reading is:

- the trolley may retain the basis it is carrying
- the trolley may replace that carried basis with the encountered basis
- or the trolley may keep the carried basis while composing its own traversal
  state against the encountered basis

So the mutable part is the traversal state, not necessarily the carried basis
reference.

## 4. Seam As A Mode Of Encounter

One tempting idea was that a seam should be a brand-new primitive object.

The current clarification is weaker and better:

- a seam is probably not a new numeric species
- a seam is a way a pinned basis is being used

More precisely:

- not every pinned basis encounter is a seam in the interesting sense
- a seam is a pinned basis encounter where continuation is nontrivial

Examples:

- identity continuation
- perspective inversion
- closure onto another endpoint
- clamping into tension
- partial transfer

So "seam" should likely be treated as a role or reading, not a separate
ontology.

This matches other Core4 ideas:

- orthogonal vs aligned is a contextual reading
- incoming vs outgoing is a contextual reading
- dominant vs recessive is a contextual reading

Likewise:

- seam vs ordinary basis encounter is contextual to how traversal is using the
  encountered basis

## 5. Why Seam Is Still Not Just A Bare Unit

It is tempting to conclude:

- if a seam is a pinned basis encounter
- and a basis is already a calibration law
- then seam is just unit and nothing more

That is almost correct, but not fully.

The missing piece is continuation.

A bare basis gives a local chart:

- orientation
- reference
- resolution
- denominator

But traversal also needs to know what happens next.

So a seam-like encounter is:

- a basis-like law
- pinned at a site or boundary
- together with the local continuation fact

That continuation fact may be trivial.

For ordinary continuation, it is effectively:

- preserve current perspective
- continue along available support

For reflective behavior, it may be:

- invert perspective
- or replace the carried basis with one whose positive direction is opposite

For clamp-like behavior, it may be:

- no admissible continuation
- residual motion becomes tension

So the missing thing is probably not a new seam object, but an explicit
continuation law attached to basis encounter.

This should not be read as if a separate list of symbolic laws is being
attached from outside.

The better reading is:

- the encountered basis already encodes possible interpretations at that place
- traversal asks what those interpretations permit or deny
- continuation falls out of that encounter

So a basis can already carry action information in the same broad sense that it
carries interpretive information.

It does not need a second symbolic payload bolted onto it in order to be
actionable when traversed.

### 5A. Partial Basis Cases

Some useful cases can already be described by the kinds of basis the trolley
encounters.

#### Dominant-only unit

A dominant-only grade-1 basis gives one-sided calibration.

It says, in effect:

- forward is intelligible here
- reverse is not symmetrically available under this calibration

If a trolley adopts or obeys this basis, then backward traversal does not
vanish from reality, but it may become unresolved or tension-bearing.

#### Opposed local unit at a boundary

If the only encountered local basis is opposed to the carried reading, then the
trolley may:

- replace its carried basis with that opposed basis
- or retain its carried basis and invert its traversal state

Either way, the result can be reflective behavior without requiring a special
reflection token.

#### Bilateral directed-segment basis

A directed-segment basis gives bilateral calibration.

It makes both forward and backward change meaningful within one bounded reading.

So a trolley that encounters such a basis may find natural continuation
available without losing bilateral capability.

#### Higher-grade basis

A higher-grade basis may expose new structured possibilities rather than merely
changing sign.

For example, it may make available:

- bounded reverse motion
- off-axis interpretation
- compound change
- orthogonal branching

So basis encounter can expand or shrink capability, not only redirect motion.

## 6. Identity, Invert, Clamp, Closure

The simplest useful continuation family looks like:

- preserve
- invert
- clamp
- close
- partially transfer

These should ideally remain numeric and basis-like rather than enum-driven.

### Preserve

The encountered basis agrees enough with the carried basis that traversal can
continue without qualitative reinterpretation.

This is the default aligned continuation case.

### Invert

The encountered basis is opposed in a way that reverses the interpretation of
positive progress.

This can be read in two equivalent ways:

- keep the trolley sign and replace the carried basis
- keep the carried basis and invert the trolley sign

Those are different operations, but they can describe the same functional
result.

### Clamp

There is no admissible onward calibration.

The trolley can still exist, but its attempted continuation is no longer
resolvable under the local basis.

So motion becomes:

- stalled
- opposed
- accumulated as tension

### Close

The encountered basis identifies the current boundary with another boundary.

The continuation may:

- preserve perspective
- invert perspective
- remap position

Closure is therefore not a separate miracle. It is a specific kind of basis
encounter with nontrivial boundary continuation.

### Partial Transfer

The local basis and the carried basis only partly agree.

Then some of the carried motion may pass and some may become residual tension.

This is likely important later for:

- weak attachment
- permeability
- dissipation
- imperfect transmission

These should ideally be thought of as interpretations made available by the
encountered basis and local topology, not as a menu of external branch rules.

## 7. Football Field Reading

The football field analogy helped expose an important distinction.

It feels like one object with:

- one shared resolution
- one bounded extent
- two inward-facing endpoint perspectives

That object is not best read as:

- a standard directed segment fused at one origin

It is better read as:

- one bounded support
- one shared calibration scale
- two endpoint-local pinned basis readings

That naturally suggests reflection-like traversal.

If a trolley reaches one end and the only available pinned basis is an inward
opposed one, then its natural continuations are:

- adopt that basis and keep moving positively under the new reading
- or retain its basis and invert its local travel sign

Either way, the effect is reflected motion.

So reflective interval behavior may emerge from endpoint-local basis encounter
rather than from a special reflection rule.

## 8. Directed Segment Vs Chained Positive Supports

One important confusion this resolves is:

- a chained run of positive supports on one carrier
- is not automatically one directed-segment basis

It may instead be:

- several grade-1 positive relations in sequence
- each locally calibrated
- later readable as one higher span if some query or freeze commits that view

So the topology may remain primitive and local while a later observer reads:

- one path
- one segment
- one chain
- one net
- one higher frozen extent

This supports two simultaneous truths:

- the primitive structure can remain a sequence of simple carrier-local objects
- higher fused interpretations can still be derived when useful

## 9. Grade Mismatch And Capability Loss

Basis encounter is not only about direction.

It is also about what kinds of motion remain intelligible.

For example:

- a trolley carrying a bilateral directed-segment basis can express forward and
  backward change within that understanding
- if it replaces that basis with a dominant-only grade-1 basis, it may lose the
  ability to interpret reverse motion symmetrically

That does not mean reverse motion disappears from reality.

It means reverse motion is no longer cleanly calibrated under the new adopted
basis and may therefore appear as tension.

So basis encounter can change capability, not only orientation.

This suggests a useful reading:

- units are not only rulers
- they are calibrations of what kinds of change are intelligible

This should remain true at higher grade:

- angle
- area
- compound change
- orthogonal branching

all depend on what the carried basis can actually express.

## 10. Same Carrier, Different Resolution

Resolution mismatch on the same carrier should not be treated as orthogonality.

The better reading is:

- same carrier
- unresolved calibration mismatch

This mismatch may appear as:

- tension
- weak attachment
- incomplete transfer
- later negotiated shared denominator

So differing resolution on one carrier is closer to:

- attachment potential
- denominator negotiation

than to:

- a new orthogonal dimension

This matters because many continuation questions are really calibration
questions, not dimensional questions.

The same point also suggests that composition should usually be read as acting
on the trolley state rather than mutating the basis reference itself.

One good base case is:

- trolley begins with identity local modifier
- encountered basis acts on that local modifier
- carried basis may be retained or replaced depending on the encounter

That keeps the distinction between:

- calibration being carried
- state currently being applied through that calibration

## 11. Site Encounter And Basis Encounter

A useful pushback was:

- when a trolley reaches a site, does it not already do something very much
  like seam encounter?

The answer appears to be yes.

That suggests the continuation law should probably not be attached to some
totally separate seam ontology.

It likely belongs in site-local basis encounter itself.

So the long-term structure is likely:

- `Site` gives a pinned place of relation and transfer
- `Basis` gives local calibration law
- traversal at a site inspects available basis-like laws and support continuity
- continuation falls out of that encounter

This also helps connect carrier perspective and structure perspective:

- the carrier offers ordered support
- a pinned basis offers local interpretation
- the trolley offers carried interpretation plus current intent

That is already close to the broader Core4 pattern:

- structure has one reading in itself
- another reading under traversal or generation

## 12. What We Understand Well Enough To State

The following seem strong enough to record now.

### 12A. Strong Conclusions

- A basis is broader than a number-line marker. It is a frozen calibration law.
- A trolley carries both a basis and its own traversal state.
- Encountering a pinned basis is already very close to what boundary or seam
  behavior needs.
- Not every pinned basis encounter is a seam in the interesting sense.
- Seam is best treated as a role of basis encounter where continuation becomes
  nontrivial.
- Composition should usually be read as acting on traversal state rather than
  mutating the carried basis reference.
- Resolution mismatch on one carrier is tension to negotiate, not orthogonality.
- Higher-grade basis encounter can change capability, not only orientation.

### 12B. Likely But Not Fully Settled

- Reflective intervals may be expressible by endpoint-local basis encounter
  without adding a new symbolic reflection object.
- True loop closure may be expressible by boundary-local continuation that
  preserves perspective under endpoint identification.
- Clamp behavior may be expressible as basis encounter with no admissible onward
  calibration.
- Partial transfer may be a natural numeric weakening of continuation rather
  than a special case.

## 13. Immediate Design Consequence

The next traversal or loop pass should probably not begin by inventing a large
`Seam` type.

It should first test whether the existing primitive family can be extended by:

- making basis encounter explicit
- making continuation law explicit
- keeping both numeric and local
- letting encountered basis act on traversal state without requiring a new seam
  ontology

In other words, the next primitive may be closer to:

- pinned basis
- encountered at a site
- with a nontrivial continuation agreement

than to:

- a wholly separate ontology for loop and boundary objects

This is promising because it keeps loops, clamps, reflection, and closure close
to the same mechanism already used for:

- calibration
- attachment
- transfer
- traversal

## 14. Relationship To Later Frames And Data

This clarification also matters beyond traversal.

If a basis is a local calibration law that can be:

- stored
- pinned
- encountered
- adopted
- composed
- resisted

then it is already close to the kind of object that can help describe:

- carrier-local interpreters
- local frames
- negotiated shared resolution
- data being held inside a calibrated context

So this note may also be one bridge toward the earlier discussion that a
carrier can:

- hold a reference to an interpreter
- still have its own present intention or value

That should later help with:

- frame-like local structure
- stored but live data
- multi-basis interpretation on one support

without introducing a second symbolic layer.
