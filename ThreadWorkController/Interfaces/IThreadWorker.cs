using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreadWorkController.Interfaces
{
    public interface IThreadWorker
    {
        void init();
        void start();
        void join();
        bool join(int timeOut);
        void work();
        int progress();
        List<Object> result();
        void setImput(List<Object> input);
    }
}
