using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyCost : MonoBehaviour
{
    public SpriteRenderer drawEnergy;
    public SpriteRenderer nc1Energy;
    public SpriteRenderer nc2Energy;


    private void EnableEnergy(bool enable, SpriteRenderer energy)
    {
        if (enable)
            energy.color = new Color(1, 1, 1, 0);
        StartCoroutine(AnimationManager.Instance.AlphaAnimation(energy, enable, 0.4f, null));
    }

    public void DisableNcEnergy( bool isNc1)
    {
        if (isNc1)
            EnableEnergy(false, nc1Energy);
        else
            EnableEnergy(false, nc2Energy);
    }
    
    public void EnableDrawEnergy(bool enable)
    {
        AvailableEnergy(enable, -1);
    }

    public void SetEnergy(int type, bool dragon)
    {
        switch (type)
        {
            case 0:
                SetSprite(nc1Energy, dragon);
                EnableEnergy(true, nc1Energy);
                break;
            case 1:
                SetSprite(nc2Energy, dragon);
                EnableEnergy(true, nc2Energy);
                break;
        }
    }

    public void AvailableEnergy(bool available, int index)
    {
        switch (index)
        {
            case -1:
                StartCoroutine(AnimationManager.Instance.DarkerAnimation(drawEnergy, !available, 0.5f, null));
                break;
            case 0:
                StartCoroutine(AnimationManager.Instance.DarkerAnimation(nc1Energy, !available, 0.5f, null));
                break;
            case 1:
                StartCoroutine(AnimationManager.Instance.DarkerAnimation(nc2Energy, !available, 0.5f, null));
                break;
        }
    }

    private void SetSprite(SpriteRenderer energy, bool dragon)
    {
        int amount = 1;
        if (dragon)
            amount = 2;
        energy.sprite = Resources.Load("Sprites/PU/" + amount + "energy", typeof(Sprite)) as Sprite;
    }
}
