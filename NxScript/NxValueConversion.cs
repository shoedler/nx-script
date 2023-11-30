//#define STRICT_MODE
//#define DEBUG_LOG

using System.Globalization;

namespace NxScript;

public partial class NxValue
{
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
    /// To Number conversions
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
    /// To String conversions
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

        var items = this.objValue.Select((pair) => $"{pair.Key.AsString()}: {pair.Value.AsString()}");
        return "{" + string.Join(", ", items) + "}";
    }

    ///
    /// To Boolean conversions
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
    /// To Array conversions
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

        return new List<NxValue> { new((float)this.numberValue) };
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
            var pairList = new List<NxValue>()
            {
                x.Key,
                x.Value,
            };

            return new NxValue(pairList);
        }).ToList();
    }

    ///
    /// To Obj conversions
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