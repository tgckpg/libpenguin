using System;
using System.Collections.Generic;
using System.Linq;
using Windows.System;
using Windows.UI.Core;
using Windows.Foundation;

namespace Net.Astropenguin.Controls
{
    using Logging;
    using Linq;

    public class KeyCombinationEventArgs: EventArgs
    {
        public bool Handled = false;
        public VirtualKey[] Combinations { get; private set; }

        public KeyCombinationEventArgs( VirtualKey[] C )
        {
            Combinations = C;
        }
    }

    public class KeyboardControl
    {
        public static readonly string ID = typeof( KeyboardControl ).Name;

        public event TypedEventHandler<object,KeyEventArgs> KeyDown;

        private Dictionary<string, HashSet<Action<KeyCombinationEventArgs>>> RegisteredCombinations;
        private int SequenceIndex = 0;

        private HashSet<VirtualKey> PressedKeys;
        private List<VirtualKey[]> RegisteredSequence;
        private List<VirtualKey[]> SelectedSequence;

        public KeyboardControl( CoreWindow RootFrame )
        {
#if DEBUG
            Logger.Log( ID, "!!!! BE CAREFUL, Debug mode is Enabled. Control will Log key events !!!!", LogType.SYSTEM );
#endif
            RegisteredCombinations = new Dictionary<string, HashSet<Action<KeyCombinationEventArgs>>>();
            RegisteredSequence = new List<VirtualKey[]>();
            SelectedSequence = new List<VirtualKey[]>();
            PressedKeys = new HashSet<VirtualKey>();

            RootFrame.KeyDown += RootFrame_KeyDown;
        }

        private void RootFrame_KeyDown( CoreWindow sender, KeyEventArgs e )
        {
            KeyDown?.Invoke( sender, e );

            PressedKeys.Add( e.VirtualKey );

            List<VirtualKey> Keys = new List<VirtualKey>();
            foreach ( VirtualKey Key in PressedKeys )
            {
                if ( Key == VirtualKey.Menu ) continue;
                if ( ( sender.GetKeyState( Key ) & CoreVirtualKeyStates.Down ) != 0 )
                    Keys.Add( Key );
            }

            string KeyCombo = GetComboHash( Keys );
#if DEBUG
            Logger.Log( ID, "Key: " + e.VirtualKey.ToString(), LogType.DEBUG );
#endif

            if ( RegisteredCombinations.ContainsKey( KeyCombo ) )
            {
                ResetSequence();
            }
            else if( TrySelectSequence( e.VirtualKey, out Keys ) )
            {
                KeyCombo = GetSeqHash( Keys );
                ResetSequence();
            }
            else
            {
                return;
            }

            KeyCombinationEventArgs d = new KeyCombinationEventArgs( Keys.ToArray() );
            foreach( Action<KeyCombinationEventArgs> P in RegisteredCombinations[ KeyCombo ] )
            {
                P( d );
                if ( d.Handled ) break;
            }
        }

        private bool TrySelectSequence( VirtualKey key, out List<VirtualKey> Keys )
        {
            Keys = null;
            SelectedSequence.Filter( x => x[ SequenceIndex ] == key );

            if( 0 < SelectedSequence.Count )
            {
                SequenceIndex++;
                if( SelectedSequence[0].Length == SequenceIndex )
                {
                    Keys = new List<VirtualKey>( SelectedSequence[ 0 ] );
#if DEBUG
                    Logger.Log( ID, "Registered sequence matched: " + GetSeqHash( Keys ), LogType.DEBUG );
#endif
                    return true;
                }

                return false;
            }


            ResetSequence( key );
            return false;
        }

        private void ResetSequence( VirtualKey K = VirtualKey.None )
        {
            SequenceIndex = 0;
            SelectedSequence.Clear();

            IEnumerable<VirtualKey[]> Keys = RegisteredSequence.Where( ( x ) => x[ 0 ] == K );

            if ( 0 < Keys.Count() )
            {
                SequenceIndex++;
                SelectedSequence = new List<VirtualKey[]>( Keys );
            }
            else
            {
                SelectedSequence = new List<VirtualKey[]>( RegisteredSequence );
            }
        }

        public Action RegisterSequence( Action<KeyCombinationEventArgs> P, params VirtualKey[] Seq )
        {
            RegisteredSequence.Add( Seq );

            Action X = PushCombination( GetSeqHash( Seq ), P );
            return () =>
            {
                X();
                RegisteredSequence.Remove( Seq );
            };
        }

        /// <summary>
        /// Register an Action to a Set of Pressed Keys
        /// </summary>
        /// <param name="p"> The Action to trigger </param>
        /// <param name="Combinations"> Key Combinations, Cast them to VirtualKey if not available </param>
        /// <returns> A handler to remove this Combination </returns>
        public Action RegisterCombination( Action<KeyCombinationEventArgs> p, params VirtualKey[] Combinations )
        {
            return PushCombination( GetComboHash( Combinations ), p );
        }

        public void UnregisterCombination( Action<KeyCombinationEventArgs> p )
        {
            foreach(
                KeyValuePair<string, HashSet<Action<KeyCombinationEventArgs>>> Combinations
                in RegisteredCombinations
            ) {
                Combinations.Value.Remove( p );
            }
        }

        public void DestroyCombination( VirtualKey[] Combinations )
        {
            RegisteredCombinations[ GetComboHash( Combinations ) ].Clear();
        }

        private Action PushCombination( string Key, Action<KeyCombinationEventArgs> p )
        {
            if( !RegisteredCombinations.ContainsKey( Key ) )
            {
                RegisteredCombinations.Add( Key, new HashSet<Action<KeyCombinationEventArgs>>() );
            }

            RegisteredCombinations[ Key ].Add( p );

            return () => { RegisteredCombinations[ Key ].Remove( p ); };
        }

        private string GetComboHash( IEnumerable<VirtualKey> Combo )
        {
            List<VirtualKey> T = new List<VirtualKey>( Combo );
            T.Sort();
            return string.Join( "+", T );
        }

        private string GetSeqHash( IEnumerable<VirtualKey> Combo )
        {
            List<VirtualKey> T = new List<VirtualKey>( Combo );
            return string.Join( "+", T );
        }
    }
}