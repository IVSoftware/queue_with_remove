There can be good reasons for wanting to 'remove' or otherwise negate certain items that are waiting in a queue. In my use case the queue consists of Google Drive folders waiting to download the files therein contained but with the additional requirement of being able to cancel a pending download before it starts. I considered answers that involved rebuilding the queue selectively or that used RemoveAt(0) on a List<T>. An alternative approach that I found simple and effective was to provide a `Cancel` property for any given queued item indicating that it should be skipped on Dequeue obviating any need to  to mess with the queue itself. The `Remove` method then uses a predicate to set the `Cancel` property using an arbitrary match. 

    internal interface ICancellableQueueItem
    {
        bool Cancel { get; set; }
    }
    class QueueWithRemove<T> : Queue<T> where T : ICancellableQueueItem
    {
        public new bool TryDequeue(out T item)
        {
            while(Count != 0)
            {
                item = base.Dequeue();
                if(!item.Cancel)
                {
                    return true;
                }
            }
            item = default(T);
            return false;
        }
        // Exceptions:
        // T:System.InvalidOperationException: The System.Collections.Generic.Queue`1 is empty.
        public new T Dequeue()
        {
            while (true)
            {
                var item = base.Dequeue();
                if (!item.Cancel)
                {
                    return item;
                }
            }
        }
        /// <summary>
        /// 'Remove' items matching the Predicate by marking them as Cancel.
        /// </summary>
        public void Remove(Predicate<T> p)
        {
            foreach (var item in this.Where(_=>p(_)).ToArray())
            {
                item.Cancel = true;
            }
        }
    }

**TEST**

    static void Main(string[] args)
    {
        var testData = new[] { "zero", "one", "two", "test-skip", "test-skip", "three" };

        var testQueue = new QueueWithRemove<QueueTestItem>();

        Console.WriteLine("Test remove by using Predicate to match text.");
        foreach (var item in testData)
        {
            testQueue.Enqueue(new QueueTestItem { Text = item });
        }
        testQueue.Remove(_ => _.Text.Equals("test-skip"));
        while(testQueue.TryDequeue(out QueueTestItem item))
        {
            Console.WriteLine(item.Text);
        }
    }

*WHERE*

    class QueueTestItem : ICancellableQueueItem
    {
        public bool Cancel { get; set; }

        public string Text { get; set; }
    }

[![Queue output skips over cancelled items]](https://github.com/IVSoftware/queue_with_remove/blob/master/queue_with_remove/Screenshots/screenshot.png)