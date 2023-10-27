using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipManager : MonoBehaviour
{
    public GameObject nose;
    public HexDirection direction;
    public HexCoords currentPos;

    internal IEnumerator Move()
    {
        foreach (var orderButton in GameManager.i.orderButtons)
        {
            orderButton.GetComponent<Button>().interactable = false;
            var nextMove = (Orders)int.Parse(orderButton.GetComponentsInChildren<TextMeshProUGUI>().FirstOrDefault(x => x.name == "OrderText").text);
            var targetPos = nose.transform.position;
            var targetRotPos = Quaternion.Euler(90, (transform.rotation.eulerAngles.y + 60) % 360, 0);
            var targetRotNeg = Quaternion.Euler(90, transform.rotation.eulerAngles.y < 60 ? 300 + transform.rotation.eulerAngles.y : transform.rotation.eulerAngles.y - 60, 0);
            var speedRot = 100f;
            var speedMove = 1f;
            switch (nextMove)
            {
                case Orders.Evade:
                    {
                        break;
                    }
                case Orders.Move:
                    {
                        var nextPosition = new HexCoords(currentPos.x + HexDirections.directionMap[direction].x, currentPos.y + HexDirections.directionMap[direction].y);
                        if (GameManager.i.mapNodes[nextPosition.x, nextPosition.y] != null)
                        {
                            while (targetPos != transform.position)
                            {
                                transform.position = Vector3.MoveTowards(transform.position, targetPos, speedMove * Time.deltaTime);
                                yield return null;
                            }
                            currentPos = nextPosition;
                        }
                        break;
                    }
                case Orders.TurnRight:
                    {
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
