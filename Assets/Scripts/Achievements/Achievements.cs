using System.Collections.Generic;

public static class Achievements
{
    public static readonly Achievement cardStack = new(
        "Stacker",
        "Create the largest stack of cards possible.",
        "Achievement Card Stack",
        Achievement.AchieveType.Achieve);

    public static readonly Achievement reactorSize = new(
        "Overcautious",
        "Have no more than 1 card in each storage container at any time.",
        "Achievement Overcautious",
        Achievement.AchieveType.Failure);

    public static readonly Achievement reactorAtLimit = new(
        "Redline",
        "Have a storage container reach its limit.",
        "Achievement Reactor At Limit",
        Achievement.AchieveType.Achieve);

    public static readonly Achievement neverReactorHighAlert = new(
        "Playing Safe",
        "Never have a storage container be on high alert when the remaining moves are low.",
        "Achievement Never Reactor High Alert",
        Achievement.AchieveType.Failure);

    public static readonly Achievement allReactorsHighAlert = new(
        "Close Call",
        "Have all storage containers be on high alert at the same time when the remaining moves are low.",
        "Achievement All Reactors High Alert",
        Achievement.AchieveType.Achieve);

    public static readonly Achievement tripleCombo = new(
        "Oh Baby, a Triple!",
        "Achieve a match combo of 3 during a game.",
        "Achievement Triple Combo",
        Achievement.AchieveType.Achieve);

    public static readonly Achievement matchAll = new(
        "Clear Plate Club",
        "Match all cards.",
        "Achievement Match All",
        Achievement.AchieveType.Achieve);

    public static readonly Achievement noUndo = new(
        "Perfection",
        "Never use undo.",
        "Achievement No Undo",
        Achievement.AchieveType.Failure);

    public static readonly Achievement noDeckFlip = new(
        "No Flipping Way",
        "Never flip the deck.",
        "Achievement No Deck Flip",
        Achievement.AchieveType.Failure);

    public static readonly Achievement neverMoves = new(
        "On My Own Terms",
        "Never let the remaining moves get to zero.",
        "Achievement Never Moves",
        Achievement.AchieveType.Failure);

    public static readonly Achievement alwaysMoves = new(
        "Not On My Own Terms",
        "Always let the remaining moves get to zero.",
        "Achievement Always Moves",
        Achievement.AchieveType.Failure);

    public static readonly Achievement noHints = new(
        "Can't Take a Hint",
        "Win a game with hints and auto placement off.",
        "Achievement No Hints",
        Achievement.AchieveType.Failure);

    public static readonly Achievement superHard = new(
        "Super Hard",
        "Win a hard game with hints off, auto placement off, and never undoing.",
        "Achievement Super Hard",
        Achievement.AchieveType.Failure);

    public static readonly Achievement speedrun5 = new(
        "Gotta Go Fast",
        "Win a game in under five minutes.",
        "Achievement Speedrun 5",
        Achievement.AchieveType.Achieve);

    public static readonly Achievement speedrun2 = new(
        "Speedrunner",
        "Win a game in under two minutes.",
        "Achievement Speedrun 2",
        Achievement.AchieveType.Achieve);

    //public static readonly Achievement prettyColors = new(
    //    "Pretty Colors",
    //    "Win a game with Max's effect on!",
    //    "Achievement Pretty Colors",
    //    Achievement.AchieveType.Achieve);

    public static readonly List<Achievement> achievementList = new() {
        matchAll,
        cardStack,
        tripleCombo,
        reactorSize,
        reactorAtLimit,
        neverReactorHighAlert,
        allReactorsHighAlert,
        neverMoves,
        alwaysMoves,
        noDeckFlip,
        noUndo,
        noHints,
        superHard,
        speedrun5,
        speedrun2,
        //prettyColors
    };
}
