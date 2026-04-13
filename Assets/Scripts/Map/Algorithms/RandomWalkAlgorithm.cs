using UnityEngine;
using System.Collections.Generic;
using Map.Shapes;

namespace Map.Algorithms
{
    /// <summary>
    /// 3. Random Walk
    /// The digger takes random steps from the center, sometimes placing rooms(shapes),
    /// and leaves everything else as an excavated tunnel.
    /// </summary>
    [CreateAssetMenu(fileName = "RandomWalkAlgorithm", menuName = "Map Generation/Random Walk")]
    public class RandomWalkAlgorithm : GenerationAlgorithm
    {
        public override void Generate(int[,] map, List<RectInt> rooms, int width, int height, List<RoomShape> shapesToPlace = null)
        {
            Vector2Int currentPos = new Vector2Int(width / 2, height / 2);
            int iterations = 400;

            for (int i = 0; i < iterations; i++)
            {
                // Leave floor under the "digger's" position
                if (currentPos.x > 0 && currentPos.x < width - 1 && currentPos.y > 0 && currentPos.y < height - 1)
                {
                    map[currentPos.x, currentPos.y] = 2;
                }

                // 5% chance to place one of the preset shapes on the path
                if (shapesToPlace != null && shapesToPlace.Count > 0 && Random.value < 0.05f)
                {
                    RoomShape shape = shapesToPlace[Random.Range(0, shapesToPlace.Count)];
                    if (currentPos.x + shape.Width < width - 1 && currentPos.y + shape.Height < height - 1)
                    {
                        rooms.Add(new RectInt(currentPos.x, currentPos.y, shape.Width, shape.Height));
                        for (int rx = 0; rx < shape.Width; rx++)
                            for (int ry = 0; ry < shape.Height; ry++)
                                if (shape.Grid[rx, ry] == 1)
                                    map[currentPos.x + rx, currentPos.y + ry] = 2;
                    }
                }

                // Step in random direction
                Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                currentPos += dirs[Random.Range(0, 4)];

                // Limit so we don't go out of bounds
                currentPos.x = Mathf.Clamp(currentPos.x, 1, width - 2);
                currentPos.y = Mathf.Clamp(currentPos.y, 1, height - 2);
            }

            if (rooms.Count == 0) // If no rooms spawned (no shapes provided), add start room
                rooms.Add(new RectInt(width/2 - 1, height/2 - 1, 2, 2));
        }
    }
}
