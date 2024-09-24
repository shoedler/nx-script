
namespace NxScript;

public class NxValueNil : NxValue
{
    public override bool IsNil => true;
    public override NxValueType Type => NxValueType.Nil;

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || this.GetType() != obj.GetType())
        {
            return false;
        }

        return obj is NxValueNil;
    }

    public override dynamic GetInternalValue() => null;

    public override float AsNumber() => 0f;

    public override string AsString() => string.Empty;

    public override bool AsBool() => false;

    public override List<NxValue> AsSeq() => [];

    public override Dictionary<NxValue, NxValue> AsObj() => new();

    public override Func<List<NxValue>, NxValue> AsFn() => _ => this;
}