using Com.InfallibleCode.TurnBasedGame.Combat;
using System.Collections;
using UnityEngine;

public class EnemyTurn : State
{
    private bool isFinalTurn = false;
    public EnemyTurn(BattleSystem battleSystem, int turnCounter) : base(battleSystem)
    {
        isFinalTurn = false;
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
        battleSystem.DealPu(false, () =>
         {
             battleSystem.Interface.SetTurnIndicator(false, true);
             battleSystem.NewTimerStarter(false);
             battleSystem.Interface.WhosTurnAnimation(false, false, isFinalTurn, () => battleSystem.turnInitInProgress = false);
         }
        );
        yield break;

    }
}