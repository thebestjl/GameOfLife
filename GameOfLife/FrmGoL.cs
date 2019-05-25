using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameOfLife {
	public partial class FrmGoL : Form {
		private CellularAutomaton[,] automata;
		private int dgvRow;
		private int dgvCol;
		private bool running;

		public FrmGoL() {
			InitializeComponent();
			SetUpControls();
		}

		#region Event Handler Functions
		private void dgvCells_CellClick(object sender, DataGridViewCellEventArgs e) {
			dgvCells.ClearSelection();
			ToggleLife(e.ColumnIndex, e.RowIndex, !automata[e.ColumnIndex, e.RowIndex].IsAlive);
		}

		private void TxtHeight_TextChanged(object sender, System.EventArgs e) {
			WidthHeightChanged(txtHeight);
		}

		private void TxtWidth_TextChanged(object sender, System.EventArgs e) {
			WidthHeightChanged(txtWidth);
		}

		private void btnStart_Click(object sender, EventArgs e) {
			if (!running) {
				running = true;
				PrintCellData();
				RunProgram();
			} else {
				running = false;
			}
		}
		#endregion

		#region Helper Functions
		private async void RunProgram() {
			await Task.Run(() => {
				while (running) {
					UpdateLiveCells();
					PrintCellData();
					System.Threading.Thread.Sleep(1000);
				}
				return;
			});
		}

		private void SetUpControls() {
			txtWidth.TextChanged += TxtWidth_TextChanged;
			txtHeight.TextChanged += TxtHeight_TextChanged;

			running = false;
			txtWidth.Text = "10";
			txtHeight.Text = "10";
		}

		private void InitializeAutomata() {
			automata = new CellularAutomaton[dgvCol, dgvRow];

			for (int i = 0; i < dgvCol; i++) {
				for (int j = 0; j < dgvRow; j++) {
					automata[i, j] = new CellularAutomaton();
					Size size = dgvCells.Size;
					size.Height = 10;
					dgvCells.Size = size;
				}
			}

			PrintCellData();
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
					dgvCells[col, row].Style.BackColor = Color.Red;
				}
				else {
					dgvCells[col, row].Style.BackColor = Color.Green;
				}
			}
		}

		private void PrintCellData() {
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
		#endregion
	}
}
