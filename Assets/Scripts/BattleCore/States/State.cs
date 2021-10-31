using Com.InfallibleCode.TurnBasedGame.Combat;
using System.Collections;
using UnityEngine;

public abstract class State 
{
    protected BattleSystem battleSystem;

    public State(BattleSystem battleSystem)
    {
        this.battleSystem = battleSystem;
    }

    public virtual IEnumerator Start()
    {
        yield break;
    }
   
}
