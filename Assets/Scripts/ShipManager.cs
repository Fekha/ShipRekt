using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipManager : MonoBehaviour
{
    public int directionNum;
    public int x;
    public int y;
    int id;
    internal HexDirection direction;
    internal HexCoords currentPos;
    private bool travelAround = true;
    internal Orders[] newOrders;
    internal Orders[] oldOrders;
    internal List<Treasure> booty = new List<Treasure>();
    private GameObject CannonBall;
    private float speedRot;
    private float speedMove;
    private GameObject explosionPrefab;

    public void CreateShip(HexCoords _currentPos, int _id)
    {
        id = _id;
        currentPos = _currentPos;
        direction = (HexDirection)_currentPos.type;
        newOrders = new Orders[] { Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade };
        oldOrders = new Orders[] { Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade };
        CannonBall = Resources.Load<GameObject>("Prefabs/CannonBall");
        speedRot = 100f;
        speedMove = 1f;
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
                    travelAround = true;
                    HexCoords nextPosition = CheckTeleports(currentPos, direction);
                    Vector3 positionToMoveTo = transform.Find("Nose").transform.position;
                    if (nextPosition == null)
                    {
                        nextPosition = GetNextPosition(currentPos, direction);
                        if (nextPosition != null)
                        {
                            var nodeToMoveTo = GameManager.i.mapNodes[nextPosition.x, nextPosition.y];
                            positionToMoveTo = new Vector3(nodeToMoveTo.transform.position.x, .1f, nodeToMoveTo.transform.position.z);
                        }
                        else
                        {
                            newOrders[orderNum] = Orders.Evade;
                            orderButton.transform.Find("Order").GetComponent<Image>().sprite = GameManager.i.OrderSpirtes[(int)Orders.Evade];
                        }
                    }
                    if (nextPosition != null)
                    {
                        while (positionToMoveTo != transform.position)
                        {
                            transform.position = Vector3.MoveTowards(transform.position, positionToMoveTo, speedMove * Time.deltaTime);
                            yield return null;
                        }
                        var nodeToMoveTo2 = GameManager.i.mapNodes[nextPosition.x, nextPosition.y];
                        transform.position = new Vector3(nodeToMoveTo2.transform.position.x, .1f, nodeToMoveTo2.transform.position.z);
                        currentPos = nextPosition;
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
                    for (int i = 1; i <= 2; i++)
                    {
                        HexCoords nextPosition = GetNextPosition(currentPos, (HexDirection)(((int)direction + i) % 6)); 
                        if (nextPosition != null)
                        {
                            StartCoroutine(ShootCannonBall(GameManager.i.mapNodes[nextPosition.x, nextPosition.y]));
                        }
                    }
                    break;
                }
            case Orders.ShootLeft:
            {
                break;
            }

        }
        for (int i = 0; i < newOrders.Length; i++)
        {
            oldOrders[i] = newOrders[i];
        }
        yield return new WaitForSeconds(.5f);
        GameManager.i.ShipsDone[id] = true;
    }

    private HexCoords GetNextPosition(HexCoords _currentPos, HexDirection _direction)
    {
        var nextPos = new HexCoords(_currentPos.x + HexDirections.directionMap[_direction].x, _currentPos.y + HexDirections.directionMap[_direction].y);
        nextPos = CheckObsticals(_currentPos, nextPos);
        nextPos = CheckOB(nextPos);
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

    private IEnumerator ShootCannonBall(GameObject nodeToMoveTo)
    {
        if (nodeToMoveTo != null)
        {
            var cannonBall = Instantiate(CannonBall);
            cannonBall.transform.position = transform.position;
            var positionToMoveTo = new Vector3(nodeToMoveTo.transform.position.x, .1f, nodeToMoveTo.transform.position.z);
            while (positionToMoveTo != cannonBall.transform.position)
            {
                cannonBall.transform.position = Vector3.MoveTowards(cannonBall.transform.position, positionToMoveTo, speedMove * Time.deltaTime);
                yield return null;
            }
            Instantiate(explosionPrefab, cannonBall.transform.position, Quaternion.identity);
            Destroy(cannonBall);
        }
    }

    private void CheckForTreasure(HexCoords nextPosition)
    {
        if (nextPosition.Compare(new HexCoords(2, 5)) && !booty.Contains(Treasure.Earring))
        {
            booty.Add(Treasure.Earring);
        }
        else if (nextPosition.Compare(new HexCoords(2, 8)) && !booty.Contains(Treasure.Necklace))
        {
            booty.Add(Treasure.Necklace);
        }
        else if (nextPosition.Compare(new HexCoords(5, 8)) && !booty.Contains(Treasure.Goblet))
        {
            booty.Add(Treasure.Goblet);
        }
        else if (nextPosition.Compare(new HexCoords(8, 5)) && !booty.Contains(Treasure.Ring))
        {
            booty.Add(Treasure.Ring);
        }
        else if (nextPosition.Compare(new HexCoords(8, 2)) && !booty.Contains(Treasure.Coins))
        {
            booty.Add(Treasure.Coins);
        }
        else if (nextPosition.Compare(new HexCoords(5, 2)) && !booty.Contains(Treasure.Crown))
        {
            booty.Add(Treasure.Crown);
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
            new HexCoords(6, 8), //12
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
        else if (_currentPos.Compare(new HexCoords(9, 0)) && _direction == HexDirection.BottomRight)
        {
            return new HexCoords(0, 9);
        }
        else if (_currentPos.Compare(new HexCoords(0, 9)) && _direction == HexDirection.TopLeft)
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
        travelAround = false;
        return null;
    }
}


