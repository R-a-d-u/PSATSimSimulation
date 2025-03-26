using SMPSOsimulation;
using SMPSOsimulation.dataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Gtk;

namespace LinuxVisual
{
    public class ResultsWindow : Window
    {
        private Thread algorithmThread;
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

        public ResultsWindow(SearchConfigSMPSO config, string exePath, string gtkPath, List<string> tracePaths)
            : base("SMPSO Simulation Results")
        {
            InitializeComponents();
            history = new List<List<(CPUConfig, double[])>>();
            var runner = new SMPSOOrchestrator();
            runner.GenerationChanged += OnGenerationChanged;
            algorithmThread = new Thread(new ThreadStart(() => runner.StartSearch(config, exePath, gtkPath, tracePaths)));
            algorithmThread.Start();

            listBoxGeneration.RowSelected += ChangeGeneration;
            listBoxLeaders.RowSelected += ChangeConfig;
            button1.Clicked += OnButton1Clicked;

            DeleteEvent += OnWindowClosed;
        }

        public ResultsWindow(SearchConfigVEGA config, string exePath, string gtkPath, List<string> tracePaths)
            : base("VEGA Simulation Results")
        {
            InitializeComponents();
            history = new List<List<(CPUConfig, double[])>>();
            var runner = new VEGAOrchestrator();
            runner.GenerationChanged += OnGenerationChanged;
            algorithmThread = new Thread(new ThreadStart(() => runner.StartSearch(config, exePath, gtkPath, tracePaths)));
            algorithmThread.Start();

            listBoxGeneration.RowSelected += ChangeGeneration;
            listBoxLeaders.RowSelected += ChangeConfig;
            button1.Clicked += OnButton1Clicked;

            DeleteEvent += OnWindowClosed;
        }

        private void InitializeComponents()
        {
            SetDefaultSize(800, 600);

            // Create containers
            hbox = new HBox(false, 5);
            vboxLeft = new VBox(false, 5);
            vboxRight = new VBox(false, 5);

            // Create widgets
            listBoxGeneration = new ListBox();
            listBoxLeaders = new ListBox();
            richTextBox1 = new TextView();
            button1 = new Button("Export to CSV");

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
            vboxLeft.PackStart(new Label("Generations"), false, false, 0);
            vboxLeft.PackStart(scrolledWindow1, true, true, 0);
            vboxLeft.PackStart(new Label("Leaders"), false, false, 0);
            vboxLeft.PackStart(scrolledWindow2, true, true, 0);
            vboxLeft.PackStart(button1, false, false, 5);

            // Add widgets to right vbox
            vboxRight.PackStart(new Label("Configuration Details"), false, false, 0);
            vboxRight.PackStart(scrolledWindow3, true, true, 0);

            // Add vboxes to hbox
            hbox.PackStart(vboxLeft, false, true, 0);
            hbox.PackStart(vboxRight, true, true, 0);

            Add(hbox);
            ShowAll();
        }

        private void ChangeGeneration(object sender, RowSelectedArgs e)
        {
            Application.Invoke(delegate
            {
                if (listBoxGeneration.SelectedRow != null)
                {
                    int generationIndex = listBoxGeneration.SelectedRow.Index;

                    // Clear current leaders
                    foreach (var child in listBoxLeaders.Children)
                    {
                        listBoxLeaders.Remove(child);
                    }

                    // Add new leaders
                    foreach (var leader in history[generationIndex])
                    {
                        var label = new Label($"{leader.Item2[0]}        {leader.Item2[1]}");
                        label.Xalign = 0;
                        listBoxLeaders.Add(label);
                    }

                    listBoxLeaders.ShowAll();
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

                    if (generationIndex != -1 && configIndex != -1)
                    {
                        richTextBox1.Buffer.Text = history[generationIndex][configIndex].Item1.ToString();
                    }
                }
            });
        }

        private void OnGenerationChanged(object sender, List<(CPUConfig, double[])> leaders)
        {
            Application.Invoke(delegate
            {
                history.Add(leaders.OrderBy(leader => leader.Item2[0]).ToList());

                var label = new Label($"Generation {history.Count}");
                label.Xalign = 0;
                listBoxGeneration.Add(label);
                listBoxGeneration.ShowAll();

                //richTextBox1.Buffer.Text = string.Empty;
            });
        }

        private void OnWindowClosed(object sender, DeleteEventArgs e)
        {
            Application.Quit();
        }

        private void OnButton1Clicked(object sender, EventArgs e)
        {
            if (history.Count > 0)
                WriteHistoryTables(history[history.Count - 1]);
        }

        public static void WriteHistoryTables(List<(CPUConfig config, double[] metrics)> history)
        {
            if (history == null || !history.Any())
            {
                Console.WriteLine("History is empty. No tables will be generated.");
                return;
            }

            // File names
            string file1 = "Table_CPI_Energy.csv";
            string file2 = "Table_CPUConfig.csv";

            // First table: ID, CPI, Energy
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(file1))
            {
                writer.WriteLine("ID,CPI,Energy");
                for (int i = 0; i < history.Count; i++)
                {
                    var (config, metrics) = history[i];
                    writer.WriteLine($"{i},{metrics[0]},{metrics[1]}");
                }
            }

            // Second table: ID and CPUConfig fields
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(file2))
            {
                // Write header
                writer.WriteLine("ID,Superscalar,Rename,Reorder,RsbArchitecture,SeparateDispatch,Iadd,Imult,Idiv,Fpadd,Fpmult,Fpdiv,Fpsqrt,Branch,Load,Store,Freq");
                for (int i = 0; i < history.Count; i++)
                {
                    var (config, metrics) = history[i];
                    writer.WriteLine($"{i}," +
                                     $"{config.Superscalar}," +
                                     $"{config.Rename}," +
                                     $"{config.Reorder}," +
                                     $"{config.RsbArchitecture}," +
                                     $"{config.SeparateDispatch}," +
                                     $"{config.Iadd}," +
                                     $"{config.Imult}," +
                                     $"{config.Idiv}," +
                                     $"{config.Fpadd}," +
                                     $"{config.Fpmult}," +
                                     $"{config.Fpdiv}," +
                                     $"{config.Fpsqrt}," +
                                     $"{config.Branch}," +
                                     $"{config.Load}," +
                                     $"{config.Store}," +
                                     $"{config.Freq}");
                }
            }

            Console.WriteLine($"Tables exported: {file1}, {file2}");
        }
    }
}