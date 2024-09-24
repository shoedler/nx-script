using System.Globalization;

namespace NxScript;

public class NxValueString : NxValue
{
    public string StringValue;
    public override bool IsString => true;
    public override NxValueType Type => NxValueType.String;

    public NxValueString(string value)
    {
        this.StringValue = value;
    }

    public NxValueString(NxParser.StringAtomContext context)
    {
        if (context.STRING() is not null)
        {
            this.StringValue = context.STRING().GetText().Trim('"');
        }
        else
        {
            throw NxEvalException.FromContext("Don't know how to build NxValue", context);
        }
    }

    public override int GetHashCode() => this.StringValue!.GetHashCode();

    public override bool Equals(object? obj)
    {
        if (obj == null || this.GetType() != obj.GetType())
        {
            return false;
        }

        if (obj is NxValueString other)
        {
            return this.StringValue == other.StringValue;
        }


        return false;
    }

    public override dynamic GetInternalValue() => this.StringValue;

    public override float AsNumber()
    {
        float.TryParse(this.StringValue, CultureInfo.InvariantCulture, out var ret);
        return ret;
    }

    public override string AsString() => this.StringValue;

    public override bool AsBool()
    {
        return this.StringValue is not "false" && this.StringValue.Length > 0;
    }

    public override List<NxValue> AsSeq()
    {
        var ret = new List<NxValue>();

        foreach (var c in this.StringValue)
        {
            ret.Add(new NxValueString(c.ToString()));
        }

        return ret;
    }

    public override Dictionary<NxValue, NxValue> AsObj() => new()
    {
        { this, new NxValueString(this.StringValue) }
    };

    public override Func<List<NxValue>, NxValue> AsFn() => _ => this;
}