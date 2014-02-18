using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ThreadWorkController.Interfaces;

namespace ThreadWorkController
{
    public class ThreadController
    {

        protected List<Object> m_data;
        protected List<Object> m_results = new List<Object>();
        protected List<IThreadWorker> m_workers = new List<IThreadWorker>();
        protected int m_workCounts = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        public ThreadController()
        {
        }

        /// <summary>
        /// Set source of data for all ThreadWorkers
        /// </summary>
        /// <param name="data">list with data</param>
        public void setData(List<Object> data)
        {
            if (data != null)
                m_data = data;
            else
                Console.WriteLine("\nEmpty data source!");
        }

        /// <summary>
        /// Add new Thread-worker to ThreadController
        /// </summary>
        /// <param name="worker">ThreadWorker</param>
        public void addWorker(IThreadWorker worker)
        {
            if (worker != null)
            {
                worker.init();
                m_workers.Add(worker);
                m_workCounts++;
            }
        }

        /// <summary>
        /// Starts all works
        /// </summary>
        /// <param name="monitoring">Set it to True if You want to observe the progress</param>
        /// <param name="manualDataPreparing">Set it to True if You want to split and prepare data yourself</param>
        public void executeWorks(bool monitoring = true, bool manualDataPreparing = false)
        {
            //prepare data
            if (!manualDataPreparing)
            {
                splitData();
            }

            //start all works
            Console.WriteLine("Start all works");
            foreach (IThreadWorker worker in m_workers)
            {
                worker.start();
            }

            //if we want to observe the progress set monitoring to True
            if (monitoring == true)
            {
                Console.WriteLine("Progress: ###");
                joinWorkWithMonitoring();
            }
            else
            {
                Console.WriteLine("Please wait...");
                joinWork();
            }

            //compress result in one list
            Console.WriteLine("Compress results");
            compressResults();

            Console.WriteLine("The execution is completed");
        }

        /// <summary>
        /// Compress results of work which was maded by thread-workers
        /// </summary>
        virtual protected void compressResults()
        {
            foreach (IThreadWorker worker in m_workers)
            {
                var res = worker.result();
                if (res != null)
                    m_results.AddRange(res);
            }
        }

        /// <summary>
        /// Join all work
        /// </summary>
        virtual protected void joinWork()
        {
            int progressCounter = 0;
            foreach (IThreadWorker worker in m_workers)
            {
                worker.join();
                progressCounter += worker.progress();
            }
            Console.WriteLine("\nWork completed. Progress: {0}", progressCounter);
        }

        /// <summary>
        /// Join work and observe the progress. Rate of refresh observing depend of number of workers and equal (m_workCounts/timeOut) s,
        /// where basic timeOut = 250ms.
        /// </summary>
        virtual protected void joinWorkWithMonitoring()
        {
            bool complete = false;
            int progressCounter = 0;
            while (!complete)
            {
                //suppose what all threads are already completed 
                complete = true;
                progressCounter = 0;

                foreach (IThreadWorker worker in m_workers)
                {
                    complete &= worker.join(250);
                    progressCounter += worker.progress();
                }
                Console.Write("\r{0}", progressCounter);
            }
            Console.WriteLine("\nWork completed. Progress: {0}", progressCounter);
        }

        /// <summary>
        /// Splite and prepare data source for thread-workers
        /// </summary>
        protected void splitData()
        {
            //counters
            int toUse = 0, alreadyUsed = 0;
            int dataCount = m_data.Count;

            //split data
            for (int i = 0; i < m_workCounts; i++)
            {
                toUse = ((i + 1) * dataCount) / m_workCounts - alreadyUsed;

                Object[] workArr = new Object[toUse];
                m_data.CopyTo(alreadyUsed, workArr, 0, toUse);
                m_workers[i].setImput(workArr.ToList<Object>());

                alreadyUsed += toUse;
            }
        }

        /// <summary>
        /// Return result of works. It should be call only after executeWorks().
        /// </summary>
        /// <returns>list with results</returns>
        public List<Object> getResult()
        {
            return m_results;
        }
    }
}
