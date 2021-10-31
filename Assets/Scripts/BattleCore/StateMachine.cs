using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachine : MonoBehaviour
{
    protected State state;

    public void SetState(State newState)
    {
        state = newState;
        StartCoroutine(state.Start());
    }
}
