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
    internal List<Button> orderButtons = new List<Button>();
    public Orders[] orders;
    internal GameObject[,] mapNodes;
    private List<HexCoords> tiles = new List<HexCoords>();

    // Start is called before the first frame update
    void Start()
    {
        i = this;
        orders = new Orders[] { Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade };
        HexCoordParent = GameObject.Find("HexCoordParent").transform;
        MapTileParent = GameObject.Find("MapTileParent").transform;
        circle = Resources.Load<GameObject>("Prefabs/Circle");
        CreateUI();
        GenerateMapNodes();
        GenerateTiles();
        ship = Instantiate(Resources.Load<ShipManager>("Prefabs/Ship"), GetShipCoords(mapNodes[5,5].transform.position), Quaternion.Euler(90,60,0));
    }
    public Vector3 GetShipCoords(Vector3 position)
    {
        return new Vector3(position.x, 0.1f, position.z);
    }
    private void CreateUI()
    {
        orderButtons.Add(GameObject.Find("Order0Button").GetComponent<Button>());
        orderButtons.Add(GameObject.Find("Order1Button").GetComponent<Button>());
        orderButtons.Add(GameObject.Find("Order2Button").GetComponent<Button>());
        orderButtons.Add(GameObject.Find("Order3Button").GetComponent<Button>());
        orderButtons.Add(GameObject.Find("Order4Button").GetComponent<Button>());
        orderButtons.Add(GameObject.Find("Order5Button").GetComponent<Button>());
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
        mapNodes = new GameObject[11, 11];
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
                    float scale = 1.73f;
                    float x = (1.732f * (j + 0.5f * i) / scale) - 7.51f;
                    float z = (1.5f * i / scale) - 4.33f;
                    var newHex = Instantiate(circle, new Vector3(x, 0, z), Quaternion.identity, HexCoordParent);
                    newHex.name = $"Hex {i},{j}";
                    if(newHex.GetComponentInChildren<TextMeshPro>() != null)
                        newHex.GetComponentInChildren<TextMeshPro>().text = $"{i},{j}";
                    mapNodes[i, j] = newHex;
                }
            }
        }
    }
    public void StartMatch()
    {
        StartCoroutine(ship.Move());
    }
    public void ChangeNumber(int orderNumber)
    {
        if (orders[orderNumber] == Orders.Evade)
        {
            GameObject.Find("Order"+ orderNumber).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Move");
            orders[orderNumber] = Orders.Move;
        }
        else if(orders[orderNumber] == Orders.Move)
        {
            GameObject.Find("Order" + orderNumber).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/TurnRight");
            orders[orderNumber] = Orders.TurnRight;
        }
        else if (orders[orderNumber] == Orders.TurnRight)
        {
            GameObject.Find("Order" + orderNumber).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/TurnLeft");
            orders[orderNumber] = Orders.TurnLeft;
        }
        else if (orders[orderNumber] == Orders.TurnLeft)
        {
            GameObject.Find("Order" + orderNumber).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/ShootRight");
            orders[orderNumber] = Orders.ShootRight;
        }
        else if (orders[orderNumber] == Orders.ShootRight)
        {
            GameObject.Find("Order" + orderNumber).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/ShootLeft");
            orders[orderNumber] = Orders.ShootLeft;
        }
        else if (orders[orderNumber] == Orders.ShootLeft)
        {
            GameObject.Find("Order" + orderNumber).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Evade");
            orders[orderNumber] = Orders.Evade;
        }
    }
    public enum Orders
    {
        Evade,
        Move,
        TurnRight,
        TurnLeft,
        ShootRight,
        ShootLeft
    }
}
