using NxScript;

namespace NxScriptTest;

public abstract class ToTypeTestsBase : IClassFixture<NxEvalFixture>
{
    protected NxEvalFixture fixture;

    protected ToTypeTestsBase(NxEvalFixture fixture)
    {
        this.fixture = fixture;
    }

    public void TypeConversion_Works(string expression, dynamic expectedInternalResult, Type expectedInternalType, NxValueType expectedNxValueType)
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

        var aValue = a.GetInternalValue(NxValueType.Number);
        Assert.IsType(expectedInternalType, aValue);
        Assert.Equal(expectedInternalResult, aValue);
    }
}


