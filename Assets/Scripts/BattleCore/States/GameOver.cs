using Com.InfallibleCode.TurnBasedGame.Combat;
using System.Collections;
using UnityEngine;

public class GameOver : State
{
    private bool isPlayerWin;
    public GameOver(BattleSystem battleSystem , bool isPlayerWin) : base(battleSystem)
    {
        this.isPlayerWin = isPlayerWin;
    }

    public override IEnumerator Start()
    {
        yield return new WaitForSeconds(4f);
        battleSystem.Interface.ShowGameOverPanel(isPlayerWin);
        if (isPlayerWin)
        {
             battleSystem.WinParticleEffect();
            battleSystem.UpdateWinnerDB();
        }
        else
        {
            battleSystem.Interface.BgFadeInColor();
        }
    }

   
}
