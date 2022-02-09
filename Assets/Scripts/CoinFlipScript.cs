
using Sirenix.OdinInspector;
using UnityEngine;

public class CoinFlipScript : MonoBehaviour
{
    public Animator animator;
    // public AnimationClip spin;
    public SpriteRenderer sp;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void RevealCoinFlip()
    {
        //  animator.anima = false;

    }


    public void FlipCoinAnimation()
    {
        // animator.SetTrigger("spin");
        animator.Play("coin_big");
        // animation.clip = spin;
    }
    
  
    public void FlipForTurn(bool playerTurn)
    {
        SetDirection(playerTurn);
        animator.Play("coin_small");
    }


    public void SetDirection(bool isPlayerFirst)
    {
        if (!isPlayerFirst)
        {
            sp.flipY = true;
        }
        else
        {
            sp.flipY = false;

        }
    }
}
