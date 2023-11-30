using System.Collections;
using NxScript;

namespace NxScriptTest;

public class ToArrayTests : ToTypeTestsBase
{
    private readonly Type ArrayType = typeof(List<NxValue>);
    private readonly NxValueType NxType = NxValueType.Array;

    public ToArrayTests(NxEvalFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void Array_IsArray()
    {
        // Act
        var result = this.fixture.CleanRun("foo = [];");

        // Assert
        Assert.Empty(result.ParseErrors);

        var foo = result.TryGetVariableFromAnyScope("foo");
        Assert.NotNull(foo);
        Assert.True(foo.IsArray);

        var fooValue = foo.GetInternalValue(NxValueType.Array);
        Assert.IsType(this.ArrayType, fooValue);
        Assert.Same(fooValue, foo.GetInternalValue(NxValueType.Array));
    }


    [Theory]
    [InlineData("[] + true", true)] // [true]
    [InlineData("[] + false", false)] // [false]
    [InlineData("[false] + false", false, false)] // [false, false]
    [InlineData("[false] + true", false, true)] // [false, true]
    public void Boolean_ToArray_Works(string expression, params dynamic[] expectedInternalResult)
    {
        this.ToArray_Works(expression, expectedInternalResult);
    }

    [Theory]
    [InlineData(@"[] + """"             ")] // []
    [InlineData(@"[] + ""1""            ", "1")] // [1]
    [InlineData(@"[] + ""123.12312""    ", "1", "2", "3", ".", "1", "2", "3", "1", "2")] // [1, 2, 3, ., 1, 2, 3, 1, 2]
    [InlineData(@"[] + ""Hello""        ", "H", "e", "l", "l", "o")] // [H, e, l, l, o]
    [InlineData(@"[] + ""abacd""        ", "a", "b", "a", "c", "d")] // [a, b, a, c, d]
    [InlineData(@"[""a"",""b"",""c""] + ""123""     ", "a", "b", "c", "1", "2", "3")] // [a, b, c, 1, 2, 3]
    public void String_ToArray_Works(string expression, params dynamic[]? expectedInternalResult)
    {
        this.ToArray_Works(expression, expectedInternalResult);
    }

    [Theory]
    [InlineData("[] + 0", 0f)] // [0]
    [InlineData("[] + 1", 1f)] // [1]
    [InlineData("[] + 123.12312", 123.12312f)] // [123.12312]
    [InlineData("[] + -123.12312", -123.12312f)] // [-123.12312]
    [InlineData("[69] + 200", 69f, 200f)] // [69, 200]
    [InlineData("[200] + 200 + 20", 200f, 200f, 20f)] // [200, 200, 20]
    public void Number_ToArray_Works(string expression, params dynamic[]? expectedInternalResult)
    {
        this.ToArray_Works(expression, expectedInternalResult);
    }

    [Theory]
    [InlineData("[] + {}")] // []
    [InlineData("[] + {1: 2}", new float[] { 1, 2 })] // [[1, 2]]
    [InlineData("[] + {1: 2, 3: 4}", new float[] { 1, 2 }, new float[] { 3, 4 })] // [[1, 2], [3, 4]]
    [InlineData("[] + {1: 2, 3: 4, 5: 6}", new float[] { 1, 2 }, new float[] { 3, 4 }, new float[] { 5, 6 })] // [[1, 2], [3, 4], [5, 6]]
    [InlineData("[[1,2]] + {1:2}", new float[] { 1, 2 }, new float[] { 1, 2 })] // [[1, 2], [1, 2]]
    public void Obj_ToArray_Works(string expression, params dynamic[]? expectedInternalResult)
    {
        if (expectedInternalResult.Length == 0)
        {
            this.ToArray_Works(expression, expectedInternalResult);
            return;
        }

        List<NxValue> expectedInternalResultArray = expectedInternalResult.Select(pair =>
        {
            var pairArrayContent = new List<NxValue>
            {
                new NxValue(pair[0]),
                new NxValue(pair[1])
            };

            return new NxValue(pairArrayContent);
        }).ToList();

        this.TypeConversion_Works(expression, expectedInternalResultArray, this.ArrayType, this.NxType);
    }

    private void ToArray_Works(string expression, params dynamic[]? expectedInternalResult)
    {
        // Arrange
        var arrayContent = expectedInternalResult ?? Array.Empty<dynamic>();
        var expectedArray = arrayContent.Select(value => new NxValue(value)).ToList();

        // Act, Assert
        this.TypeConversion_Works(expression, expectedArray, this.ArrayType, this.NxType);
    }
}
