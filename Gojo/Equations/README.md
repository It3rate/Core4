# Gojo Equations

`Gojo.Equations` is the first foothold for relation-shaped equations. An
equation is not a symbolic formula object. It is a Gojo surface that admits
input participation, predicts output participation, and exposes residue or
error as structure.

## Affine Surface

The first equation shape is an affine frame surface:

```text
Segment(input, inputStart, inputEnd)
  -> Segment(output, outputStart, outputEnd)
```

`inputStart` and `inputEnd` are basis endpoints, not consecutive samples.

```text
Segment(sensor,0,10) -> Segment(percent,0,100)
sensor = 3 reads as percent = 30
```

The same shape covers unit conversion, normalization, proportional gear reads,
inverse direction, offsets, and coarse percentage projections.

## Prediction

Prediction is a read through the equation surface:

```text
input sample -> predicted output + source residue
```

The data does not become part of the equation. It is admitted through the input
surface and read into the requested output carrier.

## Sum And Product Surfaces

Weighted sums and products use existing Gojo operation surfaces. The ports are
ordinary frame relations, and the output surface decides the operation shape:

```text
// weighted sum
((Segment(x,0,1) -> Segment(Unknown,0,2))
  -> (Segment(y,0,1) -> Segment(Unknown,0,3)))
  -> Point(output,1)

// product
((Point(width,1) -> Point(Unknown,1))
  -> (Point(height,1) -> Point(Unknown,1)))
  -> Segment(area,0,1)
```

For the sum, each operand is projected into the equation reader basis and then
added. For the product, each operand is projected and then multiplied. Biases
or constants can be represented as ordinary supplied slots, such as a bias
carrier with value `1` and a weighted port of `0..5`.

## Ordered Map

Mapping a stream is represented as structure:

```text
MapRoot
  + index span
  + equation surface
  + sample 0
  + sample 1
  + sample 2

SampleN
  + Point(index,N)
  + input location or relation-shaped input
  + predicted output location
  + source residue location or relation-shaped residue
```

The current helper still executes the per-sample loop in code, but the result
records the index sites and per-sample prediction groups. This keeps the
direction compatible with later time-step and SIMD-style execution without
making SIMD the source of meaning.

## Comparison

Comparison is also a structure:

```text
CompareRoot
  + index span
  + comparison sample 0
  + comparison sample 1

ComparisonSampleN
  + Point(index,N)
  + predicted output
  + observed output
  + error
```

For the first pass, error is `observed - predicted` on a supplied error
carrier. Later this should become richer tension/residue structure that can
drive parameter fitting and equation selection.

## Next Targets

The intended sequence is:

```text
affine frame             y = ax + b
weighted sum             y = a*x + b*z + c
difference/error         target - observed
product/bilinear         x*z
piecewise/clamp/gate     bounded local equations
polynomial               c0 + c1*x + c2*x^2 ...
state transition         state[t+1] = f(state[t], input[t])
exponential/log/trig     higher equation families
candidate fitting        samples -> parameters + residual
```

Each target should first be a clear Gojo structure, then an executable helper,
and only later a compiled/backend representation.
