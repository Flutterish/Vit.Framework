using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Graphics.Software.Shaders.Execution;

public class VariableDeclaration {
	public uint Name;
	public uint DataType;
	Dictionary<uint, DataType> DataTypes;

	public VariableDeclaration ( uint name, uint dataType, Dictionary<uint, DataType> dataTypes ) {
		Name = name;
		DataType = dataType;
		DataTypes = dataTypes;
	}

	public override string ToString () {
		return $"var_{Name}";
	}
}
