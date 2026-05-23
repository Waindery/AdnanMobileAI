using System;
using UnityEngine;

public enum UnitTeam
{
    Player,
    Enemy
}

public enum UnitRole
{
    Echo,
    Ally,
    Enemy
}

public class Unit : MonoBehaviour
{
    [SerializeField] private UnitTeam team;
    [SerializeField] private UnitRole role = UnitRole.Enemy;
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackCooldown = 1.5f;

    private float currentHP;

    public UnitTeam Team => team;
    public UnitRole Role => role;
    public float MaxHP => maxHP;
    public float CurrentHP => currentHP;
    public float AttackDamage => attackDamage;
    public float AttackCooldown => attackCooldown;
    public float HPRatio => maxHP > 0f ? currentHP / maxHP : 0f;
    public bool IsAlive => currentHP > 0f;
    public bool IsPlayerUnit => team == UnitTeam.Player;

    public event Action<Unit> OnDeath;
    public event Action<Unit, Unit> OnKilled;
    public event Action<Unit, float, Unit> OnDamageTaken;
    public event Action OnAttackPerformed;
    public event Action OnDamaged;

    private Unit lastAttacker;

    private void Awake()
    {
        currentHP = maxHP;
    }

    public void Configure(UnitTeam unitTeam, float hp, float damage, float cooldown, UnitRole unitRole = UnitRole.Enemy)
    {
        team = unitTeam;
        role = unitRole;
        maxHP = hp;
        attackDamage = damage;
        attackCooldown = cooldown;
        currentHP = maxHP;
    }

    public void UpgradeStats(float bonusHp, float bonusDamage, bool healBonusHp = true)
    {
        maxHP += bonusHp;
        attackDamage += bonusDamage;

        if (healBonusHp)
            currentHP = Mathf.Min(maxHP, currentHP + bonusHp);
        else
            currentHP = Mathf.Min(currentHP, maxHP);
    }

    public void TakeDamage(float amount, Unit attacker = null)
    {
        if (!IsAlive)
            return;

        if (attacker != null)
            lastAttacker = attacker;

        currentHP = Mathf.Max(0f, currentHP - amount);
        OnDamageTaken?.Invoke(this, amount, attacker);
        OnDamaged?.Invoke();

        FloatingDamageNumber.Show(transform.position + Vector3.up * 0.9f, amount, attacker);

        if (currentHP <= 0f)
        {
            OnKilled?.Invoke(this, lastAttacker);
            OnDeath?.Invoke(this);
        }
    }

    public void Heal(float amount)
    {
        if (!IsAlive || amount <= 0f)
            return;

        currentHP = Mathf.Min(maxHP, currentHP + amount);
        OnDamaged?.Invoke();
    }

    public void NotifyAttackPerformed()
    {
        OnAttackPerformed?.Invoke();
    }
}
