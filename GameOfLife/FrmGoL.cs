using System;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameOfLife {
	public partial class FrmGoL : Form {
		private CellularAutomaton[,] automata;
		private int dgvRow;
		private int dgvCol;
		private bool running;
		private bool finished_running;
		private const bool display_cell_info = false;
		private int run_speed;
		private const int RANDOMIZE_LIFE_THRESHOLD = 750;

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
			automata = new CellularAutomaton[dgvCol, dgvRow];

			for (int i = 0; i < dgvCol; i++) {
				for (int j = 0; j < dgvRow; j++) {
					automata[i, j] = new CellularAutomaton();
					ToggleLife(i, j, false);
				}
			}

			PrintCellData();
		}

		#region Event Handler Functions
		private void dgvCells_CellClick(object sender, DataGridViewCellEventArgs e) {
			dgvCells.ClearSelection();

			if (!running) {
				automata[e.ColumnIndex, e.RowIndex].IsInitial = !automata[e.ColumnIndex, e.RowIndex].IsAlive;
			}

			ToggleLife(e.ColumnIndex, e.RowIndex, !automata[e.ColumnIndex, e.RowIndex].IsAlive);
		}

		private void TxtHeight_TextChanged(object sender, EventArgs e) {
			WidthHeightChanged(txtHeight);
		}

		private void TxtWidth_TextChanged(object sender, EventArgs e) {
			WidthHeightChanged(txtWidth);
		}

		private void nudSpeed_ValueChanged(object sender, EventArgs e) {
			run_speed = (int) (1 / nudSpeed.Value) * 1000;
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
			for (int i = 0; i < dgvCol; i++) {
				for (int j = 0; j < dgvRow; j++) {
					ToggleLife(i, j, automata[i, j].IsInitial);
				}
			}
			PrintCellData();
		}

		private void btnRandomize_Click(object sender, EventArgs e) {
			RandomizeCells();
		}
		#endregion

		#region Helper Functions
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
			
			for (int i = 0; i < dgvCol; i++) {
				for (int j = 0; j < dgvRow; j++) {
					bool random_life;

					if (random.Next(1, 1000) > RANDOMIZE_LIFE_THRESHOLD) {
						random_life = true;
					}
					else {
						random_life = false;
					}

					automata[i, j].IsInitial = random_life;
					ToggleLife(i, j, random_life);
				}
			}
			running = resume_running;
			if (running) {
				RunProgram();
			}
		}

		private void UpdateRowAndColumnSize() {
			int.TryParse(txtHeight.Text, out dgvRow);
			int.TryParse(txtWidth.Text, out dgvCol);
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

		private void ToggleLife(int col, int row, bool new_state) {
			UpdateCellColor(col, row, new_state);
			if (automata[col, row].IsAlive != new_state) {
				automata[col, row].IsAlive = new_state;

				for (int i = col - 1; i <= col + 1; i++) {
					for (int j = row - 1; j <= row + 1; j++) {
						if (!(i == col && j == row)) {
							int row_ind = GetBoundry(j, dgvRow);
							int col_ind = GetBoundry(i, dgvCol);

							if (new_state)
								automata[col_ind, row_ind].AddNeighbor();
							else
								automata[col_ind, row_ind].SubNeighbor();
						}
					}
				}
			}
		}

		private int GetBoundry(int val, int size) {
			int ret_val = val - (val / size) * size;
			return ret_val >= 0 ? ret_val : size + ret_val;
		}

		private void UpdateLiveCells() {
			for (int i = 0; i < dgvCol; i++) {
				for (int j = 0; j < dgvRow; j++) {
					bool new_state = automata[i, j].NextState;
					ToggleLife(i, j, new_state);
				}
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

		private bool GridHasValidDimesions() {
			bool validColSize = System.Text.RegularExpressions.Regex.IsMatch(txtHeight.Text, "0*[1-9][0-9]*");
			bool validRowSize = System.Text.RegularExpressions.Regex.IsMatch(txtWidth.Text, "0*[1-9][0-9]*");

			return validColSize && validRowSize;
		}

		private void UpdateCellColor(int col, int row, bool new_state) {
			if (automata[col, row].IsAlive == new_state) {
				if (automata[col, row].IsAlive) {
					dgvCells[col, row].Style.BackColor = Color.Blue;
				} else {
					dgvCells[col, row].Style.BackColor = Color.White;
				}
			} else {
				if (automata[col, row].IsAlive) {
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

			int lblHeightY_1 = lblSpeedY + button_height;
			int lblHeightY_2 = btnStartY + dgvCells.Height - (3 * button_height);
			int lblHeightY = lblHeightY_1 >= lblHeightY_2 ? lblHeightY_1 : lblHeightY_2;
			int lblWidthY = lblHeightY + button_height;
			int btnRandomizeY = lblWidthY + button_height;

			btnStart.Location = new Point(btnStartX, btnStartY);
			btnReset.Location = new Point(btnStartX, btnResetY);
			btnClear.Location = new Point(btnStartX, btnClearY);
			lblSpeed.Location = new Point(btnStartX, lblSpeedY);
			nudSpeed.Location = new Point(nudSpeedX, lblSpeedY - 3);

			lblHeight.Location = new Point(btnStartX, lblHeightY);
			lblWidth.Location = new Point(btnStartX, lblWidthY);

			txtHeight.Location = new Point(nudSpeedX, lblHeightY - 3);
			txtWidth.Location = new Point(nudSpeedX, lblWidthY - 3);

			btnRandomize.Location = new Point(btnStartX, btnRandomizeY);

			int clientSizeHeight_1 = btnRandomizeY + button_height + 3;
			int clientSizeHeight_2 = dgvCells.Height + btnStartY;

			int clientSizeHeight = clientSizeHeight_1 > clientSizeHeight_2 ? clientSizeHeight_1 : clientSizeHeight_2;
			int clientSizeWidth = btnStartX + btnStart.Width + 3;
			ClientSize = new Size(clientSizeWidth + 20, clientSizeHeight + 50);
			MinimumSize = new Size(clientSizeWidth + 20, clientSizeHeight + 50);
			MaximumSize = new Size(clientSizeWidth + 20, clientSizeHeight + 50);
		}
		#endregion

		#region Console Functions
		private void PrintCellData() {
			if (display_cell_info) {
				StringBuilder sb = new StringBuilder();
				for (int j = 0; j < dgvRow; j++) {
					for (int i = 0; i < dgvCol; i++) {
						sb.Append(automata[i, j]);
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
