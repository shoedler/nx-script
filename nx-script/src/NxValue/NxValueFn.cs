namespace NxScript;

public class NxValueFn(Func<List<NxValue>, NxValue> value) : NxValue
{
    public Func<List<NxValue>, NxValue> FnValue = value;
    public override bool IsFn => true;
    public override NxValueType Type => NxValueType.Fn;

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

    public override List<NxValue> AsSeq() => [new NxValueFn(this.FnValue)];

    public override Dictionary<NxValue, NxValue> AsObj() => new()
    {
        { this, new NxValueFn(this.FnValue) }
    };

    public override Func<List<NxValue>, NxValue> AsFn() => this.FnValue;
}