using Sirenix.OdinInspector;
using StandardPokerHandEvaluator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PuDeckUi : MonoBehaviour, IPointerDownHandler
{

    [SerializeField] public Transform puTransform;
    private Stack<string> deck;

    private static PuDeckUi deckUi;

    private Vector3 puVector;

    ObjectPooler objectPooler;

    private const string playerPu1Plcae = "PlayerPu1";
    private const string playerPu2Plcae = "PlayerPu2";
    private const string enemyPu1Place = "EnemyPu1";
    private const string enemyPu2Place = "EnemyPu2";


    public GameObject playerPuAParent;
    public GameObject playerPuBParent;
    public GameObject enemyPuAParent;
    public GameObject enemyPuBParent;

    private GameObject[] playerPuSlots, enemyPuSlots;

    public PowerUpUi[] playerPusUi;
    public PowerUpUi[] enemyPusUi;
    public Transform playerSkill;
    public Transform enemySkill;

    public Transform pEsTranform;
    public Transform eEsTranform;


    public Image dragonBtnV;
    public Image dragonBtnBg;
    public CanvasGroup dragonParent;
    public Animation dragonBtnAnimation;
    public EnergyCost energyCoster;

    // public PowerUpUi playerSkillUi;


    public static PuDeckUi Instance()
    {
        if (!deckUi)
        {
            deckUi = FindObjectOfType(typeof(PuDeckUi)) as PuDeckUi;
        }
        return deckUi;
    }


    private void Start()
    {
        SoundManager.Instance.RandomSoundEffect(SoundManager.SoundName.ShuffleDeck);

        objectPooler = ObjectPooler.Instance;
        AddPhysics2DRaycaster();
        puVector = new Vector3(1f, 1f, 1f);

        playerPusUi = new PowerUpUi[2];
        enemyPusUi = new PowerUpUi[2];
        playerPuSlots = new GameObject[] { playerPuAParent, playerPuBParent };
        enemyPuSlots = new GameObject[] { enemyPuAParent, enemyPuBParent };
    }



    internal void InitDeckFromServer(string[] deckFromDB)
    {
        Debug.LogError("newPUDECK");
        deck = new Stack<string>();
        foreach (string puString in deckFromDB)
        {
            deck.Push(puString);
        }
    }


    internal void DestroyPu(PowerUpUi pu, Action RemoveFromList, Action OnEnd)
    {
        if (pu.isPlayer)
            energyCoster.DisableNcEnergy(pu.puIndex == 0);
        pu.DissolvePu(0f, Values.Instance.puDissolveDuration, RemoveFromList, () => ResetPuUI(pu, OnEnd));
        //  StartCoroutine(AnimationManager.Instance.FadeBurnPU(pu.spriteRenderer.material, 0f , false, 4f, null, RemoveFromList, () => ResetPuUI(pu, OnEnd)));
    }



    public void ResetPuUI(PowerUpUi puToReset, Action OnEnd)
    {
        Debug.LogWarning("RESET NC: " + puToReset.puName);
        puToReset.Activate(false);
        puToReset.EnableSelecetPositionZ(false);
        puToReset.spriteRenderer.material.SetFloat("_ShineLocation", 0f);
        puToReset.spriteRenderer.material.SetFloat("_OutlineAlpha", 0f);
        puToReset.spriteRenderer.material.SetFloat("_WaveSpeed", 0f);
        puToReset.spriteRenderer.material.SetFloat("_DistortAmount", 0f);
        puToReset.spriteRenderer.color = new Color(1, 1, 1);
        puToReset.spriteRenderer.sortingOrder = 1;
        puToReset.name = "ReadyToUsePU";
        puToReset.puName = "X";
        puToReset.puDisplayName = "NN";
        puToReset.puIndex = -10;
        puToReset.outStand = false;
        // puToReset.freeze = false;
        puToReset.tag = "PowerUp";
        puToReset.transform.position = puTransform.position;
        puToReset.transform.localScale = puTransform.localScale;
        puToReset.transform.SetParent(objectPooler.transform);
        objectPooler.ReturnPu(puToReset);
        OnEnd?.Invoke();
    }





    private int ConvertCardPlaceToIndex(string puPlace)
    {
        if (puPlace.Equals(playerPu1Plcae) || puPlace.Equals(enemyPu1Place))
        {
            return 0;
        }
        if (puPlace.Equals(playerPu2Plcae) || puPlace.Equals(enemyPu2Place))
        {
            return 1;
        }

        return -1;
    }

    private void PushPuPosition(bool isPlayer, Action EndAction)
    {
        //{} V MAYBE DIFFERENT NO ARRAY
        if (isPlayer)
        {
            energyCoster.DisableNcEnergy(false);
            energyCoster.DisableNcEnergy(true);
        }
        PowerUpUi pu;
        Transform target;
        if (isPlayer)
        {
            pu = playerPusUi[0];
            pu.puIndex = 1;
            playerPusUi[1] = pu;
            playerPusUi[0] = null;
            target = playerPuBParent.transform;

        }
        else
        {
            pu = enemyPusUi[0];
            pu.puIndex = 1;
            enemyPusUi[1] = pu;
            enemyPusUi[0] = null;
            target = enemyPuBParent.transform;
        }
        pu.transform.SetParent(target);
        StartCoroutine(AnimationManager.Instance.SmoothMove(pu.transform, target.position, pu.transform.localScale, Values.Instance.puPushNewSlotMoveDuration, EndAction, null, null,
            () =>
            {
                if (isPlayer)
                    energyCoster.SetEnergy(1, pu.isMonster);
            }));

    }

    internal void ReplacePu(bool isPlayer, int puIndex, Action OnEnd)
    {
        DestroyPu(GetPu(isPlayer, puIndex), () => RemovePuFromList(isPlayer, puIndex), () => DealPuFromDeck(isPlayer, true, puIndex, OnEnd));
    }

    internal void EnablePusZ(bool isPlayer, bool aboveDarkScreen)
    {
        foreach (PowerUpUi pu in GetPuList(isPlayer))
        {
            if (pu != null)
            {
                pu.EnableSelecetPositionZ(aboveDarkScreen);
            }
        }
    }
    internal void EnablePusSlotZ(bool isPlayer, bool aboveDarkScreen)
    {

        float interval = 0f;
        if (aboveDarkScreen)
        {
            interval = -11f;
        }
        for (int i = 0; i < 2; i++)
        {
            Vector3 target = GetPuSlotList(isPlayer)[i].transform.localPosition;
            GetPuSlotList(isPlayer)[i].transform.localPosition = new Vector3(target.x, target.y, interval - i);
        }

    }

    /* internal void drawAndReplaceCard(string cardPlace, bool isFlip, Action disableDarkScreen)
     {
         disableDarkScreen?.Invoke();
            string newCard = deck.Pop();
        CardCreatorUi(newCard, isFlip, getParentByPlace(cardPlace), cardPlace, null);
     }*/


    public PowerUpUi[] GetPuList(bool isPlayer)
    {
        if (isPlayer)
        {
            return playerPusUi;
        }
        else
        {
            return enemyPusUi;
        }

    }
    public GameObject[] GetPuSlotList(bool isPlayer)
    {
        if (isPlayer)
        {
            return playerPuSlots;
        }
        else
        {
            return enemyPuSlots;
        }

    }

    public int GetPuListCount(bool isPlayer)
    {
        int count = 0;
        if (GetPuList(isPlayer)[0] != null)
        {
            count++;
        }
        if (GetPuList(isPlayer)[1] != null)
        {
            count++;
        }
        return count;
    }


    private void ResetUiLists()
    {
        playerPusUi[0] = null;
        playerPusUi[1] = null;
        enemyPusUi[0] = null;
        enemyPusUi[1] = null;
    }




    public void DealRoutine(bool isPlayer, Action OnEnd)
    {
        int dealTasks;
        if (isPlayer)
        {
            dealTasks = GetPuDealTask(playerPusUi);
        }
        else
        {
            dealTasks = GetPuDealTask(enemyPusUi);
        }
        UpdatePuPosition(isPlayer, dealTasks, () => DealPuFromDeck(isPlayer, false, 0, OnEnd));
    }

    private int GetPuDealTask(PowerUpUi[] puArray)
    {
        if (puArray[0] == null)
        {
            return 0;
        }
        else if (puArray[1] == null)
        {
            return 1;
        }
        else
        {
            return 2;
        }

    }

    private void UpdatePuPosition(bool isPlayer, int puCount, Action DealPu)
    {

        switch (puCount)
        {
            case 0:
                DealPu();
                break;
            case 1:
                PushPuPosition(isPlayer, DealPu);
                break;
            case 2:
                PowerUpUi pu;
                if (isPlayer)
                {
                    pu = playerPusUi[1];
                }
                else
                {
                    pu = enemyPusUi[1];
                }

                DestroyPu(pu, () => RemovePuFromList(isPlayer, 1), () => PushPuPosition(isPlayer, DealPu)); // MAYBE BUG NULL PUSHED
                break;
        }
    }



    public void DealPuFromDeck(bool isPlayer, bool aboveDarkScreen, int index, Action OnEnd)
    {
        GameObject parent;
        if (isPlayer)
        {
            parent = playerPuSlots[index];
        }
        else
        {
            parent = enemyPuSlots[index];
        }
        AnimateDrawer(true, () => PuCreatorUi(deck.Pop(), aboveDarkScreen, index, isPlayer, parent,
            () => AnimateDrawer(false, OnEnd)));
        ;
    }

    private void AnimateDrawer(bool open, Action action)
    {
        float targetX;
        if (open)
        {
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.OpenDrawer, false);
            // targetX = 4.39f;
            targetX = 5.35f;
        }
        else
        {
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.CloseDrawer, false);
            targetX = 3.2f;
        }

        StartCoroutine(AnimationManager.Instance.SmoothMoveDrawer(transform.parent,
            new Vector3(targetX, transform.parent.position.y, transform.parent.position.z), Values.Instance.drawerMoveDuration, null, action));
    }

    private void PuCreatorUi(string puName, bool aboveDarkScreen, int index, bool isPlayer, GameObject puParent, Action EndAction)
    {
        string puTag;
        if (isPlayer)
        {
            puTag = "PuP";
        }
        else
        {
            puTag = "PuE";

        }
        PowerUpUi puObject = objectPooler.SpwanPuFromPool();
        puObject.transform.SetParent(puParent.transform);
        puObject.puName = puName;
        puObject.name = puName;
        puObject.tag = puTag;
        puObject.isPlayer = isPlayer;
        puObject.transform.position = puTransform.position;
        puObject.transform.localScale = puTransform.localScale;
        puObject.transform.rotation = puTransform.rotation;
        Vector3 targetPosition = puParent.transform.position;
        if (aboveDarkScreen)
        {
            //targetPosition;
        }
        puObject.Init(puName, index, PowerUpStruct.Instance.GetPowerUpDisplayName(puName), puName[0].ToString(), isPlayer);
        StartCoroutine(AnimationManager.Instance.SmoothMove(puObject.transform, puParent.transform.position, puVector,
        Values.Instance.puDrawMoveDuration, null,
        () => puObject.CardReveal(isPlayer, () =>
         {
             if (isPlayer)
                 energyCoster.SetEnergy(index, puObject.isMonster);
         }), EndAction, () => puObject.LoopShine(puObject.isMonster)));
        AddCardToList(puTag, index, puObject);

    }

    private void AddCardToList(string cardTag, int index, PowerUpUi puObject)
    {
        switch (cardTag)
        {
            case "PuP":
                {
                    playerPusUi[index] = puObject;
                    break;
                }
            case "PuE":
                {
                    enemyPusUi[index] = puObject;
                    break;
                }
        }

    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject.name == "Deck")
        {
            // gameController.EndTurn();
        }
    }



    private void AddPhysics2DRaycaster()
    {
        Physics2DRaycaster physicsRaycaster = FindObjectOfType<Physics2DRaycaster>();
        if (physicsRaycaster == null)
        {
            Camera.main.gameObject.AddComponent<Physics2DRaycaster>();
        }
    }



    internal PowerUpUi GetPu(bool isPlayer, int puIndex)
    {
        if (isPlayer)
        {
            return playerPusUi[puIndex];
        }
        else
        {
            return enemyPusUi[puIndex];
        }
    }

    internal void RemovePuFromList(bool isPlayer, int puIndex)
    {
        if (isPlayer)
        {
            playerPusUi[puIndex] = null;
        }
        else
        {
            enemyPusUi[puIndex] = null;
        }
    }


    internal Vector2 GetPuPosition(bool isPlayer, int puIndex)
    {
        if (isPlayer)
        {
            if (puIndex == -1)
            {
                return playerSkill.position;
            }
            return playerPuSlots[puIndex].transform.position;
        }
        else
        {
            if (puIndex == -1)
            {
                return enemySkill.position;
            }
            return enemyPuSlots[puIndex].transform.position;
        }
    }
    /*  internal PowerUpUi GetPuFromList(bool isPlayer, int puIndex)
      {
          if (isPlayer)
          {
              return playerPusUi[puIndex];
          }
          else
          {
              return enemyPusUi[puIndex];
          }
      }*/

    internal void ResetOutstandPus()
    {
        foreach (PowerUpUi pu in GetPuList(true))
        {
            if (pu != null && pu.outStand)
            {
                pu.EnableValuesForSelect(false);
            }
        }
    }


    [Button]
    public void DissolveNcToEs(bool isPlayer, int index, Action FillEs, Action OnEnd)
    {
        Vector3 target = pEsTranform.position;
        if (!isPlayer)
            target = eEsTranform.position;
        GetPu(isPlayer, index).DissolveNcToEs(target, FillEs, OnEnd);
    }

    internal async void EnableDragonBtn(int index, string element)
    {
        //dragonParent.gameObject.SetActive(true);
        SetOutlineBtn(false);
        dragonBtnV.color = GetColorFromElement(element);
        dragonBtnBg.material.SetColor("_OutlineColor", GetColorFromElement(element));
        dragonBtnBg.material.SetFloat("_Glow", 0f);
        dragonParent.transform.position = new Vector3(GetPu(true, index).transform.position.x, GetPu(true, index).transform.position.y, 19f);
        //dragonParent.transform.position = GetPu(true, index).transform.position;
        dragonBtnAnimation.Play("dragon_btn");
        await Task.Delay(600);
        SetOutlineBtn(true);
        dragonParent.interactable = true;
        /*  StartCoroutine(AnimationManager.Instance.SimpleSmoothMove(dragonParent.transform, 0, newPosition, 1f, null,
              () => dragonParent.interactable = true));*/
    }

    private Color GetColorFromElement(string element)
    {
        switch (element)
        {
            case "f":
                return Values.Instance.fireVision;
            case "i":
                return Values.Instance.iceVision;
            case "w":
                return Values.Instance.windVision;
            case "t":
                return Values.Instance.techVision;
        }
        return Values.Instance.shadowVision;
    }

    internal void DisableDragonBtn(bool isClicked)
    {
        string animPath = "dragon_btn_out";
        if (dragonBtnV.color.a == 1)
        {
            dragonParent.interactable = false;
            /* StartCoroutine(AnimationManager.Instance.AlphaCanvasGruop(dragonParent, false, 0.5f, 
                 () => { 
                     dragonParent.gameObject.SetActive(false);
                     dragonParent.alpha = 1;
                 }));*/
            SetOutlineBtn(false);
            if (isClicked)
            {
                animPath = "dragon_btn_click";
                StartCoroutine(AnimationManager.Instance.UpdateValue(true, "_Glow", 0.6f, dragonBtnBg.material, 5f, null));
            }
            dragonBtnAnimation.Play(animPath);
        }
    }


    private void SetOutlineBtn(bool enable)
    {
        float amount = 0;
        if (enable)
            amount = 1;
        StartCoroutine(AnimationManager.Instance.UpdateValue(!enable, "_OutlineAlpha", Values.Instance.outlineFadeDuration, dragonBtnBg.material, amount, null));
      //  dragonBtnBg.material.SetFloat("_OutlineAlpha", amount);
    }

}
