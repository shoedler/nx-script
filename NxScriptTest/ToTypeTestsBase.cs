using NxScript;

namespace NxScriptTest;

public abstract class ToTypeTestsBase : IClassFixture<NxEvalFixture>
{
    protected NxEvalFixture fixture;

    protected ToTypeTestsBase(NxEvalFixture fixture)
    {
        this.fixture = fixture;
    }

    public NxValue TypeConversion_Works(string expression, dynamic expectedInternalResult, Type expectedInternalType, NxValueType expectedNxValueType)
    {
        // Arrange
        var prog = $"a = {expression};";

        // Act
        var result = this.fixture.CleanRun(prog);

        // Assert
        Assert.Empty(result.ParseErrors);

        var a = result.TryGetVariableFromAnyScope("a");
        Assert.NotNull(a);
        Assert.Equal(a.Type, expectedNxValueType);

        var aValue = a.GetInternalValue(expectedNxValueType);
        Assert.NotNull(aValue);
        Assert.IsType(expectedInternalType, aValue);

        if (expectedInternalResult is List<NxValue>) // Arrays
        {
            this.AssertByValueEquals(new NxValue(expectedInternalResult), a);
        }
        else if (expectedInternalResult is Dictionary<NxValue, NxValue>)
        {
            throw new NotImplementedException();
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

        if (expected.Type == NxValueType.Array)
        {
            List<NxValue> expectedArray = expected.GetInternalValue(NxValueType.Array)!;
            List<NxValue>? actualArray = actual.GetInternalValue(NxValueType.Array);

            var length = Math.Max(expectedArray.Count, actualArray?.Count ?? 0);

            for (var i = 0; i < length; i++)
            {
                this.AssertByValueEquals(expectedArray[i], actualArray[i]);
            }

            return;
        }

        Assert.Equal(expected.Type, actual.Type);
        Assert.Equal(expected.GetInternalValue(expected.Type), actual.GetInternalValue(actual.Type));
    }
}


