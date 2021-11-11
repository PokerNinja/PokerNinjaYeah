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
        this.isPlayerTurn = isPlayerTurn;
        delayForStart = Values.Instance.delayBeforeStartNewRound;
        if (isFirstRound)
        {
        delayForStart = Values.Instance.delayBeforeStartFirstRound;
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
        battleSystem.UpdateHandRank(false);
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
