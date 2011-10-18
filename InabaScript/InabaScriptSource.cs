using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InabaScript;
using System.IO;

namespace InabaScript
{
    public class InabaScriptSource
    {
        class TopLevel : ISymbol
        {
            public string Name
            {
                get { return ""; }
            }

            public IType Type
            {
                get
                {
                    return null;
                }
            }
        }

        public List<IStatement> stmts = new List<IStatement>();
        public Scope scope;

        public InabaScriptSource(string filename)
        {
            scope = new Scope(new TopLevel(), null);
            Scanner.Init(filename);
            Parser.iss = this;
            Parser.Parse();
        }

        public static void Error(string errmsg)
        {
            Console.WriteLine(errmsg);
            throw new Exception(errmsg);
        }
    }

}
