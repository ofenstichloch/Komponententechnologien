using System;
using System.Text;
namespace Broker
{
    public class Message
    {

        public abstract class Instruction
        {
            public const string SOLVE = "solve";
            public const string REGISTER = "register";
            public const string UNREGISTER = "unregister";
            public const string SOLVED = "solved";
            public const string PING = "ping";
            public const string PONG = "pong";
        }
        public Guid request_id {get;set;}
		public string instruction { get; set;}
		public int[] sudoku { get; set; }
		//URI of sender
		public string sender { get; set; }

		public String printSudoku()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append('-', sudoku.Length * 2);
            sb.AppendLine();
			for (int i = 0; i < sudoku.Length; i++)
			{
				sb.Append("|");
                sb.Append(sudoku[i]);
                if ((i+1) % Math.Sqrt(sudoku.Length) == 0){
                    sb.AppendLine();
                    sb.Append('-', sudoku.Length * 2);
                    sb.AppendLine();
                }
				
			}
			return sb.ToString();
		}
	}
}
