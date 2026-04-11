using System.Collections.Generic;

namespace Core4.AvaloniaProbe.Views;

internal static class ProbeGlossary
{
    public static IReadOnlyList<GlossarySection> Sections { get; } =
    [
        new GlossarySection(
            "Claim / Creation",
            "This section reads support-first: carrier, then site, then reading. From there the same pattern appears twice: pin, origin, relation for the primitive live case, and freeze, basis, attachment for the richer reusable case.",
            [
                new GlossaryEntry("Carrier", "Ordered support.", "A carrier is the support a value lives on or moves along. It is the ordered attribute or line of possible relation, not yet a reading by itself."),
                new GlossaryEntry("Site", "Place where relation may occur.", "A site is a referencable place on a carrier together with its current relation state. It is not only a location, but a location that may be open, occupied, or tension-bearing."),
                new GlossaryEntry("Measure", "Reading of a site.", "A measure is a reading of a site with no implied zero, scale, or extent. It is the first detected value, but not yet a committed relation."),
                new GlossaryEntry("Pin", "Hold a site stable enough to reference.", "To pin is to hold a site stable enough to reference again later. It is the primitive site-level form of freeze: a place is held, but no extent has been committed yet.", DividerBefore: true),
                new GlossaryEntry("Origin", "First reusable reference site.", "An origin is the first pinned site being reused as reference. In that sense it is the primitive basis: a committed place before a frozen interval exists."),
                new GlossaryEntry("Relation", "Origin plus another site gives a live interval.", "A relation is an ordered coupling between sites. Once an origin is related to a second site, a live interval exists, and in its simplest form this is the primitive live attachment."),
                new GlossaryEntry("Freeze", "Commit current structure for reuse.", "To freeze is the general act of committing current structure for reuse. Freezing a site yields an origin; freezing a live relation yields a basis.", DividerBefore: true),
                new GlossaryEntry("Basis", "Frozen reusable calibration.", "A basis is a frozen relation, or a frozen fusion of relations. It is reusable calibration rather than a one-time observation."),
                new GlossaryEntry("Attachment", "Live structure read through basis.", "An attachment is a live binding of current sites into a frozen basis. It is the richer counterpart to primitive relation: present structure being read through committed structure.")
            ]),
        new GlossarySection(
            "Measurement / Basis",
            "These terms describe how frozen structure calibrates readings, supports placement, and grows into richer fused objects.",
            [
                new GlossaryEntry("Unit", "Simplest portable basis.", "A unit is a frozen interval reused as a denominator for later readings. It carries direction, a pinned zero, and a fixed tick count."),
                new GlossaryEntry("Ratio", "Unreduced fraction through a unit.", "A ratio expresses a reading as some number of the unit's fixed ticks. It should not be reduced, because that denominator is the unit's resolution."),
                new GlossaryEntry("Placement", "Magnitude made positional.", "A placement is what happens when a ratio is read along a carrier. It makes a value into a position rather than only an uncalibrated amount."),
                new GlossaryEntry("Resolution", "How many positions the unit can distinguish.", "Resolution is the number of ticks in the unit's denominator, and therefore the distinct placements available across that unit's range."),
                new GlossaryEntry("Tension", "Unresolved explanatory remainder.", "Tension is value that has not yet been cleanly explained by the basis in play. It remains inspectable as unresolved potential rather than error.", DividerBefore: true),
                new GlossaryEntry("Directed Segment", "Bounded same-carrier dual-unit.", "A directed segment is a same-carrier opposed pair with shared origin. It gives a bounded 1D range whose span falls out naturally."),
                new GlossaryEntry("Orthogonal Pair", "Independent units fused across carriers.", "Two independent units on different carriers form a higher-grade fused structure. They make 2D relation physically inspectable rather than symbolic."),
                new GlossaryEntry("Area Basis", "Aligned orthogonal bounded area.", "An aligned orthogonal pair gives bounded area. It reads like two dimensions cooperating."),
                new GlossaryEntry("Complex Basis", "Opposed orthogonal ray-like basis.", "An opposed orthogonal pair gives a ray-like complex structure with length and angle. It reads like two dimensions acting against each other."),
                new GlossaryEntry("Grade", "Committed fused structural freedom.", "Grade counts how much fused structural freedom has been committed. Higher grade means richer calibration, not just more labels."),
                new GlossaryEntry("Grade-3 Basis", "Fusion one level beyond 2D pair.", "A grade-3 basis is a fusion one level beyond the familiar 2D pair. It is a compact way to show how grades accumulate.")
            ]),
        new GlossarySection(
            "Transform / Composition",
            "These are the terms where structure begins to act, combine, and change what kinds of motion or reading are meaningful.",
            [
                new GlossaryEntry("Flow", "Live support-linked influence.", "A flow is live support-linked influence entering, leaving, or oscillating at a site. It is how local pressure appears before full settlement."),
                new GlossaryEntry("Fused Pair", "Interpretation internalized into structure.", "Fusing bases internalizes interpretation into structure instead of leaving it as loose data. The resulting object carries more of its own meaning."),
                new GlossaryEntry("Self Pair", "Unit fused with itself.", "A self pair fuses a unit with itself rather than with an orthogonal partner. This changes calibration and opens non-linear spacing or power-like behavior."),
                new GlossaryEntry("Encountered Basis", "Local calibration law met in traversal.", "An encountered basis is a pinned local calibration law met during traversal. The same basis can be a passive statement when frozen and an active transfer law when traversed.")
            ]),
        new GlossarySection(
            "Inspection",
            "These terms describe how a trolley reads local calibration, chooses continuation, and recognizes boundary or loop behavior.",
            [
                new GlossaryEntry("Continuation", "How present motion remains intelligible.", "Continuation is the local question of how present motion remains intelligible at the next site. It can preserve, invert, weaken, or block the current traversal."),
                new GlossaryEntry("Closure", "Endpoint remapping through basis encounter.", "Closure is endpoint remapping expressed through basis encounter and continuation agreement. It is not a separate symbolic loop flag."),
                new GlossaryEntry("Trolley", "External inspector riding structure.", "A trolley is a small external inspector that rides structure without mutating it. It carries calibration plus traversal state while comparing that to encountered local bases."),
                new GlossaryEntry("Propagation Wave", "Topology-wide family of trolley fronts.", "A propagation wave is a topology-wide family of trolley fronts. It is useful for seeing branching, parking, partial transfer, and periodic re-entry together.")
            ])
    ];

    public readonly record struct GlossarySection(string Title, string Blurb, IReadOnlyList<GlossaryEntry> Entries);

    public readonly record struct GlossaryEntry(string Term, string Summary, string Description, bool DividerBefore = false);
}
