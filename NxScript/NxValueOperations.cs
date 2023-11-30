//#define STRICT_MODE
//#define DEBUG_LOG

using System.Globalization;

namespace NxScript;

public partial class NxValue
{
    ///
    /// Operations
    ///
    public static NxValue Index(NxValue source, NxValue indexValue)
    {
        if (source.IsArray)
        {
            var index = (int)indexValue.AsNumber();

            if (index < 0 || index >= source.arrayValue!.Count)
            {
                return new NxValue();
            }

            return source.arrayValue[index];
        }

        if (source.IsObj)
        {
            source.objValue!.TryGetValue(indexValue, out var result);
            return result ?? new NxValue();
        }

#if STRICT_MODE
        throw new NotSupportedException("Auto-conversion to array for indexing is not allowed in strict mode.");
#endif

        return NxValue.Index(new NxValue(source.AsArray()), indexValue);
    }

    public static NxValue Member(NxValue source, NxValue keyValue)
    {
        var obj = source.AsObj();

        obj.TryGetValue(keyValue, out var result);

        return result ?? new NxValue();
    }

    public static NxValue Add(NxValue left, NxValue right)
    {
        return left.Type switch
        {
            NxValueType.Number => new NxValue(left.AsNumber() + right.AsNumber()), // [Not-Mut]
            NxValueType.Boolean => new NxValue(left.AsBoolean() || right.AsBoolean()), // [Not-Mut] (Boolean OR)

            NxValueType.String => new NxValue(left.AsString() + right.AsString()), // [Not-Mut]
            NxValueType.Array => new NxValue(left.AsArray().Concat(right.AsArray()).ToList()), // [Not-Mut]
            NxValueType.Obj => new NxValue(left.AsObj().ToList().Concat(right.AsObj().ToList())), // [Not-Mut]
#if STRICT_MODE
            _ => throw new NotSupportedException("Cannot add values of type " + left.Type + " and " + right.Type + ".")
#else
            _ => new NxValue(left.AsNumber() * right.AsNumber()) // [Not-Mut]
#endif
        };
    }

    public static NxValue Multiply(NxValue left, NxValue right)
    {
        return left.Type switch
        {
            NxValueType.Number => new NxValue(left.AsNumber() * right.AsNumber()), // [Not-Mut]
            NxValueType.Boolean => new NxValue(left.AsBoolean() && right.AsBoolean()), // [Not-Mut] (Boolean AND)
            _ => new NxValue(left.AsNumber() * right.AsNumber()) // [Not-Mut]
        };
    }
}