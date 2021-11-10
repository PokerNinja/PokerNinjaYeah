using Com.InfallibleCode.TurnBasedGame.Combat;
using System;
using System.Collections;
using UnityEngine;

public class BeginRound : State
{
    private bool isPlayerTurn;
    private float delayForStart;
    public BeginRound(BattleSystem battleSystem , bool isPlayerTurn, bool isFirstRound) : base(battleSystem)
    {
        Debug.LogWarning("Begin Round" + isPlayerTurn + isFirstRound);
        this.isPlayerTurn = isPlayerTurn;
        delayForStart = 2f;
        if (isFirstRound)
        {
        delayForStart = 1f;
        }
    }

    public override IEnumerator Start()
    {
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.StartRound);
        battleSystem.InitDecks();
        yield return new WaitForSeconds(delayForStart);
        battleSystem.ResetRoundSettings(()=> StartTurn());

    }

    private void StartTurn()
    {
        battleSystem.PlayMusic(true);
        battleSystem.Interface.EnableBgColor(false);
        battleSystem.isPlayerActivatePu = false;
        battleSystem.readyToPlay = true;
        battleSystem.UpdateHandRank();
        if (isPlayerTurn)
        {
            battleSystem.SetState(new PlayerTurn(battleSystem,6));
        }
        else
        {
            battleSystem.SetState(new EnemyTurn(battleSystem,6)); // create new one
        }
    }
}
