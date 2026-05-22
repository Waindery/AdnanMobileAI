using System;
using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    public static PlayerWallet Instance { get; private set; }

    [SerializeField] private int startingCrystals = 500;

    private int crystals;

    public int Crystals => crystals;
    public event Action<int> OnCrystalsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        crystals = startingCrystals;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        OnCrystalsChanged?.Invoke(crystals);
    }

    public bool TrySpend(int amount)
    {
        if (amount < 0 || crystals < amount)
            return false;

        crystals -= amount;
        OnCrystalsChanged?.Invoke(crystals);
        return true;
    }

    public void AddCrystals(int amount)
    {
        if (amount <= 0)
            return;

        crystals += amount;
        OnCrystalsChanged?.Invoke(crystals);
    }
}
