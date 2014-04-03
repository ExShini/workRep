using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using FinderEngineSharp.Data;

namespace SearcherEngine.Interfaces
{
    public interface IOutDataProvider: IDataProvider
    {
        void setResultList(List<IData> outList);
        void declareResult(SqlDataReader reader, ResultInfo<IData> res);
        void declareResult(ResultInfo<IData> res);
        void declareNegative();
        int TargetIndex
        {
            get;
        }
    }
}
