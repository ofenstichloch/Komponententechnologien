using System.IO;
using System.Net;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Net.Http;

namespace Solver
{
    class Solver
    {
        HttpListener listener;
		const string MYURI = "http://127.0.0.1:10093/";
		const string MYCAMELURI = "http://127.0.0.1:10094/send";
        int[][] currentField;
        public Solver()
        {
            listener = new HttpListener();
			listener.Prefixes.Add(MYURI);
            listener.Start();
            listen();
        }

        public void listen()
        {
            while (true)
            {
                HttpListenerContext c = listener.GetContext();
               
                HttpListenerRequest req = c.Request;
                string json = new StreamReader(req.InputStream).ReadToEnd();
                Message m = JsonConvert.DeserializeObject<Message>(json);
                if(m.instruction != Message.Instruction.Generated)
                {
                    c.Response.StatusCode = 400;
                    c.Response.Close();
                }
                else
                {
                    c.Response.StatusCode = 200;
                    c.Response.Close();
                    currentField = m.sudoku;
                    Console.Out.WriteLine("Checking Sudoku");
					Message response = new Message();
					response.instruction = Message.Instruction.Solved;
					response.origin = "restlet://"+MYURI;
					if (checkValidity(m.sudoku))
					{
						//send it back
						Console.WriteLine(printSudoku(currentField));
						response.info = Message.Info.ValidSolution;
						response.sudoku = currentField;
					}
					else {
						response.info = Message.Info.InvalidSolution;
						response.sudoku = m.sudoku;
					}
					json = "";
					json = JsonConvert.SerializeObject(response);
					//send to camel instance

                    using (var client = new HttpClient())
					{
						var content = new StringContent(json);
						client.PostAsync(MYCAMELURI, content);

					}
                }
            }
        }

        public String printSudoku(int[][] sudoku)
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

        public bool checkValidity(int[][] sudoku)
        {
            int[][] s = new int[sudoku.Length][];
            for(int i = 0; i < s.Length; i++)
            {
                s[i] = new int[sudoku.Length];
                sudoku[i].CopyTo(s[i],0);
            }
            //CHECK CONSTRAINTS
            //RETURN FALSE ON FAIL
            //RETURN TRUE ON FULL FIELD WITHOUT ERROR
         
            //Rows
            for(int i = 0; i < s.Length; i++)
            {
                bool[] numbers = new bool[s.Length];
                for (int j = 0; j < s.Length; j++)
                {
                    if (s[i][j] != 0)
                    {
                        if (!numbers[s[i][j] - 1]) numbers[s[i][j] - 1] = true;
                        else return false;
                    }
                }
            }
            //Columns
            for (int i = 0; i < s.Length; i++)
            {
                bool[] numbers = new bool[s.Length];
                for (int j = 0; j < s.Length; j++)
                {
                    if (s[j][i] != 0)
                    {
                        if (!numbers[s[j][i] - 1]) numbers[s[j][i] - 1] = true;
                        else return false;
                    }
                }
            }
            //Boxes
            int length = (int) Math.Sqrt(sudoku.Length);
            for(int i = 0;i< sudoku.Length; i = i + length)
            {
                for(int j = 0;j<sudoku.Length; j = j + length)
                {
                    bool[] numbers = new bool[s.Length];
                    for (int k= i; k < i + length; k++)
                    {
                        for(int l= j; l < j + length; l++)
                        {
                            if (s[k][l] != 0)
                            {
                                if (!numbers[s[k][l] - 1]) numbers[s[k][l] - 1] = true;
                                else return false;
                            }
                        }
                    }
                }
            }

            //Take next free spot
            int x = 0, y = 0;
            bool found = false;
            //search next free slot
            for (int i = 0; i < s.Length; i++)
            {
                for (int j = 0; j < s.Length; j++)
                {
                    if (s[i][j] == 0)
                    {
                        y = i;
                        x = j;
                        found = true;
                        break;
                    }
                }
                if (found) break;
            }
            if (!found)
            {
                currentField = s;
                return true;
            }
            //Fill this spot
            for(int i = 1; i <= s.Length; i++)
            {
                s[y][x] = i;
                if (checkValidity(s)) return true;
            }
            return false;
        }

    }
}
