namespace NxScript;

public partial class NxValueFn : NxValue
{
    public Func<List<NxValue>, NxValue> fnValue;
    public override bool IsFn => true;
    public override NxValueType Type => NxValueType.Fn;

    public NxValueFn(Func<List<NxValue>, NxValue> value)
    {
        this.fnValue = value;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return this == obj;
    }

    public override dynamic GetInternalValue() => this.fnValue;

    public override float AsNumber() => 0f; // TODO: Move to Constants

    public override string AsString() => "[Function]"; // TODO: Move to Constants

    public override bool AsBool() => true; // TODO: Move to Constants

    public override List<NxValue> AsSeq() => new List<NxValue> { new NxValueFn(this.fnValue) };

    public override Dictionary<NxValue, NxValue> AsObj() => new Dictionary<NxValue, NxValue>
        {
            { this, new NxValueFn(this.fnValue) }
        };

    public override Func<List<NxValue>, NxValue> AsFn() => this.fnValue;
}