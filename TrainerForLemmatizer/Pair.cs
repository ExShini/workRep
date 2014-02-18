using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainerForLemmatizer
{
    class Pair
    {
        public Pair(String lemma, String wordForm)
        {
            m_lemma = lemma;
            m_wordForm = wordForm;
        }

        protected String m_lemma = "";
        protected String m_wordForm = "";

        public String Lemma
        {
            set { m_lemma = value; }
            get { return m_lemma; }
        }

        public String WordForm
        {
            set { m_wordForm = value; }
            get { return m_wordForm; }
        }
    }
}
