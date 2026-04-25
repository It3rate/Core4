# Core4 Calibration Plan

This note captures the current direction for moving from raw repeated input
toward self-calibrating local basis and later toward calibration between sites.

It does not replace the main axioms. It turns the recent calibration discussion
into an implementation path.

## 1. Goal

Make the following statement true in core code:

- a site can receive repeated orthogonal input
- from that repeated input it can form a local origin and unit
- it can adjust those over time as residual tension persists
- it can do this without requiring large stored history or external symbolic
  intelligence

And after that first pass:

- two sites can begin to compare their local calibrations
- detect directional compatibility before full unit compatibility
- negotiate useful exchange without assuming they already agree on scale

The important design target is:

- keep this Core4-native
- keep it numeric
- keep it local
- avoid a second statistical ontology layered beside Core4

## 2. Canonical Interpretation

Calibration should be treated as a local tension-lowering process rather than
as a one-time external preprocessing step.

### 2A. Raw Input

Repeated orthogonal input arrives as evidence not yet explained in the local
carrier's language.

At first, the site may know almost nothing except:

- some value arrived
- it is greater, smaller, or unresolved relative to what came before

So calibration must begin from very weak structure.

### 2B. Local Basis Discovery

The first task of calibration is to form a usable local basis.

The smallest plausible local basis is:

- origin
- unit
- value projected through that unit

This gives the first local read of:

- direction
- rough magnitude
- reversal
- collapse to zero
- overflow or excessive span

### 2C. Resolution Fitting

Calibration is not only about what one means.

It is also about:

- how many ticks should be used
- whether the current denominator is too coarse
- whether the current denominator is too fine

The important rule is:

- if everything reads as `0`, the current inspection is too coarse
- if everything saturates or overflows, the current inspection is too fine
- if directional variation becomes readable, the current resolution is becoming
  useful

### 2D. Correlation Before Full Calibration

Before two sites can share a unit relation, they should first discover whether
they share a directional pattern at all.

The first weak comparisons are:

- same-direction change
- inverse-direction change
- no stable directional relation

Only after that should richer calibration be attempted.

So:

- correlation comes before conversion
- range fitting comes before hardened basis exchange

## 3. Two Stages

The work should be divided into two stages.

### Stage 1: Self-Calibration

One site adapts from its own repeated orthogonal input only.

This stage should answer:

- how does a site discover a first origin?
- how does it discover a first unit?
- how does it notice collapse, overflow, drift, or reversal?
- how does it adapt without deep memory?

### Stage 2: Inter-Site Calibration

Two or more sites exchange information after each has at least some local
calibration.

This stage should answer:

- how do sites compare direction before scale?
- how do they detect same, inverse, or irrelevant relation?
- how do they negotiate useful resolution windows?
- how do they exchange calibrated value without assuming immediate unit
  equality?

The first implementation pass should focus only on Stage 1.

## 4. Immediate Primitive Gap

The current engine already has:

- repeated orthogonal input
- local aligned settlement
- residual remainder
- local flow structure

through:

- [Site.cs](/C:/Users/Robin/source/repos/Core4/Core4/Structure/Site.cs)
- [AlignedKernel.cs](/C:/Users/Robin/source/repos/Core4/Core4/Structure/AlignedKernel.cs)
- [OrthogonalReservoir.cs](/C:/Users/Robin/source/repos/Core4/Core4/Dynamics/OrthogonalReservoir.cs)
- [SiteSettlement.cs](/C:/Users/Robin/source/repos/Core4/Core4/Dynamics/SiteSettlement.cs)

What it is missing is an explicit local calibration mechanism for:

- candidate origin
- candidate unit
- candidate tick count or resolution
- confidence or stiffness in those values

Right now settlement happens, but the local measuring law is still mostly
assumed rather than discovered.

## 5. Stage 1: Self-Calibration Plan

### 5A. Minimal First Pass

The smallest useful self-calibration object should remain tiny.

Initial target:

- one carrier
- three rolling measures
- first measure = origin
- second measure = unit marker
- third measure = projected value
- fourth measure and beyond shift the three-measure window left

This is the minimal live structure now represented by:

- [Calibrater.cs](/C:/Users/Robin/source/repos/Core4/Core4/Calibration/Calibrater.cs)

It is intentionally weak. Its job is to expose the first usable structure, not
to finish calibration.

### 5B. First Tensions To Detect

Once the minimal rolling structure exists, the next pass should derive simple
numeric tensions from it:

- collapse tension:
  - projected values repeatedly read as `0`
- saturation tension:
  - projected values repeatedly run outside the expected local span
- drift tension:
  - values consistently land far from the current center
- reversal tension:
  - direction flips more than the current local basis expects
- cost tension:
  - the site is carrying more resolution than the current behavior requires

These should be represented numerically rather than by enum as the source of
truth.

### 5C. Adjustable Quantities

The self-calibration pass should eventually let the site adapt at least:

- origin
- unit span
- tick count
- stiffness or confidence
- expected local range

These should not all be collapsed into one denominator.

The likely long-term split is:

- unit span = what one means
- tick count = how finely one is inspected
- stiffness = how hard it is to move origin or unit
- published coarseness = how finely the site chooses to report outward

