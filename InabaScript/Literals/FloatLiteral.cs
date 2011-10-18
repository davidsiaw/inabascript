using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InabaScript {
    public class FloatLiteral : IExpression {

        internal FloatLiteral(string number) {
            Number = double.Parse(number);
            Type = new RealRangeType();
        }

        public double Number { get; private set; }

        public IType Type
        {
            get;
            private set;
        }
    }
}
