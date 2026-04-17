using Core4.Encoding;

namespace Core4.Experimental;

/// <summary>
/// A tuple-shaped numeric summary of one experimental relation. The first field preserves direction by carrying
/// separate base/attached element orders, while the remaining fields describe recursive freeze depth, carrier
/// compatibility, shared-base structure, and repetition.
/// </summary>
public readonly record struct RelationSignature(
    ElementOrderSignature ElementOrder,
    Measure FreezeOrder,
    Measure CarrierPattern,
    Measure BasePattern,
    Measure AlignmentPattern,
    Measure FusionPattern,
    Measure Multiplicity);

/// <summary>
/// Ordered numeric shape of the two relation ends. This stays separate instead of being packed so later structural
/// axes can be added without losing which side was base and which side was attached.
/// </summary>
public readonly record struct ElementOrderSignature(Measure BaseOrder, Measure AttachedOrder);
