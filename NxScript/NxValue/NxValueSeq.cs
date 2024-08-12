namespace NxScript;

public class NxValueSeq : NxValue
{
    public List<NxValue> SeqValue;
    public override bool IsSeq => true;
    public override NxValueType Type => NxValueType.Seq;

    public NxValueSeq(List<NxValue> value)
    {
        this.SeqValue = value;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return this == obj;
    }

    public override dynamic GetInternalValue() => this.SeqValue;

    public override List<NxValue> AsSeq() => this.SeqValue;

    public override float AsNumber() => this.SeqValue.Count;

    public override bool AsBool() => true;

    public override string AsString()
    {
        var items = this.SeqValue.Select(item => item.AsString());
        return "[" + string.Join(", ", items) + "]";
    }

    public override Dictionary<NxValue, NxValue> AsObj()
    {
        var dict = new Dictionary<NxValue, NxValue>();

        foreach (var value in this.SeqValue)
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