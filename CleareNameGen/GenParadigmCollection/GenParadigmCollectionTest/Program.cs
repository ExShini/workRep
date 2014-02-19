using System;
using System.Collections.Generic;
using System.Linq;
using GenParadigmCollection;


namespace GenParadigmCollectionTest
{
    class Program
    {
        static void Main(string[] args)  // тесты генераторов
        {
            /*
            List<ParadigmGenerator.Paradigm> lPars = GenParadigmCollection.ParadigmGenerator.GetDeclinationAdjective_WithParam("1ая");
            lPars = GenParadigmCollection.ParadigmGenerator.GetDeclinationAdjective_WithParam("1-ая");
            lPars = GenParadigmCollection.ParadigmGenerator.GetDeclinationAdjective_WithParam("1-я");
            lPars = GenParadigmCollection.ParadigmGenerator.GetDeclinationAdjective_WithParam("1-й");
            lPars = GenParadigmCollection.ParadigmGenerator.GetDeclinationAdjective_WithParam("1й");
            lPars = GenParadigmCollection.ParadigmGenerator.GetDeclinationAdjective_WithParam("1-е");
            lPars = GenParadigmCollection.ParadigmGenerator.GetDeclinationAdjective_WithParam("1е");
            */
            //Console.WriteLine(GenParadigmCollection.ParadigmGenerator.GetInCorrectRegister("КОмПАНИЯ, ФонД", "компаНиями, фОндом"));
            GenParadigmCollection.ParadigmGenerator pg = new ParadigmGenerator(ParadigmGenerator.EParadigmGeneratorMode.EModeGeoRussia);
            pg.GenMode = ParadigmGenerator.EParadigmGeneratorMode.EModeSecondName;

            List<GenParadigmCollection.ParadigmGenerator.Paradigm> ListParadigm;
            while (1 == 1)
            {
                string r = Console.ReadLine();
                Console.WriteLine();
                //ListParadigm = pg.GetAllDeclinationWithNumber(r, 2);

                ListParadigm = pg.GetDeclinationAdjectivesAndNounWithDash_WithParam(r);

                //ListParadigm = pg.GetAllDeclinationCompanyName_WithParam(r);
                //ListParadigm = pg.GetAllDeclinationCompanyName_WithParam("ТНГ-Групп");
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
    }
}
