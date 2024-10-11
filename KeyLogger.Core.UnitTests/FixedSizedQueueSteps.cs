namespace KeyLogger.Core.UnitTests;

using FluentAssertions;
using KeyLogger.Core;
using System.Collections.Generic;

public sealed class FixedSizedQueueSteps
{
    private const int MaximumSize = 5;
    private readonly FixedSizedQueue<int?> fixedSizedQueue;

    public FixedSizedQueueSteps()
    {
        this.fixedSizedQueue = new(MaximumSize);
    }

    public FixedSizedQueueSteps WhenIEnqueueItem(int item)
    {
        this.fixedSizedQueue.Enqueue(item);
        return this;
    }

    public FixedSizedQueueSteps WhenIEnqueueItems(IEnumerable<int> items)
    {
        foreach (var item in items)
        {
            this.fixedSizedQueue.Enqueue(item);
        }

        return this;
    }

    public FixedSizedQueueSteps WhenIClearQueue()
    {
        this.fixedSizedQueue.Clear();
        return this;
    }

    public FixedSizedQueueSteps ThenIExpectQueueItemsToBeEmpty()
    {
        this.fixedSizedQueue.GetAll.Should().NotBeNull();
        this.fixedSizedQueue.GetAll.Should().BeEmpty();
        return this;
    }

    public FixedSizedQueueSteps ThenIExpectQueueItemsToBe(List<int> items)
    {
        this.fixedSizedQueue.GetAll.Should().NotBeNull();
        this.fixedSizedQueue.GetAll.Count.Should().Be(items.Count);
        this.fixedSizedQueue.GetAll.Should().BeEquivalentTo(items);
        return this;
    }

    public FixedSizedQueueSteps ThenIExpectFirstItemToBe(int item)
    {
        this.fixedSizedQueue.Peek.Should().NotBeNull();
        this.fixedSizedQueue.Peek.Should().Be(item);
        return this;
    }

    public FixedSizedQueueSteps ThenIExpectLastItemToBe(int item)
    {
        this.fixedSizedQueue.PeekLast.Should().NotBeNull();
        this.fixedSizedQueue.PeekLast.Should().Be(item);
        return this;
    }

    public FixedSizedQueueSteps ThenIExpectFirstItemToBeNull()
    {
        this.fixedSizedQueue.Peek.Should().BeNull();
        return this;
    }

    public FixedSizedQueueSteps ThenIExpectLastItemToBeNull()
    {
        this.fixedSizedQueue.PeekLast.Should().BeNull();
        return this;
    }
}
