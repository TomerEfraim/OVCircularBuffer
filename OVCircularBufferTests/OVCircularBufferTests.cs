using Microsoft.VisualStudio.TestTools.UnitTesting;
using OVCircularBuffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OVCircularBuffer.Tests
{
    [TestClass()]
    public class OVCircularBufferTests
    {
        [TestMethod()]
        public void Test1()
        {
            OVCircularBuffer<string> buffer = new OVCircularBuffer<string>(3);

            Assert.AreEqual(3, buffer.BufferSize);
            Assert.IsTrue(buffer.IsEmpty);
            Assert.IsFalse(buffer.IsFull);

            string[] data = { "This", "is" };

            buffer.Enqueue(data[0]);
            buffer.Enqueue(data[1]);

            Assert.AreEqual(3, buffer.BufferSize);
            Assert.IsFalse(buffer.IsEmpty);
            Assert.IsFalse(buffer.IsFull);
            Assert.AreEqual(2, buffer.Count);

            buffer.Enqueue(data[0]);

            Assert.IsTrue(buffer.IsFull);
            Assert.AreEqual(3, buffer.Count);

            buffer.Enqueue(data[1]);

            Assert.IsTrue(buffer.IsFull);
            Assert.AreEqual(3, buffer.Count);

            bool result = buffer.TryDequeue(out string output);

            Assert.IsFalse(buffer.IsFull);
            Assert.IsTrue(result);
            Assert.AreEqual(data[1], output);

            result = buffer.TryDequeue(out output);

            Assert.IsTrue(result);
            Assert.AreEqual(data[0], output);
            Assert.IsFalse(buffer.IsEmpty);

            result = buffer.TryDequeue(out output);

            Assert.IsTrue(result);
            Assert.AreEqual(data[1], output);
        }

        public static void ProduceLoop(object obj)
        {
            if (obj is OVCircularBuffer<int> buffer)
            {
                int i = 0;
                while (true)
                {
                    buffer.Enqueue(i);
                    //Console.WriteLine("enqueued " + i.ToString());
                    i++;
                    Task.Delay(1000).Wait(); // simulate fps
                }
            }
        }

        public static void ConsumeLoop(object obj)
        {
            if (obj is OVCircularBuffer<int> buffer)
            {
                int expected = 0;
                while (true)
                {
                    if (buffer.TryDequeue(out int i))
                    {
                        if (expected == i)
                        {
                            //Console.WriteLine("dequeued " + i.ToString());
                            expected++;
                            Task.Delay(1000).Wait(); // sumulate processing time
                        }
                        else
                        {
                            // missed frames...
                        }
                    }
                    else
                    {
                        // empty buffer....
                    }
                }
            }
        }

        [TestMethod()]
        public void Test2()
        {
            OVCircularBuffer<int> buffer = new OVCircularBuffer<int>(10);

            Task produceTask = new Task(ProduceLoop, buffer);

            Task consumeTask = new Task(ConsumeLoop, buffer);

            produceTask.Start();

            Task.Delay(100).Wait();

            consumeTask.Start();

            produceTask.Wait(5000);
            consumeTask.Wait(5000);
        }

        [TestMethod()]
        public void Test3()
        {
            OVCircularBuffer<int> buffer = new OVCircularBuffer<int>(4);

            CollectionAssert.AreEqual(new int[0], buffer.ToArray());

            buffer.Enqueue(1);

            CollectionAssert.AreEqual(new int[1] { 1 }, buffer.ToArray());

            buffer.Enqueue(2);
            buffer.Enqueue(3);
            buffer.Enqueue(4);
            buffer.Enqueue(5);

            CollectionAssert.AreEqual(new int[4] { 2, 3, 4, 5 }, buffer.ToArray());

            bool success = buffer.TryDequeue(out int dequeued);

            Assert.AreEqual(true, success);
            Assert.AreEqual(2, dequeued);
            CollectionAssert.AreEqual(new int[3] { 3, 4, 5 }, buffer.ToArray());

            buffer.TryDequeue(out dequeued);
            buffer.TryDequeue(out dequeued);
            success = buffer.TryDequeue(out dequeued);

            Assert.AreEqual(true, success);
            Assert.AreEqual(5, dequeued);
            CollectionAssert.AreEqual(new int[0], buffer.ToArray());

            success = buffer.TryDequeue(out dequeued);

            Assert.AreEqual(false, success);
            Assert.AreEqual(0, dequeued);
            CollectionAssert.AreEqual(new int[0], buffer.ToArray());
        }

        [TestMethod()]
        public void Test4()
        {
            OVCircularBuffer<string> buffer = new OVCircularBuffer<string>(10);

            bool success = buffer.TryGetMax(out string max);

            Assert.AreEqual(false, success);
            Assert.AreEqual(null, max);

            success = buffer.TryGetMin(out string min);

            Assert.AreEqual(false, success);
            Assert.AreEqual(null, min);
        }
    }
}