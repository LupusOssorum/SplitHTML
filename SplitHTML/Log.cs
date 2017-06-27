using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplitHTML {
	static class Log {
		public static int depth;

		const string indent = "  ";

		public static void Write(string line, bool newLine=true) {
			for (int i = 0; i < depth; i++) {
				Console.Write(indent);
			}
			if (newLine) {
				Console.WriteLine(line);
			} else {
				Console.Write(line);
			}
		}
	}
}
