using System.Text;

namespace GameOfLife {
	class CellularAutomaton {
		public bool IsInitial { get; set; }
		public bool IsAlive { get; set;  }
		public bool NextState { get; set; }
		public int NumNeighbors { get; private set; }

		private const bool Alive = true;
		private const bool Dead = false;

		public CellularAutomaton() {
			IsInitial = Dead;
			IsAlive = Dead;
			NextState = Dead;

			NumNeighbors = 0;
		}

		public void AddNeighbor() {
			NumNeighbors++;

			SetNextState();
		}

		public void SubNeighbor() {
			NumNeighbors--;

			SetNextState();
		}

		private void SetNextState() {
			if (IsAlive && NumNeighbors < 2) {
				NextState = Dead;
			} else if (IsAlive && (NumNeighbors == 2 || NumNeighbors == 3)) {
				NextState = Alive;
			} else if (IsAlive && NumNeighbors > 3) {
				NextState = Dead;
			} else if (!IsAlive && NumNeighbors == 3) {
				NextState = Alive;
			} else {
				NextState = Dead;
			}
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder();

			sb.Append("{ { ");
			sb.Append(IsInitial == true ? 1 : 0);
			sb.Append(" }; { ");
			sb.Append(IsAlive == true ? 1 : 0);
			sb.Append(" }; { ");
			sb.Append(NextState == true ? 1 : 0);
			sb.Append(" }; { ");
			sb.Append(NumNeighbors);
			sb.Append(" }; }");

			return sb.ToString();
		}

		public string ToStringLong() {
			StringBuilder sb = new StringBuilder();

			sb.Append("{ { init: ");
			sb.Append(IsInitial == true ? 1 : 0);
			sb.Append(" }; { alive: ");
			sb.Append(IsAlive == true ? 1 : 0);
			sb.Append(" }; { next: ");
			sb.Append(NextState == true ? 1 : 0);
			sb.Append(" }; { neighbors: ");
			sb.Append(NumNeighbors);
			sb.Append(" }; }");

			return sb.ToString();
		}
	}
}
