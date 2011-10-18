using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InabaScript
{
    class TypeDeclaration : IStatement, ISymbol
    {
        public TypeDeclaration(string str)
        {
            Name = str;
            type = new SetType(str);
        }

        SetType type;


        public void Add(string str)
        {
            members.Add(str);
        }

        public List<string> members = new List<string>();
        public string Name { get; private set; }


        public IType Type
        {
            get
            {
                return new TypeType(type);
            }
        }
    }
}
