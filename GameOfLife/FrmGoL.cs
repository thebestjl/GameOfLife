﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameOfLife {
	public partial class FrmGoL : Form {
		private CellularAutomaton[] automata;
		private int dgvRow;
		private int dgvCol;
		private bool running;
		private bool finished_running;
		private const bool display_cell_info = false;
		private int run_speed;
		private const int RANDOMIZE_LIFE_THRESHOLD = 750;
		private const char DELIMITER = ';';

		public FrmGoL() {
			InitializeComponent();
			InitializeControls();
		}

		private void InitializeControls() {
			txtWidth.TextChanged += TxtWidth_TextChanged;
			txtHeight.TextChanged += TxtHeight_TextChanged;
			nudSpeed.ValueChanged += nudSpeed_ValueChanged;
			running = false;
			txtWidth.Text = "10";
			txtHeight.Text = "10";
			run_speed = 1000;
			finished_running = true;
			ResizeGoL();
		}

		private void InitializeAutomata() {
			automata = new CellularAutomaton[dgvCol * dgvRow];

			for (int i = 0; i < automata.Length; i++) {
				automata[i] = new CellularAutomaton();
				ToggleLife(ConvertLinearIndexToCoords(i), false);
			}

			PrintCellData();
		}

		#region Event Handler Functions
		private void dgvCells_CellClick(object sender, DataGridViewCellEventArgs e) {
			dgvCells.ClearSelection();

			int index = GetLinearizedIndex(e.ColumnIndex, e.RowIndex);
			if (!running) {
				automata[index].IsInitial = !automata[index].IsAlive;
			}
			Size coords = new Size(e.ColumnIndex, e.RowIndex);

			ToggleLife(coords, !automata[index].IsAlive);
		}

		private void TxtHeight_TextChanged(object sender, EventArgs e) {
			WidthHeightChanged(txtHeight);
		}

		private void TxtWidth_TextChanged(object sender, EventArgs e) {
			WidthHeightChanged(txtWidth);
		}

		private void nudSpeed_ValueChanged(object sender, EventArgs e) {
			run_speed = (int)(1 / nudSpeed.Value) * 1000;
		}

		private void btnStart_Click(object sender, EventArgs e) {
			if (!running) {
				btnStart.Text = "Stop";
				running = true;
				PrintCellData();
				RunProgram();
			} else {
				btnStart.Text = "Start";
				running = false;
			}
		}

		private void btnClear_Click(object sender, EventArgs e) {
			InitializeAutomata();
		}

		private void btnReset_Click(object sender, EventArgs e) {
			for (int i = 0; i < automata.Length; i++) {
				ToggleLife(ConvertLinearIndexToCoords(i), automata[i].IsInitial);
			}
			PrintCellData();
		}

		private void btnRandomize_Click(object sender, EventArgs e) {
			RandomizeCells();
		}

		private void btnSnap_Click(object sender, EventArgs e) {
			RestoreBalance();
		}

		private void exportCurrentStateToolStripMenuItem_Click(object sender, EventArgs e) {
			ExportCurrentState();
		}

		private void loadStateFromFileToolStripMenuItem_Click(object sender, EventArgs e) {
			LoadStateFromFile();
		}
		#endregion

		#region async
		private async void RunProgram() {
			finished_running = false;
			txtHeight.Enabled = false;
			txtWidth.Enabled = false;

			await Task.Run(() => {
				while (running) {
					System.Threading.Thread.Sleep(run_speed);
					UpdateLiveCells();

					PrintCellData();
				}
				return;
			});

			txtWidth.Enabled = true;
			txtHeight.Enabled = true;
			finished_running = true;
		}

		private async void RestoreBalance() {
			bool resume_running = running;
			Random random = new Random();

			await Task.Run(() => {
				running = false;
				while (!finished_running) {
					System.Threading.Thread.Sleep(10);
				}
			});

			List<Size> aliveCells = new List<Size>();

			for (int i = 0; i < automata.Length; i++) {
				if (automata[i].IsAlive) {
					aliveCells.Add(new Size(i, random.Next(0, 1000)));
				}
			}

			int total_elems = aliveCells.Count;
			int num_to_balance = 0;

			if (total_elems % 2 == 0) {
				num_to_balance = total_elems / 2;
			}
			else {
				num_to_balance = random.Next(0, 1000) > 500 ? (int)Math.Ceiling(total_elems / 2.0) : (int)Math.Floor(total_elems / 2.0);
			}

			aliveCells = InsertionSort(aliveCells);

			for (int i = 0; i < num_to_balance; i++) {
				int index = aliveCells[i].Width;
				Size coords = ConvertLinearIndexToCoords(index);
				ToggleLife(coords, false);
			}

			running = resume_running;
			if (running) {
				RunProgram();
			}
		}

		private async void RandomizeCells() {
			bool resume_running = running;
			Random random = new Random();

			await Task.Run(() => {
				running = false;
				while (!finished_running) {
					System.Threading.Thread.Sleep(10);
				}
			});

			int rowSize = random.Next(1, 30);
			int colSize = random.Next(1, 30);

			txtHeight.Text = rowSize.ToString();
			txtWidth.Text = colSize.ToString();

			for (int i = 0; i < automata.Length; i++) {
				Size coords = ConvertLinearIndexToCoords(i);
				bool random_life = random.Next(0, 1000) > RANDOMIZE_LIFE_THRESHOLD ? true : false;

				automata[i].IsInitial = random_life;
				ToggleLife(coords, random_life);
			}

			running = resume_running;
			if (running) {
				RunProgram();
			}
		}

		#endregion

		#region Helper Functions
		private List<Size> InsertionSort(List<Size> sizes) {
			int i = 1;
			while (i < sizes.Count) {
				int j = i;
				while (j > 0 && sizes[j - 1].Height > sizes[j].Height) {
					Size tempSize = sizes[j];
					sizes[j] = sizes[j - 1];
					sizes[j - 1] = tempSize;
					j--;
				}
				i++;
			}

			return sizes;
		}

		private int GetBoundry(int val, int size) {
			int ret_val = val - (val / size) * size;
			return ret_val >= 0 ? ret_val : size + ret_val;
		}

		private int GetLinearizedIndex(int col, int row) {
			return (col * dgvRow) + row;
		}

		private Size ConvertLinearIndexToCoords(int i) {
			int col = i / dgvRow;
			int row = i - col * dgvRow;

			return new Size(col, row);
		}

		private bool GridHasValidDimesions() {
			bool validColSize = System.Text.RegularExpressions.Regex.IsMatch(txtHeight.Text, "0*[1-9][0-9]*");
			bool validRowSize = System.Text.RegularExpressions.Regex.IsMatch(txtWidth.Text, "0*[1-9][0-9]*");

			return validColSize && validRowSize;
		}

		private void UpdateRowAndColumnSize() {
			int.TryParse(txtHeight.Text, out dgvRow);
			int.TryParse(txtWidth.Text, out dgvCol);

			if (dgvRow > 30) {
				txtHeight.Text = "30";
				return;
			}
			if (dgvCol > 30) {
				txtWidth.Text = "30";
				return;
			}

			DataGridViewColumn[] dataGridViewColumns = new DataGridViewColumn[dgvCol];
			DataGridViewRow[] dataGridViewRows = new DataGridViewRow[dgvRow];

			for (int i = 0; i < dgvCol; i++) {
				dataGridViewColumns[i] = new DataGridViewTextBoxColumn();
				dataGridViewColumns[i].ReadOnly = true;
			}
			for (int i = 0; i < dgvRow; i++) {
				dataGridViewRows[i] = new DataGridViewRow();
			}

			dgvCells.Columns.Clear();
			dgvCells.Rows.Clear();
			dgvCells.Columns.AddRange(dataGridViewColumns);
			dgvCells.Rows.AddRange(dataGridViewRows);

			dgvCells.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			dgvCells.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;

			dgvCells.ClearSelection();

			InitializeAutomata();
		}

		private void ToggleLife(Size coords, bool new_state) {
			int col = coords.Width;
			int row = coords.Height;
			UpdateCellColor(col, row, new_state);
			int index = GetLinearizedIndex(col, row);
			if (automata[index].IsAlive != new_state) {
				automata[index].IsAlive = new_state;

				for (int i = col - 1; i <= col + 1; i++) {
					for (int j = row - 1; j <= row + 1; j++) {
						if (!(i == col && j == row)) {
							int col_ind = GetBoundry(i, dgvCol);
							int row_ind = GetBoundry(j, dgvRow);

							int inner_index = GetLinearizedIndex(col_ind, row_ind);
							if (new_state)
								automata[inner_index].AddNeighbor();
							else
								automata[inner_index].SubNeighbor();
						}
					}
				}
			}
		}

		private void UpdateLiveCells() {
			for (int i = 0; i < automata.Length; i++) {
				bool new_state = automata[i].NextState;
				ToggleLife(ConvertLinearIndexToCoords(i), new_state);
			}
		}

		private void WidthHeightChanged(TextBox textBox) {
			if (System.Text.RegularExpressions.Regex.IsMatch(textBox.Text, "[^0-9]")) {
				textBox.Text = textBox.Text.Remove(textBox.Text.Length - 1);
			}

			if (GridHasValidDimesions()) {
				UpdateRowAndColumnSize();
				dgvCells.Height = dgvCells.Rows.GetRowsHeight(DataGridViewElementStates.Visible);
				ResizeGoL();
			}
		}

		private void UpdateCellColor(int col, int row, bool new_state) {
			int index = GetLinearizedIndex(col, row);
			if (automata[index].IsAlive == new_state) {
				if (automata[index].IsAlive) {
					dgvCells[col, row].Style.BackColor = Color.Blue;
				} else {
					dgvCells[col, row].Style.BackColor = Color.White;
				}
			} else {
				if (automata[index].IsAlive) {
					dgvCells[col, row].Style.BackColor = Color.White;
				}
				else {
					dgvCells[col, row].Style.BackColor = Color.Blue;
				}
			}
		}

		private void ResizeGoL() {
			int button_spacerX = 4;
			int button_height = btnStart.Height;
			int cellHeight = dgvCells[0, 0].Size.Height;

			dgvCells.Width = dgvCol * cellHeight + cellHeight;
			dgvCells.Height = dgvRow * cellHeight + button_height;

			int btnStartX = dgvCells.Location.X + dgvCells.Width + button_spacerX;
			int btnStartY = dgvCells.Location.Y;
			int btnResetY = dgvCells.Location.Y + button_height;
			int btnClearY = dgvCells.Location.Y + 2 * button_height;
			int lblSpeedY = (int)(dgvCells.Location.Y + 3.5 * button_height);
			int nudSpeedX = btnStartX + lblSpeed.Width;
			int btnSnapY = lblSpeedY + button_height;
			int lblHeightY_1 = btnSnapY + button_height;
			int lblHeightY_2 = btnStartY + dgvCells.Height - (3 * button_height);
			int lblHeightY = lblHeightY_1 >= lblHeightY_2 ? (lblHeightY_1 + 3) : (lblHeightY_2 + 3);
			int lblWidthY = lblHeightY + button_height;
			int btnRandomizeY = lblWidthY + button_height;
			int clientSizeHeight_1 = btnRandomizeY + button_height + 3;
			int clientSizeHeight_2 = dgvCells.Height + btnStartY;
			int clientSizeHeight = clientSizeHeight_1 > clientSizeHeight_2 ? clientSizeHeight_1 : clientSizeHeight_2;
			int clientSizeWidth = btnStartX + btnStart.Width + 3;

			btnStart.Location = new Point(btnStartX, btnStartY);
			btnReset.Location = new Point(btnStartX, btnResetY);
			btnClear.Location = new Point(btnStartX, btnClearY);
			lblSpeed.Location = new Point(btnStartX, lblSpeedY);
			nudSpeed.Location = new Point(nudSpeedX, lblSpeedY - 3);
			btnSnap.Location = new Point(btnStartX, btnSnapY);
			lblHeight.Location = new Point(btnStartX, lblHeightY);
			lblWidth.Location = new Point(btnStartX, lblWidthY);
			txtHeight.Location = new Point(nudSpeedX, lblHeightY - 3);
			txtWidth.Location = new Point(nudSpeedX, lblWidthY - 3);
			btnRandomize.Location = new Point(btnStartX, btnRandomizeY);

			ClientSize = new Size(clientSizeWidth + 20, clientSizeHeight + 50);
			MinimumSize = new Size(clientSizeWidth + 20, clientSizeHeight + 50);
			MaximumSize = new Size(clientSizeWidth + 20, clientSizeHeight + 50);
		}
		#endregion

		#region Text File Functions
		private void ExportCurrentState() {
			string cur_directory = Path.GetDirectoryName(Application.ExecutablePath);
			string fp = Path.Combine(cur_directory, @"data.gol");
			try {
				File.Delete(fp);
			} catch { }
			
			using (StreamWriter sw = File.AppendText(fp)) {
				StringBuilder sb = new StringBuilder();
				sb.Append(dgvCol);
				sb.Append(DELIMITER);
				sb.Append(dgvRow);
				sb.Append(DELIMITER);
				sw.WriteLine(sb.ToString());

				for (int i = 0; i < automata.Length; i++) {
					sb.Clear();

					sb.Append(i);
					sb.Append(DELIMITER);
					sb.Append(automata[i].IsInitial);
					sb.Append(DELIMITER);
					sb.Append(automata[i].IsAlive);
					sb.Append(DELIMITER);
					//sb.Append(automata[i].NextState);
					//sb.Append(DELIMITER);
					//sb.Append(automata[i].NumNeighbors);
					//sb.Append(DELIMITER);

					sw.WriteLine(sb.ToString());
				}
			}
		}

		private void LoadStateFromFile() {
			string cur_directory = Path.GetDirectoryName(Application.ExecutablePath);
			string fp = Path.Combine(cur_directory, @"data.gol");

			try {
				using (StreamReader sr = File.OpenText(fp)) {
					string coords = sr.ReadLine();
					string[] coords_arr = coords.Split(DELIMITER);

					txtWidth.Text = coords_arr[0];
					txtHeight.Text = coords_arr[1];

					InitializeAutomata();
				
					while (!sr.EndOfStream) {
						string str_cell = sr.ReadLine();
						string[] cell_arr = str_cell.Split(DELIMITER);

						int.TryParse(cell_arr[0], out int index);
						bool.TryParse(cell_arr[1], out bool init_state);
						bool.TryParse(cell_arr[2], out bool is_alive);

						automata[index].IsInitial = init_state;

						if (is_alive)
							ToggleLife(ConvertLinearIndexToCoords(index), true);
					}
				}
			} catch (FileNotFoundException) {
				MessageBox.Show("No File to Load.");

			} catch (Exception e) {
				StringBuilder sberr = new StringBuilder();
				sberr.Append("Error in loading state:\n");
				sberr.Append(e.Message);
				sberr.Append("\nStacktrace:\n");
				sberr.Append(e.StackTrace);

				MessageBox.Show(sberr.ToString());
			}
		}
		#endregion

		#region Console Functions
		private void PrintCellData() {
			if (display_cell_info) {
				StringBuilder sb = new StringBuilder();
				for (int j = 0; j < dgvRow; j++) {
					for (int i = 0; i < dgvCol; i++) {
						sb.Append(automata[GetLinearizedIndex(i, j)]);
						sb.Append(";\t");
					}

					sb.Append("\n");
				}
				Console.WriteLine(sb.ToString());
			}
		}
		#endregion
	}
}
