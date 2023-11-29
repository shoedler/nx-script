using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using NxScript;
using Newtonsoft.Json.Linq;

namespace NxScriptTest;

public struct NxEvalRunResult
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

        this.EvalVisitor.Scopes!.Any(scope =>
        {
            scope.Variables.TryGetValue(key, out result);
            return result is not null;
        });

        return result;
    }
}

public class NxEvalFixture
{
    public NxEvalRunResult CleanRun(string input, string path = "this/path/does/not/exist")
    {
        var stream = CharStreams.fromString(input);
        var lexer = new NxLexer(stream);
        var tokens = new CommonTokenStream(lexer);

        var parseErrorListener = new CountingParseErrorListener();
        var parser = new NxParser(tokens)
        {
            BuildParseTree = true
        };
        parser.RemoveErrorListeners();
        parser.AddErrorListener(parseErrorListener);

        var parseTree = parser.parse();

        var evalVisitor = new NxEvalDiagnosticVisitor(path);

        var output = evalVisitor.Visit(parseTree);

        return new NxEvalRunResult(stream, tokens, parseTree, lexer, parser, evalVisitor, parseErrorListener,
            output);
    }

    public void Dispose()
    {
    }
}

