using Com.InfallibleCode.TurnBasedGame.Combat;
using System.Collections;
using UnityEngine;

public class EnemyTurn : State
{
    private bool isFinalTurn = false;
    private int turnCounter;
    public EnemyTurn(BattleSystem battleSystem, int turnCounter) : base(battleSystem)
    {
       battleSystem.isPlayerBotModeTurn = false;
        isFinalTurn = false;
        this.turnCounter = turnCounter;
        if (turnCounter == 1 || turnCounter == 2)
        {
            isFinalTurn = true;
        }
        if (turnCounter == 4 || turnCounter == 2)
        {
            battleSystem.DealBoardCard();
        }
        
            battleSystem.Interface.EnableBgColor(isFinalTurn);

    }
    public override IEnumerator Start()
    {
        battleSystem.turnInitInProgress = true;
        // battleSystem.Interface.EnableBgColor(false);
       // battleSystem.StartCoroutine(battleSystem.ActivatePlayerButtons(false, false));
        //MAybe not necesery
        if (turnCounter != 6)
        {
            battleSystem.Interface.ApplyTurnVisual(false);
        }
        battleSystem.DealPu(false, () =>
         {
             // battleSystem.Interface.enemyNameText.text = "afterDeal";
             battleSystem.Interface.SetTurnIndicator(false, true);
             //battleSystem.Interface.WhosTurnAnimation(false,  isFinalTurn);
             battleSystem.turnInitInProgress = false; // Maybe with delay?
             battleSystem.NewTimerStarter(false);
             if (battleSystem.BOT_MODE)
             {
                 battleSystem.SetState(new BotEnemy(battleSystem, turnCounter));
             }

         }
        );
        yield break;

    }
}