using DataStore.Utils.PackUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DataStore
{
    [DataContract]
    public class Question
    {
        public enum QuestionType
        {
            Normal,
            Bagcat,
            Auction,
            NoRisk,
        }

        [DataMember]
        private readonly Scenario[] scenarios;

        public int CountScenarios { get { return scenarios.Length; } }

        public int CountAnswer { get { return answers.Length; } }

        [DataMember]
        private readonly Scenario[] answers;

        public string StrAnswer { get; private set; }

        [DataMember]
        public int Cost { get; private set; }

        public bool IsBagcat { get { return questionType == QuestionType.Bagcat; } }

        public bool IsAuction { get { return questionType == QuestionType.Auction; } }

        public bool IsNoRisk { get { return questionType == QuestionType.NoRisk; } }

        public bool IsNormal { get { return questionType == QuestionType.Normal; } }

        [DataMember]
        public string ThemeName { get; private set; }

        [DataMember]
        public int SpecialCost { get; private set; }

        [DataMember]
        public readonly QuestionType questionType;

        public bool IsUsed { get; private set; }

        public Scenario GetScenario(int id)
        {
            if (id < CountScenarios)
                return scenarios[id];

            throw new Exception("out of range");
        }

        public Scenario GetAnswer(int id)
        {
            if (id < CountAnswer)
                return answers[id];

            throw new Exception("out of range");
        }

        public Question(List<Scenario> qScenario, List<Scenario> aScenario, int cost, QuestionType questionType)
        {
            this.scenarios = qScenario.ToArray();
            this.answers = aScenario.ToArray();
            this.Cost = cost;
            this.questionType = questionType;
        }

        public Question(List<Scenario> qScenario, List<Scenario> aScenario, int cost, QuestionType questionType, string themeName, int specialCost) :
            this(qScenario, aScenario, cost, questionType)
        {
            this.ThemeName = themeName;
            this.SpecialCost = specialCost;
        }

        public void Update(string workPath, string themeName)
        {
            foreach (var s in scenarios)
            {
                s.Update(workPath);
            }

            foreach (var s in answers)
            {
                s.Update(workPath);
            }

            if (!IsBagcat || ThemeName.Equals(""))
            {
                ThemeName = themeName;
            }

            var find = answers.First((x) => x.Type == Scenario.ScenarioType.Text);

            StrAnswer = find != null ? find.Data : "no data";
        }

        public void SetEnd()
        {
            IsUsed = true;
        }

    }
}
