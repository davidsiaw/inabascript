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

			string mainfunc = "int main(int argc, char** argv) {";

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
							if (it.Max <= sbyte.MaxValue || it.Min >= sbyte.MinValue) {
								type = "char";
							} else if (it.Max < short.MaxValue || it.Min >= short.MinValue) {
								type = "short";
							} else if (it.Max < int.MaxValue || it.Min >= int.MinValue) {
								type = "int";
							} else {
								type = "long long";
							}
						}
					} else {
						throw new Exception("Unknown type!");
					}
					string name = vdecl.Name;
					string initializer = "";

					if (vdecl.Initializer is IntegerLiteral) {
						initializer = (vdecl.Initializer as IntegerLiteral).Value.ToString();
					} else if (vdecl.Initializer is Referencer) {
						initializer = (vdecl.Initializer as Referencer).Name;
					} else {
						throw new Exception("Unknown expression!");
					}
					mainfunc += "\n\t" + type + " " + name + " = " + initializer + ";";
				}
			}

			mainfunc += "\n}\n";

			Console.WriteLine(mainfunc);

			Console.ReadKey();

        }
    }
}
