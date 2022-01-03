using System.Threading.Tasks;

public class TutorialEnemy : State
{
    private int turnCounter;

    public TutorialEnemy(BattleSystem battleSystem, int turnCounter) : base(battleSystem)
    {
        this.battleSystem = battleSystem;
        this.turnCounter = turnCounter;
        /*var task = InitAction(turnCounter);
        task.Wait();*/
     //   InitFocusTutorial();
    }

    private async void InitFocusTutorial()
    {
        await Task.Delay(1200);
       /* battleSystem.FocusOnObjectWithText(true,false, Constants.TutorialObjectEnum.startGame.GetHashCode(), true);
        CheckIfReadyToCountinue();*/
    }
    private async void CheckIfReadyToCountinue()
    {
        if (battleSystem.continueTutorial)
        {
            InitAction(turnCounter);
        }
        else
        {
            await Task.Delay(200);
            CheckIfReadyToCountinue();
        }
    }
    private  void InitAction(int turnCounter)
    {
       
            switch (turnCounter)
            {
                case 6:
                    battleSystem.FakeEnemyEndTurn();
                    break;
                case 4:
                    battleSystem.FakeEnemyPuUse(0, Constants.EnemyCard2, "", true);
                    break;
                case 2:
                    battleSystem.FakeEnemyPuUse(0, Constants.EnemyCard2, "", true);
                    break;
            
        }
    }

    private enum EnemyActions
    {
        EndTurn = 0,
        SkillUse = 1,
        DrawPu = 2,
        PuUse1 = 3,
        PuUse2 = 4,
        SendEmoji = 5,
    }
}