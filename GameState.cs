using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardGamesFramework
{
    public static class GameState
    {
        public static void Save(string filename, string state)
        {
            File.WriteAllText(filename, state);
            Console.WriteLine("Game saved.");
        }

        public static string? Load(string filename)
        {
            try
            {
                if (!File.Exists(filename))
                    return null;

                var content = File.ReadAllText(filename);
                return string.IsNullOrWhiteSpace(content) ? null : content;
            }
            catch
            {
                Console.WriteLine("Failed to load saved game.");
                return null;
            }
        }

    }
}
