using DataStore;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Editor.MyControl
{
    /// <summary>
    /// Логика взаимодействия для PackageControl.xaml
    /// </summary>
    public partial class PackageControl : UserControl
    {

        private readonly List<RoundControl> roundControls;

        public Action PackageNameChanged;

        public string PackPath { get; set; }

        public bool IsPathAvailable
        {
            get
            {
                return !PackPath.Equals("");
            }
        }

        public string PackName
        {
            get
            {
                return tbPackageName.Text;
            }
        }

        public Action ContentChanged;

        private bool isSaved;

        private bool blockCallBack;

        public bool IsSaved
        {
            get
            {
                return isSaved;
            }

            private set
            {
                isSaved = value;
                if (!blockCallBack)
                {
                    ContentChanged?.Invoke();
                }
            }
        }

        private readonly DataStore.Utils.PackUtils.PackManager loader;

        public PackageControl() : this("новый")
        {

        }

        public PackageControl(string name)
        {
            InitializeComponent();

            roundControls = new List<RoundControl>();

            tbAutors.Text = "автор";
            tbPackageName.Text = name;

            PackPath = "";

            loader = new DataStore.Utils.PackUtils.PackManager();

            IsSaved = false;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            CreateRound();
        }

        public Package GetData()
        {
            var data = new List<Round>();

            IsSaved = true;

            foreach (var control in roundControls)
            {
                var round = control.GetData();

                if (round.CountThemes != 0)
                {
                    data.Add(round);
                }
            }

            return new Package(tbPackageName.Text, data, new PackInfo(tbAutors.Text));
        }

        public void ParsePack(string packPath, Action<string> process = null)
        {
            IsSaved = true;
            blockCallBack = true;

            process?.Invoke("start load");

            var package = loader.LoadPack(packPath, process);

            this.Dispatcher.Invoke(new Action(() =>
                {
                    tbAutors.Text = package.Authors;
                    tbPackageName.Text = package.Name;
                }));

            process?.Invoke("start create view");

            int roundCount = package.CountRounds;

            for (int roundId = 0; roundId < roundCount; roundId++)
            {
                var curString = string.Format("Раунд {0} / {1} \n", roundId + 1, roundCount);
                CreateRound(package.GetRound(roundId), (text) =>
                {
                    process?.Invoke(string.Format("{0} {1} \n", curString, text));
                });
            }

            process?.Invoke("loaded");

            blockCallBack = false;

        }

        private void CreateRound(Round round = null, Action<string> process = null)
        {
            var name = round == null ? "Раунд " + (roundControls.Count + 1) : round.Name;

            RoundControl control = null;

            this.Dispatcher.Invoke(new Action(() =>
            {
                control = new RoundControl(name, loader);
                control.DeleteAction += DeleteRound;
                control.ContentChanged += () => { IsSaved = false; };
                roundControls.Add(control);
                stackPanel.Children.Add(control);
            }));

            if (round != null)
            {
                control.ParseRound(round, process);
            }

            IsSaved = false;
        }

        private void TbPackageName_TextChanged(object sender, EventArgs e)
        {
            PackageNameChanged?.Invoke();
            IsSaved = false;
        }

        private void TbAutors_TextChanged(object sender, EventArgs e)
        {
            IsSaved = false;
        }

        private void DeleteRound(RoundControl roundControl)
        {
            roundControls.Remove(roundControl);
            stackPanel.Children.Remove(roundControl);

            IsSaved = false;
        }

        public void Clear()
        {
            foreach (var control in roundControls)
            {
                control.Clear();
            }

            loader.Dispose();

            GC.Collect();
        }

        public void ForceUpdate()
        {
            IsSaved = false;
        }

    }
}
