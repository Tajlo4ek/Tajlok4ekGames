using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DataStore
{
    [DataContract]
    public class Theme
    {
        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        private readonly Question[] questions;

        public bool IsUsed { get; private set; }

        public int CountQuestions { get { return questions.Length; } }

        public Question GetQuestion(int id)
        {
            if (id < CountQuestions)
                return questions[id];

            throw new Exception("out of range");
        }

        public Theme(string name, List<Question> questions)
        {
            this.Name = name;
            this.questions = questions.ToArray();
        }

        public void Update(string workPath)
        {
            foreach (var q in questions)
            {
                q.Update(workPath, Name);
            }
        }

        public void SetEnd()
        {
            foreach (var q in this.questions)
            {
                q.SetEnd();
            }
        }

        public void SetUsed()
        {
            IsUsed = true;
        }

    }
}
