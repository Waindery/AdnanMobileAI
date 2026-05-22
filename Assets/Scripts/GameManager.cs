using System.Collections;

using System.Collections.Generic;

using DG.Tweening;

using UnityEngine;



public class GameManager : MonoBehaviour

{

    public static GameManager Instance { get; private set; }



    [SerializeField] private Unit echoPrefab;

    [SerializeField] private Unit enemyPrefab;

    [SerializeField] private Transform echoSpawnPoint;

    [SerializeField] private Transform[] enemySpawnPoints;

    [SerializeField] private float waveDelay = 2.5f;

    [SerializeField] private int maxAllies = 4;

    [SerializeField] private int wavesPerLevel = 3;

    [SerializeField] private float echoKillHealPercent = 0.15f;

    [SerializeField] private int crystalsPerKill = 30;



    private readonly List<Unit> livingEnemies = new List<Unit>();

    private readonly List<Unit> allies = new List<Unit>();

    private Unit echo;

    private Unit allyPrefab;

    private int currentLevel;

    private int currentWave;

    private int allySpawnIndex;

    private bool battleEnded;



    public Unit Echo => echo != null && echo.IsAlive ? echo : null;

    public int CurrentLevel => currentLevel;

    public IReadOnlyList<Unit> LivingEnemies => livingEnemies;

    public int AllyCount => allies.Count;



    private void Awake()

    {

        if (Instance != null && Instance != this)

        {

            Destroy(gameObject);

            return;

        }



        Instance = this;

        currentLevel = GameSession.CurrentLevel;

    }



    private void OnDestroy()

    {

        if (Instance == this)

            Instance = null;

    }



    private void Start()

    {

        Time.timeScale = 1f;

        BattleBackground.Create();

        EnsurePrefabs();

        SpawnEcho();

        StartWave(1);

        UIManager.Instance?.UpdateLevelDisplay(currentLevel);

    }



    private void EnsurePrefabs()

    {

        if (echoPrefab == null)

            echoPrefab = CreateRuntimeUnitPrefab("Echo", UnitTeam.Player, UnitRole.Echo, new Color(0.2f, 0.45f, 1f));



        if (enemyPrefab == null)

            enemyPrefab = CreateRuntimeUnitPrefab("Enemy", UnitTeam.Enemy, UnitRole.Enemy, new Color(0.9f, 0.2f, 0.2f));



        if (allyPrefab == null)

            allyPrefab = CreateRuntimeUnitPrefab("Ally", UnitTeam.Player, UnitRole.Ally, new Color(0.2f, 0.85f, 0.35f));

    }



    private static Unit CreateRuntimeUnitPrefab(string unitName, UnitTeam team, UnitRole role, Color color)

    {

        GameObject root = GameObject.CreatePrimitive(PrimitiveType.Capsule);

        root.name = unitName;

        Object.Destroy(root.GetComponent<Collider>());



        Renderer renderer = root.GetComponent<Renderer>();

        if (renderer != null)

            renderer.material.color = color;



        Unit unit = root.AddComponent<Unit>();

        float hp = role == UnitRole.Echo ? 100f : role == UnitRole.Ally ? 60f : 40f;

        float dmg = role == UnitRole.Echo ? 20f : role == UnitRole.Ally ? 15f : 10f;

        float cd = role == UnitRole.Echo ? 1.5f : role == UnitRole.Ally ? 2f : 2f;

        unit.Configure(team, hp, dmg, cd, role);

        root.AddComponent<UnitFSM>();

        root.SetActive(false);

        return unit;

    }



    private void SpawnEcho()

    {

        Vector3 position = GetEchoSpawnPosition();

        echo = Instantiate(echoPrefab, position, Quaternion.identity);

        echo.gameObject.SetActive(true);

        echo.Configure(UnitTeam.Player, 100f, 20f, 1.5f, UnitRole.Echo);

        echo.OnDeath += HandleEchoDeath;



        if (UIManager.Instance != null)

        {

            UIManager.Instance.BindEcho(echo);

            UIManager.Instance.UpdateHPBar(echo.HPRatio);

            UpdateWaveHud();

        }

    }



    private Vector3 GetEchoSpawnPosition()

    {

        if (echoSpawnPoint != null)

            return BattleGround.OnGround(echoSpawnPoint.position);



        return BattleGround.SpawnPosition(-2.5f, 0f);

    }



