using NxScript;

namespace NxScriptTest;

public class ToBooleanTests : ToTypeTestsBase
{
    private readonly Type BooleanType = typeof(bool);
    private readonly NxValueType NxType = NxValueType.Boolean;

    public ToBooleanTests(NxEvalFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void True_IsTrue()
    {
        // Act
        var result = this.fixture.CleanRun("foo = true;");

        // Assert
        Assert.Empty(result.ParseErrors);

        var foo = result.TryGetVariableFromAnyScope("foo");
        Assert.NotNull(foo);
        Assert.True(foo.IsBoolean);

        var fooValue = foo.GetInternalValue(NxValueType.Boolean);
        Assert.IsType(this.BooleanType, fooValue);
        Assert.True(fooValue);
    }

    [Fact]
    public void False_IsFalse()
    {
        // Act
        var result = this.fixture.CleanRun("foo = false;");

        // Assert
        Assert.Empty(result.ParseErrors);

        var foo = result.TryGetVariableFromAnyScope("foo");
        Assert.NotNull(foo);
        Assert.True(foo.IsBoolean);

        var fooValue = foo.GetInternalValue(NxValueType.Boolean);
        Assert.IsType(this.BooleanType, fooValue);
        Assert.False(fooValue);
    }

    [Theory]
    [InlineData("true && 0  ", false)]
    [InlineData("true && -1 ", true)]
    [InlineData("true && 1  ", true)]
    [InlineData("true && 123", true)]
    public void Number_ToBoolean_Works(string expression, bool expectedInternalResult)
    {
        this.TypeConversion_Works(expression, expectedInternalResult, this.BooleanType, this.NxType);
    }

    [Theory]
    [InlineData(@"true && """"     ", false)]
    [InlineData(@"true && ""false""", false)]
    [InlineData(@"true && ""true"" ", true)]
    [InlineData(@"true && ""asdf"" ", true)]
    [InlineData(@"true && ""0""    ", true)]
    [InlineData(@"true && ""123""  ", true)]
    public void String_ToBoolean_Works(string expression, bool expectedInternalResult)
    {
        this.TypeConversion_Works(expression, expectedInternalResult, this.BooleanType, this.NxType);
    }

    [Theory]
    [InlineData("true && []     ", true)]
    [InlineData("true && [false]", true)]
    [InlineData("true && [nil]  ", true)]
    public void Array_ToBoolean_Works(string expression, bool expectedInternalResult)
    {
        this.TypeConversion_Works(expression, expectedInternalResult, this.BooleanType, this.NxType);
    }

    [Theory]
    [InlineData("true && {}           ", true)]
    [InlineData("true && {false:false}", true)]
    [InlineData("true && {nil:false}  ", true)]
    public void Obj_ToBoolean_Works(string expression, bool expectedInternalResult)
    {
        this.TypeConversion_Works(expression, expectedInternalResult, this.BooleanType, this.NxType);
    }
}
