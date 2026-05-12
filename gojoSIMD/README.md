# gojoSIMD

`gojoSIMD` is a native C++20 implementation experiment for the current Gojo
kernel model. It is intentionally separate from the C# `Gojo` project: this is
not a binary-compatible port and it does not make the packed representation the
source of truth for the existing interpreter.

The goal is to explore how Gojo would feel if its carriers, tick refs,
locations, relations, and unit-shape surfaces were stored in C++ structures that
are friendly to lane-oriented execution.

## Design Shape

- carriers are stored in structure-of-arrays blocks split by visible value
  resolution: 8, 16, 32, and 64 bit
- tick refs are also split by signed coordinate resolution
- carrier locations and relations stay as structural references between those
  blocks
- unit shapes remain relation-shaped surfaces
- settlement and projection inspect relation shape locally instead of using an
  operation enum
- unresolved range pressure remains visible as carrier tension

The current lane layer is portable C++: it batches table passes through fixed
width lane objects and keeps the data layout ready for real intrinsics later.
That keeps this first pass buildable and inspectable before choosing AVX2,
AVX-512, ARM NEON, or compiler `std::simd` bindings.

## Build

Visual Studio:

```powershell
gojoSIMD\gojoSIMD.sln
```

CMake:

```powershell
cmake -S gojoSIMD -B gojoSIMD/build
cmake --build gojoSIMD/build
ctest --test-dir gojoSIMD/build --output-on-failure
```

The repository root `Gojo.slnx` remains the active C# solution for the live
kernel and tests. The C++ projects live in their own solution because adding
`.vcxproj` files to `Gojo.slnx` makes the dotnet CLI try to import Visual
Studio C++ targets.
