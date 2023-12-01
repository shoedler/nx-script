using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace NxScript;

public class NxEvalVisitor : AbstractParseTreeVisitor<NxValue>, INxVisitor<NxValue>
{
    public readonly Dictionary<string, NxValue> Variables = new();

    protected NxEvalVisitor? Upper = null;
    protected NxEvalVisitor? Fn = null;
    protected NxValue? ReturnValue = null;
    protected override NxValue DefaultResult => new NxValue(); // Always a new one!

    public NxEvalVisitor(string path)
    {
        // Preload variables
        this.Variables.Add("pi", new NxValue((float)Math.PI));
        this.Variables.Add("cwd", new NxValue(Path.Combine(Environment.CurrentDirectory, Path.GetDirectoryName(path)!)));

        // Preload "print" function - print to console
        this.Variables.Add("print", new NxValue((args) =>
        {
            args.ForEach(arg =>
            {
                var log = arg.Type switch
                {
                    NxValueType.String => $"{arg.AsString()}".InGreen(),
                    NxValueType.Bool => arg.AsString().InMagenta(),
                    NxValueType.Number => arg.AsString().InYellow(),
                    NxValueType.Array or 
                    NxValueType.Obj or 
                    NxValueType.Fn => arg.AsString().InCyan(),
                    NxValueType.Nil => arg.AsString().InBlue(),
                    _ => arg.AsString()
                };

                Console.Write(log + " ");
            });

            Console.WriteLine();
            return new NxValue();
        }));

        // Preload "read" function - read files from disk
        this.Variables.Add("read", new NxValue(args =>
        {
            var content = File.ReadAllText(args[0].AsString());
            return new NxValue(content);
        }));

        // Preload "write" function - write to disk
        this.Variables.Add("write", new NxValue(args =>
        {
            File.WriteAllText(args[0].AsString(), args[1].AsString() ?? "");
            return new NxValue(args[0].AsString());
        }));

        // Preload "help" function - show vars & funcs
        this.Variables.Add("help", new NxValue(args =>
        {
            Console.WriteLine("Variables in global scope:");
            this.Variables.AsEnumerable().ToList()
                .ForEach((pair) => Console.WriteLine($"{pair.Key}:\t{pair.Value.AsString()}"));
            return new NxValue();
        }));

        // Preload "typeof" function - show type of value
        this.Variables.Add("typeof", new NxValue(args =>
        {
            return new NxValue(Enum.GetName(args[0].Type));
        }));
    }

    protected NxEvalVisitor(NxEvalVisitor upper)
    {
        this.Upper = upper;
        this.Fn = upper.Fn;
    }

    public NxValue VisitParse([NotNull] NxParser.ParseContext context)
    {
        return VisitChildren(context);
    }

    public NxValue VisitBlock([NotNull] NxParser.BlockContext context)
    {
        var visitor = this.NewScope();
        return visitor.VisitChildren(context);
    }

    public NxValue VisitStat_block([NotNull] NxParser.Stat_blockContext context)
    {
        return VisitChildren(context);
    }

    ///
    /// Statements
    ///
    public NxValue VisitStat([NotNull] NxParser.StatContext context)
    {

       return VisitChildren(context);;
    }

    public NxValue VisitAssignment([NotNull] NxParser.AssignmentContext context)
    {
        var value = Visit(context.expr());
        this.SetVariable(context.ID().GetText(), value);
        return value;
    }

