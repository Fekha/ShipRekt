using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AIManager 
{
    public static void TakeTurn()
    {
        GameManager.instance.UpdateOrderNumber(2, 2);
        GameManager.instance.EndTurn();
    }
}
