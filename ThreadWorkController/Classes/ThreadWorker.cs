using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ThreadWorkController.Interfaces;

namespace ThreadWorkController
{
    public class ThreadWorker : IThreadWorker
    {

        protected Thread m_thread;
        protected int m_counter = 0;
        protected List<Object> m_result = new List<Object>();
        protected List<Object> m_input;
        protected bool m_alreadyInit = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public ThreadWorker()
        {

        }

        /// <summary>
        /// initiolize and prepare ThreadWorker's members
        /// </summary>
        virtual public void init()
        {
            if (!m_alreadyInit)
            {
                m_thread = new Thread(work);
                m_alreadyInit = true;
            }
        }

        /// <summary>
        /// Start execution
        /// </summary>
        public void start()
        {
            m_thread.Start();
        }

        /// <summary>
        /// Join thread
        /// </summary>
        public void join()
        {
            m_thread.Join();
        }

        /// <summary>
        /// Try join thread
        /// </summary>
        /// <param name="timeOut">TimeOut for waiting</param>
        /// <returns>True if thread is canceled, False in over case</returns>
        public bool join(int timeOut)
        {
            return m_thread.Join(timeOut);
        }

        /// <summary>
        /// Main work function
        /// </summary>
        virtual public void work()
        {
            //do nothing
        }


        /// <summary>
        /// Return list of result if work is completed or NULL if work in progress
        /// </summary>
        /// <returns>list with result</returns>
        public List<Object> result()
        {
            if (!m_thread.IsAlive)
                return m_result;
            else
                return null;
        }

        /// <summary>
        /// Return progress counter
        /// </summary>
        /// <returns>progress counter</returns>
        public int progress()
        {
            return m_counter;
        }

        /// <summary>
        /// Set impute data for currecnt worker
        /// </summary>
        /// <param name="input">list with data, can be used in work process</param>
        public void setImput(List<Object> input)
        {
            m_input = input;
        }
    }
}
