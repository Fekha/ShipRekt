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
    private GameObject circle;
    private GameObject OrderPanel;
    Transform HexCoordParent;
    internal List<Button> orderButtons = new List<Button>();
    internal Orders[] newOrders;
    internal Orders[] oldOrders;
    internal GameObject[,] mapNodes;
    private bool isBoardMaximized = true;
    private Sprite Move;
    private Sprite TurnRight;
    private Sprite TurnLeft;
    private Sprite ShootRight;
    private Sprite ShootLeft;
    private Sprite Evade;
    public List<HexCoords> spawns = new List<HexCoords>();
    private ShipManager shipPrefab;
    // Start is called before the first frame update
    void Start()
    {
        i = this;
        newOrders = new Orders[] { Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade };
        oldOrders = new Orders[] { Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade };
        spawns = new List<HexCoords>() { new HexCoords(2,5,0,4), new HexCoords(2,8,5,3), new HexCoords(5,8,4,2), new HexCoords(8,5,3,1), new HexCoords(8,2,2,0), new HexCoords(5,2,1,5)};
        Evade = Resources.Load<Sprite>("Sprites/Evade");
        Move = Resources.Load<Sprite>("Sprites/Move");
        TurnRight = Resources.Load<Sprite>("Sprites/TurnRight");
        TurnLeft = Resources.Load<Sprite>("Sprites/TurnLeft");
        ShootRight = Resources.Load<Sprite>("Sprites/ShootRight");
        ShootLeft = Resources.Load<Sprite>("Sprites/ShootLeft");
        HexCoordParent = GameObject.Find("HexCoordParent").transform;
        OrderPanel = GameObject.Find("OrderPanel");
        circle = Resources.Load<GameObject>("Prefabs/Circle");
        shipPrefab = Resources.Load<ShipManager>("Prefabs/Ship");

        CreateUI();
        GenerateMapNodes();
        var randomSpawn = spawns[UnityEngine.Random.Range(0, spawns.Count)];
        ship = Instantiate(shipPrefab, GetShipCoords(mapNodes[randomSpawn.x, randomSpawn.y].transform.position), Quaternion.Euler(90, (randomSpawn.rotation * 60) % 360, 0));
        ship.CreateShip(randomSpawn);
    }
    public void StartMatch()
    {
        StartCoroutine(ship.Move());
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

    public void MaximizeBoard()
    {
        isBoardMaximized = !isBoardMaximized;
        if (isBoardMaximized)
        {
            OrderPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(OrderPanel.GetComponent<RectTransform>().anchoredPosition.x, -246, 0);
        }
        else
        {
            OrderPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(OrderPanel.GetComponent<RectTransform>().anchoredPosition.x, -408.9674f, 0);
        }
        OrderPanel.transform.Find("MaximizeButton").Rotate(0, 0, 180);
    }
    public void ChangeNumber(int orderNumber)
    {
        var differences = FindDifferences(newOrders, oldOrders);
        if (differences.Count < 2 || differences.Contains(orderNumber))
        {
            if (newOrders[orderNumber] == Orders.Evade)
            {
                orderButtons[orderNumber].transform.Find("Order").GetComponent<Image>().sprite = Move;
                newOrders[orderNumber] = Orders.Move;
            }
            else if (newOrders[orderNumber] == Orders.Move)
            {
                orderButtons[orderNumber].transform.Find("Order").GetComponent<Image>().sprite = TurnRight;
                newOrders[orderNumber] = Orders.TurnRight;
            }
            else if (newOrders[orderNumber] == Orders.TurnRight)
            {
                orderButtons[orderNumber].transform.Find("Order").GetComponent<Image>().sprite = TurnLeft;
                newOrders[orderNumber] = Orders.TurnLeft;
            }
            else if (newOrders[orderNumber] == Orders.TurnLeft)
            {
                orderButtons[orderNumber].transform.Find("Order").GetComponent<Image>().sprite = ShootRight;
                newOrders[orderNumber] = Orders.ShootRight;
            }
            else if (newOrders[orderNumber] == Orders.ShootRight)
            {
                orderButtons[orderNumber].transform.Find("Order").GetComponent<Image>().sprite = ShootLeft;
                newOrders[orderNumber] = Orders.ShootLeft;
            }
            else if (newOrders[orderNumber] == Orders.ShootLeft)
            {
                orderButtons[orderNumber].transform.Find("Order").GetComponent<Image>().sprite = Evade;
                newOrders[orderNumber] = Orders.Evade;
            }
        }
        differences = FindDifferences(newOrders, oldOrders);
        for(int i = 0; i < 6; i++)
        {
            orderButtons[i].transform.Find("Selected").gameObject.SetActive(differences.Contains(i));
        }
    }

    private List<int> FindDifferences(Orders[] newOrders, Orders[] oldOrders)
    {
        var values = new List<int>();
        for(int i = 0; i < newOrders.Length; i++)
        {
            if (newOrders[i] != oldOrders[i])
            {
                values.Add(i);
            }
        }
        return values;
    }
}
