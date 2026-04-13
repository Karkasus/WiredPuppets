using UnityEngine;
using System.Collections.Generic;
using Map.Shapes;

namespace Map.Algorithms
{
    /// <summary>
    /// 4. Physics simulation (Steering Behaviors / Circle packing - simplified)
    /// We spawn rooms in a cluster at the center, then "push" them away from each other
    /// until they stop overlapping.
    /// </summary>
    [CreateAssetMenu(fileName = "SeparationAlgorithm", menuName = "Map Generation/Separation")]
    public class SeparationAlgorithm : GenerationAlgorithm
    {
        public override void Generate(int[,] map, List<RectInt> rooms, int width, int height, List<RoomShape> shapesToPlace = null)
        {
            if (shapesToPlace == null || shapesToPlace.Count == 0) return;

            // 1. Create a set of rooms in the center (with slight offset so vectors are not zero)
            int roomsToSpawn = 12;
            List<RoomData> tempRooms = new List<RoomData>();

            for (int i = 0; i < roomsToSpawn; i++)
            {
                RoomShape shape = shapesToPlace[Random.Range(0, shapesToPlace.Count)];
                float x = width / 2f + Random.Range(-2f, 2f);
                float y = height / 2f + Random.Range(-2f, 2f);
                tempRooms.Add(new RoomData { Rect = new Rect(x, y, shape.Width, shape.Height), Shape = shape });
            }

            // 2. Separate (iterative Separation)
            int safetyLimit = 100;
            bool overalapsExist = true;

            while (overalapsExist && safetyLimit > 0)
            {
                overalapsExist = false;
                for (int i = 0; i < tempRooms.Count; i++)
                {
                    for (int j = 0; j < tempRooms.Count; j++)
                    {
                        if (i == j) continue;

                        Rect rectA = tempRooms[i].Rect;
                        Rect rectB = tempRooms[j].Rect;

                        if (rectA.Overlaps(rectB))
                        {
                            overalapsExist = true;
                            // Find vector from center of B to center of A
                            Vector2 centerA = rectA.center;
                            Vector2 centerB = rectB.center;
                            Vector2 dir = (centerA - centerB).normalized;

                            // If centers are exactly the same
                            if (dir == Vector2.zero) dir = new Vector2(Random.value, Random.value).normalized;

                            // Move both rooms in opposite directions
                            tempRooms[i].Rect.position += dir * 0.5f;
                            tempRooms[j].Rect.position -= dir * 0.5f;
                        }
                    }
                }
                safetyLimit--;
            }

            // 3. Write result into grid (map) and list (rooms)
            foreach (var r in tempRooms)
            {
                int x = Mathf.Clamp(Mathf.RoundToInt(r.Rect.x), 1, width - Mathf.RoundToInt(r.Rect.width) - 1);
                int y = Mathf.Clamp(Mathf.RoundToInt(r.Rect.y), 1, height - Mathf.RoundToInt(r.Rect.height) - 1);

                rooms.Add(new RectInt(x, y, Mathf.RoundToInt(r.Rect.width), Mathf.RoundToInt(r.Rect.height)));

                // Draw room shape
                for (int rx = 0; rx < r.Shape.Width; rx++)
                {
                    for (int ry = 0; ry < r.Shape.Height; ry++)
                    {
                        if (r.Shape.Grid[rx, ry] == 1)
                            map[x + rx, y + ry] = 2; // Floor
                    }
                }
            }
        }

        private class RoomData
        {
            public Rect Rect;
            public RoomShape Shape;
        }
    }
}
