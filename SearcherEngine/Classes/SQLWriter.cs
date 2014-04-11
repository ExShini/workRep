using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace SearcherEngine.Classes
{
    public class SQLWriter
    {

        private SqlConnectionStringBuilder ConnectBuilderServer28 = new SqlConnectionStringBuilder() { DataSource = "server28", InitialCatalog = "agia", IntegratedSecurity = true };
        private List<String> m_parameters = new List<string>();


        private QueryType m_queryType = QueryType.NO_TYPE;
        private Servers m_server = Servers.NoServer;
        private String m_qPattern = "";
        private String m_tableName = "";
        private StringBuilder m_queryBuilder = new StringBuilder();

        private int m_buffSize;
        private int m_lineCount = 0;
        private int m_dataCount = 0;

        private bool m_newQueryCycle = true;

        public SQLWriter(Servers server, QueryType queryType, String tableName, int buffSize = 100)
        {
            m_queryType = queryType;
            m_buffSize = buffSize;
            m_server = server;
            m_tableName = tableName;

        }


        private void completeQuery()
        {
            switch (m_queryType)
            {
                case QueryType.INSERT:
                    m_qPattern = String.Format("INSERT INTO {0}", m_tableName) + getParameters() + "VALUES ";
                    break;
                case QueryType.CREATE:
                    m_qPattern = String.Format("CREATE TABLE {0}", m_tableName) + getParameters();
                    m_queryBuilder.Clear();
                    m_queryBuilder.Append(m_qPattern);
                    break;
                case QueryType.CREATE_IF_NOT_EXISTS:
                    m_qPattern = String.Format("IF ((SELECT OBJECT_ID('{0}', 'u')) IS NULL) BEGIN CREATE TABLE {0}", m_tableName) + getParameters() + " END;";
                    m_queryBuilder.Clear();
                    m_queryBuilder.Append(m_qPattern);
                    break;
                case QueryType.DROP:
                    m_qPattern = String.Format("DROP TABLE {0}", m_tableName);
                    m_queryBuilder.Clear();
                    m_queryBuilder.Append(m_qPattern);
                    break;

            }
        }

        /// <summary>
        /// Build string with parameters for query
        /// </summary>
        /// <returns>parameters string</returns>
        private String getParameters()
        {

            //if we have notparameters return empty string
            if (m_parameters.Count == 0)
            {
                return "";
            }

            StringBuilder parameters = new StringBuilder(" ( ");

            for (int i = 0; i < m_parameters.Count; i++)
            {
                parameters.Append(m_parameters[i]);

                if (i < m_parameters.Count - 1)
                    parameters.Append(", ");
                else
                    parameters.Append(" ) ");
            }


            return parameters.ToString();
        }

        /// <summary>
        /// Add new parameter to SQL query
        /// </summary>
        /// <param name="paramertName">parameter name</param>
        public void addParameters(String paramertName)
        {
            m_parameters.Add(paramertName);
            completeQuery();
        }

        public void addData(String data, bool unicode = false, bool normalize = true)
        {
            if (normalize)
                data = Regex.Replace(data, "'", "''");

            data = "'" + data + "'";

            if (unicode)
                data = "N" + data;

            applyData(data);
        }


        public void addData(Int32 data)
        {
            applyData(data.ToString());
        }

        public void addData(Guid data)
        {
            applyData( "'{" + data.ToString() + "}'" );
        }


        /// <summary>
        /// Apply string-data to target queru
        /// </summary>
        /// <param name="data">data</param>
        private void applyData(String data)
        {
            //if it is a new query cycle - use pattern for query builder
            if (m_newQueryCycle)
            {
                m_queryBuilder.Clear();
                m_queryBuilder.Append(m_qPattern);
                m_newQueryCycle = false;
            }

            //if it is first value in current line - add "(" to query
            if (m_dataCount == 0)
            {
                m_queryBuilder.Append(" ( ");
            }

            //add data here!
            m_queryBuilder.Append(data);
            m_dataCount++;

            //if it is last value in current line - add ")" to query and send it to writing! else add ","
            if (m_dataCount == m_parameters.Count)
            {
                m_queryBuilder.Append(" ) ");
                m_dataCount = 0;
                write();
                m_queryBuilder.Append(", ");
            }
            else
            {
                m_queryBuilder.Append(", ");
            }
        }


        /// <summary>
        /// Write data into DB
        /// </summary>
        /// <param name="forceWrite">Set true if want force write data</param>
        public void write(bool forceWrite = false)
        {

            m_lineCount++;

            if ((m_lineCount % m_buffSize == 0) || forceWrite)
            {

                using (SqlConnection conn = new SqlConnection(getConnStr(m_server)))
                {
                    conn.Open();
                    //remove last " , "
                    String query = Regex.Replace(m_queryBuilder.ToString(), @"\s?,\s?$", "");
                    SqlCommand cmd = new SqlCommand(query , conn);

                    cmd.ExecuteNonQuery();

                }


                m_lineCount = 0;
                m_newQueryCycle = true;
            }
        }



        /// <summary>
        /// Return connection string by server name
        /// </summary>
        /// <param name="server">Server name</param>
        /// <returns>connection string for current server</returns>
        private String getConnStr(Servers server)
        {
            String serv = "";

            switch (server)
            {
                case Servers.Server28:
                    serv = ConnectBuilderServer28.ToString();
                    break;
            }


            return serv;
        }


    }


    public enum Servers
    {
        Server28,
        NoServer
    }

    public enum QueryType
    {
        INSERT = 0,
        UPDATE,
        CREATE,
        CREATE_IF_NOT_EXISTS,
        DROP,
        NO_TYPE
    };


}
