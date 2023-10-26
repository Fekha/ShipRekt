using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    internal static GameManager i;
    private ShipManager ship;
    private GameObject gameBoard;
    public GameObject emptyHexPrefab;
    public GameObject cornerHexPrefab;
    public GameObject edgeHexPrefab;
    private GameObject circle;
    public HexCoords[,] mapNodes;
    Transform HexCoordParent;
    Transform MapTileParent;
    internal List<GameObject> orderButtons = new List<GameObject>();
    private List<HexCoords> hexes = new List<HexCoords>();
    private List<HexCoords> transforms = new List<HexCoords>()
    {
        new HexCoords(-1, 1),
        new HexCoords(-1, 0),
        new HexCoords(0, -1),
        new HexCoords(1, -1),
        new HexCoords(1, 0),
        new HexCoords(0, 1)
    };

    // Start is called before the first frame update
    void Start()
    {
        i = this;
        HexCoordParent = GameObject.Find("HexCoordParent").transform;
        MapTileParent = GameObject.Find("MapTileParent").transform;
        circle = Resources.Load<GameObject>("Prefabs/Circle");
        ship = GameObject.Find("Ship1").GetComponent<ShipManager>();
        gameBoard = GameObject.Find("GameBoard");
        CreateUI();
        GenerateTiles();
        GenerateMapNodes();
    }

    private void GenerateTiles()
    {
        GenerateRings();
        foreach (var hex in hexes)
        {
            var shape = hex.type == 0 ? emptyHexPrefab : hex.type == 1 ? edgeHexPrefab : cornerHexPrefab;
            var position = new Vector3(hex.y * 1.5f, 0, hex.x * Mathf.Sqrt(3) + hex.y * 0.5f * Mathf.Sqrt(3)); // Difficult transformation from hexagonal to normal coordinates, with love <3 Tris.
            var rotation = Quaternion.Euler(90, hex.rotation, 0);
            Instantiate(shape, position, rotation, MapTileParent);
        }
    }

    private void CreateUI()
    {
        orderButtons.Add(GameObject.Find("Order1Button"));
        orderButtons.Add(GameObject.Find("Order2Button"));
        orderButtons.Add(GameObject.Find("Order3Button"));
        orderButtons.Add(GameObject.Find("Order4Button"));
        orderButtons.Add(GameObject.Find("Order5Button"));
        orderButtons.Add(GameObject.Find("Order6Button"));
    }

    private void GenerateMapNodes()
    {
        mapNodes = new HexCoords[11,11];
        for (int i = 0; i < 11; i++)
        {
            for (int j = 0; j < 11; j++)
            {
                mapNodes[i,j] = new HexCoords(i, j);
                float scale = 1.73f;
                float x = (1.732f * (j + 0.5f * i) / scale) - 7.51f; 
                float z = (1.5f * i / scale) - 4.33f;
                var newHex = Instantiate(circle, new Vector3(x, -.1f, z), Quaternion.identity, HexCoordParent);
            }
        }
    }
    
    public void GenerateRings()
    {
        int ringCount = 3;
        int type = 0;
        int rotation = 0;
        for (int i = 0; i <= ringCount; i++)
        {
            if (i == ringCount)
            {
                type = 1;
                hexes.Add(new HexCoords(i, 0, 2, 60));
            }
            else
            {
                hexes.Add(new HexCoords(i, 0, type, rotation));
            }
            for (int j = 0; j < 6; j++)
            {
                if (i == ringCount)
                {
                    rotation = (120 + (j * 60)) % 360;
                }
                var max = (j == 5 ? i - 1 : i);
                for (int k = 0; k < max; k++)
                {
                    if (i == ringCount && max - 1 == k && j != 5)
                    {
                        hexes.Add(new HexCoords(hexes[hexes.Count - 1].x + transforms[j].x, hexes[hexes.Count - 1].y + transforms[j].y, 2, rotation));
                    }
                    else
                    {
                        hexes.Add(new HexCoords(hexes[hexes.Count - 1].x + transforms[j].x, hexes[hexes.Count - 1].y + transforms[j].y, type, rotation));
                    }
                }
            }
        }
    }

    public void StartMatch()
    {
        StartCoroutine(ship.Move());
    }
    public class HexCoords
    {
        public HexCoords(int i, int v, int type = 0, int rotation = 0)
        {
            this.x = i;
            this.y = v;
            this.type = type;
            this.rotation = rotation;
        }
        public int x;
        public int y;
        public int type;
        public int rotation;
        public string label { get { return x + " " + y; } }
    }
}
