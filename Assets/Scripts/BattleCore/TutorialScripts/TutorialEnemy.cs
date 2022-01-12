using System.Threading.Tasks;

public class TutorialEnemy : StateTuto
{
    private int turnCounter;

    public TutorialEnemy(BattleSystemTuto battleSystem, int turnCounter) : base(battleSystem)
    {
        this.battleSystem = battleSystem;
        this.turnCounter = turnCounter;
        /*var task = InitAction(turnCounter);
        task.Wait();*/
        InitFocusTutorial();
    }

    private async void InitFocusTutorial()
    {
        await Task.Delay(350);
        /* battleSystem.FocusOnObjectWithText(true,false, Constants.TutorialObjectEnum.startGame.GetHashCode(), true);
         CheckIfReadyToCountinue();*/
        switch (turnCounter)
        {
            case 4:
                battleSystem.tutorialUi.MoveCardsMaskPlayer(true, null);
                battleSystem.FocusOnObjectWithText(true, 0, Constants.TutorialObjectEnum.vision.GetHashCode(), false);
                break;
        }
        CheckIfReadyToCountinue();
    }
    private async void CheckIfReadyToCountinue()
    {
        if (battleSystem.continueTutorial)
        {
            InitAction(turnCounter);
        }
        else
        {
            await Task.Delay(400);
            CheckIfReadyToCountinue();
        }
    }
    private async void InitAction(int turnCounter)
    {
        await Task.Delay(800);
        switch (turnCounter)
        {
            case 6:
                battleSystem.FakeEnemyEndTurn();
                break;
            case 4:
                battleSystem.continueTutorial = false;
                battleSystem.FakeEnemyPuUse(0, Constants.EnemyCard2, "", true);
                await Task.Delay(800);
                battleSystem.FakeEnemyEmoji(1);
                break;
            case 2:
                //battleSystem.continueTutorial = false;
                battleSystem.FakeEnemyPuUse(0, Constants.BFlop2, Constants.BFlop3, false);
                await Task.Delay(800);
                battleSystem.FakeEnemyEmoji(1);
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