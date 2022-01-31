using MyBox;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : Singleton<ObjectPooler>
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public Transform cardTransform;
    public Transform puTransform;
    public Dictionary<string, Queue<CardUi>> poolDictionary;

    public CardUi cardPrefab;
    public PowerUpUi puPrefab;
    private Queue<CardUi> cardPool = new Queue<CardUi>();
    private Queue<PowerUpUi> puPool = new Queue<PowerUpUi>();
    private int cardPoolSize = 13; // Check it
    private int puPoolSize = 6;

    void Start()
    {

        for (int i = 0; i < cardPoolSize; i++)
        {
            CardUi obj = Instantiate(cardPrefab);
            obj.Activate(false);
            obj.transform.SetParent( gameObject.transform);
            obj.name = "CardUi" + (i+1);
            cardPool.Enqueue(obj);
        }
        for (int i = 0; i < puPoolSize; i++)
        {
            PowerUpUi pu = Instantiate(puPrefab);
            pu.Activate(false);
            pu.transform.SetParent(gameObject.transform);
            puPool.Enqueue(pu);
        }

    }

    public CardUi SpwanCardFromPool(string tag)
    {
        if (cardPool.Count > 0)
        {
            CardUi card = cardPool.Dequeue();
            card.transform.position = cardTransform.position;
            card.transform.localScale = cardTransform.localScale ;
            card.OnObjectSpawn();
            card.Activate(true);

            /*IPooledObject pooledObject = card.GetComponent<IPooledObject>();
            if (pooledObject != null)
            {
                pooledObject.OnObjectSpawn();
            }*/
            return card;
        }
        else
        {
            CardUi newCard = Instantiate(cardPrefab);
            return newCard;
        }
    }
    public void ReturnCard(CardUi card)
    {
        cardPool.Enqueue(card);
    }
    public PowerUpUi SpwanPuFromPool()
    {
        if (cardPool.Count > 0)
        {
            PowerUpUi pu = puPool.Dequeue();
            pu.isClickable = false;
            pu.transform.position = puTransform.position;
            pu.transform.localScale = puTransform.localScale;
            pu.OnObjectSpawn();
            pu.Activate(true);

         
            return pu;
        }
        else
        {
            PowerUpUi newPu = Instantiate(puPrefab);
            return newPu;
        }

    }

    public void ReturnPu(PowerUpUi pu)
    {
        puPool.Enqueue(pu);
    }




}
