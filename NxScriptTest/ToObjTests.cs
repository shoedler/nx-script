using System.Collections;
using NxScript;

namespace NxScriptTest;

public class ToObjTests : ToTypeTestsBase
{
    private readonly Type ObjType = typeof(Dictionary<NxValue, NxValue>);
    private readonly NxValueType NxType = NxValueType.Obj;

    public ToObjTests(NxEvalFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void Obj_IsObj()
    {
        // Act
        var result = this.fixture.CleanRun("foo = {};");

        // Assert
        Assert.Empty(result.ParseErrors);

        var foo = result.TryGetVariableFromAnyScope("foo");
        Assert.NotNull(foo);
        Assert.True(foo.IsObj);

        var fooValue = foo.GetInternalValue(NxValueType.Obj);
        Assert.IsType(this.ObjType, fooValue);
        Assert.Same(fooValue, foo.GetInternalValue(NxValueType.Obj));
    }


    [Theory]
    [InlineData("{} + true            ", new bool[] { true, true })] // {true: true}
    [InlineData("{} + false           ", new bool[] { false, false })] // {false: false}
    [InlineData("{true:false} + true  ", new bool[] { true, true })] // {true: true}
    [InlineData("{false: false} + true", new bool[] { false, false }, new bool[] { true, true })] // {false: false, true: true}
    [InlineData("{false: true} + true ", new bool[] { false, true }, new bool[] { true, true })] // {false: true, true: true}
    public void Bool_ToObj_Works(string expression, params IEnumerable[] expectedInternalResult)
    {
        this.ToObj_Works(expression, expectedInternalResult);
    }

    [Theory]
    [InlineData(@"{} + """"                    ", new string[] { "", "" })] // {"": ""}
    [InlineData(@"{} + ""1""                   ", new string[] { "1", "1" })] // {"1": "1"}
    [InlineData(@"{} + ""123.12312""           ", new string[] { "123.12312", "123.12312" })] // {"123.12312": "123.12312"}
    [InlineData(@"{} + ""Hello""               ", new string[] { "Hello", "Hello" })] // {"Hello": "Hello"}
    [InlineData(@"{} + ""abacd""               ", new string[] { "abacd", "abacd" })] // {"abacd": "abacd"}
    [InlineData(@"{""123"": false} + ""123""   ", new string[] { "123", "123" })] // {"123": "123"}
    [InlineData(@"{""Foo"": ""lol""} + ""Bar"" ", new string[] { "Foo", "lol" }, new string[] { "Bar", "Bar" })] // {"Foo": "lol", "Bar": "Bar"}
    public void String_ToObj_Works(string expression, params IEnumerable[]? expectedInternalResult)
    {
        this.ToObj_Works(expression, expectedInternalResult);
    }

    [Theory]
    [InlineData("{} + 0              ", new float[] { 0f, 0f })] // {0: 0}
    [InlineData("{} + 1              ", new float[] { 1f, 1f })] // {1: 1}
    [InlineData("{} + 123.12312      ", new float[] { 123.12312f, 123.12312f })] // {123.12312: 123.12312}
    [InlineData("{} + -123.12312     ", new float[] { -123.12312f, -123.12312f })] // {-123.12312: -123.12312}
    [InlineData("{69:69} + 200       ", new float[] { 69f, 69f }, new float[] { 200f, 200f })] // {69: 69, 200: 200}
    [InlineData("{200:100} + 200 + 20", new float[] { 200f, 200f }, new float[] { 20f, 20f })] // {200: 200, 20: 20}
    public void Number_ToObj_Works(string expression, params IEnumerable[]? expectedInternalResult)
    {
        this.ToObj_Works(expression, expectedInternalResult);
    }

    [Theory]
    [InlineData("{} + []       ")] // {}
    [InlineData("{} + [false]  ", new bool[] { false, false })] // {false: false}
    [InlineData("{} + [\"Hi\"] ", new string[] { "Hi", "Hi" })] // {"Hi": "Hi"}
    [InlineData("{} + [1,2,3,4]", new float[] { 1f, 1f }, new float[] { 2f, 2f }, new float[] { 3f, 3f }, new float[] { 4f, 4f })] // {1: 1, 2: 2, 3: 3, 4: 4}
    [InlineData("{1:10} + [1,2]", new float[] { 1f, 1f }, new float[] { 2f, 2f })] // {1: 1, 2: 2}
    public void Array_ToObj_Works(string expression, params dynamic[]? expectedInternalResult)
    {
        this.ToObj_Works(expression, expectedInternalResult);
    }

    private void ToObj_Works(string expression, params dynamic[]? expectedInternalResult)
    {
        // Arrange
        var expectedObjContent = expectedInternalResult.ToDictionary(pair => new NxValue(pair[0]), pair => new NxValue(pair[1]));

        // Act, Assert
        this.TypeConversion_Works(expression, expectedObjContent, this.ObjType, this.NxType);
    }
}
