using UnityEngine;

[CreateAssetMenu(menuName = "Echo Protocol/Game Visual Settings")]
public class GameVisualSettings : ScriptableObject
{
    [Header("Echo")]
    [SerializeField] private GameObject echoVisualPrefab;
    [SerializeField] private Vector3 echoLocalPosition;
    [SerializeField] private Vector3 echoLocalRotation;
    [SerializeField] private Vector3 echoLocalScale = Vector3.one;

    [Header("Enemy")]
    [SerializeField] private GameObject enemyVisualPrefab;
    [SerializeField] private Vector3 enemyLocalPosition;
    [SerializeField] private Vector3 enemyLocalRotation;
    [SerializeField] private Vector3 enemyLocalScale = Vector3.one;

    [Header("Ally")]
    [SerializeField] private GameObject allyVisualPrefab;
    [SerializeField] private Vector3 allyLocalPosition;
    [SerializeField] private Vector3 allyLocalRotation;
    [SerializeField] private Vector3 allyLocalScale = Vector3.one;

    [Header("Final Boss")]
    [SerializeField] private GameObject finalBossVisualPrefab;
    [SerializeField] private Vector3 finalBossLocalPosition;
    [SerializeField] private Vector3 finalBossLocalRotation;
    [SerializeField] private Vector3 finalBossLocalScale = Vector3.one;

    public GameObject EchoVisualPrefab => echoVisualPrefab;
    public Vector3 EchoLocalPosition => echoLocalPosition;
    public Vector3 EchoLocalRotation => echoLocalRotation;
    public Vector3 EchoLocalScale => echoLocalScale;

    public GameObject EnemyVisualPrefab => enemyVisualPrefab;
    public Vector3 EnemyLocalPosition => enemyLocalPosition;
    public Vector3 EnemyLocalRotation => enemyLocalRotation;
    public Vector3 EnemyLocalScale => enemyLocalScale;

    public GameObject AllyVisualPrefab => allyVisualPrefab;
    public Vector3 AllyLocalPosition => allyLocalPosition;
    public Vector3 AllyLocalRotation => allyLocalRotation;
    public Vector3 AllyLocalScale => allyLocalScale;

    public GameObject FinalBossVisualPrefab => finalBossVisualPrefab;
    public Vector3 FinalBossLocalPosition => finalBossLocalPosition;
    public Vector3 FinalBossLocalRotation => finalBossLocalRotation;
    public Vector3 FinalBossLocalScale => finalBossLocalScale;
}
