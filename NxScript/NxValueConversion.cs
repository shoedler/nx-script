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
        return this.numberValue ??
               this.StringToNumber() ??
               this.BoolToNumber() ??
               this.ArrayToNumber() ??
               this.ObjToNumber() ??
               this.FnToNumber() ??
               0f;
    }

    public string AsString()
    {
        return this.stringValue ??
               this.NumberToString() ??
               this.BoolToString() ??
               this.ArrayToString() ??
               this.ObjToString() ??
               this.FnToString() ??
               string.Empty;
    }

    public bool AsBool()
    {
        return this.booleanValue ??
               this.NumberToBool() ??
               this.StringToBool() ??
               this.ArrayToBool() ??
               this.ObjToBool() ??
               this.FnToBool() ??
               false;
    }

    public List<NxValue> AsArray()
    {
        return this.arrayValue ??
               this.StringToArray() ??
               this.NumberToArray() ??
               this.BoolToArray() ??
               this.ObjToArray() ??
               this.FnToArray() ??
               new List<NxValue>();
    }

    public Dictionary<NxValue, NxValue> AsObj()
    {
        return this.objValue ??
               this.StringToObj() ??
               this.NumberToObj() ??
               this.BoolToObj() ??
               this.ArrayToObj() ??
               this.FnToObj() ??
               new Dictionary<NxValue, NxValue>();
    }

    public Func<List<NxValue>, NxValue> AsFn()
    {
        // We can jsut return ourselves here. (args) => this
        // Then, in the visitor: Create a new Visitor, inject (somehow) the args as variables and then store 
        // set the func to () => newvisitor.visit(context)
        // Should work yo!
        if (this.fnValue is not null)
        {
            return this.fnValue;
        }

        return new Func<List<NxValue>, NxValue>((List<NxValue> Args) => this);
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

    private float? BoolToNumber()
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

    private float? FnToNumber()
    {
        if (this.fnValue is null)
        {
            return null;
        }

#if STRICT_MODE
        throw new NotSupportedException("Cannot convert fn to number in strict mode");
#endif

        return null; // Unconvertable
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

    private string? BoolToString()
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
        var objString = "{" + string.Join(", ", items) + "}";
        return objString.Length > 100 ? "[Object]" : objString;
    }

    private string? FnToString()
    {
        if (this.fnValue is null)
        {
            return null;
        }

        return "[Function]";
    }

    ///
    /// To Bool conversions
    ///
    private bool? StringToBool()
    {
        if (this.stringValue is null)
        {
            return null;
        }

        return this.stringValue is "true" ? true :
            this.stringValue is "false" ? false :
            this.stringValue.Length > 0;
    }

    private bool? NumberToBool()
    {
        if (this.numberValue is null)
        {
            return null;
        }

        return this.numberValue is not 0;
    }

    private bool? ArrayToBool()
    {
        if (this.arrayValue is null)
        {
            return null;
        }

        return true;
    }

    private bool? ObjToBool()
    {
        if (this.objValue is null)
        {
            return null;
        }

        return true;
    }

    private bool? FnToBool()
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

    private List<NxValue>? BoolToArray()
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

    private List<NxValue>? FnToArray()
    {
        if (this.fnValue is null)
        {
            return null;
        }

        return new List<NxValue> { new(this.fnValue) };
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


    private Dictionary<NxValue, NxValue>? BoolToObj()
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

#if STRICT_MODE
        throw new NotSupportedException("Cannot convert array to obj in strict mode");
#endif

        return this.arrayValue.ToDictionary(value => value, value => value);
    }

    private Dictionary<NxValue, NxValue>? FnToObj()
    {
        if (this.fnValue is null)
        {
            return null;
        }

#if STRICT_MODE
        throw new NotSupportedException("Cannot convert fn to obj in strict mode");
#endif

        return new Dictionary<NxValue, NxValue>
        {
            { this, this }
        };
    }
}