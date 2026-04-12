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

    public enum GenerationPattern { Classic, Linear, Clover }
    public GenerationPattern selectedPattern;

    public CinemachineCamera vcam;

    private int[,] map;
    private List<RectInt> rooms = new List<RectInt>();

    [Header("Prefab Settings")]
    public GameObject[] handMadeRoomPrefabs; // Список заранее созданных комнат
    [Range(0, 100)]
    public int prefabChance = 30;

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        InitializeMap();
        rooms.Clear(); // Не забывай очищать список при перегенерации

        switch (selectedPattern)
        {
            case GenerationPattern.Linear:
                GenerateLinearPath();
                break;
            case GenerationPattern.Clover:
                GenerateCloverPattern();
                break;
            default:
                CreateRooms(); // Твой стандартный метод
                ConnectRooms();
                break;
        }

        BuildMap();
        SpawnPlayer();
    }

    void GenerateCloverPattern()
    {
        // 1. Центральная комната
        RectInt centerRoom = new RectInt(width / 2 - 5, height / 2 - 5, 10, 10);
        rooms.Add(centerRoom);
        CarveRoom(centerRoom);

        Vector2Int center = GetCenter(centerRoom);
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // 2. Пускаем ветки в 4 стороны
        foreach (var dir in directions)
        {
            Vector2Int currentPos = center;
            for (int i = 0; i < roomCount / 4; i++)
            {
                currentPos += dir * Random.Range(8, 12); // Шагаем в сторону

                int w = Random.Range(roomMinSize.x, roomMaxSize.x);
                int h = Random.Range(roomMinSize.y, roomMaxSize.y);

                RectInt leafRoom = new RectInt(currentPos.x - w / 2, currentPos.y - h / 2, w, h);

                // Проверка границ
                if (leafRoom.xMin > 0 && leafRoom.xMax < width && leafRoom.yMin > 0 && leafRoom.yMax < height)
                {
                    rooms.Add(leafRoom);
                    CarveRoom(leafRoom);
                    // Соединяем текущую комнату "лепестка" с предыдущей в этой ветке
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

            // Смещаем следующую комнату вверх, но немного "шатаем" по X
            int x = lastPos.x - w / 2 + Random.Range(-3, 4);
            int y = lastPos.y + Random.Range(5, 10); // Дистанция между комнатами

            // Проверка границ массива, чтобы не вылететь за пределы
            x = Mathf.Clamp(x, 1, width - w - 1);
            y = Mathf.Clamp(y, 1, height - h - 1);

            RectInt newRoom = new RectInt(x, y, w, h);
            rooms.Add(newRoom);
            CarveRoom(newRoom);

            // Соединяем с предыдущей
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
            // 1. Сначала определяем размеры (для префаба или для случайной комнаты)
            // Если будем спавнить префаб, позже подправим размеры под него
            int w = Random.Range(roomMinSize.x, roomMaxSize.x);
            int h = Random.Range(roomMinSize.y, roomMaxSize.y);

            int x = Random.Range(1, width - w - 1);
            int y = Random.Range(1, height - h - 1);

            RectInt newRoom = new RectInt(x, y, w, h);

            // 2. Проверка на наложение (как и было)
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

                // 3. Выбор: Рандом или Префаб?
                if (handMadeRoomPrefabs.Length > 0 && Random.Range(0, 100) < prefabChance)
                {
                    SpawnPrefabRoom(newRoom);
                }
                else
                {
                    CarveRoom(newRoom); // Ваша старая логика заполнения массива map[x,y]=2
                }
            }
        }
    }

    void SpawnPrefabRoom(RectInt roomData)
    {
        // Выбираем префаб
        GameObject prefab = handMadeRoomPrefabs[Random.Range(0, handMadeRoomPrefabs.Length)];

        // Получаем его реальный размер из скрипта RoomData
        Vector2Int actualSize = prefab.GetComponent<RoomData>().size;

        // Спавним (с учетом того, что Pivot в углу)
        Instantiate(prefab, new Vector3(roomData.xMin, 0, roomData.yMin), Quaternion.identity, transform);

        // Помечаем в массиве map именно столько клеток, сколько занимает префаб
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