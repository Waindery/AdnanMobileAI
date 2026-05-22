using UnityEngine;

/// <summary>
/// Capsule primitives are 2 units tall (pivot at center). Floor surface is at y = 0.
/// </summary>
public static class BattleGround
{
    public const float FloorY = 0f;
    public const float UnitSpawnY = 1f;

    public static Vector3 SpawnPosition(float x, float z)
    {
        return new Vector3(x, UnitSpawnY, z);
    }

    public static Vector3 OnGround(Vector3 position)
    {
        return new Vector3(position.x, UnitSpawnY, position.z);
    }
}
