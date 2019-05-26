using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife {
	class DescendingComparer : IComparer<int> {
		public int Compare (int x, int y) {
			int ascendingResult = Comparer<int>.Default.Compare(x, y);

			return 0 - ascendingResult;
		}
	}
}
