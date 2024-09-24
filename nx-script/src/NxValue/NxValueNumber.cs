namespace NxScript;

public class NxValueNumber : NxValue
{
    public float NumberValue;
    public override bool IsNumber => true;
    public override NxValueType Type => NxValueType.Number;

    public NxValueNumber(float value)
    {
        this.NumberValue = value;
    }

    public NxValueNumber(NxParser.NumberAtomContext context)
    {
        if (context.FLOAT() is not null)
        {
            this.NumberValue = float.Parse(context.FLOAT().GetText());
        }
        else if (context.INT() is not null)
        {
            this.NumberValue = int.Parse(context.INT().GetText());
        }
        else
        {
            throw NxEvalException.FromContext("Don't know how to build NxValue", context);
        }
    }

    public override int GetHashCode() => this.NumberValue.GetHashCode();

    public override bool Equals(object? obj)
    {
        if (obj == null || this.GetType() != obj.GetType())
        {
            return false;
        }

        if (obj is NxValueNumber other)
        {
            return this.NumberValue.GetHashCode() == other.NumberValue.GetHashCode();
        }

        return false;
    }

    public override dynamic GetInternalValue() => this.NumberValue;

    public override float AsNumber() => this.NumberValue;

    public override string AsString() => this.NumberValue.ToString();

    public override bool AsBool() => this.NumberValue != 0;

    public override List<NxValue> AsSeq() => [new NxValueNumber(this.NumberValue)];

    public override Dictionary<NxValue, NxValue> AsObj() => new()
    {
        { this, new NxValueNumber(this.NumberValue) }
    };

    public override Func<List<NxValue>, NxValue> AsFn() => _ => this;
}