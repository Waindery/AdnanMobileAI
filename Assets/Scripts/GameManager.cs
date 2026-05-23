using System.Collections;

using System.Collections.Generic;

using DG.Tweening;

using UnityEngine;



public class GameManager : MonoBehaviour

{

    public static GameManager Instance { get; private set; }



    [SerializeField] private Unit echoPrefab;

    [SerializeField] private Unit enemyPrefab;

    [Header("Store Asset Visuals (Assign Prefabs Here)")]
    [SerializeField] private GameVisualSettings visualSettings;
    [SerializeField] private GameObject echoVisualPrefab;
    [SerializeField] private GameObject enemyVisualPrefab;
    [SerializeField] private GameObject allyVisualPrefab;
    [SerializeField] private GameObject finalBossVisualPrefab;

    [SerializeField] private Transform echoSpawnPoint;

    [SerializeField] private Transform[] enemySpawnPoints;

    [SerializeField] private float waveDelay = 2.5f;

    [SerializeField] private int maxAllies = 4;

    [SerializeField] private int wavesPerLevel = 3;

    [SerializeField] private float echoKillHealPercent = 0.1f;

    [SerializeField] private int crystalsPerKill = 30;



    private readonly List<Unit> livingEnemies = new List<Unit>();

    private readonly List<Unit> allies = new List<Unit>();

    private readonly List<Unit> livingPlayerUnits = new List<Unit>();

    private Unit echo;

    private Unit allyPrefab;

    private int currentLevel;

    private int currentWave;

    private bool bossActive;

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

