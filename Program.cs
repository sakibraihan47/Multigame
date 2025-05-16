namespace BoardGamesFramework
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("Welcome to the Board Game App!");
            Console.WriteLine("1. Numerical Tic Tac Toe");
            Console.WriteLine("2. Notakto");
            Console.WriteLine("3. Gomoku");

            Console.Write("Select a game (1-3): ");
            string choice = Console.ReadLine();

            BaseGame game = choice switch
            {
                "1" => new NumericalTicTacToe(),
                "2" => new NotaktoGame(),
                "3" => new GomokuGame(),
                _ => null
            };

            if (game == null)
            {
                Console.WriteLine("Invalid game selected.");
                return;
            }

            // Attempt load first
            bool loaded = false;
            Console.Write("Load saved game? (y/n): ");
            string load = Console.ReadLine()?.Trim().ToLower();

            if (load == "y")
            {
                try
                {
                    game.LoadGame();
                    loaded = true;
                }
                catch
                {
                    Console.WriteLine("No saved game found or load failed.");
                }
            }

            // Only ask game mode if not loaded
            if (!loaded)
            {
                Console.Write("Choose mode: 1 = Human vs Human, 2 = Human vs Computer: ");
                game.isHumanVsComputer = Console.ReadLine()?.Trim() == "2";
                game.InitializeGame();
            }

            game.Run();
        }
    }

}
