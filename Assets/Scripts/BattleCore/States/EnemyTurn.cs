using Com.InfallibleCode.TurnBasedGame.Combat;
using System.Collections;
using UnityEngine;

public class EnemyTurn : State
{
    private bool isFinalTurn = false;
    private int turnCounter;
    public EnemyTurn(BattleSystem battleSystem, int turnCounter) : base(battleSystem)
    {

        isFinalTurn = false;
        this.turnCounter = turnCounter;
        if (turnCounter == 1)
        {
            isFinalTurn = true;
        }

    }
    public override IEnumerator Start()
    {
        battleSystem.turnInitInProgress = true;
        battleSystem.Interface.EnableBgColor(false);
        battleSystem.Interface.EnablePlayerButtons(false);
        battleSystem.DisablePlayerPus();
        battleSystem.DealPu(false, () =>
         {
             battleSystem.Interface.enemyNameText.text = "afterDeal";

             battleSystem.Interface.SetTurnIndicator(false, true);
             battleSystem.Interface.WhosTurnAnimation(false, false, isFinalTurn, () => battleSystem.turnInitInProgress = false);
             /*if (battleSystem.TUTORIAL_MODE)
             {
                 battleSystem.SetState(new TutorialEnemy(battleSystem, turnCounter));
             }
             else
             {
                 battleSystem.NewTimerStarter(false);
             }*/
                 battleSystem.NewTimerStarter(false);
         }
        );
        yield break;

    }
}