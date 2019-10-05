using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{

    public GameObject wallPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Generate()
    {

    }

    interface ISpace
    {
        ISpace Previous { get; }
        int Depth { get; }       // How far from the start of the level
        List<ISpace> Children { get; }
        RectInt BoundingBox { get; }
    }

    class Room : ISpace
    {
        ISpace previous;
        private int depth;
        List<ISpace> children;
        public ISpace Previous => previous;
        public int Depth => depth;
        public List<ISpace> Children => children;

        public RectInt BoundingBox => throw new System.NotImplementedException();

        public Room(ISpace previous, int depth, int level)
        {
            this.previous = previous;
            this.depth = depth;
            children = new List<ISpace>();
        }
    }

    class HallWay
    {

    }

    class PrisonCell
    {

    }
}
