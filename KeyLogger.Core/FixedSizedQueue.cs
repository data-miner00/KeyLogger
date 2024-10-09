using System.Collections.Generic;
using System.Collections.Concurrent;

namespace KeyLogger.Core
{
    public class FixedSizedQueue<T>
    {
        private readonly ConcurrentQueue<T> q = new();
        private readonly object lockObject = new();

        public FixedSizedQueue(int limit)
        {
            this.limit = limit;
        }

        private readonly int limit;

        public List<T> GetAll => [.. this.q];

        public void Enqueue(T obj)
        {
            q.Enqueue(obj);
            lock (lockObject)
            {
               while (q.Count > limit && q.TryDequeue(out var overflow));
            }
        }

        public void Clear()
        {
            q.Clear();
        }
    }
}
