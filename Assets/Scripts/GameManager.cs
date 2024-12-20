using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    internal static GameManager instance;
    internal List<ShipManager> Ships = new List<ShipManager>();
    internal ShipManager CurrentShip = new ShipManager();
    private GameObject NodePrefab;
    private GameObject OrderPanel;
    private GameObject Booty;
    private GameObject ViewButton;
    public GameObject MainCanvas;
    Transform HexCoordParent;
    internal List<Button> orderButtons = new List<Button>();
    internal GameObject[,] mapNodes;
    private bool isBoardMaximized = true;
    public List<HexCoords> Spawns = new List<HexCoords>() { new HexCoords(2, 5, 0, 4), new HexCoords(2, 8, 5, 3), new HexCoords(5, 8, 4, 2), new HexCoords(8, 5, 3, 1), new HexCoords(8,2,2,0), new HexCoords(5,2,1,5)};
    private int playerTurn = 0;
    private int numPlayers = Settings.NumPlayers;
    internal Sprite[] OrderSpirtes;
    internal bool[] ShipsDone;
    internal int cannonRange = 2;
    internal Color[] PlayerColors = new Color[] { Color.green, Color.magenta, Color.cyan, Color.yellow, Color.red, new Color(1.0f, 0.64f, 0.0f) };
    void Start()
    {
        instance = this;
        ShipsDone = new bool[numPlayers];
        ShipsDone.Select(x => false).ToArray();
        MainCanvas.SetActive(true);
        cannonRange = numPlayers == 2 ? 4 : numPlayers < 5 ? 3 : 2;
        OrderSpirtes = new Sprite[] { Resources.Load<Sprite>("Sprites/Orders/Evade"), Resources.Load<Sprite>("Sprites/Orders/Move"), Resources.Load<Sprite>("Sprites/Orders/TurnLeft"), Resources.Load<Sprite>("Sprites/Orders/TurnRight"), Resources.Load<Sprite>("Sprites/Orders/ShootLeft"), Resources.Load<Sprite>("Sprites/Orders/ShootRight"), Resources.Load<Sprite>("Sprites/Orders/None") };
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
            MainCanvas.transform.Find($"OrderView/Viewport/Content/Player{i}").gameObject.SetActive(true);
            MainCanvas.transform.Find($"LeaderboardPanel/Player{i}").gameObject.SetActive(true);
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
            if (CurrentShip.isCPU)
            {
                StartCoroutine(AIManager.TakeTurn());
            }
        }
        else
        {
            CurrentShip = null;
            StartCoroutine(TakeOrders());
        }
    }
    public void BackToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene");
    }
    public void ShowLeaderboard()
    {
        foreach(var ship in Ships)
        {
            var player = MainCanvas.transform.Find($"LeaderboardPanel/Player{ship.id}");
            for (int i = 0; i < ship.booty.Count(); i++)
            {
                player.Find($"Booty{i}").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/Treasure/{(int)ship.booty[i]}");
            }
        }
        MainCanvas.transform.Find("LeaderboardPanel").gameObject.SetActive(true);
    }
    private IEnumerator TakeOrders()
    {
        playerTurn = 0;
        SetAllPlayersOrders();
        OrderPanel.SetActive(false);
        ViewButton.SetActive(false);
        for (int i = 0; i < 6; i++)
        {
            var ShipsToMove = Ships.Where(x => x.newOrders[i] != Orders.None && x.newOrders[i] != Orders.ShootLeft && x.newOrders[i] != Orders.ShootRight).ToList();
            foreach (var ship in ShipsToMove)
            {
                StartCoroutine(ship.Move(i));
            }
            while (ShipsToMove.Any(x => !ShipsDone[x.id]))
            {
                yield return null;
            }

            ShipsToMove = Ships.Where(x => x.newOrders[i] == Orders.ShootLeft || x.newOrders[i] == Orders.ShootRight).ToList();
            foreach (var ship in ShipsToMove)
            {
                StartCoroutine(ship.Move(i));
            }
            while (ShipsToMove.Any(x => !ShipsDone[x.id]))
            {
                yield return null;
            }

            ShipsToMove = Ships.Where(x => x.newOrders[i] == Orders.None).ToList();
            ShipsToMove.Select(x => ShipsDone[x.id] = false);
            foreach (var ship in ShipsToMove)
            {
                StartCoroutine(ship.Move(i));
            }
            while (ShipsDone.Any(x => !x))
            {
                yield return null;
            }
            ShipsDone.Select(x => false).ToArray();
        }
        var shipsToDestroy = Ships.Where(x => x.newOrders.All(y => y == Orders.None)).ToList();
        for(int i = 0; i < shipsToDestroy.Count; i++)
        {
            MainCanvas.transform.Find($"OrderView/Viewport/Content/Player{shipsToDestroy[i].id}").gameObject.SetActive(false);
            MainCanvas.transform.Find($"LeaderboardPanel/Player{shipsToDestroy[i].id}").gameObject.SetActive(false);
            numPlayers--;
            Destroy(shipsToDestroy[i].gameObject);
            Ships.Remove(shipsToDestroy[i]);
        }
        if (Ships.Any(x => !x.booty.Any(y => y == Treasure.None)) || Ships.Count <= 1)
        {
            var gameOverPanel = MainCanvas.transform.Find("GameOver");
            var gameOverText = gameOverPanel.Find("GameOverText").GetComponent<TextMeshProUGUI>();
            if (Ships.Count == 1)
            {
                gameOverText.text = $"{Ships[0].shipName} Player Wins!";
            }
            else if (Ships.Count == 0)
            {
                gameOverText.text = $"Everyone Loses!";
            }
            else
            {
                //Delcare winners that have all 6 treasure 
                foreach(var ship in Ships.Where(x => x.booty.All(y => y != Treasure.None)))
                {
                    gameOverText.text += $"{ship.shipName} Player Wins!\n";
                }
            }
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

    private void SetBoard()
    {
        CurrentShip = Ships[playerTurn];
        for (int i = 0; i < orderButtons.Count; i++)
        {
            orderButtons[i].transform.Find("Order").GetComponent<Image>().sprite = OrderSpirtes[(int)CurrentShip.oldOrders[i]];
            if (CurrentShip.oldOrders[i] == Orders.None) {
                orderButtons[i].GetComponent<Image>().color = new Color(0, 0, 0, 0);
            } else {
                orderButtons[i].GetComponent<Image>().color = PlayerColors[CurrentShip.id];
            }
            Booty.transform.Find($"Booty{i}").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/Treasure/{(int)CurrentShip.booty[i]}");
        }
    }

    private void SetAllPlayersOrders()
    {
        foreach(var ship in Ships)
        {
            var playerOrders = GameObject.Find($"OrderView/Viewport/Content/Player{ship.id}");
            for (int j = 0; j < 6; j++)
            {
                var orderButton = playerOrders.transform.Find($"Order{j}Button");
                orderButton.GetComponent<Button>().interactable = true;
                orderButton.transform.Find("Order").GetComponent<Image>().sprite = OrderSpirtes[(int)ship.newOrders[j]];
                if (ship.newOrders[j] == Orders.None)
                {
                    orderButton.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                }
                else
                {
                    orderButton.GetComponent<Image>().color = PlayerColors[ship.id];
                }
                ship.oldOrders[j] = ship.newOrders[j];
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
    public void PlayerChangeOrder(int orderNumber)
    {
        ChangeOrder(orderNumber, ((int)CurrentShip.newOrders[orderNumber] + 1) % 6);
    }

    public void ChangeOrder(int orderNumber, int numberToChangeTo)
    {
        if (CurrentShip.newOrders[orderNumber] != Orders.None)
        {
            var differences = FindDifferences(CurrentShip.newOrders, CurrentShip.oldOrders);
            if (differences.Count < 2 || differences.Contains(orderNumber))
            {
                orderButtons[orderNumber].transform.Find("Order").GetComponent<Image>().sprite = OrderSpirtes[(int)numberToChangeTo];
                CurrentShip.newOrders[orderNumber] = (Orders)numberToChangeTo;
            }
            differences = FindDifferences(CurrentShip.newOrders, CurrentShip.oldOrders);
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
