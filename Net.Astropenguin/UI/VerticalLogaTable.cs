using Net.Astropenguin.Logging;
using System;

namespace Net.Astropenguin.UI
{
    // Width Num of Samples, Precalculate Trim Length by Block height
    class VerticalLogaTable
    {
        public static readonly string ID = typeof( VerticalLogaTable ).Name;
        public double FontSize { get; private set; }

        public int CertaintyLevel { get { return NumSamples; } }
        public int TrimLen { get; private set; }

        private double BlockHeight = 0;
        private int NumSamples = 0;


        public VerticalLogaTable( double FontSize )
        {
            TrimLen = -1;
            this.FontSize = FontSize;
        }

        public int GetTrimLenForHeight( double Height ) 
        {
            return ( int ) Math.Floor( Height / BlockHeight );
        }

        public void PushTrimSample( int TrimLen, double Height )
        {
            double ThisBlockHeight = Height / ( double ) TrimLen;

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