    private void StartWave(int wave)

    {

        currentWave = wave;

        int enemyCount = GetEnemyCountForWave(currentLevel, wave);



        UpdateWaveHud();



        GetEnemyStatsForLevel(currentLevel, out float enemyHp, out float enemyDamage, out float enemyCooldown);



        for (int i = 0; i < enemyCount; i++)

        {

            Vector3 spawnPosition = GetEnemySpawnPosition(i);

            Unit enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            enemy.gameObject.SetActive(true);

            enemy.Configure(UnitTeam.Enemy, enemyHp, enemyDamage, enemyCooldown);

            enemy.OnKilled += HandleEnemyKilled;

            livingEnemies.Add(enemy);

        }

    }



    private static int GetEnemyCountForWave(int level, int wave)

    {

        int count = 3 + wave;

        if (level >= 2)

            count += 1;



        return count;

    }



    private static void GetEnemyStatsForLevel(int level, out float hp, out float damage, out float cooldown)

    {

        hp = 40f;

        damage = 10f;

        cooldown = 2f;



        if (level >= 2)

        {

            hp = 50f;

            damage = 12f;

            cooldown = 1.85f;

        }

    }



    private Vector3 GetEnemySpawnPosition(int index)

    {

        if (enemySpawnPoints != null && index < enemySpawnPoints.Length && enemySpawnPoints[index] != null)

            return BattleGround.OnGround(enemySpawnPoints[index].position);



        int row = index / 3;

        int column = index % 3;

        float x = 3f + row * 0.9f;

        float z = -2f + column * 2f;

        return BattleGround.SpawnPosition(x, z);

    }



    private void UpdateWaveHud()

    {

        UIManager.Instance?.UpdateWaveText(currentLevel, currentWave, wavesPerLevel);

    }



    public Unit GetNearestLivingEnemy(Vector3 fromPosition)

    {

        Unit nearest = null;

        float nearestDistance = float.MaxValue;



        for (int i = livingEnemies.Count - 1; i >= 0; i--)

        {

            Unit enemy = livingEnemies[i];

            if (enemy == null || !enemy.IsAlive)

            {

                livingEnemies.RemoveAt(i);

                continue;

            }



            float distance = Vector3.Distance(fromPosition, enemy.transform.position);

            if (distance < nearestDistance)

            {

                nearestDistance = distance;

                nearest = enemy;

            }

        }



        return nearest;

    }



    public Unit GetNearestLivingPlayerUnit(Vector3 fromPosition)

    {

        Unit nearest = null;

        float nearestDistance = float.MaxValue;



        if (echo != null && echo.IsAlive)

        {

            float distance = Vector3.Distance(fromPosition, echo.transform.position);

            nearestDistance = distance;

            nearest = echo;

        }



        for (int i = allies.Count - 1; i >= 0; i--)

        {

            Unit ally = allies[i];

            if (ally == null || !ally.IsAlive)

            {

                allies.RemoveAt(i);

                continue;

            }



            float distance = Vector3.Distance(fromPosition, ally.transform.position);

            if (distance < nearestDistance)

            {

                nearestDistance = distance;

                nearest = ally;

            }

        }



        return nearest;

    }



    public string ApplyGachaReward(GachaRewardType reward)

    {

        switch (reward)

        {

            case GachaRewardType.Nothing:

                return "You receive nothing.";



            case GachaRewardType.EchoUpgrade:

                UpgradeEcho(15f, 3f);

                RefreshEchoHud();

                return "Echo Upgrade! +15 HP, +3 ATK";



            case GachaRewardType.EchoUpgradeRare:

                UpgradeEcho(35f, 8f);

                RefreshEchoHud();

                return "RARE! Major Echo Boost! +35 HP, +8 ATK";



            case GachaRewardType.NewAlly:

                if (allies.Count >= maxAllies)

                {

                    UpgradeEcho(20f, 5f);

                    RefreshEchoHud();

                    return "Team full! Echo powered up instead.";

                }



                SpawnAlly();

                return $"New Ally joined! ({allies.Count}/{maxAllies})";



            default:

                return "Unknown reward.";

        }

    }



    public void UpgradeEcho(float bonusHp, float bonusDamage)

