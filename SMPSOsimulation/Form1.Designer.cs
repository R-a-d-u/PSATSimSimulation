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
            SuspendLayout();
            // 
            // StartSimulationButton
            // 
            StartSimulationButton.Location = new Point(12, 12);
            StartSimulationButton.Name = "StartSimulationButton";
            StartSimulationButton.Size = new Size(157, 95);
            StartSimulationButton.TabIndex = 0;
            StartSimulationButton.Text = "START SIM";
            StartSimulationButton.UseVisualStyleBackColor = true;
            StartSimulationButton.Click += StartSimulation_click;
            // 
            // SimulationOptionsButton
            // 
            SimulationOptionsButton.Location = new Point(530, 127);
            SimulationOptionsButton.Name = "SimulationOptionsButton";
            SimulationOptionsButton.Size = new Size(121, 23);
            SimulationOptionsButton.TabIndex = 1;
            SimulationOptionsButton.Text = "Options";
            SimulationOptionsButton.UseVisualStyleBackColor = true;
            SimulationOptionsButton.Click += SimulationOptionsButton_Click;
            // 
            // SimulationOptions_TextBox
            // 
            SimulationOptions_TextBox.Location = new Point(657, 128);
            SimulationOptions_TextBox.Name = "SimulationOptions_TextBox";
            SimulationOptions_TextBox.Size = new Size(131, 23);
            SimulationOptions_TextBox.TabIndex = 2;
            SimulationOptions_TextBox.TextChanged += SimulationOptions_TextBox_TextChanged;
            // 
            // contextMenuStrip1
            // 
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
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(richTextBox1);
            Controls.Add(SimulationOptions_TextBox);
            Controls.Add(SimulationOptionsButton);
            Controls.Add(StartSimulationButton);
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
    }
}
