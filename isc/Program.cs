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




			Console.ReadKey();
        }
    }
}
