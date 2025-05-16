using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardGamesFramework
{
    public class GomokuGame : BaseGame
    {
        private Board board;
        private const int SIZE = 15;
        private const int WIN_COUNT = 5;
        private const string FILENAME = "gomoku_save.txt";

        public override void InitializeGame()
        {
            board = new Board(SIZE);
            player1 = new HumanPlayer("Player 1 (X)");
            player2 = isHumanVsComputer ? new ComputerPlayer("Computer (O)") : new HumanPlayer("Player 2 (O)");
        }

        public override BasePlayer GetCurrentPlayer() => isPlayerOneTurn ? player1 : player2;

        public override void DisplayBoard() => board.Display();

        public override bool IsGameOver()
        {
            return board.IsFull() || CheckWin("X") || CheckWin("O");
        }

        private bool CheckWin(string symbol)
        {
            for (int r = 0; r < SIZE; r++)
            {
                for (int c = 0; c < SIZE; c++)
                {
                    if (CheckDirection(r, c, 1, 0, symbol) || // Horizontal
                        CheckDirection(r, c, 0, 1, symbol) || // Vertical
                        CheckDirection(r, c, 1, 1, symbol) || // Diagonal down-right
                        CheckDirection(r, c, 1, -1, symbol))  // Diagonal down-left
                        return true;
                }
            }
            return false;
        }

        private bool CheckDirection(int r, int c, int dr, int dc, string symbol)
        {
            for (int i = 0; i < WIN_COUNT; i++)
            {
                int nr = r + dr * i, nc = c + dc * i;
                if (nr < 0 || nc < 0 || nr >= SIZE || nc >= SIZE || board.GetCell(nr, nc) != symbol)
                    return false;
            }
            return true;
        }

        public override void MakeMove(BasePlayer player)
        {
            string symbol = isPlayerOneTurn ? "X" : "O";

            if (player is ComputerPlayer)
            {
                // Try to win immediately
                for (int r = 0; r < SIZE; r++)
                {
                    for (int c = 0; c < SIZE; c++)
                    {
                        if (board.IsEmpty(r, c))
                        {
                            board.SetCell(r, c, symbol); // O
                            if (CheckWin(symbol))
                                return;
                            board.SetCell(r, c, null); // Undo
                        }
                    }
                }

                // Try to block opponent from winning
                string opponentSymbol = "X"; 

                for (int r = 0; r < SIZE; r++)
                {
                    for (int c = 0; c < SIZE; c++)
                    {
                        if (board.IsEmpty(r, c))
                        {
                            board.SetCell(r, c, opponentSymbol);
                            if (CheckWin(opponentSymbol))
                            {
                                board.SetCell(r, c, symbol); // Block with O
                                return;
                            }
                            board.SetCell(r, c, null); // Undo
                        }
                    }
                }

                // Else, make a first available move
                for (int r = 0; r < SIZE; r++)
                {
                    for (int c = 0; c < SIZE; c++)
                    {
                        if (board.IsEmpty(r, c))
                        {
                            board.SetCell(r, c, symbol);
                            return;
                        }
                    }
                }


                Console.WriteLine("No valid move available.");
            }
            else
            {
                Console.Write("Enter row,col: ");
                var input = Console.ReadLine().Split(',');
                int r = int.Parse(input[0]) - 1;
                int c = int.Parse(input[1]) - 1;

                if (r < 0 || r >= SIZE || c < 0 || c >= SIZE || !board.IsEmpty(r, c))
                {
                    Console.WriteLine("Invalid move.");
                    return;
                }

                board.SetCell(r, c, symbol);
                if (isPlayerOneTurn)
                {
                    undoStackP1.Push($"{r},{c}");
                    redoStackP1.Clear();
                }
                else
                {
                    undoStackP2.Push($"{r},{c}");
                    redoStackP2.Clear();
                }
            }
        }

        public override void Undo()
        {
            var targetStack = isPlayerOneTurn ? undoStackP1 : undoStackP2;
            var redoStack = isPlayerOneTurn ? redoStackP1 : redoStackP2;

            if (targetStack.Count == 0)
            {
                Console.WriteLine("Nothing to undo.");
                return;
            }

            var move = targetStack.Pop().Split(',');
            int r = int.Parse(move[0]), c = int.Parse(move[1]);
            board.SetCell(r, c, null);
            redoStack.Push($"{r},{c}");
            return;
        }

        public override void Redo()
        {
            var targetStack = isPlayerOneTurn ? redoStackP1 : redoStackP2;
            var undoStack = isPlayerOneTurn ? undoStackP1 : undoStackP2;


            if (targetStack.Count == 0 || isHumanVsComputer)
            {
                Console.WriteLine("Nothing to redo.");
                return;
            }
            var move = targetStack.Pop().Split(',');
            int r = int.Parse(move[0]), c = int.Parse(move[1]);
            string symbol = isPlayerOneTurn ? "X" : "O";
            board.SetCell(r, c, symbol);
            undoStack.Push($"{r},{c}");
        }

        public override void SaveGame()
        {
            string state = $"{isHumanVsComputer}|{isPlayerOneTurn}|" +
               $"{string.Join(";", undoStackP1)}|" +
               $"{string.Join(";", undoStackP2)}";
            GameState.Save(FILENAME, state);
        }

        public override void LoadGame()
        {
            string? state = GameState.Load(FILENAME);
            if (string.IsNullOrEmpty(state))
                throw new InvalidOperationException("No saved game.");

            board = new Board(SIZE);
            var parts = state.Split('|');
            isHumanVsComputer = bool.Parse(parts[0]);
            isPlayerOneTurn = bool.Parse(parts[1]);

            undoStackP1 = parts.Length > 2 ? new Stack<string>(parts[4].Split(';')) : new Stack<string>();
            undoStackP2 = parts.Length > 3 ? new Stack<string>(parts[5].Split(';')) : new Stack<string>();

            foreach (var move in undoStackP1.Concat(undoStackP2))
            {
                var m = move.Split(',');
                int r = int.Parse(m[0]), c = int.Parse(m[1]);
                string symbol = isPlayerOneTurn ? "O" : "X"; // Alternate each time
                board.SetCell(r, c, symbol);
                isPlayerOneTurn = !isPlayerOneTurn;
            }

            player1 = new HumanPlayer("Player 1 (X)");
            player2 = isHumanVsComputer ? new ComputerPlayer("Computer (O)") : new HumanPlayer("Player 2 (O)");
        }



        public override void AnnounceResult()
        {
            if (CheckWin("X")) Console.WriteLine("Player 1 (X) wins!");
            else if (CheckWin("O")) Console.WriteLine("Player 2 (O) wins!");
            else Console.WriteLine("It's a draw.");
        }
    }


}
