using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSlot : MonoBehaviour
{
    public bool smokeEnable = false;
    public bool smokeActivateByPlayer = false;
    public string childrenName;
    public SpriteRenderer ncActionRenderer;

    public void EnableNcAction(bool enable, Constants.NcAction ncAction)
    {
        if (ncActionRenderer != null)
        {
            if (enable)
            {
                ncActionRenderer.sprite = LoadSprite(ncAction);
            }
            ncActionRenderer.gameObject.SetActive(enable);
        }
    }

    private Sprite LoadSprite(Constants.NcAction ncAction)
    {
        string spritePath = "";
        if (ncAction == Constants.NcAction.Defrost)
        {
            spritePath = "defrost";
        }
        else if (ncAction == Constants.NcAction.Shatter)
        {
            spritePath = "shatter";
        }
        return Resources.Load("Sprites/PU/" + spritePath, typeof(Sprite)) as Sprite;
    }
}
