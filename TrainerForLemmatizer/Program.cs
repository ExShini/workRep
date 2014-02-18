using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Threading;
using ThreadWorkController;
using LemmaSharp;

namespace TrainerForLemmatizer
{
    class Program
    {
        static List<String> prepareWordForm = new List<String>();


        //data for checking
        static ReaderWriterLock rwl = new ReaderWriterLock();
        static String positivePattern = @"(^[а-я]+$)";
        //static String negativePattern = @"(\.|\,|\\|[a-z]|[A-Z]|[0-9])";
        static Regex reg;
        static List<Object> wordForms = new List<Object>();
        static List<Object> pairs = new List<Object>();

        static int fnum, pnum, processed;

        static void Main(string[] args)
        {

            String connectionString = "Persist Security Info=False;Integrated Security=true;Initial Catalog=;server=server28";
            {
                List<String> freshWordForms = new List<String>();

                //read word form from Data Base and save it to wordForms list.
                using (SqlConnection conn = new SqlConnection(connectionString))
                {

                    string queryString = "SELECT DISTINCT AttribValue FROM Attrib72 WHERE AttribValue IS NOT NULL;";

                    SqlCommand command = new SqlCommand(queryString, conn);
                    conn.Open();

                    SqlDataReader reader = command.ExecuteReader();

                    Console.WriteLine("Read word form = ###");

                    fnum = 0;
                    while (reader.Read())
                    {
                        freshWordForms.Add((String)reader[0]);
                        fnum++;
                        Console.Write("\r{0}", fnum);
                    }

                }

                //check the applicability 
                reg = new Regex(positivePattern);
                pnum = 0;
                processed = 0;

                Console.WriteLine("\nWord validation\nProcessed ### from ### - added ###");
                Parallel.ForEach<String>(freshWordForms, ChecWord);
            }

            /////////////////////////////////////////////////////////////////////////////
            //lemmatize 
            Console.WriteLine("\nPrepare pair lemma-wordform");

            int numberOfWorkers = 10;
            ThreadController thrConrl = new ThreadController();

            for (int i = 0; i < numberOfWorkers; i++)
            {
                thrConrl.addWorker(new LemmaTrainerWorker(connectionString));
            }
            thrConrl.setData(wordForms);
            thrConrl.executeWorks();
            pairs = thrConrl.getResult();


            ///////////////////////////////////////////////////////////////////////////
            #region old pare prepaer
            /*
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string queryString = "SELECT I.Name "
                    + "FROM dbo.Attrib72 A WITH(NOLOCK)INNER JOIN ItemList I WITH(NOLOCK) ON I.ID = A.IdItem "
                    + "where I.UP = 18 AND A.AttribValue = \'";
                StringBuilder bld = new StringBuilder();
                int counter = 0;

                foreach (String str in wordForms)
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
                            pairs.Add(new Pair((String)reader[0], str));
                            counter++;
                            Console.Write("\r{0}", counter);
                            counter++;
                        }
                    }
                    reader.Close();

                }
                
            }
            */
            #endregion
            ///////////////////////////////////////////////////////////////////////////

            LemmatizerPrebuiltFull lemmatizer = new LemmatizerPrebuiltFull(LanguagePrebuilt.Russian);

            Console.WriteLine("\nLearning...");
            foreach (Object obj in pairs)
            {
                Pair pair = (Pair)obj;
                lemmatizer.AddExample(pair.WordForm, pair.Lemma);
            }

            Console.ReadLine();
        }


        static void ChecWord(String str)
        {
            str.ToLower();
            Interlocked.Increment(ref processed);
            if (reg.IsMatch(str))
            {
                rwl.AcquireWriterLock(-1);
                wordForms.Add(str);
                pnum++;
                Console.Write("\r{0} / {1} - {2}", processed, fnum, pnum);
                rwl.ReleaseReaderLock();
            }
        }
    }
}
