using UnityEngine;

public enum GachaRewardType
{
    Nothing,
    EchoUpgrade,
    NewAlly,
    EchoUpgradeRare
}

public class GachaManager : MonoBehaviour
{
    public static GachaManager Instance { get; private set; }

    [SerializeField] private int singlePullCost = 100;
    [SerializeField] [Range(0f, 0.5f)] private float nothingChance = 0.12f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public int SinglePullCost => singlePullCost;

    public bool TrySinglePull(out string resultMessage, out bool gotReward)
    {
        resultMessage = string.Empty;
        gotReward = false;

        if (PlayerWallet.Instance == null)
        {
            resultMessage = "Wallet unavailable.";
            return false;
        }

        if (!PlayerWallet.Instance.TrySpend(singlePullCost))
        {
            resultMessage = $"Need {singlePullCost} crystals!";
            return false;
        }

        if (GameManager.Instance == null)
        {
            resultMessage = "Battle unavailable.";
            PlayerWallet.Instance.AddCrystals(singlePullCost);
            return false;
        }

        GachaRewardType reward = RollReward();
        resultMessage = GameManager.Instance.ApplyGachaReward(reward);
        gotReward = reward != GachaRewardType.Nothing;
        return true;
    }

    private GachaRewardType RollReward()
    {
        float roll = Random.value;

        if (roll < nothingChance)
            return GachaRewardType.Nothing;

        float adjusted = (roll - nothingChance) / (1f - nothingChance);

        if (adjusted < 0.5f)
            return GachaRewardType.EchoUpgrade;

        if (adjusted < 0.85f)
            return GachaRewardType.NewAlly;

        return GachaRewardType.EchoUpgradeRare;
    }
}
