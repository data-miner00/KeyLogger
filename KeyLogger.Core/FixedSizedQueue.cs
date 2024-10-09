namespace KeyLogger.Core;

using System.Collections.Generic;
using System.Collections.Concurrent;

/// <summary>
/// Simple implementation of fixed size queue that dequeues when maximum reached.
/// </summary>
/// <typeparam name="T">The data type.</typeparam>
public class FixedSizedQueue<T>
{
    private readonly ConcurrentQueue<T> backingQueue = new();
    private readonly object lockObject = new();
    private readonly int limit;

    /// <summary>
    /// Initializes a new instance of the <see cref="FixedSizedQueue{T}"/> class.
    /// </summary>
    /// <param name="limit">The maximum size of the queue.</param>
    public FixedSizedQueue(int limit)
    {
        this.limit = Guard.ThrowIfLessThan(limit, 1);
    }

    /// <summary>
    /// Gets all the current elements in the queue.
    /// </summary>
    public List<T> GetAll => [.. this.backingQueue];

    /// <summary>
    /// Pushes an item into the queue.
    /// </summary>
    /// <param name="obj">The item to be added.</param>
    public void Enqueue(T obj)
    {
        this.backingQueue.Enqueue(obj);
        lock (this.lockObject)
        {
           while (this.backingQueue.Count > this.limit && this.backingQueue.TryDequeue(out var overflow));
        }
    }

    /// <summary>
    /// Empties the queue.
    /// </summary>
    public void Clear()
    {
        this.backingQueue.Clear();
    }
}
