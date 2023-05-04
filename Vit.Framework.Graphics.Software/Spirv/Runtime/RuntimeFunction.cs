using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Spirv.Instructions;

namespace Vit.Framework.Graphics.Software.Spirv.Runtime;

public class RuntimeFunction {
	RuntimeScope parentScope;
	List<(uint id, IRuntimeType type)> locals = new();
	Stack<RuntimeScope> scopePool = new();
	Function source;
	public readonly MemoryDebugFrame DebugFrame = new() { Name = "Function" };

	public RuntimeFunction ( RuntimeScope parentScope, Function source ) {
		this.source = source;
		this.parentScope = parentScope;
		ShaderMemory memory = default;
		foreach ( var (id, intermediate) in source.Intermediates ) {
			locals.Add(( id, intermediate.Type.GetRuntimeType() ));
			DebugFrame.Add( new() {
				Variable = memory.StackAlloc( intermediate.Type.GetRuntimeType() ),
				Name = $"Local %{id}"
			} );
		}
	}

	RuntimeScope createScope ( ref ShaderMemory memory ) {
		if ( !scopePool.TryPop( out var scope ) )
			scope = new() { Opaques = parentScope.Opaques };
		scope.CodePointer = 0;
		scope.VariableInfo.Clear();

		foreach ( var (id, var) in parentScope.VariableInfo ) {
			scope.VariableInfo.Add( id, var );
		}
		foreach ( var (id, type) in locals ) {
			var variable = memory.StackAlloc( type );
			scope.VariableInfo.Add( id, variable );
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
