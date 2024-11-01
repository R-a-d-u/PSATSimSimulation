using System.Xml.Serialization;



namespace SMPSOsimulation
{
    public partial class Form1 : Form

    {
        PSATsimSimulationFunctions functii = new PSATsimSimulationFunctions();
        public Form1()
        {
            InitializeComponent();

        }


        private async void StartSimulation_click(object sender, EventArgs e)
        {
            await Task.Run(() => functii.StartSimulator());
            //E:\PSATSIM\psatsim_con.exe FisierConfigBun.xml output.xml
        }

        private async void SimulationOptionsButton_Click(object sender, EventArgs e)
        {
            String command=SimulationOptions_TextBox.Text;
            await Task.Run(() => functii.SimulationOptionsFunction(command));
            //E:\PSATSIM\psatsim_con.exe FisierConfigBun.xml output.xml (orice: -m, -a, -A, etc)

        }

        private void SimulationOptions_TextBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
