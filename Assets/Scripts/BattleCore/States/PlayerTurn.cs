using Com.InfallibleCode.TurnBasedGame.Combat;
using System;
using System.Collections;
using UnityEngine;

public class PlayerTurn : State
{
    private bool yourLastTurn = false;
    private bool finalTurn = false;
    private int energyChargeCount;
    private int turnCounter;

    public PlayerTurn(BattleSystem battleSystem, int turnCounter) : base(battleSystem)
    {
        yourLastTurn = false;
        finalTurn = false;
        this.turnCounter = turnCounter;
        energyChargeCount = 2;
        if (turnCounter == 2)
        {
            yourLastTurn = true;
        }
        else if (turnCounter == 1)
        {
            finalTurn = true;
            energyChargeCount = 1;
        }

        /* if(turnCounter == 6 || turnCounter == 5)
         {
             cancaleTimer = true;
         }*/

    }

    public override IEnumerator Start()
    {
        Debug.Log("startingMyTurn;");
        battleSystem.endTurnInProcess = false;
        battleSystem.skillUsed = false;
        battleSystem.newPowerUpName = "x";
        /* if(Values.Instance.resetReplaceEvery == Values.GamePhase.Turn)
         {
             battleSystem.replacePuLeft = Values.Instance.replaceUseLimit;
         }*/
        if (Values.Instance.resetSkillEvery == Values.GamePhase.Turn)
        {
            battleSystem.skillUseLeft = Values.Instance.skillUseLimit;
        }
        battleSystem.DealPu(true, () =>
        {
        Debug.Log("AfterPlayerDeal;");
            Action tutorialAction = null;
            battleSystem.NewTimerStarter(true);
            battleSystem.ActivatePlayerButtons(true, true);
            battleSystem.Interface.WhosTurnAnimation(true, yourLastTurn, finalTurn, tutorialAction);
            battleSystem.ChargeEnergyCounter(energyChargeCount);
            battleSystem.Interface.SetTurnIndicator(true, true);

        }
        );
        yield break;
    }
}