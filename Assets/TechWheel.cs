using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechWheel : MonoBehaviour
{
    
    public GameObject selection;
    public SpriteRenderer spriteRenderer;
    public Transform[] optionsTransforms;

    internal void SetSelection(int option)
    {
        selection.transform.position = optionsTransforms[option].position;
        selection.SetActive(true);
    }

    internal void EnableWheel(Vector2 position,bool isSmall)
    {
        gameObject.SetActive(true);
        /*Vector3 newPosition = new Vector3(1.93f, 0f, 0);
        if (isSmall)
            newPosition = new Vector3(1.66f, 0f, 0);
        transform.position = position;
        transform.localPosition += newPosition;*/
        StartCoroutine(AnimationManager.Instance.AlphaAnimation(spriteRenderer, true, Values.Instance.defaultFadeD, null));
    }

    internal void DisableWheel()
    {
        StartCoroutine(AnimationManager.Instance.AlphaAnimation(spriteRenderer, false, Values.Instance.defaultFadeD, ()=>
        {
            gameObject.SetActive(false);
            selection.SetActive(false);
        }));
    }
}
