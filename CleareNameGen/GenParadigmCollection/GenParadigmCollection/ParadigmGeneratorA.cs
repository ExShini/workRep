using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using   GenParadigmCollection;

namespace GenParadigmCollection //от Арефьева
{
    public partial class ParadigmGenerator
    {
        static void Main(string[] args)  // тесты генераторов
        {
            Console.WriteLine(GenParadigmCollection.ParadigmGenerator.GetInCorrectRegister("КОмПАНИЯ, ФонД", "компаНиями, фОндом"));
            GenParadigmCollection.ParadigmGenerator pg = new ParadigmGenerator();
            List<GenParadigmCollection.ParadigmGenerator.Paradigm> ListParadigm = pg.GetAllDeclinationWithNumber_WithParam("синий", 2);
            List<string> List;
            while (1 == 1)
            {
                string r = Console.ReadLine();
                Console.WriteLine();
                //ListParadigm = pg.GetAllDeclinationWithNumber(r, 2); 
                List = pg.GetDeclinationAdjectivesAndNoun("8-ой апелляционный арбитражный суд");
                ListParadigm = pg.GetDeclinationAdjectivesAndNounWithDash_WithParam(r);
                List = pg.GetDeclinationAdjectivesAndNounWithDash("минюстиции");
                //ListParadigm = pg.GetAllDeclinationCompanyName_WithParam(r);
                ListParadigm = pg.GetAllDeclinationCompanyName_WithParam("ТНГ-Групп");
                ListParadigm = pg.GetAllDeclinationCompanyName_WithParam("ТНГ -Групп");
                ListParadigm = pg.GetAllDeclinationCompanyName_WithParam("ТНГ- Групп");
                ListParadigm = pg.GetAllDeclinationCompanyName_WithParam("ТНГ - Групп");
                //ListParadigm = pg.GetDeclinationAdjectivesAndNoun_Plural(r, null);
                //ListParadigm = pg.GetAllDeclinationWithNumber(r, 2);
                //ListParadigm = pg.GetDeclinationAdjectivesAndNoun(r);
                foreach (GenParadigmCollection.ParadigmGenerator.Paradigm p in ListParadigm.OrderBy(b => b.number).ThenBy(b => b.case_w))
                {
                    Console.WriteLine(p.word + "   " + p.case_w + "  " + p.number);
                }
                Console.WriteLine("----------------");
            }
        }

        public enum EParadigmGeneratorMode
        {
            EModeNone = 0,
            EModeStateInstitutions,
            EModeGeoRussia,
            EModeSecondName, // Справочник фамилий. Делаю отдельный режим на случай возникновения несоответствий с географией или др. справочниками.
            EModeOther
        };

        private static EParadigmGeneratorMode genMode = EParadigmGeneratorMode.EModeNone;
        private Dictionary<string, int> HomonimyInWords_Case;
        private Dictionary<int, string> OPF;
        private Dictionary<int, string> VED;
        private Dictionary<string, List<string>> YugoReport_srg_ALIN_1962;
        private List<IdParadigm> OPFparadigm;

        public EParadigmGeneratorMode GenMode
        {
            get
            {
                return genMode;
            }
            set
            {
                genMode = value;
            }
        }

        public class Paradigm
        {
            public string word;
            public int case_w;
            public int number;
            public int CodeOfSpeech;
            public int gender;
            public Paradigm(string _word, int _case_w, int _number)
            {
                word = _word;
                case_w = _case_w;
                number = _number;
            }
            public Paradigm(Paradigm _par)
            {
                word = _par.word;
                case_w = _par.case_w;
                number = _par.number;
                CodeOfSpeech = _par.CodeOfSpeech;
                gender = _par.gender; 
            }
        }

        public class IdParadigm
        {
            public int ID;
            public string word;
            public int case_w;
            public int number;
            public IdParadigm(int _ID, string _word, int _case_w, int _number)
            {
                ID = _ID;
                word = _word;
                case_w = _case_w;
                number = _number;
            }
        }

        public class Dash
        {
            public string leftSpace;
            public string dash;
            public string rightSpace;
            public Dash(string _left, string _dash, string _right)
            {
                leftSpace = _left;
                dash = _dash;
                rightSpace = _right;
            }
        }

        public ParadigmGenerator(EParadigmGeneratorMode genModeA = EParadigmGeneratorMode.EModeOther)
        {
            HomonimyInWords_Case = new Dictionary<string, int>();
            LoadHomonimyInWords(ref HomonimyInWords_Case);
            OPF = new Dictionary<int, string>();
            VED = new Dictionary<int, string>();
            YugoReport_srg_ALIN_1962 = new Dictionary<string, List<string>>();
            OPFparadigm = new List<IdParadigm>();
            LoadOPF(ref OPF, ref VED, ref OPFparadigm);
            LoadYugoReport_srg_ALIN_1962(ref YugoReport_srg_ALIN_1962);
            genMode = genModeA;
        }

        private static string GetStringConnection()
        {
            SqlConnectionStringBuilder bldr = new SqlConnectionStringBuilder();
            bldr.DataSource = "datamining";
            bldr.InitialCatalog = "agia";
            bldr.UserID = "dev";
            bldr.Password = "dev123";
            //bldr.IntegratedSecurity = true;
            return bldr.ConnectionString;
        }

