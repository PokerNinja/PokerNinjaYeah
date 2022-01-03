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
        Debug.LogError("Notice turn counter " + turnCounter);
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

            Action tutorialAction = null;
            if (battleSystem.TUTORIAL_MODE)
            {
               // battleSystem.ActivateButtonForTutorial(turnCounter);
                //tutorialAction = () => battleSystem.FocusOnObjectWithText(true, false, Constants.TutorialObjectEnum.coins.GetHashCode(), true);
                if (turnCounter == 5)
                {
                    tutorialAction = () => battleSystem.FocusOnObjectWithText( true, 0, Constants.TutorialObjectEnum.startGame.GetHashCode(), true);
                }
            }
            else
            {
                battleSystem.NewTimerStarter(true);
                battleSystem.ActivatePlayerButtons(true, true);
            }
            battleSystem.Interface.WhosTurnAnimation(true, yourLastTurn, finalTurn, tutorialAction);
            if (battleSystem.TUTORIAL_MODE && turnCounter == 5)
            { }
            else
            {
                battleSystem.ChargeEnergyCounter(energyChargeCount);
            }
            battleSystem.Interface.SetTurnIndicator(true, true);

        }
        );
        yield break;
    }
}