//#define STRICT_MODE
//#define DEBUG_LOG

using System.Globalization;

namespace NxScript;

public class NxValue
{
    // TODO: Use this everywhere where new NxValue() is used. Lock it, too.
    public static readonly NxValue Nil = new();

    private readonly float? numberValue = null;
    private readonly string? stringValue = null;
    private readonly bool? booleanValue = null;
    private readonly List<NxValue>? arrayValue = null;
    private readonly Dictionary<NxValue, NxValue>? objValue = null;

    public bool IsString => this.stringValue is not null;
    public bool IsNumber => this.numberValue is not null;
    public bool IsBoolean => this.booleanValue is not null;
    public bool IsArray => this.arrayValue is not null;
    public bool IsObj => this.objValue is not null;
    public bool IsNil => !this.IsBoolean && !this.IsNumber && !this.IsString && !this.IsArray && !this.IsObj;

    public NxValueType Type => this.stringValue is not null ? NxValueType.String :
        this.numberValue is not null ? NxValueType.Number :
        this.booleanValue is not null ? NxValueType.Boolean :
        this.arrayValue is not null ? NxValueType.Array :
        this.objValue is not null ? NxValueType.Obj :
        NxValueType.Nil;

    ///
    /// Pure Constructors
    /// 
    public NxValue() { }
    public NxValue(float value)
    {
        this.numberValue = value;
    }

    public NxValue(string value)
    {
        this.stringValue = value;
    }

    public NxValue(bool value)
    {
        this.booleanValue = value;
    }

    public NxValue(List<NxValue> value)
    {
        this.arrayValue = value;
    }

