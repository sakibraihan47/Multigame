using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardGamesFramework
{
    public abstract class BasePlayer
    {
        public string Name { get; set; }

        protected BasePlayer(string name)
        {
            Name = name;
        }

        public abstract void MakeMove(BaseGame game);
    }

}
