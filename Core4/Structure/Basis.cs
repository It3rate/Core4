using System.Collections.Immutable;
using Core4.Numerics;

namespace Core4.Structure;

/// <summary>
/// A frozen relation, or a frozen fusion of relations. In the support-first reading this is reusable calibration over
/// shared support structure rather than a separate primitive species. Because a basis is also a support, placements
/// can travel along it by ratio just like they do on a carrier.
/// Example: <c>var segment = Basis.Fuse(leftBasis, rightBasis); var point = segment.Place(new Ratio(1, 2));</c>
/// </summary>
public sealed class Basis : ISupport, ISupportFold
{
    private readonly ImmutableArray<Basis> _inputs;
    private readonly ImmutableArray<Relation> _relations;
    private readonly SupportStructure _structure;

    private Basis(Relation relation)
    {
        _inputs = ImmutableArray<Basis>.Empty;
        _relations = ImmutableArray.Create(relation);
        _structure = relation.Structure;
    }

    private Basis(Basis recessive, Basis dominant)
    {
        _inputs = ImmutableArray.Create(recessive, dominant);

        var relations = ImmutableArray.CreateBuilder<Relation>(recessive._relations.Length + dominant._relations.Length);
        relations.AddRange(recessive._relations);
        relations.AddRange(dominant._relations);
        _relations = relations.MoveToImmutable();
        _structure = SupportStructure.Fuse(recessive._structure, dominant._structure);
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

    public Ratio Fold => _structure.Fold;

    // Compatibility alias while callers move from ratio-as-primitive wording toward fold/support reads.
    public Ratio Ratio => Fold;

    public Measure Orientation => _structure.Orientation;

    public Ratio Stiffness => _structure.Stiffness;

    public bool IsRecessive => _structure.IsRecessive;

    public bool IsDominant => _structure.IsDominant;

    internal Relation AtomicRelation => _relations[0];

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

    public Basis Flip() => IsAtomic
        ? Freeze(AtomicRelation.Flip())
        : Fuse(Recessive.Flip(), Dominant.Flip());

    public Placement Place(Ratio ratio) => new(this, ratio.Normalize());

    public Placement Place(ISupportProjection projection) => new(this, projection.AsRatio());

    public Ratio Locate(Ratio position, Carrier carrier) => _structure.Locate(position, carrier);

    public Ratio Locate(ISupportProjection position, Carrier carrier) => Locate(position.AsRatio(), carrier);

    public Ratio Project(Site site) => _structure.Project(site);

    public Ratio Project(Measure measure) => _structure.Project(measure);

    public Attachment Attach(params Site[] sites) => Attachment.Attach(this, sites);

    public Attachment Attach(params Measure[] measures) => Attachment.Attach(this, measures);

    public ImmutableArray<Relation> SelectRelations() => _relations;

    public ImmutableArray<Relation> SelectRelations(Carrier carrier)
    {
        var relations = ImmutableArray.CreateBuilder<Relation>();

        foreach (var relation in _relations)
        {
            if (relation.Carrier == carrier)
            {
                relations.Add(relation);
            }
        }

        return relations.ToImmutable();
    }

    public ImmutableArray<Carrier> SelectCarriers() => _structure.SelectCarriers();

    public bool HasSharedOriginPerCarrier() => _structure.HasSharedOriginPerCarrier();

    public bool IsSameCarrierPair() => _structure.IsSameCarrierPair();

    public bool IsAlignedPair() => _structure.IsAlignedPair();

    public bool IsOpposedPair() => _structure.IsOpposedPair();

    public Measure KernelCode() => _structure.KernelCode();
}
