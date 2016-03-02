using Net.Astropenguin.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.Astropenguin.Helpers
{
    public enum DivertingMode
    {
        PROGRESSIVE, REGRESSIVE
    }

    public class Diverter<T>
    {
        public int Scale {
            get { return _scale; }
            set
            {
                if ( value < 1 ) _scale = 1;
                else _scale = value;
            }
        }

        public DivertingMode Mode = DivertingMode.REGRESSIVE;

        private int _scale = 10;
        private int l = 0;
        private int[] CurrentStates;
        private Weight<T>[] w;

        public Diverter( Weight<T>[] Weights )
            :this( Weights, DivertingMode.REGRESSIVE ) { }

        public Diverter( Weight<T>[] Weights, DivertingMode m )
            :this( Weights, m, 100 ) { }

        public Diverter( Weight<T>[] Weights, DivertingMode m, int Scale )
        {
            CurrentStates = new int[ l = Weights.Length ];
            w = Weights;

            Mode = m;
            this.Scale = Scale;
        }

        private void GiveWeights()
        {
            float Total = w.Sum( ( w ) => { return w.Factor; } );
            if ( Total == 0.0 ) Total = 1;

            int f = l < Scale ? Scale : l;
            switch ( Mode )
            {
                case DivertingMode.PROGRESSIVE:
                    for ( int i = 0; i < l; i++ )
                    {
                        CurrentStates[ i ] = ( int ) Math.Round( ( 1.0 - w[ i ].Factor / Total ) * f ) + 1;
                    }
                    break;
                case DivertingMode.REGRESSIVE:
                default:
                    for ( int i = 0; i < l; i++ )
                    {
                        CurrentStates[ i ] = ( int ) Math.Round( w[ i ].Factor / Total * f ) + 1;
                    }
                    break;
            }
        }

        private int TakeJ = 0;
        public T Take()
        {
            int Negative = 0;
            int t = 0;
            while ( TakeJ < l )
            {
                if ( 0 < CurrentStates[ t = TakeJ++ ]-- ) break;
                else Negative++;
            }

            if ( TakeJ == l ) TakeJ = 0;

            if ( Negative < l )
            {
                return w[ t ].Freight;
            }

            GiveWeights();
            return Take();
        }
    }
}
