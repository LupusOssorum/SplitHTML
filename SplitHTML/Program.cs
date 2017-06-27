using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DocoptNet;

namespace SplitHTML {
	class Program {

		const string usage = @"Split Html
		Usage:
			SplitHTML compile <file> [<rootDir>]
			SplitHTML compileDir <dir> [<rootDir>]
		";

		static void Main(string[] args) {
			var arguments = new Docopt().Apply(usage, args, exit:true);

			if (arguments["compile"].IsTrue) {
				if (arguments["<rootDir>"] !=null) {
					Directory.SetCurrentDirectory(arguments["<rootDir>"].ToString());
				}
				Compiler.CompileFile(arguments["<file>"].ToString());
			} else if (arguments["compileDir"].IsTrue) {
				if (arguments["<rootDir>"] != null) {
					Directory.SetCurrentDirectory(arguments["<rootDir>"].ToString());
				} else {
					Directory.SetCurrentDirectory(arguments["<dir>"].ToString());
				}
				Compiler.CompileDir(arguments["<dir>"].ToString());
			}
			
		}


	}
}