        private static void LoadHomonimyInWords(ref Dictionary<string, int> HomonimyInWords_Case)
        { 
            String ConnectionString = GetStringConnection();
            SqlConnection connection;
            connection = new SqlConnection(ConnectionString);
            SqlCommand cmd = new SqlCommand(
@"select H.word, Min(LA.CodeOfSpeech) code from LemmasInArticles LA with(nolock)
join HomonimyInWords H with(nolock) on LA.lemma_id = H.lemma_id group by H.word", connection);

            cmd.CommandTimeout = int.MaxValue;

            if (connection != null)
                connection.Close();

            try
            {
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    HomonimyInWords_Case.Add(reader[0].ToString().ToLower().Trim(), (int)reader[1]);
                    // выше качество, но на 60% затратнее 
                    //string newKey = PrepareData(reader[0].ToString().ToLower());
                    //if (!HomonimyInWords_Case.ContainsKey(newKey))
                    //    HomonimyInWords_Case.Add(newKey, (int)reader[1]);
                }
                reader.Close();
            }
            catch (Exception exc)
            {
                Console.WriteLine("LoadHomonimyInWords " + cmd.CommandText + " * " + exc.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private static void LoadOPF(ref Dictionary<int, string> OPF, ref Dictionary<int, string> VED, ref List<IdParadigm> OPFparadigm)
        { 
            String ConnectionString = GetStringConnection();
            SqlConnection connection;
            connection = new SqlConnection(ConnectionString);
            SqlCommand cmd = new SqlCommand("select ID, Name from agia.dbo.itemlist with(nolock) where up = 0 order by len(Name) desc", connection);
            cmd.CommandTimeout = int.MaxValue;
            if (connection != null)
                connection.Close();
            try
            {
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    OPF.Add((int)reader[0], PrepareData(reader[1].ToString().ToLower()));
                }
                reader.Close();
                cmd.CommandText = "select ID, Name from agia.dbo.itemlist with(nolock) where up = 5 order by len(Name) desc";
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    VED.Add((int)reader[0], PrepareData(reader[1].ToString().ToLower()));
                }
                reader.Close();
                // У почти все парадигмы относятся к ОПФ
                cmd.CommandText = "select IdItem, AttribValue, 0 case_w, case when number is null then -1 else number end number from Attrib20 where caseI = 1 " +
                    "union select IdItem, AttribValue, 2, case when number is null then -1 else number end from Attrib20 with(nolock) where caseR = 1 " +
                    "union select IdItem, AttribValue, 5, case when number is null then -1 else number end from Attrib20 with(nolock) where caseT = 1 " +
                    "union select IdItem, AttribValue, 6, case when number is null then -1 else number end from Attrib20 with(nolock) where caseV = 1 " +
                    "union select IdItem, AttribValue, 7, case when number is null then -1 else number end from Attrib20 with(nolock) where caseD = 1 " +
                    "union select IdItem, AttribValue, 8, case when number is null then -1 else number end from Attrib20 with(nolock) where caseP = 1 " +
                    "union select IdItem, AttribValue, -1, case when number is null then -1 else number end from Attrib20 with(nolock) where caseI is null or caseR is null or caseT is null or caseV is null or caseD is null or caseP is null " +
                    "order by IdItem";
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    OPFparadigm.Add(new IdParadigm((int)reader[0], PrepareData(reader[1].ToString().ToLower()), (int)reader[2], (int)reader[3]));
                }
                reader.Close();
            }
            catch (Exception exc)
            {
                Console.WriteLine("LoadHomonimyInWords " + cmd.CommandText + " * " + exc.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private static void LoadYugoReport_srg_ALIN_1962(ref Dictionary<string, List<string>> YugoReport_srg_ALIN_1962)
        { 
            // загрузка сокращений для типа объекта справочника географии России
            String ConnectionString = GetStringConnection();
            SqlConnection connection;
            connection = new SqlConnection(ConnectionString);
            SqlCommand cmd = new SqlCommand("select Name, Socr from YugoReport_srg_ALIN_1962 with(nolock) order by Name", connection);
            cmd.CommandTimeout = int.MaxValue;
            if (connection != null)
                connection.Close();
            try
            {
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string name = reader[0].ToString();
                    string socr = reader[1].ToString();
                    if (YugoReport_srg_ALIN_1962.ContainsKey(name))
                        YugoReport_srg_ALIN_1962[name].Add(socr);
                    else
                        YugoReport_srg_ALIN_1962.Add(name, new List<string> { socr });
                }
                reader.Close();
            }
            catch (Exception exc)
            {
                Console.WriteLine("LoadYugoReport_srg_ALIN_1962 " + cmd.CommandText + " * " + exc.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private static void GetTable(ref List<string> Arr, string query)
        {
            String ConnectionString = GetStringConnection();
            SqlConnection connection;
            connection = new SqlConnection(ConnectionString);
            String selectQuery = query;
            SqlCommand cmd = new SqlCommand(selectQuery, connection);
            cmd.CommandTimeout = int.MaxValue;
            if (connection != null)
                connection.Close();
            try
            {
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string item = (string)reader[0];
                    Arr.Add(item);
                }
                reader.Close();
            }
            catch (Exception exc)
            {
                Console.WriteLine("GetTableId " + cmd.CommandText + " * " + exc.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        public static List<Paradigm> GetDeclinationNoun_WithParam(string word)
        {
            // входные данные - сущ в ед.ч. именительном падеже только из киррилических символов без пробелов
            // выходные - ед.ч. все падежи кроме Именительного
            List<Paradigm> ListParadigm = new List<Paradigm>();
            word = PrepareData(word);
            if (word == string.Empty || Regex.IsMatch(word, "[^А-ЯЁа-яё—-]"))
                return ListParadigm;
            //Собственные на -ово (-ёво), -ево, -аго (-яго), -ых (-их), -ко
            if (Regex.IsMatch(word, "([оёе]во|[ая]го|[иы]х)$"))
                return ListParadigm;
            foreach (string par in GetDeclinationNoun(word, 2))
                ListParadigm.Add(new Paradigm(par, 2, 0));
            foreach (string par in GetDeclinationNoun(word, 7))
                ListParadigm.Add(new Paradigm(par, 7, 0));
            foreach (string par in GetDeclinationNoun(word, 6))
                ListParadigm.Add(new Paradigm(par, 6, 0));
            foreach (string par in GetDeclinationNoun(word, 5))
                ListParadigm.Add(new Paradigm(par, 5, 0));
            foreach (string par in GetDeclinationNoun(word, 8))
                ListParadigm.Add(new Paradigm(par, 8, 0));
            return ListParadigm;
        }

        public static List<string> GetDeclinationNoun(string word)
        {
            // входные данные - сущ в ед.ч. именительном падеже только из киррилических символов без пробелов
            // выходные - ед.ч. все падежи кроме Именительного, без дублей
            List<string> ListParadigm = new List<string>();
            word = PrepareData(word);
            if (word == string.Empty || Regex.IsMatch(word, "[^А-ЯЁа-яё—-]"))
                return ListParadigm;
            //Собственные на -ово (-ёво), -ево, -аго (-яго), -ых (-их), -ко
            if (Regex.IsMatch(word, "([оёе]во|[ая]го|[иы]х)$"))
                return ListParadigm;
            ListParadigm.AddRange(GetDeclinationNoun(word, 2));
            ListParadigm.AddRange(GetDeclinationNoun(word, 7));
            ListParadigm.AddRange(GetDeclinationNoun(word, 6));
            ListParadigm.AddRange(GetDeclinationNoun(word, 5));
            ListParadigm.AddRange(GetDeclinationNoun(word, 8));
            ListParadigm.Sort();
            List<string> ListParadigmWithoutDoubles = new List<string>();
            string lastParadigm = string.Empty;
            foreach (string par in ListParadigm)
            {
                if (par != lastParadigm)
                    ListParadigmWithoutDoubles.Add(par);
                lastParadigm = par;
            }
            ListParadigm.Clear();
            return ListParadigmWithoutDoubles;
        }

        public static List<string> GetDeclinationNoun(string word, int case_w)
        {
            /*
                ALIN-2423: В сложных словоформах, содержащие фрагмент «-на-» / «- на -» / «- на-»/«-на -», изменяемым
                считается ТОЛЬКО фрагмент предшествующий первому дефису.
            */
            word = word.Trim();
            string wordws = word.Replace(" ", "");

            int defInd = wordws.IndexOf("-на-");

            if (defInd == -1)
            {
                if (Regex.IsMatch(wordws, "-[0-9A-Za-z]"))
                {
                    defInd = wordws.Replace(" ", "").IndexOf("-");
                }
            }
            if (defInd == -1)
            {
                return GetDeclinationNounForSimpleWord(word, case_w);
            }
            else
            {
                string sWord = word.Substring(0, defInd);
                string ostString = word.Replace(sWord, "");
                sWord = sWord.Trim();
                List<string> ret = new List<string>();

                if (Regex.IsMatch(sWord, "(ий)|(ый)$"))
                {
                    ret = GetDeclinationAdjective(sWord, case_w);
                }
                else
                    ret = GetDeclinationNoun(sWord, case_w);

                List<string> rt = new List<string>();

                foreach (string var in ret)
                {
                    rt.Add(var + ostString);
                }
                return rt;
            }
        }

        private static List<string> GetDeclinationNounForSimpleWord(string word, int case_w)
        {
            // входные данные - сущ в ед.ч. именительном падеже только из киррилических символов без пробелов
            // выходные - ед.ч.
            List<string> ListParadigm = new List<string>();
            word = PrepareData(word);
            /*
                  ALIN-2421: Неизменяемыми должны считаться словоформы:
                 «Депобрразвития, Депфинансов, Депэкономразвития, Госкомзанятости, Госкомобеспечения, Госкоминформатизации, Госкомприроды,
                  Госкоммолодежи, Госкомюстиции, Госкомэкономики, Госкомэкологии, Госкомсвязи, Депполитики, Депохоты, Депимущества,
                  депсоцразвития, Дептруда»               
            */
            string[] excInstitutions = { "депобрразвития", "депфинансов", "депэкономразвития", "госкомзанятости", "госкомобеспечения",
                                         "госкоминформатизации", "госкомприроды", "госкоммолодежи", "госкомюстиции", "госкомэкономики",
                                         "госкомэкологии", "госкомсвязи", "депполитики", "депохоты", "депимущества", "депсоцразвития", "дептруда"
                                       };
            if (excInstitutions.Contains(word.ToLower()))
            {
                ListParadigm.Add(word);
                return ListParadigm;
            }
            // ALIN-2156 : требуется генерация парадигм для составных слов типа "TV-канал" или "Internet-сайт"
            if (word == string.Empty || word.Length < 3 || Regex.IsMatch(word, "([А-ЯЁа-яё][А-ЯЁа-яё])$") == false)                
            {
                return ListParadigm;
            }
            if (case_w != 2 && case_w != 7 && case_w != 6 && case_w != 5 && case_w != 8)
                return ListParadigm;
            //Собственные на -ово (-ёво), -ево, -аго (-яго), -ых (-их), -ко
            if (Regex.IsMatch(word, "([оёе]во|[ая]го|[иы]х)$"))
                return ListParadigm;
            string suffix1 = word.Substring(word.Length - 1, 1).ToLower();
            string suffix2 = word.Substring(word.Length - 2, 2).ToLower();
            string suffix3 = word.Substring(word.Length - 3, 3).ToLower();

            switch (suffix3)
            {
                case "дра":
                    switch (case_w)
                    {
                        case 2: { ListParadigm.Add(word.Substring(0, word.Length - 1)); }
                            break;
                        case 7: { ListParadigm.Add(word.Substring(0, word.Length - 3) + "драм"); }
                            break;
                        case 6: { ListParadigm.Add(word); }
                            break;
                        case 5: { ListParadigm.Add(word.Substring(0, word.Length - 3) + "драми"); }
                            break;
                        case 8: { ListParadigm.Add(word.Substring(0, word.Length - 3) + "драх"); }
                            break;
                    }
                    break;
                // ALIN-2172 :слово «рабочий» должно склоняться как прилагательное м.р. мягкого типа. Т.е. рабочий-рабочего-рабочему-рабочий-рабочим-рабочем
                case "чий":
                    switch (case_w)
                    {
                        case 2: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "его"); }
                            break;
                        case 7: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ему"); }
                            break;
                        case 6: { ListParadigm.Add(word); }
                            break;
                        case 5: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "им"); }
                            break;
                        case 8: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ем"); }
                            break;
                    }
                    break;
            }
            if (ListParadigm.Count() > 0)
            {
                return ListParadigm;
            }
            switch (suffix2)
            {
                case "ки":
                case "хи":
                case "ги":
                    {
                        switch (case_w)
                        {
                            case 2: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ов"); }
                                break;
                            case 7: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ам"); }
                                break;
                            case 6: { ListParadigm.Add(word); }
                                break;
                            case 5: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ами"); }
                                break;
                            case 8: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ах"); }
                                break;
                        }
                    }
                    break;

                case "жи":
                case "ши":
                case "чи":
                case "щи":
                    {
                        switch (case_w)
                        {
                            case 2:
                                {
                                    ListParadigm.Add(word.Substring(0, word.Length - 1));
                                    // при склонении на чи, щи, ши, жи надо добавить вариант с Р.п. на -ей
                                    ListParadigm.Add(word.Substring(0, word.Length - 1) + "ей");
                                }
                                break;
                            case 7: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ам"); }
                                break;
                            case 6: { ListParadigm.Add(word); }
                                break;
                            case 5: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ами"); }
                                break;
                            case 8: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ах"); }
                                break;
                        }
                    }
                    break;

                case "ай":
                case "ой":
                case "эй":
                    {
                        switch (case_w)
                        {
                            case 2: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "я"); }
                                break;
                            case 7: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ю"); }
                                break;
                            case 6: { ListParadigm.Add(word); }
                                break;
                            case 5: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ем"); }
                                break;
                            case 8: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "е"); }
                                break;
                        }
                    }
                    break;
                
                case "ио": // заимствованные слова типа радио и.т.п.
                    {
                        switch (case_w)
                        {
                            case 2:
                            case 7:
                            case 6:
                            case 5:
                            case 8:
                                { 
                                    ListParadigm.Add(word.Substring(0, word.Length - 1) + "о");
                                }
                                break;
                        }
                    }
                    break;
                // ALIN-2172 : cледует написать алгоритм склонения существительных на «-ье».
                case "ье":
                    {
                        switch (case_w)
                        {
                            case 2:                                
                                    ListParadigm.Add(word.Substring(0, word.Length - 1) + "я");
                                break;
                            case 7: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ю"); }
                                break;
                            case 6: { ListParadigm.Add(word); }
                                break;
                            case 5: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ем"); }
                                break;
                            case 8: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "е"); }
                                break;
                        }
                    }
                    break;

