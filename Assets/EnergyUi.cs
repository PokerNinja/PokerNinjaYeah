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
        StartCoroutine(AnimationManager.Instance.FadeEnergy(index * 0.1f, spriteRenderer.material, available,0.8f, null));
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
