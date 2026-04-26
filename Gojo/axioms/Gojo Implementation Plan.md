# Gojo Implementation Plan

Gojo is the next kernel pass after `Core4.Kernel`.

`Core4.Kernel` remains preserved for comparison. Gojo starts from the newer
carrier-layer reading:

- carrier-local storage is positive-flow storage
- sign is a cross-carrier read
- operations are carrier layers
- each layer has one transform value
- boundary overflow creates residue or tension unless routed

## Phase 1. Positive Carrier Reads

Implement the smallest executable model:

- positive carrier identity
- positive carrier location
- same-carrier reads
- orthogonal reads returning zero
- opposed carrier reads projecting as negative

This phase proves that location and read are separate.

## Phase 2. Transform Paths

Add one-transform-per-layer paths:

- expansive reads
- contractive reads
- reverse reads with residue or tension
- zero transform as additive or non-mixing attachment

This phase should cover multiplication-like and division-like reads without
ordinary algebraic operator tags.

## Phase 3. Residue And Boundary Events

Add carrier-layer boundaries:

- carrier tick overflow
- attachment residue
- four directional boundary events
- unrouted overflow as tension

This phase should produce reads such as `23 4|10 C`.

## Phase 4. Routes

Add route structures:

- resolved transfer routes
- zero-unit observation routes
- open tension routes
- counters
- wrap
- clamp
- ping-pong
- segment end-to-start closure

This phase should make boundary behavior compositional instead of flag-driven.

## Phase 5. Filters And Support

Rebuild support and boolean-like reads on top of routes:

- pass
- block
- branch
- merge
- residue
- support/tension/exclusive reads

This phase should replace boolean branching with carrier-native route support.

## Migration Rule

Only copy code from `Core4.Kernel` when it still preserves the Gojo direction.
Do not carry over signed `long` fold behavior as canonical storage. It may stay
as a projection only.
