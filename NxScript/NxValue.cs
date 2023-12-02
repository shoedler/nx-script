//#define STRICT_MODE
//#define DEBUG_LOG

using System.Globalization;

namespace NxScript;

public partial class NxValue
{
    // TODO: Use this everywhere where new NxValue() is used. Lock it, too.
    public static readonly NxValue Nil = new();

    private float? numberValue = null;
    private string? stringValue = null;
    private bool? boolValue = null;
    private List<NxValue>? arrayValue = null;
    private Dictionary<NxValue, NxValue>? objValue = null;
    private Func<List<NxValue>, NxValue>? fnValue = null;

    public bool IsString => this.stringValue is not null;
    public bool IsNumber => this.numberValue is not null;
    public bool IsBool => this.boolValue is not null;
    public bool IsArray => this.arrayValue is not null;
    public bool IsObj => this.objValue is not null;
    public bool IsFn => this.fnValue is not null;
    public bool IsNil => !this.IsBool && !this.IsNumber && !this.IsString && !this.IsArray && !this.IsObj && !this.IsFn;

    public NxValueType Type => this.stringValue is not null ? NxValueType.String :
        this.numberValue is not null ? NxValueType.Number :
        this.boolValue is not null ? NxValueType.Bool :
        this.arrayValue is not null ? NxValueType.Array :
        this.objValue is not null ? NxValueType.Obj :
        this.fnValue is not null ? NxValueType.Fn :
        NxValueType.Nil;

    ///
    /// Pure Constructors
    /// 

    // Nil
    public NxValue() { }

    // Number
    public NxValue(float value)
    {
        this.numberValue = value;
    }

    // String
    public NxValue(string value)
    {
        this.stringValue = value;
    }

    // Bool
    public NxValue(bool value)
    {
        this.boolValue = value;
    }

    // Array
    public NxValue(List<NxValue> value)
    {
        this.arrayValue = value;
    }

    // Obj
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

    // Obj (From Dictionary)
    public NxValue(IEnumerable<KeyValuePair<NxValue, NxValue>> value)
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

    // Fn
    public NxValue(Func<List<NxValue>, NxValue> value)
    {
        this.fnValue = value;
    }

    // From NxValue (Referenceless Copy)
    public NxValue(NxValue old)
    {
        NxValue.AssignInternalRTLFromRType(this, old);
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

    public NxValue(NxParser.BoolAtomContext context)
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

    public override int GetHashCode()
    {
        if (this.IsNumber)
        {
            return this.numberValue.GetHashCode();
        }

        if (this.IsBool)
        {
            return this.boolValue.GetHashCode();
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

        if (this.IsBool && other.IsBool)
        {
            return this.boolValue == other.boolValue;
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
    /// Testing
    ///
    public dynamic? GetInternalValue(NxValueType type)
    {
        return this.Type switch
        {
            NxValueType.Bool => this.boolValue,
            NxValueType.Number => this.numberValue,
            NxValueType.String => this.stringValue,
            NxValueType.Array => this.arrayValue,
            NxValueType.Obj => this.objValue,
            NxValueType.Fn => this.fnValue,
            NxValueType.Nil => null,
            _ => throw new NotSupportedException($"Don't know type {Enum.GetName(this.Type)}")
        };
    }
}