using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnArrowUi : MonoBehaviour
{
    // Start is called before the first frame update
    public SpriteRenderer spriteRenderer;
    public bool changeToRed = false;
    public Animation animation;
    void Start()
    {

    }

    public void ChangeColor()
    {
        Color color = new Color(1f, 1f, 1f);
        if (changeToRed)
        {
            color = Values.Instance.brightRed;
        }
        spriteRenderer.material.SetColor("_GradBotLeftCol", color);
        // Try To Call Only Once
    }

    public void FlipImage(bool isPlayer)
    {
        Vector3 parentArrowScale = spriteRenderer.transform.parent.transform.localScale;
        float addition = 1; // enemyTurn
        if (isPlayer)
        {
            addition = -1;
        }
        Vector3 newScale = new Vector3(parentArrowScale.x, addition, parentArrowScale.z);
        spriteRenderer.transform.parent.transform.localScale = newScale;
    }
    public void ApplyIndicatorArrow(bool enable)
    {
        animation.Rewind();
        animation.Play();
        animation.Sample();
        animation.Stop();
        changeToRed = false;
        if (enable)
        {
            animation.Play();
        }
    }
}
