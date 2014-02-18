using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ThreadWorkController;
using System.Data.SqlClient;

namespace TrainerForLemmatizer
{
    class LemmaTrainerWorker : ThreadWorker
    {
        private String m_connStr = "";


        public LemmaTrainerWorker(String connStr)
        {
            m_connStr = connStr;
        }

        public override void work()
        {
            using (SqlConnection conn = new SqlConnection(m_connStr))
            {
                conn.Open();
                String queryString = "SELECT I.Name "
                                     + "FROM dbo.Attrib72 A WITH(NOLOCK)INNER JOIN ItemList I WITH(NOLOCK) ON I.ID = A.IdItem "
                                     + "where I.UP = 18 AND A.AttribValue = \'";
                StringBuilder bld = new StringBuilder();

                foreach (String str in m_input)
                {
                    bld.Clear();
                    bld.Append(queryString);
                    bld.Append(str);
                    bld.Append("\';");

                    SqlCommand command = new SqlCommand(bld.ToString(), conn);
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        if ((String)reader[0] != str)
                        {
                            //String lemma = (String)reader[0];
                            m_result.Add(new Pair((String)reader[0], str));
                            Interlocked.Increment(ref m_counter);
                        }
                    }
                    reader.Close();
                }
            }
        }
    }
}
