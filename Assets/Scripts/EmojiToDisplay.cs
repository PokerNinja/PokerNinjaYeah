
using Sirenix.OdinInspector;
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
       animator.Play("emoji_" + id, 0, 0f);
        //animator.SetInteger("emojiId",id);
    }

    private void EnalbeAnimator(bool enable)
    {
        animator.enabled = enable;
    }
}
