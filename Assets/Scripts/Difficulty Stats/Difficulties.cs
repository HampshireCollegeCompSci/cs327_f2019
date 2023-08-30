using System.Collections.ObjectModel;
using System;

public static class Difficulties
{
    public static readonly Difficulty easy = new("EASY", 24, 24);
    public static readonly Difficulty medium = new("MEDIUM", 21, 21);
    public static readonly Difficulty hard = new("HARD", 18, 18);
    public static readonly Difficulty cheat = new("CHEAT", 100, 100);

    public static readonly ReadOnlyCollection<Difficulty> difficultyArray = Array.AsReadOnly(new Difficulty[4]
    {
        easy,
        medium,
        hard,
        cheat
    });
}
