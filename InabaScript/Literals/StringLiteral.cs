using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InabaScript {
    public class StringLiteral : IExpression {

        internal StringLiteral(string str) {
            String = str;
        }

        public string String { get; private set; }

    }
}
