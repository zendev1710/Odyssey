using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Odyssey.Core.Utils
{
    public class ObservableBucket<T> : ObservableCollection<T>
    {
        private readonly int _maxSize;
        private readonly Dictionary<T, LinkedListNode<T>> _map;
        private readonly LinkedList<T> _list;

        public int MaxSize => _maxSize;

        public ObservableBucket(int maxSize)
        {
            if (maxSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxSize), "maxSize must be greater than 0.");
            _maxSize = maxSize;
            _map = new Dictionary<T, LinkedListNode<T>>();
            _list = new LinkedList<T>();
        }

        public new void Add(T item)
        {
            if (_map.TryGetValue(item, out var node))
            {
                _list.Remove(node);
                _list.AddFirst(node);
                MoveItem(IndexOf(item), 0);
            }
            else
            {
                if (_list.Count == _maxSize)
                {
                    var last = _list.Last!;
                    _map.Remove(last.Value);
                    _list.RemoveLast();
                    RemoveItem(Items.Count - 1);
                }
                var newNode = _list.AddFirst(item);
                _map[item] = newNode;
                InsertItem(0, item);
            }
        }

        public new bool Contains(T item) => _map.ContainsKey(item);

        public new void Clear()
        {
            base.Clear();
            _list.Clear();
            _map.Clear();
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            if (_map.TryGetValue(item, out var node))
            {
                _list.Remove(node);
                _map.Remove(item);
            }
            base.RemoveItem(index);
        }

        protected override void InsertItem(int index, T item)
        {
            // Prevent direct insertion except through Add
            if (!_map.ContainsKey(item))
            {
                base.InsertItem(index, item);
            }
        }

        public new IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        //IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
