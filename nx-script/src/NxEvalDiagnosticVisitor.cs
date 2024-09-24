using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace NxScript;

public class NxEvalDiagnosticVisitor : NxEvalVisitor
{
    public readonly List<NxEvalVisitor>? Scopes = null;
    public NxEvalDiagnosticVisitor(string path) : base(path)
    {
        this.Scopes = [this];
        
    }

    protected NxEvalDiagnosticVisitor(NxEvalVisitor upper) : base(upper) 
    {
        // Add myself to Scopes list
        var root = this.GetRoot();

        if (root.Scopes is null)
        {
            throw new InvalidOperationException("Scope list must exist in root scope");
        }

        root.Scopes.Add(this);
    }

    protected override NxEvalVisitor NewScope()
    {
        return new NxEvalDiagnosticVisitor(this);
    }
    
    internal NxEvalDiagnosticVisitor GetRoot()
    {
        var ptr = this;

        while (ptr.Upper is not null)
        {
            ptr = (NxEvalDiagnosticVisitor)ptr.Upper;
        }

        return ptr;
    }
}