    public NxValue VisitFn_declaration([NotNull] NxParser.Fn_declarationContext context)
    {
        var names = context.ID().Select(n => n.GetText());
        var name = names.First();
        var argNames = names.Skip(1);

        // Some manual optimization to ensure we don't create a new scope twice when running trough 
        // VisitBlock() (Affects fns with block bodies)
        var blockBody = context.stat_block().block();
        var statBody = context.stat_block().stat();

        IParseTree fnBody = blockBody is not null ? blockBody : statBody;

        // Closure
        var fn = new NxValue((args) =>
        {
            var fnVisitor = this.NewScope();

            foreach (var (argName, value) in argNames.Zip(args))
            {
                // We deliberaltely set it manually here. Bc if there's a an identically named variable above, it's not gonna be fun.
                fnVisitor.Variables[argName] = value ?? throw NxEvalException.FromContext($"Missing arg {argName}", context);
            }

            fnVisitor.Fn = fnVisitor;
            var ret = fnVisitor.Visit(fnBody);

            return fnVisitor.Fn.ReturnValue ?? ret ?? DefaultResult;
        });

        this.SetVariable(name, fn);
        return fn;
    }

    public NxValue VisitIf_stat([NotNull] NxParser.If_statContext context)
    {
        var expressions = context.expr();
        var statBlocks = context.stat_block();

        foreach (var (condition, stat) in expressions.Zip(statBlocks))
        {
            if (!Visit(condition).AsBool())
            {
                continue;
            }

            return Visit(stat);
        }

        if (statBlocks.Length > expressions.Length)
        {
            return Visit(statBlocks.Last());
        }

        return DefaultResult;
    }

    public NxValue VisitWhile_stat([NotNull] NxParser.While_statContext context)
    {
        var panic = 0;
        NxValue lastValue = DefaultResult;

        while (Visit(context.expr()).AsBool())
        {
            lastValue = Visit(context.stat_block());
            if (panic++ > 10_000_000) // TODO: Move to Constants
            {
                throw NxEvalException.FromContext($"Panic! Loop did not finish after {panic} iterations", context);
            }
        }

        return lastValue;
    }

    public NxValue VisitReturn([NotNull] NxParser.ReturnContext context)
    {
        var ret = VisitChildren(context);

        if (this.Fn is not null)
        {
            this.Fn.ReturnValue = ret;
            return this.Fn.ReturnValue;
        }

        return ret;
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
        var function = this.GetVariable(key, context);
        var args = context.expr().Select(Visit).ToList();

        return function.AsFn().Invoke(args);
    }

    public NxValue VisitIndexExpr([NotNull] NxParser.IndexExprContext context)
    {
        var array = Visit(context.expr().First());
        var index = Visit(context.expr().Last());

        return NxValue.Index(array, index);
    }

    public NxValue VisitMemberExpr([NotNull] NxParser.MemberExprContext context)
    {
        var obj = Visit(context.expr());
        var member = new NxValue(context.ID().GetText());

        return NxValue.Member(obj, member);
    }

    public NxValue VisitArrayExpr([NotNull] NxParser.ArrayExprContext context)
    {
        return Visit(context.array_literal()); // Just pluck the literal, could also just return the default base.VisitChildren() but this is quicker
    }

    public NxValue VisitObjExpr([NotNull] NxParser.ObjExprContext context)
    {
        return Visit(context.obj_literal()); // Just pluck the literal, could also just return the default VisitChildren() but this is quicker
    }

    public NxValue VisitPowExpr([NotNull] NxParser.PowExprContext context)
    {
        var left = Visit(context.expr()[0]);
        var right = Visit(context.expr()[1]);

        return new NxValue((float)Math.Pow(left.AsNumber(), right.AsNumber()));
    }

    public NxValue VisitUnaryMinusExpr([NotNull] NxParser.UnaryMinusExprContext context)
    {
        var right = Visit(context.expr());

        return new NxValue(-right.AsNumber());
    }

    public NxValue VisitNotExpr([NotNull] NxParser.NotExprContext context)
    {
        var right = Visit(context.expr());

        return new NxValue(!right.AsBool());
    }

