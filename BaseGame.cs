using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardGamesFramework
{
    public abstract class BaseGame
    {
        protected bool isPlayerOneTurn = true;
        public bool isHumanVsComputer;
        protected BasePlayer player1;
        protected BasePlayer player2;

        protected Stack<string> undoStackP1 = new();
        protected Stack<string> undoStackP2 = new();
        protected Stack<string> redoStackP1 = new();
        protected Stack<string> redoStackP2 = new();

        public void Run()
        {
            Console.Clear();

            while (!IsGameOver())
            {
                DisplayBoard();

                bool validMove = false;
                while (!validMove)
                {
                    int prevUndoCount = CurrentUndoStack().Count;
                    GetCurrentPlayer().MakeMove(this);
                    if (isHumanVsComputer && GetCurrentPlayer() is ComputerPlayer)
                    {
                        validMove = true;
                    }
                    else
                    {
                        validMove = CurrentUndoStack().Count > prevUndoCount;
                    }
                }

                if (!IsGameOver())
                    isPlayerOneTurn = !isPlayerOneTurn;
            }

            DisplayBoard();
            AnnounceResult();
        }

        protected Stack<string> CurrentUndoStack() => isPlayerOneTurn ? undoStackP1 : undoStackP2;
        protected Stack<string> CurrentRedoStack() => isPlayerOneTurn ? redoStackP1 : redoStackP2;

        public abstract void InitializeGame();
        public abstract BasePlayer GetCurrentPlayer();
        public abstract void DisplayBoard();
        public abstract bool IsGameOver();
        public abstract void AnnounceResult();
        public abstract void SaveGame();
        public abstract void LoadGame();
        public abstract void Undo();
        public abstract void Redo();
        public bool IsPlayerOneTurn() => isPlayerOneTurn;
        public abstract void MakeMove(BasePlayer player);
        public virtual void ShowHelp()
        {
            Console.WriteLine("\nAvailable Commands:");
            Console.WriteLine("- move  : Make a move (you will be prompted for row, column, etc.)");
            Console.WriteLine("- undo  : Undo your previous move");
            Console.WriteLine("- redo  : Redo your last undone move");
            Console.WriteLine("- save  : Save the current game state");
            Console.WriteLine("- help  : Show this help menu");
        }
    }



}
