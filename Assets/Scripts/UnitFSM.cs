using System.Collections.Generic;
using UnityEngine;

public enum UnitState
{
    Idle,
    Attack,
    Dead
}

public class UnitFSM : MonoBehaviour
{
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float personalSpace = 1.15f;

    private Unit unit;
    private UnitState state = UnitState.Idle;
    private Unit target;
    private float attackTimer;

    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    private void Start()
    {
        attackTimer = unit.AttackCooldown;
        unit.OnDeath += HandleUnitDeath;
    }

    private void OnDestroy()
    {
        if (unit != null)
            unit.OnDeath -= HandleUnitDeath;
    }

    private void Update()
    {
        if (state == UnitState.Dead)
            return;

        if (!unit.IsAlive)
        {
            EnterDead();
            return;
        }

        switch (state)
        {
            case UnitState.Idle:
                target = FindTarget();
                if (target != null)
                    state = UnitState.Attack;
                break;

            case UnitState.Attack:
                if (target == null || !target.IsAlive)
                {
                    target = null;
                    state = UnitState.Idle;
                    break;
                }

                Vector3 toTarget = target.transform.position - transform.position;
                toTarget.y = 0f;
                float distance = toTarget.magnitude;
                if (distance > attackRange)
                {
                    Vector3 targetDirection = toTarget.sqrMagnitude > 0.001f ? toTarget.normalized : Vector3.right;
                    Vector3 stopPosition = target.transform.position - targetDirection * (attackRange * 0.75f);
                    stopPosition.y = transform.position.y;

                    transform.position = Vector3.MoveTowards(
                        transform.position,
                        stopPosition,
                        moveSpeed * Time.deltaTime);
                }
                else
                {
                    KeepPersonalSpace(distance, toTarget);
                    attackTimer -= Time.deltaTime;
                    if (attackTimer <= 0f)
                    {
                        target.TakeDamage(unit.AttackDamage, unit);
                        unit.NotifyAttackPerformed();
                        attackTimer = unit.AttackCooldown;
                    }
                }
                break;
        }

        KeepDistanceFromFriendlyUnits();
    }

    private void KeepPersonalSpace(float distance, Vector3 toTarget)
    {
        if (distance <= 0.001f || distance >= personalSpace)
            return;

        Vector3 awayFromTarget = -toTarget.normalized;
        transform.position += awayFromTarget * ((personalSpace - distance) * Time.deltaTime * moveSpeed);
    }

    private Unit FindTarget()
    {
        if (GameManager.Instance == null)
            return null;

        if (unit.Team == UnitTeam.Player)
            return GameManager.Instance.GetNearestLivingEnemy(transform.position);

        return GameManager.Instance.GetNearestLivingPlayerUnit(transform.position);
    }

    private void KeepDistanceFromFriendlyUnits()
    {
        if (GameManager.Instance == null)
            return;

        IReadOnlyList<Unit> friendlyUnits = GameManager.Instance.GetLivingUnits(unit.Team);
        for (int i = 0; i < friendlyUnits.Count; i++)
        {
            Unit other = friendlyUnits[i];
            if (other == null || other == unit || !other.IsAlive)
                continue;

            Vector3 away = transform.position - other.transform.position;
            away.y = 0f;

            float distance = away.magnitude;
            if (distance <= 0.001f || distance >= personalSpace)
                continue;

            transform.position += away.normalized * ((personalSpace - distance) * Time.deltaTime * moveSpeed);
        }
    }

    private void HandleUnitDeath(Unit deadUnit)
    {
        if (deadUnit == unit)
            EnterDead();
    }

    private void EnterDead()
    {
        state = UnitState.Dead;
        target = null;
        enabled = false;
        gameObject.SetActive(false);
    }
}
