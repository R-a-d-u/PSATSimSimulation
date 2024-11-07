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
            label1 = new Label();
            SuspendLayout();
            // 
            // StartSimulationButton
            // 
            StartSimulationButton.Enabled = false;
            StartSimulationButton.Location = new Point(530, 80);
            StartSimulationButton.Name = "StartSimulationButton";
            StartSimulationButton.Size = new Size(121, 30);
            StartSimulationButton.TabIndex = 0;
            StartSimulationButton.Text = "Run SMSPASim";
            StartSimulationButton.UseVisualStyleBackColor = true;
            StartSimulationButton.Click += StartSimulation_click;
            // 
            // SimulationOptionsButton
            // 
            SimulationOptionsButton.Enabled = false;
            SimulationOptionsButton.Location = new Point(530, 116);
            SimulationOptionsButton.Name = "SimulationOptionsButton";
            SimulationOptionsButton.Size = new Size(121, 34);
            SimulationOptionsButton.TabIndex = 1;
            SimulationOptionsButton.Text = "Run with command";
            SimulationOptionsButton.UseVisualStyleBackColor = true;
            SimulationOptionsButton.Click += SimulationOptionsButton_Click;
            // 
            // SimulationOptions_TextBox
            // 
            SimulationOptions_TextBox.Location = new Point(657, 123);
            SimulationOptions_TextBox.Name = "SimulationOptions_TextBox";
            SimulationOptions_TextBox.Size = new Size(131, 23);
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
            richTextBox1.Location = new Point(530, 157);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(258, 281);
            richTextBox1.TabIndex = 4;
            richTextBox1.Text = resources.GetString("richTextBox1.Text");
            // 
            // PSATSIMGeneralButton
            // 
            PSATSIMGeneralButton.Location = new Point(48, 123);
            PSATSIMGeneralButton.Margin = new Padding(2, 2, 2, 2);
            PSATSIMGeneralButton.Name = "PSATSIMGeneralButton";
            PSATSIMGeneralButton.Size = new Size(139, 49);
            PSATSIMGeneralButton.TabIndex = 5;
            PSATSIMGeneralButton.Text = "Run PsatSim General";
            PSATSIMGeneralButton.UseVisualStyleBackColor = true;
            PSATSIMGeneralButton.Click += PSATSIMGeneralButton_Click;
            // 
            // getOutputFIle
            // 
            getOutputFIle.Location = new Point(48, 176);
            getOutputFIle.Margin = new Padding(2, 2, 2, 2);
            getOutputFIle.Name = "getOutputFIle";
            getOutputFIle.Size = new Size(139, 34);
            getOutputFIle.TabIndex = 6;
            getOutputFIle.Text = "Get Output File";
            getOutputFIle.UseVisualStyleBackColor = true;
            getOutputFIle.Click += getOutputFIle_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(74, 95);
            label1.Name = "label1";
            label1.Size = new Size(90, 15);
            label1.TabIndex = 7;
            label1.Text = "Butoanele bune";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(label1);
            Controls.Add(getOutputFIle);
            Controls.Add(PSATSIMGeneralButton);
            Controls.Add(richTextBox1);
            Controls.Add(SimulationOptions_TextBox);
            Controls.Add(SimulationOptionsButton);
            Controls.Add(StartSimulationButton);
            DoubleBuffered = true;
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
        private Label label1;
    }
}
