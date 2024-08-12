using NxScript;

namespace NxScriptTest;

public abstract class ToTypeTestsBase : IClassFixture<NxEvalFixture>
{
    protected NxEvalFixture Fixture;

    protected ToTypeTestsBase(NxEvalFixture fixture)
    {
        this.Fixture = fixture;
    }

    public NxValue TypeConversion_Works(string expression, dynamic expectedInternalResult, Type expectedInternalType, NxValueType expectedNxValueType)
    {
        // Arrange
        var program = $"let a = {expression}";

        // Act
        var result = this.Fixture.CleanRun(program);

        // Assert
        Assert.Empty(result.ParseErrors);

        var a = result.TryGetVariableFromAnyScope("a");
        Assert.NotNull(a);
        Assert.Equal(a.Type, expectedNxValueType);

        var aValue = a.GetInternalValue();
        Assert.NotNull(aValue);
        Assert.IsType(expectedInternalType, aValue);

        if (expectedInternalResult is List<NxValue> l) // Arrays
        {
            this.AssertByValueEquals(new NxValueSeq(l), a);
        }
        else if (expectedInternalResult is Dictionary<NxValue, NxValue> d) // Objs
        {
            this.AssertByValueEquals(new NxValueObj(d), a);
        }
        else
        {
            Assert.Equal(expectedInternalResult, aValue);
        }

        return a;
    }

    public void AssertByValueEquals(NxValue? expected, NxValue? actual)
    {
        Assert.NotNull(expected);
        Assert.NotNull(actual);

        if (expected.Type == NxValueType.Seq)
        {
            List<NxValue> expectedArray = expected.GetInternalValue()!;
            List<NxValue>? actualArray = actual.GetInternalValue();

            var length = Math.Max(expectedArray.Count, actualArray?.Count ?? 0);

            for (var i = 0; i < length; i++)
            {
                this.AssertByValueEquals(expectedArray[i], actualArray[i]);
            }

            return;
        }

        if (expected.Type == NxValueType.Obj)
        {
            Dictionary<NxValue, NxValue> expectedDict = expected.GetInternalValue()!;
            Dictionary<NxValue, NxValue>? actualDict = actual.GetInternalValue();

            var length = Math.Max(expectedDict.Count, actualDict?.Count ?? 0);

            for (var i = 0; i < length; i++)
            {
                var (expectedKey, expectedValue) = expectedDict.ElementAtOrDefault(i);
                var (actualKey, actualValue) = actualDict.ElementAtOrDefault(i);

                this.AssertByValueEquals(expectedKey, actualKey);
                this.AssertByValueEquals(expectedValue, actualValue);
            }
        }

        Assert.Equal(expected.Type, actual.Type);
        Assert.Equal(expected.GetInternalValue(), actual.GetInternalValue());
    }
}


