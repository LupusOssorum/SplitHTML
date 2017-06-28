using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplitHTML {
	class Compiler {
		//-----static
		
		//---vars

		static string splitHtmlExtention = ".splithtml";
		static string htmlPartExtention = ".htmlpart";

		const string COMMENT_HEADER = "<!--htmlpart:";
		const string COMMENT_IF_HEADER = "<!--htmlpart if:";
		const string COMMENT_CLOSER = "-->";

		const string COMMENT_ENDIF = "<!--htmlpart endif-->";

		//---methods

		public static void CompileDir(string dirPath) {
			Log.Write("Compiling Dir: " + Directory.GetCurrentDirectory());
			Log.depth++;

			string[] files = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(),dirPath), "*"+splitHtmlExtention);
			foreach (string file in files) {
				CompileFile(file);
			}

			Log.depth--;
			Log.Write("Finished Compiling Dir: " + Directory.GetCurrentDirectory() + "\n");
		}

		public static void CompileFile(string fileName) {
			using (var writer = new StreamWriter(File.Create(fileName.Substring(0, fileName.Length - splitHtmlExtention.Length) + ".html"))) {
				new Compiler(fileName).Compile(writer);
			}
		}
		





























		//-----instance

		//---vars
		string fileName;

		//---constuctor
		public Compiler(string fileName) {
			this.fileName = fileName;
		}

		//---methods
		public void Compile(StreamWriter writer, string lineHead = "", Dictionary<string, string> args = null) {
			//---LOGGING
			Log.Write("Compiling: " + fileName);
			Log.depth++;

			//---SETTUP
			fileName = AddFileNameExtention(fileName, htmlPartExtention);

			uint ifTagDepth = 0;

			//---MAIN

			using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
				using (StreamReader reader = new StreamReader(stream)) {
					while (!reader.EndOfStream) {


						//---SETTUP
						string line = reader.ReadLine();

						string partFileName;
						string additionalLineHead;
						Dictionary<string, string> newFileArgs;

						bool keepContent;//out from function if line is a split html if line

						{
							//---HTML part
							if (CompileFile_SpliceHtmlpartLine(line, out partFileName, out additionalLineHead, out newFileArgs)) {
								Compiler newFileCompiler = new Compiler(partFileName);

								writer.WriteLine(lineHead + additionalLineHead + "<!--" + partFileName + "-->");
								newFileCompiler.Compile(writer, lineHead + additionalLineHead, newFileArgs);
								writer.WriteLine(lineHead + additionalLineHead + "<!--/" + partFileName + "-->");
							}

							//---if splitHTML condition
							else if (CompileFile_SpliceIfLine(line, args, out keepContent)) {
								//---KEEP IF
								if (keepContent) {
									/* keep code inside slit if
									 */
									ifTagDepth++;
								}
								//---EXCLUDE IF
								else {
									/* go until out of excuded if
									 */
									uint inactiveDepth = 1;
									while (!reader.EndOfStream && inactiveDepth > 0) {
										line = reader.ReadLine();
										if (CompileFile_SpliceIfLine_GetIfIfLine(line)) {
											inactiveDepth++;
										} else if (CompileFile_SpliceIfLine_GetIfIfCloseLine(line)) {
											inactiveDepth--;
										}
									}
								}
								//---
							}

							//---if exiting split condition
							else if (CompileFile_SpliceIfLine_GetIfIfCloseLine(line)) {
								ifTagDepth--;
							}
							//---nothing else keep line from file
							else {
								writer.WriteLine(lineHead + line);
							}
							//---
						}


					}// while !EndOfStream
				}// using reader
			}// using stream

			//---LOGGING
			Log.depth--;
			Log.Write("Finished Compiling: " + fileName);
		}// method Compile


		//-----private instances
		private bool CompileFile_SpliceHtmlpartLine(string line, out string fileName, out string additionalLineHead, out Dictionary<string, string> args) {
			int startIndex = line.IndexOf(COMMENT_HEADER);

			if (startIndex == -1) {
				fileName = null;
				additionalLineHead = null;
				args = null;
				return false;
			}

			additionalLineHead = line.Substring(0, startIndex);

			{
				string htmlpartData = line.Substring(startIndex + COMMENT_HEADER.Length, line.IndexOf(COMMENT_CLOSER) - startIndex - COMMENT_HEADER.Length);
				fileName = CompileFile_SpliceHtmlpartLine_GetFileName(htmlpartData, out args);
			}
			return true;
		}

		/// <summary>
		/// Give the data part of an htmlpat tag.  Will return the fileName and calculate the args.
		/// </summary>
		/// <param name="htmlpartData">The data part of htmlpart tag</param>
		/// <param name="args">The calculated args</param>
		/// <returns></returns>
		private string CompileFile_SpliceHtmlpartLine_GetFileName(string htmlpartData, out Dictionary<string, string> args) {
			int breakIndex = htmlpartData.IndexOf(':');
			if (breakIndex == -1) {
				args = null;
				return htmlpartData;
			} else {
				string[] rawArgs = htmlpartData.Substring(breakIndex + 1).Split(',');
				args = new Dictionary<string, string>();
				{//---calc args

					// will be set to -1 when default is not valid
					int currentDefaultName = 0;// if no name is given for arg

					foreach (string rawArg in rawArgs) {
						int equalIndex = rawArg.IndexOf('=');
						if (equalIndex == -1) {
							if (currentDefaultName == -1) {
								throw new CompileException("Error in Compile: Cannot have default arg name after named arg.");
							}
							args[currentDefaultName.ToString()] = rawArg;
							currentDefaultName++;
						} else {
							args[rawArg.Substring(0, equalIndex)] = rawArg.Substring(equalIndex + 1);
						}
					}

				}

				return htmlpartData.Substring(0, breakIndex);
			}
		}

		/// <summary>
		/// Adds default extention if has no extention.
		/// </summary>
		/// <returns></returns>
		private string AddFileNameExtention(string fileName, string extention) {
			string justName = fileName;// fileName without folder path
									   //---get name without path
			{
				int beginningFileNameIndex = Math.Max(justName.LastIndexOf('/'), justName.LastIndexOf('\\'));

				if (beginningFileNameIndex != -1) {
					justName = fileName.Substring(beginningFileNameIndex);
				}
			}
			//---check for existing file extension
			{
				int dotIndex = justName.IndexOf('.');
				if (justName.IndexOf('.') == -1) {
					return fileName + extention;
				} else {
					return fileName;
				}
			}
		}

		private bool CompileFile_SpliceIfLine(string line, Dictionary<string, string> args, out bool keepContent) {
			int startIndex;//// = line.IndexOf(COMMENT_IF_HEADER);

			if (!CompileFile_SpliceIfLine_GetIfIfLine(line, out startIndex)) {
				keepContent = true;// n/a
				return false;
			}

			string conditionString = line.Substring(startIndex + COMMENT_IF_HEADER.Length, line.IndexOf(COMMENT_CLOSER) - startIndex - COMMENT_IF_HEADER.Length);
			keepContent = CompileFile_SpliceIfLine_CalcCondition(conditionString, args);
			return true;
		}

		// Override is used during inactive line skipping in CompileFile
		private bool CompileFile_SpliceIfLine_GetIfIfLine(string line) { int _; return CompileFile_SpliceIfLine_GetIfIfLine(line, out _); }
		private bool CompileFile_SpliceIfLine_GetIfIfLine(string line, out int startIndex) {
			startIndex = line.IndexOf(COMMENT_IF_HEADER);
			return startIndex != -1;
		}
		private bool CompileFile_SpliceIfLine_GetIfIfCloseLine(string line) {
			return line.IndexOf(COMMENT_ENDIF) != -1;
		}


		private bool CompileFile_SpliceIfLine_CalcCondition(string conditionString, Dictionary<string, string> args) {
			Log.Write("Calculating Condition: " + conditionString);
			NCalc.Expression ex = new NCalc.Expression(conditionString);
			//string[] s = ex.Parameters.;
			foreach (string key in args.Keys) {
				ex.Parameters[key] = args[key];
			}

			bool result = (bool)ex.Evaluate();

			Log.Write("     = " + result);
			return result;
		}


	}
}
