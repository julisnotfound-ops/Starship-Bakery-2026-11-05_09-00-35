using UnityEngine;

/// <summary>
/// Inimigo que SÓ persegue o Pitcher e causa dano quando está perto dele
/// (usado na Irene Rosa). Não atira e não faz aviso: chegou perto, machuca,
/// com um intervalo entre um dano e o próximo.
///
/// Coloque junto com Rigidbody2D (Dynamic, Gravity Scale 0, Freeze Rotation Z),
/// um Collider2D sólido (Is Trigger desmarcado) e Health. Se for paralisável,
/// adicione Stunnable e arraste ESTE script em "Disable While Stunned".
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChase : MonoBehaviour
{
    [Header("Perseguição")]
    [SerializeField] private float moveSpeed = 2.5f;
    [Tooltip("Distância em que ele começa a perseguir. 0 = persegue sempre.")]
    [SerializeField] private float detectionRange = 0f;
    [Tooltip("Ele para de andar a essa distância do Pitcher.")]
    [SerializeField] private float stopDistance = 0.6f;

    [Header("Dano por proximidade")]
    [Tooltip("Distância em que o Pitcher leva dano. Deixe um pouco maior que o Stop Distance.")]
    [SerializeField] private float damageRange = 1f;
    [SerializeField] private int damage = 1;
    [Tooltip("Segundos entre um dano e o próximo.")]
    [SerializeField] private float damageCooldown = 1f;

    private Rigidbody2D rb;
    private Transform target;
    private Health targetHealth;
    private float cooldownLeft;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            targetHealth = player.GetComponentInParent<Health>();
        }
        else
        {
            Debug.LogWarning("EnemyChase: nenhum objeto com a tag Player na cena.", this);
        }
    }

    private void Update()
    {
        if (cooldownLeft > 0f) cooldownLeft -= Time.deltaTime;

        if (target == null || targetHealth == null || targetHealth.IsDead) return;

        float distance = Vector2.Distance(transform.position, target.position);
        if (distance <= damageRange && cooldownLeft <= 0f)
        {
            targetHealth.TakeDamage(damage);
            cooldownLeft = damageCooldown;
        }
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            rb.linearVelocity = Vector2.zero;   // Unity 2022: rb.velocity
            return;
        }

        Vector2 toTarget = (Vector2)target.position - rb.position;
        float distance = toTarget.magnitude;

        bool inRange = detectionRange <= 0f || distance <= detectionRange;
        if (!inRange || distance <= stopDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = toTarget.normalized * moveSpeed;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRange);
    }
}
