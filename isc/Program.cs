using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InabaScript;

// Inabascript compiler

namespace isc {
    class Program {


        static void Main(string[] args) {

            InabaScriptSource iss = new InabaScriptSource(@"..\..\..\testsources\simple.is");

			string mainfunc = "int main(int argc, char** argv)\n{";

			foreach (IStatement statement in iss.statements) {
				if (statement is VariableDeclaration) {
					VariableDeclaration vdecl = (statement as VariableDeclaration);
					string type = "";
					if (vdecl.Initializer.Type is IntegerType) {
						IntegerType it = vdecl.Initializer.Type as IntegerType;
						if (it.Min > 0) {
							if (it.Max <= byte.MaxValue) {
								type = "unsigned char";
							} else if (it.Max <= ushort.MaxValue) {
								type = "unsigned short";
							} else if (it.Max <= uint.MaxValue) {
								type = "unsigned int";
							} else {
								type = "unsigned long long";
							}
						} else {
							if (it.Max <= sbyte.MaxValue && it.Min >= sbyte.MinValue) {
								type = "char";
							} else if (it.Max <= short.MaxValue && it.Min >= short.MinValue) {
								type = "short";
							} else if (it.Max <= int.MaxValue && it.Min >= int.MinValue) {
								type = "int";
							} else {
								type = "long long";
							}
						}
					} else {
						throw new Exception("Unknown type!");
					}
					string name = vdecl.Name;
					string initializer = TranslateExpression(vdecl.Initializer);
					mainfunc += "\n\t" + type + " " + name + " = " + initializer + ";";
				}
			}

			mainfunc += "\n}\n";

			Console.WriteLine(mainfunc);

			Console.ReadKey();

        }

		private static string TranslateExpression(IExpression expression) {
			string initializer = "";

			if (expression is IntegerLiteral) {
				initializer = (expression as IntegerLiteral).Value.ToString();
			} else if (expression is Referencer) {
				initializer = (expression as Referencer).Name;
			} else if (expression is FunctionCall) {
				FunctionCall fc = expression as FunctionCall;
				initializer = TranslateExpression(fc.LeftSideExpression) + "(" + string.Join(", ", fc.Parms.Select(x=>TranslateExpression(x)).ToArray()) + ")";
			} else {
				throw new Exception("Unknown expression!");
			}
			return initializer;
		}
    }
}
