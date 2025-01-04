namespace SMPSOGui
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
            comboBoxSearchAlgorithm = new ComboBox();
            buttonStartSearch = new Button();
            numericPopulationSize = new NumericUpDown();
            labelPopulationSize = new Label();
            labelLeadersArchiveSize = new Label();
            numericLeadersArchiveSize = new NumericUpDown();
            labelMaxGenerations = new Label();
            numericMaxGenerations = new NumericUpDown();
            labelMutationRate = new Label();
            numericMutationRate = new NumericUpDown();
            labelMaxFrequency = new Label();
            numericMaxFrequency = new NumericUpDown();
            labelVdd = new Label();
            numericVdd = new NumericUpDown();
            labelmemoryArch = new Label();
            combomemoryArch = new ComboBox();
            labelL1CodeHitrate = new Label();
            numericL1CodeHitrate = new NumericUpDown();
            labelL1CodeLatency = new Label();
            numericL1CodeLatency = new NumericUpDown();
            numericL1DataLatency = new NumericUpDown();
            labelL1DataLatency = new Label();
            numericl1DataHitrate = new NumericUpDown();
            labell1DataHitrate = new Label();
            numericL2Latency = new NumericUpDown();
            labelL2Latency = new Label();
            numericL2Hitrate = new NumericUpDown();
            labelL2Hitrate = new Label();
            numericSystemMemory = new NumericUpDown();
            labelSystemMemory = new Label();
            labelSystemMemory2 = new Label();
            labelWeightCPI = new Label();
            numericWeightCPI = new NumericUpDown();
            numericWeightEnergy = new NumericUpDown();
            labelWeightEnergy = new Label();
            labelPrefferedObjective = new Label();
            comboPrefferedObjective = new ComboBox();
            buttonFilePickerExe = new Button();
            labelFilePathExe = new Label();
            labelFilePickerGTK = new Label();
            buttonFilePickerGTK = new Button();
            labelFilePathTrace = new Label();
            buttonFilePickerTrace = new Button();
            ((System.ComponentModel.ISupportInitialize)numericPopulationSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericLeadersArchiveSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericMaxGenerations).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericMutationRate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericMaxFrequency).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericVdd).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericL1CodeHitrate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericL1CodeLatency).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericL1DataLatency).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericl1DataHitrate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericL2Latency).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericL2Hitrate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericSystemMemory).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericWeightCPI).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericWeightEnergy).BeginInit();
            SuspendLayout();
            // 
            // comboBoxSearchAlgorithm
            // 
            comboBoxSearchAlgorithm.FormattingEnabled = true;
            comboBoxSearchAlgorithm.Location = new Point(31, 22);
            comboBoxSearchAlgorithm.Name = "comboBoxSearchAlgorithm";
            comboBoxSearchAlgorithm.Size = new Size(121, 23);
            comboBoxSearchAlgorithm.TabIndex = 0;
            // 
            // buttonStartSearch
            // 
            buttonStartSearch.Location = new Point(644, 22);
            buttonStartSearch.Name = "buttonStartSearch";
            buttonStartSearch.Size = new Size(115, 23);
            buttonStartSearch.TabIndex = 1;
            buttonStartSearch.Text = "Start Search";
            buttonStartSearch.UseVisualStyleBackColor = true;
            buttonStartSearch.Click += buttonStartSearch_Click;
            // 
            // numericPopulationSize
            // 
            numericPopulationSize.Location = new Point(158, 79);
            numericPopulationSize.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numericPopulationSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericPopulationSize.Name = "numericPopulationSize";
            numericPopulationSize.Size = new Size(75, 23);
            numericPopulationSize.TabIndex = 2;
            numericPopulationSize.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // labelPopulationSize
            // 
            labelPopulationSize.AutoSize = true;
            labelPopulationSize.Location = new Point(31, 81);
            labelPopulationSize.Name = "labelPopulationSize";
            labelPopulationSize.Size = new Size(88, 15);
            labelPopulationSize.TabIndex = 3;
            labelPopulationSize.Text = "Population Size";
            // 
            // labelLeadersArchiveSize
            // 
            labelLeadersArchiveSize.AutoSize = true;
            labelLeadersArchiveSize.Location = new Point(31, 116);
            labelLeadersArchiveSize.Name = "labelLeadersArchiveSize";
            labelLeadersArchiveSize.Size = new Size(113, 15);
            labelLeadersArchiveSize.TabIndex = 4;
            labelLeadersArchiveSize.Text = "Leaders Archive Size";
            // 
            // numericLeadersArchiveSize
            // 
            numericLeadersArchiveSize.Location = new Point(158, 114);
            numericLeadersArchiveSize.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numericLeadersArchiveSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericLeadersArchiveSize.Name = "numericLeadersArchiveSize";
            numericLeadersArchiveSize.Size = new Size(75, 23);
            numericLeadersArchiveSize.TabIndex = 5;
            numericLeadersArchiveSize.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // labelMaxGenerations
            // 
            labelMaxGenerations.AutoSize = true;
            labelMaxGenerations.Location = new Point(31, 152);
            labelMaxGenerations.Name = "labelMaxGenerations";
            labelMaxGenerations.Size = new Size(96, 15);
            labelMaxGenerations.TabIndex = 6;
            labelMaxGenerations.Text = "Max Generations";
            // 
            // numericMaxGenerations
            // 
            numericMaxGenerations.Location = new Point(158, 150);
            numericMaxGenerations.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numericMaxGenerations.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericMaxGenerations.Name = "numericMaxGenerations";
            numericMaxGenerations.Size = new Size(75, 23);
            numericMaxGenerations.TabIndex = 7;
            numericMaxGenerations.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // labelMutationRate
            // 
            labelMutationRate.AutoSize = true;
            labelMutationRate.Location = new Point(31, 190);
            labelMutationRate.Name = "labelMutationRate";
            labelMutationRate.Size = new Size(82, 15);
            labelMutationRate.TabIndex = 8;
            labelMutationRate.Text = "Mutation Rate";
            // 
            // numericMutationRate
            // 
            numericMutationRate.DecimalPlaces = 2;
            numericMutationRate.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            numericMutationRate.Location = new Point(158, 188);
            numericMutationRate.Maximum = new decimal(new int[] { 99, 0, 0, 131072 });
            numericMutationRate.Minimum = new decimal(new int[] { 1, 0, 0, 131072 });
            numericMutationRate.Name = "numericMutationRate";
            numericMutationRate.Size = new Size(75, 23);
            numericMutationRate.TabIndex = 9;
            numericMutationRate.Value = new decimal(new int[] { 1, 0, 0, 131072 });
            // 
            // labelMaxFrequency
            // 
            labelMaxFrequency.AutoSize = true;
            labelMaxFrequency.Location = new Point(31, 262);
            labelMaxFrequency.Name = "labelMaxFrequency";
            labelMaxFrequency.Size = new Size(124, 15);
            labelMaxFrequency.TabIndex = 10;
            labelMaxFrequency.Text = "Max Frequency (MHz)";
            // 
            // numericMaxFrequency
            // 
            numericMaxFrequency.Location = new Point(158, 260);
            numericMaxFrequency.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numericMaxFrequency.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericMaxFrequency.Name = "numericMaxFrequency";
            numericMaxFrequency.Size = new Size(75, 23);
            numericMaxFrequency.TabIndex = 11;
            numericMaxFrequency.Value = new decimal(new int[] { 10000, 0, 0, 0 });
            // 
            // labelVdd
            // 
            labelVdd.AutoSize = true;
            labelVdd.Location = new Point(31, 294);
            labelVdd.Name = "labelVdd";
            labelVdd.Size = new Size(45, 15);
            labelVdd.TabIndex = 12;
            labelVdd.Text = "vdd (V)";
            // 
            // numericVdd
            // 
            numericVdd.DecimalPlaces = 2;
            numericVdd.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            numericVdd.Location = new Point(158, 292);
            numericVdd.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            numericVdd.Name = "numericVdd";
            numericVdd.Size = new Size(75, 23);
            numericVdd.TabIndex = 13;
            numericVdd.Value = new decimal(new int[] { 22, 0, 0, 65536 });
            // 
            // labelmemoryArch
            // 
            labelmemoryArch.AutoSize = true;
            labelmemoryArch.Location = new Point(31, 327);
            labelmemoryArch.Name = "labelmemoryArch";
            labelmemoryArch.Size = new Size(120, 15);
            labelmemoryArch.TabIndex = 14;
            labelmemoryArch.Text = "Memory Architecture";
            // 
            // combomemoryArch
            // 
            combomemoryArch.FormattingEnabled = true;
            combomemoryArch.Location = new Point(157, 324);
            combomemoryArch.Name = "combomemoryArch";
            combomemoryArch.Size = new Size(76, 23);
            combomemoryArch.TabIndex = 15;
            // 
            // labelL1CodeHitrate
            // 
            labelL1CodeHitrate.AutoSize = true;
            labelL1CodeHitrate.Location = new Point(251, 262);
            labelL1CodeHitrate.Name = "labelL1CodeHitrate";
            labelL1CodeHitrate.Size = new Size(89, 15);
            labelL1CodeHitrate.TabIndex = 16;
            labelL1CodeHitrate.Text = "L1 Code Hitrate";
            // 
            // numericL1CodeHitrate
            // 
            numericL1CodeHitrate.DecimalPlaces = 2;
            numericL1CodeHitrate.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            numericL1CodeHitrate.Location = new Point(346, 260);
            numericL1CodeHitrate.Maximum = new decimal(new int[] { 99, 0, 0, 131072 });
            numericL1CodeHitrate.Minimum = new decimal(new int[] { 1, 0, 0, 131072 });
            numericL1CodeHitrate.Name = "numericL1CodeHitrate";
            numericL1CodeHitrate.Size = new Size(75, 23);
            numericL1CodeHitrate.TabIndex = 17;
            numericL1CodeHitrate.Value = new decimal(new int[] { 1, 0, 0, 131072 });
            // 
            // labelL1CodeLatency
            // 
            labelL1CodeLatency.AutoSize = true;
            labelL1CodeLatency.Location = new Point(251, 294);
            labelL1CodeLatency.Name = "labelL1CodeLatency";
            labelL1CodeLatency.Size = new Size(94, 15);
            labelL1CodeLatency.TabIndex = 18;
            labelL1CodeLatency.Text = "L1 Code Latency";
            // 
            // numericL1CodeLatency
            // 
            numericL1CodeLatency.Location = new Point(346, 292);
            numericL1CodeLatency.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numericL1CodeLatency.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericL1CodeLatency.Name = "numericL1CodeLatency";
            numericL1CodeLatency.Size = new Size(75, 23);
            numericL1CodeLatency.TabIndex = 19;
            numericL1CodeLatency.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // numericL1DataLatency
            // 
            numericL1DataLatency.Location = new Point(346, 359);
            numericL1DataLatency.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numericL1DataLatency.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericL1DataLatency.Name = "numericL1DataLatency";
            numericL1DataLatency.Size = new Size(75, 23);
            numericL1DataLatency.TabIndex = 23;
            numericL1DataLatency.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // labelL1DataLatency
            // 
            labelL1DataLatency.AutoSize = true;
            labelL1DataLatency.Location = new Point(251, 361);
            labelL1DataLatency.Name = "labelL1DataLatency";
            labelL1DataLatency.Size = new Size(90, 15);
            labelL1DataLatency.TabIndex = 22;
            labelL1DataLatency.Text = "L1 Data Latency";
            // 
            // numericl1DataHitrate
            // 
            numericl1DataHitrate.DecimalPlaces = 2;
            numericl1DataHitrate.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            numericl1DataHitrate.Location = new Point(346, 327);
            numericl1DataHitrate.Maximum = new decimal(new int[] { 99, 0, 0, 131072 });
            numericl1DataHitrate.Minimum = new decimal(new int[] { 1, 0, 0, 131072 });
            numericl1DataHitrate.Name = "numericl1DataHitrate";
            numericl1DataHitrate.Size = new Size(75, 23);
            numericl1DataHitrate.TabIndex = 21;
            numericl1DataHitrate.Value = new decimal(new int[] { 1, 0, 0, 131072 });
            // 
            // labell1DataHitrate
            // 
            labell1DataHitrate.AutoSize = true;
            labell1DataHitrate.Location = new Point(251, 329);
            labell1DataHitrate.Name = "labell1DataHitrate";
            labell1DataHitrate.Size = new Size(85, 15);
            labell1DataHitrate.TabIndex = 20;
            labell1DataHitrate.Text = "L1 Data Hitrate";
            // 
            // numericL2Latency
            // 
            numericL2Latency.Location = new Point(531, 292);
            numericL2Latency.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numericL2Latency.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericL2Latency.Name = "numericL2Latency";
            numericL2Latency.Size = new Size(75, 23);
            numericL2Latency.TabIndex = 27;
            numericL2Latency.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // labelL2Latency
            // 
            labelL2Latency.AutoSize = true;
            labelL2Latency.Location = new Point(436, 294);
            labelL2Latency.Name = "labelL2Latency";
            labelL2Latency.Size = new Size(63, 15);
            labelL2Latency.TabIndex = 26;
            labelL2Latency.Text = "L2 Latency";
            // 
            // numericL2Hitrate
            // 
            numericL2Hitrate.DecimalPlaces = 2;
            numericL2Hitrate.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            numericL2Hitrate.Location = new Point(531, 260);
            numericL2Hitrate.Maximum = new decimal(new int[] { 99, 0, 0, 131072 });
            numericL2Hitrate.Minimum = new decimal(new int[] { 1, 0, 0, 131072 });
            numericL2Hitrate.Name = "numericL2Hitrate";
            numericL2Hitrate.Size = new Size(75, 23);
            numericL2Hitrate.TabIndex = 25;
            numericL2Hitrate.Value = new decimal(new int[] { 1, 0, 0, 131072 });
            // 
            // labelL2Hitrate
            // 
            labelL2Hitrate.AutoSize = true;
            labelL2Hitrate.Location = new Point(436, 262);
            labelL2Hitrate.Name = "labelL2Hitrate";
            labelL2Hitrate.Size = new Size(58, 15);
            labelL2Hitrate.TabIndex = 24;
            labelL2Hitrate.Text = "L2 Hitrate";
            // 
            // numericSystemMemory
            // 
            numericSystemMemory.Location = new Point(158, 359);
            numericSystemMemory.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numericSystemMemory.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericSystemMemory.Name = "numericSystemMemory";
            numericSystemMemory.Size = new Size(75, 23);
            numericSystemMemory.TabIndex = 29;
            numericSystemMemory.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // labelSystemMemory
            // 
            labelSystemMemory.AutoSize = true;
            labelSystemMemory.Location = new Point(63, 363);
            labelSystemMemory.Name = "labelSystemMemory";
            labelSystemMemory.Size = new Size(93, 15);
            labelSystemMemory.TabIndex = 28;
            labelSystemMemory.Text = "System Memory";
            // 
            // labelSystemMemory2
            // 
            labelSystemMemory2.AutoSize = true;
            labelSystemMemory2.Location = new Point(63, 378);
            labelSystemMemory2.Name = "labelSystemMemory2";
            labelSystemMemory2.Size = new Size(45, 15);
            labelSystemMemory2.TabIndex = 30;
            labelSystemMemory2.Text = "latency";
            // 
            // labelWeightCPI
            // 
            labelWeightCPI.AutoSize = true;
            labelWeightCPI.Location = new Point(251, 81);
            labelWeightCPI.Name = "labelWeightCPI";
            labelWeightCPI.Size = new Size(64, 15);
            labelWeightCPI.TabIndex = 31;
            labelWeightCPI.Text = "weight CPI";
            // 
            // numericWeightCPI
            // 
            numericWeightCPI.DecimalPlaces = 3;
            numericWeightCPI.Increment = new decimal(new int[] { 1, 0, 0, 196608 });
            numericWeightCPI.Location = new Point(346, 79);
            numericWeightCPI.Name = "numericWeightCPI";
            numericWeightCPI.Size = new Size(75, 23);
            numericWeightCPI.TabIndex = 32;
            numericWeightCPI.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // numericWeightEnergy
            // 
            numericWeightEnergy.DecimalPlaces = 3;
            numericWeightEnergy.Increment = new decimal(new int[] { 1, 0, 0, 196608 });
            numericWeightEnergy.Location = new Point(346, 114);
            numericWeightEnergy.Name = "numericWeightEnergy";
            numericWeightEnergy.Size = new Size(75, 23);
            numericWeightEnergy.TabIndex = 34;
            numericWeightEnergy.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // labelWeightEnergy
            // 
            labelWeightEnergy.AutoSize = true;
            labelWeightEnergy.Location = new Point(251, 116);
            labelWeightEnergy.Name = "labelWeightEnergy";
            labelWeightEnergy.Size = new Size(82, 15);
            labelWeightEnergy.TabIndex = 33;
            labelWeightEnergy.Text = "weight Energy";
            // 
            // labelPrefferedObjective
            // 
            labelPrefferedObjective.AutoSize = true;
            labelPrefferedObjective.Location = new Point(239, 152);
            labelPrefferedObjective.Name = "labelPrefferedObjective";
            labelPrefferedObjective.Size = new Size(108, 15);
            labelPrefferedObjective.TabIndex = 35;
            labelPrefferedObjective.Text = "Preffered Objective";
            // 
            // comboPrefferedObjective
            // 
            comboPrefferedObjective.FormattingEnabled = true;
            comboPrefferedObjective.Location = new Point(346, 149);
            comboPrefferedObjective.Name = "comboPrefferedObjective";
            comboPrefferedObjective.Size = new Size(76, 23);
            comboPrefferedObjective.TabIndex = 36;
            // 
            // buttonFilePickerExe
            // 
            buttonFilePickerExe.Location = new Point(436, 77);
            buttonFilePickerExe.Name = "buttonFilePickerExe";
            buttonFilePickerExe.Size = new Size(151, 23);
            buttonFilePickerExe.TabIndex = 37;
            buttonFilePickerExe.Text = "Path to psatsim_con.exe";
            buttonFilePickerExe.UseVisualStyleBackColor = true;
            buttonFilePickerExe.Click += buttonFilePickerExe_Click;
            // 
            // labelFilePathExe
            // 
            labelFilePathExe.AutoSize = true;
            labelFilePathExe.Location = new Point(593, 81);
            labelFilePathExe.Name = "labelFilePathExe";
            labelFilePathExe.Size = new Size(82, 15);
            labelFilePathExe.TabIndex = 38;
            labelFilePathExe.Text = "No path given";
            // 
            // labelFilePickerGTK
            // 
            labelFilePickerGTK.AutoSize = true;
            labelFilePickerGTK.Location = new Point(593, 116);
            labelFilePickerGTK.Name = "labelFilePickerGTK";
            labelFilePickerGTK.Size = new Size(82, 15);
            labelFilePickerGTK.TabIndex = 40;
            labelFilePickerGTK.Text = "No path given";
            // 
            // buttonFilePickerGTK
            // 
            buttonFilePickerGTK.Location = new Point(436, 112);
            buttonFilePickerGTK.Name = "buttonFilePickerGTK";
            buttonFilePickerGTK.Size = new Size(151, 23);
            buttonFilePickerGTK.TabIndex = 39;
            buttonFilePickerGTK.Text = "Path to GTK/lib";
            buttonFilePickerGTK.UseVisualStyleBackColor = true;
            buttonFilePickerGTK.Click += buttonFilePickerGTK_Click;
            // 
            // labelFilePathTrace
            // 
            labelFilePathTrace.AutoSize = true;
            labelFilePathTrace.Location = new Point(593, 152);
            labelFilePathTrace.Name = "labelFilePathTrace";
            labelFilePathTrace.Size = new Size(82, 15);
            labelFilePathTrace.TabIndex = 42;
            labelFilePathTrace.Text = "No path given";
            // 
            // buttonFilePickerTrace
            // 
            buttonFilePickerTrace.Location = new Point(436, 148);
            buttonFilePickerTrace.Name = "buttonFilePickerTrace";
            buttonFilePickerTrace.Size = new Size(151, 23);
            buttonFilePickerTrace.TabIndex = 41;
            buttonFilePickerTrace.Text = "Path to trace";
            buttonFilePickerTrace.UseVisualStyleBackColor = true;
            buttonFilePickerTrace.Click += buttonFilePickerTrace_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(labelFilePathTrace);
            Controls.Add(buttonFilePickerTrace);
            Controls.Add(labelFilePickerGTK);
            Controls.Add(buttonFilePickerGTK);
            Controls.Add(labelFilePathExe);
            Controls.Add(buttonFilePickerExe);
            Controls.Add(comboPrefferedObjective);
            Controls.Add(labelPrefferedObjective);
            Controls.Add(numericWeightEnergy);
            Controls.Add(labelWeightEnergy);
            Controls.Add(numericWeightCPI);
            Controls.Add(labelWeightCPI);
            Controls.Add(labelSystemMemory2);
            Controls.Add(numericSystemMemory);
            Controls.Add(labelSystemMemory);
            Controls.Add(numericL2Latency);
            Controls.Add(labelL2Latency);
            Controls.Add(numericL2Hitrate);
            Controls.Add(labelL2Hitrate);
            Controls.Add(numericL1DataLatency);
            Controls.Add(labelL1DataLatency);
            Controls.Add(numericl1DataHitrate);
            Controls.Add(labell1DataHitrate);
            Controls.Add(numericL1CodeLatency);
            Controls.Add(labelL1CodeLatency);
            Controls.Add(numericL1CodeHitrate);
            Controls.Add(labelL1CodeHitrate);
            Controls.Add(combomemoryArch);
            Controls.Add(labelmemoryArch);
            Controls.Add(numericVdd);
            Controls.Add(labelVdd);
            Controls.Add(numericMaxFrequency);
            Controls.Add(labelMaxFrequency);
            Controls.Add(numericMutationRate);
            Controls.Add(labelMutationRate);
            Controls.Add(numericMaxGenerations);
            Controls.Add(labelMaxGenerations);
            Controls.Add(numericLeadersArchiveSize);
            Controls.Add(labelLeadersArchiveSize);
            Controls.Add(labelPopulationSize);
            Controls.Add(numericPopulationSize);
            Controls.Add(buttonStartSearch);
            Controls.Add(comboBoxSearchAlgorithm);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)numericPopulationSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericLeadersArchiveSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericMaxGenerations).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericMutationRate).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericMaxFrequency).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericVdd).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericL1CodeHitrate).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericL1CodeLatency).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericL1DataLatency).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericl1DataHitrate).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericL2Latency).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericL2Hitrate).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericSystemMemory).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericWeightCPI).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericWeightEnergy).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox comboBoxSearchAlgorithm;
        private Button buttonStartSearch;
        private NumericUpDown numericPopulationSize;
        private Label labelPopulationSize;
        private Label labelLeadersArchiveSize;
        private NumericUpDown numericLeadersArchiveSize;
        private Label labelMaxGenerations;
        private NumericUpDown numericMaxGenerations;
        private Label labelMutationRate;
        private NumericUpDown numericMutationRate;
        private Label labelMaxFrequency;
        private NumericUpDown numericMaxFrequency;
        private Label labelVdd;
        private NumericUpDown numericVdd;
        private Label labelmemoryArch;
        private ComboBox combomemoryArch;
        private Label labelL1CodeHitrate;
        private NumericUpDown numericL1CodeHitrate;
        private Label labelL1CodeLatency;
        private NumericUpDown numericL1CodeLatency;
        private NumericUpDown numericL1DataLatency;
        private Label labelL1DataLatency;
        private NumericUpDown numericl1DataHitrate;
        private Label labell1DataHitrate;
        private NumericUpDown numericL2Latency;
        private Label labelL2Latency;
        private NumericUpDown numericL2Hitrate;
        private Label labelL2Hitrate;
        private NumericUpDown numericSystemMemory;
        private Label labelSystemMemory;
        private Label labelSystemMemory2;
        private Label labelWeightCPI;
        private NumericUpDown numericWeightCPI;
        private NumericUpDown numericWeightEnergy;
        private Label labelWeightEnergy;
        private Label labelPrefferedObjective;
        private ComboBox comboPrefferedObjective;
        private Button buttonFilePickerExe;
        private Label labelFilePathExe;
        private Label labelFilePickerGTK;
        private Button buttonFilePickerGTK;
        private Label labelFilePathTrace;
        private Button buttonFilePickerTrace;
    }
}
