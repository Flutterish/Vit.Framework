using System.Runtime.Serialization;

namespace Vit.Framework.Exceptions;

public class InvalidStateException : Exception {
	public InvalidStateException () {
	}

	public InvalidStateException ( string? message ) : base( message ) {
	}

	public InvalidStateException ( string? message, Exception? innerException ) : base( message, innerException ) {
	}

	protected InvalidStateException ( SerializationInfo info, StreamingContext context ) : base( info, context ) {
	}
}
