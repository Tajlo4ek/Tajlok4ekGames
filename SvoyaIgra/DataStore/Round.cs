using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DataStore
{
    [DataContract]
    public class Round
    {
        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        private readonly Theme[] themes;

        [DataMember]
        public readonly bool IsFinal;

        public int CountThemes
        {
            get { return themes.Length; }
        }

        public int CountQuestions { get; private set; }

        public Theme GetTheme(int id)
        {
            if (id < CountThemes)
                return themes[id];

            throw new Exception("out of range");
        }

        public string GetThemesName()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var theme in themes)
            {
                sb.AppendLine(theme.Name);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public Round(string name, List<Theme> themes, bool isFinal)
        {
            this.Name = name;
            this.themes = themes.ToArray();
            this.IsFinal = isFinal;
        }

        public void Update(string workPath)
        {
            foreach (var t in themes)
            {
                CountQuestions += t.CountQuestions;
                t.Update(workPath);
            }
        }

    }
}
