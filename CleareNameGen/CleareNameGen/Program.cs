using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using GenParadigmCollection;

namespace CleareNameGen
{
    class Program
    {
        static void Main(string[] args)
        {
            String connectionString = "Persist Security Info=False;Integrated Security=true;Initial Catalog=;server=server28";
            SortedDictionary<int, String> dict = new SortedDictionary<int, string>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string queryString = "SELECT ID, Text1 from ItemList where Ind = \'сельское поселение\' and UP=55 and Text1 IS NOT NULL;";
                SqlCommand command = new SqlCommand(queryString, conn);
                SqlDataReader reader = command.ExecuteReader();

                Console.WriteLine("Read names from DB = ###");

                int num = 0;
                while (reader.Read())
                {
                    dict.Add((int)reader[0], (String)reader[1]);
                    num++;
                    Console.Write("\r{0}", num);
                }
                reader.Close();


                ///////
                queryString = "select ID from YugoReport_ImportReport515;";
                command = new SqlCommand(queryString, conn);
                reader = command.ExecuteReader();

                Console.WriteLine("\nRemove names from YugoReport_ImportReport515 = ###");

                num = 0;
                while (reader.Read())
                {
                    if (dict.Remove((int)reader[0]))
                    {
                        num++;
                        Console.Write("\r{0}", num);
                    }
                }
                reader.Close();

                ///////
                queryString = "select ID from YugoReport_ImportReport516;";
                command = new SqlCommand(queryString, conn);
                reader = command.ExecuteReader();

                Console.WriteLine("\nRemove names from YugoReport_ImportReport516 = ###");

                num = 0;
                while (reader.Read())
                {
                    if (dict.Remove((int)reader[0]))
                    {
                        num++;
                        Console.Write("\r{0}", num);
                    }
                }
                reader.Close();
            }

            ///////////

            String positivePattern = @"(во$)";
            Regex reg = new Regex(positivePattern);
            SortedDictionary<int, String> result = new SortedDictionary<int, string>();

            ParadigmGenerator gen = new ParadigmGenerator();
            List<ParadigmGenerator.Paradigm> lpar;
            int id = 0;

            Console.WriteLine("\nProcessing...");
            foreach (var item in dict)
            {
                id++;
                if (reg.IsMatch(item.Value))
                {
                    String res = reg.Replace(item.Value, "ва");
                    result.Add(item.Key, res);
                }
                else
                {
                    lpar = gen.GetDeclinationAdjectivesAndNoun_WithParam(item.Value);
                    result.Add(item.Key, lpar[0].word);
                }
                Console.Write("\r{0}", id);
            }


            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

    
                Console.WriteLine("\nWrite to DB");
                id = 0;
                foreach (var dic in dict)
                {
                    int id220 = 0;


                    string comLine220 = "INSERT INTO Attrib220 (AttribValue, IdItem, Ru, IsCleanName) VALUES (" +
                                                             /*id220.ToString() + */ "\'" + 
                                                             dic.Value + "\', " + 
                                                             dic.Key.ToString() + ", " +
                                                             "1, 1);";

                    SqlCommand cmd220 = new SqlCommand(comLine220);
                    cmd220.Connection = conn;
                    cmd220.ExecuteNonQuery();

                    String getNewId = "select ID from Attrib220 where AttribValue = \'" + dic.Value + "\' AND IdItem = " + dic.Key.ToString() + ";";
                    SqlCommand findId = new SqlCommand(getNewId, conn);
                    SqlDataReader readNewId = findId.ExecuteReader();

                    if (readNewId.Read())
                    {
                        id220 = (int)readNewId[0];
                    }
                    else
                    {
                        Console.WriteLine("Baaad :(");
                    }
                    readNewId.Close();

                    String attrVal222;
                    result.TryGetValue(dic.Key, out attrVal222);


                    string comLine222 = "INSERT INTO Attrib222 (AttribValue, Attr220, CaseR) VALUES (" +
                                         /*id222.ToString() + */ "\'" +
                                         attrVal222 + "\', " +
                                         id220.ToString() + ", " +
                                         "1);";

                    
                    SqlCommand cmd222 = new SqlCommand(comLine222);
                    cmd222.Connection = conn;
                    cmd222.ExecuteNonQuery();

                    id++;

                    Console.Write("\r{0}", id);
                }





                /*
                string comLine = "INSERT INTO YugoReport_Omonim_Stem (id, idItem, descriptor, omonim, omonIdItem, language) VALUES ("
 + id.ToString() + ", " + curRec.idItem + ", "
 + quote + desc.ToString() + quote + ", "
 + quote + omon.ToString() + quote + ", "
 + curRec.idItemOmon + ", "
 + (int)curRec.omonLanguage + ");";

                try
                {
                    cmd.CommandText = comLine;
                    cmd.ExecuteNonQuery();
                    Console.Write("\r{0}", id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nERROR: Can't write to DB. Exception = {0}", ex.ToString());
                }
                */
            }




            Console.ReadLine();
        }


        static int getIdForTable(String TableName, SqlConnection conn)
        {
            
            string queryString = "select MAX(Id) from " + TableName + ";";
            SqlCommand command = new SqlCommand(queryString, conn);
            SqlDataReader reader = command.ExecuteReader();

            int id = 0;
            if (reader.Read())
            {
                id = (int)reader[0];
            }
            else
            {
                Console.WriteLine("ERROR!!!!!!!!!");
                Console.ReadLine();
                return 0;
            }
            reader.Close();

            return id;
        }

    }
}
