//#define STRICT_MODE
//#define DEBUG_LOG

using System.Globalization;

namespace NxScript;

public partial class NxValue
{
    // TODO: Use this everywhere where new NxValue() is used. Lock it, too.
    public static readonly NxValue Nil = new();

    private readonly float? numberValue = null;
    private readonly string? stringValue = null;
    private readonly bool? booleanValue = null;
    private readonly List<NxValue>? arrayValue = null;
    private readonly Dictionary<NxValue, NxValue>? objValue = null;
    private readonly Func<List<NxValue>, NxValue>? fnValue = null;

    public bool IsString => this.stringValue is not null;
    public bool IsNumber => this.numberValue is not null;
    public bool IsBoolean => this.booleanValue is not null;
    public bool IsArray => this.arrayValue is not null;
    public bool IsObj => this.objValue is not null;
    public bool IsFn => this.fnValue is not null;
    public bool IsNil => !this.IsBoolean && !this.IsNumber && !this.IsString && !this.IsArray && !this.IsObj && !this.IsFn;

    public NxValueType Type => this.stringValue is not null ? NxValueType.String :
        this.numberValue is not null ? NxValueType.Number :
        this.booleanValue is not null ? NxValueType.Boolean :
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

    // Boolean
    public NxValue(bool value)
    {
        this.booleanValue = value;
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
            NxValueType.Fn => this.fnValue,
            NxValueType.Nil => null,
            _ => throw new NotSupportedException($"Don't know type {Enum.GetName(this.Type)}")
        };
    }
}