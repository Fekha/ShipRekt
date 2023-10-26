using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShipManager : MonoBehaviour
{
    private List<int> moves = new List<int>() { 1, 2, 3, 4, 5, 6 };
    public GameObject nose;
    public int direction;
    public HexCords currentPos;

    internal IEnumerator Move()
    {
        foreach (var orderButton in GameManager.i.orderButtons)
        {
            orderButton.GetComponent<Button>().interactable = false;
            var nextMove = orderButton.GetComponentsInChildren<Text>().FirstOrDefault(x => x.name == "Text").text;
            var targetPos = nose.transform.position;
            var targetRotPos = Quaternion.Euler(90, (transform.rotation.eulerAngles.y + 60) % 360, 0);
            var targetRotNeg = Quaternion.Euler(90, transform.rotation.eulerAngles.y < 60 ? 300 + transform.rotation.eulerAngles.y : transform.rotation.eulerAngles.y - 60, 0);
            var speedRot = 100f;
            var speedMove = 1f;
            switch (nextMove)
            {
                case "1":
                    {
                        break;
                    }
                case "2":
                    {
                        var nextPosition = new HexCords(currentPos.x + GameManager.i.directions[direction].x, currentPos.y + GameManager.i.directions[direction].y);
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
                case "3":
                    {
                        direction = (direction + 1) % 6;
                        while (targetRotPos != transform.rotation)
                        {
                            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotPos, speedRot * Time.deltaTime);
                            yield return null;

                        }
                        break;
                    }
                case "4":
                    {
                        direction = direction == 0 ? 5 : direction - 1;
                        while (targetRotNeg != transform.rotation)
                        {
                            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotNeg, speedRot * Time.deltaTime);
                            yield return null;

                        }
                        break;
                    }
            }
            yield return new WaitForSeconds(.5f);
        }
        GameManager.i.orderButtons.ForEach(x => x.GetComponent<Button>().interactable = true);
    }

}
