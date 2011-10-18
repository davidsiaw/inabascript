using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InabaScript {

    public class ObjectDeclaration : IExpression {

        internal ObjectDeclaration() {
            Members = new List<VariableDeclaration>();
        }

        internal void AddMember(VariableDeclaration vardecl) {
            Members.Add(vardecl);
        }

        public List<VariableDeclaration> Members { get; private set; }


        public IType Type
        {
            get;
            private set;
        }
    }
}
