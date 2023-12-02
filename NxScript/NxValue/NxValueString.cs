using System.Globalization;

namespace NxScript;

public partial class NxValueString : NxValue
{
    public string stringValue;
    public override bool IsString => true;
    public override NxValueType Type => NxValueType.String;

    public NxValueString(string value)
    {
        this.stringValue = value;
    }

    public NxValueString(NxParser.StringAtomContext context)
    {
        if (context.STRING() is not null)
        {
            this.stringValue = context.STRING().GetText().Trim('"');
        }
        else
        {
            throw NxEvalException.FromContext("Don't know how to build NxValue", context);
        }
    }

    public override int GetHashCode() => this.stringValue!.GetHashCode();

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        if (obj is NxValueString other)
        {
            return this.stringValue == other.stringValue;
        }


        return false;
    }

    public override dynamic GetInternalValue() => this.stringValue;

    public override float AsNumber()
    {
        float.TryParse(this.stringValue, CultureInfo.InvariantCulture, out var ret);
        return ret;
    }

    public override string AsString() => this.stringValue;

    public override bool AsBool()
    {
        return this.stringValue is "false" ? false :
            this.stringValue.Length > 0;
    }

    public override List<NxValue> AsSeq()
    {
        var ret = new List<NxValue>();

        foreach (var c in this.stringValue)
        {
            ret.Add(new NxValueString(c.ToString()));
        }

        return ret;
    }

    public override Dictionary<NxValue, NxValue> AsObj() => new Dictionary<NxValue, NxValue>
        {
            { this, new NxValueString(this.stringValue) }
        };

    public override Func<List<NxValue>, NxValue> AsFn() => new Func<List<NxValue>, NxValue>((List<NxValue> Args) => this);
}