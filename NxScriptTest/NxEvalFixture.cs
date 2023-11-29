using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using NxScript;

namespace NxScriptTest;

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

