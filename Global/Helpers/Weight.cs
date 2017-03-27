namespace Net.Astropenguin.Helpers
{
	public class Weight<T>
	{
		public T Freight;
		public int Factor;

		public Weight( T Freight, int Factor )
		{
			this.Freight = Freight;
			this.Factor = Factor;
		}
	}
}