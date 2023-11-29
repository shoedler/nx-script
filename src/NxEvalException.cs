using Antlr4.Runtime;

namespace NxScript;

internal class NxEvalException : Exception
{
    public readonly ParserRuleContext Context;

    private NxEvalException(string message, ParserRuleContext context) : base(message)
    {
        this.Context = context;
    }

    public static NxEvalException FromContext(string message, ParserRuleContext context)
    {
        var pos = (context.Start.Line == context.Stop.Line) ?
            $"{context.Start.Line}:{context.Start.Column}-{context.Stop.Column}" :
            $"{context.Start.Line}:{context.Start.Column}-{context.Stop.Line}:{context.Stop.Column}";

        var exceptionMessage = $"ERROR: {context.Start.InputStream.SourceName}:{pos}: {message}";

        return new NxEvalException(exceptionMessage, context);
    }
}