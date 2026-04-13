using UnityEngine;
using System.Collections.Generic;
using Map.Shapes;

namespace Map.Algorithms
{
    public abstract class GenerationAlgorithm : ScriptableObject
    {
        public abstract void Generate(int[,] map, List<RectInt> rooms, int width, int height, List<RoomShape> shapesToPlace = null);
    }
}
