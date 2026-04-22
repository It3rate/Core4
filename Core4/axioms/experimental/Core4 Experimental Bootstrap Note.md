# Core4 Experimental Bootstrap Note

This note rewrites the bootstrap picture around the current minimal experiment.

It intentionally stays close to what is currently concrete in code:

- `Measure`
- binary `Relation`
- external `RelationFold`

The goal is not to fully explain every future grade. The goal is to state the
smallest physical picture that currently seems consistent.

## 1. Current Concrete Bootstrap Pass

The current bootstrap pass assumes only three concrete things:

- a `Measure` is one occupied site on an ordered substrate
- a `Relation` is one ordered pair of `IElement`s
- `RelationFold.Fold(left, right)` reduces two elements by recursively reading
  them down to leaf measures and then smearing them together

Nothing else should be treated as primitive yet.

In particular, this pass does not take as primitive:

- signature
- operation objects
- path APIs
- node/whole flags
- precomputed extent caches

Those may emerge later if they prove necessary, but they are not part of the
bootstrap floor.

## 2. Ordered Substrate

Before anything is related, the substrate is just ordered occupied sites.

For the current concrete pass, those sites are still represented by `long`
values through `Measure`.

The important correction is:

- a measure is not pinned to a globally fixed universe
- it is only locally stable enough to participate in relation

So the whole substrate may slide while the relation remains lawful internally.

That is why the current concrete pass can use a shared offset on `Measure`
without changing the folded relation result.

## 3. Relation

The primitive relation act is:

- choose `A`
- choose `B`
- relate `A -> B` in that order

`Relation` itself stores only:

- `Left`
- `Right`

It does not store:

- precomputed denominator
- precomputed magnitude
- precomputed grade label

All of that is deferred to fold.

This is important because the same stored relation may later be read as:

- basis
- value
- calibration
- addition-like append
- multiplication-like smear

depending on how fold encounters it.

## 4. Fold Is External

The current bootstrap pass treats fold as an external act:

- `RelationFold.Fold(left, right)`

This is deliberate.

The relation should stay as raw structure. The reduction logic should remain
outside it while the mechanics are still being discovered.

So fold currently acts more like:

- read left
- read right
- keep descending until leaves
- smear what was found

There is no `TryFold`, no invalid case, and no grade check that halts the
process. Every structure reduces to something meaningful, even if the meaning is
still provisional.

## 5. The Temporary Fold Projection

For the current bootstrap testing pass, fold projects to:

- `(numerator, denominator)`

This pair is not the final ontology.

It is only the current visible surface of the mechanical process.

The current working readings are:

- `(0, 0)` = open or coincident site with no partition matter
- `(m, 0)` = unpartitioned claim or point-like participation
- `(m, u)` = value resolved through a partitioned basis

This does not mean the system fundamentally "is" fractions. It means the
current fold experiment can be inspected through that shape.

## 6. Grade 0: Measure To Measure

At grade 0, both leaves are raw measures.

For example:

- `10 -> 20`

At first, only the endpoints are occupied matter. The interior is not yet
materialized as committed structure.

When fold runs, it walks every tick between the two endpoints. Since the partner
side contributes exactly one occupied site of matter at this grade, each walked
tick is filled without extra expansion or contraction.

So:

- `10 -> 20` folds to `(10, 10)`
- `20 -> 10` folds to `(-10, -10)`
- `10 -> 10` folds to `(0, 0)`

At this earliest grade:

- extent and partition count coincide
- denominator is just the walked extent
- a raw segment is the first partitioned basis

## 7. Unpartitioned Participation

An unpartitioned object is any folded value with denominator `0`.

This is the key point that now needs to be stated very strongly:

- unpartitioned participation is the only primitive additive participation

So:

- if an object folds to `(m, 0)`, it has magnitude but no interior partition
  matter available for direct interior-to-interior smearing
- it may still be resolved later through another basis
- but by itself it is not already a partitioned interior

This is the natural home of:

- point claims
- unresolved magnitudes
- additive append

## 8. Addition Requires Unpartitioned Participation

The stronger bootstrap claim is:

- addition is not the default fold
- addition requires one or both participations to be unpartitioned

That means:

- `(a, 0)` folded with `(b, 0)` appends to `(a + b, 0)`
- `(a, 0)` folded through a shared partitioned basis can be read as additive
  presentation on that basis
- but two already partitioned interiors do not directly add

This is why familiar arithmetic should be read carefully.

For example:

- `1/2 + 1/2`

should structurally read more like:

- `2/2`

than:

- `2/4`

because addition does not duplicate the denominator.

The denominator is the shared basis that the claims are being appended on.

So if partitioned values appear to add, the natural reading is not:

