using UnityEngine;
using System.Collections.Generic;
using Map.Shapes;

namespace Map.Algorithms
{
    /// <summary>
    /// 1. BSP (Binary Space Partitioning)
    /// Splits space in half until it gets cells of wanted size.
    /// In the centers of resulting cells, rooms/shapes are placed.
    /// </summary>
    [CreateAssetMenu(fileName = "BSPAlgorithm", menuName = "Map Generation/BSP Algorithm")]
    public class BSPAlgorithm : GenerationAlgorithm
    {
        public override void Generate(int[,] map, List<RectInt> rooms, int width, int height, List<RoomShape> shapesToPlace = null)
        {
            Queue<RectInt> partitions = new Queue<RectInt>();
            partitions.Enqueue(new RectInt(1, 1, width - 2, height - 2));

            int minPartitionSize = 12;

            while (partitions.Count < 10 && partitions.Count > 0)
            {
                RectInt current = partitions.Dequeue();

                // Select axis for cut
                bool splitHorizontal = Random.value > 0.5f;
                if (current.width > current.height && current.width / current.height >= 1.25)
                    splitHorizontal = false;
                else if (current.height > current.width && current.height / current.width >= 1.25)
                    splitHorizontal = true;

                if (current.width <= minPartitionSize * 2 || current.height <= minPartitionSize * 2)
                {
                    // Too small piece - leave as is, this will be a room
                    PlaceRoomInPartition(current, map, rooms, shapesToPlace);
                    continue;
                }

                if (splitHorizontal)
                {
                    int cut = Random.Range(minPartitionSize, current.height - minPartitionSize);
                    partitions.Enqueue(new RectInt(current.x, current.y, current.width, cut));
                    partitions.Enqueue(new RectInt(current.x, current.y + cut, current.width, current.height - cut));
                }
                else
                {
                    int cut = Random.Range(minPartitionSize, current.width - minPartitionSize);
                    partitions.Enqueue(new RectInt(current.x, current.y, cut, current.height));
                    partitions.Enqueue(new RectInt(current.x + cut, current.y, current.width - cut, current.height));
                }
            }

            // Place rooms in remaining partitions
            foreach (var partition in partitions)
            {
                PlaceRoomInPartition(partition, map, rooms, shapesToPlace);
            }
        }

        private void PlaceRoomInPartition(RectInt partition, int[,] map, List<RectInt> rooms, List<RoomShape> shapes)
        {
            if (shapes == null || shapes.Count == 0) return;

            RoomShape shape = shapes[Random.Range(0, shapes.Count)];
            int x = partition.x + (partition.width - shape.Width) / 2;
            int y = partition.y + (partition.height - shape.Height) / 2;

            if (x < 1 || y < 1 || x + shape.Width >= map.GetLength(0) - 1 || y + shape.Height >= map.GetLength(1) - 1)
                return;

            RectInt newRoom = new RectInt(x, y, shape.Width, shape.Height);
            rooms.Add(newRoom);

            for (int rX = 0; rX < shape.Width; rX++)
            {
                for (int rY = 0; rY < shape.Height; rY++)
                {
                    if (shape.Grid[rX, rY] == 1)
                        map[x + rX, y + rY] = 2; // Floor
                }
            }
        }
    }
}
