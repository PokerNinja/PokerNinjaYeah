using Firebase.Analytics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class TestScript : MonoBehaviour
{
    [Required]
    public KobiTestScript KobiTestScript;
    public int magicNumber = 4;
   

    public void dostuffWhenAviadsIntChanged(int original, int newValue)
    {
        Debug.LogWarning($"not sure what we do but here is the info: \n" +
            $"aviad's int was originally {original} but was somehow changed to {newValue} \n " +
            $"and by the way if you multiply it by the magic number it comes out {newValue * magicNumber} ");
    }

    

    void Start()
    {
        KobiTestScript.onIntChangedValue += dostuffWhenAviadsIntChanged;
        
    }

    
}
