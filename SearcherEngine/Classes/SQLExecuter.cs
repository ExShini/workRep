using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace elementWithGeo.Classes
{
    public class SQLExecuter
    {
        StringBuilder m_query = new StringBuilder();
        private int m_preparedCount = 0;

        String m_tableName = "";
        String m_connStr = "";
        String m_parameters = "";
        int m_writerCapasitor = 30;
        SqlConnection m_conn;

        public SQLExecuter()
        {
        }

        public void write(String data)
        {
            if (m_preparedCount == 0)
            {
                m_query.Clear();
                m_query.Append("INSERT INTO ");
                m_query.Append(m_tableName);
                m_query.Append(" ");
                m_query.Append(m_parameters);
                m_query.Append(" VALUES ");
            }

            m_query.Append("(");
            m_query.Append(data);
            m_query.Append(")");
            m_preparedCount++;

            if (m_preparedCount > m_writerCapasitor)
            {
                exucuteWrite();
                return;
            }

            m_query.Append(", ");
        }

        public void setTargetSettings(String connStr, String tableName, String parameters, int writerCapasitor = 120)
        {
            m_connStr = connStr;
            m_tableName = tableName;
            m_parameters = parameters;
            m_writerCapasitor = writerCapasitor;
            m_conn = new SqlConnection(connStr);
            m_conn.Open();
        }


        public void exucuteWrite()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = m_query.ToString();
            cmd.Connection = m_conn;

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DB Writing: " + ex.ToString());
            }
            m_preparedCount = 0;
        }

        public void free()
        {
            if (m_preparedCount > 0)
            {
                String tmpStr = Regex.Replace(m_query.ToString(), ", $", "");
                m_query.Clear();
                m_query.Append(tmpStr);

                exucuteWrite();
            }
            m_conn.Close();
        }
    }
}
