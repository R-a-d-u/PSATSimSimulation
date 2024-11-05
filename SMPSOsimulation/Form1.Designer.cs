namespace SMPSOsimulation
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            StartSimulationButton = new Button();
            SimulationOptionsButton = new Button();
            SimulationOptions_TextBox = new TextBox();
            contextMenuStrip1 = new ContextMenuStrip(components);
            richTextBox1 = new RichTextBox();
            PSATSIMGeneralButton = new Button();
            getOutputFIle = new Button();
            SuspendLayout();
            // 
            // StartSimulationButton
            // 
            StartSimulationButton.Location = new Point(757, 45);
            StartSimulationButton.Margin = new Padding(4, 5, 4, 5);
            StartSimulationButton.Name = "StartSimulationButton";
            StartSimulationButton.Size = new Size(173, 50);
            StartSimulationButton.TabIndex = 0;
            StartSimulationButton.Text = "Run SMSPASim";
            StartSimulationButton.UseVisualStyleBackColor = true;
            StartSimulationButton.Click += StartSimulation_click;
            // 
            // SimulationOptionsButton
            // 
            SimulationOptionsButton.Location = new Point(757, 193);
            SimulationOptionsButton.Margin = new Padding(4, 5, 4, 5);
            SimulationOptionsButton.Name = "SimulationOptionsButton";
            SimulationOptionsButton.Size = new Size(173, 57);
            SimulationOptionsButton.TabIndex = 1;
            SimulationOptionsButton.Text = "Run with command";
            SimulationOptionsButton.UseVisualStyleBackColor = true;
            SimulationOptionsButton.Click += SimulationOptionsButton_Click;
            // 
            // SimulationOptions_TextBox
            // 
            SimulationOptions_TextBox.Location = new Point(939, 205);
            SimulationOptions_TextBox.Margin = new Padding(4, 5, 4, 5);
            SimulationOptions_TextBox.Name = "SimulationOptions_TextBox";
            SimulationOptions_TextBox.Size = new Size(185, 31);
            SimulationOptions_TextBox.TabIndex = 2;
            SimulationOptions_TextBox.TextChanged += SimulationOptions_TextBox_TextChanged;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new Size(24, 24);
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(61, 4);
            // 
            // richTextBox1
            // 
            richTextBox1.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            richTextBox1.Location = new Point(757, 262);
            richTextBox1.Margin = new Padding(4, 5, 4, 5);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(367, 466);
            richTextBox1.TabIndex = 4;
            richTextBox1.Text = resources.GetString("richTextBox1.Text");
            // 
            // PSATSIMGeneralButton
            // 
            PSATSIMGeneralButton.Location = new Point(757, 103);
            PSATSIMGeneralButton.Name = "PSATSIMGeneralButton";
            PSATSIMGeneralButton.Size = new Size(173, 82);
            PSATSIMGeneralButton.TabIndex = 5;
            PSATSIMGeneralButton.Text = "Run PsatSim General";
            PSATSIMGeneralButton.UseVisualStyleBackColor = true;
            PSATSIMGeneralButton.Click += PSATSIMGeneralButton_Click;
            // 
            // getOutputFIle
            // 
            getOutputFIle.Location = new Point(454, 224);
            getOutputFIle.Name = "getOutputFIle";
            getOutputFIle.Size = new Size(199, 56);
            getOutputFIle.TabIndex = 6;
            getOutputFIle.Text = "Get Output File";
            getOutputFIle.UseVisualStyleBackColor = true;
            getOutputFIle.Click += getOutputFIle_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1143, 750);
            Controls.Add(getOutputFIle);
            Controls.Add(PSATSIMGeneralButton);
            Controls.Add(richTextBox1);
            Controls.Add(SimulationOptions_TextBox);
            Controls.Add(SimulationOptionsButton);
            Controls.Add(StartSimulationButton);
            Margin = new Padding(4, 5, 4, 5);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button StartSimulationButton;
        private Button SimulationOptionsButton;
        private TextBox SimulationOptions_TextBox;
        private ContextMenuStrip contextMenuStrip1;
        private RichTextBox richTextBox1;
        private Button PSATSIMGeneralButton;
        private Button getOutputFIle;
    }
}
