namespace KeyLogger.Core;

using System.Collections.Generic;
using System.Collections.Concurrent;
using System;

/// <summary>
/// Simple implementation of fixed size queue that dequeues when maximum reached.
/// </summary>
/// <typeparam name="T">The data type.</typeparam>
public class FixedSizedQueue<T> : IDisposable
{
    private readonly ConcurrentQueue<T> backingQueue = new();
    private readonly object lockObject = new();
    private readonly int limit;

    private T? lastEntry;
    private bool isDisposed;

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
    /// Gets the first item in the queue. Returns <c>null</c> if nothing is in the queue.
    /// </summary>
    public T? Peek
    {
        get
        {
            this.backingQueue.TryPeek(out var item);

            return item;
        }
    }

    /// <summary>
    /// Gets the last item in the queue. Returns <c>null</c> if nothing is in the queue.
    /// </summary>
    public T? PeekLast => this.lastEntry;

    /// <summary>
    /// Pushes an item into the queue.
    /// </summary>
    /// <param name="obj">The item to be added.</param>
    public void Enqueue(T obj)
    {
        this.lastEntry = obj;
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
        this.lastEntry = default;
        this.backingQueue.Clear();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose the states.
    /// </summary>
    /// <param name="disposing">Whether currently is disposing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.isDisposed)
        {
            if (disposing)
            {
                this.Clear();
            }

            this.isDisposed = true;
        }
    }
}
