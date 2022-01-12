using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValuesText : Singleton<Values>
{
   [SerializeField] public const string Start = "GET a better hand ranking than your opponents!";
    public const string Coins = "Each player has 2 coins. When you win, the other player lose 1 coin";
    public const string Energy = "Your energy. bla bla bli blo bla bla bla bolo";
    public const string NinjaHold = "Every turn you get 1 Ninja card. Hold down your finger on the Ninja Card";
    public const string CardCost = "This is the energy cost of this Ninja Card.\nYou can activate your ninja card on your turn, depends on your energy";
    public const string Pair = "You got pair of seven!";
    public const string FreezeSelf = "Freeze your '7' using your Ninja card.";
    public const string Choose = "Now - Click on your card '7'";
    public const string Nice = "Great job!";
    public const string EndTurnEnergy = "Now you have left 1 energy left to use. You can use it but maybe better save it.\nEnd your turn";
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
    public const string OopsFreeze = "It’s Frozen. Choose the other card";
    public const string EyeFlip = "A card with this symbol means its revealed";
    public const string Draw = "With the cost of 1 energy you can draw another Ninja card";
    public const string AutoEnd = "Got no energy left! The turn will pass automatically";
    public const string NinjaDissolve = "When you finish your turn with 2 Ninja cards, the one from the left will be destroyd before you next turn.";
    /// <su>
    /// ///////////////Starts Enemy Second Turn
    /// </sy>
    public const string Emoji = "Hold your finger on your ninja and drag it to select an Emoji";
    /// <su>
    /// ////////////// Player Last Turn
    /// </sy>
    public const string Final1Energy = "In the final turn of the round, you get only 1 energy";
    public const string WindHold = "\nHold to read     ";
    public const string WindInfo = "Use the Ninja card to swap\nthe enemy’s card\nwith 1 from the board.";
    public const string WindSelect = "Choose the '7' from the\nopponent's hand.And\nfrom the board - other than '7'.";
    public const string RankUpd = "You just UP’d your rank!";
    public const string tuto24 = "GOOD LUCK!";
    public static string[] tutoInfo = { Start, Coins, Energy, NinjaHold, CardCost, FreezeSelf, Choose, Nice, EndTurnEnergy,
        Vision,RankInfo, SkillBtn, Flip, OopsFreeze, EyeFlip, Draw, AutoEnd, NinjaDissolve, Emoji,Final1Energy,WindHold,WindInfo
            ,WindSelect,RankUpd,tuto24,
    "","","","","",Pair};
}
