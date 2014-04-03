using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinderEngineSharp;
using FinderEngineSharp.Data;
using FinderEngineSharp.Utils;
using TokenizeUtils;
using System.Data.SqlClient;
using SearcherEngine.Interfaces;

namespace SearcherEngine.Classes
{
    public class Searcher
    {
        #region Data
        protected TokenizeOptions m_tokOptions;
        protected Tokenizer m_tokenizer;

        protected IDataProvider m_inputDataProvider;
        protected IOutDataProvider m_outputDataProvider;
        protected FinderEngine<IData> m_finderEngine;

        protected List<IData> m_outputList = new List<IData>();

        //SQL members
        protected SqlConnectionStringBuilder m_connStrBld;
        protected SQLExecuter m_executor;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inputDataProvider">DataProvider - factory, which can produce you IData type</param>
        public Searcher(IDataProvider inputDataProvider, IOutDataProvider outputDataProvider)
        {
            m_inputDataProvider = inputDataProvider;
            m_outputDataProvider = outputDataProvider;
            m_outputDataProvider.setResultList(m_outputList);

            m_tokOptions = new TokenizeOptions(new List<KeyValuePair<string, string>>(0), SharedTypes.CaseSensType.CIgnore);
            m_tokenizer = new Tokenizer(m_tokOptions);
            

            InitializeOptions<IData> initOptions;
            initOptions = new InitializeOptions<IData>(m_tokenizer, StringComparer.InvariantCultureIgnoreCase,
                (a, b) => { if (!a.UserData.Contains(b[0])) a.UserData.Add(b[0]); }
                , (rdr) => { return m_inputDataProvider.produce(rdr); });
            m_finderEngine = new FinderEngine<IData>(initOptions);
        }

        public void learnFE()
        {

        }

        /// <summary>
        /// Learn FE one by one word
        /// </summary>
        /// <param name="token">String with word for dictionary</param>
        /// <param name="cortage">cortage data for current token</param>
        public void learnFE(String token, IData cortage)
        {
            m_finderEngine.AddElement(m_tokenizer.Tokenize(token).Toks, cortage);
        }

        /// <summary>
        /// Learn finder engine uses words from SQL query
        /// </summary>
        /// <param name="cmd">Command for SQL query</param>
        public void learnFE_DB()
        {
            if (m_inputDataProvider.SearchConnStr != null && m_inputDataProvider.SearchConnStr != String.Empty)
            {
                using (SqlConnection conn = new SqlConnection(m_inputDataProvider.SearchConnStr))
                {
                    SqlCommand cmd = new SqlCommand(m_inputDataProvider.TargetSqlQuery);
                    cmd.Connection = conn;
                    conn.Open();
                    m_finderEngine.AddFromSql(cmd);
                }
            }
            else
            {
                Console.WriteLine("Searcher haven't connection string settings");
            }
        }

        public void init()
        {
            m_finderEngine.Init();
        }

        /// <summary>
        /// Search matches with input from string 
        /// </summary>
        /// <param name="strInput">string for searching</param>
        public void Search(String strInput)
        {
            List<ResultInfo<IData>> result = m_finderEngine.PyramidalSearch(m_tokenizer.Tokenize(strInput));

            //if we haven't any result - declare it
            if (result.Count == 0)
            {
                m_outputDataProvider.declareNegative();
            }

            //declare result
            foreach (ResultInfo<IData> res in result)
            {
                m_outputDataProvider.declareResult(res);
            }
        }

        /// <summary>
        /// Search matches with input from DataBase
        /// </summary>
        /// <param name="searchImput">SQLCommand for providing input</param>
        /// <param name="searchColumnIndex">Index from input line for comparing</param>
        public void SearchDB()
        {

            using (SqlConnection conn = new SqlConnection(m_outputDataProvider.SearchConnStr))
            {
                SqlCommand searchImput = new SqlCommand(m_outputDataProvider.TargetSqlQuery);
                conn.Open();
                searchImput.Connection = conn;
                SqlDataReader reader = searchImput.ExecuteReader();

                while (reader.Read())
                {
                    String strInput = reader.GetString(m_outputDataProvider.TargetIndex);
                    List<ResultInfo<IData>> result = m_finderEngine.PyramidalSearch(m_tokenizer.Tokenize(strInput));

                    //if we haven't any result - declare it
                    if (result.Count == 0)
                    {
                        m_outputDataProvider.declareNegative();
                    }

                    //declare result
                    foreach (ResultInfo<IData> res in result)
                    {
                        m_outputDataProvider.declareResult(reader, res);
                    }
                }
            }
        }
    }
}
