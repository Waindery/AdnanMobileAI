public static class GameSession
{
    public const int MaxLevel = 5;
    public const int NextLevelCrystalBonus = 700;
    public const int Level2CrystalBonus = NextLevelCrystalBonus;

    public static int CurrentLevel { get; set; } = 1;

    public static void ResetRun()
    {
        CurrentLevel = 1;
    }

    public static int GetLevelStartBonus(int level)
    {
        if (level <= 1)
            return 0;

        return NextLevelCrystalBonus + (level - 2) * 250;
    }
}
