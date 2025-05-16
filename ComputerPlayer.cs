using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardGamesFramework
{
    public class ComputerPlayer : BasePlayer
    {
        public ComputerPlayer(string name) : base(name) { }

        public override void MakeMove(BaseGame game)
        {
            Console.WriteLine($"{Name} is making a move...");
            game.MakeMove(this);
        }
    }

}
