# Core4 Support Structure Plan

This note captures a bottom-level refactor direction for making Core4
support-first rather than ratio-first.

It does not replace the axioms. It translates the current discussion into a
concrete structural shape that code can move toward gradually.

## 1. Canonical Stance

Core4 should treat support as the one bottom-level structural family.

- a carrier is the atomic support
- a one-point frozen carrier is still support
- a two-point frozen carrier is still support
- a many-point frozen carrier is still support
- a fused multi-carrier object is still support

Different capabilities appear as more structure is frozen, but this should not
create a new primitive ontology every time.

So the intended family is:

- carrier
- frozen carrier structure
- fused support structure
- reused calibration
- live attachment into that structure

These are role distinctions inside one structural family, not unrelated
species.

## 2. Domain, Carrier, And Instance

The current code only has `Carrier(long Id)`.

That is enough to model one ordered line, but it does not distinguish:

- the semantic family the line belongs to
- the concrete local instance of that line

Core4 should eventually distinguish these two ideas.

### Domain

A domain is the semantic family:

- temperature
- distance
- light
- pressure

Domain is useful metadata and later may help with interpretation, unit
selection, or relevance of calculations.

But domain should not be the primitive math object.

### Carrier

A carrier is one concrete ordered line instance inside some domain.

Examples:

- one Celsius line
- one Fahrenheit line
- one private thermometer's uncoordinated scale
- one user's local distance line

Several carriers may belong to the same domain without already agreeing on:

- origin
- scale
- orientation witness
- calibration

So:

- same domain does not imply same carrier
- same carrier does not imply same support structure

The likely code shape is:

```csharp
public readonly record struct CarrierDomain(long Id);

public readonly record struct Carrier(long Id, CarrierDomain Domain);
```

This should be read as:

- `Domain` says what kind of thing this line measures
- `Id` says which concrete line instance this is

For backward compatibility, the current single `long` id can remain the primary
identity at first, while `Domain` is introduced with a neutral default.

## 3. Primitive Frozen Ingredients

The bottom-level frozen ingredients should be simpler than the current
`Relation` / `Basis` split.

The likely primitive ingredients are:

- carrier
- landmark
- coincidence
- support structure

### Landmark

A landmark is one frozen location on one carrier.

That covers:

- an origin
- a second point used as a unit-like witness
- a third or fourth point on the same line

Nothing ontologically new happens when a second or third point appears.
Capability changes, but the primitive ingredient is still:

- carrier plus frozen landmark

The likely code shape is:

```csharp
public readonly record struct LandmarkId(long Id);

public readonly record struct SupportLandmark(
    LandmarkId Id,
    Carrier Carrier,
    Measure Location,
    Ratio Commitment);
```

`Commitment` here means:

- how frozen this landmark currently is
- whether it is only weakly held
- whether it is fully reusable

### Coincidence

A coincidence is a frozen agreement that two landmarks should be treated as one
effective place for interaction.

This is the primitive behind:

- shared origin
- fused origins between supports
- cross-domain attachment points

It should not require the two stored locations to literally become the same raw
measure. They only need to agree structurally.

The likely code shape is:

```csharp
public readonly record struct LandmarkCoincidence(
    LandmarkId Left,
    LandmarkId Right,
    Ratio Commitment);
```

## 4. Support Structure

One general support structure should replace most of the current low-level
specialization.

The likely shape is:

```csharp
public sealed class SupportStructure : ISupport
{
    public ImmutableArray<SupportLandmark> Landmarks { get; }

    public ImmutableArray<LandmarkCoincidence> Coincidences { get; }

    public ImmutableArray<SupportStructure> Inputs { get; }
}
```

Interpretation then falls out from the structure already present:

- one carrier, one landmark: origin-capable support
- one carrier, two landmarks: two-point support
- one carrier, many landmarks: richer same-carrier support
- multiple inputs with coincidences: fused support

This means the current named objects become classifiers or aliases:

- `Relation` = a two-landmark same-carrier support view
- `Basis` = a reusable frozen support view
- `DirectedSegment` = an opposed same-carrier pair view
- `AreaBasis` = an aligned orthogonal pair view
- `ComplexBasis` = an opposed orthogonal pair view

The underlying structure stays the same.

## 5. Relation And Basis As Aliases

The current system treats these as separate concrete types:

- `Relation`
- `Basis`

That is useful ergonomically, but too strong ontologically.

The intended longer-term reading is:

- `Relation` is a low-level alias for a same-carrier support with two landmarks
- `Basis` is a role alias for support being reused as calibration

That suggests the following migration path:

### Phase 1

Keep `Relation` and `Basis` as real public types.

Internally, let them construct through one shared support structure.

### Phase 2

Move structural logic into the shared support structure:

- grade
- carrier participation
- shared-origin checks
- opposed/aligned checks
- fused input traversal

### Phase 3

Leave `Relation` and `Basis` as thin convenience wrappers or factories.

This preserves readability in user code without making them the ontology.

## 6. Ratio As Derived Fold

`Ratio` is currently carrying too much of the system.

It is being used as:

- primitive numeric storage
- placement coordinate
- projection result
- local slot state
- arithmetic engine

That is too much if support is the real primitive family.

The intended correction is:

- support is primary
- ratio-like readings are derived folds of support

The broad reading of ratio should be:

- a proportion or fold read generated from support
- usually from the highest usable shared origin
- usually from the first two relevant pinned witnesses

That means even weak supports may still produce a lawful fold:

- one landmark may fold to zero
- two landmarks may fold to span
- richer supports may fold to more complex proportional reads

So the future system likely wants a more general projection layer, for example:

```csharp
public interface ISupportProjection
{
    Measure Potential { get; }
}
```

and then:

- the current `Ratio` becomes one projection type
- support arithmetic asks supports for projections instead of assuming ratio is
  the primitive object

For the near term, `Ratio` can remain the compatibility projection.

## 7. Placement

`Placement` is currently:

- support plus ratio

That bakes the current projection choice into the primitive location model.

Longer-term, placement should become:

- support plus support-derived position read

The likely transitional shape is:

```csharp
public readonly record struct Placement(
    ISupport Support,
    Ratio Projection);
```

with the understanding that:

- `Projection` is temporary compatibility vocabulary
- later it should widen to a support-derived position object

This allows migration without breaking every caller immediately.

## 8. Site

`Site` should remain local.

It should not be the universal numeric object.

The preferred reading is:

- a site is where support meets support
- where support is pinned strongly enough to matter locally
- where local projection into aligned and orthogonal forms can happen

So this refactor should not try to eliminate `Site`.

Instead it should stop forcing every support object to look like a site-local
aligned kernel plus orthogonal reservoir.

Those remain:

- local projection forms
- useful for settlement
- useful for communication
- not the universal underlying ontology

## 9. Immediate Bottom-Level Code Changes

The smallest safe changes are:

1. Introduce a support-structure type behind the current public types.
2. Add domain metadata to `Carrier` without changing existing math behavior.
3. Re-express `Relation` construction through the shared support structure.
4. Re-express `Basis` construction through the shared support structure.
5. Leave `Ratio`, `Placement`, and `Site` behavior unchanged at first.

This gives a support-first bottom layer without forcing a same-week rewrite of:

- runtime settlement
- topology
- traversal
- carrier communication

## 10. First Refactor Targets

The first files likely to change are:

- `Carrier.cs`
- `ISupport.cs`
- `Relation.cs`
- `Basis.cs`
- `Placement.cs`

The likely staging rule is:

- move construction and structural classification first
- move projection and ratio assumptions second
- move local site settlement last

That keeps the refactor bottom-up rather than disturbing working runtime logic
too early.

## 11. Guiding Constraint

If a new grade or fused object seems to require:

- a new primitive class
- a new arithmetic species
- a new dedicated traversal ontology

then first check whether it can be expressed as:

- one more support structure
- one more support coincidence
- one more derived projection of that support

That should remain the default Core4-native direction.