- partitioned interior plus partitioned interior directly

The natural reading is:

- one shared basis
- one or more unpartitioned claims being appended on that basis

or equivalently:

- one side has been presented additively rather than interior-to-interior

## 9. Two Partitioned Objects Fold Multiplicatively

This is the other strong claim:

- if both participations are already partitioned, direct fold is in the
  multiplicative family

That is not a flaw. It is the natural consequence of what is meeting what.

The moment partitioned interior meets partitioned interior:

- ticks meet ticks
- smearing begins
- basis is being substituted or scaled

So direct partitioned-to-partitioned fold should not be read as addition by
default.

It should be read as multiplication-like interaction.

This also means:

- two fully partitioned objects cannot be directly added at bootstrap grade

That sounds restrictive only if one assumes addition should be the default. In
this picture, it should not be.

## 10. Why Denominator Normalization Already Looks Multiplicative

This also clarifies why fraction normalization feels like multiplication before
addition.

If two partitioned operands do not already share a usable denominator, then any
step required to make them appendable must:

- scale one side
- or scale both
- or rebuild them on a shared basis

That is already multiplicative work.

So mismatched-denominator addition is not one primitive bootstrap act. It is:

1. multiplicative normalization or calibration work
2. then additive append through unpartitioned participation

This is why "just add two partitioned objects" is not the natural bootstrap
read.

## 11. The Mechanical Smear Rule

The current experimental fold uses an explicit smear loop rather than a closed
formula.

At the current concrete pass:

- leaf `Measure -> Measure` walks the raw ticks directly
- higher-grade fold first reads each side into `(numerator, denominator)`
- then smears those results by looping host ticks and partner ticks

This is important because it keeps the physical picture visible:

- host basis = what exposes the ticks being filled
- partner structure = what is inserted into each exposed tick

So multiplication-like fold is not merely "multiply numerators." It is:

- walk the host basis
- walk the partner interior
- accumulate what each host tick receives

## 12. Grade 1 Reading

Once a point-like claim is read through a unit-like basis, it becomes a resolved
value:

- point-like claim: `(8, 0)`
- shared unit: `(4, 4)`
- resolved value: `(8, 4)`

Then if another resolved value with the same basis participates:

- `(8, 4)` with `(12, 4)`

direct fold is multiplicative, not additive.

In the current concrete mechanical pass this reads as:

- every host partition receives the partner proportion
- the result becomes `(24, 4)`

which projects to `6`.

So the current bootstrap read is:

- partitioned with partitioned = multiplication-like smear
- unpartitioned with unpartitioned = addition-like append
- unpartitioned with partitioned = additive presentation or resolution through a
  shared basis

## 13. Reuse And Sliding Substrate

Because relations store only references and fold rereads the leaves every time:

- reused substructure stays live
- later substrate offset changes are seen automatically
- no extent is precomputed into the relation itself

This is useful because it keeps the physical picture honest:

- the structure remains what it is
- the fold result is always a current reading of that structure

## 14. What This Currently Clarifies

This rewrite sharpens several earlier ambiguities.

### Addition

Addition is now best read as:

- zero-partition append
- shared-basis accumulation
- no interior-to-interior smearing

### Multiplication

Multiplication is now best read as:

- direct partitioned-to-partitioned fold
- host/partner smearing
- interior meeting interior

### Normalization

Normalization is now best read as:

- multiplicative preparation
- not primitive addition itself

## 15. What Still Feels Open

Several things are still not settled.

### Unpartitioned With Partitioned

This currently reads well as:

- additive presentation
- or resolution through an existing basis

But the exact split between those two readings is still not fully formalized.

### Host Choice

The current mechanical pass still chooses a host basis by partition availability.

That seems reasonable, but it is still a choice that may need tightening.

### Remainders And Exactness

The current smear loop still uses integer pieces in the concrete experiment.

So if a numerator is not cleanly divisible by its partition count, more exact
remainder handling will be needed later.

### Inverse Orientation

The deeper treatment of recessive or inverted denominators is still open beyond
the current early intuition.

## 16. Current Working Summary

The current bootstrap picture is:

- a `Measure` is one occupied substrate site
- a `Relation` is one ordered pair of `IElement`s
- fold is external and recursive
- grade-0 measure-to-measure fold materializes a partitioned basis
- unpartitioned participation is the only primitive additive participation
- addition requires one or both sides to be unpartitioned
- direct partitioned-to-partitioned fold is multiplicative by default
- mismatched-denominator "addition" already contains multiplicative
  normalization work

This feels substantially more coherent than the earlier wording.

It is still not final, but it now states much more clearly that:

- addition is not the default
- multiplication is not an extra case
- partition matter is the decisive seam
