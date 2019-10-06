using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{

    public GameObject floorPrefab;
    public GameObject wallPrefab1;
    public GameObject wallPrefab2;

    List<ISpace> currentLevel;

    // Start is called before the first frame update
    void Start()
    {
        GenerateLevel(0);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GenerateLevel(int level)
    {
        currentLevel = new List<ISpace>();
        Queue<ISpace> toBeGenerated = new Queue<ISpace>();
        Room firstRoom = Room.Generate(null, 0, new Vector2Int(0, 0), level);
        currentLevel.Add(firstRoom);
        toBeGenerated.Enqueue(firstRoom);
        while (toBeGenerated.Count > 0)
        {
            toBeGenerated.Dequeue().GenerateNext().ForEach(sp =>
            {
                if (sp.CollidesWithAny(currentLevel))
                {
                    sp.CancelConstruction();
                    return;
                }
                currentLevel.Add(sp);
                toBeGenerated.Enqueue(sp);
            });
        }

        currentLevel.ForEach(sp => sp.GenerateGameobjects(floorPrefab, wallPrefab1, wallPrefab2));
    }

    interface ISpace
    {
        int Depth { get; }       // How far from the start of the level
        RectInt BoundingBox { get; }

        // ISpace Generate(ISpace previous, Cardinal buildingDirection, Vector2Int startPos, int level);

        List<ISpace> GenerateNext();

        List<GameObject> GenerateGameobjects(GameObject floorPrefab, GameObject wallPrefab1, GameObject wallPrefab2);

        bool CollidesWithAny(List<ISpace> spaces);
        void CancelConstruction();
    }

    class Room : ISpace
    {
        HallWay[] hallWays = new HallWay[4];
        private int depth;
        private int level;
        RectInt box;
        public int Depth => depth;

        public RectInt BoundingBox => box;

        Room() { }

        public Room(ISpace previous, Vector2Int startposition, int depth, int level)
        {
            if (previous == null)
                box = new RectInt(-10, -10, 20, 20);
            else
                throw new NotImplementedException();
            this.depth = depth;
        }

        public static Room Generate(HallWay previous, Cardinal buildingDirection, Vector2Int startPos, int level)
        {
            Room room = new Room();
            room.level = level;
            if (previous == null) {
                room.box = new RectInt(-20, -20, 40, 40);
                room.depth = 0;
            } else {
                room.hallWays[((int)buildingDirection + 2) % 4] = previous;
                int width = UnityEngine.Random.Range(20, 50);
                int height = UnityEngine.Random.Range(20, 50);
                switch (buildingDirection)
                {
                case Cardinal.NORTH:
                    int startpoint = UnityEngine.Random.Range(2, width - 3);
                    room.box = new RectInt(startPos.x - startpoint, startPos.y, width, height);
                    break;
                case Cardinal.SOUTH:
                    startpoint = UnityEngine.Random.Range(2, width - 3);
                    room.box = new RectInt(startPos.x - startpoint, startPos.y - height, width, height);
                    break;
                case Cardinal.EAST:
                    startpoint = UnityEngine.Random.Range(2, height - 3);
                    room.box = new RectInt(startPos.x, startPos.y - startpoint, width, height);
                    break;
                case Cardinal.WEST:
                    startpoint = UnityEngine.Random.Range(2, height - 3);
                    room.box = new RectInt(startPos.x - width, startPos.y - startpoint, width, height);
                    break;
                }
                room.depth = previous.Depth;
            }
            return room;
        }

        public List<ISpace> GenerateNext()
        {
            List<ISpace> ways = new List<ISpace>();
            int hallHalfWidth = 2;
            for (int i = 0; i < 4; i++)
            {
                if (hallWays[i] != null) continue;                      // Is already used
                if (UnityEngine.Random.value < 0.2 * depth) continue;   // RNG sais no
                Vector2Int startPos = Vector2Int.zero;
                switch ((Cardinal)i)
                {
                case Cardinal.NORTH: startPos = new Vector2Int(box.xMin + UnityEngine.Random.Range(hallHalfWidth + 1, box.width - hallHalfWidth - 2), box.yMax); break;
                case Cardinal.EAST: startPos = new Vector2Int(box.xMax, box.yMin + UnityEngine.Random.Range(hallHalfWidth + 1, box.height - hallHalfWidth - 2)); break;
                case Cardinal.SOUTH: startPos = new Vector2Int(box.xMin + UnityEngine.Random.Range(hallHalfWidth + 1, box.width - hallHalfWidth - 2), box.yMin); break;
                case Cardinal.WEST: startPos = new Vector2Int(box.xMin, box.yMin + UnityEngine.Random.Range(hallHalfWidth + 1, box.height - hallHalfWidth - 2)); break;
                default: throw new Exception();
                }
                hallWays[i] = HallWay.Generate(this, (Cardinal)i, startPos, hallHalfWidth, level);
                ways.Add(hallWays[i]);
            }
            return ways;
        }

        public List<GameObject> GenerateGameobjects(GameObject floorPrefab, GameObject wallPrefab1, GameObject wallPrefab2)
        {
            Vector3 first = new Vector3(box.xMin, 0, box.yMax);
            Vector3 sec = new Vector3(box.xMax, 0, box.yMax);
            Vector3 third = new Vector3(box.xMax, 0, box.yMin);
            Vector3 fourth = new Vector3(box.xMin, 0, box.yMin);
            List<GameObject> gos = new List<GameObject>() {
                MakeFloor(floorPrefab, new Rect(box.position,box.size)),
            };
            BuildRoomWall(wallPrefab1, wallPrefab2, 0, first, sec, gos);
            BuildRoomWall(wallPrefab1, wallPrefab2, 1, sec, third, gos);
            BuildRoomWall(wallPrefab1, wallPrefab2, 2, third, fourth, gos);
            BuildRoomWall(wallPrefab1, wallPrefab2, 3, fourth, first, gos);

            return gos;
        }

        private void BuildRoomWall(GameObject wallPrefab1, GameObject wallPrefab2, int cardinal, Vector3 left, Vector3 right, List<GameObject> gos)
        {
            if (hallWays[cardinal] == null)
                gos.Add(MakeWall(wallPrefab1, wallPrefab2, left, right));
            else
            {
                bool useStartPos = hallWays[cardinal].Depth > depth;

                Vector3 hallStart1 = (useStartPos ? hallWays[cardinal].StartPos : hallWays[cardinal].EndPos) +
                    new Vector3(cardinal == 0 ? -1 : cardinal == 2 ? 1 : 0, 0, cardinal == 1 ? 1 : cardinal == 3 ? -1 : 0) * hallWays[cardinal].halfWidth;
                Vector3 hallStart2 = (useStartPos ? hallWays[cardinal].StartPos : hallWays[cardinal].EndPos) +
                    new Vector3(cardinal == 0 ? 1 : cardinal == 2 ? -1 : 0, 0, cardinal == 1 ? -1 : cardinal == 3 ? 1 : 0) * hallWays[cardinal].halfWidth;
                gos.Add(MakeWall(wallPrefab1, wallPrefab2, left, hallStart1));
                gos.Add(MakeWall(wallPrefab1, wallPrefab2, hallStart2, right));
            }
        }

        public bool CollidesWithAny(List<ISpace> spaces)
        {
            return spaces.Any(sp => RectOverlap(BoundingBox, sp.BoundingBox));
        }

        public void CancelConstruction()
        {

        }

        internal void RemoveCorridor(Cardinal direction) => hallWays[(int)direction] = null;
    }

    class HallWay : ISpace
    {
        Room[] rooms = new Room[2];
        PrisonCell[] cells = new PrisonCell[3];
        Vector2Int startPoint;
        Vector2Int wayPoint;
        Vector2Int endPoint;
        public float halfWidth;
        Cardinal direction;
        bool turnRight;
        private int depth;
        private int level;
        public int Depth => depth;
        public Vector3 StartPos => new Vector3(startPoint.x, 0, startPoint.y);
        public Vector3 EndPos => new Vector3(endPoint.x, 0, endPoint.y);

        public RectInt BoundingBox =>
            new RectInt((int)(Mathf.Min(startPoint.x, wayPoint.x, endPoint.x)), 
                (int)(Mathf.Min(startPoint.y, wayPoint.y, endPoint.y) ),
                Mathf.Abs(startPoint.x - endPoint.x), 
                Mathf.Abs(startPoint.y - endPoint.y)
                );

        public static HallWay Generate(Room previous, Cardinal buildingDirection, Vector2Int startPos, float halfWidth, int level)
        {
            HallWay hallWay = new HallWay();
            hallWay.rooms[0] = previous;
            hallWay.startPoint = startPos;
            int length1 = UnityEngine.Random.Range(15, 60);
            int length2 = UnityEngine.Random.Range(10, 70);
            Vector2Int dir = GetDir(buildingDirection);
            hallWay.wayPoint = startPos + dir * length1;
            hallWay.turnRight = UnityEngine.Random.value > 0.5;
            Vector2Int nexDir = GetDir((Cardinal)(((int)buildingDirection + (hallWay.turnRight ? 1 : -1)) % 4));
            hallWay.endPoint = hallWay.wayPoint + nexDir * length2;
            hallWay.halfWidth = 2;
            hallWay.depth = previous.Depth + 1;
            hallWay.level = level;
            hallWay.direction = buildingDirection;
            return hallWay;
        }

        public List<ISpace> GenerateNext()
        {
            List<ISpace> spaces = new List<ISpace>();
            Vector2Int dir = endPoint - wayPoint;
            Cardinal cardinal = dir.x > 0 ? Cardinal.EAST : dir.x < 0 ? Cardinal.WEST : dir.y > 0 ? Cardinal.NORTH : dir.y < 0 ? Cardinal.SOUTH : throw new Exception();
            rooms[1] = Room.Generate(this, cardinal, endPoint, level);
            spaces.Add(rooms[1]);
            // Add prison cells
            return spaces;
        }

        public List<GameObject> GenerateGameobjects(GameObject floorPrefab, GameObject wallPrefab1, GameObject wallPrefab2)
        {
            Rect Floor1 = GetFloorRect(startPoint, wayPoint, halfWidth);
            Rect Floor2 = GetFloorRect(wayPoint, endPoint, halfWidth);
            List<Tuple<Vector3, Vector3>> walls = GetHallwayWalls(startPoint, wayPoint, endPoint, halfWidth, direction, turnRight);
            return new List<GameObject>() {
                MakeFloor(floorPrefab, Floor1),
                MakeFloor(floorPrefab, Floor2),
                MakeWall(wallPrefab1, wallPrefab2, walls[0].Item1, walls[0].Item2),
                MakeWall(wallPrefab1, wallPrefab2, walls[1].Item1, walls[1].Item2),
                MakeWall(wallPrefab1, wallPrefab2, walls[2].Item1, walls[2].Item2),
                MakeWall(wallPrefab1, wallPrefab2, walls[3].Item1, walls[3].Item2),
            };
        }

        public bool CollidesWithAny(List<ISpace> spaces)
        {
            return spaces.Any(sp => RectOverlap(BoundingBox, sp.BoundingBox));
        }

        public void CancelConstruction()
        {
            rooms[0].RemoveCorridor(direction);
        }
    }

    class PrisonCell : ISpace
    {

        HallWay parent;
        Vector2Int position;
        int width, height;
        Cardinal direction;

        int depth;
        public int Depth => depth;

        public RectInt BoundingBox => throw new NotImplementedException();


        public static PrisonCell Generate(HallWay previous, Cardinal buildingDirection, Vector2Int startPos, int level)
        {
            PrisonCell cell = new PrisonCell() {
                parent = previous,
                position = startPos,
                direction = buildingDirection,
                depth = previous.Depth,
                width = UnityEngine.Random.Range(3, 5),
                height = UnityEngine.Random.Range(2, 5)
            };
            return cell;
        }

        public void CancelConstruction()
        {
            throw new NotImplementedException();
        }

        public bool CollidesWithAny(List<ISpace> spaces)
        {
            throw new NotImplementedException();
        }

        public List<GameObject> GenerateGameobjects(GameObject floorPrefab, GameObject wallPrefab1, GameObject wallPrefab2)
        {
            throw new NotImplementedException();
        }

        public List<ISpace> GenerateNext()
        {
            throw new NotImplementedException();
        }
    }

    private enum Cardinal { NORTH, EAST, SOUTH, WEST};

    static Vector2Int GetDir(Cardinal cardinal)
    {
        if (cardinal < 0) cardinal += 4;
        switch (cardinal)
        {
        case Cardinal.NORTH: return new Vector2Int(0, 1); 
        case Cardinal.EAST: return new Vector2Int(1, 0); 
        case Cardinal.SOUTH: return new Vector2Int(0, -1); 
        case Cardinal.WEST: return new Vector2Int(-1, 0);
        default: throw new Exception("Cardinal: " + cardinal);
        }
    }

    /// <summary>
    /// For hallway
    /// </summary>
    /// <param name="pos1"></param>
    /// <param name="pos2"></param>
    /// <param name="halfWidth"></param>
    /// <returns></returns>
    static Rect GetFloorRect(Vector2Int pos1, Vector2Int pos2, float halfWidth)
    {
        Vector2Int diff1 = pos1 - pos2;
        float x = Mathf.Min(pos1.x, pos2.x) - halfWidth;
        float y = Mathf.Min(pos1.y, pos2.y) - halfWidth;
        float width = Mathf.Max(Mathf.Abs(diff1.x), 0) + halfWidth * 2;
        float height = Mathf.Max(Mathf.Abs(diff1.y), 0) + halfWidth * 2;
        return new Rect(x, y, width, height);
    }

    static List<Tuple< Vector3,Vector3>> GetHallwayWalls(Vector2Int pos1, Vector2Int pos2, Vector2Int pos3, float halfWidth, Cardinal direction, bool turnRight)
    {
        bool FirstHallVertical = direction == Cardinal.NORTH || direction == Cardinal.SOUTH;
        bool TurnAtHighNumber = direction == Cardinal.NORTH || direction == Cardinal.EAST;
        // outer wall
        float v1outx = pos1.x + (direction == Cardinal.NORTH ? -1 : direction == Cardinal.SOUTH ? 1 : 0) * (turnRight ? 1 : -1) * halfWidth;
        float v1outy = pos1.y + (direction == Cardinal.EAST ? 1 : direction == Cardinal.WEST ? -1 : 0) * (turnRight ? 1 : -1) * halfWidth;
        Vector3 v1out = new Vector3(v1outx, 0, v1outy);
        //Vector3 v1out = new Vector3(pos1.x, 0, pos1.y) + (FirstHallVertical ? new Vector3(TurnAtHighNumber ^ turnRight ? 1 : -1, 0, 0) : new Vector3(0, 0, TurnAtHighNumber ^ turnRight ? -1 : 1)) * halfWidth;
        float v2outx = FirstHallVertical ? v1out.x : (pos2.x + (TurnAtHighNumber ? 1 : -1) * halfWidth);
        float v2outy = FirstHallVertical ? (pos2.y + (TurnAtHighNumber ? 1 : -1) * halfWidth) : v1out.z;
        Vector3 v2out = new Vector3(v2outx, 0, v2outy);
        float v3outx = FirstHallVertical ? pos3.x : v2out.x;
        float v3outy = FirstHallVertical ? v2out.z : pos3.y;
        Vector3 v3out = new Vector3(v3outx, 0, v3outy);
        // inner wall
        float v1inx = pos1.x + (direction == Cardinal.NORTH ? 1 : direction == Cardinal.SOUTH ? -1 : 0) * (turnRight ? 1 : -1) * halfWidth;
        float v1iny = pos1.y + (direction == Cardinal.EAST ? -1 : direction == Cardinal.WEST ? 1 : 0) * (turnRight ? 1 : -1) * halfWidth;
        Vector3 v1in = new Vector3(v1inx, 0, v1iny);
        //Vector3 v1in = new Vector3(pos1.x, 0, pos1.y) + (FirstHallVertical ? new Vector3(TurnAtHighNumber ^ turnRight ? -1 : 1, 0, 0) : new Vector3(0, 0, TurnAtHighNumber ^ turnRight ? 1 : -1)) * halfWidth;
        float v2inx = FirstHallVertical ? v1in.x : (pos2.x + (TurnAtHighNumber ? -1 : 1) * halfWidth);
        float v2iny = FirstHallVertical ? (pos2.y + (TurnAtHighNumber ? -1 : 1) * halfWidth) : v1in.z;
        Vector3 v2in = new Vector3(v2inx, 0, v2iny);
        float v3inx = FirstHallVertical ? pos3.x : v2in.x;
        float v3iny = FirstHallVertical ? v2in.z : pos3.y;
        Vector3 v3in = new Vector3(v3inx, 0, v3iny);
        
        if (turnRight)
            return new List<Tuple<Vector3, Vector3>>() {
            new Tuple<Vector3, Vector3>(v1out,v2out),
            new Tuple<Vector3, Vector3>(v2out, v3out),
            new Tuple<Vector3, Vector3>(v3in, v2in),
            new Tuple<Vector3, Vector3>(v2in,v1in)
        };
        else 
            return new List<Tuple<Vector3, Vector3>>() {
                new Tuple<Vector3, Vector3>(v1in,v2in),
                new Tuple<Vector3, Vector3>(v2in,v3in),
                new Tuple<Vector3, Vector3>(v3out,v2out),
                new Tuple<Vector3, Vector3>(v2out, v1out)
            };
    }

    static bool RectOverlap(RectInt rect1, RectInt rect2)
    {
        bool Hcheck = rect1.x < rect2.x + rect2.width && rect1.x + rect1.width > rect2.x; // If true, overlap possible
        bool Vcheck = rect1.y < rect2.y + rect2.height && rect1.y + rect1.height > rect2.y;
        return Hcheck && Vcheck;
    }

    static GameObject MakeFloor(GameObject floorPrefab, Rect box)
    {
        GameObject go = Instantiate(floorPrefab);
        go.transform.localScale = new Vector3(box.width / 10, 1, box.height / 10);
        go.transform.position = new Vector3(box.center.x, 0, box.center.y);
        go.GetComponent<Renderer>().material.mainTextureScale = new Vector2(box.width/5, box.height/5);
        return go;
    }

    static GameObject MakeWall(GameObject wallPrefab1, GameObject wallPrefab2, Vector3 startPoint, Vector3 endPoint)
    {
        GameObject go = Instantiate((startPoint.x == endPoint.x) ? wallPrefab1 : wallPrefab2);
        go.name = "Wall " + startPoint + endPoint;
        go.transform.localScale = new Vector3((startPoint-endPoint).magnitude / 10, 1, .4f);
        go.transform.position = new Vector3((startPoint+endPoint).x / 2, 2, (startPoint + endPoint).z / 2);
        float angle = Mathf.Atan2(startPoint.z - endPoint.z, endPoint.x - startPoint.x) * Mathf.Rad2Deg;
        go.transform.rotation = Quaternion.Euler(-90, angle, 0);
        go.GetComponent<Renderer>().material.mainTextureScale = new Vector2(go.transform.localScale.x, go.transform.localScale.z);
        return go;
    }
}
