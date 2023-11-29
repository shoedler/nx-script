using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using NxScript.src;

namespace NxScript;

internal class NxEvalVisitor : AbstractParseTreeVisitor<NxValue>, INxVisitor<NxValue>
{
    internal NxEvalVisitor? Upper = null;

    internal Dictionary<string, NxValue> Variables = new();
    internal Dictionary<string, Func<List<NxValue>, NxValue>> Functions = new();

    public NxEvalVisitor()
    {
        // Preload variables
        this.Variables.Add("pi", new NxValue((float)Math.PI));
        this.Variables.Add("cwd", new NxValue(Environment.CurrentDirectory));

        // Preload "print" function - print to console
        this.Functions.Add("print", (args) =>
        {
            args.ForEach(arg =>
            {
                var log = arg.IsString ? $"\"{arg.AsString().InGreen()}\"" : arg.AsString();
                Console.Write(log + " ");
            });

            Console.WriteLine();
            return new NxValue();
        });

        // Preload "read" function - read files from disk
        this.Functions.Add("read", args =>
        {
            var content = File.ReadAllText(args[0].AsString());
            return new NxValue(content);
        });

        // Preload "write" function - write to disk
        this.Functions.Add("write", args =>
        {
            File.WriteAllText(args[0].AsString(), args[1].AsString() ?? "");
            return new NxValue(args[0].AsString());
        });

        // Preload "help" function - show vars & funcs
        this.Functions.Add("help", args =>
        {
            Console.WriteLine("Variables in global scope:");
            this.Variables.AsEnumerable().ToList()
                .ForEach((kvp) => Console.WriteLine($"{kvp.Key}:\t{kvp.Value.AsString()}"));
            Console.WriteLine("Functions in global scope:");
            this.Functions.AsEnumerable().ToList()
                .ForEach((kvp) => Console.WriteLine($"{kvp.Key}:\t{kvp.Value}"));
            return new NxValue();
        });

        // Preload "typeof" function - show type of value
        this.Functions.Add("typeof", (args =>
        {
            var type = args[0].IsBoolean ? "Boolean" :
                args[0].IsString ? "String" :
                args[0].IsNumber ? "Number" : "Nil";
            return new NxValue(type);
        }));
    }

    private NxEvalVisitor(NxEvalVisitor upper)
    {
        this.Upper = upper;
    }

    public NxValue VisitParse([NotNull] NxParser.ParseContext context)
    {
        VisitChildren(context);
        return new NxValue();
    }

    public NxValue VisitBlock([NotNull] NxParser.BlockContext context)
    {
        var visitor = new NxEvalVisitor(this);
        visitor.VisitChildren(context);

        return new NxValue();
    }

    ///
    /// Statements
    ///
    public NxValue VisitStat([NotNull] NxParser.StatContext context)
    {
        VisitChildren(context);
        return new NxValue();
    }

    public NxValue VisitAssignment([NotNull] NxParser.AssignmentContext context)
    {
        this.SetVariable(context.ID().GetText(), Visit(context.expr()));
        return new NxValue();
    }

    public NxValue VisitIf_stat([NotNull] NxParser.If_statContext context)
    {
        var expressions = context.expr();
        var statBlocks = context.stat_block();

        foreach (var (condition, stat) in expressions.Zip(statBlocks))
        {
            if (!Visit(condition).AsBoolean())
            {
                continue;
            }

            Visit(stat);
            return new NxValue();
        }

        if (statBlocks.Length > expressions.Length)
        {
            Visit(statBlocks.Last());
        }

        return new NxValue();
    }

    public NxValue VisitWhile_stat([NotNull] NxParser.While_statContext context)
    {
        var panic = 0;

        while (Visit(context.expr()).AsBoolean())
        {
            Visit(context.stat_block());
            if (panic++ > 1e6)
            {
                throw NxEvalException.FromContext($"Panic! Loop did not finish after {panic} iterations", context);
            }
        }

        return new NxValue();

    }

