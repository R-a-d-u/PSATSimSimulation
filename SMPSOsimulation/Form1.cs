using System.Xml.Serialization;



namespace SMPSOsimulation
{
    public partial class Form1 : Form

    {
        PSATsimSimulationFunctions pSATsimSimulationFunctions = new PSATsimSimulationFunctions(); 
        public Form1()
        {
            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            pSATsimSimulationFunctions.RunProcess("-m");
            //pSATsimSimulationFunctions.StartSimulator();
            //pSATsimSimulationFunctions.
            
        }
    }
}
