namespace NxScript;

public partial class NxValueNumber : NxValue
{
    public float numberValue;
    public override bool IsNumber => true;
    public override NxValueType Type => NxValueType.Number;

    public NxValueNumber(float value)
    {
        this.numberValue = value;
    }

    public NxValueNumber(NxParser.NumberAtomContext context)
    {
        if (context.FLOAT() is not null)
        {
            this.numberValue = float.Parse(context.FLOAT().GetText());
        }
        else if (context.INT() is not null)
        {
            this.numberValue = int.Parse(context.INT().GetText());
        }
        else
        {
            throw NxEvalException.FromContext("Don't know how to build NxValue", context);
        }
    }

    public override int GetHashCode() => this.numberValue.GetHashCode();

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        if (obj is NxValueNumber other)
        {
            return this.numberValue.GetHashCode() == other.numberValue.GetHashCode();
        }

        return false;
    }

    public override dynamic GetInternalValue() => this.numberValue;

    public override float AsNumber() => this.numberValue;

    public override string AsString() => this.numberValue.ToString();

    public override bool AsBool() => this.numberValue != 0;

    public override List<NxValue> AsSeq() => new List<NxValue> { new NxValueNumber(this.numberValue) };

    public override Dictionary<NxValue, NxValue> AsObj() => new Dictionary<NxValue, NxValue>
        {
            { this, new NxValueNumber(this.numberValue) }
        };

    public override Func<List<NxValue>, NxValue> AsFn() => new Func<List<NxValue>, NxValue>((List<NxValue> Args) => this);
}