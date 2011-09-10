using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InabaScript {

	public class FunctionBody {

		internal FunctionBody(List<VariableDeclaration> parameters, List<IStatement> statements, Scope scope) {

			Parameters = parameters;
			Statements = statements;
            Scope = scope;
		}

		public List<VariableDeclaration> Parameters { get; private set; }
		public List<IStatement> Statements { get; private set; }
        public Scope Scope { get; private set; }

	}
}
