using System;
using System.Collections.Generic;
using Gtk;
using SMPSOsimulation.dataStructures;

namespace LinuxVisual
{
    public class SMPSOGuiGtk : Window
    {
        // Combo boxes
        private ComboBox comboBoxSearchAlgorithm;
        private ComboBox combomemoryArch;
        private ComboBox comboPrefferedObjective;

        // Numeric entries
        private SpinButton numericPopulationSize;
        private SpinButton numericLeadersArchiveSize;
        private SpinButton numericMaxGenerations;
        private SpinButton numericMutationRate;
        private SpinButton numericMaxFrequency;
        private SpinButton numericWeightCPI;
        private SpinButton numericWeightEnergy;
        private SpinButton numericVdd;
        private SpinButton numericL1CodeHitrate;
        private SpinButton numericL1CodeLatency;
        private SpinButton numericl1DataHitrate;
        private SpinButton numericL1DataLatency;
        private SpinButton numericL2Hitrate;
        private SpinButton numericL2Latency;
        private SpinButton numericSystemMemory;

        // Buttons and file choosers
        private Button buttonStartSearch;
        private Button buttonFilePickerExe;
        private Button buttonFilePickerGTK;
        private Button buttonFilePickerTraces;

        // Labels
        private Label labelFilePathExe;
        private Label labelFilePickerGTK;
        private TextView textTraces;
        private Label emptySpace;

        private string? exePath = null;
        private string? gtkPath = null;
        private List<string>? tracePaths = null;

        private enum SearchType
        {
            WEIGHT_PSO,
            LEXICOGRAPHIC_PSO,
            VEGA,
            SMPSO
        }

