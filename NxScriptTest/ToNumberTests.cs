using NxScript;

namespace NxScriptTest;

public class ToNumberTests : ToTypeTestsBase
{
    private readonly Type FloatType = typeof(float);
    private readonly NxValueType NxType = NxValueType.Number;

    public ToNumberTests(NxEvalFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void Zero_IsNumber()
    {
        // Act
        var result = this.fixture.CleanRun("foo = 0;");

        // Assert
        Assert.Empty(result.ParseErrors);

        var foo = result.TryGetVariableFromAnyScope("foo");
        Assert.NotNull(foo);
        Assert.True(foo.IsNumber);

        var fooValue = foo.GetInternalValue(NxValueType.Number);
        Assert.IsType(this.FloatType, fooValue);
        Assert.Equal(0f, fooValue);
    }


    [Theory]
    [InlineData("0 + true ", 1f)]
    [InlineData("0 + false", 0f)]
    public void Boolean_ToNumber_Works(string expression, float expectedInternalResult)
    {
        this.TypeConversion_Works(expression, expectedInternalResult, this.FloatType, this.NxType);
    }

    [Theory]
    [InlineData(@"0 + """"              ", 0f)]
    [InlineData(@"0 + ""1""             ", 1f)]
    [InlineData(@"0 + ""10.23""         ", 10.23f)]
    [InlineData(@"0 + ""123.12312""     ", 123.12312f)]
    [InlineData(@"0 + ""Hello""         ", 0f)]
    [InlineData(@"0 + ""1Hello""        ", 0f)]
    [InlineData(@"0 + ""12.345 Hello""  ", 0f)]
    [InlineData(@"1234 + """"           ", 1234f)]
    [InlineData(@"-1 + ""1""            ", 0f)]
    [InlineData(@"10 + ""abacd""        ", 10f)]
    [InlineData(@"10 + ""abacd123""     ", 10f)]
    [InlineData(@"10 + ""123abacd123""  ", 10f)]
    [InlineData(@"(""-1"" + 0) * ""-2"" ", 20f)]

    public void String_ToNumber_Works(string expression, float expectedInternalResult)
    {
        this.TypeConversion_Works(expression, expectedInternalResult, this.FloatType, this.NxType);
    }

    [Theory]
    [InlineData("0 + []       ", 0f)]
    [InlineData("0 + [false]  ", 1f)]
    [InlineData("0 + [nil]    ", 1f)]
    [InlineData("0 + [1,2,3,4]", 4f)]
    public void Array_ToNumber_Works(string expression, float expectedInternalResult)
    {
        this.TypeConversion_Works(expression, expectedInternalResult, this.FloatType, this.NxType);
    }

    [Theory]
    [InlineData("0 + {}               ", 0f)]
    [InlineData("0 + {false:false}    ", 1f)]
    [InlineData("0 + {1:2,3:4,5:6,7:8}", 4f)]
    public void Obj_ToNumber_Works(string expression, float expectedInternalResult)
    {
        this.TypeConversion_Works(expression, expectedInternalResult, this.FloatType, this.NxType);
    }
}
