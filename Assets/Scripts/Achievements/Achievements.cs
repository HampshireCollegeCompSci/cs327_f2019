using System.Collections.Generic;

public static class Achievements
{
    public static readonly Achievement cardStack = new(
        "Stacker",
        "Create the largest stack of cards possible.",
        "Achievement Card Stack",
        0,
        Achievement.AchieveType.Achieve);

    public static readonly Achievement reactorSize = new(
        "Overcautious",
        "Have no more than 1 card in each storage container at any time.",
        "Achievement Overcautious",
        1,
        Achievement.AchieveType.Failure);

    public static readonly Achievement reactorsAtLimit = new(
        "Redline",
        "Have all storage containers be at their limit at the same time.",
        "Achievement Reactors At Limit",
        2,
        Achievement.AchieveType.Achieve);

    public static readonly Achievement neverReactorHighAlert = new(
        "Playing Safe",
        "Never have a storage container be on high alert when the remaining moves are low.",
        "Achievement Never Reactor High Alert",
        3,
        Achievement.AchieveType.Failure);

    public static readonly Achievement allReactorsHighAlert = new(
        "Close Call",
        "Have all storage containers be on high alert at the same time when the remaining moves are low.",
        "Achievement All Reactors High Alert",
        4,
        Achievement.AchieveType.Achieve);

    public static readonly Achievement tripleCombo = new(
        "Oh Baby, a Triple!",
        "Achieve a match combo of 3 during a game.",
        "Achievement Triple Combo",
        5,
        Achievement.AchieveType.Achieve);

    public static readonly Achievement matchAll = new(
        "Clear Plate Club",
        "Match all cards.",
        "Achievement Match All",
        6,
        Achievement.AchieveType.Achieve);

    public static readonly Achievement noUndo = new(
        "Perfection",
        "Never use undo.",
        "Achievement No Undo",
        7,
        Achievement.AchieveType.Failure);

    public static readonly Achievement noDeckFlip = new(
        "No Flipping Way",
        "Never flip the deck.",
        "Achievement No Deck Flip",
        8,
        Achievement.AchieveType.Failure);

    public static readonly Achievement neverMoves = new(
        "On My Own Terms",
        "Never let the remaining moves get to zero.",
        "Achievement Never Moves",
        9,
        Achievement.AchieveType.Failure);

    public static readonly Achievement alwaysMoves = new(
        "Not On My Own Terms",
        "Always let the remaining moves get to zero.",
        "Achievement Always Moves",
        10,
        Achievement.AchieveType.Failure);

    public static readonly Achievement noHints = new(
        "Can't Take a Hint",
        "Win a game with hints and auto placement off.",
        "Achievement No Hints",
        11,
        Achievement.AchieveType.Failure);

    public static readonly Achievement superHard = new(
        "Super Hard",
        "Win a hard game with hints off, auto placement off, and never undoing.",
        "Achievement Super Hard",
        12,
        Achievement.AchieveType.Failure);

    public static readonly Achievement speedrun5 = new(
        "Gotta Go Fast",
        "Win a game in under five minutes.",
        "Achievement Speedrun 5",
        13,
        Achievement.AchieveType.Achieve);

    public static readonly Achievement speedrun2 = new(
        "Speedrunner",
        "Win a game in under two minutes.",
        "Achievement Speedrun 2",
        14,
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
        reactorsAtLimit,
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
