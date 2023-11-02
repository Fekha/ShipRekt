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
    internal int id;
    internal HexDirection direction;
    internal HexCoords currentPosition;
    private bool[] cannonsFinished = new bool[2] { false, false };
    internal Orders[] newOrders;
    internal Orders[] oldOrders;
    internal Treasure[] booty = new Treasure[6] { Treasure.None, Treasure.None, Treasure.None, Treasure.None, Treasure.None, Treasure.None };
    private GameObject CannonBall;
    private float speedRot;
    private float speedMove;
    private float speedCannon;
    private GameObject explosionPrefab;

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
    }
    internal IEnumerator Move(int orderNum)
    {
        GameManager.i.ShipsDone[id] = false;
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
                    var teleporting = nextPosition == null;
                    if (teleporting)
                    {
                        nextPosition = CheckTeleports(currentPosition, direction);
                        if (nextPosition == null)
                        {
                            newOrders[orderNum] = Orders.Evade;
                            orderButton.transform.Find("Order").GetComponent<Image>().sprite = GameManager.i.OrderSpirtes[(int)Orders.Evade];
                        }
                    }
                    if (nextPosition != null)
                    {
                        var nodeToMoveTo = GameManager.i.mapNodes[nextPosition.x, nextPosition.y];
                        var positionToEndAt = new Vector3(nodeToMoveTo.transform.position.x, .1f, nodeToMoveTo.transform.position.z);
                        var positionToMoveTo = teleporting ? transform.Find("Nose").transform.position : positionToEndAt;
                        while (positionToMoveTo != transform.position)
                        {
                            transform.position = Vector3.MoveTowards(transform.position, positionToMoveTo, speedMove * Time.deltaTime);
                            yield return null;
                        }
                        transform.position = positionToEndAt;
                        currentPosition = nextPosition;
                        CheckForTreasure(nextPosition);
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
                break;
            }
        }
        yield return new WaitForSeconds(.5f);
        GameManager.i.ShipsDone[id] = true;
    }

    private IEnumerator ShootCannonBall(HexDirection hexDirection, int i, int orderNum)
    {
        var cannonballPos = currentPosition;
        for (int j = 0; j < 3; j++)
        {
            HexCoords nextPosition = GetNextPosition(cannonballPos, hexDirection);
            var teleporting = nextPosition == null;
            if (teleporting)
            {
                nextPosition = CheckTeleports(cannonballPos, hexDirection);
            }
            var hitShip = GameManager.i.Ships.FirstOrDefault(i => i.currentPosition.Compare(nextPosition) && i.newOrders[orderNum] != Orders.Evade);
            if (hitShip)
            {
                j = 2; //tells animation to stop
                for (int k = hitShip.newOrders.Length-1; k >= 0 ; k--)
                {
                    if(hitShip.newOrders[k] != Orders.None)
                    {
                        hitShip.newOrders[k] = Orders.None;
                        GameObject.Find($"Player{hitShip.id}").transform.Find($"Order{k}Button").transform.Find("Order").GetComponent<Image>().sprite = GameManager.i.OrderSpirtes[(int)Orders.None];
                        break;
                    }
                }
                AddBooty(hitShip.id == 0 ? Treasure.GreenDie : hitShip.id == 1 ? Treasure.BlueDie : Treasure.PinkDie);
            }
            yield return StartCoroutine(AnimateCannonBall(cannonballPos, nextPosition, j, teleporting));
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

    private void AddBooty(Treasure treasure)
    {
        for (int k = 0; k < 6; k++)
        {
            if (booty[k] == Treasure.None)
            {
                booty[k] = treasure;
                break;
            }
        }
    }

    private HexCoords GetNextPosition(HexCoords _currentPos, HexDirection _direction)
    {
        var nextPos = new HexCoords(_currentPos.x + HexDirections.directionMap[_direction].x, _currentPos.y + HexDirections.directionMap[_direction].y);
        nextPos = CheckObsticals(_currentPos, nextPos);
        nextPos = CheckOB(nextPos);
        if(nextPos == null || GameManager.i.mapNodes[nextPos.x, nextPos.y] == null)
            return null;
        return nextPos;
    }

    private HexCoords CheckOB(HexCoords nextPos)
    {
        if (nextPos != null)
        {
            if (nextPos.x >= 0 && nextPos.x < 11 && nextPos.y >= 0 && nextPos.y < 11)
            {
                return nextPos;
            }
        }
        return null;
    }

    private IEnumerator AnimateCannonBall(HexCoords currPosition, HexCoords nextPosition, int j, bool teleporting)
    {
        var currentNode = GameManager.i.mapNodes[currPosition.x, currPosition.y].transform.position;
        if (nextPosition != null)
        {
            var cannonBall = Instantiate(CannonBall);
            var nodeToMoveTo = GameManager.i.mapNodes[nextPosition.x, nextPosition.y];
            if (nodeToMoveTo != null)
            {
                var nodeToMoveToPos = nodeToMoveTo.transform.position;
                cannonBall.transform.position = currentNode;
                var positionToMoveTo = new Vector3(nodeToMoveToPos.x, .1f, nodeToMoveToPos.z);
                while (positionToMoveTo != cannonBall.transform.position && !teleporting)
                {
                    cannonBall.transform.position = Vector3.MoveTowards(cannonBall.transform.position, positionToMoveTo, speedCannon * Time.deltaTime);
                    yield return null;
                }
                cannonBall.transform.position = positionToMoveTo;
                if (j == 2)
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
            AddBooty(Treasure.Earring);
        }
        else if (nextPosition.Compare(new HexCoords(2, 8)) && !booty.Contains(Treasure.Necklace))
        {
            AddBooty(Treasure.Necklace);
        }
        else if (nextPosition.Compare(new HexCoords(5, 8)) && !booty.Contains(Treasure.Goblet))
        {
            AddBooty(Treasure.Goblet);
        }
        else if (nextPosition.Compare(new HexCoords(8, 5)) && !booty.Contains(Treasure.Ring))
        {
            AddBooty(Treasure.Ring);
        }
        else if (nextPosition.Compare(new HexCoords(8, 2)) && !booty.Contains(Treasure.Coins))
        {
            AddBooty(Treasure.Coins);
        }
        else if (nextPosition.Compare(new HexCoords(5, 2)) && !booty.Contains(Treasure.Crown))
        {
            AddBooty(Treasure.Crown);
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
            new HexCoords(10, 1),//16
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
            new HexCoords(10, 2),//16
            new HexCoords(8, 2), //17
            new HexCoords(7, 3), //18
            new HexCoords(5, 5), //19
            new HexCoords(5, 5), //20
            new HexCoords(5, 5), //21
            new HexCoords(5, 5), //22
            new HexCoords(5, 8), //23
        };
        return DoesEitherEqual(_currentPosition, _nextPosition, firstCords, secondCords);
    }

    private HexCoords DoesEitherEqual(HexCoords _currentPos, HexCoords _nextPosition, List<HexCoords> hexCoords1, List<HexCoords> hexCoords2)
    {
        for (int i = 0; i < hexCoords1.Count; i++)
        {
            if ((_currentPos.Compare(hexCoords1[i]) || _currentPos.Compare(hexCoords2[i])) && (_nextPosition.Compare(hexCoords1[i]) || _nextPosition.Compare(hexCoords2[i])))
            {
                return null;
            }
        }
        return _nextPosition;
    }

    private HexCoords CheckTeleports(HexCoords _currentPos, HexDirection _direction)
    {
        if (_currentPos.Compare(new HexCoords(8, 0)) && _direction == HexDirection.TopLeft)
        {
            return new HexCoords(0, 8);
        }
        else if (_currentPos.Compare(new HexCoords(0, 8)) && _direction == HexDirection.BottomRight)
        {
            return new HexCoords(8, 0);
        }
        else if (_currentPos.Compare(new HexCoords(9, 1)) && _direction == HexDirection.TopLeft)
        {
            return new HexCoords(1, 9);
        }
        else if (_currentPos.Compare(new HexCoords(1, 9)) && _direction == HexDirection.BottomRight)
        {
            return new HexCoords(9, 1);
        }
        else if (_currentPos.Compare(new HexCoords(10, 2)) && _direction == HexDirection.TopLeft)
        {
            return new HexCoords(2, 10);
        }
        else if (_currentPos.Compare(new HexCoords(2, 10)) && _direction == HexDirection.BottomRight)
        {
            return new HexCoords(10, 2);
        }
        else if (_currentPos.Compare(new HexCoords(7, 0)) && _direction == HexDirection.Left)
        {
            return new HexCoords(7, 8);
        }
        else if (_currentPos.Compare(new HexCoords(7, 8)) && _direction == HexDirection.Right)
        {
            return new HexCoords(7, 0);
        }
        else if (_currentPos.Compare(new HexCoords(5, 1)) && _direction == HexDirection.Left)
        {
            return new HexCoords(5, 9);
        }
        else if (_currentPos.Compare(new HexCoords(5, 9)) && _direction == HexDirection.Right)
        {
            return new HexCoords(5, 1);
        }
        else if (_currentPos.Compare(new HexCoords(3, 2)) && _direction == HexDirection.Left)
        {
            return new HexCoords(3, 10);
        }
        else if (_currentPos.Compare(new HexCoords(3, 10)) && _direction == HexDirection.Right)
        {
            return new HexCoords(3, 2);
        } 
        else if (_currentPos.Compare(new HexCoords(2, 3)) && _direction == HexDirection.BottomLeft)
        {
            return new HexCoords(10, 3);
        }
        else if (_currentPos.Compare(new HexCoords(10, 3)) && _direction == HexDirection.TopRight)
        {
            return new HexCoords(2, 3);
        } 
        else if (_currentPos.Compare(new HexCoords(1, 5)) && _direction == HexDirection.BottomLeft)
        {
            return new HexCoords(9, 5);
        }
        else if (_currentPos.Compare(new HexCoords(9, 5)) && _direction == HexDirection.TopRight)
        {
            return new HexCoords(1, 5);
        } 
        else if (_currentPos.Compare(new HexCoords(0, 7)) && _direction == HexDirection.BottomLeft)
        {
            return new HexCoords(8, 7);
        }
        else if (_currentPos.Compare(new HexCoords(8, 7)) && _direction == HexDirection.TopRight)
        {
            return new HexCoords(0, 7);
        }
        else if (_currentPos.Compare(new HexCoords(9, 0)) && _direction == HexDirection.TopLeft)
        {
            return new HexCoords(0, 9);
        }
        else if (_currentPos.Compare(new HexCoords(0, 9)) && _direction == HexDirection.BottomRight)
        {
            return new HexCoords(9, 0);
        }
        else if (_currentPos.Compare(new HexCoords(6, 0)) && _direction == HexDirection.Left)
        {
            return new HexCoords(6, 9);
        }
        else if (_currentPos.Compare(new HexCoords(6, 9)) && _direction == HexDirection.Right)
        {
            return new HexCoords(6, 0);
        }
        return null;
    }
}


