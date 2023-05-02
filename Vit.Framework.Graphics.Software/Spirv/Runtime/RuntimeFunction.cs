using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Spirv.Instructions;

namespace Vit.Framework.Graphics.Software.Spirv.Runtime;

public class RuntimeFunction {
	RuntimeScope parentScope;
	List<(uint id, IRuntimeType type)> locals = new();
	Stack<RuntimeScope> scopePool = new();
	Function source;

	public RuntimeFunction ( RuntimeScope parentScope, Function source ) {
		this.source = source;
		this.parentScope = parentScope;
		foreach ( var (id, intermediate) in source.Intermediates ) {
			locals.Add(( id, intermediate.Type.GetRuntimeType() ));
		}
	}

	RuntimeScope createScope () {
		var scope = new RuntimeScope();
		foreach ( var (id, var) in parentScope.Variables ) {
			scope.Variables.Add( id, var );
		}
		foreach ( var (id, type) in locals ) {
			scope.Variables.Add( id, type.CreateVariable() );
		}

		return scope;
	}

	public void Call () {
		if ( !scopePool.TryPop( out var scope ) ) {
			scope = createScope();
		}
		scope.CodePointer = 0;

		var instructions = source.Instructions;
		var length = instructions.Count;
		while ( scope.CodePointer < length ) {
			var instruction = instructions[scope.CodePointer++];
			instruction.Execute( scope );
		}

		scopePool.Push( scope );
	}
}
