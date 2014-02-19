using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;


namespace GenParadigmCollection
{
    public partial class ParadigmGenerator
    {
        public static int? GetIntFromDB(string sql)
        {
            SqlConnection conn = new SqlConnection(GetStringConnection());
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 120;
            try
            {
                conn.Open();
                object val = cmd.ExecuteScalar();
                if (val != DBNull.Value && val != null)
                {
                    return (int?)val;
                }
                return null;
            }
            catch (Exception ex)
            {
            }
            finally
            {
                conn.Close();
            }
            return null;
        }

        public static string GetStringFromDB(string sql)
        {
            SqlConnection conn = new SqlConnection(GetStringConnection());
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 120;
            try
            {
                conn.Open();
                object val = cmd.ExecuteScalar();
                if (val != DBNull.Value)
                {
                    return (string)val;
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debugger.Break();
            }
            finally
            {
                conn.Close();
            }
            return null;
        }

        public static List<string> GetDeclinationNoun_Plural(string wordIn, int caseIn)
        {
            List<string> ListParadigm = new List<string>();
            List<string> tbl = new List<string>();
            string word, suffix1, @suffix2;
            int caseIterator, len;
            int? gender;
            word = wordIn;
            len = word.Length;
            if (new Regex("[оёе]во").IsMatch(word.Substring(word.Length - 3, 3)) | new Regex("[ая]го").IsMatch(word.Substring(word.Length - 3, 3)) | new Regex("[иы]х").IsMatch(word.Substring(word.Length - 3, 3)) | (word.Substring(word.Length - 2, 2) == "ко"))
                return ListParadigm;
            if (caseIn == null)
                caseIterator = 2;
            else
                caseIterator = caseIn;
            while (caseIterator != 0)
            {
                if (wordIn != "" && !wordIn.Contains(" ") && !(new Regex("[a-z]").IsMatch(wordIn)))
                {
                    suffix1 = word.Substring(word.Length - 1, 1);
                    suffix2 = word.Substring(word.Length - 2, 2);

                    if (suffix2 == "ои")
                    {
                        if (caseIterator == 2) tbl.Add(word.Substring(0, word.Length - 2) + "оев");
                        if (caseIterator == 7) tbl.Add(word.Substring(0, word.Length - 2) + "оям");
                        if (caseIterator == 6) tbl.Add(word.Substring(0, word.Length - 2) + "ои");
                        if (caseIterator == 5) tbl.Add(word.Substring(0, word.Length - 2) + "оями");
                        if (caseIterator == 8) tbl.Add(word.Substring(0, word.Length - 2) + "ях");
                    }
                    if (suffix2 == "ги" || suffix2 == "ки" || suffix2 == "хи")
                    {
                        if (caseIterator == 2) tbl.Add(word.Substring(0, word.Length - 1) + "ов");  // CaseR
                        if (caseIterator == 7) tbl.Add(word.Substring(0, word.Length - 1) + "ам");  // CaseD
                        if (caseIterator == 6) tbl.Add(word.Substring(0, word.Length));             // CaseV
                        if (caseIterator == 5) tbl.Add(word.Substring(0, word.Length - 1) + "ами"); // CaseT
                        if (caseIterator == 8) tbl.Add(word.Substring(0, word.Length - 1) + "ах");  // CaseP
                    }
                    else if (suffix1 == "а")
                    {
                        if (caseIterator == 2) tbl.Add(word.Substring(0, word.Length - 1) + "");
                        if (caseIterator == 7) tbl.Add(word.Substring(0, word.Length - 1) + "ам");
                        if (caseIterator == 6) tbl.Add(word.Substring(0, word.Length - 1) + "а");
                        if (caseIterator == 5) tbl.Add(word.Substring(0, word.Length - 1) + "ами");
                        if (caseIterator == 8) tbl.Add(word.Substring(0, word.Length - 1) + "ах");
                    }
                    else if (suffix1 == "о")
                    {
                        if (caseIterator == 2) tbl.Add(word.Substring(0, word.Length - 1) + "а");
                        if (caseIterator == 7) tbl.Add(word.Substring(0, word.Length - 1) + "у");
                        if (caseIterator == 6) tbl.Add(word);
                        if (caseIterator == 5) tbl.Add(word.Substring(0, word.Length - 1) + "ом");
                        if (caseIterator == 8) tbl.Add(word.Substring(0, word.Length - 1) + "е");
                    }
                    else if (suffix2 == "ай")
                    {
                        if (caseIterator == 2) tbl.Add(word.Substring(0, word.Length - 1) + "я");
                        if (caseIterator == 7) tbl.Add(word.Substring(0, word.Length - 1) + "ю");
                        if (caseIterator == 6) tbl.Add(word);
                        if (caseIterator == 5) tbl.Add(word.Substring(0, word.Length - 1) + "ем");
                        if (caseIterator == 8) tbl.Add(word.Substring(0, word.Length - 1) + "е");
                    }
                    else if (suffix2 == "ия")
                    {
                        if (caseIterator == 2) tbl.Add(word.Substring(0, word.Length - 1) + "и");
                        if (caseIterator == 7) tbl.Add(word.Substring(0, word.Length - 1) + "и");
                        if (caseIterator == 6) tbl.Add(word.Substring(0, word.Length - 1) + "ю");
                        if (caseIterator == 5) tbl.Add(word.Substring(0, word.Length - 1) + "ей");
                        if (caseIterator == 8) tbl.Add(word.Substring(0, word.Length - 1) + "и");
                    }
                    else if (suffix1 == "я")
                    {
                        if (caseIterator == 2) tbl.Add(word.Substring(0, word.Length - 1) + "и");
                        if (caseIterator == 7) tbl.Add(word.Substring(0, word.Length - 1) + "е");
                        if (caseIterator == 6) tbl.Add(word.Substring(0, word.Length - 1) + "ю");
                        if (caseIterator == 5) tbl.Add(word.Substring(0, word.Length - 1) + "ей");
                        if (caseIterator == 8) tbl.Add(word.Substring(0, word.Length - 1) + "е");
                    }
                    else if (new Regex("[цкнгшщзхждлрпвфчсмтб]").IsMatch(suffix1))
                    {
                        if (caseIterator == 2) tbl.Add(word + "а");
                        if (caseIterator == 7) tbl.Add(word + "у");
                        if (caseIterator == 6) tbl.Add(word);
                        if (caseIterator == 5) tbl.Add(word + "ом");
                        if (caseIterator == 8) tbl.Add(word + "е");
                    }
                    else if (suffix2 == "ий")
                    {
                        if (caseIterator == 2) tbl.Add(word.Substring(0, word.Length - 1) + "я");
                        if (caseIterator == 7) tbl.Add(word.Substring(0, word.Length - 1) + "ю");
                        if (caseIterator == 6) tbl.Add(word);
                        if (caseIterator == 5) tbl.Add(word.Substring(0, word.Length - 1) + "ем");
                        if (caseIterator == 8) tbl.Add(word.Substring(0, word.Length - 1) + "и");

                    }
                    else if (suffix2 == "ие")
                    {
                        if (caseIterator == 2) tbl.Add(word.Substring(0, word.Length - 1) + "я");
                        if (caseIterator == 7) tbl.Add(word.Substring(0, word.Length - 1) + "ю");
                        if (caseIterator == 6) tbl.Add(word);
                        if (caseIterator == 5) tbl.Add(word.Substring(0, word.Length - 1) + "ем");
                        if (caseIterator == 8) tbl.Add(word.Substring(0, word.Length - 1) + "и");
                    }
                    else if (suffix2 == "ии")
                    {
                        if (caseIterator == 2) tbl.Add(word.Substring(0, word.Length - 1) + "й");
                        if (caseIterator == 2) tbl.Add(word.Substring(0, word.Length - 1) + "ев");
                        if (caseIterator == 7) tbl.Add(word.Substring(0, word.Length - 1) + "ям");
                        if (caseIterator == 6) tbl.Add(word);
                        if (caseIterator == 5) tbl.Add(word.Substring(0, word.Length - 1) + "ями");
                        if (caseIterator == 8) tbl.Add(word.Substring(0, word.Length - 1) + "ях");
                    }
                    else if (suffix1 == "ы")
                    {
                        if (caseIterator == 2) tbl.Add(word.Substring(0, word.Length - 1) + "ов");
                        if (caseIterator == 2) tbl.Add(word.Substring(0, word.Length - 1) + "");
                        if (caseIterator == 7) tbl.Add(word.Substring(0, word.Length - 1) + "ам");
                        if (caseIterator == 6) tbl.Add(word);
                        if (caseIterator == 5) tbl.Add(word.Substring(0, word.Length - 1) + "ями");
                        if (caseIterator == 8) tbl.Add(word.Substring(0, word.Length - 1) + "ах");
                    }
                    else if (suffix1 == "и")
                    {
                        if (caseIterator == 2) tbl.Add(word.Substring(0, word.Length - 1) + "");
                        if (caseIterator == 7) tbl.Add(word.Substring(0, word.Length - 1) + "ам");
                        if (caseIterator == 6) tbl.Add(word);
                        if (caseIterator == 5) tbl.Add(word.Substring(0, word.Length - 1) + "ами");
                        if (caseIterator == 8) tbl.Add(word.Substring(0, word.Length - 1) + "ах");
                    }
                    else if (suffix1 == "ь")
                    {
                        gender = (int)GetIntFromDB(string.Format("select top 1 h.gender from HomonimyInWords h where h.word = '{0}'", word));
                        if (gender == 1 | word.Substring(word.Length - 4, 4) == "ость" | suffix2 == "жь" | suffix2 == "шь" | suffix2 == "щь" | suffix2 == "чь")
                        {
                            if (caseIterator == 2) tbl.Add(word.Substring(0, word.Length - 1) + "и");
                            if (caseIterator == 7) tbl.Add(word.Substring(0, word.Length - 1) + "и");
                            if (caseIterator == 6) tbl.Add(word);
                            if (caseIterator == 5) tbl.Add(word.Substring(0, word.Length - 1) + "ью");
                            if (caseIterator == 8) tbl.Add(word.Substring(0, word.Length - 1) + "и");
                        }
                        else
                        {
                            if (caseIterator == 2) tbl.Add(word.Substring(0, word.Length - 1) + "я");
                            if (caseIterator == 7) tbl.Add(word.Substring(0, word.Length - 1) + "ю");
                            if (caseIterator == 6) tbl.Add(word);
                            if (caseIterator == 5) tbl.Add(word.Substring(0, word.Length - 1) + "ем");
                            if (caseIterator == 8) tbl.Add(word.Substring(0, word.Length - 1) + "е");

                            if (caseIterator == 2) tbl.Add(word.Substring(0, word.Length - 1) + "и");
                            if (caseIterator == 7) tbl.Add(word.Substring(0, word.Length - 1) + "и");
                            if (caseIterator == 6) tbl.Add(word);
                            if (caseIterator == 5) tbl.Add(word.Substring(0, word.Length - 1) + "ью");
                            if (caseIterator == 8) tbl.Add(word.Substring(0, word.Length - 1) + "и");
                        }
                    }
                }
                if (caseIn != null)
                    caseIterator = 0;
                else if (caseIterator == 8)
                    caseIterator = 0;
                else if (caseIterator == 5)
                    caseIterator = 8;
                else if (caseIterator == 6)
                    caseIterator = 5;
                else if (caseIterator == 7)
                    caseIterator = 6;
                else if (caseIterator == 2)
                    caseIterator = 7;
            }
            List<string> ListParadigmWithReg = new List<string>();
            foreach (string par in tbl.Distinct())
                ListParadigmWithReg.Add(GetInCorrectRegister(word, par)); 
            return ListParadigmWithReg;
        }

        public List<Paradigm> GetDeclinationAdjectivesAndNoun_Plural(string strIn, int? str_case)
        {
            List<Paradigm> tableOut = new List<Paradigm>();
            string strOut = "", strOut2 = "", word, wordDict = null, lastWord, wordForVarDecl;
            List<string> tblWordIn = new List<string>();
            List<Paradigm> tblWordsWithParam = new List<Paradigm>();
            int? lemma_id, CodeOfSpeech, gender = 0;
            int? lastCodeOfSpeech;
            tblWordsWithParam.AddRange(strIn.Split(' ').Select(b => new Paradigm(b, 0, 0)));
            lastCodeOfSpeech = null;
            int? caseIterator;
            if (str_case == null)
                caseIterator = 2;
            else
                caseIterator = str_case;
            while (caseIterator != 0 && caseIterator != null)
            {
                for (int i = 0; i < tblWordsWithParam.Count; i += 1)
                {
                    word = tblWordsWithParam[i].word;
                    CodeOfSpeech = tblWordsWithParam[i].CodeOfSpeech;
                    lemma_id = GetIntFromDB(string.Format("select top 1 lemma_id from HomonimyInWords where word = '{0}' COLLATE Cyrillic_General_CS_AS", word));
                    if ((caseIterator == 2) | str_case != null)
                    {

                        if (lemma_id != null && GetIntFromDB(string.Format("select top 1 LA.CodeOfSpeech from LemmasInArticles LA where LA.lemma_id = {0}", lemma_id)) != 0)
                        {
                            CodeOfSpeech = GetIntFromDB(string.Format("select top 1 LA.CodeOfSpeech from LemmasInArticles LA where LA.lemma_id = {0}", lemma_id));
                            gender = GetIntFromDB(string.Format("select top 1 H.gender code from HomonimyInWords H where H.lemma_id = {0}", lemma_id));
                            if (CodeOfSpeech != null)
                                tblWordsWithParam[i].CodeOfSpeech = (int)CodeOfSpeech;
                            if (gender != null)
                                tblWordsWithParam[i].gender = (int)gender;
                        }
                        else if (Regex.IsMatch(word.Substring(word.Length - 3, 3), "(ому|ему|ими|ыми|ого|его)$", RegexOptions.IgnoreCase) | (Regex.IsMatch(word.Substring(word.Length - 2, 2), "(ая|яя|ые|ие|ой|ый|ий|ое|ее|ой|ей|ых|их|ым|им|ую|юю|ом|ем)$", RegexOptions.IgnoreCase)))
                        {
                            CodeOfSpeech = 10;
                            tblWordsWithParam[i].CodeOfSpeech = 10;
                        }
                        else
                        {
                            CodeOfSpeech = 7;
                            tblWordsWithParam[i].CodeOfSpeech = 7;
                        }
                    }
                    if (lastCodeOfSpeech != 10 && lastCodeOfSpeech != null)
                    {
                        strOut += " " + word;
                        if (strOut2 != "")
                            strOut2 += " " + word;
                    }
                    else
                    {
                        if (CodeOfSpeech == 10)
                            if (lastCodeOfSpeech == null)
                            {
                                if (lemma_id != 0 && lemma_id != null)
                                {
                                    wordDict = GetStringFromDB(string.Format("(select top 1 LOWER(word) from HomonimyInWords where lemma_id = {0} and [case] = {1} and (gender = {2} or gender = -1) and number = 1) ", lemma_id, caseIterator, gender));
                                    if (wordDict != null | wordDict != "") strOut = GetInCorrectRegister(word, wordDict.Trim());
                                }
                                if (lemma_id == 0 | lemma_id == null | wordDict == null | wordDict == "")
                                    strOut = GetDeclinationAdjective(word, (int)caseIterator).First();
                            }
                            else
                            {
                                if (lemma_id == 0 && lemma_id != null)
                                {
                                    wordDict = GetStringFromDB(string.Format("(select top 1 LOWER(word) from HomonimyInWords where lemma_id = {0} and [case] = {1} and (gender = {2} or gender = -1) and number = 1) ", lemma_id, caseIterator, gender));
                                    if (wordDict != null | wordDict != "") strOut += " " + GetInCorrectRegister(word, wordDict.Trim());
                                }
                                if (lemma_id == 0 | lemma_id == null | wordDict == null | wordDict == "")
                                    strOut += " " + GetDeclinationAdjective(word, (int)caseIterator).First();
                            }
                        else
                            if (CodeOfSpeech == 7)
                            {
                                if (lastCodeOfSpeech == null)
                                {
                                    if (lemma_id != 0 && lemma_id != null)
                                    {
                                        wordDict = GetStringFromDB(string.Format("(select top 1 LOWER(word) from HomonimyInWords where lemma_id = {0} and [case] = {1} and gender = {2} and number = 1) ", lemma_id, caseIterator, gender));
                                        if (wordDict != null | wordDict != "") strOut = GetInCorrectRegister(word, wordDict.Trim());
                                    }
                                    if (lemma_id == 0 | lemma_id == null | wordDict == null | wordDict == "")
                                    {
                                        if (GetDeclinationNoun_Plural(word, (int)caseIterator).Count > 0)
                                            wordForVarDecl = GetDeclinationNoun_Plural(word, (int)caseIterator).First();
                                        else
                                            wordForVarDecl = null;
                                        if (GetDeclinationNoun_Plural(word, (int)caseIterator).Where(b => (b != wordForVarDecl)).Count() > 0)
                                            strOut2 = GetDeclinationNoun_Plural(word, (int)caseIterator).Where(b => (b != wordForVarDecl)).First();
                                        else
                                            strOut2 = null;
                                        strOut = wordForVarDecl;
                                    }
                                }
                                else
                                    if (lastCodeOfSpeech == 10)
                                    {
                                        if (lemma_id != 0 && lemma_id != null)
                                        {
                                            wordDict = GetStringFromDB(string.Format("(select top 1 LOWER(word) from HomonimyInWords where lemma_id = {0} and [case] = {1} and gender = {2} and number = 1) ", lemma_id, caseIterator, gender));
                                            if (wordDict != null | wordDict != "") strOut += " " + GetInCorrectRegister(word, wordDict.Trim());

                                        }
                                        if (lemma_id == 0 | lemma_id == null | wordDict == null | wordDict == "")
                                        {
                                            if (GetDeclinationNoun_Plural(word, (int)caseIterator).Count > 0)
                                                wordForVarDecl = GetDeclinationNoun_Plural(word, (int)caseIterator).First();
                                            else
                                                wordForVarDecl = null;
                                            if (GetDeclinationNoun_Plural(word, (int)caseIterator).Where(b => (b != wordForVarDecl)).Count() > 0)
                                                strOut2 = strOut + " " + GetDeclinationNoun_Plural(word, (int)caseIterator).Where(b => (b != wordForVarDecl)).First();
                                            else
                                                strOut2 = null;
                                            strOut += " " + wordForVarDecl;
                                        }
                                    }
                                    else if (lastCodeOfSpeech == 7)
                                    {
                                        strOut += " " + word;
                                    }
                                    else
                                    {
                                        strOut += " " + word;
                                    }
                            }
                            else
                                if (lastCodeOfSpeech == null)
                                    strOut += word;
                                else
                                    strOut += " " + word;
                        lastWord = word;
                        lastCodeOfSpeech = CodeOfSpeech;
                        wordDict = null;
                    }
                }
                tableOut.Add(new Paradigm(strOut, (int)caseIterator, 0));
                if (strOut2 != "")
                    tableOut.Add(new Paradigm(strOut2, (int)caseIterator, 0));
                strOut = "";
                strOut2 = "";
                lastCodeOfSpeech = null;
                lastWord = null;
                if (str_case != null)
                    caseIterator = 0;
                else if (caseIterator == 8)
                    caseIterator = 0;
                else if (caseIterator == 5)
                    caseIterator = 8;
                else if (caseIterator == 6)
                    caseIterator = 5;
                else if (caseIterator == 7)
                    caseIterator = 6;
                else if (caseIterator == 2)
                    caseIterator = 7;
            }
            return tableOut.Where(b => b.word != null).ToList();
        }

        public List<Paradigm> GetAllDeclinationWithNumber_WithParam(string strIn, int? flag_num)
        {
            List<Paradigm> ListParadigm = new List<Paradigm>();
            string afterComma = "";
            strIn = strIn.Trim();
            if (strIn.Contains(","))
            {
                afterComma = strIn.Substring(strIn.IndexOf(","), strIn.Length - strIn.IndexOf(","));
                strIn = strIn.Substring(0, strIn.IndexOf(","));
            }
            string quotes = "";
            if (strIn.Length > 2 && strIn.Substring(0, 1) == "\"" && strIn.Substring(strIn.Length - 1, 1) == "\"")
            {
                strIn = strIn.Substring(1, strIn.Length - 2);
                quotes = "\"";
            }
            if (flag_num == 1)
            {
                ListParadigm.AddRange(GetDeclinationAdjectivesAndNoun_WithParam(strIn).Select(f => new Paradigm(quotes + f.word + quotes + afterComma, f.case_w, 0)));
                ListParadigm.Add(new Paradigm(quotes + strIn + quotes + afterComma, 0, 0));
            }
            else if (flag_num == 2 | flag_num == null)
            {
                List<IdParadigm> tblOut = new List<IdParadigm>();
                List<IdParadigm> tblWordsWithParam = new List<IdParadigm>();
                int? flag_gr, lemma_id, gender, number, CodeOfSpeech = null, lastCodeOfSpeech = null, genderFirstNoun = null;
                string wordAdd_2a = null, wordAdd_2b = null, strInNumber_2 = "";
                List<Paradigm> tblStrInSwap = new List<Paradigm>();
                List<Paradigm> tblStrIn = new List<Paradigm>();
                tblOut.AddRange(GetDeclinationAdjectivesAndNoun_WithParam(strIn).Where(b => b.word != strIn).Select(f => new IdParadigm(-1, f.word, f.case_w, 0)));
                tblWordsWithParam.AddRange(strIn.Split(' ').Select(b => new IdParadigm(0, b, 0, 0)));
                for (int i = 0; i < tblWordsWithParam.Count; i += 1)
                {
                    string _word = tblWordsWithParam[i].word.Replace(" ", "");
                    lemma_id = GetIntFromDB(string.Format("select top 1 H.lemma_id from HomonimyInWords H where H.word = '{0}'", _word));
                    if (lemma_id != null)
                        CodeOfSpeech = GetIntFromDB(string.Format("select top 1 LA.CodeOfSpeech code from LemmasInArticles LA where LA.lemma_id = {0}", lemma_id));
                    else
                        lemma_id = -1;
                    if (CodeOfSpeech == null)
                    {
                        List<string> l3 = new List<string>() { "ому", "ему", "ими", "ыми", "ого", "его" };
                        List<string> l2 = new List<string>() { "ая", "яя", "ые", "ие", "ой", "ый", "ий", "ое", "ее", "ой", "ей", "ых", "их", "ым", "им", "ую", "юю", "ом", "ем" };
                        if (l3.Contains(_word.Substring(_word.Length - 3, 3)) | l2.Contains(_word.Substring(_word.Length - 2, 2)))
                        {
                            CodeOfSpeech = 10;
                        }
                        else
                        {
                            CodeOfSpeech = 7;
                        }
                    }
                    tblWordsWithParam[i].ID = (int)lemma_id;
                    tblWordsWithParam[i].case_w = (int)CodeOfSpeech;
                    if (lastCodeOfSpeech == 10 | lastCodeOfSpeech == null)
                    {
                        lastCodeOfSpeech = CodeOfSpeech;
                        if (lastCodeOfSpeech != 10 | lastCodeOfSpeech == null)
                        {
                            genderFirstNoun = GetIntFromDB(string.Format("select top 1 H.gender from HomonimyInWords H where H.word = '{0}'", _word));
                            if (lastCodeOfSpeech == null)
                                lastCodeOfSpeech = 0;
                        }
                    }
                }
                lastCodeOfSpeech = null;
                for (int i = 0; i < tblWordsWithParam.Count; i += 1)
                {
                    string _word = tblWordsWithParam[i].word.Replace(" ", "");
                    lemma_id = tblWordsWithParam[i].ID;
                    CodeOfSpeech = tblWordsWithParam[i].case_w;
                    if (lastCodeOfSpeech != 10 && lastCodeOfSpeech != null)
                    {
                        if (tblStrIn.Count == 1)
                        {
                            tblStrIn.Add(new Paradigm(_word, 0, 0));
                        }
                        else
                        {
                            tblStrIn = tblStrIn.Select(v => new Paradigm(v.word + " " + _word, v.case_w, v.number)).ToList();
                        }
                    }
                    else
                    {
                        lastCodeOfSpeech = CodeOfSpeech;
                        if (genderFirstNoun != null && lemma_id != null && lemma_id > 0)
                        {
                            gender = GetIntFromDB(string.Format("(select top 1 H.gender from HomonimyInWords H where H.word = '{0}')", _word));
                            number = GetIntFromDB(string.Format("(select top 1 H.number from HomonimyInWords H where H.word ='{0}')", _word));
                            if (CodeOfSpeech == 10)
                                wordAdd_2a = GetStringFromDB(string.Format("(select REPLACE(LOWER(max(H.word)),' ','') from HomonimyInWords H where H.lemma_id = {0} and H.number = 1 and H.gender = -1 and H.[case] = 0)", lemma_id));
                            else
                                wordAdd_2a = GetStringFromDB(string.Format("(select REPLACE(LOWER(max(H.word)),' ','') from HomonimyInWords H where H.lemma_id = {0} and H.number = 1 and H.gender = {1} and H.[case] = 0)", lemma_id, genderFirstNoun));
                        }
                        else
                        {
                            if (CodeOfSpeech == 10)
                            {
                                List<string> l = new List<string>() { "ее", "ое", "ий", "ой", "ая", "яя" };
                                if (l.Contains(_word.Substring(_word.Length - 2, 2)))
                                {
                                    wordAdd_2a = _word.Substring(0, _word.Length - 2) + "ие";
                                }
                                else
                                {
                                    if (_word.Substring(_word.Length - 2, 2) == "ый")
                                    {
                                        wordAdd_2a = _word.Substring(0, _word.Length - 2) + "ые";
                                    }
                                    else
                                    {
                                        wordAdd_2a = _word;
                                    }
                                    List<string> l4 = new List<string>() { "ая", "ое" };
                                    if (l4.Contains(_word.Substring(_word.Length - 2, 2)))
                                    {
                                        wordAdd_2b = _word.Substring(0, _word.Length - 2) + "ые";
                                    }
                                }
                            }
                            string right2 = _word.Substring(_word.Length - 2, 2);
                            string right1 = _word.Substring(_word.Length - 1, 1);
                            if (CodeOfSpeech == 7)
                            {
                                List<string> g = new List<string>() { "ия", "ий", "ко" };
                                if (g.Contains(right2))
                                    wordAdd_2a = _word.Substring(0, _word.Length - 1) + "и";
                                else
                                {
                                    g = new List<string>() { "ща", "ча", "ша", "жа" };
                                    if (g.Contains(right2))
                                        wordAdd_2a = _word.Substring(0, _word.Length - 1) + "и";
                                    else if (right2 == "ие")
                                        wordAdd_2a = _word.Substring(0, _word.Length - 1) + "я";
                                    else if (right2 == "ца")
                                        wordAdd_2a = _word.Substring(0, _word.Length - 1) + "ы";
                                    else if (right1 == "о")
                                        wordAdd_2a = _word.Substring(0, _word.Length - 1) + "а";
                                    else if (right1 == "е")
                                        wordAdd_2a = _word.Substring(0, _word.Length - 1) + "я";
                                    else if (right1 == "а")
                                        wordAdd_2a = _word.Substring(0, _word.Length - 1) + "ы";
                                    else if (right1 == "я")
                                        wordAdd_2a = _word.Substring(0, _word.Length - 1) + "и";
                                    else if (right1 == "ь")
                                    {
                                        wordAdd_2a = _word.Substring(0, _word.Length - 1) + "и";
                                        wordAdd_2b = _word.Substring(0, _word.Length - 1) + 'я';
                                    }
                                    else if (new Regex("[цкнгшщзхждлрпвфчсмтб]").IsMatch(right1))
                                    {
                                        wordAdd_2a = _word + "ы";
                                        wordAdd_2b = _word + "ы";
                                    }
                                    else
                                        wordAdd_2a = _word;
                                }
                            }
                        }
                        if (tblStrIn.Count == 0)
                        {
                            tblStrIn.Add(new Paradigm(wordAdd_2a, -1, 0));
                            tblStrIn.Add(new Paradigm(wordAdd_2b, 2, 0));
                        }
                        else
                        {
                            for (int j = 0; j < tblStrIn.Count; j += 1)
                            {
                                strInNumber_2 = tblStrIn[j].word;
                                if (tblStrIn[j].word != null)
                                    tblStrIn[j].word = tblStrIn[j].word + " " + wordAdd_2a;
                                if (wordAdd_2b != null)
                                    tblStrInSwap.Add(new Paradigm(strInNumber_2 + " " + wordAdd_2b, 0, 0));
                            }
                            tblStrIn.AddRange(tblStrInSwap);
                        }
                        wordAdd_2a = null;
                        wordAdd_2b = null;
                    }
                }
                tblStrIn = tblStrIn.Where(n => (n.word != null)).ToList();
                if (tblStrIn.Count == 1)
                {
                    tblOut.AddRange(GetDeclinationAdjectivesAndNoun_Plural(tblStrIn.First().word.Trim(), null).Select(m => new IdParadigm(-1, m.word, m.case_w, 1)));
                    tblOut.AddRange(tblStrIn.Select(m => new IdParadigm(-1, m.word, m.case_w, m.number)));
                }
                else
                {
                    flag_gr = 2;
                    foreach (Paradigm p in tblStrIn)
                    {
                        strInNumber_2 = p.word;
                        if (strInNumber_2 != null | strInNumber_2 == "")
                        {
                            tblOut.Add(new IdParadigm((int)flag_gr, p.word, 0, 0));
                            tblOut.AddRange(GetDeclinationAdjectivesAndNoun_Plural(strInNumber_2.Trim(), null).Select(f => new IdParadigm((int)flag_gr, f.word, f.case_w, 1)));
                            flag_gr += 1;
                        }
                    }
                }
                ListParadigm.AddRange(tblOut.Select(f => new Paradigm(quotes + f.word + quotes + afterComma, f.case_w, f.number)));
                ListParadigm.Add(new Paradigm(quotes + strIn + quotes + afterComma, 0, 0));
            }
            //List<Paradigm> tmp = ListParadigm.Select(f => new Paradigm(f.word.Replace("-", " - "), f.case_w, f.number)).ToList();
            //ListParadigm.AddRange(tmp.Except(ListParadigm));
            foreach (Paradigm par in ListParadigm)
                par.word = GetInCorrectRegister(strIn, par.word);
            return ListParadigm.Distinct(new ParadigmComparer()).ToList();
        }

        public List<string> GetAllDeclinationWithNumber(string strIn, int? flag_num)
        {
            List<string> ListParadigm = new List<string>();
            foreach (Paradigm p in GetAllDeclinationWithNumber_WithParam(strIn, flag_num))
                ListParadigm.Add(p.word);
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
    }

    class ParadigmComparer : IEqualityComparer<ParadigmGenerator.Paradigm>
    {
        public bool Equals(ParadigmGenerator.Paradigm x, ParadigmGenerator.Paradigm y)
        {
            if (y.word == x.word && y.case_w == x.case_w && y.number == x.number)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(ParadigmGenerator.Paradigm obj)
        {
            return obj.ToString().ToLower().GetHashCode();
        }
    }
}
