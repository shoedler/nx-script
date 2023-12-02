namespace NxScript;

public partial class NxValueObj : NxValue
{
    public Dictionary<NxValue, NxValue> objValue;
    public override bool IsObj => true;
    public override NxValueType Type => NxValueType.Obj;

    public NxValueObj(IEnumerable<(NxValue, NxValue)> value)
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

    public NxValueObj(IEnumerable<KeyValuePair<NxValue, NxValue>> value)
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

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return this == obj;
    }

    public override dynamic GetInternalValue() => this.objValue;

    public override float AsNumber() => this.objValue.Count;

    public override string AsString()
    {
        var items = this.objValue.Select((pair) => $"{pair.Key.AsString()}: {pair.Value.AsString()}");
        var objString = "{" + string.Join(", ", items) + "}";
        return objString.Length > 100 ? "[Object]" : objString;
    }

    public override bool AsBool() => true;

    public override List<NxValue> AsSeq()
    {
        return this.objValue.Select(x =>
        {
            var pairList = new List<NxValue>()
            {
                x.Key,
                x.Value,
            };

            return new NxValueSeq(pairList) as NxValue;
        }).ToList();
    }

    public override Dictionary<NxValue, NxValue> AsObj() => this.objValue;

    public override Func<List<NxValue>, NxValue> AsFn() => new Func<List<NxValue>, NxValue>((List<NxValue> Args) => this);
}