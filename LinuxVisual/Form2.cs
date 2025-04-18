using SMPSOsimulation;
using SMPSOsimulation.dataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using System.IO;
using ScottPlot;

namespace LinuxVisual
{
    public class ResultsWindow : Window
    {
        private Task algorithmTask;
        private CancellationTokenSource cts;
        private List<List<(CPUConfig, double[])>> history;

        // GTK Widgets
        private ListBox listBoxGeneration;
        private ListBox listBoxLeaders;
        private TextView richTextBox1;
        private Button button1;
        private ScrolledWindow scrolledWindow1;
        private ScrolledWindow scrolledWindow2;
        private ScrolledWindow scrolledWindow3;
        private HBox hbox;
        private VBox vboxLeft;
        private VBox vboxRight;

        // Keep track of the plot window to potentially close/reuse it
        private Window plotWindow = null;

        // Constructor SMPSO
        public ResultsWindow(SearchConfigSMPSO config, string exePath, List<string> tracePaths)
            : base("SMPSO Simulation Results")
        {
            InitializeComponents();
            history = new List<List<(CPUConfig, double[])>>();
            var runner = new SMPSOOrchestrator();
            runner.GenerationChanged += OnGenerationChanged;

            // Create a cancellation token source
            cts = new CancellationTokenSource();

            // Start the search as a Task
            algorithmTask = Task.Run(() => runner.StartSearch(config, exePath, tracePaths), cts.Token);

            listBoxGeneration.RowSelected += ChangeGeneration;
            listBoxLeaders.RowSelected += ChangeConfig;
            button1.Clicked += OnButton1Clicked;

            DeleteEvent += OnWindowClosed;
        }

        // The InitializeComponents method remains unchanged
        private void InitializeComponents()
        {
            // Existing implementation...
            SetDefaultSize(800, 600);

            // Create containers
            hbox = new HBox(false, 5);
            vboxLeft = new VBox(false, 5);
            vboxRight = new VBox(false, 5);

            // Create widgets
            listBoxGeneration = new ListBox();
            listBoxLeaders = new ListBox();
            richTextBox1 = new TextView();
            button1 = new Button("Export Last Gen to CSV");

            // Configure text view
            richTextBox1.Editable = false;
            richTextBox1.WrapMode = WrapMode.Word;

            // Create scrolled windows
            scrolledWindow1 = new ScrolledWindow();
            scrolledWindow2 = new ScrolledWindow();
            scrolledWindow3 = new ScrolledWindow();

            // Add widgets to scrolled windows
            scrolledWindow1.Add(listBoxGeneration);
            scrolledWindow2.Add(listBoxLeaders);
            scrolledWindow3.Add(richTextBox1);

            // Add widgets to left vbox
            vboxLeft.PackStart(new Gtk.Label("Generations"), false, false, 0);
            vboxLeft.PackStart(scrolledWindow1, true, true, 0);
            vboxLeft.PackStart(new Gtk.Label("Leaders (CPI / Energy)"), false, false, 0);
            vboxLeft.PackStart(scrolledWindow2, true, true, 0);
            vboxLeft.PackStart(button1, false, false, 5);

            // Add widgets to right vbox
            vboxRight.PackStart(new Gtk.Label("Selected Configuration Details"), false, false, 0);
            vboxRight.PackStart(scrolledWindow3, true, true, 0);

            // Add vboxes to hbox
            hbox.PackStart(vboxLeft, false, true, 0);
            hbox.PackStart(vboxRight, true, true, 0);

            Add(hbox);
            ShowAll();
        }

        // The existing event handler methods remain largely unchanged
        // ChangeGeneration, ChangeConfig methods remain the same...

        private readonly object plotLock = new object();
        private bool isPlotting = false;

