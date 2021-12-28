using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Teams.Apps.FAQPlusPlus.ModelQuestions
{
    public class NoSearchedWords
    {
        private string lWord;
        public NoSearchedWords()
        { }

        public bool ValidateWord(string newWord)
        {
            bool answer = false;
            string ignoredWords = "Preguntar a un experto,Compartir comentarios,Share feedback,Ask an expert,Take a tour,Siguiente,Finalizar,Cerrar,Hola,Buenas,Buenos,afternoon,morning,hello,hey";
            string novalidword;
            for (int i = 0; i <= ignoredWords.Split(",").Length; i++)
            {
                novalidword = ignoredWords.Split(",")[i].ToLower().Trim();
                newWord = newWord.ToLower().Trim();

                if (newWord == novalidword)
                {
                    answer = true;
                    return answer;
                }
            }

            return answer;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="NoSearchedWords"/> class.
        /// </summary>
        /// <param name="word"></param>
        public string getValidQuestion(string word)
        {
            this.lWord = word;
            this.lWord = this.lWord.ToLower().Trim();
            return this.lWord;
        }
    }
}
