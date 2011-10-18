using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InabaScript {

    public interface IType
    {   
    }

    public class TypeType : IType
    {
        public TypeType(IType type)
        {
            this.type = type;
        }
        IType type;

    }

    public class StringType : IType
    {
    }

    public class RealRangeType : IType
    {
    }

    public class IntegerRangeType : IType
    {
    }

    public class SetType : IType
    {
        public SetType(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }

    public class ParameterType : IType
    {
        public ParameterType() { }
    }

    public class FunctionType : IType
    {
        List<IType> parameterTypes = new List<IType>();
        public IType ReturnType { get; private set; }

        public FunctionType(List<IType> param, IType returntype)
        {
            parameterTypes = param;
            this.ReturnType = returntype;
        }
    }

    public class ObjectType : IType
    {
    }



	public interface ISymbol {
		string Name { get; }
        IType Type { get; }
	}
}
