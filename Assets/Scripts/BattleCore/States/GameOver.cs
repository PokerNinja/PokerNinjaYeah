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
        SoundManager.Instance.StopMusic();
        yield return new WaitForSeconds(4f);
        battleSystem.Interface.ShowGameOverPanel(isPlayerWin);
        if (isPlayerWin)
        {
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.Win, true);
            battleSystem.WinParticleEffect();
            battleSystem.UpdateWinnerDB();
        }
        else
        {
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.Lose, true);
            battleSystem.Interface.BgFadeInColor();
        }
    }

   
}
