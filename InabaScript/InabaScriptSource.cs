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

        public InabaScriptSource(string filename)
        {
            Scanner.Init(filename);
			Parser.iss = this;
            Parser.Parse();
        }
    }
}
