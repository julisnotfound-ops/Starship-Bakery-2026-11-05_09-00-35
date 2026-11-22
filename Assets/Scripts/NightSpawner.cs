
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Sequência da noite:
/// 1) Irene zumbi aparece e ensina o combate.
/// 2) Primeira horda aparece inteira.
/// 3) Segunda horda aparece inteira.
/// 4) Mini-boss aparece.
/// 5) Quando tudo termina, avisa o DayManager.
/// </summary>
public class NightSpawner : MonoBehaviour
{
    public static event System.Action OnNightCleared;

    [System.Serializable]
    public class Group
    {
        public GameObject prefab;

        [Min(1)]
        public int count = 1;
    }

    [System.Serializable]
    public class NightPlan
    {
        public string name = "Noite";

        [Header("Tutorial")]
        public GameObject tutorialIrenePrefab;

        [Header("Primeira horda")]
        public Group[] firstHorde;

        [Header("Segunda horda")]
        public Group[] secondHorde;

        [Header("Mini-boss")]
        public GameObject bossPrefab;
    }

    [Header("Planos das noites")]
    [SerializeField] private NightPlan[] nights;

    [Header("Onde os inimigos nascem")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Tempos")]
    [SerializeField] private float startDelay = 1.5f;
    [SerializeField] private float hordeDelay = 2f;
    [SerializeField] private float endDelay = 1.5f;

    private int alive;

    private void OnEnable()
    {
        TimeOfDay.OnNightChanged += HandleNight;
    }

    private void OnDisable()
    {
        TimeOfDay.OnNightChanged -= HandleNight;
    }

    private void HandleNight(bool night)
    {
        StopAllCoroutines();

        if (night)
            StartCoroutine(RunNight());
    }

    private IEnumerator RunNight()
    {
        alive = 0;

        if (nights == null || nights.Length == 0)
        {
            Debug.LogWarning("NightSpawner: nenhum plano de noite configurado.", this);
            yield break;
        }

        NightPlan plan = nights[
            Mathf.Clamp(DayManager.DayNumber - 1, 0, nights.Length - 1)
        ];

        yield return new WaitForSeconds(startDelay);

        // 1. Tutorial da Irene zumbi.
        if (plan.tutorialIrenePrefab != null)
        {
            Spawn(plan.tutorialIrenePrefab);

            yield return new WaitUntil(() => alive <= 0);

            yield return new WaitForSeconds(hordeDelay);
        }

        // 2. Primeira horda: todos aparecem juntos.
        SpawnHorde(plan.firstHorde);

        yield return new WaitUntil(() => alive <= 0);

        yield return new WaitForSeconds(hordeDelay);

        // 3. Segunda horda: todos aparecem juntos.
        SpawnHorde(plan.secondHorde);

        yield return new WaitUntil(() => alive <= 0);

        yield return new WaitForSeconds(hordeDelay);

        // 4. Mini-boss.
        if (plan.bossPrefab != null)
        {
            Spawn(plan.bossPrefab);

            yield return new WaitUntil(() => alive <= 0);
        }

        yield return new WaitForSeconds(endDelay);

        OnNightCleared?.Invoke();
    }

    private void SpawnHorde(Group[] horde)
    {
        if (horde == null)
            return;

        foreach (Group group in horde)
        {
            if (group == null || group.prefab == null)
                continue;

            for (int i = 0; i < group.count; i++)
            {
                Spawn(group.prefab);
            }
        }

        Debug.Log("Horda apareceu! Inimigos vivos: " + alive);
    }

    private void Spawn(GameObject prefab)
    {
        Vector3 position = transform.position;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform point = spawnPoints[
                Random.Range(0, spawnPoints.Length)
            ];

            if (point != null)
                position = point.position;
        }

        GameObject enemy = Instantiate(
            prefab,
            position,
            Quaternion.identity
        );

        Health health = enemy.GetComponent<Health>();

        if (health != null)
        {
            alive++;

            health.OnDied.AddListener(() =>
            {
                alive--;
            });
        }
        else
        {
            Debug.LogError(
                "O inimigo " + prefab.name +
                " não possui o componente Health!",
                enemy
            );
        }
    }
}
