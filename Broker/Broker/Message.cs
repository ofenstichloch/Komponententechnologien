using System;
using System.Text;
namespace Broker
{
	public class Message
	{

		public enum Instruction {None, Generated, Solved, Register, Unregister};
        public enum Type { Broker, Generator, Solver, GUI};
		public Instruction instruction { get; set;}
		//Additional Info (how many solutions are there)
		public Type info { get; set; }
		public int[][] sudoku { get; set; }
		//URI of sender
		public string origin { get; set; }

		public String printSudoku()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append('-', sudoku.Length * 2);
			for (int i = 0; i < sudoku.Length; i++)
			{
				sb.AppendLine();
				sb.Append("|");
				for (int j = 0; j < sudoku.Length; j++)
				{
					sb.Append(sudoku[i][j].ToString() + "|");	
				}
				sb.AppendLine();
				sb.Append('-', sudoku.Length * 2);
			}
			return sb.ToString();
		}
	}
}
