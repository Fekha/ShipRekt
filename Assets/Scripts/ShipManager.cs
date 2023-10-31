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
    public void CreateShip(HexCoords _currentPos, int _id)
    {
        id = _id;
        currentPos = _currentPos;
        direction = (HexDirection)_currentPos.type;
        newOrders = new Orders[] { Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade };
        oldOrders = new Orders[] { Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade, Orders.Evade };
    }
    internal IEnumerator Move(int orderNum)
    {
        GameManager.i.ShipsDone[id] = false;
        var speedRot = 100f;
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
                    var speedMove = 1f;
                    travelAround = true;
                    HexCoords nextPosition = GetNextPosition();
                    if (!WillHitObstical(nextPosition) && nextPosition.x >= 0 && nextPosition.x < 11 && nextPosition.y >= 0 && nextPosition.y < 11)
                    {
                        var nodeToMoveTo = GameManager.i.mapNodes[nextPosition.x, nextPosition.y];
                        if (nodeToMoveTo != null)
                        {
                            var realPosition = new Vector3(nodeToMoveTo.transform.position.x, .1f, nodeToMoveTo.transform.position.z);
                            var positionToMoveTo = travelAround ? transform.Find("Nose").transform.position : realPosition;
                            while (positionToMoveTo != transform.position)
                            {
                                transform.position = Vector3.MoveTowards(transform.position, positionToMoveTo, speedMove * Time.deltaTime);
                                yield return null;
                            }
                            transform.position = realPosition;
                            currentPos = nextPosition;
                            CheckForTreasure(nextPosition);
                        }
                        else
                        {
                            newOrders[orderNum] = Orders.Evade;
                            orderButton.transform.Find("Order").GetComponent<Image>().sprite = GameManager.i.OrderSpirtes[(int)Orders.Evade];
                        }
                    }
                    else
                    {
                        newOrders[orderNum] = Orders.Evade;
                        orderButton.transform.Find("Order").GetComponent<Image>().sprite = GameManager.i.OrderSpirtes[(int)Orders.Evade];
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

    private bool WillHitObstical(HexCoords nextPosition)
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
        return DoesEitherEqual(currentPos, nextPosition, firstCords, secondCords);
    }

    private bool DoesEitherEqual(HexCoords currentPos, HexCoords nextPosition, List<HexCoords> hexCoords1, List<HexCoords> hexCoords2)
    {
        for (int i = 0; i < hexCoords1.Count; i++)
        {
            if ((currentPos.Compare(hexCoords1[i]) || currentPos.Compare(hexCoords2[i])) && (nextPosition.Compare(hexCoords1[i]) || nextPosition.Compare(hexCoords2[i])))
            {
                return true;
            }
        }
        return false;
    }

    private HexCoords GetNextPosition()
    {
        if (currentPos.Compare(new HexCoords(8, 0)) && direction == HexDirection.TopLeft)
        {
            return new HexCoords(0, 8);
        }
        else if (currentPos.Compare(new HexCoords(0, 8)) && direction == HexDirection.BottomRight)
        {
            return new HexCoords(8, 0);
        }
        else if (currentPos.Compare(new HexCoords(9, 1)) && direction == HexDirection.TopLeft)
        {
            return new HexCoords(1, 9);
        }
        else if (currentPos.Compare(new HexCoords(1, 9)) && direction == HexDirection.BottomRight)
        {
            return new HexCoords(9, 1);
        }
        else if (currentPos.Compare(new HexCoords(10, 2)) && direction == HexDirection.TopLeft)
        {
            return new HexCoords(2, 10);
        }
        else if (currentPos.Compare(new HexCoords(2, 10)) && direction == HexDirection.BottomRight)
        {
            return new HexCoords(10, 2);
        }
        else if (currentPos.Compare(new HexCoords(7, 0)) && direction == HexDirection.Left)
        {
            return new HexCoords(7, 8);
        }
        else if (currentPos.Compare(new HexCoords(7, 8)) && direction == HexDirection.Right)
        {
            return new HexCoords(7, 0);
        }
        else if (currentPos.Compare(new HexCoords(5, 1)) && direction == HexDirection.Left)
        {
            return new HexCoords(5, 9);
        }
        else if (currentPos.Compare(new HexCoords(5, 9)) && direction == HexDirection.Right)
        {
            return new HexCoords(5, 1);
        }
        else if (currentPos.Compare(new HexCoords(3, 2)) && direction == HexDirection.Left)
        {
            return new HexCoords(3, 10);
        }
        else if (currentPos.Compare(new HexCoords(3, 10)) && direction == HexDirection.Right)
        {
            return new HexCoords(3, 2);
        } 
        else if (currentPos.Compare(new HexCoords(2, 3)) && direction == HexDirection.BottomLeft)
        {
            return new HexCoords(10, 3);
        }
        else if (currentPos.Compare(new HexCoords(10, 3)) && direction == HexDirection.TopRight)
        {
            return new HexCoords(2, 3);
        } 
        else if (currentPos.Compare(new HexCoords(1, 5)) && direction == HexDirection.BottomLeft)
        {
            return new HexCoords(9, 5);
        }
        else if (currentPos.Compare(new HexCoords(9, 5)) && direction == HexDirection.TopRight)
        {
            return new HexCoords(1, 5);
        } 
        else if (currentPos.Compare(new HexCoords(0, 7)) && direction == HexDirection.BottomLeft)
        {
            return new HexCoords(8, 7);
        }
        else if (currentPos.Compare(new HexCoords(8, 7)) && direction == HexDirection.TopRight)
        {
            return new HexCoords(0, 7);
        }
        else if (currentPos.Compare(new HexCoords(9, 0)) && direction == HexDirection.BottomRight)
        {
            return new HexCoords(0, 9);
        }
        else if (currentPos.Compare(new HexCoords(0, 9)) && direction == HexDirection.TopLeft)
        {
            return new HexCoords(9, 0);
        }
        else if (currentPos.Compare(new HexCoords(6, 0)) && direction == HexDirection.Left)
        {
            return new HexCoords(6, 9);
        }
        else if (currentPos.Compare(new HexCoords(6, 9)) && direction == HexDirection.Right)
        {
            return new HexCoords(6, 0);
        }
        else
        {
            travelAround = false;
            return new HexCoords(currentPos.x + HexDirections.directionMap[direction].x, currentPos.y + HexDirections.directionMap[direction].y);
        }
    }
}


