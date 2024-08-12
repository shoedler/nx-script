using NxScript;

namespace NxScriptTest;

public class ToStringTests : ToTypeTestsBase
{
    private readonly Type StringType = typeof(string);
    private readonly NxValueType NxType = NxValueType.String;

    public ToStringTests(NxEvalFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void String_IsString()
    {
        // Act
        var result = this.Fixture.CleanRun("let foo = \"foo\"");

        // Assert
        Assert.Empty(result.ParseErrors);

        var foo = result.TryGetVariableFromAnyScope("foo");
        Assert.NotNull(foo);
        Assert.True(foo.IsString);

        var fooValue = foo.GetInternalValue();
        Assert.IsType(this.StringType, fooValue);
        Assert.Equal("foo", fooValue);
    }


    [Theory]
    [InlineData(@"""foo"" + true ", "footrue")]
    [InlineData(@"""foo"" + false", "foofalse")]
    public void Bool_ToString_Works(string expression, string expectedInternalResult)
    {
        this.TypeConversion_Works(expression, expectedInternalResult, this.StringType, this.NxType);
    }

    [Theory]
    [InlineData(@"""1234"" + 0", "12340")]
    [InlineData(@"""0"" + 0   ", "00")]
    [InlineData(@"""-1"" + 0  ", "-10")]
    public void Number_ToString_Works(string expression, string expectedInternalResult)
    {
        this.TypeConversion_Works(expression, expectedInternalResult, this.StringType, this.NxType);
    }

    [Theory]
    [InlineData(@"""foo"" + []       ", "foo[]")]
    [InlineData(@"""foo"" + [false]  ", "foo[false]")]
    [InlineData(@"""foo"" + [nil]    ", "foo[]")]
    [InlineData(@"""foo"" + [1,2,3,4]", "foo[1, 2, 3, 4]")]
    public void Array_ToString_Works(string expression, string expectedInternalResult)
    {
        this.TypeConversion_Works(expression, expectedInternalResult, this.StringType, this.NxType);
    }

    [Theory]
    [InlineData(@"""foo"" + {}                          ", "foo{}")]
    [InlineData(@"""foo"" + {false:false}               ", "foo{false: false}")]
    [InlineData(@"""foo"" + {1:2,3:4,5:6,7:8}           ", "foo{1: 2, 3: 4, 5: 6, 7: 8}")]
    [InlineData(@"""foo"" + {""a"":{""nested"":""Lol""}}", "foo{a: {nested: Lol}}")]
    public void Obj_ToString_Works(string expression, string expectedInternalResult)
    {
        this.TypeConversion_Works(expression, expectedInternalResult, this.StringType, this.NxType);
    }
}
