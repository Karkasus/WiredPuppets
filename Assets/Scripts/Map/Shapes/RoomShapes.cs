using UnityEngine;

namespace Map.Shapes
{
    public abstract class RoomShape
    {
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        // 0 - empty/wall, 1 - floor
        public int[,] Grid { get; protected set; }

        public abstract void Generate();
    }

    // Small square 3x3 (9 sections)
    public class SmallSquareShape : RoomShape
    {
        public SmallSquareShape()
        {
            Width = 3;
            Height = 3;
            Grid = new int[Width, Height];
        }

        public override void Generate()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    // Make all 9 sections floor
                    Grid[x, y] = 1; 
                }
            }
        }
    }

    // Large square 4x4 (16 sections)
    public class LargeSquareShape : RoomShape
    {
        public LargeSquareShape()
        {
            Width = 4;
            Height = 4;
            Grid = new int[Width, Height];
        }

        public override void Generate()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    // For example, we can make corners empty so the room becomes more rounded
                    bool isCorner = (x == 0 && y == 0) || (x == 0 && y == Height - 1) ||
                                    (x == Width - 1 && y == 0) || (x == Width - 1 && y == Height - 1);

                    Grid[x, y] = isCorner ? 0 : 1;
                }
            }
        }
    }
}