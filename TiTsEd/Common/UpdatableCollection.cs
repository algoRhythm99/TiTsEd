using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections;
using System.Runtime.CompilerServices;
using TiTsEd.ViewModel;

namespace System.ComponentModel
{
    public interface IUpdatableList : IList, INotifyCollectionChanged, INotifyPropertyChanged
    {
        void Update();
    }

    public interface IObservableList<T> : IList<T>, IUpdatableList
    {
    }

    public class UpdatableCollection<TSource, TResult> : BindableBase, IObservableList<TResult>
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected readonly IEnumerable<TSource> _source;
        protected readonly List<TSource> _oldSource = new List<TSource>();
        protected readonly List<TResult> _items = new List<TResult>();
        protected readonly Func<TSource, TResult> _selector;
        protected readonly IEqualityComparer<TSource> _equalityComparer;

        public UpdatableCollection(IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            _source = source;
            _selector = selector;
            _oldSource.AddRange(source);
            _items.AddRange(_oldSource.Select(selector));
            _equalityComparer = EqualityComparer<TSource>.Default;

        }

        public int Count
        {
            get { return _items.Count; }
        }

        public IEnumerable<TSource> Source
        {
            get { return _source; }
        }

        public TResult this[int index]
        {
            get { return _items[index]; }
            set { ThrowReadOnlyException(); }
        }

        public int IndexOf(TResult item)
        {
            return _items.IndexOf(item);
        }

        public bool Contains(TResult item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(TResult[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null) CollectionChanged(this, e);
        }

        public void Update()
        {
            int oldCount = _items.Count;
            var newSource = (_source as IList<TSource>) ?? _source.ToList();
            var differences = _oldSource.Diff(newSource, _equalityComparer.Equals);
            if (differences.Count == 0) return;

            // Met à jour le cache de la source
            _oldSource.Clear();
            _oldSource.AddRange(newSource);

            // One difference only? Report it exactly. Replace is not supported by wpf, so deals with it as a "reset"
            if (differences.Count == 1 && differences[0].Operation != ListOperations.Substitution)
            {
                var diff = differences[0];
                if (diff.Operation == ListOperations.Insertion)
                {
                    var items = newSource.SelectRange(diff.NewIndex, diff.Count).Select(_selector).ToArray();
                    _items.InsertRange(diff.NewIndex, items);
                    diff = NotifyInsertion(diff, items);
                }
                else if (diff.Operation == ListOperations.Deletion)
                {
                    var items = _items.SelectRange(diff.NewIndex, diff.Count).ToArray();
                    _items.RemoveRange(diff.NewIndex, diff.Count);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    //diff = NotifyDeletion(diff, items);
                }
            }
            else
            {
                _items.UpdateFrom(newSource, _selector, differences);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }

            // Compare elements count
            if (_items.Count != oldCount) OnPropertyChanged("Count");
        }

        ListDifference NotifyInsertion(ListDifference diff, TResult[] items)
        {
            int index = diff.NewIndex;
            foreach (var item in items)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
                ++index;
            }

            /*
            if (items.Length == 1)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items[0], diff.NewIndex));
            }
            else
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items, diff.NewIndex));
            }*/
            return diff;
        }

        ListDifference NotifyDeletion(ListDifference diff, TResult[] items)
        {
            if (items.Length == 1)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items[0], diff.NewIndex));
            }
            else
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items, diff.NewIndex));
            }
            return diff;
        }

        private void ThrowReadOnlyException()
        {
            throw new InvalidOperationException("This list is read-only.");
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        bool ICollection<TResult>.IsReadOnly
        {
            get { return IsReadOnly; }
        }

        public void Insert(int index, TResult item)
        {
            ThrowReadOnlyException();
        }

        void IList<TResult>.Insert(int index, TResult item)
        {
            Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ThrowReadOnlyException();
        }

        void IList<TResult>.RemoveAt(int index)
        {
            RemoveAt(index);
        }

        public void Add(TResult item)
        {
            ThrowReadOnlyException();
        }

        void ICollection<TResult>.Add(TResult item)
        {
            Add(item);
        }

        public void Clear()
        {
            ThrowReadOnlyException();
        }

        void ICollection<TResult>.Clear()
        {
            Clear();
        }

        public bool Remove(TResult item)
        {
            ThrowReadOnlyException();
            return false;
        }

        bool ICollection<TResult>.Remove(TResult item)
        {
            return Remove(item);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)_items).GetEnumerator();
        }

        #region IList Members
        public bool Contains(object value)
        {
            return Contains((TResult)value);
        }

        bool IList.Contains(object value)
        {
            return Contains(value);
        }

        public int IndexOf(object value)
        {
            return IndexOf((TResult)value);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf(value);
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        bool IList.IsFixedSize
        {
            get { return IsFixedSize; }
        }

        bool IList.IsReadOnly
        {
            get { return IsReadOnly; }
        }

        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        int IList.Add(object value)
        {
            return Add(value);
        }

        void IList.Clear()
        {
            Clear();
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (TResult)value);
        }

        void IList.Remove(object value)
        {
            Remove((TResult)value);
        }

        void IList.RemoveAt(int index)
        {
            RemoveAt(index);
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void CopyTo(Array array, int index)
        {
            ((IList)_items).CopyTo(array, index);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            CopyTo(array, index);
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        bool ICollection.IsSynchronized
        {
            get { return IsSynchronized; }
        }

        public object SyncRoot
        {
            get { return this; }
        }

        object ICollection.SyncRoot
        {
            get { return SyncRoot; }
        }

        #endregion
    }

    public sealed class UpdatableCollection<T> : UpdatableCollection<T, T>
    {
        public UpdatableCollection(IEnumerable<T> initialItems)
            : base(initialItems, x => x)
        {
        }
    }
}
