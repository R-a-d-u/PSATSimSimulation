using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using Gtk;
using SMPSOsimulation.dataStructures;
using LinuxVisual;

namespace LinuxVisual
{
    public class SMPSOGuiGtk : Window
    {
        // Numeric entries
        private SpinButton numericPopulationSize;
        private SpinButton numericLeadersArchiveSize;
        private SpinButton numericMaxGenerations;
        private SpinButton numericMutationRate;

        // Buttons and file choosers
        private Button buttonStartSearch;
        private Button buttonFilePickerExe;
        private Button buttonFilePickerTraces;

        // Labels
        private Label labelFilePathExe;
        private TextView textTraces;

        private string? exePath = null;
        private List<string>? tracePaths = null;

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
            buttonStartSearch.Clicked += OnStartSearchClicked;
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

            // Row 4 (Row index 3)
            numericMaxGenerations = CreateSpinButton(0, 10000, 1);
            numericMaxGenerations.Value = 1;
            grid.Attach(new Label("Max Generations:"), 0, 3, 1, 1);
            grid.Attach(numericMaxGenerations, 1, 3, 1, 1);

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

            // Show all widgets
            ShowAll();

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
            if (exePath == null || tracePaths == null)
            {
                MessageDialog md = new MessageDialog(this,
                     DialogFlags.Modal,
                     MessageType.Error,
                     ButtonsType.Ok,
                     "Must provide all of psatsim_con.exe path, GTK/lib path, and at least one trace file");
                md.Run();
                md.Destroy();
                return;
            }

            // Create environment configuration
            var envConf = new EnvironmentConfig(
           );
            Window resultsWindow=null;

            var searchConfigSMPSO = new SearchConfigSMPSO(
                         (int)numericPopulationSize.Value,
                         (int)numericLeadersArchiveSize.Value,
                         (int)numericMaxGenerations.Value,
                         numericMutationRate.Value
                     );
            resultsWindow = new ResultsWindow(searchConfigSMPSO, exePath, tracePaths);

            if (resultsWindow != null)
            {
                resultsWindow.DeleteEvent += (o, args) => Application.Quit();
                resultsWindow.ShowAll();
                this.Hide();
            }

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