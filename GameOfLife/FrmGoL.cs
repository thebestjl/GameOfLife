﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
 *	CONWAY'S GAME OF LIFE 
 *	
 *	RULES:
 *	Any live cell with fewer than two live neighbours dies, as if by underpopulation.
 *	Any live cell with two or three live neighbours lives on to the next generation.
 *	Any live cell with more than three live neighbours dies, as if by overpopulation.
 *	Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.
 *	
 *	THINGS I WOULD STILL LIKE TO DO:
 *	Create a custom control to use in place of a datagridview full of text boxes because:
 *		The text boxes are not always consistent sizes
 *		The DataGridView always has some extra space at the bottom for some reason
 *		The DataGridView sometimes has scroll bars
 */

namespace GameOfLife {

	public partial class FrmGoL : Form {
		private CellularAutomaton[] automata;
		private int dgvRow;			//RowSize
		private int dgvCol;			//ColumnSize
		private bool running;		
		private bool finished_running;	//Prevents bad things from happening while a loop is still iterating.
		private const bool display_cell_info = false;	//A fun console logging control
		private int run_speed;		//How fast the program runs. Currently capped between 0.25x and 5.00x speed.
		private const int RANDOMIZE_LIFE_THRESHOLD = 750;	//Divided by 10, roughly the percent chance a cell alive when randomizing cells.
		private const char DELIMITER = ';';	//Used in saving/loading states, to distinguish one cell from another.
		private CancellationTokenSource ctsCancel;
		private CancellationTokenSource ctsPause;

		public FrmGoL() {
			InitializeComponent();
			InitializeControls();
		}

		/*
		 * Sets event handlers, and default variable values
		 */
		private void InitializeControls() {
			ctsCancel = new CancellationTokenSource();
			ctsPause = new CancellationTokenSource();

			txtWidth.TextChanged += TxtWidth_TextChanged;
			txtHeight.TextChanged += TxtHeight_TextChanged;
			txtWidth.LostFocus += txtWidth_FocusOut;
			txtHeight.LostFocus += txtHeight_FocusOut;
			nudSpeed.ValueChanged += nudSpeed_ValueChanged;

			run_speed = 1000;
			running = false;
			finished_running = true;

			txtHeight.Text = "10";
			txtWidth.Text = "10";

			WidthHeightChanged(txtHeight);
			WidthHeightChanged(txtWidth);
			ResizeGoL();
			InitializeOpenFileDialog();
		}

		private void InitializeOpenFileDialog() {
			string init_dir = Path.GetDirectoryName(Application.ExecutablePath);
			init_dir = Path.Combine(init_dir, "SavedGoLStates");
			if (!Directory.Exists(init_dir)) {
				Directory.CreateDirectory(init_dir);
			}

			ofdOpen = new OpenFileDialog {
				InitialDirectory = init_dir,
				Title = "Browse GoL States",
				CheckFileExists = true,
				CheckPathExists = true,
				DefaultExt = "gol",
				Filter = "gol files (*.gol)|*.gol|All files (*.*)|*.*",
				FilterIndex = 2,
				Multiselect = false,
				RestoreDirectory = true
		};
		}

		/*
		 * Reverts automata to initial, unalive state.
		 */
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

		private void txtHeight_FocusOut(object sender, EventArgs e) {
			WidthHeightChanged(txtHeight);
		}

		private void txtWidth_FocusOut(object sender, EventArgs e) {
			WidthHeightChanged(txtWidth);
		}

		private void TxtHeight_TextChanged(object sender, EventArgs e) {
			FixWidthHeightFormat(txtHeight);
		}

		private void TxtWidth_TextChanged(object sender, EventArgs e) {
			FixWidthHeightFormat(txtWidth);
		}

		private void nudSpeed_ValueChanged(object sender, EventArgs e) {
			run_speed = (int)(1 / nudSpeed.Value * 1000);
		}

