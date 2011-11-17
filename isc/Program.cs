﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InabaScript;
using System.IO;

// Inabascript compiler

namespace isc {
    class Program {

        class CSource
        {
            public CSource(string source)
            {
                this.source = source;
                progname = Path.GetFileNameWithoutExtension(source);
                iss = new InabaScriptSource(source);
            }

            string progname;
            string source;
            InabaScriptSource iss;

            public void WriteOut(DirectoryInfo directory)
            {
                if (!directory.Exists)
                {
                    directory.Create();
                }

                string main = Path.Combine(directory.FullName, progname + ".c");

                WriteSourceFile(main);
                WriteMakeFile(directory);
            }

            private void WriteMakeFile(DirectoryInfo directory)
            {
                using (StreamWriter sw = new StreamWriter(Path.Combine(directory.FullName, "Makefile")))
                {
                    sw.WriteLine("# This file was generated by InabaScriptCompiler");
                    sw.WriteLine("# Program name: {0}", progname);
                    sw.WriteLine("# Generation time: {0} {1}", DateTime.Now.ToString(), TimeZone.CurrentTimeZone.StandardName);
                    sw.WriteLine("");

                    sw.WriteLine("CC = gcc");
                    sw.WriteLine("CFLAGS = -Wall -pedantic -g -std=c99");
                    sw.WriteLine("LDFLAGS = ");

                    sw.WriteLine("");
                    sw.WriteLine("# target: all - build {0}", progname);
                    sw.WriteLine("all: {0}", progname);
                    sw.WriteLine("\t# -----------------------------------");
                    sw.WriteLine("\t# {0}", progname);
                    sw.WriteLine("\t# Written in inabascript");
                    sw.WriteLine("\t# Transformation done with InabaScriptCompiler");

                    sw.WriteLine("");
                    sw.WriteLine("{0}: {0}.o", progname);
                    sw.WriteLine("\t# Linking {0}...", progname);
                    sw.WriteLine("\t$(CC) $(LDFLAGS) -o $@ $^");

                    sw.WriteLine("");
                    sw.WriteLine("{0}.o: {1}.c", progname, progname);
                    sw.WriteLine("\t# Making {0}...", progname);
                    sw.WriteLine("\t$(CC) $(CFLAGS) -c -o $@ $<");

                    sw.WriteLine("");
                    sw.WriteLine("# target: clean - Delete all files generated by make all");
                    sw.WriteLine("clean:");
                    sw.WriteLine("\trm -f {0} {0}.o", progname, progname);

                    sw.WriteLine("");
                    sw.WriteLine("# target: help - Display callable targets");
                    sw.WriteLine("help:");
                    sw.WriteLine("\tcat Makefile | egrep \"^# target:\"");


                    sw.WriteLine("");
                    sw.WriteLine(".PHONY: clean");

                }
            }



            List<string> functions = new List<string>();
            Dictionary<StaticFunctionType, KeyValuePair<string, string>> funcTypeDefs = new Dictionary<StaticFunctionType, KeyValuePair<string, string>>();

            private void WriteSourceFile(string main)
            {
                string mainfunc = "int main(int argc, char** argv)\n{";

                WriteStatementList("", ref mainfunc, iss.statements);

                mainfunc += "\n\treturn 0;\n}\n";

                using (StreamWriter sw = new StreamWriter(main))
                {
                    sw.WriteLine("// Created from " + progname + ".is");
                    sw.WriteLine("// by InabaScriptCompiler");
                    sw.WriteLine("// on " + DateTime.Now + " " + TimeZone.CurrentTimeZone.StandardName);
                    sw.WriteLine();

                    foreach (var v in funcTypeDefs.Values)
                    {
                        sw.WriteLine("typedef {0};", v.Value);
                    }
                    sw.WriteLine("");

                    functions.ForEach(x => sw.WriteLine(x));
                    sw.WriteLine(mainfunc);
                }
            }

            private void WriteStatementList(string outerfuncname, ref string funcstr, List<IStatement> statements)
            {
                foreach (IStatement statement in statements)
                {
                    if (statement is VariableDeclaration)
                    {
                        VariableDeclaration vdecl = (statement as VariableDeclaration);

                        if (vdecl.Initializer is Function)
                        {
                            MakeFunction(vdecl.Name, outerfuncname, vdecl.Initializer as Function);
                        }
                        else
                        {
                            string type;
                            GetCType(vdecl.Initializer.Type, out type);
                            string name = vdecl.Name;
                            string initializer = TranslateExpression(vdecl.Initializer, outerfuncname);
                            funcstr += "\n\t" + type + " " + name + " = " + initializer + ";";
                        }
                    }
                    else if (statement is Invoker)
                    {
                        funcstr += "\n\t" + TranslateExpression((statement as Invoker).FuncCall, outerfuncname) + ";";
                    }
                    else if (statement is ReturnStatement)
                    {
                        ReturnStatement rs = statement as ReturnStatement;
                        funcstr += "\n\treturn " + TranslateExpression(rs.Expression, outerfuncname) + ";";
                    }
                    else
                    {
                        throw new Exception("unknown statement!");
                    }
                }
            }

            

