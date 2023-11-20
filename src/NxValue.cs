using System.Globalization;

namespace NxScript;

internal class NxValue
{
    private const string NIL_STRING = "nil";
    private const bool NIL_BOOLEAN = false;
    private const float NIL_NUMBER = float.NaN;

    private readonly float? _numberValue = null;
    private readonly string? _stringValue = null;
    private readonly bool? _booleanValue = null;

    public bool IsString => this._stringValue is not null;
    public bool IsNumber => this._numberValue is not null;
    public bool IsBoolean => this._booleanValue is not null;

    public NxValue() { }
    public NxValue(float value)
    {
        this._numberValue = value;
    }

    public NxValue(string value)
    {
        this._stringValue = value;
    }

    public NxValue(bool value)
    {
        this._booleanValue = value;
    }

    public NxValue(NxParser.NumberAtomContext context)
    {
        if (context.FLOAT() is not null)
            this._numberValue = float.Parse(context.FLOAT().GetText());
        else if (context.INT() is not null)
            this._numberValue = int.Parse(context.INT().GetText());
        else
            throw NxEvalException.FromContext("Don't know how to build NxValue", context);
    }

    public NxValue(NxParser.StringAtomContext context)
    {
        if (context.STRING() is not null)
            this._stringValue = context.STRING().GetText().Trim('"');
        else
            throw NxEvalException.FromContext("Don't know how to build NxValue", context);
    }

    public NxValue(NxParser.BooleanAtomContext context)
    {
        if (context.TRUE() is not null)
            this._booleanValue = true;
        else if (context.FALSE() is not null)
            this._booleanValue = false;
        else
            throw NxEvalException.FromContext("Don't know how to build NxValue", context);
    }

    public float AsNumber()
    {
        return this._numberValue ??
               this.StringToNumber() ??
               this.BooleanToNumber() ??
               NIL_NUMBER;
    }

    public string AsString()
    {
        return this._stringValue ??
               this.NumberToString() ??
               this.BooleanToString() ??
               NIL_STRING;
    }

    public bool AsBoolean()
    {
        return this._booleanValue ??
               this.NumberToBoolean() ??
               this.StringToBoolean() ??
               NIL_BOOLEAN;
    }

    private float? StringToNumber()
    {
        if (this._stringValue is null)
            return null;

        var ret = NIL_NUMBER;
        float.TryParse(this._stringValue, CultureInfo.InvariantCulture, out ret);
        return ret;
    }

    private float? BooleanToNumber()
    {
        if (this._booleanValue is null)
            return null;

        return (bool)this._booleanValue ? 1f : 0f;
    }

    private string? NumberToString()
    {
        if (this._numberValue is null)
            return null;

        return this._numberValue.ToString();
    }

    private string? BooleanToString()
    {
        if (this._booleanValue is null)
            return null;

        return (bool)this._booleanValue ? "true" : "false";
    }

    private bool? StringToBoolean()
    {
        if (this._stringValue is null)
            return null;

        return this._stringValue is "true" ? true :
            this._stringValue is "false" ? false :
            this._stringValue.Length > 0;
    }

    private bool? NumberToBoolean()
    {
        if (this._numberValue is null)
            return null;

        return this._numberValue is not 0;
    }
}