		private void btnStart_Click(object sender, EventArgs e) {
			if (!running) {
				btnStart.Text = "Stop";
				running = true;
				PrintCellData();

				RunProgram();
			} else {
				btnStart.Text = "Start";
				txtWidth.Enabled = true;
				txtHeight.Enabled = true;
				running = false;
				ctsPause.Cancel();
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
			ctsCancel.Cancel();
			RandomizeCells();
		}

		private void btnSnap_Click(object sender, EventArgs e) {
			ctsCancel.Cancel();
			RestoreBalance();
		}

		private void exportCurrentStateToolStripMenuItem_Click(object sender, EventArgs e) {
			ExportCurrentState();
		}

		private void loadStateFromFileToolStripMenuItem_Click(object sender, EventArgs e) {
			if (ofdOpen.ShowDialog() == DialogResult.OK) {
				LoadStateFromFile(ofdOpen.FileName);
			}
		}
		#endregion

		#region async functions
		/*
		 * Main Loop
		 */
		private async void RunProgram() {
			try {
				txtWidth.Enabled = false;
				txtHeight.Enabled = false;
				ctsCancel = new CancellationTokenSource();
				ctsPause = new CancellationTokenSource();
				finished_running = false;				

				await Task.Run(() => {
					while (running) {
						if (!ctsPause.Token.IsCancellationRequested)
							ctsPause.Token.ThrowIfCancellationRequested();
						else if (!ctsCancel.Token.IsCancellationRequested)
							ctsCancel.Token.ThrowIfCancellationRequested();
						else if (ctsPause.Token.IsCancellationRequested || ctsCancel.Token.IsCancellationRequested)
							break;

						Thread.Sleep(run_speed);
						UpdateLiveCells();

						PrintCellData();
					}
				});

			} catch (OperationCanceledException) {
				MessageBox.Show("Paused");
			} catch (Exception ex) {
				string strEx = BuildExceptionString(ex);
				MessageBox.Show(strEx);
			} finally {
				finished_running = true;
			}
		}

		/*
		 * Destroys half of all living cells
		 */
		private async void RestoreBalance() {
			bool resume_running = running;
			Random random = new Random();

			await Task.Run(() => {
				running = false;
				while (!finished_running) {
					Thread.Sleep(0);
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

		/*
		 * Randomizes currently alive cells
		 */
		private async void RandomizeCells() {
			bool resume_running = running;
			Random random = new Random();

			await Task.Run(() => {
				running = false;
				while (!finished_running) {
					Thread.Sleep(0);
				}
			});

			for (int i = 0; i < automata.Length; i++) {
				Size coords = ConvertLinearIndexToCoords(i);
				bool random_life = random.Next(0, 1000) > RANDOMIZE_LIFE_THRESHOLD ? true : false;

				automata[i].IsInitial = random_life;
				automata[i].IsAlive = random_life;
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

		private void UpdateLife(int i) {
			Size coords = ConvertLinearIndexToCoords(i);
			int col = coords.Height;
			int row = coords.Width;
			if (automata[i].IsAlive == automata[i].NextState) {
				if (automata[i].IsAlive) {
					dgvCells[row, col].Style.BackColor = Color.Blue;
				}
				else {
					dgvCells[row, col].Style.BackColor = Color.White;
				}
			}
			else {
				if (automata[i].IsAlive) {
					dgvCells[row, col].Style.BackColor = Color.White;
				}
				else {
					dgvCells[row, col].Style.BackColor = Color.Blue;
				}
			}
		}

		private void UpdateNeighbors(int ind) {
			Size coords = ConvertLinearIndexToCoords(ind);
			bool new_state = automata[ind].NextState;
			int col = coords.Width;
			int row = coords.Height;
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

			if (!ctsCancel.Token.IsCancellationRequested)
				ctsCancel.Token.ThrowIfCancellationRequested();
		}

		private void UpdateLiveCells() {
			if (chkAsync.Checked) {
				while(!Parallel.For(0, automata.Length, i => UpdateLife(i)).IsCompleted);
			} else {
				for (int i = 0; i < automata.Length; i++) {
					UpdateLife(i);
				}
			}
			for (int i = 0; i < automata.Length; i++) {
				UpdateNeighbors(i);
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
				} else {
					dgvCells[col, row].Style.BackColor = Color.Blue;
				}
			}
		}

		private void UpdateRowAndColumnSize() {
			int.TryParse(txtHeight.Text, out int row);
			int.TryParse(txtWidth.Text, out int col);

			if (dgvRow == row && dgvCol == col) {
				return;
			}
			else {
				dgvRow = row;
				dgvCol = col;
			}

			if (dgvRow > 30) {
				txtHeight.Text = "30";
				WidthHeightChanged(txtHeight);
				return;
			}
			if (dgvCol > 30) {
				txtWidth.Text = "30";
				WidthHeightChanged(txtWidth);
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

		private bool GridHasValidDimesions() {
			bool validColSize = System.Text.RegularExpressions.Regex.IsMatch(txtHeight.Text, "0*[1-9][0-9]*");
			bool validRowSize = System.Text.RegularExpressions.Regex.IsMatch(txtWidth.Text, "0*[1-9][0-9]*");

			return validColSize && validRowSize;
		}

		private void WidthHeightChanged(TextBox textBox) {
			if (GridHasValidDimesions()) {
				UpdateRowAndColumnSize();
				dgvCells.Height = dgvCells.Rows.GetRowsHeight(DataGridViewElementStates.Visible);
				ResizeGoL();
			}
			else {
				txtHeight.Text = dgvRow.ToString();
				txtWidth.Text = dgvCol.ToString();
			}
		}

		private void FixWidthHeightFormat(TextBox textBox) {
			if (System.Text.RegularExpressions.Regex.IsMatch(textBox.Text, "[^0-9]")) {
				textBox.Text = textBox.Text.Remove(textBox.Text.Length - 1);
			}

			textBox.Text = textBox.Text.TrimStart(new char[] { '0' });
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
			int chkAsyncY = btnSnapY + button_height;
			int lblHeightY_1 = chkAsyncY + button_height;
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

			chkAsync.Location = new Point(btnStartX, chkAsyncY);

			ClientSize = new Size(clientSizeWidth + 20, clientSizeHeight + 50);
			MinimumSize = new Size(clientSizeWidth + 20, clientSizeHeight + 50);
			MaximumSize = new Size(clientSizeWidth + 20, clientSizeHeight + 50);
		}

		private string BuildExceptionString(Exception ex) {
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("Exception Message:");
			sb.AppendLine(ex.Message);
			sb.AppendLine("Stack Trace:");
			sb.AppendLine(ex.StackTrace);

			return sb.ToString();
		}
		#endregion

		#region Text File Functions
		private void ExportCurrentState() {
			string cur_directory = Path.GetDirectoryName(Application.ExecutablePath);
			cur_directory = Path.Combine(cur_directory, "SavedGoLStates");
			string saveFileName = GetDefaultFileName();
			string fp = Path.Combine(cur_directory, saveFileName);
			try {
				File.Delete(fp);
			} catch { }
			
			using (StreamWriter sw = File.AppendText(fp)) {
				StringBuilder sb = new StringBuilder();
				sb.AppendWithDelimiter(dgvCol, DELIMITER);
				sb.AppendWithDelimiter(dgvRow, DELIMITER);
				sw.WriteLine(sb.ToString());

				for (int i = 0; i < automata.Length; i++) {
					sb.Clear();
					sb.AppendWithDelimiter(i, DELIMITER);
					sb.AppendWithDelimiter(automata[i].IsInitial, DELIMITER);
					sb.AppendWithDelimiter(automata[i].IsAlive, DELIMITER);

					//sb.AppendWithDelimiter(automata[i].NextState, DELIMITER);
					//sb.AppendWithDelimiter(automata[i].NumNeighbors, DELIMITER);

					sw.WriteLine(sb.ToString());
				}
			}
		}

		private string GetDefaultFileName() {
			StringBuilder sb = new StringBuilder();
			sb.Append("data");
			sb.Append(DateTime.Now.ToString("yyMMddhhmmss"));
			sb.Append(".gol");
			return sb.ToString();
		}

		private void LoadStateFromFile(string fileName) {
			string fp = fileName;

			try {
				using (StreamReader sr = File.OpenText(fp)) {
					string coords = sr.ReadLine();
					string[] coords_arr = coords.Split(DELIMITER);

					txtWidth.Text = coords_arr[0];
					txtHeight.Text = coords_arr[1];
					UpdateRowAndColumnSize();
					ResizeGoL();
				
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
				sberr.AppendLine("Error in loading state:");
				sberr.AppendLine(e.Message);
				sberr.AppendLine("Stacktrace:");
				sberr.AppendLine(e.StackTrace);

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

	public static class StringBuilderExt {
		public static void AppendWithDelimiter(this StringBuilder sb, object msg, object del) {
			sb.Append(msg);
			sb.Append(del);
		}
	}
}
