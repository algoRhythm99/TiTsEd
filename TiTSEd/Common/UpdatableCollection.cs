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

        bool ICollection<TResult>.IsReadOnly
        {
            get { return true; }
        }

        void IList<TResult>.Insert(int index, TResult item)
        {
            ThrowReadOnlyException();
        }

        void IList<TResult>.RemoveAt(int index)
        {
            ThrowReadOnlyException();
        }

        void ICollection<TResult>.Add(TResult item)
        {
            ThrowReadOnlyException();
        }

        void ICollection<TResult>.Clear()
        {
            ThrowReadOnlyException();
        }

        bool ICollection<TResult>.Remove(TResult item)
        {
            ThrowReadOnlyException();
            return false;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)_items).GetEnumerator();
        }

        #region IList Members
        bool IList.Contains(object value)
        {
            return Contains((TResult)value);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((TResult)value);
        }
        bool IList.IsFixedSize
        {
            get { return false; }
        }

        bool IList.IsReadOnly
        {
            get { return true; }
        }

        int IList.Add(object value)
        {
            throw new NotImplementedException();
        }

        void IList.Clear()
        {
            throw new NotImplementedException();
        }

        void IList.Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        void IList.Remove(object value)
        {
            throw new NotImplementedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set
            {
                throw new NotImplementedException();
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((IList)_items).CopyTo(array, index);
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return this; }
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