    public NxValue(IEnumerable<(NxValue, NxValue)> value)
    {

        this.objValue = new();

        foreach (var (key, val) in value)
        {
#if DEBUG_LOG
            Console.WriteLine($"Adding '{key.AsString()}' with Hashcode {key.GetHashCode()}");
#endif
            if (!this.objValue.TryAdd(key, val))
            {
                this.objValue[key] = val;
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

    public NxValue(NxParser.StringAtomContext context)
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

    public NxValue(NxParser.BooleanAtomContext context)
    {
        if (context.TRUE() is not null)
        {
            this.booleanValue = true;
        }
        else if (context.FALSE() is not null)
        {
            this.booleanValue = false;
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
            return this.numberValue.GetHashCode();
        }

        if (this.IsBoolean)
        {
            return this.booleanValue.GetHashCode();
        }

        if (this.IsString)
        {
            return this.stringValue!.GetHashCode();
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
            return this.numberValue!.GetHashCode() == other.numberValue!.GetHashCode();
        }

        if (this.IsBoolean && other.IsBoolean)
        {
            return this.booleanValue == other.booleanValue;
        }

        if (this.IsString && other.IsString)
        {
            return this.stringValue == other.stringValue;
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
        // TODO: Try cast from current type first
        return this.numberValue ??
               this.StringToNumber() ??
               this.BooleanToNumber() ??
               this.ArrayToNumber() ??
               this.ObjToNumber() ??
               0f;
    }

    public string AsString()
    {
        // TODO: Try cast from current type first
        return this.stringValue ??
               this.NumberToString() ??
               this.BooleanToString() ??
               this.ArrayToString() ??
               this.ObjToString() ??
               string.Empty;
    }

    public bool AsBoolean()
    {
        // TODO: Try cast from current type first
        return this.booleanValue ??
               this.NumberToBoolean() ??
               this.StringToBoolean() ??
               this.ArrayToBoolean() ??
               this.ObjToBoolean() ??
               false;
    }

    public List<NxValue> AsArray()
    {
        // TODO: Try cast from current type first
        return this.arrayValue ??
               this.StringToArray() ??
               this.NumberToArray() ??
               this.BooleanToArray() ??
               this.ObjToArray() ??
               new List<NxValue>();
    }

    public Dictionary<NxValue, NxValue> AsObj()
    {
        // TODO: Try cast from current type first
        return this.objValue ??
               this.StringToObj() ??
               this.NumberToObj() ??
               this.BooleanToObj() ??
               this.ArrayToObj() ??
               new Dictionary<NxValue, NxValue>();
    }

    ///
    /// Operations
    ///
    public NxValue Index(NxValue indexValue)
    {
        if (this.IsArray)
        {
            var index = (int)indexValue.AsNumber();

            if (index < 0 || index >= this.arrayValue!.Count)
            {
                return new NxValue();
            }

            return this.arrayValue[index];
        }

        if (this.IsObj)
        {
            this.objValue!.TryGetValue(indexValue, out var result);
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
    /// Testing
    ///
    public dynamic? GetInternalValue(NxValueType type)
    {
        return this.Type switch
        {
            NxValueType.Boolean => this.booleanValue,
            NxValueType.Number => this.numberValue,
            NxValueType.String => this.stringValue,
            NxValueType.Array => this.arrayValue,
            NxValueType.Obj => this.objValue,
            NxValueType.Nil => null,
            _ => throw new NotSupportedException($"Don't know type {Enum.GetName(this.Type)}")
        };
    }

    ///
    /// To Number Casts
    /// 
    private float? StringToNumber()
    {
        if (this.stringValue is null)
        {
            return null;
        }

        float.TryParse(this.stringValue, CultureInfo.InvariantCulture, out var ret);
        return ret;
    }

    private float? BooleanToNumber()
    {
        if (this.booleanValue is null)
        {
            return null;
        }

        return (bool)this.booleanValue ? 1f : 0f;
    }

    private float? ArrayToNumber()
    {
        if (this.arrayValue is null)
        {
            return null;
        }

#if STRICT_MODE
        throw new NotSupportedException("Cannot convert array to number in strict mode");
#endif

        return this.arrayValue.Count;
    }

    private float? ObjToNumber()
    {
        if (this.objValue is null)
        {
            return null;
        }

#if STRICT_MODE
        throw new NotSupportedException("Cannot convert obj to number in strict mode");
#endif

        return this.objValue.Count;
    }

    ///
    /// To String Casts
    /// 
    private string? NumberToString()
    {
        if (this.numberValue is null)
        {
            return null;
        }

        return this.numberValue.ToString();
    }

    private string? BooleanToString()
    {
        if (this.booleanValue is null)
        {
            return null;
        }

        return (bool)this.booleanValue ? "true" : "false";
    }

    private string? ArrayToString()
    {
        if (this.arrayValue is null)
        {
            return null;
        }

        var items = this.arrayValue.Select(item => item.AsString());
        return "[" + string.Join(", ", items) + "]";
    }

    private string? ObjToString()
    {
        if (this.objValue is null)
        {
            return null;
        }

        var items = this.objValue.Select((kvp) => $"{kvp.Key.AsString()}: {kvp.Value.AsString()}");
        return "{" + string.Join(", ", items) + "}";
    }

    ///
    /// To Boolean Casts
    /// 
    private bool? StringToBoolean()
    {
        if (this.stringValue is null)
        {
            return null;
        }

        return this.stringValue is "true" ? true :
            this.stringValue is "false" ? false :
            this.stringValue.Length > 0;
    }

    private bool? NumberToBoolean()
    {
        if (this.numberValue is null)
        {
            return null;
        }

        return this.numberValue is not 0;
    }

    private bool? ArrayToBoolean()
    {
        if (this.arrayValue is null)
        {
            return null;
        }

        return true;
    }

    private bool? ObjToBoolean()
    {
        if (this.objValue is null)
        {
            return null;
        }

        return true;
    }

    ///
    /// To Array Casts
    /// 
    private List<NxValue>? StringToArray()
    {
        if (this.stringValue is null)
        {
            return null;
        }

        var ret = new List<NxValue>();

        foreach (var c in this.stringValue)
        {
            ret.Add(new NxValue(c.ToString()));
        }

        return ret;
    }

    private List<NxValue>? NumberToArray()
    {
        if (this.numberValue is null)
        {
            return null;
        }

        var ret = new List<NxValue>();

        for (var i = 0; i < this.numberValue; i++)
        {
            ret.Add(new NxValue(i));
        }

        return ret;
    }

    private List<NxValue>? BooleanToArray()
    {
        if (this.booleanValue is null)
        {
            return null;
        }

        return new List<NxValue> { new((bool)this.booleanValue) };
    }

    private List<NxValue>? ObjToArray()
    {
        if (this.objValue is null)
        {
            return null;
        }

        return this.objValue.Select(x =>
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
    /// To Obj Casts
    ///
    private Dictionary<NxValue, NxValue>? StringToObj()
    {
        if (this.stringValue is null)
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
        if (this.numberValue is null)
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
        if (this.booleanValue is null)
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
        if (this.arrayValue is null)
        {
            return null;
        }

        return this.arrayValue.ToDictionary(value => value, value => value);
    }
}