using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.Astropenguin.DataModel
{
    using System.Collections;
    using Helpers;

    public class ObservableHashSet<T> : ActiveData, ISet<T>, INotifyCollectionChanged
    {
        private HashSet<T> HSet;
        public NotifyCollectionChangedEventHandler DataChanged { get; private set; }

        public ObservableHashSet()
        {
            HSet = new HashSet<T>();
        }

        public ObservableHashSet( IEnumerable<T> Items )
        {
            HSet = new HashSet<T>( Items );
        }

        public int Count { get { return HSet.Count; } }

        public bool IsReadOnly { get { return false; } }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { DataChanged += value; } 
            remove { DataChanged -= value; }
        }

        private void NotifyDataChanged( NotifyCollectionChangedAction Action )
        {
            if ( DataChanged != null )
                DataChanged( this, new NotifyCollectionChangedEventArgs( Action ) );
        }

        public bool Add( T item )
        {
            bool b = ( ( ISet<T> ) HSet ).Add( item );
            if ( b )
            {
                NotifyDataChanged( NotifyCollectionChangedAction.Add );
                NotifyChanged( "Count" );
            }
            return b;
        }

        public void ExceptWith( IEnumerable<T> other )
        {
            ( ( ISet<T> ) HSet ).ExceptWith( other );
        }

        public void IntersectWith( IEnumerable<T> other )
        {
            ( ( ISet<T> ) HSet ).IntersectWith( other );
        }

        public bool IsProperSubsetOf( IEnumerable<T> other )
        {
            return ( ( ISet<T> ) HSet ).IsProperSubsetOf( other );
        }

        public bool IsProperSupersetOf( IEnumerable<T> other )
        {
            return ( ( ISet<T> ) HSet ).IsProperSupersetOf( other );
        }

        public bool IsSubsetOf( IEnumerable<T> other )
        {
            return ( ( ISet<T> ) HSet ).IsSubsetOf( other );
        }

        public bool IsSupersetOf( IEnumerable<T> other )
        {
            return ( ( ISet<T> ) HSet ).IsSupersetOf( other );
        }

        public bool Overlaps( IEnumerable<T> other )
        {
            return ( ( ISet<T> ) HSet ).Overlaps( other );
        }

        public bool SetEquals( IEnumerable<T> other )
        {
            return ( ( ISet<T> ) HSet ).SetEquals( other );
        }

        public void SymmetricExceptWith( IEnumerable<T> other )
        {
            ( ( ISet<T> ) HSet ).SymmetricExceptWith( other );
        }

        public void UnionWith( IEnumerable<T> other )
        {
            ( ( ISet<T> ) HSet ).UnionWith( other );
        }

        void ICollection<T>.Add( T item )
        {
            Add( item );
        }

        public void Clear()
        {
            ( ( ISet<T> ) HSet ).Clear();
            NotifyChanged( "Count" );
            NotifyDataChanged( NotifyCollectionChangedAction.Reset );
        }

        public bool Contains( T item )
        {
            return ( ( ISet<T> ) HSet ).Contains( item );
        }

        public void CopyTo( T[] array, int arrayIndex )
        {
            ( ( ISet<T> ) HSet ).CopyTo( array, arrayIndex );
        }

        public bool Remove( T item )
        {
            bool b = ( ( ISet<T> ) HSet ).Remove( item );
            if ( b )
            {
                NotifyChanged( "Count" );
                NotifyDataChanged( NotifyCollectionChangedAction.Remove );
            }
            return b;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ( ( ISet<T> ) HSet ).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ( ( ISet<T> ) HSet ).GetEnumerator();
        }
    }

}