    public NxValue VisitMultiplicationExpr([NotNull] NxParser.MultiplicationExprContext context)
    {
        var left = Visit(context.expr()[0]);
        var right = Visit(context.expr()[1]);

        return context.op.Type switch
        {
            NxParser.MULT => NxValue.Multiply(left, right),
            NxParser.DIV => new NxValue(left.AsNumber() / right.AsNumber()),
            NxParser.MOD => new NxValue(left.AsNumber() % right.AsNumber()),
            _ => throw NxEvalException.FromContext($"Don't know this multiplicative operator '{context.GetText()}'", context)
        };
    }

    public NxValue VisitAdditiveExpr([NotNull] NxParser.AdditiveExprContext context)
    {
        var left = Visit(context.expr()[0]);
        var right = Visit(context.expr()[1]);

        return context.op.Type switch
        {
            NxParser.PLUS => NxValue.Add(left, right),
            NxParser.MINUS => new NxValue(left.AsNumber() - right.AsNumber()),
            _ => throw NxEvalException.FromContext($"Don't know this additive operator '{context.GetText()}'", context)
        };
    }

    public NxValue VisitRelationalExpr([NotNull] NxParser.RelationalExprContext context)
    {
        var left = Visit(context.expr()[0]);
        var right = Visit(context.expr()[1]);

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
        var left = Visit(context.expr()[0]);
        var right = Visit(context.expr()[1]);

        return context.op.Type switch
        {
            NxParser.EQ => NxValue.Eq(left,right),
            NxParser.NEQ => NxValue.Neq(left, right),
            _ => throw NxEvalException.FromContext($"Don't know this equality operator '{context.GetText()}'", context)
        };
    }

    public NxValue VisitAndExpr([NotNull] NxParser.AndExprContext context)
    {
        var left = Visit(context.expr()[0]);
        var right = Visit(context.expr()[1]);

        return new NxValue(left.AsBool() && right.AsBool());
    }

    public NxValue VisitOrExpr([NotNull] NxParser.OrExprContext context)
    {
        var left = Visit(context.expr()[0]);
        var right = Visit(context.expr()[1]);

        return new NxValue(left.AsBool() || right.AsBool());
    }

    public NxValue VisitAtomExpr([NotNull] NxParser.AtomExprContext context)
    {
        return VisitChildren(context);
    }

    ///
    /// Atoms
    /// 
    public NxValue VisitParExpr([NotNull] NxParser.ParExprContext context) { return Visit(context.expr()); }
    public NxValue VisitNumberAtom([NotNull] NxParser.NumberAtomContext context) { return new NxValue(context); }
    public NxValue VisitStringAtom([NotNull] NxParser.StringAtomContext context) { return new NxValue(context); }
    public NxValue VisitBoolAtom([NotNull] NxParser.BoolAtomContext context) { return new NxValue(context); }
    public NxValue VisitNilAtom([NotNull] NxParser.NilAtomContext context) { return new NxValue(); }
    public NxValue VisitIdAtom([NotNull] NxParser.IdAtomContext context) { return this.GetVariable(context.ID().GetText(), context); }

    ///
    /// Visitor logic overrides
    /// 
    public override NxValue VisitChildren(IRuleNode node)
    {
        // Don't burn your fingers - this is extremely hot!

        var val = DefaultResult;
        int childCount = node.ChildCount;

        for (int i = 0; i < childCount; i++)
        {
            var child = node.GetChild(i);

            // We skip over ";" so that the return value does not get ignored
            if (child.Payload is IToken token && token.Type == NxParser.SCOL)
            {
                break;
            }

            val = Visit(child);
        }

        return val;
    }

    public override NxValue Visit(IParseTree tree)
    {
        // If we're in a function, return early if we have a return value
        // If you thought VistChildren() was hot - this is even hotter!

        if (this.Fn is not null && this.Fn.ReturnValue is not null)
        {
            // Use return value if there is one!
            return this.Fn.ReturnValue;
        }

        return tree.Accept(this);
    }

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

    protected virtual NxEvalVisitor NewScope()
    {
        return new NxEvalVisitor(this);
    }
}