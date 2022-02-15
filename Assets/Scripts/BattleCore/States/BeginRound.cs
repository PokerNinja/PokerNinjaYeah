using Com.InfallibleCode.TurnBasedGame.Combat;
using System;
using System.Collections;
using UnityEngine;

public class BeginRound : State
{
    private bool isPlayerTurn;
    private bool isFirstRound;
    private float delayForStart;
    private int startingTurnCounter = 6;
    public BeginRound(BattleSystem battleSystem, bool isPlayerTurn, bool isFirstRound) : base(battleSystem)
    {
        this.isPlayerTurn = isPlayerTurn;
        delayForStart = Values.Instance.delayBeforeStartNewRound;
        if (isFirstRound)
        {
            delayForStart = Values.Instance.delayBeforeStartFirstRound;
        }
        this.isFirstRound = isFirstRound;
    }

    public override IEnumerator Start()
    {
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.StartRound, true);
        battleSystem.InitDecks();
        yield return new WaitForSeconds(delayForStart);
        battleSystem.ResetRoundSettings(() => StartTurn());

    }

    private void StartTurn()
    {
        battleSystem.PlayMusic(true);
        //battleSystem.Interface.EnableBgColor(false);
        battleSystem.isPlayerActivatePu = false;
        battleSystem.readyToPlay = true;
        if (!isFirstRound)
        {
            battleSystem.Interface.MoveDealerBtn(false, !isPlayerTurn);
        }
        battleSystem.Interface.EnableVisionClick(true);
        /*  if (Values.Instance.resetReplaceEvery == Values.GamePhase.Round)
          {
              battleSystem.replacePuLeft = Values.Instance.replaceUseLimit;
          }*/
        if (Values.Instance.resetSkillEvery == Values.GamePhase.Round)
        {
            battleSystem.skillUseLeft = Values.Instance.skillUseLimit;
        }
        battleSystem.UpdateHandRank(false);
        if (isPlayerTurn)
        {
            battleSystem.SetState(new PlayerTurn(battleSystem, startingTurnCounter));
        }
        else
        {
            battleSystem.SetState(new EnemyTurn(battleSystem, startingTurnCounter)); // create new one
        }
    }
}
