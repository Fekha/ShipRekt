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
    Transform HexCoordParent;
    internal List<Button> orderButtons = new List<Button>();
    internal Orders[] orders;
    internal GameObject[,] mapNodes;

    // Start is called before the first frame update
    void Start()
    {
        i = this;
        orders = new Orders[] { Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade };
        HexCoordParent = GameObject.Find("HexCoordParent").transform;
        circle = Resources.Load<GameObject>("Prefabs/Circle");
        CreateUI();
        GenerateMapNodes();
        ship = Instantiate(Resources.Load<ShipManager>("Prefabs/Ship"), GetShipCoords(mapNodes[5,5].transform.position), Quaternion.Euler(90,60,0));
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
}
