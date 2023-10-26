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
    public MapNodeCoords[,] mapNodes;
    internal List<GameObject> orderButtons = new List<GameObject>();
    private List<HexCoords> coords = new List<HexCoords>();
    private List<HexCoords> hexes = new List<HexCoords>();
    // Start is called before the first frame update
    void Start()
    {
        i = this;
        circle = Resources.Load<GameObject>("Prefabs/Circle");
        ship = GameObject.Find("Ship1").GetComponent<ShipManager>();
        gameBoard = GameObject.Find("GameBoard");
        orderButtons.Add(GameObject.Find("Order1Button"));
        orderButtons.Add(GameObject.Find("Order2Button"));
        orderButtons.Add(GameObject.Find("Order3Button"));
        orderButtons.Add(GameObject.Find("Order4Button"));
        orderButtons.Add(GameObject.Find("Order5Button"));
        orderButtons.Add(GameObject.Find("Order6Button"));
      
        GenerateRings();
        GameObject hexObj;
        foreach (var hex in hexes)
        {
            if (hex.type == 0)
            {
                hexObj = Instantiate(emptyHexPrefab);
            }
            else if (hex.type == 1)
            {
                hexObj = Instantiate(edgeHexPrefab);
            } 
            else 
            {
                hexObj = Instantiate(cornerHexPrefab);
            }
            hexObj.transform.position = new Vector3(hex.y * 1.5f, 0, hex.x * Mathf.Sqrt(3) + hex.y * 0.5f * Mathf.Sqrt(3)); // Difficult transformation from hexagonal to normal coordinates, with love <3 Tris.
            hexObj.transform.rotation = Quaternion.Euler(90, hex.rotation, 0);
        }

        GenerateMapNodes();
    }

    private void GenerateMapNodes()
    {
        mapNodes = new MapNodeCoords[10,11];
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 11; j++)
            {
                mapNodes[i,j] = new MapNodeCoords(i, j);
                float offset = 7;
                float scale = 1.7f;
                float x = (1.732f * (j + 0.5f * i) - 11.7f) / scale; 
                float z = (1.5f * i - 6.5f) / scale;
                var nodePos = new Vector3(x, -.1f, z);
                var newHex = Instantiate(circle, nodePos, Quaternion.identity);
            }
        }
    }

    public void GenerateRings()
    {
        int ringCount = 3;
        List<HexCoords> transforms = new List<HexCoords>()
        {
            new HexCoords(-1, 1),
            new HexCoords(-1, 0),
            new HexCoords(0, -1),
            new HexCoords(1, -1),
            new HexCoords(1, 0),
            new HexCoords(0, 1)
        };
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
    public class MapNodeCoords
    {
        public MapNodeCoords(int i, int v, int type = 0, int rotation = 0)
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
    public class Blockade
    {
        public HexCoords x { get; set; }
        public HexCoords y { get; set; }
    } 
}
