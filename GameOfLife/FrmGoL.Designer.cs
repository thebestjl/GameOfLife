namespace GameOfLife {
	partial class FrmGoL {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmGoL));
			this.tsOptions = new System.Windows.Forms.ToolStrip();
			this.btnStart = new System.Windows.Forms.Button();
			this.btnReset = new System.Windows.Forms.Button();
			this.btnClear = new System.Windows.Forms.Button();
			this.btnRandomize = new System.Windows.Forms.Button();
			this.txtWidth = new System.Windows.Forms.TextBox();
			this.txtHeight = new System.Windows.Forms.TextBox();
			this.lblWidth = new System.Windows.Forms.Label();
			this.lblHeight = new System.Windows.Forms.Label();
			this.dgvCells = new System.Windows.Forms.DataGridView();
			this.nudSpeed = new System.Windows.Forms.NumericUpDown();
			this.lblSpeed = new System.Windows.Forms.Label();
			this.btnSnap = new System.Windows.Forms.Button();
			this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
			this.tsOptions.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvCells)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudSpeed)).BeginInit();
			this.SuspendLayout();
			// 
			// tsOptions
			// 
			this.tsOptions.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.tsOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1});
			this.tsOptions.Location = new System.Drawing.Point(0, 0);
			this.tsOptions.Name = "tsOptions";
			this.tsOptions.Size = new System.Drawing.Size(800, 27);
			this.tsOptions.TabIndex = 0;
			// 
			// btnStart
			// 
			this.btnStart.Location = new System.Drawing.Point(671, 31);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(117, 31);
			this.btnStart.TabIndex = 1;
			this.btnStart.Text = "Start";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// btnReset
			// 
			this.btnReset.Location = new System.Drawing.Point(671, 61);
			this.btnReset.Name = "btnReset";
			this.btnReset.Size = new System.Drawing.Size(117, 31);
			this.btnReset.TabIndex = 2;
			this.btnReset.Text = "Reset";
			this.btnReset.UseVisualStyleBackColor = true;
			this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
			// 
			// btnClear
			// 
			this.btnClear.Location = new System.Drawing.Point(671, 91);
			this.btnClear.Name = "btnClear";
			this.btnClear.Size = new System.Drawing.Size(117, 31);
			this.btnClear.TabIndex = 3;
			this.btnClear.Text = "Clear";
			this.btnClear.UseVisualStyleBackColor = true;
			this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
			// 
			// btnRandomize
			// 
			this.btnRandomize.Location = new System.Drawing.Point(671, 407);
			this.btnRandomize.Name = "btnRandomize";
			this.btnRandomize.Size = new System.Drawing.Size(117, 31);
			this.btnRandomize.TabIndex = 4;
			this.btnRandomize.Text = "Randomize";
			this.btnRandomize.UseVisualStyleBackColor = true;
			this.btnRandomize.Click += new System.EventHandler(this.btnRandomize_Click);
			// 
			// txtWidth
			// 
			this.txtWidth.Location = new System.Drawing.Point(723, 370);
			this.txtWidth.Name = "txtWidth";
			this.txtWidth.Size = new System.Drawing.Size(65, 22);
			this.txtWidth.TabIndex = 5;
			// 
			// txtHeight
			// 
			this.txtHeight.Location = new System.Drawing.Point(723, 342);
			this.txtHeight.Name = "txtHeight";
			this.txtHeight.Size = new System.Drawing.Size(65, 22);
			this.txtHeight.TabIndex = 6;
			// 
			// lblWidth
			// 
			this.lblWidth.AutoSize = true;
			this.lblWidth.Location = new System.Drawing.Point(668, 373);
			this.lblWidth.Name = "lblWidth";
			this.lblWidth.Size = new System.Drawing.Size(48, 17);
			this.lblWidth.TabIndex = 7;
			this.lblWidth.Text = "Width:";
			// 
			// lblHeight
			// 
			this.lblHeight.AutoSize = true;
			this.lblHeight.Location = new System.Drawing.Point(668, 342);
			this.lblHeight.Name = "lblHeight";
			this.lblHeight.Size = new System.Drawing.Size(53, 17);
			this.lblHeight.TabIndex = 8;
			this.lblHeight.Text = "Height:";
			// 
			// dgvCells
			// 
			this.dgvCells.AllowUserToAddRows = false;
			this.dgvCells.AllowUserToDeleteRows = false;
			this.dgvCells.AllowUserToResizeColumns = false;
			this.dgvCells.AllowUserToResizeRows = false;
			this.dgvCells.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.dgvCells.ColumnHeadersVisible = false;
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dgvCells.DefaultCellStyle = dataGridViewCellStyle3;
			this.dgvCells.Location = new System.Drawing.Point(15, 35);
			this.dgvCells.Name = "dgvCells";
			this.dgvCells.RowHeadersVisible = false;
			this.dgvCells.RowTemplate.Height = 24;
			this.dgvCells.Size = new System.Drawing.Size(650, 405);
			this.dgvCells.TabIndex = 9;
			this.dgvCells.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCells_CellClick);
			// 
			// nudSpeed
			// 
			this.nudSpeed.DecimalPlaces = 2;
			this.nudSpeed.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
			this.nudSpeed.Location = new System.Drawing.Point(723, 140);
			this.nudSpeed.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.nudSpeed.Minimum = new decimal(new int[] {
            25,
            0,
            0,
            131072});
			this.nudSpeed.Name = "nudSpeed";
			this.nudSpeed.Size = new System.Drawing.Size(65, 22);
			this.nudSpeed.TabIndex = 10;
			this.nudSpeed.Value = new decimal(new int[] {
            10,
            0,
            0,
            65536});
			// 
			// lblSpeed
			// 
			this.lblSpeed.AutoSize = true;
			this.lblSpeed.Location = new System.Drawing.Point(668, 142);
			this.lblSpeed.Name = "lblSpeed";
			this.lblSpeed.Size = new System.Drawing.Size(53, 17);
			this.lblSpeed.TabIndex = 11;
			this.lblSpeed.Text = "Speed:";
			// 
			// btnSnap
			// 
			this.btnSnap.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.btnSnap.Location = new System.Drawing.Point(671, 168);
			this.btnSnap.Name = "btnSnap";
			this.btnSnap.Size = new System.Drawing.Size(117, 31);
			this.btnSnap.TabIndex = 12;
			this.btnSnap.Text = "Snap";
			this.btnSnap.UseVisualStyleBackColor = true;
			this.btnSnap.Click += new System.EventHandler(this.btnSnap_Click);
			// 
			// toolStripDropDownButton1
			// 
			this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
			this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
			this.toolStripDropDownButton1.Size = new System.Drawing.Size(75, 24);
			this.toolStripDropDownButton1.Text = "Options";
			this.toolStripDropDownButton1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// FrmGoL
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.btnSnap);
			this.Controls.Add(this.lblSpeed);
			this.Controls.Add(this.nudSpeed);
			this.Controls.Add(this.dgvCells);
			this.Controls.Add(this.lblHeight);
			this.Controls.Add(this.lblWidth);
			this.Controls.Add(this.txtHeight);
			this.Controls.Add(this.txtWidth);
			this.Controls.Add(this.btnRandomize);
			this.Controls.Add(this.btnClear);
			this.Controls.Add(this.btnReset);
			this.Controls.Add(this.btnStart);
			this.Controls.Add(this.tsOptions);
			this.Name = "FrmGoL";
			this.Text = "Conway\'s Game of Life";
			this.tsOptions.ResumeLayout(false);
			this.tsOptions.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvCells)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudSpeed)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private System.Windows.Forms.ToolStrip tsOptions;
		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.Button btnReset;
		private System.Windows.Forms.Button btnClear;
		private System.Windows.Forms.Button btnRandomize;
		private System.Windows.Forms.TextBox txtWidth;
		private System.Windows.Forms.TextBox txtHeight;
		private System.Windows.Forms.Label lblWidth;
		private System.Windows.Forms.Label lblHeight;
		private System.Windows.Forms.DataGridView dgvCells;
		private System.Windows.Forms.NumericUpDown nudSpeed;
		private System.Windows.Forms.Label lblSpeed;
		private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
		private System.Windows.Forms.Button btnSnap;
	}
}

