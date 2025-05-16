using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardGamesFramework
{
    public class NumericalTicTacToe : BaseGame
    {
        private Board board;
        private bool[] usedNumbers;
        private int size;
        private int maxNum;
        private int targetSum;
        private const string FILENAME = "numerical_save.txt";

        public override void InitializeGame()
        {
            Console.Write("Enter board size (n): ");
            size = int.Parse(Console.ReadLine());
            maxNum = size * size;
            targetSum = size * (maxNum + 1) / 2;

            board = new Board(size);
            usedNumbers = new bool[maxNum + 1];

            player1 = new HumanPlayer("Player 1 (Odd)");
            player2 = isHumanVsComputer ? new ComputerPlayer("Computer (Even)") : new HumanPlayer("Player 2 (Even)");
        }

        public override BasePlayer GetCurrentPlayer() => isPlayerOneTurn ? player1 : player2;

        public override void DisplayBoard() => board.Display();

        public override bool IsGameOver() => board.IsFull() || CheckWin();

        public bool CheckWin()
        {
            for (int i = 0; i < size; i++)
                if (SumRow(i) == targetSum || SumCol(i) == targetSum)
                    return true;

            return SumMainDiag() == targetSum || SumAntiDiag() == targetSum;
        }

        private int SumRow(int row) => Sum(i => board.GetCell(row, i));
        private int SumCol(int col) => Sum(i => board.GetCell(i, col));
        private int SumMainDiag() => Sum(i => board.GetCell(i, i));
        private int SumAntiDiag() => Sum(i => board.GetCell(i, size - 1 - i));

        private int Sum(Func<int, string> selector)
        {
            int sum = 0;
            for (int i = 0; i < size; i++)
                if (int.TryParse(selector(i), out int num))
                    sum += num;
            return sum;
        }

        public bool IsValidMove(int row, int col, int num)
        {
            return row >= 1 && row <= size &&
                   col >= 1 && col <= size &&
                   num >= 1 && num <= maxNum &&
                   !usedNumbers[num] &&
                   board.IsEmpty(row - 1, col - 1) &&
                   ((isPlayerOneTurn && num % 2 == 1) || (!isPlayerOneTurn && num % 2 == 0));
        }

        public override void MakeMove(BasePlayer player)
        {
            if (player is ComputerPlayer)
            {
                // Try immediate winning move first
                for (int r = 1; r <= size; r++)
                {
                    for (int c = 1; c <= size; c++)
                    {
                        for (int num = 2; num <= maxNum; num += 2)
                        {
                            if (IsValidMove(r, c, num))
                            {
                                board.SetCell(r - 1, c - 1, num.ToString());
                                usedNumbers[num] = true;
                                return;
                            }
                        }
                    }
                }

                // If no winning move, pick random valid move
                var rnd = new Random();
                while (true)
                {
                    int row = rnd.Next(1, size + 1);
                    int col = rnd.Next(1, size + 1);
                    int num = rnd.Next(2, maxNum + 1);

                    if (num % 2 == 0 && IsValidMove(row, col, num))
                    {
                        board.SetCell(row - 1, col - 1, num.ToString());
                        usedNumbers[num] = true;
                        return;
                    }
                }
            }
            else
            {
                Console.Write("Enter row,col,num: ");
                var parts = Console.ReadLine()?.Split(',');

                if (parts?.Length != 3 ||
                    !int.TryParse(parts[0], out int r) ||
                    !int.TryParse(parts[1], out int c) ||
                    !int.TryParse(parts[2], out int n))
                {
                    Console.WriteLine("Invalid input format. Please use row,col,num (e.g., 1,1,3).");
                    return;
                }

                if (r < 1 || r > size || c < 1 || c > size)
                {
                    Console.WriteLine($"Row and column must be between 1 and {size}.");
                    return;
                }

                if (n < 1 || n > maxNum)
                {
                    Console.WriteLine($"Number must be between 1 and {maxNum}.");
                    return;
                }

                if (!board.IsEmpty(r - 1, c - 1))
                {
                    Console.WriteLine("That cell is already filled. Choose another.");
                    return;
                }

                if (usedNumbers[n])
                {
                    Console.WriteLine("That number has already been used.");
                    return;
                }

                if ((isPlayerOneTurn && n % 2 == 0) || (!isPlayerOneTurn && n % 2 == 1))
                {
                    Console.WriteLine(isPlayerOneTurn
                        ? "Player 1 must use odd numbers."
                        : "Player 2 must use even numbers.");
                    return;
                }

                board.SetCell(r - 1, c - 1, n.ToString());
                usedNumbers[n] = true;
                if (isPlayerOneTurn)
                {
                    undoStackP1.Push($"{r - 1},{c - 1},{n}");
                    redoStackP1.Clear();
                }
                else
                {
                    undoStackP2.Push($"{r - 1},{c - 1},{n}");
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
            string[] move = targetStack.Pop().Split(',');
            int r = int.Parse(move[0]), c = int.Parse(move[1]), n = int.Parse(move[2]);
            board.SetCell(r, c, null);
            usedNumbers[n] = false;
            redoStack.Push($"{r},{c},{n}");
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
            string[] move = targetStack.Pop().Split(',');
            int r = int.Parse(move[0]), c = int.Parse(move[1]), n = int.Parse(move[2]);
            if (!board.IsEmpty(r, c))
            {
                Console.WriteLine("Cannot redo. Cell already occupied.");
                return;
            }
            board.SetCell(r, c, n.ToString());
            usedNumbers[n] = true;
            undoStack.Push($"{r},{c},{n}");
        }

        public override void SaveGame()
        {
            string state = $"{size}|{isHumanVsComputer}|{isPlayerOneTurn}|" +
               $"{string.Join(",", usedNumbers.Select(b => b ? "1" : "0"))}|" +
               $"{string.Join(";", undoStackP1)}|" +
               $"{string.Join(";", undoStackP2)}";
            GameState.Save(FILENAME, state);
        }

        public override void LoadGame()
        {
            string? state = GameState.Load(FILENAME);
            if (string.IsNullOrEmpty(state))
                throw new InvalidOperationException("No saved game.");

            var parts = state.Split('|');
            size = int.Parse(parts[0]);
            isHumanVsComputer = bool.Parse(parts[1]);
            isPlayerOneTurn = bool.Parse(parts[2]);

            string[] used = parts[3].Split(',');
            usedNumbers = used.Select(x => x == "1").ToArray();

            undoStackP1 = parts.Length > 4 ? new Stack<string>(parts[4].Split(';')) : new Stack<string>();
            undoStackP2 = parts.Length > 5 ? new Stack<string>(parts[5].Split(';')) : new Stack<string>();

            maxNum = size * size;
            targetSum = size * (maxNum + 1) / 2;
            board = new Board(size);

            foreach (var move in undoStackP1.Concat(undoStackP2))
            {
                var m = move.Split(',');
                int r = int.Parse(m[0]), c = int.Parse(m[1]), n = int.Parse(m[2]);
                board.SetCell(r, c, n.ToString());
            }

            player1 = new HumanPlayer("Player 1 (Odd)");
            player2 = isHumanVsComputer ? new ComputerPlayer("Computer (Even)") : new HumanPlayer("Player 2 (Even)");
        }



        public override void AnnounceResult()
        {
            if (CheckWin())
                Console.WriteLine($"{(isPlayerOneTurn ? player1.Name : player2.Name)} wins!");
            else
                Console.WriteLine("It's a draw.");
        }
    }


}
