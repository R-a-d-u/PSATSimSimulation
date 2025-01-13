using SMPSOsimulation;
using SMPSOsimulation.dataStructures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SMPSOGui
{
    public partial class Form2 : Form
    {
        private Thread algorithmThread;
        private List<List<(CPUConfig, double[])>> history;
        public Form2(SearchConfigSMPSO config, string exePath, string gtkPath, string tracePath)
        {
            InitializeComponent();
            history = new();
            var runner = new SMPSOOrchestrator();
            runner.GenerationChanged += OnGenerationChanged;
            algorithmThread = new Thread(new ThreadStart(() => runner.StartSearch(config, exePath, gtkPath, tracePath)));
            algorithmThread.Start();
            listBoxGeneration.SelectedIndexChanged += changeGeneration;
            listBoxLeaders.SelectedIndexChanged += changeConfig;
        }

        public Form2(SearchConfigVEGA config, string exePath, string gtkPath, string tracePath)
        {
            InitializeComponent();
            history = new();
            var runner = new VEGAOrchestrator();
            runner.GenerationChanged += OnGenerationChanged;
            algorithmThread = new Thread(new ThreadStart(() => runner.StartSearch(config, exePath, gtkPath, tracePath)));
            algorithmThread.Start();
            listBoxGeneration.SelectedIndexChanged += changeGeneration;
            listBoxLeaders.SelectedIndexChanged += changeConfig;
        }

        private void changeGeneration(object? sender, EventArgs e)
        {
            this.Invoke(new Action(() =>
            {
                int generationIndex = listBoxGeneration.SelectedIndex;
                listBoxLeaders.DataSource = null;
                listBoxLeaders.DataSource = history[generationIndex].Select(leader => $"{leader.Item2[0]}        {leader.Item2[1]}").ToList();
            }));
        }

        private void changeConfig(object? sender, EventArgs e)
        {
            this.Invoke(new Action(() =>
            {
                int generationIndex = listBoxGeneration.SelectedIndex;
                int configIndex = listBoxLeaders.SelectedIndex;
                if (generationIndex != -1 && configIndex != -1)
                    richTextBox1.Text = history[generationIndex][configIndex].Item1.ToString();
            }));
        }

        private void OnGenerationChanged(object? sender, List<(CPUConfig, double[])> leaders)
        {
            this.Invoke(new Action(() =>
            {
                history.Add(leaders.OrderBy(leader => leader.Item2[0]).ToList());
                listBoxGeneration.Items.Add($"Generation {history.Count}");
                richTextBox1.Text = null;
            }));
        }


        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
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
            using (StreamWriter writer = new StreamWriter(file1))
            {
                writer.WriteLine("ID,CPI,Energy");
                for (int i = 0; i < history.Count; i++)
                {
                    var (config, metrics) = history[i];
                    writer.WriteLine($"{i},{metrics[0]},{metrics[1]}");
                }
            }

            // Second table: ID and CPUConfig fields
            using (StreamWriter writer = new StreamWriter(file2))
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
