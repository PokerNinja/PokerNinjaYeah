
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

        if (id != -1)
        {
            this.gameObject.SetActive(true);
        }
        animator.SetInteger("emojiId",id);
        if (id == -1)
        {
            this.gameObject.SetActive(false);
        }
    }


    private void EnalbeAnimator(bool enable)
    {
        animator.enabled = enable;
    }
}