    {

        if (echo == null || !echo.IsAlive)

            return;



        echo.UpgradeStats(bonusHp, bonusDamage);

        echo.transform.DOPunchScale(Vector3.one * 0.25f, 0.3f, 6, 0.5f);

    }



    private void RefreshEchoHud()

    {

        if (echo == null || UIManager.Instance == null)

            return;



        UIManager.Instance.UpdateHPBar(echo.HPRatio);

    }



    public void SpawnAlly()

    {

        EnsurePrefabs();



        Vector3 basePosition = GetEchoSpawnPosition();

        float zOffset = (allySpawnIndex % 3 - 1) * 1.2f;

        float xOffset = -1f - (allySpawnIndex / 3) * 0.8f;

        Vector3 spawnPosition = basePosition + new Vector3(xOffset, 0f, zOffset);

        allySpawnIndex++;



        Unit ally = Instantiate(allyPrefab, spawnPosition, Quaternion.identity);

        ally.gameObject.SetActive(true);

        ally.name = $"Ally_{allies.Count + 1}";

        ally.Configure(UnitTeam.Player, 60f, 15f, 2f, UnitRole.Ally);

        ally.OnDeath += HandleAllyDeath;

        allies.Add(ally);

    }



    private void HandleAllyDeath(Unit ally)

    {

        ally.OnDeath -= HandleAllyDeath;

        allies.Remove(ally);

    }



    private void HandleEnemyKilled(Unit enemy, Unit killer)

    {

        if (battleEnded)

            return;



        enemy.OnKilled -= HandleEnemyKilled;

        livingEnemies.Remove(enemy);

        ApplyKillRewards(killer);



        if (livingEnemies.Count > 0)

            return;



        if (currentWave < wavesPerLevel)

            StartCoroutine(SpawnNextWaveAfterDelay());

        else

            HandleLevelComplete();

    }



    private void ApplyKillRewards(Unit killer)

    {

        if (killer == null || killer.Team != UnitTeam.Player)

            return;



        PlayerWallet.Instance?.AddCrystals(crystalsPerKill);



        if (killer.Role == UnitRole.Echo)

            HealEchoOnKill();
    }



    private void HealEchoOnKill()

    {

        if (echo == null || !echo.IsAlive)

            return;



        float healAmount = echo.MaxHP * echoKillHealPercent;

        echo.Heal(healAmount);

        RefreshEchoHud();

        echo.transform.DOPunchScale(Vector3.one * 0.12f, 0.2f, 4, 0.6f);
    }



    private IEnumerator SpawnNextWaveAfterDelay()

    {

        yield return new WaitForSeconds(waveDelay);

        if (battleEnded || echo == null || !echo.IsAlive)

            yield break;



        StartWave(currentWave + 1);

    }



    private void HandleLevelComplete()

    {

        if (battleEnded)

            return;



        battleEnded = true;



        if (currentLevel < GameSession.MaxLevel)

            UIManager.Instance?.ShowLevelCompletePanel(currentLevel);

        else

            UIManager.Instance?.ShowFinalVictoryPanel();

    }



    private void HandleEchoDeath(Unit deadEcho)

    {

        if (battleEnded)

            return;



        deadEcho.OnDeath -= HandleEchoDeath;

        HandleDefeat();

    }



    private void HandleDefeat()

    {

        if (battleEnded)

            return;



        battleEnded = true;

        UIManager.Instance?.ShowLosePanel();

    }



    public void ContinueToLevel2()

    {

        if (currentLevel >= 2)

            return;



        Time.timeScale = 1f;

        UIManager.Instance?.HideWinPanel();



        currentLevel = 2;

        GameSession.CurrentLevel = 2;

        battleEnded = false;



        PlayerWallet.Instance?.AddCrystals(GameSession.Level2CrystalBonus);



        UIManager.Instance?.UpdateLevelDisplay(currentLevel);

        StartWave(1);

    }



    public void LoadMainMenu()

    {

        Time.timeScale = 1f;

        GameSession.ResetRun();

        SceneLoader.Load("MainMenuScene");

    }



    public void RestartGame()

    {

        Time.timeScale = 1f;

        GameSession.ResetRun();

        SceneLoader.Load("GameScene");

    }



    public void SetSpawnPoints(Transform echoPoint, Transform[] enemyPoints)

    {

        echoSpawnPoint = echoPoint;

        enemySpawnPoints = enemyPoints;

    }

}


