namespace Net.Astropenguin.UnitTest
{
	public class TestCompleteArgs
	{
		public TestResult Result { get; private set; }
		public TestCompleteArgs( TestResult t )
		{
			Result = t;
		}
	}
}