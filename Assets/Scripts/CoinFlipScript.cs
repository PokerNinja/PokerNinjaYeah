
using UnityEngine;

public class CoinFlipScript : MonoBehaviour
{
   public  Animator animator;
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
        animator.SetTrigger("spin");
       // animator.Play("spin_coin");
       // animation.clip = spin;
    }

    public void SetDirection(bool isPlayerFirst)
    {
        if (isPlayerFirst)
        {
            sp.flipY = true;
        }
    }
}
