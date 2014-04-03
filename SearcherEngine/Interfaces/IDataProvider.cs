using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace SearcherEngine.Interfaces
{
    public interface IDataProvider
    {
        IData produce();
        IData produce(SqlDataReader reader);
        String SearchConnStr
        {
            get;
        }
        String TargetSqlQuery
        {
            get;
        }
    }
}