        public SMPSOGuiGtk() : base("SMPSO Simulation")
        {
            // Window setup
            SetDefaultSize(1000, 700);
            SetPosition(WindowPosition.Center);
            DeleteEvent += OnDeleteEvent;

            // Create main vertical box
            var mainBox = new VBox(false, 10);
            Add(mainBox);

            // Create grid for layout
            var grid = new Grid();
            grid.ColumnSpacing = 10;
            grid.RowSpacing = 10;
            mainBox.PackStart(grid, true, true, 10);

            // Row 1 (Row index 0)
            buttonStartSearch = new Button("Start Search");
            grid.Attach(buttonStartSearch, 0, 0, 1, 1);

            // Row 2 (Row index 1)
            numericPopulationSize = CreateSpinButton(0, 1000, 1);
            numericPopulationSize.Value = 1;
            buttonFilePickerExe = new Button("Pick Exe");
            buttonFilePickerExe.Clicked += OnFilePickerExeClicked;
            labelFilePathExe = new Label("No file selected");
            grid.Attach(new Label("Population Size:"), 0, 1, 1, 1);
            grid.Attach(numericPopulationSize, 1, 1, 1, 1);
            grid.Attach(buttonFilePickerExe, 2, 1, 1, 1);
            grid.Attach(labelFilePathExe, 3, 1, 1, 1);

            // Row 3 (Row index 2)
            numericLeadersArchiveSize = CreateSpinButton(0, 1000, 1);
            numericLeadersArchiveSize.Value = 1;
            buttonFilePickerGTK = new Button("Pick GTK Lib");
            buttonFilePickerGTK.Clicked += OnFilePickerGTKClicked;
            labelFilePickerGTK = new Label("No folder selected");
            grid.Attach(new Label("Leaders Archive Size:"), 0, 2, 1, 1);
            grid.Attach(numericLeadersArchiveSize, 1, 2, 1, 1);
            grid.Attach(buttonFilePickerGTK, 2, 2, 1, 1);
            grid.Attach(labelFilePickerGTK, 3, 2, 1, 1);

            // Row 4 (Row index 3)
            numericMaxGenerations = CreateSpinButton(0, 10000, 1);
            numericMaxGenerations.Value = 1;
            grid.Attach(new Label("Max Generations:"), 0, 3, 1, 1);
            grid.Attach(numericMaxGenerations, 1, 3, 1, 1);
            grid.Attach(comboPrefferedObjective, 3, 3, 1, 1);

            // Row 5 (Row index 4)
            numericMutationRate = CreateSpinButton(0, 1, 0.01);
            numericMutationRate.Value = 0.01;
            buttonFilePickerTraces = new Button("Pick Traces");
            buttonFilePickerTraces.Clicked += OnFilePickerTracesClicked;
            textTraces = new TextView();
            textTraces.SetSizeRequest(200, 50);
            grid.Attach(new Label("Mutation Rate:"), 0, 4, 1, 1);
            grid.Attach(numericMutationRate, 1, 4, 1, 1);
            grid.Attach(buttonFilePickerTraces, 2, 4, 1, 1);
            grid.Attach(textTraces, 3, 4, 2, 1);

            // Empty row (Row index 5) - full-width spacer
            Label emptyRow = new Label(" ");
            grid.Attach(emptyRow, 0, 5, 6, 1);

            // Row 6 (Row index 6)
            numericMaxFrequency = CreateSpinButton(0, 10000, 1);
            numericMaxFrequency.Value = 10000;
            numericL1CodeHitrate = CreateSpinButton(0, 1, 0.01);
            numericL1CodeHitrate.Value = 0.01;
            numericL1CodeHitrate.Sensitive = false;
            numericL2Hitrate = CreateSpinButton(0, 1, 0.01);
            numericL2Hitrate.Value = 0.01;
            numericL2Hitrate.Sensitive = false;
            grid.Attach(new Label("Max Frequency:"), 0, 6, 1, 1);
            grid.Attach(numericMaxFrequency, 1, 6, 1, 1);
            grid.Attach(new Label("L1 Code Hitrate:"), 2, 6, 1, 1);
            grid.Attach(numericL1CodeHitrate, 3, 6, 1, 1);
            grid.Attach(new Label("L2 Hitrate:"), 4, 6, 1, 1);
            grid.Attach(numericL2Hitrate, 5, 6, 1, 1);

            // Row 7 (Row index 7)
            numericVdd = CreateSpinButton(0, 10, 0.1);
            numericVdd.Value = 2.20;
            numericL1CodeLatency = CreateSpinButton(0, 1000, 1);
            numericL1CodeLatency.Value = 1;
            numericL1CodeLatency.Sensitive = false;
            numericL2Latency = CreateSpinButton(0, 1000, 1);
            numericL2Latency.Value = 1;
            numericL2Latency.Sensitive = false;
            grid.Attach(new Label("Vdd:"), 0, 7, 1, 1);
            grid.Attach(numericVdd, 1, 7, 1, 1);
            grid.Attach(new Label("L1 Code Latency:"), 2, 7, 1, 1);
            grid.Attach(numericL1CodeLatency, 3, 7, 1, 1);
            grid.Attach(new Label("L2 Latency:"), 4, 7, 1, 1);
            grid.Attach(numericL2Latency, 5, 7, 1, 1);

            // Row 8 (Row index 8)
            combomemoryArch = CreateComboBox(Enum.GetNames(typeof(MemoryArchEnum)));
            numericl1DataHitrate = CreateSpinButton(0, 1, 0.01);
            numericl1DataHitrate.Value = 0.01;
            numericl1DataHitrate.Sensitive = false;
            grid.Attach(new Label("Memory Architecture:"), 0, 8, 1, 1);
            grid.Attach(combomemoryArch, 1, 8, 1, 1);
            grid.Attach(new Label("L1 Data Hitrate:"), 2, 8, 1, 1);
            grid.Attach(numericl1DataHitrate, 3, 8, 1, 1);

            // Row 9 (Row index 9)
            numericSystemMemory = CreateSpinButton(0, 100000, 1);
            numericSystemMemory.Value = 1;
            numericL1DataLatency = CreateSpinButton(0, 1000, 1);
            numericL1DataLatency.Value = 1;
            numericL1DataLatency.Sensitive = false;
            grid.Attach(new Label("System Memory:"), 0, 9, 1, 1);
            grid.Attach(numericSystemMemory, 1, 9, 1, 1);
            grid.Attach(new Label("L1 Data Latency:"), 2, 9, 1, 1);
            grid.Attach(numericL1DataLatency, 3, 9, 1, 1);

            // Event handlers for combo boxes
            combomemoryArch.Changed += OnMemoryArchChanged;

            // Show all widgets
            ShowAll();

        }