        echoKillHealPercent = 0.1f;

    }



    private void OnDestroy()

    {

        if (Instance == this)

            Instance = null;

    }



    private void Start()

    {

        Time.timeScale = 1f;

        MobileScreenSetup.Apply();

        ConfigureBattleCamera();

        BattleBackground.Create();

        EnsurePrefabs();

        LoadVisualSettings();

        SpawnEcho();

        StartWave(1);

        UIManager.Instance?.UpdateLevelDisplay(currentLevel);

    }



    private static void ConfigureBattleCamera()

    {

        Camera cam = Camera.main;

        if (cam == null)

            return;



        cam.orthographic = false;
        cam.fieldOfView = 50f;

        cam.transform.position = new Vector3(0.4f, 6.5f, -6.6f);

        cam.transform.rotation = Quaternion.Euler(48f, 0f, 0f);

        cam.backgroundColor = new Color(0.12f, 0.14f, 0.18f);

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



    private void LoadVisualSettings()

    {

        if (visualSettings == null)

            visualSettings = Resources.Load<GameVisualSettings>("GameVisualSettings");



        if (visualSettings == null)

            return;



        if (echoVisualPrefab == null)

            echoVisualPrefab = visualSettings.EchoVisualPrefab;



        if (enemyVisualPrefab == null)

            enemyVisualPrefab = visualSettings.EnemyVisualPrefab;



        if (allyVisualPrefab == null)

            allyVisualPrefab = visualSettings.AllyVisualPrefab;



        if (finalBossVisualPrefab == null)

            finalBossVisualPrefab = visualSettings.FinalBossVisualPrefab;

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

        ApplyVisualPrefab(
            echo,
            echoVisualPrefab,
            visualSettings != null ? visualSettings.EchoLocalPosition : Vector3.zero,
            visualSettings != null ? visualSettings.EchoLocalRotation : Vector3.zero,
            visualSettings != null ? visualSettings.EchoLocalScale : Vector3.one);

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



    private Vector3 GetAllyFormationPosition(int index)

    {

        Vector3 basePosition = GetEchoSpawnPosition();

        float zOffset = (index % 4 - 1.5f) * 1.15f;

        float xOffset = -1f - (index / 4) * 0.9f;

        return basePosition + new Vector3(xOffset, 0f, zOffset);

    }



    private void ResetPlayerFormation()

    {

        if (echo != null && echo.IsAlive)

        {

            echo.transform.position = GetEchoSpawnPosition();

            ResetEchoScale();

        }



        int formationIndex = 0;

        for (int i = 0; i < allies.Count; i++)

        {

            Unit ally = allies[i];

            if (ally == null || !ally.IsAlive)

                continue;



            ally.transform.position = GetAllyFormationPosition(formationIndex);

            formationIndex++;

        }

    }



    private void StartWave(int wave)

    {

        bossActive = false;

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

            ApplyVisualPrefab(
                enemy,
                enemyVisualPrefab,
                visualSettings != null ? visualSettings.EnemyLocalPosition : Vector3.zero,
                visualSettings != null ? visualSettings.EnemyLocalRotation : Vector3.zero,
                visualSettings != null ? visualSettings.EnemyLocalScale : Vector3.one);

            if (enemy.GetComponent<WorldUnitHealthBar>() == null)
                enemy.gameObject.AddComponent<WorldUnitHealthBar>();

            enemy.OnKilled += HandleEnemyKilled;

            livingEnemies.Add(enemy);

        }

    }



    private static int GetEnemyCountForWave(int level, int wave)

    {

        int count = 3 + wave;

        count += Mathf.Max(0, level - 1);



        return count;

    }



    private static void GetEnemyStatsForLevel(int level, out float hp, out float damage, out float cooldown)

    {

        hp = 40f;

        damage = 10f;

        cooldown = 2f;



        if (level >= 2)
        {
            hp = 40f + (level - 1) * 18f;
            damage = 10f + (level - 1) * 4f;
            cooldown = Mathf.Max(1.25f, 2f - (level - 1) * 0.12f);
        }

    }



    private Vector3 GetEnemySpawnPosition(int index)

    {

        int row = index / 4;
        int column = index % 4;
        float x = 3.2f + row * 1.35f;
        float z = -2.7f + column * 1.8f;

        return BattleGround.SpawnPosition(x, z);

    }



    private void StartBossEncounter()

    {

        bossActive = true;

        currentWave = wavesPerLevel + 1;

        UpdateWaveHud();



        GetBossStatsForLevel(currentLevel, out float bossHp, out float bossDamage, out float bossCooldown);

        Vector3 spawnPosition = BattleGround.SpawnPosition(5.2f, 0f);

        Unit boss = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        boss.name = $"Level {currentLevel} Final Boss";

        boss.transform.localScale = Vector3.one * 1.45f;

        boss.transform.position = new Vector3(spawnPosition.x, 1.45f, spawnPosition.z);

        boss.gameObject.SetActive(true);

        boss.Configure(UnitTeam.Enemy, bossHp, bossDamage, bossCooldown);

        bool hasBossVisual = finalBossVisualPrefab != null;
        ApplyVisualPrefab(
            boss,
            hasBossVisual ? finalBossVisualPrefab : enemyVisualPrefab,
            visualSettings != null ? (hasBossVisual ? visualSettings.FinalBossLocalPosition : visualSettings.EnemyLocalPosition) : Vector3.zero,
            visualSettings != null ? (hasBossVisual ? visualSettings.FinalBossLocalRotation : visualSettings.EnemyLocalRotation) : Vector3.zero,
            visualSettings != null ? (hasBossVisual ? visualSettings.FinalBossLocalScale : visualSettings.EnemyLocalScale) : Vector3.one);

        if (boss.GetComponent<WorldUnitHealthBar>() == null)
            boss.gameObject.AddComponent<WorldUnitHealthBar>();

        boss.OnKilled += HandleEnemyKilled;

        livingEnemies.Add(boss);

    }



    private static void GetBossStatsForLevel(int level, out float hp, out float damage, out float cooldown)

    {

        hp = 150f + level * 75f;
        damage = 12f + level * 5f;
        cooldown = Mathf.Max(1.05f, 1.9f - level * 0.12f);

    }



    private void UpdateWaveHud()

    {

        UIManager.Instance?.UpdateWaveText(currentLevel, currentWave, wavesPerLevel, bossActive);

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



    public IReadOnlyList<Unit> GetLivingUnits(UnitTeam team)

    {

        if (team == UnitTeam.Enemy)

            return livingEnemies;



        livingPlayerUnits.Clear();

        if (echo != null && echo.IsAlive)

            livingPlayerUnits.Add(echo);



        for (int i = 0; i < allies.Count; i++)

        {

            Unit ally = allies[i];

            if (ally != null && ally.IsAlive)

                livingPlayerUnits.Add(ally);

        }



        return livingPlayerUnits;

    }



    private void ApplyVisualPrefab(
        Unit unit,
        GameObject visualPrefab,
        Vector3 localPosition,
        Vector3 localRotation,
        Vector3 localScale)

    {

        if (unit == null || visualPrefab == null)

            return;



        Transform oldVisual = unit.transform.Find("UnitVisual");

        if (oldVisual != null)

            Destroy(oldVisual.gameObject);



        Renderer[] placeholderRenderers = unit.GetComponentsInChildren<Renderer>();

        for (int i = 0; i < placeholderRenderers.Length; i++)

            placeholderRenderers[i].enabled = false;



        GameObject visual = Instantiate(visualPrefab, unit.transform);

        visual.name = "UnitVisual";

        visual.transform.localPosition = localPosition;

        visual.transform.localRotation = Quaternion.Euler(localRotation);

        visual.transform.localScale = localScale;

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



        Vector3 spawnPosition = GetAllyFormationPosition(allies.Count);



        Unit ally = Instantiate(allyPrefab, spawnPosition, Quaternion.identity);

        ally.gameObject.SetActive(true);

        ally.name = $"Ally_{allies.Count + 1}";

        ally.Configure(UnitTeam.Player, 60f, 15f, 2f, UnitRole.Ally);

        ApplyVisualPrefab(
            ally,
            allyVisualPrefab,
            visualSettings != null ? visualSettings.AllyLocalPosition : Vector3.zero,
            visualSettings != null ? visualSettings.AllyLocalRotation : Vector3.zero,
            visualSettings != null ? visualSettings.AllyLocalScale : Vector3.one);

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



        ResetPlayerFormation();



        if (currentWave < wavesPerLevel)

            StartCoroutine(SpawnNextWaveAfterDelay());

        else if (!bossActive)

            StartCoroutine(SpawnBossAfterDelay());

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

        echo.transform.DOKill();
        echo.transform.localScale = Vector3.one;
        echo.transform.DOPunchScale(Vector3.one * 0.12f, 0.2f, 4, 0.6f)
            .OnComplete(ResetEchoScale);
    }



    private void ResetEchoScale()

    {

        if (echo == null)

            return;



        echo.transform.DOKill();

        echo.transform.localScale = Vector3.one;

    }



    private IEnumerator SpawnNextWaveAfterDelay()

    {

        yield return new WaitForSeconds(waveDelay);

        if (battleEnded || echo == null || !echo.IsAlive)

            yield break;



        StartWave(currentWave + 1);

    }



    private IEnumerator SpawnBossAfterDelay()

    {

        yield return new WaitForSeconds(waveDelay);

        if (battleEnded || echo == null || !echo.IsAlive)

            yield break;



        StartBossEncounter();

    }



    private void HandleLevelComplete()

    {

        if (battleEnded)

            return;



        ResetEchoScale();

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

        if (currentLevel >= GameSession.MaxLevel)

            return;



        Time.timeScale = 1f;

        UIManager.Instance?.HideWinPanel();

        ResetPlayerFormation();



        currentLevel++;

        GameSession.CurrentLevel = currentLevel;

        battleEnded = false;



        PlayerWallet.Instance?.AddCrystals(GameSession.GetLevelStartBonus(currentLevel));



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


