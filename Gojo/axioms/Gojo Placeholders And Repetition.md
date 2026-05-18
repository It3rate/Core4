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
