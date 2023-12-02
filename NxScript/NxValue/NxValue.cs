namespace NxScript;

public abstract class NxValue
{
    public virtual bool IsString => false;
    public virtual bool IsNumber => false;
    public virtual bool IsBool => false;
    public virtual bool IsSeq => false;
    public virtual bool IsObj => false;
    public virtual bool IsFn => false;
    public virtual bool IsNil => false;

    public abstract NxValueType Type { get; }

    public NxValue() { }

    public abstract dynamic GetInternalValue();

    ///
    /// Type Conversions
    /// 
    public abstract float AsNumber();
    public abstract string AsString();
    public abstract bool AsBool();
    public abstract List<NxValue> AsSeq();
    public abstract Dictionary<NxValue, NxValue> AsObj();
    public abstract Func<List<NxValue>, NxValue> AsFn();

    ///
    /// Operations
    ///
    public static NxValue Index(NxValue source, NxValue indexValue)
    {
        if (source.IsSeq)
        {
            var index = (int)indexValue.AsNumber();
            var seq = source.AsSeq();

            if (index < 0 || index >= seq.Count)
            {
                return new NxValueNil();
            }

            return seq[index];
        }

        if (source.IsObj)
        {
            source.AsObj().TryGetValue(indexValue, out var result);
            return result ?? new NxValueNil();
        }

#if STRICT_MODE
        throw new NotSupportedException("Auto-conversion to array for indexing is not allowed in strict mode.");
#endif

        return NxValue.Index(new NxValueSeq(source.AsSeq()), indexValue);
    }

    public static NxValue Member(NxValue source, NxValue keyValue)
    {
        var obj = source.AsObj();

        obj.TryGetValue(keyValue, out var result);

        return result ?? new NxValueNil();
    }

    public static NxValue Add(NxValue left, NxValue right)
    {
        return left.Type switch
        {
            NxValueType.Number => new NxValueNumber(left.AsNumber() + right.AsNumber()), // [Not-Mut]
            NxValueType.Bool => new NxValueBool(left.AsBool() || right.AsBool()), // [Not-Mut] (Bool OR)

            NxValueType.String => new NxValueString(left.AsString() + right.AsString()), // [Not-Mut]
            NxValueType.Seq => new NxValueSeq(left.AsSeq().Concat(right.AsSeq()).ToList()), // [Not-Mut]
            NxValueType.Obj => new NxValueObj(left.AsObj().ToList().Concat(right.AsObj().ToList())), // [Not-Mut]
#if STRICT_MODE
            _ => throw new NotSupportedException("Cannot add values of type " + left.Type + " and " + right.Type + ".")
#else
            _ => new NxValueNumber(left.AsNumber() * right.AsNumber()) // [Not-Mut]
#endif
        };
    }

    public static NxValueBool Eq(NxValue left, NxValue right)
    {
        return left.Type switch
        {
            NxValueType.Number => new NxValueBool(left.AsNumber() == right.AsNumber()), // [By-Val]
            NxValueType.Bool => new NxValueBool(left.AsBool() == right.AsBool()), // [By-Val]
            NxValueType.String => new NxValueBool(left.AsString() == right.AsString()), // [By-Val]
            NxValueType.Seq or
            NxValueType.Obj or
            NxValueType.Fn => new NxValueBool(left == right), // [By-Ref]
#if STRICT_MODE
            _ => throw new NotSupportedException("Cannot compare values of type " + left.Type + " and " + right.Type + ".")
#else
            _ => new NxValueBool(left.AsNumber() == right.AsNumber()) // [By-Val]
#endif
        };
    }

    public static NxValueBool Neq(NxValue left, NxValue right)
    {
        return left.Type switch
        {
            NxValueType.Number => new NxValueBool(left.AsNumber() != right.AsNumber()), // [By-Val]
            NxValueType.Bool => new NxValueBool(left.AsBool() != right.AsBool()), // [By-Val]
            NxValueType.String => new NxValueBool(left.AsString() != right.AsString()), // [By-Val]
            NxValueType.Seq or
            NxValueType.Obj or
            NxValueType.Fn => new NxValueBool(left != right), // [By-Ref]
#if STRICT_MODE
            _ => throw new NotSupportedException("Cannot compare values of type " + left.Type + " and " + right.Type + ".")
#else
            _ => new NxValueBool(left.AsNumber() != right.AsNumber()) // [By-Val]
#endif
        };
    }

    public static NxValue Multiply(NxValue left, NxValue right)
    {
        return left.Type switch
        {
            NxValueType.Number => new NxValueNumber(left.AsNumber() * right.AsNumber()), // [Not-Mut]
            NxValueType.Bool => new NxValueBool(left.AsBool() && right.AsBool()), // [Not-Mut] (Bool AND)
            _ => new NxValueNumber(left.AsNumber() * right.AsNumber()) // [Not-Mut]
        };
    }

    public static NxValue CopyUnknown(NxValue value)
    {
        return value.Type switch
        {
            NxValueType.Number => new NxValueNumber(value.AsNumber()),
            NxValueType.Bool => new NxValueBool(value.AsBool()),
            NxValueType.String => new NxValueString(value.AsString()),
            NxValueType.Seq => new NxValueSeq(value.AsSeq()),
            NxValueType.Obj => new NxValueObj(value.AsObj()),
            NxValueType.Fn => new NxValueFn(value.AsFn()),
            _ => new NxValueNil()
        };
    }

    public static NxValue AssignInternalRTLFromLType(NxValue left, NxValue right)
    {
        if (left is NxValueNumber numVal)
        {
            numVal.numberValue = right.AsNumber();
            return left;
        }

        if (left is NxValueBool boolVal)
        {
            boolVal.boolValue = right.AsBool();
            return left;
        }

        if (left is NxValueString strVal)
        {
            strVal.stringValue = right.AsString();
            return left;
        }

        if (left is NxValueSeq seqVal)
        {
            seqVal.seqValue = right.AsSeq();
            return left;
        }

        if (left is NxValueObj objVal)
        {
            objVal.objValue = right.AsObj();
            return left;
        }

        if (left is NxValueFn fnVal)
        {
            fnVal.fnValue = right.AsFn();
            return left;
        }

        throw new NotSupportedException("Cannot assign to value of type " + left.Type + ".");
    }
}