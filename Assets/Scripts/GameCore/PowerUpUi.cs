using Sirenix.OdinInspector;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PowerUpUi : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] public string puElement;
    [SerializeField] public int puIndex;
    [SerializeField] public string puName;
    [SerializeField] public string puDisplayName;
    [SerializeField] public bool isPlayer;
    [SerializeField] public bool isFaceDown;
    private bool flipInProgress;
    [SerializeField] public bool isMonster;
    [SerializeField] public bool isSkill;
    [SerializeField] public bool availableForReuse;
    [SerializeField] public bool aboutToDestroy;
    public bool freeze = false;
    public bool isClickable = false;
    public bool replaceMode = false;



    public int energyCost;
    [SerializeField] public SpriteRenderer spriteRenderer;
    [SerializeField]
    [Tooltip("How long must pointer be down on this object to trigger a long press")]
    private float holdTime = 0.3f;


    private bool held = false;
    public UnityEvent onClick = new UnityEvent();
    public UnityEvent onLongPress = new UnityEvent();


    #region Settings

    private void Start()
    {

        if (isSkill && isPlayer)
        {
            LoopShine(true);
        }
    }

    public void Activate(bool enable)
    {
        availableForReuse = !enable;
        gameObject.SetActive(enable);
    }

    public void Init(string puName, int puIndex, string puDisplayName, string puElement, bool isPlayer)
    {
        this.puName = puName;
        this.puIndex = puIndex;
        this.puDisplayName = puDisplayName;
        this.isFaceDown = !isPlayer;
        this.isPlayer = isPlayer;
        this.puElement = puElement;
        isMonster = GetIsMonster(puName);
        LoadSprite(false);
        energyCost = SetEnergyCost();
        replaceMode = false;
        aboutToDestroy = false;
    }

    private int SetEnergyCost()
    {
        if (isMonster)
        {
            return 2;
        }
        else
        {
            return 1;
        }
    }


    public void OnClick()
    {
        // TODO move this to battleSystem
        // BattleSystem battleSystem = GameObject.Find("BattleSystem").GetComponent<BattleSystem>();
        //Maybe Better One
        if (BattleSystem.Instance.infoShow)
        {
            BattleSystem.Instance.HideDialog();
        }
        else if (isPlayer && !freeze && isClickable)
        {
            isClickable = false;
            aboutToDestroy = true;
            BattleSystem.Instance.Interface.EnablePlayerButtons(false);
            BattleSystem.Instance.DisablePlayerPus();
            BattleSystem.Instance.Interface.EnableDarkScreen(isPlayer, true, null);
            if (puIndex != -1)
            {
                AnimatePuUse(() => BattleSystem.Instance.OnPowerUpPress(puName, puIndex, energyCost), null);
                //  AnimatePuUse(() => BattleSystem.Instance.OnPowerUpPress(puName, puIndex , energyCost), null, () => BattleSystem.Instance.ResetPuUi(true, puIndex));
            }
            else
            {
                BattleSystem.Instance.OnPowerUpPress(puName, puIndex, energyCost);
            }
        }
        else if (isPlayer && replaceMode)
        {
            BattleSystem.Instance.ReplacePu(true, puIndex);
        }
        else if (isPlayer || !isPlayer)
        {
            StartCoroutine(AnimationManager.Instance.Shake(spriteRenderer.material));
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.CantClick,false);
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
    /*    Debug.LogError("count " + eventData.clickCount);
        Debug.LogError("time " + eventData.clickTime);*/
        CancelInvoke("OnLongPress");
        if (isPlayer || isSkill)
        {
            if (!held)
            {
                onClick.Invoke();
            }
            else if (BattleSystem.Instance.infoShow)
            {
                BattleSystem.Instance.HideDialog();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isPlayer || isSkill)
        {
            held = false;
            Invoke("OnLongPress", holdTime);
        }
    }
    public void OND()
    {

        if (!BattleSystem.Instance.infoShow)
        {
            BattleSystem.Instance.ShowPuInfo(transform.position, puName, puDisplayName);
        }
    }


    private void OnLongPress()
    {
        held = true;
        onLongPress.Invoke();
    }


    private bool GetIsMonster(string puName)
    {
        return puName.Contains("m");
    }

    #endregion

    #region Visual
    public void LoopShine(bool enable)
    {
        if (isPlayer)
        {
            if (enable)
            {
                StartCoroutine(AnimationManager.Instance.ShinePU(true, 1, Values.Instance.puShineEvery, spriteRenderer.material, () => LoopShine(true)));
            }
            else
            {
                spriteRenderer.material.SetFloat("_ShineGlow", 0);
            }
        }
    }

    private void LoadSprite(bool revealCard)
    {
        string sprite = puName;

        if (!revealCard)
        {
            sprite = "pu_back";
        }
        spriteRenderer.sprite = Resources.Load("Sprites/PU/" + sprite, typeof(Sprite)) as Sprite;
    }

    public void OnObjectSpawn()
    {
        isClickable = false;
        spriteRenderer.material.SetFloat("_FadeAmount", -0.1f);
        spriteRenderer.material.SetFloat("_GradBlend", 0f);

        spriteRenderer.material.SetFloat("_ShineGlow", 0);

        EnableShake(false);
        SoundManager.Instance.RandomSoundEffect(0);
    }

    [Button]
    public void EnablePu(bool enable)
    {
        float value = 0.65f;
        if (enable)
        {
            value = 0;
        }
        if (!isSkill)
        {
            EnableShake(enable);
        }

        StartCoroutine(AnimationManager.Instance.UpdateValue(enable, "_GradBlend", Values.Instance.puChangeColorDisableDuration, spriteRenderer.material, value, () => isClickable = enable));
    }

    public void EnableShake(bool enable)
    {
        float targetSpeed;
        float x = Values.Instance.floatingShakeAnimationX;
        float y = Values.Instance.floatingShakeAnimationY;
        if (enable)
        {
            targetSpeed = Values.Instance.floatingShakeAnimationSpeed;
            if (puIndex == 1)
            {
                x += 0.02f;
                y -= 0.01f;
            }
            spriteRenderer.material.SetFloat("_ShakeUvX", x);
            spriteRenderer.material.SetFloat("_ShakeUvY", y);
        }
        else
        {
            targetSpeed = 0f;
        }
        spriteRenderer.material.SetFloat("_ShakeUvSpeed", targetSpeed);
    }
    public void AnimatePuUse(Action OnStart, Action OnEndRoutine3)
    {
        OnStart?.Invoke();
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.PuUse,false);
        EnableSelecetPositionZ(true);
        // transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -25f);
        if (isPlayer)
        {
            StartCoroutine(AnimationManager.Instance.PulseSize(true, transform, 1.15f, 0.4f, true,
            () => StartCoroutine(AnimationManager.Instance.ShinePU(false, 1, 1, spriteRenderer.material, OnEndRoutine3))));
        }
        else
        {
            CardReveal(true, () =>
         StartCoroutine(AnimationManager.Instance.PulseSize(false, transform, 1.15f, 0.4f, true,
            () => StartCoroutine(AnimationManager.Instance.ShinePU(false, 1, 1, spriteRenderer.material, OnEndRoutine3)))));
            //() => StartCoroutine(AnimationManager.FadeBurnPU(spriteRenderer.material, false, 1f,null, OnEndRoutine2, OnEndRoutine3))
        }
    }


    public void DissolvePu(float delayBefor, float duration, Action End, Action End2)
    {

        StartCoroutine(AnimationManager.Instance.FadeBurnPU(spriteRenderer.material, delayBefor, false, duration, null, End, End2));
    }

    public void CardReveal(bool reveal, Action onFinish)
    {
        if (reveal)
        {
            FlipCard(reveal, onFinish);
        }
        else
        {
            onFinish?.Invoke();
        }
    }
    public void FlipCard(bool reveal, Action onFinish)
    {
        isFaceDown = !reveal;
        flipInProgress = true;
        StartCoroutine(AnimationManager.Instance.FlipCard(gameObject.transform, Values.Instance.cardFlipDuration, () => flipInProgress = false, () =>
        {
            LoadSprite(reveal);
            if (isPlayer)
            {
                spriteRenderer.material.SetFloat("_GradBlend", 0.65f);
            }
        }, onFinish));
        StartCoroutine(AnimationManager.Instance.PulseSize(true, gameObject.transform.parent, 1.15f, Values.Instance.cardFlipDuration, false, null));

    }

    internal void SetReplaceMode(bool enable)
    {
        if (enable)
        {
            isClickable = false;
        }
        replaceMode = enable;
        //  isClickable = !enable;
        if (enable)
        {
            StartCoroutine(AnimationManager.Instance.UpdateValue(enable, "_GradBlend", Values.Instance.puChangeColorDisableDuration, spriteRenderer.material, 0f, null));
        }
    }

    public void EnableSelecetPositionZ(bool aboveDarkScreen)
    {
        float interval = 0f;
        if (aboveDarkScreen)
        {
            interval = -11f;
        }
        transform.parent.localPosition = new Vector3(transform.parent.localPosition.x, transform.parent.localPosition.y, interval - puIndex);
    }

    #endregion
}

