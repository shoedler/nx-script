
namespace NxScript;

public partial class NxValueNil : NxValue
{
    public override bool IsNil => true;
    public override NxValueType Type => NxValueType.Nil;

    public NxValueNil() { }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        if (obj is NxValueNil)
        {
            return true;
        }

        return false;
    }

    public override dynamic GetInternalValue() => null;

    public override float AsNumber() => 0f;

    public override string AsString() => string.Empty;

    public override bool AsBool() => false;

    public override List<NxValue> AsSeq() => new();

    public override Dictionary<NxValue, NxValue> AsObj() => new();

    public override Func<List<NxValue>, NxValue> AsFn() => new((List<NxValue> args) => this);
}