    ///
    /// Blocks
    /// 
    public NxValue VisitStat_block([NotNull] NxParser.Stat_blockContext context)
    {
        VisitChildren(context);
        return new NxValue();
    }

    ///
    /// Literals
    ///
    public NxValue VisitArray_literal([NotNull] NxParser.Array_literalContext context)
    {
        var items = context.expr().Select(Visit).ToList();

        return new NxValue(items);
    }

    public NxValue VisitObj_literal([NotNull] NxParser.Obj_literalContext context)
    {
        var atoms = context.atom().Select(Visit);
        var expressions = context.expr().Select(Visit);

        return new NxValue(atoms.Zip(expressions));
    }

    ///
    /// Expressions
    /// 
    public NxValue VisitFnCallExpr([NotNull] NxParser.FnCallExprContext context)
    {
        var key = context.ID().GetText();

        var function = this.GetFunction(key, context);
        var args = context.expr().Select(base.Visit).ToList();
        return function.Invoke(args);
    }

    public NxValue VisitIndexExpr([NotNull] NxParser.IndexExprContext context)
    {
        var array = base.Visit(context.expr().First());
        var index = base.Visit(context.expr().Last());

        return array.Index(index);
    }

    public NxValue VisitMemberExpr([NotNull] NxParser.MemberExprContext context)
    {
        var obj = base.Visit(context.expr());
        var member = new NxValue(context.ID().GetText());
            
        return obj.Member(member);
    }

    public NxValue VisitArrayExpr([NotNull] NxParser.ArrayExprContext context)
    {
        return base.Visit(context.array_literal()); // Just pluck the literal, could also just return the default base.VisitChildren() but this is quicker
    }

    public NxValue VisitObjExpr([NotNull] NxParser.ObjExprContext context)
    {
        return base.Visit(context.obj_literal()); // Just pluck the literal, could also just return the default base.VisitChildren() but this is quicker
    }

    public NxValue VisitPowExpr([NotNull] NxParser.PowExprContext context)
    {
        var left = base.Visit(context.expr()[0]);
        var right = base.Visit(context.expr()[1]);

        return new NxValue((float)Math.Pow(left.AsNumber(), right.AsNumber()));
    }

    public NxValue VisitUnaryMinusExpr([NotNull] NxParser.UnaryMinusExprContext context)
    {
        var right = base.Visit(context.expr());

        return new NxValue(-right.AsNumber());
    }

    public NxValue VisitNotExpr([NotNull] NxParser.NotExprContext context)
    {
        var right = base.Visit(context.expr());

        return new NxValue(!right.AsBoolean());
    }

    public NxValue VisitMultiplicationExpr([NotNull] NxParser.MultiplicationExprContext context)
    {
        var left = base.Visit(context.expr()[0]);
        var right = base.Visit(context.expr()[1]);

        return context.op.Type switch
        {
            NxParser.MULT => new NxValue(left.AsNumber() * right.AsNumber()),
            NxParser.DIV => new NxValue(left.AsNumber() / right.AsNumber()),
            NxParser.MOD => new NxValue(left.AsNumber() % right.AsNumber()),
            _ => throw NxEvalException.FromContext($"Don't know this multiplicative operator '{context.GetText()}'", context)
        };
    }

    public NxValue VisitAdditiveExpr([NotNull] NxParser.AdditiveExprContext context)
    {
        var left = base.Visit(context.expr()[0]);
        var right = base.Visit(context.expr()[1]);

        return context.op.Type switch
        {
            NxParser.PLUS => left.IsString ? new NxValue(left.AsString() + right.AsString()) : new NxValue(left.AsNumber() + right.AsNumber()),
            NxParser.MINUS => new NxValue(left.AsNumber() - right.AsNumber()),
            _ => throw NxEvalException.FromContext($"Don't know this additive operator '{context.GetText()}'", context)
        };
    }

