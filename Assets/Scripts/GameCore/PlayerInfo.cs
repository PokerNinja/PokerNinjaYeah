using Serializables;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo
{

    public string id { get; set; }
    public int rank { get; set; }
    public int currentGameHealth { get; set; }
    public int startingGameHealth { get; set; }
    public int currentGameTimer { get; set; }
    public int totalCoins { get; set; }
    public List<PowerUpInfo> userPowerUps { get; set; }

    public PlayerInfo()
    {
       
    }

    public void SetStartingGameHealth(int startingHealth)
    {
        startingGameHealth = startingHealth;
        currentGameHealth = startingHealth;
    }
    public void TakeDamage(int damage)
    {
        currentGameHealth -= damage;
    }
}