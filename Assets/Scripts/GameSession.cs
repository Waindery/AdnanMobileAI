public static class GameSession
{
    public const int MaxLevel = 2;
    public const int Level2CrystalBonus = 600;

    public static int CurrentLevel { get; set; } = 1;

    public static void ResetRun()
    {
        CurrentLevel = 1;
    }
}
