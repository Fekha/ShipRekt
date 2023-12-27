using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AIManager 
{
    public static IEnumerator TakeTurn()
    {
        
        GameManager.instance.ChangeOrder(Random.Range(0, 6), Random.Range(0, 6));
        yield return new WaitForSeconds(1f);
        GameManager.instance.ChangeOrder(Random.Range(0, 6), Random.Range(0, 6));
        yield return new WaitForSeconds(1f);
        GameManager.instance.EndTurn();
        yield return new WaitForSeconds(.1f);

    }
}
