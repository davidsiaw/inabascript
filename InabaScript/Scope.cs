using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InabaScript
{
    public class Scope
    {
        public Scope(ISymbol symbol, Scope previous)
        {
            this.previous = previous;
            this.symbol = symbol;

            if (previous != null && previous.IdentifierDeclared(symbol.Name))
            {
                string errmsg = string.Format("\"{0}\" redefined!", symbol.Name);
                InabaScriptSource.Error(errmsg);
            }
        }

        ISymbol symbol;
        Scope previous;

        public bool IdentifierDeclared(string ident)
        {
            if (previous == null)
            {
                return false;
            }
            Scope current = this;
            while (current != null)
            {
                if (current.symbol.Name == ident)
                {
                    return true;
                }
                current = current.previous;
            } 
            return false;
        }
    }
}
