using SMPSOsimulation.dataStructures;

namespace SMPSOGui
{
    public partial class Form1 : Form
    {
        private string? exePath = null, gtkPath = null;
        List<string>? tracePaths = null;

        private enum SearchType
        {
            WEIGHT_PSO,
            LEXICOGRAPHIC_PSO,
            VEGA,
            SMPSO
        }

        private void enablePopulationSize(bool value)
        {
            labelPopulationSize.Enabled = value;
            numericPopulationSize.Enabled = value;
        }

        private void enableLeadersArchive(bool value)
        {
            labelLeadersArchiveSize.Enabled = value;
            numericLeadersArchiveSize.Enabled = value;
        }

        private void enableMaxGenerations(bool value)
        {
            labelMaxGenerations.Enabled = value;
            numericMaxGenerations.Enabled = value;
        }

        private void enableMutationRate(bool value)
        {
            labelMutationRate.Enabled = value;
            numericMutationRate.Enabled = value;
        }

        private void enableWeightCPI(bool value)
        {
            labelWeightCPI.Enabled = value;
            numericWeightCPI.Enabled = value;
        }

        private void enableWeightEnergy(bool value)
        {
            labelWeightEnergy.Enabled = value;
            numericWeightEnergy.Enabled = value;
        }

        private void enablePrefferedObjective(bool value)
        {
            labelPrefferedObjective.Enabled = value;
            comboPrefferedObjective.Enabled = value;
        }

        private void enableL1Cache(bool value)
        {
            labelL1CodeHitrate.Enabled = value;
            labelL1CodeLatency.Enabled = value;
            labelL1DataLatency.Enabled = value;
            labell1DataHitrate.Enabled = value;
            numericL1CodeHitrate.Enabled = value;
            numericL1CodeLatency.Enabled = value;
            numericL1DataLatency.Enabled = value;
            numericl1DataHitrate.Enabled = value;
        }

        private void enableL2Cache(bool value)
        {
            labelL2Hitrate.Enabled = value;
            labelL2Latency.Enabled = value;
            numericL2Hitrate.Enabled = value;
            numericL2Latency.Enabled = value;
        }

        private void disableAllContextInputs()
        {
            enableL1Cache(false);
            enableL2Cache(false);
            enableLeadersArchive(false);
            enableMaxGenerations(false);
            enableMutationRate(false);
            enablePopulationSize(false);
            enablePrefferedObjective(false);
            enableWeightCPI(false);
            enableWeightEnergy(false);
        }

        private void enableSMPSO(bool value)
        {
            enablePopulationSize(value);
            enableLeadersArchive(value);
            enableMaxGenerations(value);
            enableMutationRate(value);
        }

        private void enableVESA(bool value)
        {
            enablePopulationSize(value);
            enableMaxGenerations(value);
            enableMutationRate(value);
        }


        private void initSearchTypeCombo()
        {
            comboBoxSearchAlgorithm.DataSource = Enum.GetValues(typeof(SearchType));
            comboBoxSearchAlgorithm.SelectedItem = SearchType.WEIGHT_PSO;
            comboBoxSearchAlgorithm.SelectedIndexChanged += comboBoxSearchAlgorithm_SelectedIndexChanged;
            combomemoryArch.DataSource = Enum.GetValues(typeof(MemoryArchEnum));
            combomemoryArch.SelectedItem = MemoryArchEnum.system;
            combomemoryArch.SelectedIndexChanged += combomemoryArch_SelectedIndexChanged;
            comboPrefferedObjective.DataSource = Enum.GetValues(typeof(DominationConfig.PrefferedObjective));
            comboPrefferedObjective.SelectedItem = DominationConfig.PrefferedObjective.CPI;
            enableSMPSO(true);
            enableWeightCPI(true);
            enableWeightEnergy(true);
            enablePrefferedObjective(false);
        }

        private void comboBoxSearchAlgorithm_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var selectedEnvironment = (SearchType)comboBoxSearchAlgorithm.SelectedItem!;
            switch (selectedEnvironment)
            {
                case SearchType.WEIGHT_PSO:
                    enableVESA(false);
                    enableSMPSO(true);
                    enableWeightCPI(true);
                    enableWeightEnergy(true);
                    enablePrefferedObjective(false);
                    break;
                case SearchType.LEXICOGRAPHIC_PSO:
                    enableVESA(false);
                    enableSMPSO(true);
                    enableWeightCPI(false);
                    enableWeightEnergy(false);
                    enablePrefferedObjective(true);
                    break;
                case SearchType.SMPSO:
                    enableVESA(false);
                    enableSMPSO(true);
                    enableWeightCPI(false);
                    enableWeightEnergy(false);
                    enablePrefferedObjective(false);
                    break;
                case SearchType.VEGA:
                    enableSMPSO(false);
                    enableVESA(true);
                    enableWeightCPI(false);
                    enableWeightEnergy(false);
                    enablePrefferedObjective(false);
                    break;
            }
        }

