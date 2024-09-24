namespace NxScript;

public class NxValueSeq(List<NxValue> value) : NxValue
{
    public List<NxValue> SeqValue = value;
    public override bool IsSeq => true;
    public override NxValueType Type => NxValueType.Seq;

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
            var valueCopy = CopyUnknown(value);
            if (!dict.TryAdd(value, valueCopy))
            {
                dict[value] = valueCopy;
            }
        }

        return dict;
    }

    public override Func<List<NxValue>, NxValue> AsFn() => _ => this;
    
}