using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValuesText : Singleton<Values>
{
   [SerializeField] public const string Start = "GET a better hand ranking than your opponents!";
    public const string Coins = "Players both have 2 coins. When you win, the other player lose 1 coin";
    public const string Energy = "Each player receives 2 energy per turn, each action requires using that energy";
    public const string NinjaHold = "Each turn you draw a ninja card. they have the power to manipulate the board in different ways. Hold your finger on the card to see what it does";
    public const string CardCost = "Each ninja card uses a cost of 1 energy. Dragon cards costs 2.";
    public const string Pair = "Looks like you have a pair of seven!";
    public const string FreezeSelf = "Freeze your 7 to secure it's place!!";
    public const string Choose = "Press the 7 in your hand to freeze it.";
    public const string Nice = "Great job!";
    public const string EndTurnEnergy = "You still got 1 energy remaining! you can make another action or save it and pass your turn. for now- press End Turn.";
    /// <su>
    ///////////////////Starts Enemy First Turn 
    /// </sy>
    public const string Vision = "You can hold your finger on your hand to see your current hand and rank";
    public const string RankInfo = "Here you can see all hand rankings";
    /// <su>
    /// ///////////////Starts Player Second Turn
    /// </sy>
    public const string SkillBtn = "With the cost of 2 energy, You can use flip to see the opponents card once per round.";
    public const string Flip = "Choose a card to flip";
    public const string OopsFreeze = "It’s Frozen! Choose the other card.";
    public const string EyeFlip = "This eye symbol indicated that the card is revealed.";
    public const string Draw = "With the cost of 1 energy you can draw another Ninja card.";
    public const string AutoEnd = "You got no energy left! The turn will pass automatically.";
    public const string NinjaDissolve = "When a turn is finished with 2 Ninja cards, the left one will be destroyd before you next draw.";
    /// <su>
    /// ///////////////Starts Enemy Second Turn
    /// </sy>
    public const string Emoji = "To communicate with the other player you can hold you finger on you ninja and drag it to a chosen Expression :)";
    /// <su>
    /// ////////////// Player Last Turn
    /// </sy>
    public const string Final1Energy = "In the final turn of the round, you only get  1 energy!";
    public const string WindHold = "\nHold to read the new Ninja card.";
    public const string WindInfo = "Use the Ninja card to swap\nthe enemy’s card\nwith 1 from the board.";
    public const string WindSelect = "Choose the '7' of your\nopponent's hand.And\nreplace it with a card other than '7' so your can have three of a kind.";
    public const string RankUpd = "You just made a better hand rank! good job!";
    public const string tuto24 = "End tutorial";
    public static string[] tutoInfo = { Start, Coins, Energy, NinjaHold, CardCost, FreezeSelf, Choose, Nice, EndTurnEnergy,
        Vision,RankInfo, SkillBtn, Flip, OopsFreeze, EyeFlip, Draw, AutoEnd, NinjaDissolve, Emoji,Final1Energy,WindHold,WindInfo
            ,WindSelect,RankUpd,tuto24,
    "","","","","",Pair};
}
