﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InabaScript;
using System.IO;

namespace InabaScript
{
	public interface IExpression {
		IType Type { get; }
	}

	public interface IStatement {
	}

	public interface IType {
		bool IsAssignableTo(IType type);
	}

	public class Referencer : IExpression {

		IDeclaration declaration;

		public Referencer(Scope scope, string identifier) {

			Scope s = Scope.FindDeclOfName(identifier, scope);
			if (s == null) {
				throw new Exception("Identifier " + identifier + " not found!");
			}
			declaration = s.Declaration;
		}

		public IType Type {
			get {
				return declaration.Type;
			}
		}

		public string Name {
			get {
				return declaration.Name;
			}
		}
	}

	public class IntegerType : IType {

		decimal min, max;
		public IntegerType(decimal min, decimal max) {
			this.min = min;
			this.max = max;
		}

		public decimal Min { get { return min; } }
		public decimal Max { get { return max; } }

		public bool IsAssignableTo(IType type) {
			if (type is IntegerType) {
				IntegerType it = type as IntegerType;
				if (min >= it.min && max <= it.max) {
					return true;
				}
			}
			return false;
		}
	}

    public class StringType : IType
    {
        public StringType()
        {
        }

        public bool IsAssignableTo(IType type)
        {
            if (type is StringType)
            {
                return true;
            }
            return false;
        }
    }


	public class IntegerLiteral : IExpression {
		IntegerType type;
		decimal value;
		public IntegerLiteral(decimal value) {
			if (decimal.Floor(value) != value) {
				throw new Exception("Not integer!");
			}
			this.value = value;
			this.type = new IntegerType(value, value);
		}

		#region IExpression Members

		public decimal Value {
			get {
				return value;
			}
		}

		public IType Type {
			get { return type; }
		}

		#endregion
	}

    public class StringLiteral : IExpression
    {
        string str;
        public StringLiteral(string str)
        {
            this.str = str;
        }

        public IType Type
        {
            get { return new StringType(); }
        }

        public string Value
        {
            get
            {
                return str;
            }
        }
    }

	public class VariableDeclaration : IStatement, IDeclaration {

		IExpression initializer;
		string name;
		public VariableDeclaration(string name, IExpression initializer) {
			this.name = name;
			this.initializer = initializer;

            if (initializer.Type is NothingType)
            {
                throw new Exception("Attempted to initialize a variable with nothing type");
            }
		}

		public IExpression Initializer { get { return initializer; } }
		public string Name { get { return name; } }
		public IType Type { get { return initializer.Type; } }
	}

	public class NothingType : IType {

		public bool IsAssignableTo(IType type) {
			return false;
		}
	}

	public class StaticFunctionType : IType {

		IType returntype;
		IType[] parameterTypes;
		public StaticFunctionType(IType returntype, IType[] parameterTypes) {
			this.returntype = returntype;
			this.parameterTypes = parameterTypes;
		}

		public IType ReturnType { get { return returntype; } }
		public IType[] ParameterTypes { get { return parameterTypes; } }

		public bool IsAssignableTo(IType type) {
			if (type is StaticFunctionType) {
				StaticFunctionType ft = type as StaticFunctionType;
				if (returntype.IsAssignableTo(ft.ReturnType)) {
					if (parameterTypes.Length <= ft.parameterTypes.Length) {
						for (int i = 0; i < parameterTypes.Length; i++) {
							if (!ft.parameterTypes[i].IsAssignableTo(parameterTypes[i])) {
								return false;
							}
						}
						return true;
					}
				}
			}
			return false;
		}
	}

    public class FunctionType : IType
    {
        
        public bool IsAssignableTo(IType type)
        {
            throw new NotImplementedException();

        }

        public IType GetCalledType()
        {
            throw new NotImplementedException();
        }
    }


    public class Function : IExpression, IDeclaration
    {
        string name;
        public Function(string name, List<string> parameternames)
        {
            this.name = name;
        }

        public void Add(IStatement statement)
        {
            statements.Add(statement);
        }

        List<IStatement> statements = new List<IStatement>();

        public List<IStatement> Statements
        {
            get
            {
                return statements;
            }
        }

        public string Name
        {
            get { return name; }
        }

        public IType Type
        {
            get {
                return new StaticFunctionType(new NothingType(), new IType[0]);
            }
        }
    }

	public class ForeignFunctionDeclaration : IDeclaration {

		string name;
		IType functionType;
		public ForeignFunctionDeclaration(string name, IType returntype, IType[] parameterTypes) {
			this.name = name;
			this.functionType = new StaticFunctionType(returntype, parameterTypes);
		}

		public string Name {
			get { return name; }
		}

		public IType Type {
			get { return functionType; }
		}
	}


	public class FunctionCall : IExpression {
		IExpression lhs;
		List<IExpression> parms;
		public FunctionCall(IExpression lhs, List<IExpression> parms) {


			if (!(lhs.Type is StaticFunctionType)) {
				throw new Exception(lhs + " is not a function!");
			}
			StaticFunctionType type = lhs.Type as StaticFunctionType;
			if (type.ParameterTypes.Length != parms.Count) {
				throw new Exception("not enough/too many parameters!");
			}
			for (int i = 0; i < parms.Count; i++) {
				if (!parms[i].Type.IsAssignableTo(type.ParameterTypes[i])) {
					throw new Exception("parameter " + i + " cannot be assigned to " + type.ParameterTypes[i]);
				}
			}
			this.lhs = lhs;
			this.parms = parms;
		}

		public IExpression LeftSideExpression {
			get {
				return lhs;
			}
		}

		public IExpression[] Parms {
			get {
				return parms.ToArray();
			}
		}

		public IType Type {
			get { return (lhs.Type as StaticFunctionType).ReturnType; }
		}
	}


	public interface IDeclaration {
		string Name { get; }
		IType Type { get; }
	}

	public class Scope {
		Scope parent;
		IDeclaration decl;

		public Scope(IDeclaration decl, Scope parent) {
			this.decl = decl;
			this.parent = parent;

			if (FindDeclOfName(decl.Name, parent) != null) {
				throw new Exception(decl.Name + " already defined!");
			}
		}

		public static Scope FindDeclOfName(string name, Scope startfrom) {
			Scope curr = startfrom;
			while (curr != null) {
				if (name == curr.decl.Name) {
					return curr;
				}
				curr = curr.parent;
			}
			return null;
		}

		public IDeclaration Declaration { get { return decl; } }

	}

    public class InabaScriptSource
    {
		public List<IStatement> statements = new List<IStatement>();
		public Scope intrinsics = null;

        public InabaScriptSource(string filename)
        {


            Scanner.Init(filename);
			Parser.iss = this;
            Parser.Parse();
        }
    }
}
