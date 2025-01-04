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
    }
}
