using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechWheel : MonoBehaviour
{
    
    public GameObject selection;
    public SpriteRenderer spriteRenderer;
    public Transform[] optionsTransforms;
    public bool isDragon;

    internal void SetSelection(int option)
    {
        selection.transform.position = optionsTransforms[option].position;
        selection.SetActive(true);
    }

    internal void EnableWheel(Vector2 position)
    {
        gameObject.SetActive(true);
        /*Vector3 newPosition = new Vector3(1.93f, 0f, 0);
        if (isSmall)
            newPosition = new Vector3(1.66f, 0f, 0);
        transform.position = position;
        transform.localPosition += newPosition;*/
        if (isDragon)
        {
            Vector3 newPosition = new Vector3(0f, 1.36f, 0);
            transform.position = position;
            transform.localPosition += newPosition;
        }
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

    public void OnSelected(int option)
    {
        SetSelection(option);
        if (isDragon)
        {
            if (option == 0 || option == 1)
                option -= 2;
            else
                option -= 1;
        }
        BattleSystem.Instance.OnWheelSelected(isDragon, option);
    }
}
