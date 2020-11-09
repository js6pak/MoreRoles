using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreRoles
{
    public class WeightedRandomList<T> : IEnumerable<WeightedRandomList<T>.Entry>
    {
        public readonly struct Entry
        {
            public T Item { get; }
            public double AccumulatedWeight { get; }

            public Entry(T item, double accumulatedWeight)
            {
                Item = item;
                AccumulatedWeight = accumulatedWeight;
            }
        }

        private readonly List<Entry> _entries = new List<Entry>();
        private double _accumulatedWeight;

        public void Add(T item, double weight)
        {
            _accumulatedWeight += weight;
            _entries.Add(new Entry(item, _accumulatedWeight));
        }

        public T GetRandom()
        {
            var r = Random.value * _accumulatedWeight;

            return _entries.FirstOrDefault(entry => entry.AccumulatedWeight >= r).Item;
        }

        public IEnumerator<Entry> GetEnumerator() => _entries.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}