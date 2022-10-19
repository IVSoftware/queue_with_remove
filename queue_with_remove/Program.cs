using System;
using System.Collections.Generic;
using System.Linq;

namespace queue_with_remove
{
    class Program
    {
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
    }

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
        /// 'Remove' items matching the Predicate by marking them as Skip.
        /// </summary>
        /// <remarks>
        /// Recommended way to remove items.
        /// </remarks>
        public void Remove(Predicate<T> p)
        {
            foreach (var item in this.Where(_=>p(_)).ToArray())
            {
                item.Cancel = true;
            }
        }
    }
    class QueueTestItem : ICancellableQueueItem
    {
        public bool Cancel { get; set; }

        public string Text { get; set; }
    }
}
