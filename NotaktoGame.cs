using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardGamesFramework
{
    public class NotaktoGame : BaseGame
    {
        private Board[] boards = new Board[3];
        private const string FILENAME = "notakto_save.txt";

        public override void InitializeGame()
        {
            for (int i = 0; i < 3; i++)
                boards[i] = new Board(3);

            player1 = new HumanPlayer("Player 1");
            player2 = isHumanVsComputer ? new ComputerPlayer("Computer") : new HumanPlayer("Player 2");
        }

        public override BasePlayer GetCurrentPlayer() => isPlayerOneTurn ? player1 : player2;

        public override void DisplayBoard()
        {
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine($"\nBoard {i + 1}:");
                boards[i].Display();
            }
        }

        public override bool IsGameOver()
        {
            return IsBoardComplete(boards[0]) &&
                   IsBoardComplete(boards[1]) &&
                   IsBoardComplete(boards[2]);
        }

        private bool IsBoardComplete(Board b)
        {
            for (int i = 0; i < 3; i++)
            {
                if (b.GetCell(i, 0) == "X" && b.GetCell(i, 1) == "X" && b.GetCell(i, 2) == "X") return true;
                if (b.GetCell(0, i) == "X" && b.GetCell(1, i) == "X" && b.GetCell(2, i) == "X") return true;
            }
            if (b.GetCell(0, 0) == "X" && b.GetCell(1, 1) == "X" && b.GetCell(2, 2) == "X") return true;
            if (b.GetCell(0, 2) == "X" && b.GetCell(1, 1) == "X" && b.GetCell(2, 0) == "X") return true;
            return false;
        }

        public override void MakeMove(BasePlayer player)
        {
            if (player is ComputerPlayer)
            {
                for (int b = 0; b < 3; b++)
                {
                    if (IsBoardComplete(boards[b])) continue;

                    for (int r = 0; r < 3; r++)
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            if (boards[b].IsEmpty(r, c))
                            {
                                boards[b].SetCell(r, c, "X");
                                if (!IsBoardComplete(boards[b]))
                                {
                                    // Try winning move - doesn’t complete board
                                    return;
                                }

                                // Undo move
                                boards[b].SetCell(r, c, null);
                            }
                        }
                    }
                }
                // If no safe move found, make any valid move
                for (int b = 0; b < 3; b++)
                {
                    if (IsBoardComplete(boards[b])) continue;

                    for (int r = 0; r < 3; r++)
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            if (boards[b].IsEmpty(r, c))
                            {
                                boards[b].SetCell(r, c, "X");
                                return;
                            }
                        }
                    }
                }

                Console.WriteLine("No valid moves for computer.");
            }
            else
            {
                Console.Write("Enter board,row,col (1-based index): ");
                var input = Console.ReadLine().Split(',');
                int bIndex = int.Parse(input[0]) - 1;
                int r = int.Parse(input[1]) - 1;
                int c = int.Parse(input[2]) - 1;

                if (bIndex < 0 || bIndex >= 3 || r < 0 || r >= 3 || c < 0 || c >= 3)
                {
                    Console.WriteLine("Invalid input.");
                    return;
                }

                if (IsBoardComplete(boards[bIndex]))
                {
                    Console.WriteLine("Board is already completed.");
                    return;
                }

                if (!boards[bIndex].IsEmpty(r, c))
                {
                    Console.WriteLine("Cell is not empty.");
                    return;
                }

                boards[bIndex].SetCell(r, c, "X");
                if (isPlayerOneTurn)
                {
                    undoStackP1.Push($"{bIndex},{r},{c}");
                    redoStackP1.Clear();
                }
                else
                {
                    undoStackP2.Push($"{bIndex},{r},{c}");
                    redoStackP2.Clear();
                }


            }
        }

        public override void Undo()
        {
            var targetStack = isPlayerOneTurn ? undoStackP1 : undoStackP2;
            var redoStack = isPlayerOneTurn ? redoStackP1 : redoStackP2;

            if (targetStack.Count == 0 || isHumanVsComputer)
            {
                Console.WriteLine("Nothing to undo.");
                return;
            }
            var move = targetStack.Pop().Split(',');
            int b = int.Parse(move[0]), r = int.Parse(move[1]), c = int.Parse(move[2]);
            boards[b].SetCell(r, c, null);
            redoStack.Push($"{b},{r},{c}");
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
            int b = int.Parse(move[0]), r = int.Parse(move[1]), c = int.Parse(move[2]);
            boards[b].SetCell(r, c, "X");
            undoStack.Push($"{b},{r},{c}");
        }

        public override void SaveGame()
        {
            string state = $"{isHumanVsComputer}|{isPlayerOneTurn}|" +
               $"{string.Join(";", undoStackP1)}|" +
               $"{string.Join(";", undoStackP2)}";
            GameState.Save(FILENAME, state);
            GameState.Save(FILENAME, state);
        }

        public override void LoadGame()
        {
            string? state = GameState.Load(FILENAME);
            if (string.IsNullOrEmpty(state))
                throw new InvalidOperationException("No saved game.");

            boards = new Board[3];
            for (int i = 0; i < 3; i++)
                boards[i] = new Board(3);

            var parts = state.Split('|');
            isHumanVsComputer = bool.Parse(parts[0]);
            isPlayerOneTurn = bool.Parse(parts[1]);
            undoStackP1 = parts.Length > 2 ? new Stack<string>(parts[4].Split(';')) : new Stack<string>();
            undoStackP2 = parts.Length > 3 ? new Stack<string>(parts[5].Split(';')) : new Stack<string>();

            foreach (var move in undoStackP1.Concat(undoStackP2))
            {
                var m = move.Split(',');
                int b = int.Parse(m[0]), r = int.Parse(m[1]), c = int.Parse(m[2]);
                boards[b].SetCell(r, c, "X");
            }

            player1 = new HumanPlayer("Player 1");
            player2 = isHumanVsComputer ? new ComputerPlayer("Computer") : new HumanPlayer("Player 2");
        }



        public override void AnnounceResult()
        {
            Console.WriteLine($"{(isPlayerOneTurn ? player2.Name : player1.Name)} wins! (Other player made last move)");
        }
    }
}
