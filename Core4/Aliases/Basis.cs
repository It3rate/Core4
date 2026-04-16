using System.Collections.Immutable;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.Aliases;

/// <summary>
/// A frozen relation, or a frozen fusion of relations. In the support-first reading this is reusable calibration over
/// shared support structure rather than a separate primitive species. Because a basis is also a support, placements
/// can travel along it by ratio just like they do on a carrier.
/// Example: <c>var segment = Basis.Fuse(leftBasis, rightBasis); var point = segment.Place(new Ratio(1, 2));</c>
/// </summary>
public sealed class Basis : ISupport, ISupportFold
{
    private readonly SupportStructure _structure;
    private readonly ImmutableArray<Basis> _inputs;
    private readonly ImmutableArray<Relation> _relations;

    internal Basis(SupportStructure structure)
    {
        _structure = structure;
        _inputs = _structure.Inputs.Select(static input => new Basis(input)).ToImmutableArray();
        _relations = _structure.SelectRelations();
    }

    private Basis(Relation relation) : this(relation.Structure)
    {
    }

    private Basis(Basis recessive, Basis dominant) : this(SupportStructure.Fuse(recessive._structure, dominant._structure))
    {
    }

    public bool IsAtomic => _structure.IsAtomic;

    public bool IsComposite => _structure.IsComposite;

    public bool IsAtomicPair => _structure.IsAtomicPair();

    public Measure Grade => _structure.SupportGrade;

    public Carrier PrimaryCarrier => _structure.PrimaryCarrier;

    public Measure SupportGrade => _structure.SupportGrade;

    public Measure RelationCount => _structure.RelationCount;

    public Measure AttachmentArity => RelationCount;

    public Measure CarrierCount => _structure.CarrierCount;

    public ImmutableArray<Basis> Inputs => _inputs;

    public ImmutableArray<Relation> Relations => _relations;

    internal SupportStructure Structure => _structure;

    public Basis Recessive => IsComposite ? _inputs[0] : this;

    public Basis Dominant => IsComposite ? _inputs[1] : this;

    public Measure Potential => _structure.Potential;

    public ISupportProjection FoldRead => _structure.FoldRead;

    // TODO(core): Delete this compatibility alias once callers consume FoldRead directly.
    public Ratio Fold => FoldRead.AsRatio();

    // TODO(core): Delete this compatibility alias once callers stop expecting basis-wide Ratio.
    public Ratio Ratio => Fold;

    public Measure Orientation => _structure.Orientation;

    public Ratio Stiffness => _structure.Stiffness;

    public bool IsRecessive => _structure.IsRecessive;

    public bool IsDominant => _structure.IsDominant;

    internal Relation AtomicRelation => _structure.AtomicRelation;

    public static Basis Freeze(Relation relation) => Freeze(relation, new Ratio(Measure.One, Measure.One));

    public static Basis Freeze(Relation relation, Ratio stiffness) => new(relation.Harden(stiffness));

    public static Basis Freeze(Carrier carrier, Measure origin, Measure anchor) =>
        Freeze(carrier, origin, anchor, new Ratio(Measure.One, Measure.One));

    public static Basis Freeze(Carrier carrier, Measure origin, Measure anchor, Ratio stiffness) => Freeze(
        new Relation(
            carrier,
            Site.Open(carrier, origin),
            Site.Open(carrier, anchor)),
        stiffness);

    public static Basis Freeze(Site origin, Site anchor) => Freeze(origin, anchor, new Ratio(Measure.One, Measure.One));

    public static Basis Freeze(Site origin, Site anchor, Ratio stiffness) => Freeze(new Relation(origin, anchor), stiffness);

    public static Basis Freeze(Carrier carrier, Site origin, Site anchor) =>
        Freeze(carrier, origin, anchor, new Ratio(Measure.One, Measure.One));

    public static Basis Freeze(Carrier carrier, Site origin, Site anchor, Ratio stiffness) =>
        Freeze(new Relation(carrier, origin, anchor), stiffness);

    public static Basis Fuse(Relation recessive, Relation dominant, params Relation[] remaining)
    {
        var bases = new Basis[remaining.Length + 2];
        bases[0] = Freeze(recessive);
        bases[1] = Freeze(dominant);

        for (var index = 0; index < remaining.Length; index++)
        {
            bases[index + 2] = Freeze(remaining[index]);
        }

        var rest = new Basis[Math.Max(0, bases.Length - 2)];

        for (var index = 2; index < bases.Length; index++)
        {
            rest[index - 2] = bases[index];
        }

        return Fuse(bases[0], bases[1], rest);
    }

    public static Basis Fuse(Basis recessive, Basis dominant, params Basis[] remaining)
    {
        var fused = new Basis(recessive, dominant);

        for (var index = 0; index < remaining.Length; index++)
        {
            fused = new Basis(fused, remaining[index]);
        }

        return fused;
    }

    public Basis Flip() => new(_structure.Flip());

    public Placement Place(ISupportProjection projection) => new(this, projection);

    // TODO(core): Delete this compatibility overload once callers pass support projections directly.
    public Placement Place(Ratio ratio) => Place((ISupportProjection)ratio);

    public Ratio Locate(ISupportProjection position, Carrier carrier) => _structure.Locate(position, carrier);

    // TODO(core): Delete this compatibility overload once callers pass support projections directly.
    public Ratio Locate(Ratio position, Carrier carrier) => Locate((ISupportProjection)position, carrier);

    public ISupportProjection ProjectRead(Site site) => _structure.ProjectRead(site);

    // TODO(core): Delete this compatibility alias once callers consume ProjectRead directly.
    public Ratio Project(Site site) => ProjectRead(site).AsRatio();

    public ISupportProjection ProjectRead(Measure measure) => _structure.ProjectRead(measure);

    // TODO(core): Delete this compatibility alias once callers consume ProjectRead directly.
    public Ratio Project(Measure measure) => ProjectRead(measure).AsRatio();

    public Attachment Attach(params Site[] sites) => Attachment.Attach(this, sites);

    public Attachment Attach(params Measure[] measures) => Attachment.Attach(this, measures);

    public ImmutableArray<Relation> SelectRelations() => _relations;

    public ImmutableArray<Relation> SelectRelations(Carrier carrier) => _structure.SelectRelations(carrier);

    public ImmutableArray<Carrier> SelectCarriers() => _structure.SelectCarriers();

    public bool HasSharedOriginPerCarrier() => _structure.HasSharedOriginPerCarrier();

    public bool IsSameCarrierPair() => _structure.IsSameCarrierPair();

    public bool IsAlignedPair() => _structure.IsAlignedPair();

    public bool IsOpposedPair() => _structure.IsOpposedPair();

    public Measure KernelCode() => _structure.KernelCode();
}
