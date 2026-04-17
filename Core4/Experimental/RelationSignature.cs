using Core4.Encoding;

namespace Core4.Experimental;

/// <summary>
/// A tuple-shaped numeric summary of one experimental relation. The first field preserves direction by carrying
/// separate base/attached element orders, while the remaining fields describe recursive freeze depth, carrier
/// compatibility, shared-base structure, higher-order operand shape, and repetition.
/// </summary>
public readonly record struct RelationSignature(
    ElementOrderSignature ElementOrder,
    Measure FreezeOrder,
    Measure CarrierPattern,
    Measure BasePattern,
    Measure ReadPattern,
    Measure AlignmentPattern,
    Measure FusionPattern,
    OperandShapeSignature OperandShape,
    Measure Multiplicity);

/// <summary>
/// Ordered numeric shape of the two relation ends. This stays separate instead of being packed so later structural
/// axes can be added without losing which side was base and which side was attached.
/// </summary>
public readonly record struct ElementOrderSignature(Measure BaseOrder, Measure AttachedOrder);

/// <summary>
/// Recursive shape summary of the two operands when they are themselves structured relations. This surfaces what kind
/// of things are being fused without requiring a second full relation signature at the top level.
/// </summary>
public readonly record struct OperandShapeSignature(
    Measure BaseFreezeOrder,
    Measure AttachedFreezeOrder,
    Measure BaseReadPattern,
    Measure AttachedReadPattern,
    Measure BaseFusionPattern,
    Measure AttachedFusionPattern);
