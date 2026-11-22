using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Lord Flan (mini-boss da noite 2). Anda devagar atrás do Pitcher, mantém
/// distância e arremessa bombas de cereja (dano em área, com aviso no chão).
/// Com pouca vida entra na FASE 2: lança várias bombas e com mais frequência.
///
/// Coloque junto com Rigidbody2D (Dynamic, Gravity Scale 0, Freeze Rotation Z),
/// um Collider2D sólido e Health (Destroy On Death marcado).
/// Se usar Stunnable, marque "Immune" (boss não pode ser paralisado).
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Health))]
public class LordFlan : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 1.2f;
    [SerializeField] private float preferredDistance = 4f;
    [SerializeField] private float distanceTolerance = 0.5f;

    [Header("Bombas de cereja")]
    [SerializeField] private CherryBomb bombPrefab;
    [SerializeField] private float throwCooldown = 3f;
    [SerializeField] private float firstThrowDelay = 1.5f;

    [Header("Fase 2")]
    [Tooltip("Entra na fase 2 quando a vida cai até essa fração (0.5 = metade).")]
    [SerializeField, Range(0.1f, 0.9f)] private float phase2HealthPercent = 0.5f;
    [Tooltip("Quantas bombas lança de uma vez na fase 2.")]
    [SerializeField, Min(1)] private int phase2BombCount = 3;
    [Tooltip("Espalhamento das bombas extras ao redor do Pitcher.")]
    [SerializeField] private float phase2Spread = 2.5f;
    [Tooltip("Multiplica o intervalo entre arremessos na fase 2 (menor = mais rápido).")]
    [SerializeField] private float phase2CooldownMultiplier = 0.6f;
    public UnityEvent OnPhase2;

    private Rigidbody2D rb;
    private Health health;
    private Transform target;
    private float cooldownLeft;
    private bool inPhase2;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) target = player.transform;
        else Debug.LogWarning("LordFlan: nenhum objeto com a tag Player na cena.", this);

        cooldownLeft = firstThrowDelay;
    }

    private void Update()
    {
        if (health.IsDead) return;

        if (!inPhase2 && health.Current <= health.Max * phase2HealthPercent)
        {
            inPhase2 = true;
            OnPhase2?.Invoke();
        }

        if (cooldownLeft > 0f) cooldownLeft -= Time.deltaTime;

        if (cooldownLeft <= 0f && target != null && bombPrefab != null)
        {
            ThrowBombs();
            cooldownLeft = throwCooldown * (inPhase2 ? phase2CooldownMultiplier : 1f);
        }
    }

    private void FixedUpdate()
    {
        if (target == null || health.IsDead)
        {
            rb.linearVelocity = Vector2.zero;   // Unity 2022: rb.velocity
            return;
        }

        Vector2 toTarget = (Vector2)target.position - rb.position;
        float distance = toTarget.magnitude;
        Vector2 direction = toTarget.normalized;

        if (distance > preferredDistance + distanceTolerance)
            rb.linearVelocity = direction * moveSpeed;
        else if (distance < preferredDistance - distanceTolerance)
            rb.linearVelocity = -direction * moveSpeed;
        else
            rb.linearVelocity = Vector2.zero;
    }

    private void ThrowBombs()
    {
        int count = inPhase2 ? phase2BombCount : 1;

        for (int i = 0; i < count; i++)
        {
            // A primeira vai onde o Pitcher está; as extras caem ao redor.
            Vector2 targetPos = target.position;
            if (i > 0) targetPos += Random.insideUnitCircle * phase2Spread;

            CherryBomb bomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);
            bomb.Launch(targetPos);
        }
    }
}
