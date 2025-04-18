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

        private SpinButton _maxSecondsPerSimulationSpinButton;
        private SpinButton _maxParallelProcessesSpinButton;
        private SpinButton _maxInstructionsSpinButton;         // Note: SpinButton uses double, cast needed for ulong
        private SpinButton _fastForwardInstructionsSpinButton; // Note: SpinButton uses double, cast needed for ulong
        private SpinButton _fetchSpeedSpinButton;
        private SpinButton _fetchRenameDelaySpinButton;
        private CheckButton _issueWrongPathCheckButton;
        private SpinButton _issueExecDelaySpinButton;
        private SpinButton _cacheDl1LatencySpinButton;
        private SpinButton _cacheDl2LatencySpinButton;
        private SpinButton _cacheIl1LatencySpinButton;
        private SpinButton _cacheIl2LatencySpinButton;
        private CheckButton _cacheFlushOnSyscallCheckButton;
        private CheckButton _cacheInstructionCompressCheckButton;
        private SpinButton _memLatencyFirstChunkSpinButton;
        private SpinButton _memLatencyInterChunkSpinButton;
        private SpinButton _tlbMissLatencySpinButton;
        private SpinButton _renameDispatchDelaySpinButton;

        private void AttachEnvironmentConfig(Grid grid)
        {
            int currentRow = 8; // Keep track of the current row index

            // Helper function to add a standard Label + Widget row
            void AddRow(string labelText, Widget inputWidget)
            {
                var label = new Label(labelText + ":")
                {
                    Halign = Align.End, // Right-align label in the first column
                    Valign = Align.Center
                };
                inputWidget.Valign = Align.Center;
                inputWidget.Hexpand = true; // Allow widget to expand horizontally

                grid.Attach(label,       0, currentRow, 1, 1); // Column 0, current row
                grid.Attach(inputWidget, 1, currentRow, 1, 1); // Column 1, current row
                currentRow++;
            }

            // Helper function specifically for MemLatencyConfig
            void AddMemLatencyRow(string labelText, SpinButton firstChunkSpin, SpinButton interChunkSpin)
            {
                var label = new Label(labelText + ":")
                {
                    Halign = Align.End, // Right-align label in the first column
                    Valign = Align.Center
                };

                // Use a Box to group the two spin buttons horizontally
                var valueBox = new Box(Orientation.Horizontal, 6) { Valign = Align.Center, Hexpand = true };

                valueBox.PackStart(new Label("First:") { Valign = Align.Center } , false, false, 0);
                valueBox.PackStart(firstChunkSpin, true, true, 0);
                valueBox.PackStart(new Label("Inter:") { Valign = Align.Center, MarginStart = 10 }, false, false, 0);
                valueBox.PackStart(interChunkSpin, true, true, 0);

                grid.Attach(label,    0, currentRow, 1, 1);
                grid.Attach(valueBox, 1, currentRow, 1, 1);
                currentRow++;
            }

            // --- Populate Grid ---

            // MaxSecondsPerSimulation (int)
            // Assumes _maxSecondsPerSimulationSpinButton is a declared field
            _maxSecondsPerSimulationSpinButton = new SpinButton(new Adjustment(
                value: 60,        // Default value
                lower: 1,         // Minimum value
                upper: 86400,     // Maximum value (e.g., 1 day)
                step_increment: 1,      // Step increment
                page_increment: 10,     // Page increment
                page_size: 0       // Page size (unused)
            ), 1.0, 0); // Climb rate, digits to display (0 for integer)
            AddRow("Max Seconds Per Simulation", _maxSecondsPerSimulationSpinButton);

            // MaxParallelProcesses (int)
            // Assumes _maxParallelProcessesSpinButton is a declared field
            int defaultParallel = Environment.ProcessorCount;
            _maxParallelProcessesSpinButton = new SpinButton(new Adjustment(defaultParallel, 1, defaultParallel * 32, 1, 2, 0), 1.0, 0); // Increased upper range example
            AddRow("Max Parallel Processes", _maxParallelProcessesSpinButton);

            // MaxInstructions (ulong) -> Use SpinButton (double), needs care with large values/casting
            // Assumes _maxInstructionsSpinButton is a declared field
            _maxInstructionsSpinButton = new SpinButton(new Adjustment(2000000, 1000, double.MaxValue, 1000, 1000000, 0), 1.0, 0);
            AddRow("Max Instructions (0 for none)", _maxInstructionsSpinButton);

            // FastForwardInstructions (ulong) -> Use SpinButton (double)
            // Assumes _fastForwardInstructionsSpinButton is a declared field
            _fastForwardInstructionsSpinButton = new SpinButton(new Adjustment(1000000, 1000, double.MaxValue, 1000, 1000000, 0), 1.0, 0);
            AddRow("Fast Forward Instructions", _fastForwardInstructionsSpinButton);

            // FetchSpeed (int?) -> Using int, default 1
            // Assumes _fetchSpeedSpinButton is a declared field
            _fetchSpeedSpinButton = new SpinButton(new Adjustment(8, 1, 16, 1, 1, 0), 1.0, 0);
            AddRow("Fetch Speed", _fetchSpeedSpinButton);

            // FetchRenameDelay (int?) -> Using int, default 1
            // Assumes _fetchRenameDelaySpinButton is a declared field
            _fetchRenameDelaySpinButton = new SpinButton(new Adjustment(8, 1, 32, 1, 1, 0), 1.0, 0);
            AddRow("Fetch Rename Delay (cycles)", _fetchRenameDelaySpinButton);

            // IssueWrongPath (bool?) -> Using bool, default true
            // Assumes _issueWrongPathCheckButton is a declared field
            _issueWrongPathCheckButton = new CheckButton("Issue down wrong paths") { Active = true, Hexpand = true };
            grid.Attach(new Label(""), 0, currentRow, 1, 1); // Placeholder label for alignment
            grid.Attach(_issueWrongPathCheckButton, 1, currentRow, 1, 1);
            currentRow++;

            // IssueExecDelay (int?) -> Using int, default 1
            // Assumes _issueExecDelaySpinButton is a declared field
            _issueExecDelaySpinButton = new SpinButton(new Adjustment(8, 1, 32, 1, 1, 0), 1.0, 0);
            AddRow("Issue Exec Delay (cycles)", _issueExecDelaySpinButton);

            // CacheDl1Latency (int?) -> Using int, default 1
            // Assumes _cacheDl1LatencySpinButton is a declared field
            _cacheDl1LatencySpinButton = new SpinButton(new Adjustment(8, 1, 100, 1, 5, 0), 1.0, 0);
            AddRow("Cache DL1 Latency (cycles)", _cacheDl1LatencySpinButton);

            // CacheDl2Latency (int?) -> Using int, default 10
            // Assumes _cacheDl2LatencySpinButton is a declared field
            _cacheDl2LatencySpinButton = new SpinButton(new Adjustment(16, 1, 200, 1, 10, 0), 1.0, 0);
            AddRow("Cache DL2 Latency (cycles)", _cacheDl2LatencySpinButton);

            // CacheIl1Latency (int?) -> Using int, default 1
            // Assumes _cacheIl1LatencySpinButton is a declared field
            _cacheIl1LatencySpinButton = new SpinButton(new Adjustment(8, 1, 100, 1, 5, 0), 1.0, 0);
            AddRow("Cache IL1 Latency (cycles)", _cacheIl1LatencySpinButton);

            // CacheIl2Latency (int?) -> Using int, default 10
            // Assumes _cacheIl2LatencySpinButton is a declared field
            _cacheIl2LatencySpinButton = new SpinButton(new Adjustment(16, 1, 200, 1, 10, 0), 1.0, 0);
            AddRow("Cache IL2 Latency (cycles)", _cacheIl2LatencySpinButton);

            // CacheFlushOnSyscall (bool?) -> Using bool, default false
            // Assumes _cacheFlushOnSyscallCheckButton is a declared field
            _cacheFlushOnSyscallCheckButton = new CheckButton("Flush caches on syscall") { Active = false, Hexpand = true };
            grid.Attach(new Label(""), 0, currentRow, 1, 1); // Placeholder label for alignment
            grid.Attach(_cacheFlushOnSyscallCheckButton, 1, currentRow, 1, 1);
            currentRow++;

            // CacheInstructionCompress (bool?) -> Using bool, default false
            // Assumes _cacheInstructionCompressCheckButton is a declared field
            _cacheInstructionCompressCheckButton = new CheckButton("Compress instruction addresses (64->32 bit)") { Active = false, Hexpand = true };
            grid.Attach(new Label(""), 0, currentRow, 1, 1); // Placeholder label for alignment
            grid.Attach(_cacheInstructionCompressCheckButton, 1, currentRow, 1, 1);
            currentRow++;

            // MemLatency (MemLatencyConfig?) -> Using two SpinButtons
            // Assumes _memLatencyFirstChunkSpinButton and _memLatencyInterChunkSpinButton are declared fields
            _memLatencyFirstChunkSpinButton = new SpinButton(new Adjustment(64, 1, 1000, 1, 10, 0), 1.0, 0);
            _memLatencyInterChunkSpinButton = new SpinButton(new Adjustment(32, 1, 200, 1, 5, 0), 1.0, 0);
            AddMemLatencyRow("Memory Latency (cycles)", _memLatencyFirstChunkSpinButton, _memLatencyInterChunkSpinButton);

            // TlbMissLatency (int?) -> Using int, default 30
            // Assumes _tlbMissLatencySpinButton is a declared field
            _tlbMissLatencySpinButton = new SpinButton(new Adjustment(32, 1, 1000, 1, 10, 0), 1.0, 0);
            AddRow("TLB Miss Latency (cycles)", _tlbMissLatencySpinButton);

            // RenameDispatchDelay (int?) -> Using int, default 1
            // Assumes _renameDispatchDelaySpinButton is a declared field
            _renameDispatchDelaySpinButton = new SpinButton(new Adjustment(2, 1, 32, 1, 1, 0), 1.0, 0);
            AddRow("Rename Dispatch Delay (cycles)", _renameDispatchDelaySpinButton);

            // The function now includes all properties listed in the EnvironmentConfig definition provided.
        }

        public SMPSOGuiGtk() : base("SMPSO Simulation")
        {
            // Window setup
            SetDefaultSize(1000, 700);
            SetPosition(WindowPosition.Center);
            DeleteEvent += OnDeleteEvent; // Make sure OnDeleteEvent handler exists

            // Create main vertical box
            // Note: VBox is obsolete in Gtk3+, using Box with Vertical orientation instead
            var mainBox = new Box(Orientation.Vertical, 10); // Spacing between elements in mainBox
            // Add some padding around the mainBox content if desired
            mainBox.Margin = 10; // Adds margin on all sides
            Add(mainBox); // Add mainBox directly to the Window

            // --- Create the Grid that will hold your content ---
            var grid = new Grid();
            grid.ColumnSpacing = 10;
            grid.RowSpacing = 10;
            // grid.Hexpand = true; // Grid should expand horizontally within its container


            // Row 1 (Row index 0)
            buttonStartSearch = new Button("Start Search");
            buttonStartSearch.Clicked += OnStartSearchClicked;
            grid.Attach(buttonStartSearch, 0, 0, 1, 1);

            // Row 2 (Row index 1)
            numericPopulationSize = CreateSpinButton(5, 1000, 1);
            numericPopulationSize.Value = 100;
            buttonFilePickerExe = new Button("Pick sim-outorder");
            buttonFilePickerExe.Clicked += OnFilePickerExeClicked;
            labelFilePathExe = new Label("No file selected");
            grid.Attach(new Label("Population Size:"), 0, 1, 1, 1);
            grid.Attach(numericPopulationSize, 1, 1, 1, 1);
            grid.Attach(buttonFilePickerExe, 0, 2, 1, 1);
            grid.Attach(labelFilePathExe, 1, 2, 1, 1);

            // Row 4 (Row index 3)
            numericMaxGenerations = CreateSpinButton(0, 10000, 1);
            numericMaxGenerations.Value = 10;
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
            grid.Attach(buttonFilePickerTraces, 0, 5, 1, 1);
            grid.Attach(textTraces, 1, 5, 2, 1);

            numericLeadersArchiveSize = CreateSpinButton(0, 10000, 1);
            numericLeadersArchiveSize.Value = 50;
            grid.Attach(new Label("Max Leaders Archive Size:"), 0, 6, 1, 1);
            grid.Attach(numericLeadersArchiveSize, 1, 6, 1, 1);
 
            // Empty row (Row index 5) - full-width spacer
            Label emptyRow = new Label(" ");
            grid.Attach(emptyRow, 0, 7, 6, 1);

            AttachEnvironmentConfig(grid);


            ScrolledWindow scrolledWindow = new ScrolledWindow();
            scrolledWindow.SetPolicy(PolicyType.Never, PolicyType.Automatic);
            scrolledWindow.AddWithViewport(grid);
            mainBox.PackStart(scrolledWindow, true, true, 0);
            // Show all widgets
            ShowAll();

        }


        private void OnFilePickerExeClicked(object sender, EventArgs e)
        {
            using (var fileChooser = new FileChooserDialog("Select sim-outorder", this,
                FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Select", ResponseType.Accept))
            {
                fileChooser.Filter = new FileFilter();
                fileChooser.Filter.AddPattern("*sim-outorder");

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
            using (var fileChooser = new FileChooserDialog("Select Benchmark Files", this,
                FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Select", ResponseType.Accept))
            {
                fileChooser.Filter = new FileFilter();
                fileChooser.Filter.AddPattern("*.arg");
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
                     "Must provide sim-outorder path and at least one trace file");
                md.Run();
                md.Destroy();
                return;
            }

            // Create environment configuration
            var envConf = new EnvironmentConfig(
                maxInstructions: (ulong)_maxInstructionsSpinButton.ValueAsInt,
                maxParallelProcesses: _maxParallelProcessesSpinButton.ValueAsInt,
                maxSecondsPerSimulation: _maxSecondsPerSimulationSpinButton.ValueAsInt,
                fastForwardInstructions: (ulong)_fastForwardInstructionsSpinButton.ValueAsInt,
                fetchRenameDelay: _fetchRenameDelaySpinButton.ValueAsInt,
                fetchSpeed: _fetchSpeedSpinButton.ValueAsInt,
                issueExecDelay: _issueExecDelaySpinButton.ValueAsInt,
                issueWrongPath: _issueWrongPathCheckButton.Active,
                cacheDl1Latency: _cacheDl1LatencySpinButton.ValueAsInt,
                cacheDl2Latency: _cacheDl2LatencySpinButton.ValueAsInt,
                cacheFlushOnSyscall: _cacheFlushOnSyscallCheckButton.Active,
                cacheIl1Latency: _cacheIl1LatencySpinButton.ValueAsInt,
                cacheIl2Latency: _cacheIl2LatencySpinButton.ValueAsInt,
                cacheInstructionCompress: _cacheInstructionCompressCheckButton.Active,
                tlbMissLatency: _tlbMissLatencySpinButton.ValueAsInt,
                memLatency: new(_memLatencyFirstChunkSpinButton.ValueAsInt, _memLatencyInterChunkSpinButton.ValueAsInt),
                renameDispatchDelay: _renameDispatchDelaySpinButton.ValueAsInt
           );
            Window resultsWindow=null;

            var searchConfigSMPSO = new SearchConfigSMPSO(
                         numericPopulationSize.ValueAsInt,
                         numericLeadersArchiveSize.ValueAsInt,
                         numericMaxGenerations.ValueAsInt,
                         numericMutationRate.Value,
                         envConf
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