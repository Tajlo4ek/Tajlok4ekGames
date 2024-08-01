using DataStore.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using static DataStore.Question;
using static DataStore.Scenario;

namespace DataStore.Utils.PackUtils
{
    public static class PackConverter
    {
        private const string BufPath = @"\buf";

        public static void Convert(string packPath, PackManager packManager, string packName, Action<string> process = null)
        {
            var bufPath = packManager.WorkDirectory + BufPath;

            CreateDirectory(bufPath);

            Dictionary<string, string> rename = new Dictionary<string, string>();

            process?.Invoke("start unzip");

            using (ZipArchive zipArchive = ZipFile.OpenRead(packPath))
            {
                var entries = zipArchive.Entries;
                int countMax = zipArchive.Entries.Count;
                int count = 1;

                foreach (ZipArchiveEntry entry in entries)
                {
                    process?.Invoke(string.Format("unzip {0} / {1}", count, countMax));

                    var newName = count.ToString();

                    int ind = entry.FullName.LastIndexOf('.');
                    if (ind != -1)
                    {
                        newName += entry.FullName.Substring(ind);
                    }

                    if (!rename.ContainsKey(entry.Name))
                    {
                        rename.Add(entry.Name, bufPath + @"\" + newName);
                        entry.ExtractToFile(bufPath + @"\" + newName);
                        count++;
                    }
                }
            }

            process?.Invoke("end unzip");

            File.Move(rename["content.xml"], bufPath + @"\content.xml");

            var doc = new XmlDocument();
            doc.Load(bufPath + @"\content.xml");
            var root = doc.DocumentElement;

            process?.Invoke("start parse content");

            var package = ParseXml(root, rename, packManager);//new Package(root, rename, packManager);

            process?.Invoke("end parse content");

            process?.Invoke("start create zip");

            FileStream writer = new FileStream(packManager.WorkDirectory + @"\content.xml", FileMode.Create);
            DataContractSerializer ser = new DataContractSerializer(typeof(Package));
            ser.WriteObject(writer, package);
            writer.Close();

            DeleteDirectory(bufPath);

            var bufManager = new PackManager();
            var buf = bufManager.WorkDirectory;

            ZipFile.CreateFromDirectory(packManager.WorkDirectory, buf + @"\" + packName);

            process?.Invoke("zip created. finishing...");

            RecreateDirectory(packManager.WorkDirectory);

            File.Copy(buf + @"\" + packName, packManager.WorkDirectory + @"\" + packName);

            DeleteDirectory(buf);
        }

        public static void SavePack(Package package, string path, Action<string> process = null)
        {
            var packManager = new PackManager();
            var bufDir = packManager.WorkDirectory;

            int countRound = package.CountRounds;
            for (int roundId = 0; roundId < countRound; roundId++)
            {
                var roundString = string.Format("Раунд {0} / {1} \n", roundId + 1, countRound);
                var round = package.GetRound(roundId);
                int themeCount = round.CountThemes;
                for (int themeId = 0; themeId < themeCount; themeId++)
                {
                    var themeString = string.Format("{0} Тема {1} / {2} \n", roundString, themeId + 1, themeCount);
                    var theme = round.GetTheme(themeId);
                    var countQuestion = theme.CountQuestions;
                    for (int questionId = 0; questionId < countQuestion; questionId++)
                    {
                        process?.Invoke(string.Format("{0} Вопрос {1} / {2} \n", themeString, questionId + 1, countQuestion));
                        var question = theme.GetQuestion(questionId);
                        for (int scenarioId = 0; scenarioId < question.CountScenarios; scenarioId++)
                        {
                            var scenario = question.GetScenario(scenarioId);
                            if (scenario.IsMedia)
                            {
                                scenario.UpdataData(packManager.AddToPackFolder(scenario.Data));
                            }
                        }

                        for (int scenarioId = 0; scenarioId < question.CountAnswer; scenarioId++)
                        {
                            var scenario = question.GetAnswer(scenarioId);
                            if (scenario.IsMedia)
                            {
                                scenario.UpdataData(packManager.AddToPackFolder(scenario.Data));
                            }
                        }
                    }
                }
            }

            using (FileStream writer = new FileStream(bufDir + @"\content.xml", FileMode.Create))
            {
                DataContractSerializer ser = new DataContractSerializer(typeof(Package));
                ser.WriteObject(writer, package);
            }


            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Create))
            {
                var files = Directory.EnumerateFiles(bufDir);
                int total = files.Count();
                int count = 1;
                foreach (var file in files)
                {
                    FileInfo info = new FileInfo(file);
                    archive.CreateEntryFromFile(file, info.Name);
                    process?.Invoke(string.Format("сохранение медиафайлов \n {0} / {1}", count++, total));
                }
            }

            packManager.Dispose();
        }

        private static void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public static void DeleteDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            foreach (var dir in Directory.EnumerateDirectories(path))
            {
                DeleteDirectory(dir);
            }

            foreach (var file in Directory.EnumerateFiles(path))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            Directory.Delete(path, true);
        }

        public static void RecreateDirectory(string path)
        {
            DeleteDirectory(path);
            CreateDirectory(path);
        }

        private static Package ParseXml(XmlElement root, Dictionary<string, string> rename, PackManager packManager)
        {
            int.TryParse(root.GetAttribute("version"), out var version);

            switch (version)
            {
                case 4:
                    return ParseXmlVersion_4(root, rename, packManager);
                case 5:
                    return ParseXmlVersion_5(root, rename, packManager);
                default:
                    throw new BadFileException("unknow version: " + version);
            }
        }

        private static Package ParseXmlVersion_4(XmlElement root, Dictionary<string, string> rename, PackManager packManager)
        {
            var rounds = new List<Round>();

            foreach (var roundsDataXml in Utils.MyXmlUtils.GetNodeWithName(root, "rounds"))
            {
                foreach (var roundXml in Utils.MyXmlUtils.GetNodeWithName(roundsDataXml, "round"))
                {
                    var themes = new List<Theme>();
                    foreach (var themesXml in Utils.MyXmlUtils.GetNodeWithName(roundXml, "themes"))
                    {
                        foreach (var themeXml in Utils.MyXmlUtils.GetNodeWithName(themesXml, "theme"))
                        {
                            var themeName = themeXml.GetAttribute("name");

                            var questions = new List<Question>();

                            foreach (var questionsXml in Utils.MyXmlUtils.GetNodeWithName(themeXml, "questions"))
                            {
                                foreach (var questionXml in Utils.MyXmlUtils.GetNodeWithName(questionsXml, "question"))
                                {
                                    var cost = int.Parse(questionXml.GetAttribute("price"));

                                    var scenarios = new List<Scenario>();
                                    var answer = new List<Scenario>();

                                    StringBuilder sb = new StringBuilder();

                                    foreach (var right in Utils.MyXmlUtils.GetNodeWithName(questionXml, "right"))
                                    {
                                        foreach (var anwer in Utils.MyXmlUtils.GetNodeWithName(right, "answer"))
                                        {
                                            sb.AppendLine(anwer.InnerText);
                                        }
                                    }

                                    foreach (var scenariosXml in Utils.MyXmlUtils.GetNodeWithName(questionXml, "scenario"))
                                    {
                                        foreach (var scenarioXml in Utils.MyXmlUtils.GetNodeWithName(scenariosXml, "atom"))
                                        {
                                            ScenarioType type = ScenarioType.Text;
                                            switch (scenarioXml.GetAttribute("type"))
                                            {
                                                case "image":
                                                    type = ScenarioType.Image;
                                                    break;

                                                case "video":
                                                    type = ScenarioType.Video;
                                                    break;

                                                case "voice":
                                                    type = ScenarioType.Audio;
                                                    break;

                                                case "marker":
                                                    type = ScenarioType.Marker;
                                                    break;

                                                default:
                                                    type = ScenarioType.Text;
                                                    break;
                                            }

                                            int.TryParse(scenarioXml.GetAttribute("time"), out int delay);
                                            var time = delay == 0 ? -1 : delay;

                                            var Data = scenarioXml.InnerText;
                                            if (Data.StartsWith("@"))
                                            {
                                                try
                                                {
                                                    Data = "@" + rename[Uri.EscapeUriString(Data.Substring(1))];
                                                }
                                                catch
                                                {
                                                    Data = "Отсутствует файл: " + Data;
                                                    type = ScenarioType.Text;
                                                }
                                            }

                                            if (type != ScenarioType.Text && type != ScenarioType.Marker)
                                            {
                                                Data = packManager.AddToPackFolder(Data);
                                            }

                                            scenarios.Add(new Scenario(Data, type, time));
                                        }
                                    }

                                    var questionType = QuestionType.Normal;
                                    var specialCost = 0;
                                    var ThemeName = "";

                                    foreach (var typeXml in Utils.MyXmlUtils.GetNodeWithName(questionXml, "type"))
                                    {
                                        var questionTypeName = typeXml.GetAttribute("name");
                                        if (questionTypeName.Equals("cat") || questionTypeName.Equals("bagcat"))
                                        {
                                            questionType = QuestionType.Bagcat;

                                            foreach (var param in Utils.MyXmlUtils.GetNodeWithName(typeXml, "param"))
                                            {
                                                var paramName = param.GetAttribute("name");
                                                if (paramName.Equals("theme"))
                                                {
                                                    ThemeName = param.InnerText;
                                                }
                                                else if (paramName.Equals("cost"))
                                                {
                                                    if (int.TryParse(param.InnerText, out specialCost) == false)
                                                    {
                                                        specialCost = new Random().Next(100, 1000);
                                                    }
                                                }
                                            }
                                        }
                                        else if (questionTypeName.Equals("auction"))
                                        {
                                            questionType = QuestionType.Auction;
                                        }
                                    }

                                    int markerId = -1;
                                    for (int scId = 0; scId < scenarios.Count; scId++)
                                    {
                                        if (scenarios[scId].Type == Scenario.ScenarioType.Marker)
                                        {
                                            markerId = scId;
                                            break;
                                        }
                                    }

                                    if (markerId != -1)
                                    {
                                        for (int scId = markerId + 1; scId < scenarios.Count; scId++)
                                        {
                                            answer.Add(scenarios[scId]);
                                        }

                                        for (int scId = scenarios.Count - 1; scId >= markerId; scId--)
                                        {
                                            scenarios.RemoveAt(scId);
                                        }
                                    }

                                    answer.Add(new Scenario(sb.ToString(), Scenario.ScenarioType.Text));
                                    questions.Add(new Question(scenarios, answer, cost, questionType, themeName, specialCost));
                                }
                            }

                            themes.Add(new Theme(themeName, questions));
                        }
                    }
                    rounds.Add(new Round(roundXml.GetAttribute("name"), themes, roundXml.GetAttribute("type").Equals("final")));
                }
            }

            var infoData = Utils.MyXmlUtils.GetNodeWithName(root, "info");
            var packInfo = infoData.Count > 0 ? new PackInfo(infoData[0]) : new PackInfo("No data");
            return new Package(root.GetAttribute("name"), rounds, packInfo);
        }

        private static Package ParseXmlVersion_5(XmlElement root, Dictionary<string, string> rename, PackManager packManager)
        {
            var rounds = new List<Round>();

            foreach (var roundsDataXml in Utils.MyXmlUtils.GetNodeWithName(root, "rounds"))
            {
                foreach (var roundXml in Utils.MyXmlUtils.GetNodeWithName(roundsDataXml, "round"))
                {
                    var themes = new List<Theme>();
                    foreach (var themesXml in Utils.MyXmlUtils.GetNodeWithName(roundXml, "themes"))
                    {
                        foreach (var themeXml in Utils.MyXmlUtils.GetNodeWithName(themesXml, "theme"))
                        {
                            var themeName = themeXml.GetAttribute("name");

                            var questions = new List<Question>();

                            foreach (var questionsXml in Utils.MyXmlUtils.GetNodeWithName(themeXml, "questions"))
                            {
                                foreach (var questionXml in Utils.MyXmlUtils.GetNodeWithName(questionsXml, "question"))
                                {
                                    var cost = int.Parse(questionXml.GetAttribute("price"));

                                    var questionTypeString = questionXml.GetAttribute("type");
                                    var questionType = QuestionType.Normal;

                                    if (questionTypeString.Equals("noRisk", StringComparison.OrdinalIgnoreCase))
                                    {
                                        questionType = QuestionType.NoRisk;
                                    }

                                    var scenarios = new List<Scenario>();
                                    var answer = new List<Scenario>();


                                    var specialTheme = "";
                                    var specialCost = cost;

                                    StringBuilder sb = new StringBuilder();

                                    foreach (var right in Utils.MyXmlUtils.GetNodeWithName(questionXml, "right"))
                                    {
                                        foreach (var anwer in Utils.MyXmlUtils.GetNodeWithName(right, "answer"))
                                        {
                                            sb.AppendLine(anwer.InnerText);
                                        }
                                    }


                                    foreach (var paramsXml in Utils.MyXmlUtils.GetNodeWithName(questionXml, "params"))
                                    {
                                        foreach (var paramXml in Utils.MyXmlUtils.GetNodeWithName(paramsXml, "param"))
                                        {
                                            var paramType = paramXml.GetAttribute("type");
                                            var paramName = paramXml.GetAttribute("name");

                                            if (paramName.Equals("answer") || paramName.Equals("question"))
                                            {
                                                bool isAnswer = paramName.Equals("answer");

                                                foreach (var itemXml in Utils.MyXmlUtils.GetNodeWithName(paramXml, "item"))
                                                {
                                                    ScenarioType type = ScenarioType.Text;
                                                    switch (itemXml.GetAttribute("type"))
                                                    {
                                                        case "image":
                                                            type = ScenarioType.Image;
                                                            break;

                                                        case "video":
                                                            type = ScenarioType.Video;
                                                            break;

                                                        case "audio":
                                                            type = ScenarioType.Audio;
                                                            break;

                                                        default:
                                                            type = ScenarioType.Text;
                                                            break;
                                                    }

                                                    int.TryParse(itemXml.GetAttribute("time"), out int delay);
                                                    var time = delay == 0 ? -1 : delay;

                                                    var Data = itemXml.InnerText;
                                                    if (itemXml.GetAttribute("isRef").ToLower().Equals("true"))
                                                    {
                                                        try
                                                        {
                                                            Data = "@" + rename[Uri.EscapeUriString(Data)];
                                                        }
                                                        catch (Exception)
                                                        {
                                                            Data = "Отсутствует файл: " + Data;
                                                            type = ScenarioType.Text;
                                                        }
                                                    }

                                                    if (type != ScenarioType.Text && type != ScenarioType.Marker)
                                                    {
                                                        Data = packManager.AddToPackFolder(Data);
                                                    }

                                                    if (isAnswer)
                                                    {
                                                        answer.Add(new Scenario(Data, type, time));
                                                    }
                                                    else
                                                    {
                                                        scenarios.Add(new Scenario(Data, type, time));
                                                    }
                                                }
                                            }
                                            else if (paramName.Equals("theme"))
                                            {
                                                specialTheme = paramXml.InnerText;
                                            }
                                            else if (paramName.Equals("selectionMode"))
                                            {
                                                if (questionType == QuestionType.NoRisk)
                                                {
                                                    continue;
                                                }

                                                if (paramXml.InnerText.Equals("exceptCurrent"))
                                                {
                                                    questionType = QuestionType.Auction;
                                                }
                                                else if (paramsXml.InnerText.Equals("any"))
                                                {
                                                    questionType = QuestionType.Bagcat;
                                                }
                                            }
                                            else if (paramName.Equals("price"))
                                            {
                                                var numberSet = Utils.MyXmlUtils.GetNodeWithName(paramsXml, "numberSet");
                                                if (numberSet.Count == 0)
                                                {
                                                    continue;
                                                }

                                                int.TryParse(numberSet[0].GetAttribute("minimum"), out int min);
                                                int.TryParse(numberSet[0].GetAttribute("maximum"), out int max);

                                                specialCost = Math.Max(cost, min);
                                            }
                                        }
                                    }

                                    answer.Add(new Scenario(sb.ToString(), Scenario.ScenarioType.Text));
                                    questions.Add(new Question(scenarios, answer, cost, questionType, themeName, specialCost));
                                }
                            }

                            themes.Add(new Theme(themeName, questions));
                        }
                    }
                    rounds.Add(new Round(roundXml.GetAttribute("name"), themes, roundXml.GetAttribute("type").Equals("final")));
                }
            }

            var infoData = Utils.MyXmlUtils.GetNodeWithName(root, "info");
            var packInfo = infoData.Count > 0 ? new PackInfo(infoData[0]) : new PackInfo("No data");
            return new Package(root.GetAttribute("name"), rounds, packInfo);
        }

    }
}