            private void MakeFunction(string funcname, string outerfuncname, Function func)
            {
                StaticFunctionType sft = func.Type as StaticFunctionType;
                string returntype;
                GetCType(sft.ReturnType, out returntype);

                string funcDef = returntype + " " + funcname + outerfuncname + "(" + ")\n";
                funcDef += "{";
                WriteStatementList("_in_" + GetFunctionName(funcname, outerfuncname), ref funcDef, func.Statements);
                funcDef += "\n}\n";
                functions.Add(funcDef);
            }

            private static string GetFunctionName(string funcname, string outerfuncname)
            {
                return funcname + outerfuncname;
            }

            private void GetCType(IType t, out string type)
            {
                if (t is IntegerType)
                {
                    IntegerType it = t as IntegerType;
                    //if (it.Min > 0)
                    //{
                    //    if (it.Max <= byte.MaxValue)
                    //    {
                    //        type = "unsigned char";
                    //    }
                    //    else if (it.Max <= ushort.MaxValue)
                    //    {
                    //        type = "unsigned short";
                    //    }
                    //    else if (it.Max <= uint.MaxValue)
                    //    {
                    //        type = "unsigned int";
                    //    }
                    //    else
                    //    {
                            //type = "unsigned long long";
                            //suffix = "ULL";
                        //}
                    //}
                    //else
                    //{
                    //    if (it.Max <= sbyte.MaxValue && it.Min >= sbyte.MinValue)
                    //    {
                    //        type = "char";
                    //    }
                    //    else if (it.Max <= short.MaxValue && it.Min >= short.MinValue)
                    //    {
                    //        type = "short";
                    //    }
                    //    else if (it.Max <= int.MaxValue && it.Min >= int.MinValue)
                    //    {
                    //        type = "int";
                    //    }
                    //    else
                    //    {
                            type = "long long";
                    //    }
                    //}
                }
                else if (t is StringType)
                {
                    type = "const char*";
                }
                else if (t is NothingType)
                {
                    type = "void";
                }
                else if (t is StaticFunctionType)
                {
                    StaticFunctionType sft = t as StaticFunctionType;

                    if (!funcTypeDefs.ContainsKey(sft))
                    {
                        string rettype;
                        GetCType(sft.ReturnType, out rettype);
                        string alias = rettype.Replace(" ", "");
                        List<string> paramtypes = new List<string>();
                        foreach (IType pt in sft.ParameterTypes)
                        {
                            string paramtype;
                            GetCType(pt, out paramtype);
                            paramtypes.Add(paramtype);
                            alias += "_p_" + paramtype;
                        }
                        alias += "_func_t";

                        string typedef = rettype + " (*" + alias + ")" + "(" + string.Join(", ", paramtypes.ToArray()) + ")";
                        funcTypeDefs[sft] = new KeyValuePair<string, string>(alias, typedef);
                    }

                    type = funcTypeDefs[sft].Key;
                }
                else
                {
                    throw new Exception("Unknown type!");
                }
            }


            private string TranslateExpression(IExpression expression, string outerfuncname)
            {
                string initializer = "";

                if (expression is IntegerLiteral)
                {
                    initializer = (expression as IntegerLiteral).Value.ToString() + "LL";
                }
                else if (expression is StringLiteral)
                {
                    initializer = "\"" + (expression as StringLiteral).Value + "\"";
                }
                else if (expression is Referencer)
                {
                    initializer = (expression as Referencer).Name;
                }
                else if (expression is FunctionCall)
                {
                    FunctionCall fc = expression as FunctionCall;
                    initializer = TranslateExpression(fc.LeftSideExpression, outerfuncname) + "(" + string.Join(", ", fc.Parms.Select(x => TranslateExpression(x, outerfuncname)).ToArray()) + ")";
                }
                else if (expression is Function)
                {
                    Function func = expression as Function;
                    MakeFunction(func.Name, outerfuncname, func);
                    initializer = "" + GetFunctionName(func.Name, outerfuncname);
                }
                else
                {
                    throw new Exception("Unknown expression!");
                }
                return initializer;
            }
        }

        static void Main(string[] args) {

            string source = @"..\..\..\testsources\simple.is";

            DirectoryInfo di = new DirectoryInfo(@"D:\starlight\");
            CSource cs = new CSource(source);
            cs.WriteOut(di);

        }

    }
}
