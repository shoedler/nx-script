using NxScript;

namespace NxScriptTest;

public class ToBoolTests : ToTypeTestsBase
{
    private readonly Type BoolType = typeof(bool);
    private readonly NxValueType NxType = NxValueType.Bool;

    public ToBoolTests(NxEvalFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void True_IsTrue()
    {
        // Act
        var result = this.Fixture.CleanRun("let foo = true");

        // Assert
        Assert.Empty(result.ParseErrors);

        var foo = result.TryGetVariableFromAnyScope("foo");
        Assert.NotNull(foo);
        Assert.True(foo.IsBool);

        var fooValue = foo.GetInternalValue();
        Assert.IsType(this.BoolType, fooValue);
        Assert.True(fooValue);
    }

    [Fact]
    public void False_IsFalse()
    {
        // Act
        var result = this.Fixture.CleanRun("let foo = false");

        // Assert
        Assert.Empty(result.ParseErrors);

        var foo = result.TryGetVariableFromAnyScope("foo");
        Assert.NotNull(foo);
        Assert.True(foo.IsBool);

        var fooValue = foo.GetInternalValue();
        Assert.IsType(this.BoolType, fooValue);
        Assert.False(fooValue);
    }

    [Theory]
    [InlineData("true && 0  ", false)]
    [InlineData("true && -1 ", true)]
    [InlineData("true && 1  ", true)]
    [InlineData("true && 123", true)]
    public void Number_ToBool_Works(string expression, bool expectedInternalResult)
    {
        this.TypeConversion_Works(expression, expectedInternalResult, this.BoolType, this.NxType);
    }

    [Theory]
    [InlineData(@"true && """"     ", false)]
    [InlineData(@"true && ""false""", false)]
    [InlineData(@"true && ""true"" ", true)]
    [InlineData(@"true && ""asdf"" ", true)]
    [InlineData(@"true && ""0""    ", true)]
    [InlineData(@"true && ""123""  ", true)]
    public void String_ToBool_Works(string expression, bool expectedInternalResult)
    {
        this.TypeConversion_Works(expression, expectedInternalResult, this.BoolType, this.NxType);
    }

    [Theory]
    [InlineData("true && []     ", true)]
    [InlineData("true && [false]", true)]
    [InlineData("true && [nil]  ", true)]
    public void Array_ToBool_Works(string expression, bool expectedInternalResult)
    {
        this.TypeConversion_Works(expression, expectedInternalResult, this.BoolType, this.NxType);
    }

    [Theory]
    [InlineData("true && {}           ", true)]
    [InlineData("true && {false:false}", true)]
    [InlineData("true && {nil:false}  ", true)]
    public void Obj_ToBool_Works(string expression, bool expectedInternalResult)
    {
        this.TypeConversion_Works(expression, expectedInternalResult, this.BoolType, this.NxType);
    }
}
