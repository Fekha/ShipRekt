using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    internal static GameManager i;
    private ShipManager ship;
    public GameObject emptyHexPrefab;
    public GameObject cornerHexPrefab;
    public GameObject edgeHexPrefab;
    private GameObject circle;
    Transform HexCoordParent;
    Transform MapTileParent;
    internal List<GameObject> orderButtons = new List<GameObject>();
    internal HexCoords[,] mapNodes;
    private List<HexCoords> tiles = new List<HexCoords>();

    // Start is called before the first frame update
    void Start()
    {
        i = this;
        HexCoordParent = GameObject.Find("HexCoordParent").transform;
        MapTileParent = GameObject.Find("MapTileParent").transform;
        circle = Resources.Load<GameObject>("Prefabs/Circle");
        ship = GameObject.Find("Ship1").GetComponent<ShipManager>();
        ship.currentPos = new HexCoords(5, 5);
        ship.direction = HexDirection.TopLeft;
        CreateUI();
        GenerateMapNodes();
        GenerateTiles();
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

    private void GenerateTiles()
    {
        GenerateRings();
        foreach (var hex in tiles)
        {
            var shape = hex.type == 0 ? emptyHexPrefab : hex.type == 1 ? edgeHexPrefab : cornerHexPrefab;
            var position = new Vector3(hex.y * 1.5f, 0, hex.x * Mathf.Sqrt(3) + hex.y * 0.5f * Mathf.Sqrt(3)); // Difficult transformation from hexagonal to normal coordinates, with love <3 Tris.
            var rotation = Quaternion.Euler(90, hex.rotation, 0);
            Instantiate(shape, position, rotation, MapTileParent);
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
                tiles.Add(new HexCoords(i, 0, 2, 60));
            }
            else
            {
                tiles.Add(new HexCoords(i, 0, type, rotation));
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
                        tiles.Add(new HexCoords(tiles[tiles.Count - 1].x + HexDirections.directionMap[(HexDirection)j].x, tiles[tiles.Count - 1].y + HexDirections.directionMap[(HexDirection)j].y, 2, rotation));
                    }
                    else
                    {
                        tiles.Add(new HexCoords(tiles[tiles.Count - 1].x + HexDirections.directionMap[(HexDirection)j].x, tiles[tiles.Count - 1].y + HexDirections.directionMap[(HexDirection)j].y, type, rotation));
                    }
                }
            }
        }
    }


    private void GenerateMapNodes()
    {
        mapNodes = new HexCoords[11, 11];
        for (int i = 0; i < 11; i++)
        {
            for (int j = 0; j < 11; j++)
            {
                if ((j == 0 && i >= 6 && i < 10) ||
                    (j == 1 && i >= 4) ||
                    (j == 2 && i >= 3) ||
                    (j == 3 && i >= 2) ||
                    (j == 4 && i >= 1) ||
                    (j == 5 && (i >= 1 && i < 10)) ||
                    (j == 6 && i < 10) ||
                    (j == 7 && i < 9) ||
                    (j == 8 && i < 8) ||
                    (j == 9 && i < 7) ||
                    (j == 10 && (i >= 1 && i < 5)))
                {
                    mapNodes[i, j] = new HexCoords(i, j);
                    float scale = 1.73f;
                    float x = (1.732f * (j + 0.5f * i) / scale) - 7.51f;
                    float z = (1.5f * i / scale) - 4.33f;
                    var newHex = Instantiate(circle, new Vector3(x, 0, z), Quaternion.identity, HexCoordParent);
                }
            }
        }
    }
    public void StartMatch()
    {
        StartCoroutine(ship.Move());
    }

    public void ChangeNumber(TextMeshProUGUI text)
    {
        if (text.text == "0")
            text.text = "1";
        else if(text.text == "1")
            text.text = "2";
        else if (text.text == "2")
            text.text = "3";
        else if (text.text == "3")
            text.text = "4";
        else if (text.text == "4")
            text.text = "5";
        else if (text.text == "5")
            text.text = "0";
    }
   
}
