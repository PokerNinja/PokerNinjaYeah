using Com.InfallibleCode.TurnBasedGame.Combat;
using System.Collections;
using UnityEngine;

public abstract class StateTuto
{
    protected BattleSystemTuto battleSystem;

    public StateTuto(BattleSystemTuto battleSystem)
    {
        this.battleSystem = battleSystem;
    }

    public virtual IEnumerator Start()
    {
        yield break;
    }
   
}
