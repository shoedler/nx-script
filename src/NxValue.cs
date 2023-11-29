#define STRICT_MODE
//#define DEBUG_LOG

using System.Globalization;

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
    private readonly Dictionary<NxValue, NxValue>? _objValue = null;

    public bool IsString => this._stringValue is not null;
    public bool IsNumber => this._numberValue is not null;
    public bool IsBoolean => this._booleanValue is not null;
    public bool IsArray => this._arrayValue is not null;
    public bool IsObj => this._objValue is not null;
    public bool IsNil => !this.IsBoolean && !this.IsNumber && !this.IsString && !this.IsArray && !this.IsObj;

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

    public NxValue(IEnumerable<(NxValue, NxValue)> value)
    {

        this._objValue = new();

        foreach (var (key, val) in value)
        {
#if DEBUG_LOG
            Console.WriteLine($"Adding '{key.AsString()}' with Hashcode {key.GetHashCode()}");
#endif
            if (!this._objValue.TryAdd(key, val))
            {
                this._objValue[key] = val;
            }
        }
    }

    ///
    /// Context Constructors
    /// 
    public NxValue(NxParser.NumberAtomContext context)
    {
        if (context.FLOAT() is not null)
        {
            this._numberValue = float.Parse(context.FLOAT().GetText());
        }
        else if (context.INT() is not null)
        {
            this._numberValue = int.Parse(context.INT().GetText());
        }
        else
        {
            throw NxEvalException.FromContext("Don't know how to build NxValue", context);
        }
    }

    public NxValue(NxParser.StringAtomContext context)
    {
        if (context.STRING() is not null)
        {
            this._stringValue = context.STRING().GetText().Trim('"');
        }
        else
        {
            throw NxEvalException.FromContext("Don't know how to build NxValue", context);
        }
    }

    public NxValue(NxParser.BooleanAtomContext context)
    {
        if (context.TRUE() is not null)
        {
            this._booleanValue = true;
        }
        else if (context.FALSE() is not null)
        {
            this._booleanValue = false;
        }
        else
        {
            throw NxEvalException.FromContext("Don't know how to build NxValue", context);
        }
    }

    public override int GetHashCode()
    {
        if (this.IsNumber)
        {
            return this._numberValue.GetHashCode();
        }

        if (this.IsBoolean)
        {
            return this._booleanValue.GetHashCode();
        }

        if (this.IsString)
        {
            return this._stringValue!.GetHashCode();
        }


        return base.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        var other = (NxValue)obj;

        if (this.IsNumber && other.IsNumber)
        {
            return this._numberValue!.GetHashCode() == other._numberValue!.GetHashCode();
        }

        if (this.IsBoolean && other.IsBoolean)
        {
            return this._booleanValue == other._booleanValue;
        }

        if (this.IsString && other.IsString)
        {
            return this._stringValue == other._stringValue;
        }

        if (this.IsNil && other.IsNil)
        {
            return true;
        }

        return false;
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
               this.ObjToNumber() ??
               NilNumber;
    }

    public string AsString()
    {
        return this._stringValue ??
               this.NumberToString() ??
               this.BooleanToString() ??
               this.ArrayToString() ??
               this.ObjToString() ??
               NilString;
    }

    public bool AsBoolean()
    {
        return this._booleanValue ??
               this.NumberToBoolean() ??
               this.StringToBoolean() ??
               this.ArrayToBoolean() ??
               this.ObjToBoolean() ??
               NilBoolean;
    }

    public List<NxValue> AsArray()
    {
        return this._arrayValue ??
               this.StringToArray() ??
               this.NumberToArray() ??
               this.BooleanToArray() ??
               this.ObjToArray() ??
               new List<NxValue>();
    }

    public Dictionary<NxValue, NxValue> AsObj()
    {
        return this._objValue ??
               this.StringToObj() ??
               this.NumberToObj() ??
               this.BooleanToObj() ??
               this.ArrayToObj() ??
               new Dictionary<NxValue, NxValue>();
    }

    ///
    /// Other
    ///
    public NxValue Index(NxValue indexValue)
    {
        if (this.IsArray)
        {
            var index = (int)indexValue.AsNumber();

            if (index < 0 || index >= this._arrayValue!.Count)
            {
                return new NxValue();
            }

            return this._arrayValue[index];
        }

        if (this.IsObj)
        {
            this._objValue!.TryGetValue(indexValue, out var result);
            return result ?? new NxValue();
        }

#if STRICT_MODE
        throw new NotSupportedException("Auto-conversion to array for indexing is not allowed in strict mode.");
#endif

        return this.Index(new NxValue(this.AsArray()));
    }

    public NxValue Member(NxValue keyValue)
    {
        var obj = this.AsObj();

        obj.TryGetValue(keyValue, out var result);

        return result ?? new NxValue();
    }

    ///
    /// To Number Castings
    /// 
    private float? StringToNumber()
    {
        if (this._stringValue is null)
        {
            return null;
        }

        float.TryParse(this._stringValue, CultureInfo.InvariantCulture, out var ret);
        return ret;
    }

    private float? BooleanToNumber()
    {
        if (this._booleanValue is null)
        {
            return null;
        }

        return (bool)this._booleanValue ? 1f : 0f;
    }

    private float? ArrayToNumber()
    {
        if (this._arrayValue is null)
        {
            return null;
        }

#if STRICT_MODE
        throw new NotSupportedException("Cannot convert array to number in strict mode");
#endif

        return this._arrayValue.Count;
    }

    private float? ObjToNumber()
    {
        if (this._objValue is null)
        {
            return null;
        }

#if STRICT_MODE
        throw new NotSupportedException("Cannot convert obj to number in strict mode");
#endif

        return this._objValue.Count;
    }

    ///
    /// To String Castings
    /// 
    private string? NumberToString()
    {
        if (this._numberValue is null)
        {
            return null;
        }

        return this._numberValue.ToString();
    }

    private string? BooleanToString()
    {
        if (this._booleanValue is null)
        {
            return null;
        }

        return (bool)this._booleanValue ? "true" : "false";
    }

    private string? ArrayToString()
    {
        if (this._arrayValue is null)
        {
            return null;
        }

        var items = this._arrayValue.Select(item => item.AsString());
        return "[" + string.Join(", ", items) + "]";
    }

    private string? ObjToString()
    {
        if (this._objValue is null)
        {
            return null;
        }

        var items = this._objValue.Select((kvp) => $"{kvp.Key.AsString()}: {kvp.Value.AsString()}");
        return "{\n  " + string.Join(",\n  ", items) + "\n}";
    }

    ///
    /// To Boolean Castings
    /// 
    private bool? StringToBoolean()
    {
        if (this._stringValue is null)
        {
            return null;
        }

        return this._stringValue is "true" ? true :
            this._stringValue is "false" ? false :
            this._stringValue.Length > 0;
    }

    private bool? NumberToBoolean()
    {
        if (this._numberValue is null)
        {
            return null;
        }

        return this._numberValue is not 0;
    }

    private bool? ArrayToBoolean()
    {
        if (this._arrayValue is null)
        {
            return null;
        }

        return true;
    }

    private bool? ObjToBoolean()
    {
        if (this._objValue is null)
        {
            return null;
        }

        return true;
    }

    ///
    /// To Array Castings
    /// 
    private List<NxValue>? StringToArray()
    {
        if (this._stringValue is null)
        {
            return null;
        }

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
        {
            return null;
        }

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
        {
            return null;
        }

        return new List<NxValue> { new((bool)this._booleanValue) };
    }

    private List<NxValue>? ObjToArray()
    {
        if (this._objValue is null)
        {
            return null;
        }

        return this._objValue.Select(x =>
        {
            var kvpList = new List<NxValue>()
            {
                x.Key,
                x.Value,
            };

            return new NxValue(kvpList);
        }).ToList();
    }

    ///
    /// To Obj Castings
    ///
    private Dictionary<NxValue, NxValue>? StringToObj()
    {
        if (this._stringValue is null)
        {
            return null;
        }

#if STRICT_MODE
        throw new NotSupportedException("Cannot convert string to obj in strict mode");
#endif

        return new Dictionary<NxValue, NxValue>
        {
            { this, this }
        };
    }

    private Dictionary<NxValue, NxValue>? NumberToObj()
    {
        if (this._numberValue is null)
        {
            return null;
        }

#if STRICT_MODE
        throw new NotSupportedException("Cannot convert number to obj in strict mode");
#endif

        return new Dictionary<NxValue, NxValue>
        {
            { this, this }
        };
    }


    private Dictionary<NxValue, NxValue>? BooleanToObj()
    {
        if (this._booleanValue is null)
        {
            return null;
        }

#if STRICT_MODE
        throw new NotSupportedException("Cannot convert boolean to obj in strict mode");
#endif

        return new Dictionary<NxValue, NxValue>
        {
            { this, this }
        };
    }


    private Dictionary<NxValue, NxValue>? ArrayToObj()
    {
        if (this._arrayValue is null)
        {
            return null;
        }

        return this._arrayValue.ToDictionary(value => value, value => value);
    }
}