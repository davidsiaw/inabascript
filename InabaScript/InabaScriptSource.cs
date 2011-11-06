using System;
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

	public class VariableDeclaration : IStatement, IDeclaration {

		IExpression initializer;
		string name;
		public VariableDeclaration(string name, IExpression initializer) {
			this.name = name;
			this.initializer = initializer;
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

	public class FunctionType : IType {

		IType returntype;
		IType[] parameterTypes;
		public FunctionType(IType returntype, IType[] parameterTypes) {
			this.returntype = returntype;
			this.parameterTypes = parameterTypes;
		}

		public IType ReturnType { get { return returntype; } }
		public IType[] ParameterTypes { get { return parameterTypes; } }

		public bool IsAssignableTo(IType type) {
			if (type is FunctionType) {
				FunctionType ft = type as FunctionType;
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

	public class FunctionDeclaration : IDeclaration {

		string name;
		IType functionType;
		public FunctionDeclaration(string name, IType returntype, IType[] parameterTypes) {
			this.name = name;
			this.functionType = new FunctionType(returntype, parameterTypes);
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
			if (!(lhs.Type is FunctionType)) {
				throw new Exception(lhs + " is not a function!");
			}
			FunctionType type = lhs.Type as FunctionType;
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
			get { return (lhs.Type as FunctionType).ReturnType; }
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
			intrinsics = new Scope(new FunctionDeclaration("printversion", new NothingType(), new IType[0]), intrinsics);
            intrinsics = new Scope(new FunctionDeclaration("getversion", new IntegerType(0, uint.MaxValue), new IType[] { new IntegerType(0,100) }), intrinsics);

            Scanner.Init(filename);
			Parser.iss = this;
            Parser.Parse();
        }
    }
}