        private void combomemoryArch_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var selectedEnvironment = (MemoryArchEnum)combomemoryArch.SelectedItem!;
            switch (selectedEnvironment)
            {
                case MemoryArchEnum.system:
                    enableL1Cache(false);
                    enableL2Cache(false);
                    break;
                case MemoryArchEnum.l1:
                    enableL1Cache(true);
                    enableL2Cache(false);
                    break;
                case MemoryArchEnum.l2:
                    enableL1Cache(true);
                    enableL2Cache(true);
                    break;
            }
        }


        public Form1()
        {
            InitializeComponent();
            disableAllContextInputs();
            initSearchTypeCombo();
        }

        private void buttonFilePickerExe_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "C:\\";
                openFileDialog.Filter = "PSAT Simulation Executable|psatsim_con.exe";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    exePath = openFileDialog.FileName;
                    labelFilePathExe.Text = exePath;
                }
            }
        }

        private void buttonFilePickerGTK_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "Select the GTK/lib folder.";
                folderBrowserDialog.ShowNewFolderButton = true; // Allow creating new folders
                folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer; // Starting point

                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    gtkPath = folderBrowserDialog.SelectedPath;
                    labelFilePickerGTK.Text = gtkPath;
                }
            }
        }

        private void buttonFilePickerTrace_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "C:\\";
                openFileDialog.Filter = "TRA Files (*.tra)|*.tra";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Multiselect = true; 

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string[] selectedFiles = openFileDialog.FileNames; // Get all selected file paths
                    tracePaths = new List<string>(selectedFiles);
                    textTraces.Text = string.Join(Environment.NewLine, selectedFiles); // Display selected file paths
                }
            }
        }

        private void buttonStartSearch_Click(object sender, EventArgs e)
        {
            if (exePath == null || gtkPath == null || tracePaths == null)
            {
                MessageBox.Show("Must provide all of psatsim_con.exe path, GTK/lib path, and trace file path");
            }
            else
            {
                var envConf = new EnvironmentConfig(
                    (double)numericVdd.Value,
                    (MemoryArchEnum)combomemoryArch.SelectedValue!,
                    (double)numericL1CodeHitrate.Value,
                    (int)numericL1CodeLatency.Value,
                    (double)numericl1DataHitrate.Value,
                    (int)numericL1DataLatency.Value,
                    (double)numericL2Hitrate.Value,
                    (int)numericL2Latency.Value,
                    (int)numericSystemMemory.Value
                );
                Form2 form2;
                switch ((SearchType)comboBoxSearchAlgorithm.SelectedIndex)
                {
                    case SearchType.WEIGHT_PSO:
                        var searchConfigWeight = new SearchConfigSMPSO(
                            (int)numericPopulationSize.Value,
                            (int)numericLeadersArchiveSize.Value,
                            (int)numericMaxGenerations.Value,
                            (double)numericMutationRate.Value,
                            (int)numericMaxFrequency.Value,
                            envConf,
                            DominationConfig.GetWeightedSumDominationConfig((double)numericWeightCPI.Value, (double)numericWeightEnergy.Value)
                        );
                        form2 = new Form2(searchConfigWeight, exePath, gtkPath, tracePaths);
                        form2.FormClosing += (s, args) => { this.Close(); Application.Exit(); } ;
                        form2.Show();
                        break;
                    case SearchType.LEXICOGRAPHIC_PSO:
                        var searchConfigLexicographic = new SearchConfigSMPSO(
                            (int)numericPopulationSize.Value,
                            (int)numericLeadersArchiveSize.Value,
                            (int)numericMaxGenerations.Value,
                            (double)numericMutationRate.Value,
                            (int)numericMaxFrequency.Value,
                            envConf,
                            DominationConfig.GetLexicographicDominationConfig((DominationConfig.PrefferedObjective)comboPrefferedObjective.SelectedValue!)
                        );
                        form2 = new Form2(searchConfigLexicographic, exePath, gtkPath, tracePaths);
                        form2.FormClosing += (s, args) => { this.Close(); Application.Exit(); };
                        form2.Show();
                        break;
                    case SearchType.VEGA:
                        var searchConfigVEGA = new SearchConfigVEGA(
                            (int)numericMaxGenerations.Value,
                            (int)numericPopulationSize.Value,
                            (double)numericMutationRate.Value,
                            (int)numericMaxFrequency.Value,
                            envConf
                        );
                        form2 = new Form2(searchConfigVEGA, exePath, gtkPath, tracePaths);
                        form2.FormClosing += (s, args) => { this.Close(); Application.Exit(); };
                        form2.Show();
                        break;
                    case SearchType.SMPSO:
                        var searchConfigSMPSO = new SearchConfigSMPSO(
                            (int)numericPopulationSize.Value,
                            (int)numericLeadersArchiveSize.Value,
                            (int)numericMaxGenerations.Value,
                            (double)numericMutationRate.Value,
                            (int)numericMaxFrequency.Value,
                            envConf,
                            DominationConfig.GetSMPSODominationConfig()
                        );
                        form2 = new Form2(searchConfigSMPSO, exePath, gtkPath, tracePaths);
                        form2.FormClosing += (s, args) => { this.Close(); Application.Exit(); };
                        form2.Show();
                        break;
                
                }

                this.Hide();
            }
        }
    }
}
