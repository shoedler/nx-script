namespace NxScript;

public class NxValueBool : NxValue
{
    public bool BoolValue = false;
    public override bool IsBool => true;
    public override NxValueType Type => NxValueType.Bool;

    public NxValueBool(bool value)
    {
        this.BoolValue = value;
    }

    public NxValueBool(NxParser.BoolAtomContext context)
    {
        if (context.TRUE() is not null)
        {
            this.BoolValue = true;
        }
        else if (context.FALSE() is not null)
        {
            this.BoolValue = false;
        }
        else
        {
            throw NxEvalException.FromContext("Don't know how to build NxValue", context);
        }
    }

    public override int GetHashCode() => this.BoolValue.GetHashCode();
    
    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        if (obj is NxValueBool other)
        {
            return this.BoolValue == other.BoolValue;
        }

        return false;
    }

    public override dynamic GetInternalValue() => this.BoolValue;

    public override float AsNumber() => this.BoolValue ? 1f : 0f;

    public override string AsString() => this.BoolValue ? "true" : "false";

    public override bool AsBool() => this.BoolValue;

    public override List<NxValue> AsSeq() => new List<NxValue> { new NxValueBool(this.BoolValue) };

    public override Dictionary<NxValue, NxValue> AsObj() => new Dictionary<NxValue, NxValue>
        {
            { this, new NxValueBool(this.BoolValue) }
        };

    public override Func<List<NxValue>, NxValue> AsFn() => new Func<List<NxValue>, NxValue>((List<NxValue> Args) => this);
}