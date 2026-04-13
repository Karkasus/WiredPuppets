using UnityEngine;
using System.Collections.Generic;
using Map.Shapes;

namespace Map.Algorithms
{
    /// <summary>
    /// 2. Cellular Automata
    /// Generates cave structures by filling with random noise and smoothing it through neighbors.
    /// Room shapes are embedded optionally here, it is more suitable for organic structures.
    /// </summary>
    [CreateAssetMenu(fileName = "CellularAutomataAlgorithm", menuName = "Map Generation/Cellular Automata")]
    public class CellularAutomataAlgorithm : GenerationAlgorithm
    {
        public override void Generate(int[,] map, List<RectInt> rooms, int width, int height, List<RoomShape> shapesToPlace = null)
        {
            float fillPercent = 0.45f;

            // Fill with noise
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    map[x, y] = (Random.value < fillPercent) ? 1 : 2; // 1-Wall, 2-Floor
                }
            }

            // Smoothing (4 iterations)
            for (int i = 0; i < 4; i++)
            {
                int[,] newMap = (int[,])map.Clone();
                for (int x = 1; x < width - 1; x++)
                {
                    for (int y = 1; y < height - 1; y++)
                    {
                        int wallCount = GetSurroundingWallCount(map, x, y, width, height);
                        if (wallCount > 4)
                            newMap[x, y] = 1;
                        else if (wallCount < 4)
                            newMap[x, y] = 2;
                    }
                }
                map = newMap;
            }

            // As a room, we will add one general zone (conditionally the whole center) to the list of rooms
            rooms.Add(new RectInt(width/2 - 5, height/2 - 5, 10, 10));
        }

        private int GetSurroundingWallCount(int[,] map, int gridX, int gridY, int width, int height)
        {
            int wallCount = 0;
            for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
            {
                for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
                {
                    if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
                    {
                        if (neighbourX != gridX || neighbourY != gridY)
                        {
                            if (map[neighbourX, neighbourY] == 1) wallCount++;
                        }
                    }
                    else wallCount++; // We count the edges of the map as walls
                }
            }
            return wallCount;
        }
    }
}
