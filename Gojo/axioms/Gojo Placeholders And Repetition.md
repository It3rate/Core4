# Gojo Placeholders And Repetition

This note records the first placeholder and repeat foothold.

## Placeholder

A placeholder is the lowest-resolution element site:

```text
identity only
no carrier
no basis
no value
no unit shape until bound or constrained
```

The identity is important. `A * A` and `A * B` are different structures even
before `A` or `B` has a concrete carrier, relation, or value. A placeholder
also has an opposed face so dual participation can stay visible while sharing
the same abstract slot identity.

## Binding

Binding is ordinary relation-shaped structure:

```text
Placeholder(A) -> concrete element
```

The concrete element can be any `IElement`: a carrier, a carrier location, a
point surface, a segment, a port, a whole relation pattern, or another
placeholder. Binding is not assignment in hidden code. It is a relation that a
local pattern helper can read to instantiate a frame.

Unit-shape descriptor slots are also relation-shaped slots:

```text
DescriptorSlot(0) -> concrete descriptor or element
```

They remain the existing unit-shape algebra surface, not a new metadata table.
The pattern helper can now bind them the same way it binds identity
placeholders, so a bare unit-shape formula can be rebound to a concrete
provenance or value surface without changing the crystallized unit-shape
encoding. This is a bridge between "slot" as a repeat placeholder and "slot" as
an algebraic unit-shape descriptor.

The helper reads both forms through one local slot-surface pass:

```text
slot surface -> concrete element
```

At the moment, a slot surface can be an identity placeholder or a unit-shape
descriptor slot. The source of truth is still the element on the left of the
binding relation. The code-side helper only recognizes and substitutes that
visible surface.

Slot discovery is also local traversal:

```text
read slots(pattern) => first distinct slot surfaces found inside pattern
```

This gives equation and repeat helpers a way to ask which sites are open
without storing a separate list of ports. Descriptor slots are treated as whole
surfaces during this read; the helper does not descend into their marker
relation.

## Repeat

The first executable repeat helper does not create a durable repeat object.
It takes:

```text
pattern element
binding frame 0
binding frame 1
...
```

and produces ordinary instantiated Gojo elements. For example:

```text
pattern:
  CurrentBoundary -> NextOrigin

frame 0:
  CurrentBoundary -> P(ones,10)
  NextOrigin      -> P(tens,0)

frame 1:
  CurrentBoundary -> P(tens,10)
  NextOrigin      -> P(hundreds,0)
```

The result is the visible carry chain:

```text
P(ones,10) -> P(tens,0)
P(tens,10) -> P(hundreds,0)
```

This is intentionally mundane. A power-like repeat is the same mechanism with
the same placeholder bound repeatedly. A recurrence can later use slot-to-slot
bindings between frames, such as `A_next -> B_current`.

## Frame Continuation

A transition is also relation-shaped binding, but its right side may contain
placeholders from the current frame:

```text
A -> B
B -> A -> B
```

Applying the current frame to these transition bindings creates the next frame:

```text
current:
  A -> first
  B -> second

next:
  A -> second
  B -> first -> second
```

This is the first foothold for recurrence-like structure. It can describe a
Fibonacci-style state shift, a repeated manufacturing step, a commute episode
that carries forward yesterday's residue, or a power machine that reuses the
same operand several times. The transition does not mutate an equation during
ordinary tension flow. It produces the next set of bindings at a frame
boundary.

## Step Boundary

Live placeholder rebinding should happen at frame boundaries:

```text
bind current frame
settle values and tension
read residue/output
advance bindings for the next frame
```

Flow-time rebinding is intentionally avoided for now. If a structure appears to
need it, that is probably a sign that the model needs a finer time step or
should expose unresolved tension.

## Controller Surfaces

The first repeat controller helpers create ordinary phase carriers and
relations:

```text
wrap        P(phase,k) -> P(phase,0)
clamp       P(phase,k) -> P(phase,k)
cycle count P(phase,k) -> P(cycles,0)
```

These are not symbolic loop modes. They are the same carrier settlement shapes
used elsewhere in Gojo. A wrapped phase can select one repeated frame by its
current residue. A cycle counter can absorb crossed wraps. A clamp exposes
rejected continuation as tension.

Ping-pong is currently only a read over a doubled wrap period:

```text
P(phase,2k) -> P(phase,0)
read phase as 0..k..0
```

That keeps the reflected value inspectable as a local read while we decide how
much of the direction reversal should become executable relation structure.

## Port And Operation Patterns

Placeholders can stand for whole port, input, and output surfaces:

```text
operation pattern:
  (XPort -> YPort) -> Output

input pattern:
  XInput -> YInput
```

Binding those placeholders produces ordinary relation structure before the
existing projection, operation, and crystallization reads run. This lets a
single operation pattern serve many repeated frames without introducing a
parallel equation executor:

```text
XPort  -> P(x,1) -> P(Unknown,1)
YPort  -> P(y,1) -> P(Unknown,1)
Output -> P(result,1)
XInput -> L(x,3)
YInput -> L(y,4)
```

The helper layer is only an adapter. Once instantiated, Gojo sees normal port
and operation relations.
