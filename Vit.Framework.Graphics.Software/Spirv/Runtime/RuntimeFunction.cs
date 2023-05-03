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

	RuntimeScope createScope ( ref ShaderMemory memory ) {
		var scope = new RuntimeScope();
		foreach ( var (id, var) in parentScope.VariableInfo ) {
			scope.VariableInfo.Add( id, var );
		}
		foreach ( var (id, type) in locals ) {
			var variable = memory.StackAlloc( type );
			scope.VariableInfo.Add( id, variable );
#if SHADER_DEBUG
			memory.AddDebug( new() {
				Variable = variable,
				Name = $"Local %{id}"
			} );
#endif
		}

		return scope;
	}

	public void Call ( ShaderMemory memory ) {
		var scope = createScope( ref memory );

		var instructions = source.Instructions;
		var length = instructions.Count;
		while ( scope.CodePointer < length ) {
			var instruction = instructions[scope.CodePointer++];
			instruction.Execute( scope, memory );
		}

		scopePool.Push( scope );
	}
}
