namespace Net.Astropenguin.Helpers
{
	public class AsyncTryOut<T>
	{
		public bool Status = false;
		public T Out;

		public AsyncTryOut() { }

		public AsyncTryOut( bool b, T r )
		{
			Status = b;
			Out = r;
		}

		public static bool operator true( AsyncTryOut<T> s ) { return s.Status; }
		public static bool operator false( AsyncTryOut<T> s ) { return s.Status; }
	}
}
