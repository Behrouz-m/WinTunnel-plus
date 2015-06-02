using System;
using System.Collections;
using System.Threading;

namespace WinTunnel
{
	/// <summary>
	/// Summary description for ThreadPool.
	/// This is a singleton class.
	/// </summary>
	public class MyThreadPool
	{
		private int m_JobCounts = 0;

		private Logger logger;

		private Queue taskQueue = Queue.Synchronized( new Queue() );
		
		private static MyThreadPool m_pool;

		private MyThreadPool() //private constructor--to prevent instantiation
		{
			logger = Logger.getInstance();
			logger.debug("ThreadPool object instantiated.");
		}

		public static MyThreadPool getInstance()
		{
			if (m_pool == null)
			{
				m_pool = new MyThreadPool();
			}
			return m_pool;
		}
		
		public void initialize()
		{
			logger.debug("ThreadPool initializing");
		}
	
		private void threadFunc(object arg)  
		{
			int thread_count = (int)arg;
			try
			{
				logger.info("Thread #{0} is starting...", thread_count);

				if (taskQueue.Count > 0)
				{
					ITask task = (ITask)taskQueue.Dequeue();
					if (task != null)
					{
						logger.debug("Thread is processing {0} ...", task.getName());
						task.run();
					}
				}

			}
			catch (Exception e)
			{
				logger.error("Thread has encountered exception: {0} ", e.StackTrace);
			}
			finally
			{
				logger.info("Thread #{0} is terminating ...", thread_count);
				--m_JobCounts;
			}
		}
		
	
		public void Stop()  //Shutdown each thread
		{
			logger.info("Task Queue currently contains {0} tasks.", m_JobCounts);
			logger.info("Signaled all threads to exit.");
			while (m_JobCounts > 0) Thread.Sleep(1000);
		}  

		public bool addTask(ITask newTask)
		{
			//add a task to the Queue
			taskQueue.Enqueue(newTask);
			bool status = ThreadPool.QueueUserWorkItem(new WaitCallback(threadFunc), ++m_JobCounts);
			if(!status)
				--m_JobCounts;

			return true;
		}
	} 
}
