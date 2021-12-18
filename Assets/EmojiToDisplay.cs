
using UnityEngine;

public class EmojiToDisplay : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayEmoji(int id)
    {
        animator.SetInteger("emojiId",id);
    }


    private void EnalbeAnimator(bool enable)
    {
        animator.enabled = enable;
    }
}
