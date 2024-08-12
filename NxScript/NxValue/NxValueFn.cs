namespace NxScript;

public class NxValueFn : NxValue
{
    public Func<List<NxValue>, NxValue> FnValue;
    public override bool IsFn => true;
    public override NxValueType Type => NxValueType.Fn;

    public NxValueFn(Func<List<NxValue>, NxValue> value)
    {
        this.FnValue = value;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return this == obj;
    }

    public override dynamic GetInternalValue() => this.FnValue;

    public override float AsNumber() => 0f; // TODO: Move to Constants

    public override string AsString() => "[Function]"; // TODO: Move to Constants

    public override bool AsBool() => true; // TODO: Move to Constants

    public override List<NxValue> AsSeq() => new List<NxValue> { new NxValueFn(this.FnValue) };

    public override Dictionary<NxValue, NxValue> AsObj() => new Dictionary<NxValue, NxValue>
        {
            { this, new NxValueFn(this.FnValue) }
        };

    public override Func<List<NxValue>, NxValue> AsFn() => this.FnValue;
}