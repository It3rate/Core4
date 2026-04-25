# Core4 Experimental Bootstrap Higher Grade Fold Note

This note extends the bootstrap pass from raw relation and scalar-like fold into
closed higher-grade fold.

It does not replace the earlier bootstrap note. It adds the next structural
layer that emerged once the bootstrap experiment could:

- preserve open structure
- preserve closed composite structure
- recurse above grade 2 without adding grade-specific rules

The main question here is:

- what is the primitive result of folding one higher-grade object into another?

The current answer is:

- not immediate scalar projection
- not immediate collapse to a signed complex-style formula
- but preservation of the full contribution lattice first

## 1. Current Concrete Surface

The current bootstrap code now has the following concrete pieces:

- `Carrier`
- `Measure`
- binary `Relation`
- external `RelationFold`

`Measure` now carries a minimal numeric carrier identity.

This is intentionally small:

- `Carrier(Id, Domain)`

No symbolic aspect tags or enum channels are introduced yet.

Carrier currently exists so the structure can distinguish:

- same-carrier participation
- different-carrier participation

without changing the basic fold mechanism.

## 2. Higher Grade Is Recursive, Not A New Engine

The most important thing to keep from the bootstrap floor is:

- higher grade is still just structure made out of `IElement`s

There is no separate "grade 2 engine" or "grade 3 engine."

Instead:

1. `Relation` stays raw structure
2. `RelationFold` reduces two elements
3. if the elements are composite, the fold can preserve composite results
4. that preserved result can itself participate in later fold

So grade increase is structural recursion, not a case split.

## 2A. Condensing Into A Carrier Tick Is Not Collapse

The bootstrap pass also needs one more distinction that sits near higher-grade
fold but should not be collapsed into it.

That distinction is:

- condensing structure into a carrier tick

This is not the same act as:

- preserving a richer fold result
- collapsing that result to a lower-grade read

Collapse says:

- read richer structure through a lower-grade view

Carrier-tick condensation says:

- freeze some stable construction as one new opaque tick
- create a new carrier whose local floor is that tick
- then count future structures by that new tick

This matters because the resulting carrier may be simpler than the structure
that created it.

For example:

- `mph` may be treated as its own one-dimensional carrier
- `meters per second squared` may be treated as its own carrier
- `liters per 100 kilometers` may be treated as its own carrier

Those carriers do not need to re-express their full lower provenance every time
they are used.

They only need:

- a frozen condensed tick
- a stable way to project values into that tick count

But the larger system may still preserve outside that local carrier:

- unit shape
- carrier lineage
- historical construction

So carrier condensation should be read as:

- creating a new accessible floor

not as:

- deleting the structure that produced it

## 3. Closed Branch-Composite Fold

Once two closed operands each present two already resolved branches, fold does
not immediately collapse them into one scalarized value.

It first preserves the full four-contribution lattice:

- `AA`
- `BB`
- `AB`
- `BA`

where:

- `A` is the first branch of the left or right operand
- `B` is the second branch of the left or right operand

Each of those contributions is itself just an ordinary bootstrap fold.

So the primitive higher-grade fold result is not:

- `AA - BB`
- `AB + BA`

The primitive result is the full preserved lattice.

## 4. Channel Grouping

The current bootstrap read groups those contributions into two channels:

- aligned channel
- orthogonal channel

The preserved structure is:

- aligned channel = `Relation(AA, BB)`
- orthogonal channel = `Relation(AB, BA)`

and the full higher-grade result is:

- `Relation(aligned, orthogonal)`

So the current primitive output of a closed branch-composite fold is a
relation-of-relations:

- left side preserves aligned contributions
- right side preserves orthogonal contributions

This is intentionally richer than ordinary complex multiplication formulae.

## 5. Why This Is More Primitive Than The Complex Formula

In ordinary complex multiplication one usually projects to:

- real part = `AA - BB`
- imaginary part = `AB + BA`

That is a valid later read, but in this bootstrap picture it is already a
collapse.

The more primitive Core4-native statement is:

- `AA` is one aligned contribution
- `BB` is another aligned contribution
- `AB` is one orthogonal contribution
- `BA` is another orthogonal contribution

These should be preserved before they are netted against each other.

So:

- subtraction is not the primitive law
- subtraction is one later consequence of collapsing opposed-flow
  contributions into one lower-grade read

This matters because preserving all four contributions also preserves
information about:

- expansive vs contractive participation
- start-like vs end-like structure
- same-channel opposition before cancellation

## 6. Directed Segment And Complex-Like Values

The current working view is:

- directed segments and complex-like values are the same family

The difference is not that one needs a different fold mechanism.

The difference is the context in which the branches live:

- directed segment style = same-carrier opposed participation
- complex-like style = orthogonal-carrier participation

So the same higher-grade fold kernel can produce both.

What differs is:

- where the contributions are routed
- how later collapse is interpreted

This is why the current bootstrap experiment can already preserve:

- same-carrier branch lattices
- cross-carrier branch lattices

with the same structural mechanism.

## 7. Reduced, Collapsed, And Projected

The current higher-grade fold now exposes three readings:

- `Reduced`
- `Collapsed`
- `Projected`

These are not duplicates. They are different intentional reads of the same
folded structure.

### Reduced

`Reduced` is the richest preserved result currently available.

For a closed branch-composite fold, `Reduced` preserves:

- the full `AA/BB/AB/BA` contribution lattice

This is the best place to inspect higher-grade semantics without losing
construction lineage.

### Collapsed

`Collapsed` lowers the result by one structural grade.

It does not fully scalarize the object.

This is important:

- one collapse step should lower one structural grade
- it should not jump all the way to the bottom unless the object is already one
  collapse away from the bottom

So collapse is not a single destructive event. It is a ladder.

### Projected

`Projected` repeats collapse until the structure bottoms out into the current
lowest available folded read.

This is the current bootstrap answer to:

- "what is the final lower-grade read if I keep collapsing?"

So:

- `Reduced` preserves the richest available structure
- `Collapsed` lowers one step
- `Projected` lowers repeatedly until no further collapse is possible

## 8. Collapse As The Inverse Of Grade Growth

One useful way to think about this is:

- grade increase distributes structure outward
- collapse gathers it back inward

So if structure may be grown from grade 1 upward, then it should also be
possible to collapse from a higher grade back down through any lower grade
instead of only collapsing directly to the bottom.

That is why the current collapse semantics are intentionally repetitive.

The system should be able to:

- preserve grade 4
- collapse to grade 3
- collapse to grade 2
- collapse to grade 1

using the same lowering move repeatedly.

This currently feels much more physical than treating projection as one special
formula that destroys all intermediate grade information at once.

## 9. What The Current Tests Show

The current bootstrap tests now show several important things.

### Same-Carrier Closed Branch Fold

A directed-segment-like closed branch fold can preserve:

- aligned contributions
- orthogonal contributions

without forcing immediate cancellation.

### Cross-Carrier Closed Branch Fold

An orthogonal-carrier closed branch fold can preserve:

- same-channel contributions
- cross-channel contributions

with the same mechanism.

### Repeated Collapse

Those richer results can then be lowered intentionally:

- one grade at a time
- then all the way down if desired

So the code now reflects the distinction between:

- preserving the full four-quadrant structure
- intentionally reading out a lower-grade result

## 10. What This Currently Suggests About Meaning

The present interpretation is:

- `AA` and `BB` are not wrong-sign terms
- they are distinct aligned contributions
- `AB` and `BA` are not secondary artifacts
- they are distinct orthogonal contributions

Then later collapse may:

- net opposed-flow aligned contributions
- net orthogonal contributions
- or continue preserving them if a richer read is desired

So the complex-like signed formula is best read as:

- one later projection of a richer preserved fold

not as the primitive law itself.

## 11. What Still Feels Open

Several important questions are still open.

### Channel Meaning

The current code preserves aligned and orthogonal channel groupings, but the
full semantic meaning of those channels still needs tightening.

In particular:

- when should same-carrier opposed contributions read as directed segment
  start/end rather than as net length?
- when should orthogonal contributions read as complex-like angle/ray structure
  rather than as a lower-grade scalar?

### Carrier-Aware Collapse

Carrier now exists structurally, but current collapse still uses a simple
numeric gather rule.

That is good enough for the current bootstrap pass, but later collapse may need
to pay more explicit attention to:

- same carrier
- orthogonal carrier
- domain lineage

### Carrier-Tick Condensation

The current bootstrap code still does not have an explicit act for condensing a
stable structure into one new carrier tick.

That means the notes are now slightly ahead of the code here.

The missing act is not ordinary fold.

It is:

- choosing a stable construction pattern
- freezing it as the local tick of a new carrier
- then projecting later values into that new carrier without requiring the
  carrier itself to know the full lower provenance

This feels like a real next seam for the bootstrap experiment.

### Exact Final Projection

The current `Projected` read is the current repeated collapse result, not the
final permanent ontology of higher-grade numbers.

It is a useful and physically meaningful lowering rule, but it should still be
treated as provisional.

## 12. Current Working Summary

The current higher-grade bootstrap picture is:

- `Measure` now carries minimal numeric carrier identity
- higher grade is structural recursion, not a new engine
- closed branch-composite fold preserves the full `AA/BB/AB/BA` lattice
- those four contributions are grouped into aligned and orthogonal channels
- `Reduced` preserves the richest current structure
- `Collapsed` lowers one structural grade
- `Projected` lowers repeatedly until no more collapse is possible
- carrier-tick condensation is a separate later act, not the same as collapse
- directed-segment-like and complex-like values are the same family under
  different carrier contexts

This feels like a meaningful step beyond the earlier bootstrap floor.

The old bootstrap note still describes the floor well.

This note adds the next layer:

- preserve first
- collapse later
- lower one grade at a time
