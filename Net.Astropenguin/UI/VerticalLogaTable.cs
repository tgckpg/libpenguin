using Net.Astropenguin.Logging;
using System;

namespace Net.Astropenguin.UI
{
	// Width Num of Samples, Precalculate Trim Length by Block height
	class VerticalLogaTable
	{
		public static readonly string ID = typeof( VerticalLogaTable ).Name;
		public bool Calibrated { get; private set; }
		public double FontSize { get; private set; }

		public int CertaintyLevel { get { return NumSamples; } }
		public int TrimLen { get; private set; }

		public double BlockHeight { get; private set; }
		public double Low { get; private set; }
		public double High { get; private set; }

		private int NumSamples = 0;

		public VerticalLogaTable( double FontSize )
		{
			BlockHeight = 0;
			Low = int.MaxValue;
			High = int.MinValue;

			TrimLen = -1;
			this.FontSize = FontSize;
		}

		public int GetTrimLenForHeight( double Height ) 
		{
			return ( int ) Math.Floor( Height / BlockHeight );
		}

		public void Override( double Height )
		{
			// Override has 100% certainty
			NumSamples = 100;

			Calibrated = true;
			BlockHeight = Height;
		}

		public void PushTrimSample( int TrimLen, double Height )
		{
			double ThisBlockHeight = Height / ( double ) TrimLen;

			Low = Math.Min( Low, ThisBlockHeight );
			High = Math.Min( High, ThisBlockHeight );

			// Max the Blockheight
			BlockHeight = Math.Ceiling( Math.Max( ThisBlockHeight, BlockHeight ) );
			NumSamples++;
#if DEBUG
			Logger.Log(
				ID
				, string.Format(
					"TrimLen {0}, Height {1} => BlockHeight {2}"
					, TrimLen, Height, BlockHeight
				), LogType.DEBUG
			);
#endif
		}
	}
}