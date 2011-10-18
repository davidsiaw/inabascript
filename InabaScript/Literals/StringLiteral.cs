using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InabaScript {
    public class StringLiteral : IExpression {

        internal StringLiteral(string str) {
            String = str;
            Type = new StringType();
        }

        public string String { get; private set; }


        public IType Type
        {
            get;
            private set;
        }
    }
}
