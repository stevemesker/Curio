using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moneyKiller : MonoBehaviour
{
    //testing lowering player money
    public int moneyDecrease;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player.player.DecreaseMoney(moneyDecrease);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player.player.IncreaseMoney(moneyDecrease);
        }
    }
}
