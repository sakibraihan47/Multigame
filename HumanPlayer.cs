using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardGamesFramework
{
    public class HumanPlayer : BasePlayer
    {
        public HumanPlayer(string name) : base(name) { }

        public override void MakeMove(BaseGame game)
        {
            Console.WriteLine($"\n{(game.IsPlayerOneTurn() ? "Player 1" : "Player 2")}'s turn ({game.GetCurrentPlayer().Name})");
            Console.Write("Enter command (move | save | undo | redo | help): ");
            string input = Console.ReadLine()?.Trim().ToLower();

            switch (input)
            {
                case "move":
                    game.MakeMove(this);
                    return;

                case "save":
                    game.SaveGame();
                    return;

                case "undo":
                    game.Undo();
                    return;

                case "redo":
                    game.Redo();
                    return;

                case "help":
                    game.ShowHelp();
                    break;

                default:
                    Console.WriteLine("Unknown command. Type 'help' for available options.");
                    break;
            }
        }
    }
}
