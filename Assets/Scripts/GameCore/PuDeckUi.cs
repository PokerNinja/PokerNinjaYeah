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
        deck = new Stack<string>();
        foreach (string puString in deckFromDB)
        {
            deck.Push(puString);
        }
    }


    internal void DestroyPu(PowerUpUi pu, Action RemoveFromList, Action OnEnd)
    {
        pu.DissolvePu(0f, Values.Instance.puDissolveDuration, RemoveFromList, () => ResetPuUI(pu, OnEnd));
      //  StartCoroutine(AnimationManager.Instance.FadeBurnPU(pu.spriteRenderer.material, 0f , false, 4f, null, RemoveFromList, () => ResetPuUI(pu, OnEnd)));
    }



    public void ResetPuUI(PowerUpUi puToReset, Action OnEnd)
    {
        puToReset.Activate(false);
        puToReset.spriteRenderer.material.SetFloat("_ShineLocation", 0f);
        puToReset.spriteRenderer.material.SetFloat("_OutlineAlpha", 0f);
        puToReset.name = "ReadyToUsePU";
        puToReset.puName = "X";
        puToReset.puDisplayName = "NN";
        puToReset.puIndex = -10;
        puToReset.freeze = false;
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
        StartCoroutine(AnimationManager.Instance.SmoothMove(pu.transform, target.position, pu.transform.localScale, Values.Instance.puPushNewSlotMoveDuration, EndAction, null, null, null));

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

    private IEnumerable<CardUi> FindAllCardsObjects()
    {
        return FindObjectsOfType<CardUi>();
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

                DestroyPu(pu, () => RemovePuFromList(isPlayer, 1), () => PushPuPosition(isPlayer, DealPu));
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
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.OpenDrawer);
            targetX = 5.2f;
        }
        else
        {
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.CloseDrawer);
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
        Values.Instance.puDrawMoveDuration, null, () => puObject.CardReveal(isPlayer, null), EndAction, () => puObject.LoopShine(puObject.isMonster)));
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


    private GameObject GetParentByPlace(string cardPlace)
    {
        return GameObject.Find(cardPlace + "Slut");
    }


    public string CardPlaceToTag(string cardPlace)
    {
        if (cardPlace.Contains("Player"))
        {
            return "PuP";
        }
        else if (cardPlace.Contains("Enemy"))
        {
            return "PuE";

        }
        return "";
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

    internal bool IsCardFreeze(string cardPlace)
    {
        return GameObject.Find(cardPlace).GetComponent<CardUi>().freeze;
    }

    internal Vector2 GetPuPosition(bool isPlayer, int puIndex)
    {
        if (isPlayer)
        {
            return playerPuSlots[puIndex].transform.position;
        }
        else
        {
            return enemyPuSlots[puIndex].transform.position;
        }
    }
    internal PowerUpUi GetPuFromList(bool isPlayer, int puIndex)
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
}
