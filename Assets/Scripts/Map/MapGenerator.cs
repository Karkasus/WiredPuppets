using UnityEngine;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class MapGenerator : MonoBehaviour
{
    public int width = 50;
    public int height = 50;

    public int roomCount = 8;
    public Vector2Int roomMinSize = new Vector2Int(4, 4);
    public Vector2Int roomMaxSize = new Vector2Int(10, 10);

    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject playerPrefab;

    public enum GenerationPattern { Classic, Linear, Clover, Modular }
    public GenerationPattern selectedPattern;

    public Unity.Cinemachine.CinemachineCamera vcam;

    public int[,] map;
    public List<RectInt> rooms = new List<RectInt>();

    [Header("Prefab Settings")]
    public GameObject[] handMadeRoomPrefabs; // Array of hand-made room prefabs
    [Range(0, 100)]
    public int prefabChance = 30;

    [Header("Modular Generation Settings")]
    [Tooltip("Drag and drop algorithm component here (e.g. BSPAlgorithm, RandomWalkAlgorithm, etc.)")]
    public Map.Algorithms.GenerationAlgorithm customAlgorithm;

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        InitializeMap();
        rooms.Clear(); // Clear rooms

        switch (selectedPattern)
        {
            case GenerationPattern.Linear:
                GenerateLinearPath();
                break;
            case GenerationPattern.Clover:
                GenerateCloverPattern();
                break;
            case GenerationPattern.Modular:
                GenerateModularMap();
                break;
            default:
                CreateRooms(); // Default: classic room generation
                ConnectRooms();
                break;
        }

        BuildMap();
        SpawnPlayer();
    }

    void GenerateModularMap()
    {
        // 1. Create empty map
        InitializeMap();
        rooms.Clear();

        // 2. Generate room shapes
        List<Map.Shapes.RoomShape> generatedShapes = new List<Map.Shapes.RoomShape>();
        for (int i = 0; i < roomCount; i++)
        {
            Map.Shapes.RoomShape shape;
            if (Random.value > 0.5f)
                shape = new Map.Shapes.SmallSquareShape();
            else
                shape = new Map.Shapes.LargeSquareShape();

            shape.Generate();
            generatedShapes.Add(shape);
        }

        // 3. Place shapes on map
        if (customAlgorithm != null)
        {
            customAlgorithm.Generate(map, rooms, width, height, generatedShapes);
        }
        else
        {
            Debug.LogWarning("Algorithm component is not assigned! Running random placement variant.");
            foreach (var shape in generatedShapes)
            {
                PlaceShapeOnMap(shape);
            }
        }

        // 4. Connect
        ConnectRooms();
    }

    void PlaceShapeOnMap(Map.Shapes.RoomShape shape)
    {
        int retries = 50;
        int maxW = shape.Width;
        int maxH = shape.Height;

        while (retries > 0)
        {
            int x = Random.Range(1, width - maxW - 1);
            int y = Random.Range(1, height - maxH - 1);

            RectInt newRoom = new RectInt(x, y, maxW, maxH);

            // Check overlap
            bool overlaps = false;
            foreach (var room in rooms)
            {
                // Leave 1 cell gap
                RectInt expandedRoom = new RectInt(room.x - 1, room.y - 1, room.width + 2, room.height + 2);
                if (expandedRoom.Overlaps(newRoom))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                rooms.Add(newRoom);
                // "Transfer" shape to map
                for (int sx = 0; sx < maxW; sx++)
                {
                    for (int sy = 0; sy < maxH; sy++)
                    {
                        if (shape.Grid[sx, sy] == 1)
                        {
                            map[x + sx, y + sy] = 2; // 2 - floor
                        }
                    }
                }
                break; // Successfully placed
            }
            retries--;
        }
    }

    void GenerateCloverPattern()
    {
        // 1. Create center room
        RectInt centerRoom = new RectInt(width / 2 - 5, height / 2 - 5, 10, 10);
        rooms.Add(centerRoom);
        CarveRoom(centerRoom);

        Vector2Int center = GetCenter(centerRoom);
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // 2. Generate path in 4 directions
        foreach (var dir in directions)
        {
            Vector2Int currentPos = center;
            for (int i = 0; i < roomCount / 4; i++)
            {
                currentPos += dir * Random.Range(8, 12); // distance between rooms

                int w = Random.Range(roomMinSize.x, roomMaxSize.x);
                int h = Random.Range(roomMinSize.y, roomMaxSize.y);

                RectInt leafRoom = new RectInt(currentPos.x - w / 2, currentPos.y - h / 2, w, h);

                // Check map bounds
                if (leafRoom.xMin > 0 && leafRoom.xMax < width && leafRoom.yMin > 0 && leafRoom.yMax < height)
                {
                    rooms.Add(leafRoom);
                    CarveRoom(leafRoom);
                    // Create corridor to previous room
                    CreateCorridor(currentPos - dir * 10, GetCenter(leafRoom));
                }
            }
        }
    }

    void GenerateLinearPath()
    {
        Vector2Int lastPos = new Vector2Int(width / 2, 10);

        for (int i = 0; i < roomCount; i++)
        {
            int w = Random.Range(roomMinSize.x, roomMaxSize.x);
            int h = Random.Range(roomMinSize.y, roomMaxSize.y);

            // New room position with slight X offset
            int x = lastPos.x - w / 2 + Random.Range(-3, 4);
            int y = lastPos.y + Random.Range(5, 10); // Offset up

            // Clamp room position so it does not exceed map
            x = Mathf.Clamp(x, 1, width - w - 1);
            y = Mathf.Clamp(y, 1, height - h - 1);

            RectInt newRoom = new RectInt(x, y, w, h);
            rooms.Add(newRoom);
            CarveRoom(newRoom);

            // Connect to previous room
            if (i > 0)
            {
                CreateCorridor(GetCenter(rooms[i - 1]), GetCenter(rooms[i]));
            }

            lastPos = new Vector2Int(GetCenter(newRoom).x, newRoom.yMax);
        }
    }

    void InitializeMap()
    {
        map = new int[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map[x, y] = 1;
    }

    void CreateRooms()
    {
        for (int i = 0; i < roomCount; i++)
        {
            // 1. Generate room dimensions
            // Randomly choose width and height within limits
            int w = Random.Range(roomMinSize.x, roomMaxSize.x);
            int h = Random.Range(roomMinSize.y, roomMaxSize.y);

            int x = Random.Range(1, width - w - 1);
            int y = Random.Range(1, height - h - 1);

            RectInt newRoom = new RectInt(x, y, w, h);

            // 2. Overlap check
            bool overlaps = false;
            foreach (var room in rooms)
            {
                if (room.Overlaps(newRoom))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                rooms.Add(newRoom);

                // 3. Decide: prefab or carve?
                if (handMadeRoomPrefabs.Length > 0 && Random.Range(0, 100) < prefabChance)
                {
                    SpawnPrefabRoom(newRoom);
                }
                else
                {
                    CarveRoom(newRoom); // Paint room floor on map: map[x,y]=2
                }
            }
        }
    }

    void SpawnPrefabRoom(RectInt roomData)
    {
        // Pick random prefab
        GameObject prefab = handMadeRoomPrefabs[Random.Range(0, handMadeRoomPrefabs.Length)];

        // Get actual size from RoomData
        Vector2Int actualSize = prefab.GetComponent<RoomData>().size;

        // Instantiate (at floor level, assuming Pivot is at bottom-left)
        Instantiate(prefab, new Vector3(roomData.xMin, 0, roomData.yMin), Quaternion.identity, transform);

        // Record room floor on map array to carve correctly
        for (int x = roomData.xMin; x < roomData.xMin + actualSize.x; x++)
            for (int y = roomData.yMin; y < roomData.yMin + actualSize.y; y++)
                if (x < width && y < height) map[x, y] = 2;
    }

    void CarveRoom(RectInt room)
    {
        for (int x = room.xMin; x < room.xMax; x++)
            for (int y = room.yMin; y < room.yMax; y++)
                map[x, y] = 2;
    }

    void ConnectRooms()
    {
        for (int i = 1; i < rooms.Count; i++)
        {
            Vector2Int prevCenter = GetCenter(rooms[i - 1]);
            Vector2Int currentCenter = GetCenter(rooms[i]);

            CreateCorridor(prevCenter, currentCenter);
        }
    }

    Vector2Int GetCenter(RectInt room)
    {
        return new Vector2Int(
            room.xMin + room.width / 2,
            room.yMin + room.height / 2
        );
    }

    void CreateCorridor(Vector2Int from, Vector2Int to)
    {
        Vector2Int pos = from;

        while (pos.x != to.x)
        {
            map[pos.x, pos.y] = 2;
            pos.x += (to.x > pos.x) ? 1 : -1;
        }

        while (pos.y != to.y)
        {
            map[pos.x, pos.y] = 2;
            pos.y += (to.y > pos.y) ? 1 : -1;
        }
    }

    void BuildMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x, 0, y);

                if (map[x, y] == 1)
                    Instantiate(wallPrefab, pos, Quaternion.identity, transform);
                else if (map[x, y] == 2)
                    Instantiate(floorPrefab, pos, Quaternion.identity, transform);
            }
        }
    }
    void SpawnPlayer()
    {
        Vector2Int center = GetCenter(rooms[0]);
        Vector3 spawnPos = new Vector3(center.x, 1, center.y);

        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        vcam.Follow = player.transform;
        vcam.LookAt = player.transform;
    }
}