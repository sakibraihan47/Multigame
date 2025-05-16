using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardGamesFramework
{
    public class Board
    {
        public string[,] Grid;
        public int Size;

        public Board(int size)
        {
            Size = size;
            Grid = new string[size, size];
        }

        public void Display()
        {
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    Console.Write(Grid[r, c]?.PadRight(3) ?? ".  ");
                }
                Console.WriteLine();
            }
        }

        public bool IsFull()
        {
            for (int r = 0; r < Size; r++)
                for (int c = 0; c < Size; c++)
                    if (string.IsNullOrEmpty(Grid[r, c])) return false;
            return true;
        }

        public bool IsEmpty(int r, int c) => string.IsNullOrEmpty(Grid[r, c]);

        public void SetCell(int r, int c, string value) => Grid[r, c] = value;

        public string GetCell(int r, int c) => Grid[r, c];
    }
}
