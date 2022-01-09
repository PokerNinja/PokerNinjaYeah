using Managers;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    public static MainManager Instance;


    public MatchMakingManager matchmakingManager;
    public GameManager gameManager;

    public string currentLocalPlayerId; // You can use Firebase Auth to turn this into a userId. Just using the player name for a player id as an example for now!

    public void Awake()
    {
        if (Instance == null)
        {

            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            //Rest of your Awake code

        }
        else
        {
            Destroy(this);
        }
    }
  
    private void Start()
    {
        matchmakingManager = GetComponent<MatchMakingManager>();
        gameManager = GetComponent<GameManager>();
    }



    /*  public void SharedInstance ()
          {
          instance = this;
          matchmakingManager = GetComponent<MatchMakingManager>();
          gameManager = GetComponent<GameManager>();
      }*/
}
