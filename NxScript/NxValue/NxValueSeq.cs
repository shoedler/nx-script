namespace NxScript;

public partial class NxValueSeq : NxValue
{
    public List<NxValue> seqValue;
    public override bool IsSeq => true;
    public override NxValueType Type => NxValueType.Seq;

    public NxValueSeq(List<NxValue> value)
    {
        this.seqValue = value;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return this == obj;
    }

    public override dynamic GetInternalValue() => this.seqValue;

    public override List<NxValue> AsSeq() => this.seqValue;

    public override float AsNumber() => this.seqValue.Count;

    public override bool AsBool() => true;

    public override string AsString()
    {
        var items = this.seqValue.Select(item => item.AsString());
        return "[" + string.Join(", ", items) + "]";
    }

    public override Dictionary<NxValue, NxValue> AsObj()
    {
        var dict = new Dictionary<NxValue, NxValue>();

        foreach (var value in this.seqValue)
        {
            var valueCopy = NxValue.CopyUnknown(value);
            if (!dict.TryAdd(value, valueCopy))
            {
                dict[value] = valueCopy;
            }
        }

        return dict;
    }

    public override Func<List<NxValue>, NxValue> AsFn()
    {
        return new Func<List<NxValue>, NxValue>((List<NxValue> Args) => this);
    }
}