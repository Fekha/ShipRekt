using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShipManager : MonoBehaviour
{
    public int directionNum;
    public int x;
    public int y;
    internal bool isCPU;
    internal int id;
    internal string shipName;
    internal HexDirection direction;
    internal HexCoords currentPosition;
    private bool[] cannonsFinished = new bool[2] { false, false };
    internal Orders[] newOrders;
    internal Orders[] oldOrders;
    internal Treasure[] booty = new Treasure[6] { Treasure.None, Treasure.None, Treasure.None, Treasure.None, Treasure.None, Treasure.None };
    internal string[] PlayerColorNames = new string[] { "Green", "Pink", "Blue", "Yellow", "Red", "Orange" };
    private GameObject CannonBall;
    private float speedRot;
    private float speedMove;
    private float speedCannon;
    private GameObject explosionPrefab;
    Vector3 initialRotation;
    Vector3 initialPosition;

    public void CreateShip(HexCoords _currentPos, int _id)
    {
        id = _id;
        currentPosition = _currentPos;
        direction = (HexDirection)_currentPos.type;
        newOrders = new Orders[] { Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade };
        oldOrders = new Orders[] { Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade };
        CannonBall = Resources.Load<GameObject>("Prefabs/CannonBall");
        speedRot = 100f;
        speedMove = 1f;
        speedCannon = 2f;
        explosionPrefab = Resources.Load<GameObject>("Prefabs/Explosion");
        initialPosition = transform.position;
        shipName = PlayerColorNames[id];
        isCPU = id >= Settings.NumPlayers-Settings.NumNPCs;
    }
    internal IEnumerator Move(int orderNum)
    {
        GameManager.instance.ShipsDone[id] = false;
        var orderButton = GameObject.Find($"Player{id}").transform.Find($"Order{orderNum}Button");
        orderButton.GetComponent<Button>().interactable = false;
        switch (newOrders[orderNum])
        {
            case Orders.Evade:
                {
                    break;
                }
            case Orders.Move:
                {
                    HexCoords nextPosition = GetNextPosition(currentPosition, direction);
                    if (nextPosition == null)
                    {
                        LoseMovement(orderNum);
                    }
                    else 
                    {
                        var otherShipMovingToSameNode = GameManager.instance.Ships.Where(i => i != this && i.newOrders[orderNum] == Orders.Move && nextPosition.Compare(i.GetNextPosition(i.currentPosition, i.direction)));
                        ShipManager collidesWithShip = null;
                        foreach (var ship in GameManager.instance.Ships.Where(x => x != this)) {
                            if (nextPosition.Compare(ship.currentPosition))
                            {
                                if (ship.newOrders[orderNum] != Orders.Move || ship.GetNextPosition(ship.currentPosition, ship.direction) == null)
                                {
                                    collidesWithShip = ship;
                                }
                            }
                        }
                        if (collidesWithShip)
                        {
                            StartCoroutine(Ram());
                            StartCoroutine(collidesWithShip.TakeDamageFrom(this));
                            LoseMovement(orderNum);
                        }
                        else if (otherShipMovingToSameNode.Count() > 0)
                        {
                            foreach (var otherShip in otherShipMovingToSameNode)
                            {
                                otherShip.LoseMovement(orderNum);
                                StartCoroutine(otherShip.Ram());
                            }
                            LoseMovement(orderNum);
                            StartCoroutine(Ram());
                        }
                        else
                        {
                            var nodeToMoveTo = GameManager.instance.mapNodes[nextPosition.x, nextPosition.y];
                            var positionToEndAt = new Vector3(nodeToMoveTo.transform.position.x, .1f, nodeToMoveTo.transform.position.z);
                            var positionToMoveTo = nextPosition?.type == 1 ? transform.Find("Nose").transform.position : positionToEndAt;
                            while (positionToMoveTo != transform.position)
                            {
                                transform.position = Vector3.MoveTowards(transform.position, positionToMoveTo, speedMove * Time.deltaTime);
                                yield return null;
                            }
                            transform.position = positionToEndAt;
                            currentPosition = nextPosition;
                            CheckForTreasure(nextPosition);
                            initialPosition = transform.position;
                        }
                    }
                    break;
                }
            case Orders.TurnRight:
                {
                    var targetRotPos = Quaternion.Euler(90, (transform.rotation.eulerAngles.y + 60) % 360, 0);
                    direction = (HexDirection)(((int)direction + 1) % 6);
                    while (targetRotPos != transform.rotation)
                    {
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotPos, speedRot * Time.deltaTime);
                        yield return null;
                    }
                    break;
                }
            case Orders.TurnLeft:
                {
                    var targetRotNeg = Quaternion.Euler(90, transform.rotation.eulerAngles.y < 60 ? 300 + transform.rotation.eulerAngles.y : transform.rotation.eulerAngles.y - 60, 0);
                    direction = direction == HexDirection.BottomRight ? HexDirection.Right : direction - 1;
                    while (targetRotNeg != transform.rotation)
                    {
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotNeg, speedRot * Time.deltaTime);
                        yield return null;
                    }
                    break;
                }
            case Orders.ShootRight:
            {
                cannonsFinished = new bool[2] { false, false };
                for (int i = 1; i <= 2; i++)
                {
                    StartCoroutine(ShootCannonBall((HexDirection)(((int)direction + i) % 6),i-1, orderNum));
                }
                while(!cannonsFinished[0] || !cannonsFinished[1])
                {
                    yield return null;
                }
                break;
            }
            case Orders.ShootLeft:
            {
                cannonsFinished = new bool[2] { false, false };
                for (int i = 1; i <= 2; i++)
                {
                    StartCoroutine(ShootCannonBall((HexDirection)(((int)direction - i) < 0 ? (6 + (int)direction - i) : ((int)direction - i)),i-1, orderNum));
                }
                while (!cannonsFinished[0] || !cannonsFinished[1])
                {
                    yield return null;
                }
                break;
            }
            case Orders.None:
            {
                int bootyIndex = -1;
                for (int i = 6; i < 12; i++)
                {
                    if (booty.Contains((Treasure)i))
                    {
                        bootyIndex = booty.ToList().IndexOf((Treasure)i);
                        break;
                    }
                }
                if (bootyIndex != -1)
                {
                    yield return StartCoroutine(Heal(bootyIndex));
                }
                break;
            }
        }
        yield return new WaitForSeconds(.5f);
        GameManager.instance.ShipsDone[id] = true;
    }

    private void LoseMovement(int orderNum)
    {
        var orderButton = GameObject.Find($"Player{id}").transform.Find($"Order{orderNum}Button");
        newOrders[orderNum] = Orders.Evade;
        orderButton.transform.Find("Order").GetComponent<Image>().sprite = GameManager.instance.OrderSpirtes[(int)Orders.Evade];
    }

    private IEnumerator ShootCannonBall(HexDirection hexDirection, int i, int orderNum)
    {
        var cannonballPos = currentPosition;
        for (int j = 0; j < GameManager.instance.cannonRange; j++)
        {
            HexCoords nextPosition = GetNextPosition(cannonballPos, hexDirection);
            var hitShip = GameManager.instance.Ships.FirstOrDefault(i => i.currentPosition.Compare(nextPosition));
            if (hitShip)
            {
                if (hitShip.newOrders[orderNum] != Orders.Evade)
                {
                    j = GameManager.instance.cannonRange; //tells animation to stop
                    StartCoroutine(hitShip.TakeDamageFrom(this));
                }
                else
                {
                    StartCoroutine(hitShip.DodgeCannon());
                }
            }
            yield return StartCoroutine(AnimateCannonBall(cannonballPos, nextPosition, j, nextPosition?.type == 1));
            if (nextPosition == null)
            {
                break;
            }
            else
            {
                cannonballPos = nextPosition;
            }
        }
        cannonsFinished[i] = true;
    }
    private IEnumerator AddBooty(Treasure treasure)
    {
        for (int k = 0; k < 6; k++)
        {
            if (booty[k] == Treasure.None)
            {
                booty[k] = treasure;
                break;
            }
        }
        if ((int)treasure < 6)
        {
            var treasureObj = GameObject.Find($"Treasure/{(int)treasure}");
            var originalPos = GameObject.Find($"Treasure/{(int)treasure}").transform.position;
            treasureObj.transform.position = new Vector3(originalPos.x, .2f, originalPos.z);
            treasureObj.transform.localScale = new Vector3(.5f, .5f, .5f);
            yield return new WaitForSeconds(1f);
            treasureObj.transform.position = originalPos;
            treasureObj.transform.localScale = new Vector3(.3f, .3f, .3f);
        }  
    }
    private HexCoords GetNextPosition(HexCoords _currentPos, HexDirection _direction)
    {
        var nextPos = CheckTeleports(_currentPos, _direction);
        nextPos = CheckObsticals(_currentPos, nextPos);
        nextPos = CheckOB(nextPos);
        return nextPos;
    }
    private IEnumerator Ram()
    {
        yield return new WaitForSeconds(.25f);
        float moveSpeed = 2.0f; 
        Vector3 forwardPosition = transform.Find("Nose").transform.position;
        float startTime = Time.time;
        float journeyLength = Vector3.Distance(initialPosition, forwardPosition);
        float journeyTime = journeyLength / moveSpeed;

        // Move forward
        while (Time.time - startTime < journeyTime)
        {
            float distanceCovered = (Time.time - startTime) * moveSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;
            transform.position = Vector3.Lerp(initialPosition, forwardPosition, fractionOfJourney);
            yield return null;
        }

        // Ensure it reaches the exact forward position
        transform.position = forwardPosition;

        // Pause for a moment at the forward position (you can adjust the duration)
        yield return new WaitForSeconds(.1f);

        // Move backward to the initial position
        startTime = Time.time;
        while (Time.time - startTime < journeyTime)
        {
            float distanceCovered = (Time.time - startTime) * moveSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;
            transform.position = Vector3.Lerp(forwardPosition, initialPosition, fractionOfJourney);
            yield return null;
        }

        // Ensure it reaches the initial position
        transform.position = initialPosition;

        // You can repeat this movement by calling the coroutine again if needed
    }
    IEnumerator DodgeCannon()
    {
        initialRotation = transform.rotation.eulerAngles;
        yield return new WaitForSeconds(.25f);
        float spinDuration = 0.5f;
        float startTime = Time.time;
        float elapsedTime = 0f;
        Vector3 targetRotation = new Vector3(90f, 0f, initialRotation.z + 360f);

        while (elapsedTime < spinDuration)
        {
            elapsedTime = Time.time - startTime;
            float t = Mathf.Clamp01(elapsedTime / spinDuration); // Interpolate between 0 and 1
            transform.rotation = Quaternion.Euler(Vector3.Lerp(initialRotation, targetRotation, t));
            yield return null; // Wait for the next frame
        }

        // Ensure that the final rotation is exactly 360 degrees
        transform.rotation = Quaternion.Euler(initialRotation);
    }
    private IEnumerator TakeDamageFrom(ShipManager attacker)
    {
        for (int k = newOrders.Length - 1; k >= 0; k--)
        {
            if (newOrders[k] != Orders.None)
            {
                newOrders[k] = Orders.None;
                var orderButton = GameObject.Find($"Player{id}").transform.Find($"Order{k}Button");
                orderButton.transform.Find("Order").GetComponent<Image>().sprite = GameManager.instance.OrderSpirtes[(int)Orders.None];
                orderButton.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                break;
            }
        }
        StartCoroutine(attacker.AddBooty(id == 0 ? Treasure.GreenDie : id == 1 ? Treasure.BlueDie : Treasure.PinkDie));
        transform.Find("Skull").gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        transform.Find("Skull").gameObject.SetActive(false);
    }
    internal IEnumerator Heal(int bootyIndex)
    {
        booty[bootyIndex] = Treasure.None;
        var orderIndex = newOrders.ToList().IndexOf(Orders.None);
        newOrders[orderIndex] = Orders.Evade;
        transform.Find("Heal").gameObject.SetActive(true);
        var orderButton = GameObject.Find($"Player{id}").transform.Find($"Order{orderIndex}Button");
        orderButton.transform.Find("Order").GetComponent<Image>().sprite = GameManager.instance.OrderSpirtes[(int)Orders.Evade];
        orderButton.GetComponent<Image>().color = GameManager.instance.PlayerColors[id];
        yield return new WaitForSeconds(1f);
        transform.Find("Heal").gameObject.SetActive(false);
    }
    private HexCoords CheckOB(HexCoords nextPos)
    {
        if (nextPos != null)
        {
            if (nextPos.x >= 0 && nextPos.x < 11 && nextPos.y >= 0 && nextPos.y < 11)
            {
                if (GameManager.instance.mapNodes[nextPos.x, nextPos.y] != null)
                {
                    return nextPos;
                }
            }
        }
        return null;
    }

    private IEnumerator AnimateCannonBall(HexCoords currPosition, HexCoords nextPosition, int j, bool teleporting)
    {
        var currentNode = GameManager.instance.mapNodes[currPosition.x, currPosition.y].transform.position;
        if (nextPosition != null)
        {
            var cannonBall = Instantiate(CannonBall);
            var nodeToMoveTo = GameManager.instance.mapNodes[nextPosition.x, nextPosition.y];
            if (nodeToMoveTo != null)
            {
                var nodeToMoveToPos = nodeToMoveTo.transform.position;
                cannonBall.transform.position = currentNode;
                var positionToMoveTo = new Vector3(nodeToMoveToPos.x, .2f, nodeToMoveToPos.z);
                while (positionToMoveTo != cannonBall.transform.position && !teleporting)
                {
                    cannonBall.transform.position = Vector3.MoveTowards(cannonBall.transform.position, positionToMoveTo, speedCannon * Time.deltaTime);
                    yield return null;
                }
                cannonBall.transform.position = positionToMoveTo;
                if (j >= GameManager.instance.cannonRange-1)
                {
                    Instantiate(explosionPrefab, nodeToMoveToPos, Quaternion.identity);
                }
            }
            else
            {
                Instantiate(explosionPrefab, currentNode, Quaternion.identity);
            }
            Destroy(cannonBall); 
        }
        else
        {
            if(j != 0)
                Instantiate(explosionPrefab, currentNode, Quaternion.identity);
        }
    }

    private void CheckForTreasure(HexCoords nextPosition)
    {
        if (nextPosition.Compare(new HexCoords(2, 5)) && !booty.Contains(Treasure.Earring))
        {
            StartCoroutine(AddBooty(Treasure.Earring));
        }
        else if (nextPosition.Compare(new HexCoords(2, 8)) && !booty.Contains(Treasure.Necklace))
        {
            StartCoroutine(AddBooty(Treasure.Necklace));
        }
        else if (nextPosition.Compare(new HexCoords(5, 8)) && !booty.Contains(Treasure.Goblet))
        {
            StartCoroutine(AddBooty(Treasure.Goblet));
        }
        else if (nextPosition.Compare(new HexCoords(8, 5)) && !booty.Contains(Treasure.Ring))
        {
            StartCoroutine(AddBooty(Treasure.Ring));
        }
        else if (nextPosition.Compare(new HexCoords(8, 2)) && !booty.Contains(Treasure.Coins))
        {
            StartCoroutine(AddBooty(Treasure.Coins));
        }
        else if (nextPosition.Compare(new HexCoords(5, 2)) && !booty.Contains(Treasure.Crown))
        {
            StartCoroutine(AddBooty(Treasure.Crown));
        }
    }

    private HexCoords CheckObsticals(HexCoords _currentPosition, HexCoords _nextPosition)
    {
        var firstCords = new List<HexCoords>() { 
            new HexCoords(7, 0), //1
            new HexCoords(5, 1), //2
            new HexCoords(5, 2), //3
            new HexCoords(5, 2), //4
            new HexCoords(3, 4), //5
            new HexCoords(1, 6), //6
            new HexCoords(0, 8), //7
            new HexCoords(2, 8), //8
            new HexCoords(2, 8), //9
            new HexCoords(5, 7), //10
            new HexCoords(6, 7), //11
            new HexCoords(5, 9), //12
            new HexCoords(7, 6), //13
            new HexCoords(8, 5), //14
            new HexCoords(9, 4), //15
            new HexCoords(9, 1), //17
            new HexCoords(8, 2), //18
            new HexCoords(5, 4), //19
            new HexCoords(4, 5), //20
            new HexCoords(6, 5), //21
            new HexCoords(5, 6), //22
            new HexCoords(5, 9), //23
        };
        var secondCords = new List<HexCoords>() { 
            new HexCoords(6, 0), //1
            new HexCoords(5, 2), //2
            new HexCoords(4, 3), //3
            new HexCoords(6, 1), //4
            new HexCoords(2, 5), //5
            new HexCoords(0, 6), //6
            new HexCoords(0, 9), //7
            new HexCoords(2, 7), //8
            new HexCoords(3, 7), //9
            new HexCoords(5, 8), //10
            new HexCoords(5, 8), //11
            new HexCoords(6, 9), //12
            new HexCoords(8, 5), //13
            new HexCoords(8, 4), //14
            new HexCoords(10, 4),//15
            new HexCoords(8, 2), //17
            new HexCoords(7, 3), //18
            new HexCoords(5, 5), //19
            new HexCoords(5, 5), //20
            new HexCoords(5, 5), //21
            new HexCoords(5, 5), //22
            new HexCoords(5, 8), //23
        };
        if (_nextPosition != null)
        {
            for (int i = 0; i < firstCords.Count; i++)
            {
                if ((_currentPosition.Compare(firstCords[i]) || _currentPosition.Compare(secondCords[i])) && (_nextPosition.Compare(firstCords[i]) || _nextPosition.Compare(secondCords[i])))
                {
                    return null;
                }
            }
        }
        return _nextPosition;
    }
    
    private HexCoords CheckTeleports(HexCoords _currentPos, HexDirection _direction)
    {
        if (_currentPos.Compare(new HexCoords(8, 0)) && _direction == HexDirection.TopLeft)
        {
            return new HexCoords(0, 8, 1);
        }
        else if (_currentPos.Compare(new HexCoords(0, 8)) && _direction == HexDirection.BottomRight)
        {
            return new HexCoords(8, 0, 1);
        }
        else if (_currentPos.Compare(new HexCoords(9, 1)) && _direction == HexDirection.TopLeft)
        {
            return new HexCoords(1, 9, 1);
        }
        else if (_currentPos.Compare(new HexCoords(1, 9)) && _direction == HexDirection.BottomRight)
        {
            return new HexCoords(9, 1, 1);
        }
        else if (_currentPos.Compare(new HexCoords(10, 2)) && _direction == HexDirection.TopLeft)
        {
            return new HexCoords(2, 10, 1);
        }
        else if (_currentPos.Compare(new HexCoords(2, 10)) && _direction == HexDirection.BottomRight)
        {
            return new HexCoords(10, 2, 1);
        }
        else if (_currentPos.Compare(new HexCoords(7, 0)) && _direction == HexDirection.Left)
        {
            return new HexCoords(7, 8, 1);
        }
        else if (_currentPos.Compare(new HexCoords(7, 8)) && _direction == HexDirection.Right)
        {
            return new HexCoords(7, 0, 1);
        }
        else if (_currentPos.Compare(new HexCoords(5, 1)) && _direction == HexDirection.Left)
        {
            return new HexCoords(5, 9, 1);
        }
        else if (_currentPos.Compare(new HexCoords(5, 9)) && _direction == HexDirection.Right)
        {
            return new HexCoords(5, 1, 1);
        }
        else if (_currentPos.Compare(new HexCoords(3, 2)) && _direction == HexDirection.Left)
        {
            return new HexCoords(3, 10, 1);
        }
        else if (_currentPos.Compare(new HexCoords(3, 10)) && _direction == HexDirection.Right)
        {
            return new HexCoords(3, 2, 1);
        } 
        else if (_currentPos.Compare(new HexCoords(2, 3)) && _direction == HexDirection.BottomLeft)
        {
            return new HexCoords(10, 3, 1);
        }
        else if (_currentPos.Compare(new HexCoords(10, 3)) && _direction == HexDirection.TopRight)
        {
            return new HexCoords(2, 3, 1);
        } 
        else if (_currentPos.Compare(new HexCoords(1, 5)) && _direction == HexDirection.BottomLeft)
        {
            return new HexCoords(9, 5, 1);
        }
        else if (_currentPos.Compare(new HexCoords(9, 5)) && _direction == HexDirection.TopRight)
        {
            return new HexCoords(1, 5, 1);
        } 
        else if (_currentPos.Compare(new HexCoords(0, 7)) && _direction == HexDirection.BottomLeft)
        {
            return new HexCoords(8, 7, 1);
        }
        else if (_currentPos.Compare(new HexCoords(8, 7)) && _direction == HexDirection.TopRight)
        {
            return new HexCoords(0, 7, 1);
        }
        else if (_currentPos.Compare(new HexCoords(9, 0)) && _direction == HexDirection.TopLeft)
        {
            return new HexCoords(0, 9, 1);
        }
        else if (_currentPos.Compare(new HexCoords(0, 9)) && _direction == HexDirection.BottomRight)
        {
            return new HexCoords(9, 0, 1);
        }
        else if (_currentPos.Compare(new HexCoords(6, 0)) && _direction == HexDirection.Left)
        {
            return new HexCoords(6, 9, 1);
        }
        else if (_currentPos.Compare(new HexCoords(6, 9)) && _direction == HexDirection.Right)
        {
            return new HexCoords(6, 0, 1);
        }
        return new HexCoords(_currentPos.x + HexDirections.directionMap[_direction].x, _currentPos.y + HexDirections.directionMap[_direction].y);
    }
}


