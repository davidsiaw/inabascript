using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InabaScript {
    public class FloatLiteral : IExpression {

        internal FloatLiteral(string number) {
            Number = double.Parse(number);
        }

        public double Number { get; private set; }

    }
}
