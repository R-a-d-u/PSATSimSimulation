namespace SMPSOGui
{
    partial class Form2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            listBoxGeneration = new ListBox();
            listBoxLeaders = new ListBox();
            richTextBox1 = new RichTextBox();
            SuspendLayout();
            // 
            // listBoxGeneration
            // 
            listBoxGeneration.FormattingEnabled = true;
            listBoxGeneration.ItemHeight = 15;
            listBoxGeneration.Location = new Point(29, 43);
            listBoxGeneration.Name = "listBoxGeneration";
            listBoxGeneration.Size = new Size(206, 289);
            listBoxGeneration.TabIndex = 0;
            // 
            // listBoxLeaders
            // 
            listBoxLeaders.FormattingEnabled = true;
            listBoxLeaders.ItemHeight = 15;
            listBoxLeaders.Location = new Point(261, 43);
            listBoxLeaders.Name = "listBoxLeaders";
            listBoxLeaders.Size = new Size(214, 289);
            listBoxLeaders.TabIndex = 1;
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new Point(512, 41);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.ReadOnly = true;
            richTextBox1.Size = new Size(243, 291);
            richTextBox1.TabIndex = 2;
            richTextBox1.Text = "";
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(richTextBox1);
            Controls.Add(listBoxLeaders);
            Controls.Add(listBoxGeneration);
            DoubleBuffered = true;
            Name = "Form2";
            Text = "Form2";
            FormClosed += Form2_FormClosed;
            ResumeLayout(false);
        }

        #endregion

        private ListBox listBoxGeneration;
        private ListBox listBoxLeaders;
        private RichTextBox richTextBox1;
    }
}