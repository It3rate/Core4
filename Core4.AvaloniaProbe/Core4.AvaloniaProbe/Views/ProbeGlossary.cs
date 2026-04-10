using System.Collections.Generic;

namespace Core4.AvaloniaProbe.Views;

internal static class ProbeGlossary
{
    public static IReadOnlyList<GlossarySection> Sections { get; } =
    [
        new GlossarySection(
            "Claim / Creation",
            "These are the first Core4 constructions: a reading, a place to pin it, and the live or frozen relations that follow.",
            [
                new GlossaryEntry("Measure", "A measure is a single reading with no implied zero, scale, or extent. It is only a detected value."),
                new GlossaryEntry("Origin", "An origin is a measure that has been pinned so later readings can be compared to it. It does not create a reusable unit by itself."),
                new GlossaryEntry("Carrier", "A carrier is the support a value lives on or moves along. It is the line of possible relation, not the value itself."),
                new GlossaryEntry("Site", "A site is a referencable place where relation may occur. It is a location together with its current openness, occupancy, or tension."),
                new GlossaryEntry("Pin", "To pin is to choose a site as stable enough to reference. Pinning selects a place; it does not yet create a basis."),
                new GlossaryEntry("Relation", "A relation is an ordered coupling between sites. It is the first live extent on a carrier."),
                new GlossaryEntry("Freeze", "To freeze is to commit a live relation for reuse. It stabilizes what is already there instead of inventing new content."),
                new GlossaryEntry("Basis", "A basis is a frozen relation, or a frozen fusion of relations. It is reusable calibration rather than a one-time observation.")
            ]),
        new GlossarySection(
            "Measurement / Basis",
            "These terms describe how frozen structure calibrates readings, supports placement, and grows into richer fused objects.",
            [
                new GlossaryEntry("Unit", "A unit is the simplest portable basis: one frozen interval reused as a denominator. It carries local direction, resolution, and its own zero."),
                new GlossaryEntry("Placement", "A placement is a thing located on a support by ratio. It turns magnitude into position."),
                new GlossaryEntry("Ratio", "A ratio is a resolved or unresolved reading through a basis. It carries polarity, agreement, and tension numerically."),
                new GlossaryEntry("Tension", "Tension is value that has not yet been cleanly explained by the basis in play. It remains inspectable as unresolved potential rather than error."),
                new GlossaryEntry("Attachment", "An attachment is a live binding of current sites into a frozen basis. It is present structure being read through reusable structure."),
                new GlossaryEntry("Directed Segment", "A directed segment is a same-carrier opposed pair with shared origin. It gives a bounded 1D range whose span falls out naturally."),
                new GlossaryEntry("Orthogonal Pair", "Two independent units on different carriers form a higher-grade fused structure. They make 2D relation physically inspectable rather than symbolic."),
                new GlossaryEntry("Area Basis", "An aligned orthogonal pair gives bounded area. It reads like two dimensions cooperating."),
                new GlossaryEntry("Complex Basis", "An opposed orthogonal pair gives a ray-like complex structure with length and angle. It reads like two dimensions acting against each other."),
                new GlossaryEntry("Grade", "Grade counts how much fused structural freedom has been committed. Higher grade means richer calibration, not just more labels."),
                new GlossaryEntry("Grade-3 Basis", "A grade-3 basis is a fusion one level beyond the familiar 2D pair. It is a compact way to show how grades accumulate.")
            ]),
        new GlossarySection(
            "Transform / Composition",
            "These are the terms where structure begins to act, combine, and change what kinds of motion or reading are meaningful.",
            [
                new GlossaryEntry("Flow", "A flow is live support-linked influence entering, leaving, or oscillating at a site. It is how local pressure appears before full settlement."),
                new GlossaryEntry("Fused Pair", "Fusing bases internalizes interpretation into structure instead of leaving it as loose data. The resulting object carries more of its own meaning."),
                new GlossaryEntry("Self Pair", "A self pair fuses a unit with itself rather than with an orthogonal partner. This changes calibration and opens non-linear spacing or power-like behavior."),
                new GlossaryEntry("Encountered Basis", "An encountered basis is a pinned local calibration law met during traversal. The same basis can be a passive statement when frozen and an active transfer law when traversed.")
            ]),
        new GlossarySection(
            "Inspection",
            "These terms describe how a trolley reads local calibration, chooses continuation, and recognizes boundary or loop behavior.",
            [
                new GlossaryEntry("Continuation", "Continuation is the local question of how present motion remains intelligible at the next site. It can preserve, invert, weaken, or block the current traversal."),
                new GlossaryEntry("Closure", "Closure is endpoint remapping expressed through basis encounter and continuation agreement. It is not a separate symbolic loop flag."),
                new GlossaryEntry("Trolley", "A trolley is a small external inspector that rides structure without mutating it. It carries calibration plus traversal state while comparing that to encountered local bases."),
                new GlossaryEntry("Propagation Wave", "A propagation wave is a topology-wide family of trolley fronts. It is useful for seeing branching, parking, partial transfer, and periodic re-entry together.")
            ])
    ];

    public readonly record struct GlossarySection(string Title, string Blurb, IReadOnlyList<GlossaryEntry> Entries);

    public readonly record struct GlossaryEntry(string Term, string Description);
}
