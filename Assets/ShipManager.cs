using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipManager : MonoBehaviour
{
    public int directionNum;
    public int x;
    public int y;
    internal HexDirection direction;
    internal HexCoords currentPos;
    private bool travelAround = true;

    void Start()
    {
        currentPos = new HexCoords(5, 5);
        direction = HexDirection.TopLeft;
    }
    internal IEnumerator Move()
    {
        foreach (var orderButton in GameManager.i.orderButtons)
        {
            orderButton.GetComponent<Button>().interactable = false;
            var nextOrder = (Orders)int.Parse(orderButton.GetComponentsInChildren<TextMeshProUGUI>().FirstOrDefault(x => x.name == "OrderText").text);
            var speedRot = 100f;
            switch (nextOrder)
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
                        if (nextPosition.x >= 0 && nextPosition.x < 11 && nextPosition.y >= 0 && nextPosition.y < 11)
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
                    break;
                }
                case Orders.ShootLeft:
                {
                    break;
                }
            }
            yield return new WaitForSeconds(.5f);
        }
        GameManager.i.orderButtons.ForEach(x => x.GetComponent<Button>().interactable = true);
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
        else
        {
            travelAround = false;
            return new HexCoords(currentPos.x + HexDirections.directionMap[direction].x, currentPos.y + HexDirections.directionMap[direction].y);
        }
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
