using Com.InfallibleCode.TurnBasedGame.Combat;
using System.Collections;
using UnityEngine;

public class GameOver : State
{
    private bool isPlayerWin;
    public GameOver(BattleSystem battleSystem , bool isPlayerWin) : base(battleSystem)
    {
        this.isPlayerWin = isPlayerWin;
        if (battleSystem.BOT_MODE)
        {
            if (isPlayerWin)
            {
            battleSystem.SavePrefsInt(Constants.Instance.PLAYER_WIN_BOT, battleSystem.LoadPrefsInt(Constants.Instance.PLAYER_WIN_BOT)+1);
            }
            else
            {
            battleSystem.SavePrefsInt(Constants.Instance.PLAYER_LOSE_BOT, battleSystem.LoadPrefsInt(Constants.Instance.PLAYER_LOSE_BOT)+1);
            }
        }
    }

    public override IEnumerator Start()
    {
        //battleSystem.disab
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
