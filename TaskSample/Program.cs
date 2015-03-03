using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskSample
{
    class Program
    {
        static void Main()
        {
            NewTask();

            //TaskSequenceDemo();

            //Console.WriteLine("New Task");
            //NewTask();

            //Console.WriteLine("Task Factory");
            //FactoryCreateTask();

            //Console.WriteLine("Parent Task");
            //ParentTask();
           
            TaskScheduler.UnobservedTaskException += (sender, ex) => { ex.SetObserved(); };
                
            Console.ReadKey();
        }

        private static void ParentTask()
        {
            //子任务没有完成，父任务不会完成
            Task<int[]> taskParent = new Task<int[]>(() =>
            {
                int[] rets = new int[3];
                Task subtask1 = new Task(() =>
                {
                    rets[0] = Sum(10);
                }, TaskCreationOptions.AttachedToParent);
                Task subtask2 = new Task(() =>
                {
                    rets[1] = Sum(15);
                }, TaskCreationOptions.AttachedToParent);
                Task subtask3 = new Task(() =>
                {
                    rets[2] = Sum(5);
                }, TaskCreationOptions.AttachedToParent);

                subtask1.Start();
                subtask2.Start();
                subtask3.Start();

                Console.WriteLine("task method finish");
                return rets;
            });
            

            taskParent.ContinueWith(task =>
            {
                Console.WriteLine(task.Result[0]);
                Console.WriteLine(task.Status);
            }, TaskContinuationOptions.AttachedToParent);

            taskParent.Start();
            taskParent.Wait();
            
            //Thread.Sleep(6000);
            Console.WriteLine("parent task status:" + taskParent.Status);
        }

        private static void FactoryCreateTask()
        {
            Task task2 = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(5000);//模拟做其他的事情
            });
        }

        private static void NewTask()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            Task task1 = new Task(() => 
            {
                Console.WriteLine("Beign");
                
                Thread.Sleep(5000);

                Console.WriteLine("End");
                
            },cts.Token,TaskCreationOptions.AttachedToParent);//此时并没有分配支援

            task1.Start();//此时才把工作项加入到线程池的某一个线程的工作队列里面
            //Thread.Sleep(1000);
            //Console.WriteLine(task1.Status);
            cts.Cancel();
            //Console.WriteLine(task1.Status);
        }

        public static int Sum(int number)
        {
            int ret = 0;
            for (int i = 0; i < 10; i++)
            {
                ret += i;
                Thread.Sleep(200);
            }
            Console.WriteLine("parameter:" + number + ", ret:" + ret);
            return ret;
        }

        private static void TaskSequenceDemo()
        {
            Task parent = Task.Factory.StartNew(() =>
            {
                Task child1 = null, child2 = null, child3 = null, child4 = null;

                child1 = Task.Factory.StartNew(() =>
                {
                    Console.WriteLine("The child task 1 says hello! Current Thread: " + Thread.CurrentThread.ManagedThreadId);
                    
                    Console.WriteLine("");
                }, TaskCreationOptions.AttachedToParent);

                child2 = child1.ContinueWith((t) =>
                {
                    Console.WriteLine("The child task 2 says hello, Current Thread: " + Thread.CurrentThread.ManagedThreadId);   
                    Thread.Sleep(3000);
                });

                child4 = Task.Factory.StartNew((t) =>
                {
                    Console.WriteLine("The child task 4 says hello! Current Thread: " + Thread.CurrentThread.ManagedThreadId);
                    Thread.Sleep(5000);
                }, TaskCreationOptions.AttachedToParent);

                
                
                child3 = Task.Factory.StartNew(() =>
                {
                    //Task.WaitAll(new Task[2] { child4, child2 });
                    Console.WriteLine("The child task 3 says hello! Current Thread: " + Thread.CurrentThread.ManagedThreadId);
                }, TaskCreationOptions.AttachedToParent);

                
            });

            parent.ContinueWith(t =>
            {
                Console.WriteLine("Current Thread: " + Thread.CurrentThread.ManagedThreadId);
                Console.WriteLine("The child task 5 says hello!");
            });
        }



    }

}
