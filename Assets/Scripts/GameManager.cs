using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    internal static GameManager i;
    internal List<ShipManager> Ships = new List<ShipManager>();
    private GameObject NodePrefab;
    private GameObject OrderPanel;
    private GameObject Booty;
    private GameObject ViewButton;
    public GameObject MainCanvas;
    Transform HexCoordParent;
    internal List<Button> orderButtons = new List<Button>();
    internal GameObject[,] mapNodes;
    private bool isBoardMaximized = true;
    public List<HexCoords> Spawns = new List<HexCoords>();
    private int playerTurn = 0;
    private int numPlayers = 2;
    internal Sprite[] OrderSpirtes;
    internal bool[] ShipsDone;
    // Start is called before the first frame update
    void Start()
    {
        i = this;
        ShipsDone = new bool[numPlayers];
        ShipsDone.Select(x => false).ToArray();
        MainCanvas.SetActive(true);
        Spawns = new List<HexCoords>() { new HexCoords(2, 5, 0, 4), new HexCoords(2, 8, 5, 3), new HexCoords(5, 8, 4, 2), new HexCoords(8, 5, 3, 1), new HexCoords(8,2,2,0), new HexCoords(5,2,1,5)};
        OrderSpirtes = new Sprite[] { Resources.Load<Sprite>("Sprites/Evade"), Resources.Load<Sprite>("Sprites/Move"), Resources.Load<Sprite>("Sprites/TurnLeft"), Resources.Load<Sprite>("Sprites/TurnRight"), Resources.Load<Sprite>("Sprites/ShootLeft"), Resources.Load<Sprite>("Sprites/ShootRight"), Resources.Load<Sprite>("Sprites/None") };
        HexCoordParent = GameObject.Find("HexCoordParent").transform;
        OrderPanel = GameObject.Find("OrderPanel");
        Booty = OrderPanel.transform.Find("Booty").gameObject;
        NodePrefab = Resources.Load<GameObject>("Prefabs/Circle");
        ViewButton = GameObject.Find("ViewButton");
        CreateUI();
        GenerateMapNodes();
        for (int i = 0; i < numPlayers; i++)
        {
            var randomSpawn = Spawns[Random.Range(0, Spawns.Count)];
            var ship = Instantiate(Resources.Load<ShipManager>("Prefabs/Ship"+i), GetShipCoords(mapNodes[randomSpawn.x, randomSpawn.y].transform.position), Quaternion.Euler(90, (randomSpawn.rotation * 60) % 360, 0));
            ship.CreateShip(randomSpawn, i);
            Ships.Add(ship);
            Spawns.Remove(randomSpawn);
            MainCanvas.transform.Find($"Player{i}").gameObject.SetActive(true);
        }
        SetBoard();
    }
    public void EndTurn()
    {
        orderButtons.ForEach(x => x.transform.Find("Selected").gameObject.SetActive(false));
        if (playerTurn < numPlayers-1)
        {
            playerTurn++;
            SetBoard();
        }
        else
        {
            StartCoroutine(TakeOrders());
        }
    }
    
    private IEnumerator TakeOrders()
    {
        playerTurn = 0;
        SetAllPlayersOrders();
        OrderPanel.SetActive(false);
        ViewButton.SetActive(false);
        for (int i = 0; i < 6; i++)
        {
            var ShipsToMove = Ships.Where(x => x.newOrders[i] == Orders.Move).ToList();
            foreach (var ship in ShipsToMove)
            {
                StartCoroutine(ship.Move(i));
            }
            while (ShipsToMove.Any(x => !ShipsDone[x.id]))
            {
                yield return null;
            }
            foreach (var ship in Ships.Where(x => x.newOrders[i] != Orders.Move))
            {
                StartCoroutine(ship.Move(i));
            }
            while (ShipsDone.Any(x => !x))
            {
                yield return null;
            }
            ShipsDone.Select(x => false).ToArray();
        }
        for(int i = 0; i < Ships.Count; i++)
        {
            if (Ships[i].newOrders.All(x => x == Orders.None))
            {
                MainCanvas.transform.Find($"Player{Ships[i].id}").gameObject.SetActive(false);
                numPlayers--;
                Destroy(Ships[i]);
            }
        }
        if (Ships.Any(x => !x.booty.Any(y=>y == Treasure.None)))
        {
            MainCanvas.transform.Find("GameOver").gameObject.SetActive(true);
        }
        else
        {
            SetAllPlayersOrders();
            SetBoard();
            OrderPanel.SetActive(true);
            ViewButton.SetActive(true);
        }
    }

    private IEnumerator CheckForHeals(ShipManager ship)
    {
        for (int j = 0; j < ship.newOrders.Count(); j++)
        {
            if (ship.newOrders[j] == Orders.None)
            {
                int bootyIndex = -1;
                if (ship.booty.Contains(Treasure.GreenDie))
                {
                    bootyIndex = ship.booty.ToList().IndexOf(Treasure.GreenDie);
                }
                else if (ship.booty.Contains(Treasure.BlueDie))
                {
                    bootyIndex = ship.booty.ToList().IndexOf(Treasure.BlueDie);
                }
                else if (ship.booty.Contains(Treasure.PinkDie))
                {
                    bootyIndex = ship.booty.ToList().IndexOf(Treasure.PinkDie);
                }
                if (bootyIndex != -1)
                {
                    yield return StartCoroutine(ship.Heal(bootyIndex));
                }
            }
        }
        ShipsDone[ship.id] = true;
    }

    private void SetBoard()
    {
        for (int i = 0; i < orderButtons.Count; i++)
        {
            orderButtons[i].transform.Find("Order").GetComponent<Image>().sprite = OrderSpirtes[(int)Ships[playerTurn].oldOrders[i]];
            orderButtons[i].GetComponent<Image>().color = playerTurn == 0? Color.green : playerTurn == 1? Color.cyan : Color.magenta;
            Booty.transform.Find($"Booty{i}").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/Treasure/{(int)Ships[playerTurn].booty[i]}");
        }
    }

    private void SetAllPlayersOrders()
    {
        for (int i = 0; i < numPlayers; i++)
        {
            var playerOrders = GameObject.Find($"Player{i}");
            for (int j = 0; j < 6; j++)
            {
                var orderButton = playerOrders.transform.Find($"Order{j}Button");
                orderButton.GetComponent<Button>().interactable = true;
                orderButton.transform.Find("Order").GetComponent<Image>().sprite = OrderSpirtes[(int)Ships[i].newOrders[j]];
                orderButton.GetComponent<Image>().color = i == 0 ? Color.green : i == 1 ? Color.cyan : Color.magenta;
                Ships[i].oldOrders[j] = Ships[i].newOrders[j];
            }
        }
    }

    public Vector3 GetShipCoords(Vector3 position)
    {
        return new Vector3(position.x, 0.1f, position.z);
    }
    private void CreateUI()
    {
        orderButtons.Add(OrderPanel.transform.Find("Order0Button").GetComponent<Button>());
        orderButtons.Add(OrderPanel.transform.Find("Order1Button").GetComponent<Button>());
        orderButtons.Add(OrderPanel.transform.Find("Order2Button").GetComponent<Button>());
        orderButtons.Add(OrderPanel.transform.Find("Order3Button").GetComponent<Button>());
        orderButtons.Add(OrderPanel.transform.Find("Order4Button").GetComponent<Button>());
        orderButtons.Add(OrderPanel.transform.Find("Order5Button").GetComponent<Button>());
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
                    var newHex = Instantiate(NodePrefab, new Vector3(x, 0, z), Quaternion.identity, HexCoordParent);
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
        OrderPanel.gameObject.SetActive(isBoardMaximized);
        ViewButton.GetComponentInChildren<TextMeshProUGUI>().text = isBoardMaximized ? "Show All" : "Show Yours";
    }
    public void ChangeNumber(int orderNumber)
    {
        var currentShip = Ships[playerTurn];
        if (currentShip.newOrders[orderNumber] != Orders.None)
        {
            var differences = FindDifferences(currentShip.newOrders, currentShip.oldOrders);
            if (differences.Count < 2 || differences.Contains(orderNumber))
            {
                var newOrderInt = ((int)currentShip.newOrders[orderNumber] + 1) % 6;
                orderButtons[orderNumber].transform.Find("Order").GetComponent<Image>().sprite = OrderSpirtes[newOrderInt];
                currentShip.newOrders[orderNumber] = (Orders)newOrderInt;
            }
            differences = FindDifferences(currentShip.newOrders, currentShip.oldOrders);
            for (int i = 0; i < 6; i++)
            {
                orderButtons[i].transform.Find("Selected").gameObject.SetActive(differences.Contains(i));
            }
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
