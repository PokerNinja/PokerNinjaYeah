using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public class KobiTestScript : MonoBehaviour
{

    private int aviadsInt;

    [ShowInInspector]
    public int AviadsInt { 
        get => aviadsInt; 
        set  {
            
            if (value != aviadsInt)
            {
                onIntChanged?.Invoke();
                onIntChangedValue?.Invoke(aviadsInt, value);
            }

            aviadsInt = value; 
        }
         
    }

    public event Action onIntChanged;

    public event Action<int,int> onIntChangedValue;

    public event Func<int, int> FUncInt;

    public int someIntFunction( int param)
    {

        return 5;
    }

    [ShowInInspector]
    public int AviadX2 => AviadsInt * 2;

    [Button]
    public void debugStuuf()
    {
        Debug.LogWarning("asdasdasd");
    }

    [Button]
    public void IncrementAviad(int delta)
    {
        AviadsInt += delta;
    }

    // Start is called before the first frame update
    void Start()
    {
        onIntChanged += () =>  { Debug.LogWarning("int was changed!"); };
        onIntChangedValue += (int i, int ii) => { Debug.LogWarning($"old int was {i} and new int is {ii} "); };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
