namespace KeyLogger.Core.UnitTests;

using System.Collections.Generic;

public sealed class FixedSizedQueueTests
{
    private readonly FixedSizedQueueSteps steps = new();

    [Fact]
    public void Initialize_ItemsShouldBeEmpty()
    {
        this.steps
            .ThenIExpectQueueItemsToBeEmpty()
            .ThenIExpectFirstItemToBeNull()
            .ThenIExpectLastItemToBeNull();
    }

    [Fact]
    public void Enqueue_Enqueue1Item_FirstAndLastShouldEqual()
    {
        var number = 5;

        this.steps
            .WhenIEnqueueItem(number)
            .ThenIExpectFirstItemToBe(number)
            .ThenIExpectLastItemToBe(number)
            .ThenIExpectQueueItemsToBe([number]);
    }

    [Fact]
    public void Enqueue_Enqueue5Items_EnqueuedSuccessfully()
    {
        List<int> numbers = [5, 1, 2, 3, 4];

        this.steps
            .WhenIEnqueueItems(numbers)
            .ThenIExpectFirstItemToBe(5)
            .ThenIExpectLastItemToBe(4)
            .ThenIExpectQueueItemsToBe(numbers);
    }

    [Fact]
    public void Clear_ExistingQueue_ShouldBeEmptied()
    {
        List<int> numbers = [5, 1, 2, 3, 4];

        this.steps
            .WhenIEnqueueItems(numbers)
            .WhenIClearQueue()
            .ThenIExpectQueueItemsToBeEmpty()
            .ThenIExpectFirstItemToBeNull()
            .ThenIExpectLastItemToBeNull();
    }
}
