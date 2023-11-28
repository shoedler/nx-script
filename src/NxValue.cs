using System.Globalization;
using System.Text;

namespace NxScript;

internal class NxValue
{
    private const string NilString = "nil";
    private const bool NilBoolean = false;
    private const float NilNumber = float.NaN;

    private readonly float? _numberValue = null;
    private readonly string? _stringValue = null;
    private readonly bool? _booleanValue = null;
    private readonly List<NxValue>? _arrayValue = null;

    public bool IsString => this._stringValue is not null;
    public bool IsNumber => this._numberValue is not null;
    public bool IsBoolean => this._booleanValue is not null;
    public bool IsArray => this._arrayValue is not null;

    ///
    /// Pure Constructors
    /// 
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
    public NxValue(List<NxValue> value)
    {
        this._arrayValue = value;
    }

    ///
    /// Context Constructors
    /// 
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

    ///
    /// Type Conversions
    /// 
    public float AsNumber()
    {
        return this._numberValue ??
               this.StringToNumber() ??
               this.BooleanToNumber() ??
               this.ArrayToNumber() ??
               NilNumber;
    }
    public string AsString()
    {
        return this._stringValue ??
               this.NumberToString() ??
               this.BooleanToString() ??
                this.ArrayToString() ??
               NilString;
    }
    public bool AsBoolean()
    {
        return this._booleanValue ??
               this.NumberToBoolean() ??
               this.StringToBoolean() ??
                this.ArrayToBoolean() ??
               NilBoolean;
    }
    public List<NxValue> AsArray()
    {
        return this._arrayValue ??
               this.StringToArray() ??
               this.NumberToArray() ??
               this.BooleanToArray() ??
               new List<NxValue>();
    }

    ///
    /// Other
    ///
    public NxValue Index(NxValue indexValue)
    {
        var index = (int)indexValue.AsNumber();
        var array = this.AsArray();

        if (index < 0 || index >= array.Count)
        {
            return new NxValue();
        }

        return array[index];
    }

    ///
    /// To Number Castings
    /// 
    private float? StringToNumber()
    {
        if (this._stringValue is null)
            return null;

        var ret = NilNumber;
        float.TryParse(this._stringValue, CultureInfo.InvariantCulture, out ret);
        return ret;
    }

    private float? BooleanToNumber()
    {
        if (this._booleanValue is null)
            return null;

        return (bool)this._booleanValue ? 1f : 0f;
    }

    private float? ArrayToNumber()
    {
        if (this._arrayValue is null)
            return null;

        return this._arrayValue.Count;
    }

    ///
    /// To String Castings
    /// 
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

    private string? ArrayToString()
    {
        if (this._arrayValue is null)
            return null;

        var items = this._arrayValue.Select(item => item.AsString());
        return "[" + string.Join(", ", items) + "]";
    }

    ///
    /// To Boolean Castings
    /// 
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

    private bool? ArrayToBoolean()
    {
        if (this._arrayValue is null)
            return null;

        return true;
    }

    ///
    /// To Array Castings
    /// 
    private List<NxValue>? StringToArray()
    {
        if (this._stringValue is null)
            return null;

        var ret = new List<NxValue>();

        foreach (var c in this._stringValue)
        {
            ret.Add(new NxValue(c.ToString()));
        }

        return ret;
    }

    private List<NxValue>? NumberToArray()
    {
        if (this._numberValue is null)
            return null;

        var ret = new List<NxValue>();

        for (var i = 0; i < this._numberValue; i++)
        {
            ret.Add(new NxValue(i));
        }

        return ret;
    }

    private List<NxValue>? BooleanToArray()
    {
        if (this._booleanValue is null)
            return null;

        var ret = new List<NxValue>();

        ret.Add(new NxValue((bool)this._booleanValue));

        return ret;
    }
}