        private void OnSearchAlgorithmChanged(object sender, EventArgs e)
        {
            var selectedAlgorithm = (SearchType)comboBoxSearchAlgorithm.Active;
            switch (selectedAlgorithm)
            {
                case SearchType.WEIGHT_PSO:
                    // Enable/disable appropriate controls for Weight PSO
                    break;
                case SearchType.LEXICOGRAPHIC_PSO:
                    // Enable/disable appropriate controls for Lexicographic PSO
                    break;
                case SearchType.SMPSO:
                    // Enable/disable appropriate controls for SMPSO
                    break;
                case SearchType.VEGA:
                    // Enable/disable appropriate controls for VEGA
                    break;
            }
        }

        private void OnMemoryArchChanged(object sender, EventArgs e)
        {
            var selectedArch = (MemoryArchEnum)combomemoryArch.Active;
            switch (selectedArch)
            {
                case MemoryArchEnum.system:
                    // Disable L1 and L2 cache controls
                    break;
                case MemoryArchEnum.l1:
                    // Enable L1 cache controls, disable L2
                    break;
                case MemoryArchEnum.l2:
                    // Enable L1 and L2 cache controls
                    break;
            }
        }

        private void OnFilePickerExeClicked(object sender, EventArgs e)
        {
            using (var fileChooser = new FileChooserDialog("Select Executable", this,
                FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Select", ResponseType.Accept))
            {
                fileChooser.Filter = new FileFilter();
                fileChooser.Filter.AddPattern("*.exe");

                if (fileChooser.Run() == (int)ResponseType.Accept)
                {
                    exePath = fileChooser.Filename;
                    labelFilePathExe.Text = exePath;
                }
                fileChooser.Destroy();
            }
        }

        private void OnFilePickerGTKClicked(object sender, EventArgs e)
        {
            using (var folderChooser = new FileChooserDialog("Select GTK/lib Folder", this,
                FileChooserAction.SelectFolder, "Cancel", ResponseType.Cancel, "Select", ResponseType.Accept))
            {
                if (folderChooser.Run() == (int)ResponseType.Accept)
                {
                    gtkPath = folderChooser.Filename;
                    labelFilePickerGTK.Text = gtkPath;
                }
                folderChooser.Destroy();
            }
        }

        private void OnFilePickerTracesClicked(object sender, EventArgs e)
        {
            using (var fileChooser = new FileChooserDialog("Select Trace Files", this,
                FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Select", ResponseType.Accept))
            {
                fileChooser.Filter = new FileFilter();
                fileChooser.Filter.AddPattern("*.tra");
                fileChooser.SelectMultiple = true;

                if (fileChooser.Run() == (int)ResponseType.Accept)
                {
                    tracePaths = new List<string>(fileChooser.Filenames);
                    textTraces.Buffer.Text = string.Join(Environment.NewLine, tracePaths);
                }
                fileChooser.Destroy();
            }
        }

        private void OnStartSearchClicked(object sender, EventArgs e)
        {
            if (exePath == null || gtkPath == null || tracePaths == null)
            {
                var dialog = new MessageDialog(this, DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok,
                    "Must provide all of psatsim_con.exe path, GTK/lib path, and trace file path");
                dialog.Run();
                dialog.Destroy();
                return;
            }

            // Create environment configuration
            var envConf = new EnvironmentConfig(
                numericVdd.Value,
                (MemoryArchEnum)combomemoryArch.Active,
                numericL1CodeHitrate.Value,
                (int)numericL1CodeLatency.Value,
                numericl1DataHitrate.Value,
                (int)numericL1DataLatency.Value,
                numericL2Hitrate.Value,
                (int)numericL2Latency.Value,
                (int)numericSystemMemory.Value
            );

            // TODO: Implement search configuration and next form/simulation logic
        }

        private void OnDeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
            a.RetVal = true;
        }

        private ComboBox CreateComboBox(string[] items)
        {
            var combo = new ComboBox(items);
            combo.Active = 0;
            return combo;
        }

        private SpinButton CreateSpinButton(double min, double max, double step)
        {
            var spinButton = new SpinButton(min, max, step);
            spinButton.Numeric = true;
            return spinButton;
        }

     
    }
}