### 5D. Update Rule

The first useful update loop is:

1. receive raw orthogonal input
2. project it through current origin and unit
3. measure local tensions
4. nudge origin, unit, or ticks
5. settle a new aligned reading
6. retain remainder for the next round

This should remain local and low-memory.

### 5E. Fast And Slow Layers

A promising later extension is to separate:

- fast exploratory calibration
- slow published calibration

The fast layer moves more freely and tests possible local readings.

The slow layer is what the site believes strongly enough to publish outward.

This should not be required in the first pass, but it is likely the right
second step after the rolling three-measure model.

## 6. Stage 2: Inter-Site Calibration Plan

Once sites have some local basis, a second stage can begin.

### 6A. Weak Comparison First

Two sites should first compare only weak structure:

- same-direction correlation
- inverse-direction correlation
- no useful relation

This should work even when:

- one side is still too coarse
- one side still saturates
- one side only knows trend and not scale

### 6B. Resolution Window Negotiation

If directional compatibility is detected, the sites should next test:

- can both see useful variation at all?
- is one side always seeing `0`?
- is one side always saturating?

This is the stage where useful exchange may still fail because the resolution
window is wrong even though the information is relevant.

### 6C. Unit Relation Negotiation

Only after directional compatibility and usable range fit should the sites try
to negotiate:

- scale
- inversion
- offset
- piecewise or range-limited compatibility

This is where a stable conversion law may eventually harden.

### 6D. Uptake And Exchange

A later site-to-site calibration pass should work alongside the communication
model described in:

- [Core4 Site Relation Supplement.md](/C:/Users/Robin/source/repos/Core4/Core4/axioms/Core4%20Site%20Relation%20Supplement.md)

especially:

- local aligned value
- orthogonal input and output
- pressure
- transfer
- stored imbalance

So later calibration between sites should not be treated as a detached
conversion table. It should be part of the same tension-lowering communication
process.

## 7. Proposed Namespace Direction

Calibration should likely remain its own small namespace:

- `Core4.Calibration`

This is useful because calibration is not:

- topology
- basis encounter
- ordinary traversal

but it still needs to feed all of those later.

The namespace should stay small and avoid class explosion.

The default rule should be:

- one small calibrater mechanism
- richer calibration behavior grows by extending that numeric state
- do not create many symbolic calibration role types too early

## 8. Planned Core Additions

The most plausible additions after the current rolling calibrater are:

- one local calibration state carrying:
  - origin
  - unit span
  - tick count
  - stiffness
  - expected range
- one local calibration step carrying:
  - incoming sample
  - projected reading
  - tensions detected this round
  - next state

These should remain derived and immutable.

Prefer:

- calculated values
- explicit numeric tensions

over:

- cached symbolic status flags
- case-by-case calibration modes

## 9. Demo Expectations

The calibration probe section should make the weakest stage visible first.

The first demo should show:

- rolling three-value calibration windows
- many real-world raw datasets
- no claim yet that the calibrater is smart
- only the first structural bridge from repeated data into local basis

That is what the current calibration page is for:

- [CalibrationPageView.cs](/C:/Users/Robin/source/repos/Core4/Core4.AvaloniaProbe/Core4.AvaloniaProbe/Views/CalibrationPageView.cs)

Later calibration demos should show:

- zero-collapse
- saturation
- tick refinement
- tick coarsening
- drift correction
- eventually cross-site directional matching

## 10. Tests To Add

### 10A. Current Minimal Pass

Already covered:

- first three samples become origin/unit/value
- later samples shift left
- zero-span unit produces tension

through:

- [CalibraterTests.cs](/C:/Users/Robin/source/repos/Core4/Core4.Tests/Calibration/CalibraterTests.cs)

### 10B. Next Calibration Pass

Once the adaptive pass stabilizes, add tests for:

- repeated zero causes finer resolution
- repeated overflow causes coarser resolution
- stable drift nudges origin
- stable same-sign trend preserves direction
- repeated reversal pushes toward richer or reset structure

### 10C. Later Inter-Site Pass

Add tests for:

- same-direction correlation detection
- inverse-direction correlation detection
- incompatible resolution windows blocking useful exchange
- negotiated resolution fit restoring useful exchange

## 11. Suggested Order Of Work

1. Keep the current rolling three-value calibrater as the baseline primitive.
2. Add numeric tensions for collapse, saturation, drift, and reversal.
3. Let the calibrater adapt origin and unit span in response.
4. Add adaptive tick count once origin/unit adaptation is stable.
5. Add stiffness or confidence so calibration can harden over time.
6. Only then add directional comparison between two calibrated sites.
7. Add resolution-window negotiation between sites before trying full scale
   conversion.
8. Only after that let stable inter-site calibration harden into reusable basis
   relations.

## 12. Guiding Constraint

If calibration begins to require:

- many symbolic mode enums
- many disconnected helper classes
- a separate non-Core4 statistical bookkeeping layer

then first check whether the same behavior can instead be expressed as:

- repeated measures
- local relation
- explicit numeric tension
- origin / unit / ticks / stiffness updates

The calibration mechanism should stay as close as possible to the same Core4
relation law already used elsewhere.
