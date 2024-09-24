using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using NxScript;

namespace NxScriptTest;

public readonly struct NxEvalRunResult
{
    public readonly ICharStream Stream;
    public readonly ITokenStream TokenStream;
    public readonly IParseTree ParseTree;

    public readonly NxLexer Lexer;
    public readonly NxParser Parser;

    public readonly NxEvalDiagnosticVisitor EvalVisitor;
    public readonly CountingParseErrorListener ParseErrorListener;

    public readonly NxValue Output;

    public List<string> ParseErrors => this.ParseErrorListener.OccurredErrors;

    public NxEvalRunResult(
        ICharStream stream,
        ITokenStream tokenStream,
        IParseTree parseTree,
        NxLexer lexer,
        NxParser parser,
        NxEvalDiagnosticVisitor evalVisitor,
        CountingParseErrorListener parseErrorListener,
        NxValue output
    )
    {
        this.Stream = stream;
        this.TokenStream = tokenStream;
        this.ParseTree = parseTree;
        this.Lexer = lexer;
        this.Parser = parser;
        this.EvalVisitor = evalVisitor;
        this.ParseErrorListener = parseErrorListener;
        this.Output = output;
    }

    public NxValue? TryGetVariableFromAnyScope(string key)
    {
        NxValue? result = null;

        var _ = this.EvalVisitor.Scopes!.Any(scope =>
        {
            scope.Variables.TryGetValue(key, out result);
            return result is not null;
        });

        return result;
    }
}