                case "ия":
                    {
                        switch (case_w)
                        {
                            case 2: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "и"); }
                                break;
                            case 7: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "и"); }
                                break;
                            case 6: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ю"); }
                                break;
                            case 5: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ей"); }
                                break;
                            case 8: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "и"); }
                                break;
                        }
                    }
                    break;
                case "ий":
                case "ие":
                    {
                        switch (case_w)
                        {
                            case 2: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "я"); }
                                break;
                            case 7: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ю"); }
                                break;
                            case 6: { ListParadigm.Add(word); }
                                break;
                            case 5: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ем"); }
                                break;
                            case 8: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "и"); }
                                break;
                        }
                    }
                    break;
                case "ии":
                    {
                        switch (case_w)
                        {
                            case 2:
                                {
                                    ListParadigm.Add(word.Substring(0, word.Length - 1) + "й");
                                    ListParadigm.Add(word.Substring(0, word.Length - 1) + "ев");
                                }
                                break;
                            case 7: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ям"); }
                                break;
                            case 6: { ListParadigm.Add(word); }
                                break;
                            case 5: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ями"); }
                                break;
                            case 8: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ях"); }
                                break;
                        }
                    }
                    break;
            }
            if (ListParadigm.Count != 0)
                return ListParadigm;
            else
                switch (suffix1)
                {
                    case "а":
                        {
                            if (Regex.IsMatch(suffix2, "[гкх]а", RegexOptions.IgnoreCase))
                                switch (case_w)
                                {
                                    case 2: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "и"); }
                                        break;
                                    case 7: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "е"); }
                                        break;
                                    case 6: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "у"); }
                                        break;
                                    case 5: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ой"); }
                                        break;
                                    case 8: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "е"); }
                                        break;
                                }
                            else if (Regex.IsMatch(suffix2, "[жшщч]а", RegexOptions.IgnoreCase))
                                switch (case_w)
                                {
                                    case 2: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "и"); }
                                        break;
                                    case 7: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "е"); }
                                        break;
                                    case 6: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "у"); }
                                        break;
                                    case 5: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ей"); }
                                        break;
                                    case 8: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "е"); }
                                        break;
                                }
                            else if (suffix2 == "ца")
                                switch (case_w)
                                {
                                    case 2: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ы"); }
                                        break;
                                    case 7: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "е"); }
                                        break;
                                    case 6: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "у"); }
                                        break;
                                    case 5: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ей"); }
                                        break;
                                    case 8: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "е"); }
                                        break;
                                }
                            else
                                switch (case_w)
                                {
                                    case 2: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ы"); }
                                        break;
                                    case 7: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "е"); }
                                        break;
                                    case 6: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "у"); }
                                        break;
                                    case 5: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ой"); }
                                        break;
                                    case 8: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "е"); }
                                        break;
                                }
                        }
                        break;
                    case "о":
                        {
                            switch (case_w)
                            {
                                case 2: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "а"); }
                                    break;
                                case 7: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "у"); }
                                    break;
                                case 6: { ListParadigm.Add(word); }
                                    break;
                                case 5: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ом"); }
                                    break;
                                case 8: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "е"); }
                                    break;
                            }
                        }
                        break;
                    case "я":
                        {
                            switch (case_w)
                            {
                                case 2: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "и"); }
                                    break;
                                case 7: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "е"); }
                                    break;
                                case 6: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ю"); }
                                    break;
                                case 5: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ей"); }
                                    break;
                                case 8: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "е"); }
                                    break;
                            }
                        }
                        break;
                    case "ы":
                        {
                            switch (case_w)
                            {
                                case 2:
                                    {
                                        ListParadigm.Add(word.Substring(0, word.Length - 1) + "ов");
                                        ListParadigm.Add(word.Substring(0, word.Length - 1));
                                    }
                                    break;
                                case 7: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ам"); }
                                    break;
                                case 6: { ListParadigm.Add(word); }
                                    break;
                                case 5: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ами"); }
                                    break;
                                case 8: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ам"); }
                                    break;
                            }
                        }
                        break;
                    case "и":
                        {
                            switch (case_w)
                            {
                                case 2:
                                    {
                                        ListParadigm.Add(word.Substring(0, word.Length - 1) + "ей");
                                        //ь со вставкой гласного Е перед конечным согласным основы, к которой присоединяется Ь
                                        ListParadigm.Add(word.Substring(0, word.Length - 2) + "е" + word.Substring(word.Length - 2, 1) + "ь");
                                    }
                                    break;
                                case 7: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ям"); }
                                    break;
                                case 6: { ListParadigm.Add(word); }
                                    break;
                                case 5: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ями"); }
                                    break;
                                case 8: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ях"); }
                                    break;
                            }
                        }
                        break;
                    case "ь":
                        {
                            // ALIN-2156: Добавим варианты 3-го склонения: сеть, снедь, сельдь
                            if (Regex.IsMatch(word, "([жшщч]ь|ость|еть|едь|льдь)$", RegexOptions.IgnoreCase))
                                //проверим на принадлежность к 3ьему склонению
                                switch (case_w)
                                {
                                    case 2: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "и"); }
                                        break;
                                    case 7: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "и"); }
                                        break;
                                    case 6: { ListParadigm.Add(word); }
                                        break;
                                    case 5: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ью"); }
                                        break;
                                    case 8: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "и"); }
                                        break;
                                }
                            else
                                //предлагаем все варианты
                                switch (case_w)
                                {
                                    case 2:
                                        {
                                            ListParadigm.Add(word.Substring(0, word.Length - 1) + "я");
                                            ListParadigm.Add(word.Substring(0, word.Length - 1) + "и");
                                        }
                                        break;
                                    case 7:
                                        {
                                            ListParadigm.Add(word.Substring(0, word.Length - 1) + "ю");
                                            ListParadigm.Add(word.Substring(0, word.Length - 1) + "и");
                                        }
                                        break;
                                    case 6: { ListParadigm.Add(word); }
                                        break;
                                    case 5:
                                        {
                                            ListParadigm.Add(word.Substring(0, word.Length - 1) + "ем");
                                            ListParadigm.Add(word.Substring(0, word.Length - 1) + "ью");
                                        }
                                        break;
                                    case 8:
                                        {
                                            ListParadigm.Add(word.Substring(0, word.Length - 1) + "е");
                                            ListParadigm.Add(word.Substring(0, word.Length - 1) + "и");
                                        }
                                        break;
                                }
                        }
                        break;
                    default:
                        {
                            bool deleteVowel = !((genMode == EParadigmGeneratorMode.EModeSecondName) && hasOnlyOneVowel(word));

                            if (Regex.IsMatch(suffix1, "[цкнгшщзхждлрпвфчсмтб]", RegexOptions.IgnoreCase));
                                switch (case_w)
                                {
                                    case 2:
                                        {
                                            ListParadigm.Add(word.Substring(0, word.Length) + "а");

                                            if (deleteVowel && Regex.IsMatch(word, "[ое]к$", RegexOptions.IgnoreCase))
                                            {
                                                ListParadigm.Add(word.Substring(0, word.Length - 2) + "ка");
                                            }                                           
                                            else if (Regex.IsMatch(word, "лец$", RegexOptions.IgnoreCase))
                                            {
                                                ListParadigm.Add(word.Substring(0, word.Length - 2) + "ьца");
                                            }
                                            else if (Regex.IsMatch(word, "ец$", RegexOptions.IgnoreCase))
                                            {
                                                ListParadigm.Add(word.Substring(0, word.Length - 2) + "ца");
                                            }
                                        }
                                        break;
                                    case 7:
                                        {
                                            ListParadigm.Add(word.Substring(0, word.Length) + "у");

                                        if (deleteVowel && Regex.IsMatch(word, "[ое]к$", RegexOptions.IgnoreCase))
                                        {
                                            ListParadigm.Add(word.Substring(0, word.Length - 2) + "ку");
                                        }
                                        else if (Regex.IsMatch(word, "лец$", RegexOptions.IgnoreCase))
                                        {
                                            ListParadigm.Add(word.Substring(0, word.Length - 2) + "ьцу");
                                        }
                                        else if (Regex.IsMatch(word, "ец$", RegexOptions.IgnoreCase))
                                        {
                                            ListParadigm.Add(word.Substring(0, word.Length - 2) + "цу");
                                        }
                                        }
                                        break;

                                    case 6:
                                        {
                                            if (deleteVowel)
                                            {
                                                ListParadigm.Add(word);
                                            }
                                            else
                                            {
                                                ListParadigm.Add(word + 'а');
                                            }
                                        }
                                        break;

                                    case 5:
                                        {
                                            ListParadigm.Add(word.Substring(0, word.Length) + "ом");

                                        if (deleteVowel && Regex.IsMatch(word, "[ое]к$", RegexOptions.IgnoreCase))
                                        {
                                            ListParadigm.Add(word.Substring(0, word.Length - 2) + "ком");
                                        }
                                        else if (Regex.IsMatch(word, "лец$", RegexOptions.IgnoreCase))
                                        {
                                            ListParadigm.Add(word.Substring(0, word.Length - 2) + "ьцом");
                                        }
                                        else if (Regex.IsMatch(word, "ец$", RegexOptions.IgnoreCase))
                                        {
                                            ListParadigm.Add(word.Substring(0, word.Length - 2) + "цом");
                                        }
                                        }
                                        break;

                                    case 8:
                                        {
                                            ListParadigm.Add(word.Substring(0, word.Length) + "е");

                                            if (deleteVowel && Regex.IsMatch(word, "[ое]к$", RegexOptions.IgnoreCase))
                                            {
                                                    ListParadigm.Add(word.Substring(0, word.Length - 2) + "ке");
                                            }
                                            else if (Regex.IsMatch(word, "лец$", RegexOptions.IgnoreCase))
                                            {
                                                    ListParadigm.Add(word.Substring(0, word.Length - 2) + "ьце");
                                            }
                                            else if (Regex.IsMatch(word, "ец$", RegexOptions.IgnoreCase))
                                            {
                                                    ListParadigm.Add(word.Substring(0, word.Length - 2) + "це");
                                            }
                                       }
                                    break;
                                }
                        }
                        break;
                }
            List<string> ListParadigmWithReg = new List<string>();
            foreach (string par in ListParadigm)
                ListParadigmWithReg.Add(GetInCorrectRegister(word, par));
            ListParadigm.Clear();
            return ListParadigmWithReg;
        }

        private static bool hasOnlyOneVowel(string word)
        {
            char[] Vowels = { 'а', 'е', 'ё', 'и', 'о', 'у', 'э', 'ю', 'я', 'А', 'Е', 'Ё', 'И', 'О', 'У', 'Э', 'Ю', 'Я' };
            int count = 0;

            for (int i = 0; i < word.Length; i++)
            {
                if (Vowels.Contains(word[i])) count++;
            }
            return (count == 1);
        }

        public static List<Paradigm> GetDeclinationNumeral_WithParam(string word)
        {
            // входные данные - числительное в ед.ч. именительном падеже только из киррилических символов и цифр без пробелов
            // выходные - ед.ч. все падежи кроме Именительного
            List<Paradigm> ListParadigm = new List<Paradigm>();
            word = PrepareData(word);
            if (word == string.Empty || Regex.IsMatch(word, "[^0-9А-ЯЁа-яё—-]"))
                return ListParadigm;
            foreach (string par in GetDeclinationNumeral(word, 2))
                ListParadigm.Add(new Paradigm(par, 2, 0));
            foreach (string par in GetDeclinationNumeral(word, 7))
                ListParadigm.Add(new Paradigm(par, 7, 0));
            foreach (string par in GetDeclinationNumeral(word, 6))
                ListParadigm.Add(new Paradigm(par, 6, 0));
            foreach (string par in GetDeclinationNumeral(word, 5))
                ListParadigm.Add(new Paradigm(par, 5, 0));
            foreach (string par in GetDeclinationNumeral(word, 8))
                ListParadigm.Add(new Paradigm(par, 8, 0));
            return ListParadigm;
            
        }

        private static List<string> GetDeclinationNumeral(string word)
        {
            // входные данные - числительное в ед.ч. именительном падеже только из киррилических символов и цифр без пробелов
            // выходные - ед.ч. все падежи кроме Именительного, без дублей
            List<string> ListParadigm = new List<string>();
            word = PrepareData(word);
            if (word == string.Empty || Regex.IsMatch(word, "[^0-9А-ЯЁа-яё—-]") || word.Length < 3)
                return ListParadigm;
            ListParadigm.AddRange(GetDeclinationNumeral(word, 2));
            ListParadigm.AddRange(GetDeclinationNumeral(word, 7));
            ListParadigm.AddRange(GetDeclinationNumeral(word, 6));
            ListParadigm.AddRange(GetDeclinationNumeral(word, 5));
            ListParadigm.AddRange(GetDeclinationNumeral(word, 8));
            ListParadigm.Sort();
            List<string> ListParadigmWithoutDoubles = new List<string>();
            string lastParadigm = string.Empty;
            foreach (string par in ListParadigm)
            {
                if (par != lastParadigm)
                    ListParadigmWithoutDoubles.Add(par);
                lastParadigm = par;
            }
            ListParadigm.Clear();
            return ListParadigmWithoutDoubles;
        }

        private static List<string> GetDeclinationNumeral(string word, int case_w)
        {
            // входные данные - числительное в ед.ч. именительном падеже только из киррилических символов и цифр без пробелов
            // выходные - ед.ч.
            List<string> ListParadigm = new List<string>();
            word = PrepareData(word);
            if (word == string.Empty || Regex.IsMatch(word, "[^0-9А-ЯЁа-яё—-]") || word.Length < 3)
                return ListParadigm;
            if (case_w != 2 && case_w != 7 && case_w != 6 && case_w != 5 && case_w != 8)
                return ListParadigm; 
            string suffix1 = word.Substring(word.Length - 1, 1).ToLower();
            string suffix2 = word.Substring(word.Length - 2, 2).ToLower();
            switch (suffix2)
            {
                case "ый":
                case "ой":
                    {
                        switch (case_w)
                        {
                            case 2: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ого"); }
                                break;
                            case 7: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ому"); }
                                break;
                            case 6: { ListParadigm.Add(word); }
                                break;
                            case 5: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ым"); }
                                break;
                            case 8: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ом"); }
                                break;
                        }
                    }
                    break;
                case "ий":
                    {
                        switch (case_w)
                        {
                            case 2: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "его"); }
                                break;
                            case 7: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ему"); }
                                break;
                            case 6: { ListParadigm.Add(word); }
                                break;
                            case 5: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "им"); }
                                break;
                            case 8: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ем"); }
                                break;
                        }
                    }
                    break;
                case "ая":
                    {
                        switch (case_w)
                        {
                            case 2: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ой"); }
                                break;
                            case 7: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ой"); }
                                break;
                            case 6: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ую"); }
                                break;
                            case 5: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ой"); }
                                break;
                            case 8: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ой"); }
                                break;
                        }
                    }
                    break;
                case "ое":
                    {
                        switch (case_w)
                        {
                            case 2: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ого"); }
                                break;
                            case 7: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ому"); }
                                break;
                            case 6: { ListParadigm.Add(word); }
                                break;
                            case 5: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ым"); }
                                break;
                            case 8: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ом"); }
                                break;
                        }
                    }
                    break;
            }
            if (ListParadigm.Count != 0)
                return ListParadigm;
            else
                switch (suffix1)
                {
                    case "й":
                        {
                            switch (case_w)
                            {
                                case 2: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "го"); }
                                    break;
                                case 7: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "му"); }
                                    break;
                                case 6: { ListParadigm.Add(word); }
                                    break;
                                case 5: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "м"); }
                                    break;
                                case 8: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "м"); }
                                    break;
                            }
                        }
                        break;
                    case "е":
                        {
                            if (Regex.IsMatch(word, "^[0-9]*3[^0-9]")) 
                                switch (case_w)
                                {
                                    case 2: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "его"); }
                                        break;
                                    case 7: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ему"); }
                                        break;
                                    case 6: { ListParadigm.Add(word); }
                                        break;
                                    case 5: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "им"); }
                                        break;
                                    case 8: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ем"); }
                                        break;
                                }
                            else
                                switch (case_w)
                                {
                                    case 2: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "го"); }
                                        break;
                                    case 7: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "му"); }
                                        break;
                                    case 6: { ListParadigm.Add(word); }
                                        break;
                                    case 5: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "м"); }
                                        break;
                                    case 8: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "м"); }
                                        break;
                                }
                        }
                        break;
                    case "я":
                        {
                            if (Regex.IsMatch(word, "^[0-9]*13[^0-9]")) 
                                switch (case_w)
                                {
                                    case 2: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ой"); }
                                        break;
                                    case 7: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ой"); }
                                        break;
                                    case 6: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ую"); }
                                        break;
                                    case 5: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ой"); }
                                        break;
                                    case 8: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ой"); }
                                        break;
                                }
                            else if (Regex.IsMatch(word, "^[0-9]*3[^0-9]")) 
                                switch (case_w)
                                {
                                    case 2: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ей"); }
                                        break;
                                    case 7: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ей"); }
                                        break;
                                    case 6: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ю"); }
                                        break;
                                    case 5: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ей"); }
                                        break;
                                    case 8: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ей"); }
                                        break;
                                }
                            else
                                switch (case_w)
                                {
                                    case 2: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "й"); }
                                        break;
                                    case 7: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "й"); }
                                        break;
                                    case 6: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "ю"); }
                                        break;
                                    case 5: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "м"); }
                                        break;
                                    case 8: { ListParadigm.Add(word.Substring(0, word.Length - 1) + "м"); }
                                        break;
                                }
                        }
                        break;
                }
            List<string> ListParadigmWithReg = new List<string>();
            foreach (string par in ListParadigm)
                ListParadigmWithReg.Add(GetInCorrectRegister(word, par));
            ListParadigm.Clear();
            return ListParadigmWithReg;
        }

        public static List<Paradigm> GetDeclinationAdjective_WithParam(string word)
        {
            // входные данные - прилаг в ед.ч. именительном падеже только из киррилических символов без пробелов
            // выходные - ед.ч. все падежи кроме Именительного
            List<Paradigm> ListParadigm = new List<Paradigm>();
            word = PrepareData(word);

            if ((word == string.Empty || Regex.IsMatch(word, "[^А-ЯЁа-яё—-]"))
                && false == Regex.IsMatch(word, "[0-9]-?[оыи]й") &&
                   false == Regex.IsMatch(word, "[0-9]-?[ая]я") &&
                   false == Regex.IsMatch(word, "[0-9]-?[ое]е") &&
                   false == Regex.IsMatch(word, "[0-9]-?[иы]е")
                )
            {
                return ListParadigm;
            }
            foreach (string par in GetDeclinationAdjective(word, 2))
                ListParadigm.Add(new Paradigm(par, 2, 0));
            foreach (string par in GetDeclinationAdjective(word, 7))
                ListParadigm.Add(new Paradigm(par, 7, 0));
            foreach (string par in GetDeclinationAdjective(word, 6))
                ListParadigm.Add(new Paradigm(par, 6, 0));
            foreach (string par in GetDeclinationAdjective(word, 5))
                ListParadigm.Add(new Paradigm(par, 5, 0));
            foreach (string par in GetDeclinationAdjective(word, 8))
                ListParadigm.Add(new Paradigm(par, 8, 0));
            return ListParadigm;
        }

        public static List<string> GetDeclinationAdjective(string word)
        {
            // входные данные - прилаг в ед.ч. именительном падеже только из киррилических символов без пробелов
            // выходные - ед.ч. все падежи кроме Именительного, без дублей
            List<string> ListParadigm = new List<string>();
            word = PrepareData(word);
            if (word == string.Empty || Regex.IsMatch(word, "[^А-ЯЁа-яё—-]"))
                return ListParadigm;
            ListParadigm.AddRange(GetDeclinationAdjective(word, 2));
            ListParadigm.AddRange(GetDeclinationAdjective(word, 7));
            ListParadigm.AddRange(GetDeclinationAdjective(word, 6));
            ListParadigm.AddRange(GetDeclinationAdjective(word, 5));
            ListParadigm.AddRange(GetDeclinationAdjective(word, 8));
            ListParadigm.Sort();
            List<string> ListParadigmWithoutDoubles = new List<string>();
            string lastParadigm = string.Empty;
            foreach (string par in ListParadigm)
            {
                if (par != lastParadigm)
                    ListParadigmWithoutDoubles.Add(par);
                lastParadigm = par;
            }
            ListParadigm.Clear();
            return ListParadigmWithoutDoubles;
        }

        public static List<string> GetDeclinationAdjective(string word, int case_w)
        {
            // входные данные - прилаг в ед.ч. именительном падеже только из киррилических символов без пробелов
            // выходные - ед.ч.
            List<string> ListParadigm = new List<string>();

            if (case_w != 2 && case_w != 7 && case_w != 6 && case_w != 5 && case_w != 8)
                return ListParadigm;

            word = PrepareData(word);

            if (word == string.Empty || Regex.IsMatch(word, "[^А-ЯЁа-яё—-]") || word.Length < 3)
            {
                if (false == Regex.IsMatch(word, "[0-9]-[оыи]й")
                    && false == Regex.IsMatch(word, "[0-9]-?[ая]я")
                    && false == Regex.IsMatch(word, "[0-9]-?[ое]е")
                    && false == Regex.IsMatch(word, "[0-9]-?[иы]е")
                    )
                {
                    return ListParadigm;
                }
            }
            //Собственные на -ово (-ёво), -ево, -аго (-яго), -ых (-их), -ко
            if (Regex.IsMatch(word, "([оёе]во|[ая]го|[иы]х)$"))
                return ListParadigm;
            string suffix2 = word.Substring(word.Length - 2, 2).ToLower();
            string suffix3 = word.Substring(word.Length - 3, 3).ToLower();

            switch (suffix2)
            {
                case "ая":
                    {
                        if (Regex.IsMatch(suffix3, "^[чцщ]", RegexOptions.IgnoreCase))
                            switch (case_w)
                            {
                                case 2: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ей"); }
                                    break;
                                case 7: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ей"); }
                                    break;
                                case 6: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ую"); }
                                    break;
                                case 5: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ей"); }
                                    break;
                                case 8: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ей"); }
                                    break;
                            }
                        else
                            switch (case_w)
                            {
                                case 2: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ой"); }
                                    break;
                                case 7: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ой"); }
                                    break;
                                case 6: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ую"); }
                                    break;
                                case 5: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ой"); }
                                    break;
                                case 8: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ой"); }
                                    break;
                            }
                    }
                    break;
                case "яя":
                    {
                        switch (case_w)
                        {
                            case 2: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ей"); }
                                break;
                            case 7: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ей"); }
                                break;
                            case 6: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "юю"); }
                                break;
                            case 5: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ей"); }
                                break;
                            case 8: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ей"); }
                                break;
                        }
                    }
                    break;
                case "ые":
                    {
                        switch (case_w)
                        {
                            case 2: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ых"); }
                                break;
                            case 7: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ым"); }
                                break;
                            case 6: { ListParadigm.Add(word); }
                                break;
                            case 5: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ыми"); }
                                break;
                            case 8: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ых"); }
                                break;
                        }
                    }
                    break;
                case "ие":
                    {
                        switch (case_w)
                        {
                            case 2: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "их"); }
                                break;
                            case 7: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "им"); }
                                break;
                            case 6: { ListParadigm.Add(word); }
                                break;
                            case 5: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ими"); }
                                break;
                            case 8: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "их"); }
                                break;
                        }
                    }
                    break;
                case "ой":
                case "ый":
                case "ое":
                    {
                        switch (case_w)
                        {
                            case 2: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ого"); }
                                break;
                            case 7: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ому"); }
                                break;
                            case 6: { ListParadigm.Add(word); }
                                break;
                            case 5:
                                {
                                    if (Regex.IsMatch(suffix3, "^[гкх]", RegexOptions.IgnoreCase))
                                        ListParadigm.Add(word.Substring(0, word.Length - 2) + "им");
                                    else
                                        ListParadigm.Add(word.Substring(0, word.Length - 2) + "ым");
                                }
                                break;
                            case 8: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ом"); }
                                break;
                        }
                    }
                    break;
                case "ий":
                    {
                        if (Regex.IsMatch(suffix3, "^[гкх]", RegexOptions.IgnoreCase))
                            switch (case_w)
                            {
                                case 2: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ого"); }
                                    break;
                                case 7: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ому"); }
                                    break;
                                case 6: { ListParadigm.Add(word); }
                                    break;
                                case 5: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "им"); }
                                    break;
                                case 8: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ом"); }
                                    break;
                            }
                        else
                            switch (case_w)
                            {
                                case 2: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "его"); }
                                    break;
                                case 7: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ему"); }
                                    break;
                                case 6: { ListParadigm.Add(word); }
                                    break;
                                case 5: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "им"); }
                                    break;
                                case 8: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ем"); }
                                    break;
                            }
                    }
                    break;
                case "ее":
                    {
                        switch (case_w)
                        {
                            case 2: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "его"); }
                                break;
                            case 7: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ему"); }
                                break;
                            case 6: { ListParadigm.Add(word); }
                                break;
                            case 5: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "им"); }
                                break;
                            case 8: { ListParadigm.Add(word.Substring(0, word.Length - 2) + "ем"); }
                                break;
                        }
                    }
                    break;
            }
            List<string> ListParadigmWithReg = new List<string>();
            foreach (string par in ListParadigm)
                ListParadigmWithReg.Add(GetInCorrectRegister(word, par));
            ListParadigm.Clear();
            return ListParadigmWithReg;
        }
        
        public List<Paradigm> GetDeclinationAdjectivesAndNoun_WithParam(string phrase)//use dictionary
        {
            // входные данные - набор прилаг и сущ в ед.ч. именительном падеже только из киррилических символов
            // выходные - ед.ч. все падежи кроме Именительного. Согласованно склоняются первое существительное и все прилагательные до него
            List<Paradigm> ListParadigm = new List<Paradigm>();
            phrase = PrepareData(phrase);
            if (phrase == string.Empty)
                return ListParadigm;
            //if (Regex.IsMatch(phrase, @"[^А-ЯЁа-яё\s-]", RegexOptions.IgnoreCase))
            //    return ListParadigm;
            foreach (string par in GetDeclinationAdjectivesAndNoun(phrase, 2))
                ListParadigm.Add(new Paradigm(par, 2, 0));
            foreach (string par in GetDeclinationAdjectivesAndNoun(phrase, 7))
                ListParadigm.Add(new Paradigm(par, 7, 0));
            foreach (string par in GetDeclinationAdjectivesAndNoun(phrase, 6))
                ListParadigm.Add(new Paradigm(par, 6, 0));
            foreach (string par in GetDeclinationAdjectivesAndNoun(phrase, 5))
                ListParadigm.Add(new Paradigm(par, 5, 0));
            foreach (string par in GetDeclinationAdjectivesAndNoun(phrase, 8))
                ListParadigm.Add(new Paradigm(par, 8, 0));
            return ListParadigm;
        }

        public List<string> GetDeclinationAdjectivesAndNoun(string phrase)
        {
            // входные данные - набор прилаг и сущ в ед.ч. именительном падеже только из киррилических символов
            // выходные - ед.ч. все падежи кроме Именительного. Согласованно склоняются первое существительное и все прилагательные до него
            List<string> ListParadigm = new List<string>();
            foreach (Paradigm par in GetDeclinationAdjectivesAndNoun_WithParam(phrase))
                ListParadigm.Add(par.word);
            ListParadigm.Sort();
            List<string> ListParadigmWithoutDoubles = new List<string>();
            string lastParadigm = string.Empty;
            foreach (string par in ListParadigm)
            {
                if (par != lastParadigm && par != phrase)
                    ListParadigmWithoutDoubles.Add(par);
                lastParadigm = par;
            }
            ListParadigm.Clear();
            return ListParadigmWithoutDoubles;
        }

        public List<Paradigm> GetDeclinationAdjectivesAndNounWithDash_WithParam(string phrase)
        {
            // входные данные - набор прилаг и сущ в ед.ч. им. п. только из кириллических символов.
            // выходные - ед.ч.. Согласованно склоняются первое существительное и все прилагательные до него.
            // генерируемые парадигмы фразы, содержащей тире и разомкнутые дефисы(значения вида "Усть - Янский район", "Усть -Янский район"), 
            //  совпадают по символьному составу с парадигмой значений, имеющих в своём составе дефис (значения вида "Усть-Янский район") 
            List<Dash> ListDash = new List<Dash>();
            int pos = 0;
            foreach (Match match in Regex.Matches(phrase, @"([ ]?)([–—-])([ ]?)", RegexOptions.IgnoreCase))
            {
                pos++;  
                ListDash.Add(new Dash(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value));
            } 
            List<Paradigm> ListParadigm = new List<Paradigm>();
            phrase = PrepareData(phrase);
            phrase = Regex.Replace(phrase, @"[ ]?[–—-][ ]?", "-", RegexOptions.IgnoreCase);
            foreach (Paradigm par in GetDeclinationAdjectivesAndNoun_WithParam(phrase))
            {
                Paradigm newPar = new Paradigm(par);
                // обработка дефисов
                pos = 0;
                int lastIndex = 0;
                string word = string.Empty;
                foreach (Match match in Regex.Matches(par.word, @"[-]", RegexOptions.IgnoreCase))
                {
                    if (pos == ListDash.Count)
                        break;
                    word += par.word.Substring(lastIndex, match.Index - lastIndex) + ListDash[pos].leftSpace + ListDash[pos].dash + ListDash[pos].rightSpace;
                    lastIndex = match.Index + 1;
                    pos++; 
                }
                newPar.word = word + par.word.Substring(lastIndex);
                ListParadigm.Add(newPar);
            }
            return ListParadigm;
        }

        public List<string> GetDeclinationAdjectivesAndNounWithDash(string phrase)
        {
            // входные данные - набор прилаг и сущ в ед.ч. им. п. только из кириллических символов.
            // выходные - ед.ч. Согласованно склоняются первое существительное и все прилагательные до него.
            // генерируемые парадигмы фразы, содержащей тире и разомкнутые дефисы(значения вида "Усть - Янский район", "Усть -Янский район"), 
            //  совпадают по символьному составу с парадигмой значений, имеющих в своём составе дефис (значения вида "Усть-Янский район") 
            List<string> ListParadigm = new List<string>();
            foreach (Paradigm par in GetDeclinationAdjectivesAndNounWithDash_WithParam(phrase))
                ListParadigm.Add(par.word);
            ListParadigm.Sort();
            List<string> ListParadigmWithoutDoubles = new List<string>();
            string lastParadigm = string.Empty;
            foreach (string par in ListParadigm)
            {
                if (par != lastParadigm && par != phrase)
                    ListParadigmWithoutDoubles.Add(par);
                lastParadigm = par;
            }
            ListParadigm.Clear();
            return ListParadigmWithoutDoubles;
        }

        public List<string> GetDeclinationAdjectivesAndNoun(string phrase, int case_w)//use dictionary
        {
            // входные данные - набор прилаг и сущ в ед.ч. именительном падеже только из киррилических символов
            // выходные - ед.ч. в заданном падеже. Согласованно склоняются первое существительное и все прилагательные до него
            List<string> phraseParadigm = new List<string>();
            phrase = PrepareData(phrase);
            if (phrase == string.Empty)
                return phraseParadigm;
            //if (Regex.IsMatch(phrase, @"[^А-ЯЁа-яё\s-]", RegexOptions.IgnoreCase))
            //    return phraseParadigm;
            List<string> ListWords = new List<string>();
            // разбиение на слова с сохранением тегов разметки
            foreach (Match match in Regex.Matches(phrase, @"<[A-Za-zА-ЯЁа-яё0-9—-]+>|[A-Za-zА-ЯЁа-яё0-9—-]+|[^A-Za-zА-ЯЁа-яё0-9<>—-]+|[<>]+", RegexOptions.IgnoreCase))
            {
                ListWords.Add(match.Value);
            }
            int lastCodeOfSpeech = -1;
            bool stopDeclination = false;
            int wordCount = 0;
            string[] exceptInstitutions = { "МИД", "ФНС", "ФМС", "ФАВТ", "МЭРТ", "РЭК", "УФНС", "УФМС" };
            string[] startExceptions = { "КОМИ", "САХА" };

            foreach (string word in ListWords)
            {
                wordCount++;

                if (wordCount == 1 && ListWords.Count > 1 && word != "" && word.ToLower() == "интернет")
                {
                    ConcatWordToList(ref phraseParadigm, word);                    
                    continue;
                }
                // ALIN-2421: Словоформы, состоящие только из заглавных букв, должны считаться неизменяемыми и не склоняться.
                // Исключение составляют аббревиатуры МИД, ФНС, ФМС, ФАВТ, МЭРТ, РЭК, УФНС, УФМС,
                // для которых парадигма должна генерироваться в соответствии со стандартными правилами
                if (genMode == EParadigmGeneratorMode.EModeStateInstitutions)
                {
                    if (Regex.IsMatch(word, "[А-ЯЁ]") && word == word.ToUpper() && exceptInstitutions.Contains(word) == false && word.Length < 5)
                    {
                        ConcatWordToList(ref phraseParadigm, word);
                        stopDeclination = true;
                        continue;
                    }
                }
                // ALIN-2421: Словоформы "Коми", "Саха", стоящие с начальной позиции, также должны считаться неизменяемыми
                if (wordCount == 1)
                {
                    if (startExceptions.Contains(word.ToUpper()))
                    {
                        ConcatWordToList(ref phraseParadigm, word);
                        continue;
                    }
                }
                if (Regex.IsMatch(word, @"[^A-Za-zА-ЯЁа-яё0-9-]", RegexOptions.IgnoreCase) || stopDeclination)
                {   // пропускаем разделители и теги
                    ConcatWordToList(ref phraseParadigm, word);
                    continue;
                }
                // определение части речи 
                //(select top 1 LA.CodeOfSpeech code from LemmasInArticles LA, HomonimyInWords H where LA.lemma_id = H.lemma_id and H.word = @word) // способ через обращение к базе
                int codeOfSpeech;  // способ с предзагрузкой словаря

                if (genMode == EParadigmGeneratorMode.EModeGeoRussia && word.ToLower() == "рабочий")
                {
                    codeOfSpeech = 10;
                }
                else if (HomonimyInWords_Case.ContainsKey(word.ToLower()))
                {
                    codeOfSpeech = HomonimyInWords_Case[word.ToLower()];
                }
                else
                {
                    if (Regex.IsMatch(word, "[0-9]", RegexOptions.IgnoreCase))
                        if (Regex.IsMatch(word, "^[0-9]{1,6}[-]([йея]|ый|ий|ой|ая|ое)$", RegexOptions.IgnoreCase))
                            codeOfSpeech = 11;  // числительное ALIN-1615, ALIN-1619
                        else if (Regex.IsMatch(word.Replace(" ", ""), "[а-яА-Яa-zA-Z][-][0-9]"))
                        {
                            codeOfSpeech = 7; // существительное
                        }
                        else
                        {
                            codeOfSpeech = 2; // не прилагательное и не существительное.
                        }
                    else if (Regex.IsMatch(word, "(рой)$") == false)
                    {
                        if (Regex.IsMatch(word, "(ому|ему|ими|ыми|ого|его|ая|яя|ые|ой|ый|ий|ое|ее|ой|ей|ых|их|ым|им|ую|юю)$", RegexOptions.IgnoreCase)) //,'ом','ем','ие'                        
                            codeOfSpeech = 10;      //	10	ПРИЛАГАТЕЛЬНОЕ
                        else
                            codeOfSpeech = 7; //	7	СУЩЕСТВИТЕЛЬНОЕ
                    }
                    else // все что не знаем считаем существительным
                        codeOfSpeech = 7;   //	7	СУЩЕСТВИТЕЛЬНОЕ
                }
                // если было не прилагательное и не "x-й" - оставшаяся часть фразы не склоняется
                if (lastCodeOfSpeech != 10 && lastCodeOfSpeech != 99999999 && lastCodeOfSpeech != 11 && lastCodeOfSpeech != -1)
                {
                    ConcatWordToList(ref phraseParadigm, word);
                    stopDeclination = true;
                    continue;
                }
                if (codeOfSpeech == 10 || codeOfSpeech == 99999999 || codeOfSpeech == 11 || codeOfSpeech == 7)	//если прилаг 
                {
                    List<string> newPhraseParadigm = new List<string>();
                    List<string> copyPhraseParadigm = new List<string>();
                    List<string> listDeclination = new List<string>();
                    switch (codeOfSpeech)
                    {
                        case 7: listDeclination = GetDeclinationNoun(word, case_w); break;
                        case 11: listDeclination = GetDeclinationNumeral(word, case_w); break;
                        case 10:
                        case 99999999: listDeclination = GetDeclinationAdjective(word, case_w); break;
                    } 
                    if (listDeclination.Count != 0)
                    {
                        foreach (string parAdj in listDeclination)
                        {
                            copyPhraseParadigm.AddRange(phraseParadigm);
                            ConcatWordToList(ref copyPhraseParadigm, parAdj);
                            newPhraseParadigm.AddRange(copyPhraseParadigm);
                            copyPhraseParadigm.Clear();
                        }
                        phraseParadigm.Clear();
                        phraseParadigm.AddRange(newPhraseParadigm);
                        newPhraseParadigm.Clear();
                    }
                    else
                        ConcatWordToList(ref phraseParadigm, word);
                }
                else    //слово не прилаг и не сущ
                    ConcatWordToList(ref phraseParadigm, word);
                lastCodeOfSpeech = codeOfSpeech;
            }
            ListWords.Clear();
            return phraseParadigm;
        }

        public List<Paradigm> GetAllDeclinationCompanyName_WithParam(string companyIn)
        {
            // получает на вход полное наименование компании в имен. пад. един. ч. и возвращает все парадигмы в единственном числе
            // парадигмы для опф/вэд берутся из вариантов названия соответствующих справочников
            // сначала вычленяется фраза в кавычках, затем опф и вэд. все 3 части и оставшийся фрагмент склоняются раздельно с последующим согласованием падежей.
            // фраза в кавычках склоняется только если нет других фрагментов
            List<Paradigm> companyParadigm = new List<Paradigm>();
            string company = PrepareData(companyIn);
            company = Regex.Replace(company, @"[\s]+", " ", RegexOptions.IgnoreCase);
            string quotes = Regex.Match(" " + company + " ", "(?<=[\"]).*(?=[\"])", RegexOptions.IgnoreCase).Value;
            if (quotes != string.Empty)
                company = company.Replace(quotes, "<quotesParadigm>");
            // работаем с ОПФ
            string opf = string.Empty;
            int opfID = -1;
            foreach (KeyValuePair<int, string> item in OPF)
            {
                if (company.ToLower().Contains(item.Value))
                {
                    opf = Regex.Match(" " + company + " ", "(?<=[^a-zа-яё])" + item.Value + "(?=[^a-zа-яё])", RegexOptions.IgnoreCase).Groups[0].Value.ToString(); // сохраняем исходный регистр
                    opfID = item.Key;
                    if (opf == string.Empty)
                        continue;
                    try
                    { company = Regex.Replace(company, opf.Replace("-", "[-]").Replace(".", "[.]"), "<opfParadigm>", RegexOptions.IgnoreCase); }
                    catch { }
                    break; 
                }
            }
            // работаем с ВЭД
            string ved = string.Empty;
            int vedID = -1;
            foreach (KeyValuePair<int, string> item in VED)
            {
                if (company.ToLower().Contains(item.Value))
                {
                    ved = Regex.Match(" " + company + " ", "(?<=[^a-zа-яё])" + item.Value + "(?=[^a-zа-яё])", RegexOptions.IgnoreCase).Groups[0].Value.ToString(); // сохраняем исходный регистр
                    vedID = item.Key;
                    if (ved == string.Empty)
                        continue;
                    try
                    { company = Regex.Replace(company, ved.Replace("-", "[-]").Replace(".", "[.]"), "<vedParadigm>", RegexOptions.IgnoreCase); }
                    catch { }
                    break; 
                }
            }
            List<Paradigm> ListCompanyWithTag = new List<Paradigm>();
            ListCompanyWithTag = GetDeclinationAdjectivesAndNoun_WithParam(company);
            List<Paradigm> ListQuotesParadigm = new List<Paradigm>();
            if (!Regex.IsMatch(company, "[а-яё]", RegexOptions.IgnoreCase)) // если существует фраза вне кавычек - название не склоняется
            //    ListQuotesParadigm = GetDeclinationAdjectivesAndNoun(companyIn);
            //else
                ListQuotesParadigm = GetDeclinationAdjectivesAndNoun_WithParam(quotes);
            // получение парадигмы для ОПФ и ВЭД
            List<Paradigm> ListOPFparadigm = new List<Paradigm>();
            List<Paradigm> ListVEDparadigm = new List<Paradigm>();
            bool case_2 = false;
            bool case_7 = false;
            bool case_6 = false;
            bool case_5 = false;
            bool case_8 = false;
            bool case_2v = false;
            bool case_7v = false;
            bool case_6v = false;
            bool case_5v = false;
            bool case_8v = false;
            foreach (IdParadigm item in OPFparadigm)
            {
                if (opfID == item.ID)
                {
                    ListOPFparadigm.Add(new Paradigm(GetInCorrectRegister(opf, item.word), item.case_w, item.number));
                    switch (item.case_w)
                    {
                        case 2: case_2 = true; break;
                        case 7: case_7 = true; break;
                        case 6: case_6 = true; break;
                        case 5: case_5 = true; break;
                        case 8: case_8 = true; break;
                    }
                }
                else if (vedID == item.ID)
                {
                    ListVEDparadigm.Add(new Paradigm(GetInCorrectRegister(ved, item.word), item.case_w, item.number));
                    switch (item.case_w)
                    {
                        case 2: case_2v = true; break;
                        case 7: case_7v = true; break;
                        case 6: case_6v = true; break;
                        case 5: case_5v = true; break;
                        case 8: case_8v = true; break;
                    }
                }
            }
            // добавляем пропущенные падежи для ОПФ
            if (!case_2)
                foreach (string opfPar in GetDeclinationAdjectivesAndNoun(opf, 2))
                    ListOPFparadigm.Add(new Paradigm(opfPar, 2, 0));
            if (!case_7)
                foreach (string opfPar in GetDeclinationAdjectivesAndNoun(opf, 7))
                    ListOPFparadigm.Add(new Paradigm(opfPar, 7, 0));
            if (!case_6)
                foreach (string opfPar in GetDeclinationAdjectivesAndNoun(opf, 6))
                    ListOPFparadigm.Add(new Paradigm(opfPar, 6, 0));
            if (!case_5)
                foreach (string opfPar in GetDeclinationAdjectivesAndNoun(opf, 5))
                    ListOPFparadigm.Add(new Paradigm(opfPar, 5, 0));
            if (!case_8)
                foreach (string opfPar in GetDeclinationAdjectivesAndNoun(opf, 8))
                    ListOPFparadigm.Add(new Paradigm(opfPar, 8, 0));
            // добавляем пропущенные падежи для ВЭД
            if (!case_2v)
                foreach (string vedPar in GetDeclinationAdjectivesAndNoun(ved, 2))
                    ListVEDparadigm.Add(new Paradigm(vedPar, 2, 0));
            if (!case_7v)
                foreach (string vedPar in GetDeclinationAdjectivesAndNoun(ved, 7))
                    ListVEDparadigm.Add(new Paradigm(vedPar, 7, 0));
            if (!case_6v)
                foreach (string vedPar in GetDeclinationAdjectivesAndNoun(ved, 6))
                    ListVEDparadigm.Add(new Paradigm(vedPar, 6, 0));
            if (!case_5v)
                foreach (string vedPar in GetDeclinationAdjectivesAndNoun(ved, 5))
                    ListVEDparadigm.Add(new Paradigm(vedPar, 5, 0));
            if (!case_8v)
                foreach (string vedPar in GetDeclinationAdjectivesAndNoun(ved, 8))
                    ListVEDparadigm.Add(new Paradigm(vedPar, 8, 0));
            // согласуем парадигмы
            List<Paradigm> ListBase = new List<Paradigm>();
            ListBase.Add(new Paradigm("", 0, 0));
            ListBase.Add(new Paradigm("", 2, 0));
            ListBase.Add(new Paradigm("", 7, 0));
            ListBase.Add(new Paradigm("", 6, 0));
            ListBase.Add(new Paradigm("", 5, 0));
            ListBase.Add(new Paradigm("", 8, 0));
            foreach (Paradigm baseCase in ListBase)
            {
                // парадигма фразы без опф и кавычек
                List<string> ListCompanyWithTag_oneCase = new List<string>();
                foreach (Paradigm parWithTag in ListCompanyWithTag)
                {
                    if (parWithTag.case_w == baseCase.case_w)
                        ListCompanyWithTag_oneCase.Add(parWithTag.word);
                }
                if (quotes == string.Empty && (opf != string.Empty || ved != string.Empty) && !ListCompanyWithTag_oneCase.Contains(company))
                {
                    ListCompanyWithTag_oneCase.Add(company);
                }
                if (ListCompanyWithTag_oneCase.Count == 0)
                    ListCompanyWithTag_oneCase.Add(company);
                foreach (string parWithTag in ListCompanyWithTag_oneCase)
                {
                    if (ListOPFparadigm.Count != 0)
                    {
                        if (ListVEDparadigm.Count != 0)
                        {   // ОПФ и ВЭД
                            bool parOPFExists = false;
                            foreach (Paradigm parOPF in ListOPFparadigm)
                            {
                                if (parOPF.case_w == baseCase.case_w && (parOPF.number == baseCase.number || parOPF.number == -1))
                                {
                                    parOPFExists = true;
                                    bool parVEDExists = false;
                                    foreach (Paradigm parVED in ListVEDparadigm)
                                    {
                                        if (parVED.case_w == baseCase.case_w && (parVED.number == baseCase.number || parVED.number == -1))
                                        {
                                            parVEDExists = true;
                                            companyParadigm.Add(new Paradigm(parWithTag.Replace("<opfParadigm>", parOPF.word).Replace("<vedParadigm>", parVED.word).Replace("<quotesParadigm>", quotes), baseCase.case_w, baseCase.number));
                                        }
                                    }
                                    if (!parVEDExists)
                                        companyParadigm.Add(new Paradigm(parWithTag.Replace("<opfParadigm>", parOPF.word).Replace("<vedParadigm>", ved).Replace("<quotesParadigm>", quotes), baseCase.case_w, baseCase.number));
                                }
                            }
                            if (!parOPFExists)
                            {
                                bool parVEDExists = false;
                                foreach (Paradigm parVED in ListVEDparadigm)
                                {
                                    if (parVED.case_w == baseCase.case_w && (parVED.number == baseCase.number || parVED.number == -1))
                                    {
                                        parVEDExists = true;
                                        companyParadigm.Add(new Paradigm(parWithTag.Replace("<opfParadigm>", opf).Replace("<vedParadigm>", parVED.word).Replace("<quotesParadigm>", quotes), baseCase.case_w, baseCase.number));
                                    }
                                }
                                if (!parVEDExists)
                                    companyParadigm.Add(new Paradigm(parWithTag.Replace("<opfParadigm>", opf).Replace("<vedParadigm>", ved).Replace("<quotesParadigm>", quotes), baseCase.case_w, baseCase.number));
                            }
                        }
                        else
                        {   // только ОПФ
                            bool parOPFExists = false;
                            foreach (Paradigm parOPF in ListOPFparadigm)
                            {
                                if (parOPF.case_w == baseCase.case_w && (parOPF.number == baseCase.number || parOPF.number == -1))
                                {
                                    parOPFExists = true;
                                    companyParadigm.Add(new Paradigm(parWithTag.Replace("<opfParadigm>", parOPF.word).Replace("<quotesParadigm>", quotes), baseCase.case_w, baseCase.number));
                                }
                            }
                            if (!parOPFExists)
                                companyParadigm.Add(new Paradigm(parWithTag.Replace("<opfParadigm>", opf).Replace("<quotesParadigm>", quotes), baseCase.case_w, baseCase.number));
                        }
                    }
                    else
                    {
                        // парадигма ВЭД
                        if (ListVEDparadigm.Count != 0)
                        {   // только ВЭД
                            bool parVEDExists = false;
                            foreach (Paradigm parVED in ListVEDparadigm)
                            {
                                if (parVED.case_w == baseCase.case_w && (parVED.number == baseCase.number || parVED.number == -1))
                                {
                                    parVEDExists = true;
                                    companyParadigm.Add(new Paradigm(parWithTag.Replace("<vedParadigm>", parVED.word).Replace("<quotesParadigm>", quotes), baseCase.case_w, baseCase.number));
                                }
                            }
                            if (!parVEDExists)
                                companyParadigm.Add(new Paradigm(parWithTag.Replace("<vedParadigm>", ved).Replace("<quotesParadigm>", quotes), baseCase.case_w, baseCase.number));
                        }
                        else
                        {
                            // парадигма фразы в кавычках
                            bool parQuotesExists = false;
                            foreach (Paradigm parQuotes in ListQuotesParadigm)
                            {
                                if (parQuotes.case_w == baseCase.case_w)
                                {
                                    parQuotesExists = true;
                                    companyParadigm.Add(new Paradigm(parWithTag.Replace("<quotesParadigm>", parQuotes.word), baseCase.case_w, baseCase.number));
                                }
                            }
                            if (!parQuotesExists)
                                companyParadigm.Add(new Paradigm(parWithTag.Replace("<quotesParadigm>", quotes).Replace("<opfParadigm>", opf), baseCase.case_w, baseCase.number));
                        }
                    }
                }
                ListCompanyWithTag_oneCase.Clear();
            }
            return companyParadigm;
        }

        public List<string> GetAllDeclinationCompanyName(string company)
        {
            // получает на вход наименование компании в имен. пад. един. ч. и возвращает все парадигмы в единственном числе
            // парадигмы для опф/вэд берутся из вариантов названия соответствующих справочников
            // сначала вычленяется фраза в кавычках, затем опф и вэд. все 3 части и оставшийся фрагмент склоняются раздельно с последующим согласованием падежей
            List<string> companyParadigm = new List<string>();
            foreach (Paradigm item in GetAllDeclinationCompanyName_WithParam(company))
                companyParadigm.Add(item.word);
            companyParadigm.Sort();
            List<string> ListParadigmWithoutDoubles = new List<string>();
            string lastParadigm = string.Empty;
            foreach (string par in companyParadigm)
            {
                if (par != lastParadigm && par != company)
                    ListParadigmWithoutDoubles.Add(par);
                lastParadigm = par;
            }
            companyParadigm.Clear();
            return ListParadigmWithoutDoubles;
        }

        public List<string> GetGeoRuLinguisticRepresentations(string name, string type)
        {
            List<string> ListVariants = new List<string>();
            type = Regex.Replace(type, "\\s+", " ").Trim();
            //1. выделение эталонного названия, далее Название, по модели «содержание строки словарной карточки «Эталонное название» минус содержание строки «тип объекта»»: 
            string ideal = Regex.Replace(Regex.Replace(name, "^\\s*" + type, "", RegexOptions.IgnoreCase), "\\s+", " ").Trim();
            ListVariants.Add(ideal);
            // 2. автоматическое внесение в столбец карточки «лингвистические представления» вариантов названия по модели «сокращение + Название». 
            // Сокращения определяются по типу объекта, список сокращений (с чувствительностью к пробелам и знакам препинания), соответствующих типам объекта, можно найти в прикрепленном файле (\\server01\Документы\Отдел лингвистики\ сокращения в зависимости от типа объекта для справочника Гео России.docx)
            if (YugoReport_srg_ALIN_1962.ContainsKey(type))
                foreach (string socr in YugoReport_srg_ALIN_1962[type])
                {
                    ListVariants.Add(socr + ideal); // пробел добавлен в самих сокращениях
                }
            // 3. автоматическое внесение в столбец карточки «лингвистические представления» вариантов названия по модели «Название (тип объекта)»
            ListVariants.Add(ideal + "(" + type + ")");
            ListVariants.Add(ideal + " (" + type + ")");
            // 4. автоматическое внесение в столбец карточки «лингвистические представления» вариантов названия по модели «тип объекта + Название» 
            // для объектов типа «сельское поселение», «слобода», «промышленная зона», «сельсовет», «сельский округ», «квартал» и «улус». 
            if (Regex.IsMatch(type, "^(сельское поселение|слобода|промышленная зона|сельсовет|сельский округ|квартал|улус)$", RegexOptions.IgnoreCase))
                ListVariants.Add(type + " " + ideal);
            //5. автоматическое внесение в столбец карточки «лингвистические представления» вариантов названия по модели «Название + тип объекта» 
            // для объектов типа «аул», «хутор», «арбан», «починок», «волость», «станица» и «заимка»
            if (Regex.IsMatch(type, "^(аул|хутор|арбан|починок|волость|станица|заимка)$", RegexOptions.IgnoreCase))
                ListVariants.Add(ideal + " " + type);

            List<string> ListVariantWithoutDoubles = new List<string>();
            string lastVariant = string.Empty;
            foreach (string var in ListVariants)
            {
                if (var != lastVariant && var != name)
                    ListVariantWithoutDoubles.Add(var);
                lastVariant = var;
            }
            ListVariants.Clear();
            return ListVariantWithoutDoubles; 
        }

        private static void ConcatWordToList(ref List<string> list, string right)
        {
            if (list.Count == 0)
            {
                list.Add(right);
                return;
            }
            List<string> newlist = new List<string>();
            foreach (string item in list)
                newlist.Add(item + right);
            list.Clear();
            list.AddRange(newlist);
            newlist.Clear();
        }

        private static string PrepareData(string data)
        {
            if (string.IsNullOrEmpty(data)) return data;
            //Замена букв Ё,ё на Е, е, соответственно
            data = data.Replace('ё', 'е');
            data = data.Replace('Ё', 'Е');
            //тире и дефисы, разные виды кавычек
            data = data.Replace('«', '"');
            data = data.Replace('‹', '"');
            data = data.Replace('»', '"');
            data = data.Replace('›', '"');
            data = data.Replace('„', '"');
            data = data.Replace('‚', '"');
            data = data.Replace('“', '"');
            data = data.Replace('‟', '"');
            data = data.Replace('‘', '"');
            data = data.Replace('”', '"');
            data = data.Replace('″', '"');
            data = data.Replace('\u2013', '-');
            data = data.Replace('\u2014', '-');
            data = data.Replace('\u2015', '-');
            //data = data.Replace('—', '-');
            data = Regex.Replace(data, @"\s{1,}", " ");
            data = data.Trim();
            return data;
        }

        public static string GetInCorrectRegister(string text, string paradigm)
        {
            // получает на вход исходные текст для которого генерируется парадигма и результат генерации.
            // сгенерированный вариант приводится к регистру исходно текста
            string result = string.Empty;
            List<string> ListWordText = new List<string>();
            List<string> ListWordParadigm = new List<string>();
            text = Regex.Replace(text, "\\s+", " ").Trim();
            paradigm = Regex.Replace(paradigm, "\\s+", " ").Trim();
            Match m = Regex.Match(text, "[\\s]|[^\\s]+");
            while(m.Success)
            {
                ListWordText.Add(m.Groups[0].Value.ToString());
                m = m.NextMatch();                
            }
            m = Regex.Match(paradigm, "[\\s]|[^\\s]+");
            while (m.Success)
            {
                ListWordParadigm.Add(m.Groups[0].Value.ToString());
                m = m.NextMatch();
            }
            // если число элементов различно - оставляем парадигму как есть
            if (ListWordText.Count != ListWordParadigm.Count)
                return paradigm;
            for (int i = 0; i < ListWordText.Count; i++)
            {
                //если парадигма короче, проверяем первую букву на заглавную и все слово целиком
                if (ListWordText[i].Length > ListWordParadigm[i].Length)
                {
                    if (ListWordText[i] == ListWordText[i].ToUpper())
                        result += ListWordParadigm[i].ToUpper();
                    else if (ListWordText[i].Substring(0, 1) == ListWordText[i].Substring(0, 1).ToUpper())
                        result += ListWordParadigm[i].Substring(0, 1).ToUpper() + ListWordParadigm[i].Substring(1);
                    else
                        result += ListWordParadigm[i];
                }
                else
                {
                    for (int j = 0; j < ListWordText[i].Length; j++)
                    {
                        if (ListWordText[i].Substring(j, 1) == ListWordText[i].Substring(j, 1).ToUpper())
                            result += ListWordParadigm[i].Substring(j, 1).ToUpper();
                        else
                            result += ListWordParadigm[i].Substring(j, 1);
                    }
                    if (ListWordText[i].Length < ListWordParadigm[i].Length)   
                    {   // если парадигма длиннее переводим ее в верхний регистр, только если все слово в верхнем
                        if (ListWordText[i] == ListWordText[i].ToUpper())
                            result += ListWordParadigm[i].Substring(ListWordText[i].Length).ToUpper();
                        else
                            result += ListWordParadigm[i].Substring(ListWordText[i].Length);
                    }
                }
            }
            return result;
        }
    }
}
