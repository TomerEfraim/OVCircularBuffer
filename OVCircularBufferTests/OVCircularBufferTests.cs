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
            OVCircularBuffer<string> instance = new OVCircularBuffer<string>(3);

            string[] data = { "This", "is" };

            instance.Enqueue(data[0]);
            instance.Enqueue(data[1]);

            bool result = instance.TryDequeue(out string output0);
            Assert.AreEqual(result, true);
            Assert.AreEqual(data[0], output0);

            result = instance.TryDequeue(out string output1);
            Assert.AreEqual(result, true);
            Assert.AreEqual(data[1], output1);

            Assert.IsTrue(instance.IsEmpty);
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



            produceTask.Wait();
            consumeTask.Wait();
        }
    }
}