using Com.InfallibleCode.TurnBasedGame.Combat;
using System.Collections;
using UnityEngine;

public class PlayerTurn : State
{
    private bool yourLastTurn = false;
    private bool finalTurn = false;
    private bool cancaleTimer;

    public PlayerTurn(BattleSystem battleSystem, int turnCounter) : base(battleSystem)
    {
        cancaleTimer = false;

        yourLastTurn = false;
        finalTurn = false;
        if (turnCounter == 2)
        {
            yourLastTurn = true;
        }
        else if (turnCounter == 1)
        {
            finalTurn = true;
        }
       /* if(turnCounter == 6 || turnCounter == 5)
        {
            cancaleTimer = true;
        }*/
        
    }

    public override IEnumerator Start()
    {
        battleSystem.skillUsed = false;
        battleSystem.Interface.SetTurnIndicator(true, true);
        if (!cancaleTimer)
        {
        battleSystem.NewTimerStarter(true);
        }
        battleSystem.DealPu(true, () =>
        {
            battleSystem.ActivatePlayerButtons();
            battleSystem.Interface.WhosTurnAnimation(true, yourLastTurn, finalTurn);
            battleSystem.ChargeEnergyCounter(2);
        }
        );
        yield break;
    }
}