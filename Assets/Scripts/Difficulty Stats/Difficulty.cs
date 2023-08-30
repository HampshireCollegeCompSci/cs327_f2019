public class Difficulty
{
    public Difficulty(string name, int reactorLimit, int moveLimit)
    {
        Name = name;
        ReactorLimit = reactorLimit;
        MoveLimit = moveLimit;
        Stats = new Stats(name);
    }

    public readonly string Name;

    public readonly int ReactorLimit;

    public readonly int MoveLimit;

    public readonly Stats Stats;
}