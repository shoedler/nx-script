namespace NxScript;

public partial class NxValueBool : NxValue
{
    public bool boolValue = false;
    public override bool IsBool => true;
    public override NxValueType Type => NxValueType.Bool;

    public NxValueBool(bool value)
    {
        this.boolValue = value;
    }

    public NxValueBool(NxParser.BoolAtomContext context)
    {
        if (context.TRUE() is not null)
        {
            this.boolValue = true;
        }
        else if (context.FALSE() is not null)
        {
            this.boolValue = false;
        }
        else
        {
            throw NxEvalException.FromContext("Don't know how to build NxValue", context);
        }
    }

    public override int GetHashCode() => this.boolValue.GetHashCode();
    
    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        if (obj is NxValueBool other)
        {
            return this.boolValue == other.boolValue;
        }

        return false;
    }

    public override dynamic GetInternalValue() => this.boolValue;

    public override float AsNumber() => this.boolValue ? 1f : 0f;

    public override string AsString() => this.boolValue ? "true" : "false";

    public override bool AsBool() => this.boolValue;

    public override List<NxValue> AsSeq() => new List<NxValue> { new NxValueBool(this.boolValue) };

    public override Dictionary<NxValue, NxValue> AsObj() => new Dictionary<NxValue, NxValue>
        {
            { this, new NxValueBool(this.boolValue) }
        };

    public override Func<List<NxValue>, NxValue> AsFn() => new Func<List<NxValue>, NxValue>((List<NxValue> Args) => this);
}