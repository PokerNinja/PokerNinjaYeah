using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyUi : MonoBehaviour
{

    public int index;
    public SpriteRenderer spriteRenderer;
    public bool _available;

    public bool Available
    {
        get => _available;
        set
        {
            _available = value;
            updateAvailableAnimation(_available);
        }
    }

    private void updateAvailableAnimation(bool available)
    {
        UpdateDistortionInterval(available);
        StartCoroutine(AnimationManager.Instance.FadeEnergy(
            ()=>StartCoroutine(AnimationManager.Instance.PulseSize(true,transform,1.5f,Values.Instance.pulseDuration,false, 
            null)),
            index * 0.2f, spriteRenderer.material, available,Values.Instance.enrgyDissolveDuration, null));
    }

    private void UpdateDistortionInterval(bool available)
    {
        float value = 0f;
        if (available)
        {
            value = 1f;
        }
        spriteRenderer.material.SetFloat("_DistortAmount", value);
    }


    void Start()
    {
        spriteRenderer.material.SetFloat("_DistortTexXSpeed", 5f + index * 0.1f);
    }



}
