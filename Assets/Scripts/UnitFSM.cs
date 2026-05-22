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

                float distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance > attackRange)
                {
                    transform.position = Vector3.MoveTowards(
                        transform.position,
                        target.transform.position,
                        moveSpeed * Time.deltaTime);
                }
                else
                {
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
    }

    private Unit FindTarget()
    {
        if (GameManager.Instance == null)
            return null;

        if (unit.Team == UnitTeam.Player)
            return GameManager.Instance.GetNearestLivingEnemy(transform.position);

        return GameManager.Instance.GetNearestLivingPlayerUnit(transform.position);
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