        private void ShowParetoPlot(int generationIndex)
        {
            // Use a lock to prevent multiple simultaneous plot operations
            lock (plotLock)
            {
                if (isPlotting)
                {
                    Console.WriteLine("Plot operation already in progress. Please wait.");
                    return;
                }

                isPlotting = true;
            }

            try
            {
                // Close previous plot window if it exists
                Application.Invoke(delegate {
                    if (plotWindow != null)
                    {
                        try
                        {
                            var windowToDestroy = plotWindow;
                            plotWindow = null;
                            windowToDestroy.Destroy();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error closing previous plot window: {ex.Message}");
                            plotWindow = null;
                        }
                    }
                });

                if (generationIndex < 0 || generationIndex >= history.Count)
                {
                    Console.WriteLine("Cannot plot: Invalid generation index.");
                    lock (plotLock) { isPlotting = false; }
                    return;
                }

                var leaders = history[generationIndex];

                if (leaders == null || !leaders.Any())
                {
                    Console.WriteLine($"No leaders to plot for Generation {generationIndex + 1}.");
                    lock (plotLock) { isPlotting = false; }
                    return;
                }

                // Extract data for plotting
                double[] cpiValues = leaders.Select(l => l.Item2[0]).ToArray();
                double[] energyValues = leaders.Select(l => l.Item2[1]).ToArray();

                // Create the plot in a separate task
                Task.Run(() => {
                    try
                    {
                        var plot = new ScottPlot.Plot();
                        plot.Add.Scatter(cpiValues, energyValues);
                        plot.Title($"Pareto Front - Generation {generationIndex + 1}");
                        plot.XLabel("CPI");
                        plot.YLabel("Putere (Watt)");
                        plot.ShowLegend();

                        // Render the plot to a byte array
                        int plotWidth = 600;
                        int plotHeight = 400;
                        byte[] imageBytes = plot.GetImageBytes(plotWidth, plotHeight, ScottPlot.ImageFormat.Png);

                        // Update UI on the main thread
                        Application.Invoke(delegate {
                            try
                            {
                                using (var stream = new MemoryStream(imageBytes))
                                {
                                    var pixbuf = new Gdk.Pixbuf(stream);

                                    // Create a new window for displaying the plot
                                    var newPlotWindow = new Window($"Pareto Plot - Generation {generationIndex + 1}");
                                    newPlotWindow.SetDefaultSize(620, 420);
                                    newPlotWindow.WindowPosition = WindowPosition.Center;

                                    var imageWidget = new Gtk.Image(pixbuf);
                                    newPlotWindow.Add(imageWidget);

                                    // Handle window closing properly
                                    newPlotWindow.DeleteEvent += (s, a) => {
                                        try
                                        {
                                            lock (plotLock)
                                            {
                                                if (plotWindow == (Window)s)
                                                    plotWindow = null;
                                            }
                                            ((Window)s).Destroy();
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"Error in plot window DeleteEvent: {ex.Message}");
                                        }
                                        a.RetVal = true;
                                    };

                                    newPlotWindow.ShowAll();

                                    // Update reference with lock
                                    lock (plotLock)
                                    {
                                        if (plotWindow != null)
                                        {
                                            try
                                            {
                                                plotWindow.Destroy();
                                            }
                                            catch { /* Ignore if already closed */ }
                                        }
                                        plotWindow = newPlotWindow;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error displaying plot: {ex.Message}");
                            }
                            finally
                            {
                                lock (plotLock) { isPlotting = false; }
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error creating plot: {ex.Message}");
                        Application.Invoke(delegate {
                            lock (plotLock) { isPlotting = false; }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ShowParetoPlot: {ex.Message}");
                lock (plotLock) { isPlotting = false; }
            }
        }

        private void OnWindowClosed(object sender, DeleteEventArgs e)
        {
            // Cancel the running task if it's still active
            if (cts != null && !cts.IsCancellationRequested)
            {
                try
                {
                    cts.Cancel();
                    // Note: Your orchestrators must check for cancellation
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error cancelling task: {ex.Message}");
                }
            }

            // Close any open plot window
            if (plotWindow != null)
            {
                plotWindow.Destroy();
                plotWindow = null;
            }

            Application.Quit(); // Quit the GTK application loop
        }

        // Other methods remain unchanged
        private void OnGenerationChanged(object? sender, List<(CPUConfig, double[])> leaders)
        {
            Application.Invoke(delegate
            {
                // Sort leaders by the first objective (e.g., CPI) before adding to history
                history.Add(leaders.OrderBy(leader => leader.Item2[0]).ToList());

                int generationNumber = history.Count; // Generation number (1-based)
                var label = new Gtk.Label($"Generation {generationNumber}");
                label.Xalign = 0;
                listBoxGeneration.Add(label);
                listBoxGeneration.ShowAll();
            });
        }

        private void ChangeGeneration(object sender, RowSelectedArgs e)
        {
            Application.Invoke(delegate
            {
                if (listBoxGeneration.SelectedRow != null)
                {
                    int generationIndex = listBoxGeneration.SelectedRow.Index;

                    // Clear current leaders
                    var children = listBoxLeaders.Children.ToList();
                    foreach (var child in children)
                    {
                        listBoxLeaders.Remove(child);
                    }

                    // Add new leaders
                    if (generationIndex >= 0 && generationIndex < history.Count)
                    {
                        var currentLeaders = history[generationIndex];
                        foreach (var leader in currentLeaders)
                        {
                            // Format numbers for better readability
                            var label = new Gtk.Label($"{leader.Item2[0]:F4}    {leader.Item2[1]:F4}");
                            label.Xalign = 0; // Align text left
                            listBoxLeaders.Add(label);
                        }
                        listBoxLeaders.ShowAll();

                        // Show Pareto Plot
                        ShowParetoPlot(generationIndex);
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Invalid generation index selected: {generationIndex}");
                        if (plotWindow != null)
                        {
                            plotWindow.Destroy();
                            plotWindow = null;
                        }
                    }
                }
                else
                {
                    // No row selected, clear leaders and close plot
                    var children = listBoxLeaders.Children.ToList();
                    foreach (var child in children)
                    {
                        listBoxLeaders.Remove(child);
                    }
                    if (plotWindow != null)
                    {
                        plotWindow.Destroy();
                        plotWindow = null;
                    }
                }
            });
        }

        private void ChangeConfig(object sender, RowSelectedArgs e)
        {
            Application.Invoke(delegate
            {
                if (listBoxGeneration.SelectedRow != null && listBoxLeaders.SelectedRow != null)
                {
                    int generationIndex = listBoxGeneration.SelectedRow.Index;
                    int configIndex = listBoxLeaders.SelectedRow.Index;

                    // Add bounds checking for safety
                    if (generationIndex >= 0 && generationIndex < history.Count &&
                        configIndex >= 0 && configIndex < history[generationIndex].Count)
                    {
                        richTextBox1.Buffer.Text = history[generationIndex][configIndex].Item1.ToString();
                    }
                    else
                    {
                        richTextBox1.Buffer.Text = "Invalid selection.";
                    }
                }
            });
        }

        private void OnButton1Clicked(object sender, EventArgs e)
        {
            // Export the *last* generation's leaders
            if (history.Count > 0)
            {
                WriteHistoryTables(history[history.Count - 1]);
                // Optional: Show a confirmation dialog
                MessageDialog md = new MessageDialog(this,
                    DialogFlags.DestroyWithParent | DialogFlags.Modal,
                    MessageType.Info,
                    ButtonsType.Ok,
                    $"Exported data for Generation {history.Count} to Table_CPI_Energy.csv and Table_CPUConfig.csv");
                md.Run();
                md.Destroy();
            }
            else
            {
                MessageDialog md = new MessageDialog(this,
                   DialogFlags.DestroyWithParent | DialogFlags.Modal,
                   MessageType.Warning,
                   ButtonsType.Ok,
                   "No history available to export.");
                md.Run();
                md.Destroy();
            }
        }

        // WriteHistoryTables method remains unchanged
        public static void WriteHistoryTables(List<(CPUConfig config, double[] metrics)> history)
        {
            // Existing implementation...
            if (history == null || !history.Any())
            {
                Console.WriteLine("History is empty. No tables will be generated.");
                return;
            }

            string file1 = "Table_CPI_Energy.csv";
            string file2 = "Table_CPUConfig.csv";

            try
            {
                // First table: ID, CPI, Energy
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(file1))
                {
                    writer.WriteLine("ID,CPI,Energy");
                    for (int i = 0; i < history.Count; i++)
                    {
                        var (config, metrics) = history[i];
                        writer.WriteLine($"{i},{metrics[0].ToString(System.Globalization.CultureInfo.InvariantCulture)},{metrics[1].ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                    }
                }

                // Second table: ID and CPUConfig fields
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(file2))
                {
                    writer.WriteLine("ID,Superscalar,Rename,Reorder,RsbArchitecture,SeparateDispatch,Iadd,Imult,Idiv,Fpadd,Fpmult,Fpdiv,Fpsqrt,Branch,Load,Store,Freq");
                    for (int i = 0; i < history.Count; i++)
                    {
                        var (config, metrics) = history[i];
                        writer.WriteLine($"{i}," +
                                         $"{config.DescribeConfiguration()}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting tables: {ex.Message}");
            }
        }
    }
}