namespace NxScript;

public class NxValueObj : NxValue
{
    public Dictionary<NxValue, NxValue> ObjValue;
    public override bool IsObj => true;
    public override NxValueType Type => NxValueType.Obj;

    public NxValueObj(IEnumerable<(NxValue, NxValue)> value)
    {

        this.ObjValue = new();

        foreach (var (key, val) in value)
        {
#if DEBUG_LOG
            Console.WriteLine($"Adding '{key.AsString()}' with Hashcode {key.GetHashCode()}");
#endif
            if (!this.ObjValue.TryAdd(key, val))
            {
                this.ObjValue[key] = val;
            }
        }
    }

    public NxValueObj(IEnumerable<KeyValuePair<NxValue, NxValue>> value)
    {

        this.ObjValue = new();

        foreach (var (key, val) in value)
        {
#if DEBUG_LOG
            Console.WriteLine($"Adding '{key.AsString()}' with Hashcode {key.GetHashCode()}");
#endif
            if (!this.ObjValue.TryAdd(key, val))
            {
                this.ObjValue[key] = val;
            }
        }
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return this == obj;
    }

    public override dynamic GetInternalValue() => this.ObjValue;

    public override float AsNumber() => this.ObjValue.Count;

    public override string AsString()
    {
        var items = this.ObjValue.Select((pair) => $"{pair.Key.AsString()}: {pair.Value.AsString()}");
        var objString = "{" + string.Join(", ", items) + "}";
        return objString.Length > 100 ? "[Object]" : objString;
    }

    public override bool AsBool() => true;

    public override List<NxValue> AsSeq()
    {
        return this.ObjValue.Select(x =>
        {
            var pairList = new List<NxValue>()
            {
                x.Key,
                x.Value,
            };

            return new NxValueSeq(pairList) as NxValue;
        }).ToList();
    }

    public override Dictionary<NxValue, NxValue> AsObj() => this.ObjValue;

    public override Func<List<NxValue>, NxValue> AsFn() => new Func<List<NxValue>, NxValue>((List<NxValue> Args) => this);
}