    public NxValue VisitRelationalExpr([NotNull] NxParser.RelationalExprContext context)
    {
        var left = base.Visit(context.expr()[0]);
        var right = base.Visit(context.expr()[1]);

        return context.op.Type switch
        {
            NxParser.LTEQ => new NxValue(left.AsNumber() <= right.AsNumber()),
            NxParser.GTEQ => new NxValue(left.AsNumber() >= right.AsNumber()),
            NxParser.LT => new NxValue(left.AsNumber() < right.AsNumber()),
            NxParser.GT => new NxValue(left.AsNumber() > right.AsNumber()),
            _ => throw NxEvalException.FromContext($"Don't know this relational operator '{context.GetText()}'", context)
        };
    }

    public NxValue VisitEqualityExpr([NotNull] NxParser.EqualityExprContext context)
    {
        var left = base.Visit(context.expr()[0]);
        var right = base.Visit(context.expr()[1]);

        return context.op.Type switch
        {
            NxParser.EQ => new NxValue(left.AsBoolean() == right.AsBoolean()),
            NxParser.NEQ => new NxValue(left.AsBoolean() != right.AsBoolean()),
            _ => throw NxEvalException.FromContext($"Don't know this equality operator '{context.GetText()}'", context)
        };
    }

    public NxValue VisitAndExpr([NotNull] NxParser.AndExprContext context)
    {
        var left = base.Visit(context.expr()[0]);
        var right = base.Visit(context.expr()[1]);

        return new NxValue(left.AsBoolean() && right.AsBoolean());
    }

    public NxValue VisitOrExpr([NotNull] NxParser.OrExprContext context)
    {
        var left = base.Visit(context.expr()[0]);
        var right = base.Visit(context.expr()[1]);

        return new NxValue(left.AsBoolean() || right.AsBoolean());
    }

    public NxValue VisitAtomExpr([NotNull] NxParser.AtomExprContext context)
    {
        return base.VisitChildren(context);
    }

    ///
    /// Atoms
    /// 
    public NxValue VisitParExpr([NotNull] NxParser.ParExprContext context) { return base.Visit(context.expr()); }
    public NxValue VisitNumberAtom([NotNull] NxParser.NumberAtomContext context) { return new NxValue(context); }
    public NxValue VisitStringAtom([NotNull] NxParser.StringAtomContext context) { return new NxValue(context); }
    public NxValue VisitBooleanAtom([NotNull] NxParser.BooleanAtomContext context) { return new NxValue(context); }
    public NxValue VisitNilAtom([NotNull] NxParser.NilAtomContext context) { return new NxValue(); }
    public NxValue VisitIdAtom([NotNull] NxParser.IdAtomContext context) { return this.GetVariable(context.ID().GetText(), context); }

    ///
    /// Helpers
    /// 
    internal NxValue GetVariable(string key, ParserRuleContext context)
    {
        if (this.Variables.ContainsKey(key))
        {
            return this.Variables[key];
        }

        if (this.Upper is null)
        {
            throw NxEvalException.FromContext($"Variable '{key}' is not defined", context);
        }

        return this.Upper.GetVariable(key, context);
    }

    internal void SetVariable(string key, NxValue value)
    {
        if (this.UpdateVariable(key, value))
        {
            return;
        }

        this.Variables[key] = value;
    }


    internal bool UpdateVariable(string key, NxValue value)
    {
        if (this.Variables.ContainsKey(key))
        {
            this.Variables[key] = value;
            return true;
        }

        if (this.Upper is null)
        {
            return false;
        }

        return this.Upper.UpdateVariable(key, value);
    }

    internal Func<List<NxValue>, NxValue> GetFunction(string key, ParserRuleContext context)
    {
        if (this.Functions.ContainsKey(key))
        {
            return this.Functions[key];
        }

        if (this.Upper is null)
        {
            throw NxEvalException.FromContext($"Function '{key}' is not defined", context);
        }

        return this.Upper.GetFunction(key, context);
    }
}