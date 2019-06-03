using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
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
		private CancellationTokenSource ctsCancel;
		private CancellationTokenSource ctsPause;

		public FrmGoL() {
			InitializeComponent();
			InitializeControls();
		}

		private void InitializeControls() {
			ctsCancel = new CancellationTokenSource();
			ctsPause = new CancellationTokenSource();
			txtWidth.TextChanged += TxtWidth_TextChanged;
			txtHeight.TextChanged += TxtHeight_TextChanged;

			txtWidth.LostFocus += txtWidth_FocusOut;
			txtHeight.LostFocus += txtHeight_FocusOut;

			nudSpeed.ValueChanged += nudSpeed_ValueChanged;
			txtWidth.Text = "10";
			txtHeight.Text = "10";

			WidthHeightChanged(txtHeight);
			WidthHeightChanged(txtWidth);

			run_speed = 1000;
			running = false;
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
			LoadStateFromFile();
		}
		#endregion

		#region async
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

		private async void RandomizeCells() {
			bool resume_running = running;
			Random random = new Random();

			await Task.Run(() => {
				running = false;
				while (!finished_running) {
					Thread.Sleep(0);
				}
			});

			int rowSize = random.Next(1, 30);
			int colSize = random.Next(1, 30);

			txtHeight.Text = rowSize.ToString();
			txtWidth.Text = colSize.ToString();
			UpdateRowAndColumnSize();
			ResizeGoL();

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
			for (int i = 0; i < dgvCol; i++) {
				UpdateLiveCellsAsync(i);
			}
		}

		private async void UpdateLiveCellsAsync(int col) {
			await Task.Run(() => {
				for (int j = GetLinearizedIndex(col, 0); j < GetLinearizedIndex(col, dgvRow); j++) {
					if (!ctsCancel.Token.IsCancellationRequested)
						ctsCancel.Token.ThrowIfCancellationRequested();

					bool new_state = automata[j].NextState;
					ToggleLife(ConvertLinearIndexToCoords(j), new_state);
				}
			});
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
			//textBox.Select(textBox.Text.Length, 0);
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
			string fp = Path.Combine(cur_directory, @"data.gol");
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
