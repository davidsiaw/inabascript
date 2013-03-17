using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace InabaScript
{
    public interface IElement
    {
        string Name { get; }
    }

    public interface IType : IElement
    {
        bool Accepts(IType rtype);
        IType DeTuple();
    }


    
    public interface IDeclaration : IElement
    {
    }

    public interface IExpression : IStatement
    {
        IType Type { get; }
    }

    public interface IVariable
    {
        string Name { get; }
        IType Type { get; }
    }

    public interface IStatement
    {
    }
    

    class IntegerType : IType
    {
        public readonly long max;
        public readonly long min;

        public IntegerType(long num)
        {
            max = num;
            min = num;
        }

        public IntegerType(long min, long max)
        {
            this.max = max;
            this.min = min;
        }

        public bool IsWithin(IntegerRange other)
        {
            return (min >= other.min && max <= other.max);
        }

        public bool Accepts(IType rtype)
        {
            if (rtype.DeTuple() is IntegerType)
            {
                var inttype = (IntegerType)rtype.DeTuple();
                if (inttype.min >= min && inttype.max <= max)
                {
                    return true;
                }
            }
            return false;
        }

        public string Name
        {
            get
            {
                return min + ".." + max;
            }
        }

        public override string ToString()
        {
            return "integer:" + Name;
        }

        public IType DeTuple()
        {
            return this;
        }
    }

    interface ISetMember
    {
        string Name { get; }
    }

    class ArrayType : IType
    {
        public readonly IType innerType;
        public readonly IntegerType sizeType;

        public ArrayType(IType innerType, IntegerType sizeType)
        {
            this.innerType = innerType;
            this.sizeType = sizeType;
        }

        public bool Accepts(IType rtype)
        {
            if (rtype is ArrayType)
            {
                ArrayType arrType = rtype as ArrayType;
                // special case
                if (arrType.sizeType.min == 0 && arrType.sizeType.max == 0 && arrType.innerType is UnknownType)
                {
                    return true;
                }

                return innerType.Accepts(arrType.innerType) && sizeType.Accepts(arrType.sizeType);
            }
            return false;
        }

        public string Name
        {
            get
            {
                return "[" + innerType.Name + "<" + sizeType.Name + ">" + "]";
            }
        }

        public override string ToString()
        {
            return "array:" + Name;
        }

        public IType DeTuple()
        {
            return this;
        }
    }

    class UnknownType : IType
    {
        public bool Accepts(IType exp)
        {
            return true;
        }

        public string Name
        {
            get { return "unknown"; }
        }

        public IType DeTuple()
        {
            return this;
        }
    }

    public class FunctionType : IType
    {
        public readonly IType ArgumentType;
        public readonly IType ReturnType;

        public FunctionType(IType argumentType, IType returnType)
        {
            this.ArgumentType = argumentType;
            this.ReturnType = returnType;
        }

        public bool Accepts(IType rtype)
        {
            return rtype.Name == Name;
        }

        public string Name
        {
            get
            {
                return ArgumentType.Name + "->" + ReturnType.Name;
            }
        }

        public override string ToString()
        {
            return "function:" + Name;
        }

        public IType DeTuple()
        {
            return this;
        }
    }


    class FunctionCall : IExpression, IStatement
    {

        public readonly IExpression Rhs;
        public readonly IExpression Lhs;

        public FunctionCall(IExpression lhs, IExpression rhs)
        {
            if (!(lhs.Type.DeTuple() is FunctionType))
            {
                throw new Exception(lhs + " not a function!");
            }

            FunctionType lhsType = (FunctionType)lhs.Type.DeTuple();
            if (!lhsType.ArgumentType.Accepts(rhs.Type))
            {
                throw new Exception(lhs.Type + " does not accept " + rhs.Type);
            }

            Type = lhsType.ReturnType;

            Lhs = lhs;
            Rhs = rhs;
        }

        public FunctionCall(IExpression lhs, IExpression rhs, IType rettype)
        {
            Lhs = lhs;
            Rhs = rhs;
            Type = rettype;
        }

        public IType Type
        {
            get;
            private set;
        }

    }

    public class FunctionLiteral : IExpression
    {
        public readonly List<IStatement> Statements;
        public readonly MultiVariableDeclaration Parameter;
        public readonly MultiVariableDeclaration ReturnValue;

        public FunctionLiteral(MultiVariableDeclaration returnValue, MultiVariableDeclaration parameter, List<IStatement> statements)
        {
            this.Type = new FunctionType(parameter.Type, returnValue.Type);
            this.Statements = statements;
            this.Parameter = parameter;
            this.ReturnValue = returnValue;
        }

        public IType Type
        {
            get;
            private set;
        }

        public IExpression Bind(Scope scope)
        {
            BoundFunctionLiteral bfl = new BoundFunctionLiteral(this, scope);
            scope.RegisterFuncLiteral(bfl);
            return bfl;
        }
    }


    public class BoundFunctionLiteral : IExpression
    {
        FunctionLiteral prototype;
        Scope scopeAtPrototype;
        Dictionary<string, FunctionLiteral> boundFunctions = new Dictionary<string, FunctionLiteral>();
        public BoundFunctionLiteral(FunctionLiteral prototype, Scope scopeAtPrototype)
        {
            this.prototype = prototype;
            this.scopeAtPrototype = scopeAtPrototype;
        }

        public class BoundFunctionType : IType
        {
            BoundFunctionLiteral bfl;

            public BoundFunctionType(BoundFunctionLiteral bfl)
            {
                this.bfl = bfl;
            }

            public bool Accepts(IType rtype)
            {
                return bfl.prototype.Type.Accepts(rtype);
            }

            public string Name
            {
                get { return bfl.prototype.Type.Name; }
            }

            public IType ReturnType(IExpression rhs)
            {
                return bfl.boundFunctions[rhs.Type.Name].ReturnValue.Type;
            }

            public IType DeTuple()
            {
                return this;
            }
        }


        public IType Type
        {
            get { return new BoundFunctionType(this); }
        }

    }

    class ArrayLiteral : IExpression
    {

        List<IExpression> contents;

        public ArrayLiteral(List<IExpression> contents)
        {
            Type = new ArrayType(new UnknownType(), new IntegerType(0));
            this.contents = contents;
            if (contents.Count > 0)
            {
                Type = new ArrayType(contents[0].Type, new IntegerType(contents.Count));
            }
        }

        public IType Type { get; set; }

    }

    class IntegerRange : IDeclaration, IExpression, ISetMember
    {

        public readonly long min, max;

        public IntegerRange(long min, long max)
        {
            this.min = min;
            this.max = max;
            if (min == max)
            {
                Type = new IntegerType(min);
            }
            else
            {
                Type = new ArrayType(new IntegerType(min, max), new IntegerType(max - min + 1));
            }
        }

        public IType Type
        {
            get;
            private set;
        }

        public string Name
        {
            get
            {
                return min + ".." + max;
            }
        }

        public IExpression Bind(Scope scope)
        {
            return new IntegerRange(min, max);
        }
    }

    //class SymbolType : IType
    //{

    //    public SymbolType(string name)
    //    {
    //        this.Name = name;
    //    }

    //    public bool Accepts(IType rtype)
    //    {
    //        return rtype.ToString() == rtype.ToString();
    //    }

    //    public string Name
    //    {
    //        get;
    //        private set;
    //    }

    //    public override string ToString()
    //    {
    //        return "symbol:" + Name;
    //    }
    //}

    class Symbol : IDeclaration, IExpression, ISetMember
    {

        public Symbol(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return "symbol:" + Name;
        }

        public IType Type
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public IExpression Bind(Scope scope)
        {
            return this;
        }
    }

    class TupleType : IType
    {

        public IType DeTuple()
        {
            if (types.Count == 1)
            {
                return types[0].DeTuple();
            }
            return this;
        }

        public readonly List<IType> types;

        public TupleType(List<IType> types)
        {
            this.types = types;
        }

        public bool Accepts(IType type)
        {
            if (types.Count == 1)
            {
                // special case.
                return types[0].Accepts(type);
            }
            if (type is TupleType)
            {
                if (types.Count == 0)
                {
                    return (type as TupleType).types.Count == 0;
                }
                TupleType other = (TupleType)type;
                // This (receiving tuple) must be shorter than the tuple assigning in
                for (int i = 0; i < types.Count; i++)
                {
                    if (other.types.Count == i || !types[i].Accepts(other.types[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public string Name
        {
            get
            {
                if (types.Count == 1)
                {
                    // special case.
                    return types[0].Name;
                }
                return "(" + string.Join(",", types.Select(x => x.Name)) + ")";
            }
        }

        public override string ToString()
        {
            if (types.Count == 1)
            {
                // special case.
                return types[0].ToString();
            }
            return "tuple:" + Name;
        }
    }

    class TupleExpression : IExpression
    {

        List<IExpression> expressions;

        public TupleExpression(List<IExpression> exprs)
        {
            expressions = exprs;
            Type = new TupleType(exprs.Select(x => x.Type).ToList());
        }

        public IType Type
        {
            get;
            private set;
        }

    }

    public class VariableDeclaration : IExpression, IDeclaration, IVariable
    {

        public IType Type
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public MultiVariableDeclaration MVD
        {
            get;
            set;
        }

        public VariableDeclaration(string name, IType type)
        {
            this.Name = name;
            this.Type = type;
        }

        public override string ToString()
        {
            return "var:" + Name;
        }

        public static void Scopify(ref Scope scope, List<VariableDeclaration> declarations)
        {
            foreach (var decl in declarations)
            {
                scope = new Scope(scope, decl);
            }
        }

        public static VariableDeclaration Find(ref Scope scope, VariableDeclaration vardecl, string name, bool isDeclaration)
        {
            if (vardecl != null)
            {
                return vardecl;
            }

            IDeclaration decl = scope.GetDeclarationForOrNull(name);
            if (decl == null && isDeclaration)
            {
                var vd = new VariableDeclaration(name, new UnknownType());
                scope = new Scope(scope, vd);
                return vd;
            }
            if (decl is VariableDeclaration)
            {
                return (VariableDeclaration)decl;
            }
            throw new Exception(name + " not a variable");
        }

        public IExpression Bind(Scope scope)
        {
            return (IExpression)scope.GetDeclarationFor(Name);
        }
    }

    public class MultiVariableDeclaration : IStatement, IDeclaration, IVariable
    {
        public readonly List<VariableDeclaration> Declarations = new List<VariableDeclaration>();
        public readonly IExpression initializer;

        public MultiVariableDeclaration(List<VariableDeclaration> declarations, IExpression rexp)
        {
            this.Declarations = declarations;
            this.Type = new TupleType(declarations.Select(x =>
            {
                x.MVD = this;
                return x.Type;
            }).ToList());

            if (rexp != null)
            {
                if (!Type.Accepts(rexp.Type))
                {
                    throw new Exception("var type: " + Type + " does not accept: " + rexp.Type);
                }

                // closer checks
                List<IType> types = new List<IType>();
                if (rexp.Type is TupleType)
                {
                    types = new List<IType>((rexp.Type as TupleType).types);
                }
                else
                {
                    types.Add(rexp.Type);
                }

                if (declarations.Count == 1)
                {
                    if (declarations[0].Type is UnknownType)
                    {
                        declarations[0].Type = new TupleType(types);
                    }
                }
                else
                {
                    for (int i = 0; i < types.Count; i++)
                    {
                        if (declarations[i].Type == null || declarations[i].Type is UnknownType)
                        {
                            declarations[i].Type = types[i];
                        }
                    }
                }

                this.Type = new TupleType(types);
            }
            this.initializer = rexp;
        }

        public string Name
        {
            get { return string.Join(",", Declarations.Select(x => x.Name)); }
        }

        public override string ToString()
        {
            return "multivar:" + Name;
        }

        public IType Type
        {
            get;
            private set;
        }

    }

    class SetDeclaration : IStatement, IType, IDeclaration
    {
        public SetDeclaration(string name, IEnumerable<ISetMember> elements)
        {
            foreach (var ele in elements)
            {
                theset.Add(ele.Name, ele);
                if (ele is Symbol)
                {
                    (ele as Symbol).Type = this;
                }
            }
            this.Name = name;
        }

        public override string ToString()
        {
            return "set:" + Name;
        }

        public string Name
        {
            get;
            set;
        }

        Dictionary<string, ISetMember> theset = new Dictionary<string, ISetMember>();

        public bool Accepts(IType type)
        {
            if (type.ToString() == this.ToString())
            {
                return true;
            }
            // only accepts members
            foreach (var kvpair in theset)
            {
                if (kvpair.Value is IntegerRange)
                {
                    if (type is IntegerType)
                    {
                        if ((type as IntegerType).IsWithin(kvpair.Value as IntegerRange))
                        {
                            return true;
                        }
                    }

                }
                else if (kvpair.Value is Symbol)
                {
                    if (type.ToString() == kvpair.Value.ToString())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public IStatement Bind(ref Scope scope)
        {
            // This thing would be full declared
            scope = new Scope(scope, this);
            return this;
        }

        public IType DeTuple()
        {
            return this;
        }
    }

    public class Scope
    {
        readonly Scope parent;
        readonly string name;
        readonly Dictionary<string, List<BoundFunctionLiteral>> funcTypeToLiterals;
        readonly Dictionary<string, IDeclaration> scopeTable;
        readonly Dictionary<string, IType> typeTable;

        public Scope()
        {
            this.scopeTable = new Dictionary<string, IDeclaration>();
            this.typeTable = new Dictionary<string, IType>();
            this.funcTypeToLiterals = new Dictionary<string, List<BoundFunctionLiteral>>();
        }

        public Scope(Scope parent)
        {
            this.scopeTable = new Dictionary<string, IDeclaration>(parent.scopeTable);
            this.typeTable = new Dictionary<string, IType>(parent.typeTable);
            this.funcTypeToLiterals = new Dictionary<string, List<BoundFunctionLiteral>>(parent.funcTypeToLiterals);
        }

        public Scope(Scope parent, IDeclaration decl)
        {
            this.name = decl.Name;
            this.parent = parent;
            this.scopeTable = parent.scopeTable;
            this.funcTypeToLiterals = parent.funcTypeToLiterals;
           
            if (scopeTable.ContainsKey(decl.Name))
            {
                throw new Exception("already has " + decl);
            }
            this.typeTable = parent.typeTable;
            scopeTable.Add(decl.Name, decl);
            if (decl is IType)
            {
                this.typeTable.Add(decl.Name, (IType)decl);
            }
        }

        public void RegisterFuncLiteral(BoundFunctionLiteral literal)
        {
            List<BoundFunctionLiteral> list;
            if (!funcTypeToLiterals.TryGetValue(literal.Type.Name, out list))
            {
                list = new List<BoundFunctionLiteral>();
                funcTypeToLiterals[literal.Type.Name] = list;
            }
            list.Add(literal);
        }

        public IDeclaration GetDeclarationFor(string ident)
        {
            IDeclaration decl = GetDeclarationForOrNull(ident);
            if (decl != null)
            {
                return decl;
            }
            throw new Exception(ident + " not found");
        }

        public IDeclaration GetDeclarationForOrNull(string ident)
        {
            IDeclaration decl;
            if (scopeTable.TryGetValue(ident, out decl))
            {
                return decl;
            }
            return null;
        }

        public IType GetTypeDeclaration(string type)
        {
            return typeTable[type];
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string file = @"D:\VS\Programs\InabaScript\InabaScript\basicExpressions.is";
            string[] lines = File.ReadAllLines(file);
            Parser p = new Parser(new Scanner(file));

            try
            {
                p.Parse();
            }
            catch (Exception e)
            {
                for (int i = Math.Max(0, p.la.line - 3); i < Math.Min(p.scanner.lines.Count, p.la.line + 2); i++)
                {
                    WriteColoredLine(lines, p, i);
                    if (i == p.la.line - 1)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(Enumerable.Range(1, p.la.col - 1).Aggregate("", (x, y) => x + " ") + "^");
                    }
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(e.Message);
            }

            //for (int i = 0; i < lines.Length; i++)
            //{
            //    WriteColoredLine(lines, p, i);
            //}

            List<IStatement> boundStatements = new List<IStatement>();
            Scope s = new Scope();

            foreach (var statement in p.statements)
            {
                if (statement is MultiVariableDeclaration)
                {
                }
            }

            Console.ReadKey();
        }

        private static void WriteColoredLine(string[] lines, Parser p, int line)
        {
            Console.WriteLine(lines[line]);
            Console.CursorTop--;
            var tokens = p.scanner.lines[line];
            for (int i = 0; i < tokens.Count; i++)
            {
                var x = tokens[i];
                Console.CursorLeft = x.col - 1;
                switch (x.kind)
                {
                    case Parser._ident:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    case Parser._typeident:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;
                    case Parser._integer:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    case Parser._validStringLiteral:
                        Console.ResetColor();
                        break;
                    case Parser._arrow:
                        if (tokens[i + 1].kind != Parser._typeident)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                        }
                        break;
                    case Parser._colon:
                        Console.ResetColor();
                        break;
                    default:
                        if (x.val == "{" || x.val == "}" || x.val == "[" || x.val == "]")
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else if (x.val == "<" || x.val == ">" || x.val == "(" || x.val == ")" || x.val == "[" || x.val == "]" || x.val == ",")
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else if (x.val == "..")
                        {
                            Console.ResetColor();
                        }
                        else if (x.val == "Int" && tokens[i + 1].val == "{")
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                        }
                        else if (x.val == "var" || x.val == "set")
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                        }
                        else
                        {
                            Console.ResetColor();
                        }

                        break;
                }
                Console.Write(x.val);
            }
            Console.WriteLine();
        }

    }
}
