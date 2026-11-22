using UnityEngine;

/// <summary>
/// Projétil. Causa dano em quem tem Health e some ao bater em algo sólido.
/// Ignora quem tem a mesma tag de quem atirou (a bala do Pitcher não acerta o Pitcher).
/// Efeitos extras, definidos por quem atira (PlayerShooting):
///  - StunDuration: deixa o inimigo parado (precisa do componente Stunnable nele).
///  - Bumerangue: vai até uma distância, volta pro dono e atravessa inimigos.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float boomerangLifetime = 8f;
    [SerializeField] private float boomerangSpinSpeed = 720f;

    [Header("Rotação do sprite")]
    [Tooltip("Gira a bala para apontar na direção do tiro (certo em todas as direções, inclusive diagonais).")]
    [SerializeField] private bool rotateToDirection = true;
    [Tooltip("Pra onde o sprite aponta no desenho original: direita = 0, cima = 90, esquerda = 180, baixo = 270 (ou -90).")]
    [SerializeField] private float spriteAngleOffset = 0f;

    /// <summary>Segundos que o inimigo fica parado ao ser acertado (0 = nenhum).</summary>
    public float StunDuration { get; set; }

    public bool IsBoomerang { get; private set; }

    private Rigidbody2D rb;
    private int damage;
    private float speed;
    private string ownerTag;
    private float age;

    // Bumerangue
    private Transform returnTarget;
    private Vector2 startPosition;
    private float boomerangRange;
    private bool returning;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    /// <summary>Chamado por quem atira, logo depois de criar a bala.</summary>
    public void Init(Vector2 direction, float bulletSpeed, int damageAmount, string ownerTagName)
    {
        speed = bulletSpeed;
        damage = damageAmount;
        ownerTag = ownerTagName;
        rb.linearVelocity = direction.normalized * speed;   // Unity 2022: rb.velocity

        if (rotateToDirection && direction.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - spriteAngleOffset;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    /// <summary>Transforma a bala em bumerangue. Chame logo depois do Init.</summary>
    public void SetBoomerang(Transform target, float range)
    {
        IsBoomerang = true;
        returnTarget = target;
        boomerangRange = range;
        startPosition = transform.position;
    }

    private void Update()
    {
        age += Time.deltaTime;
        float maxLife = IsBoomerang ? boomerangLifetime : lifetime;
        if (age >= maxLife)
        {
            Destroy(gameObject);
            return;
        }

        if (IsBoomerang)
            transform.Rotate(0f, 0f, boomerangSpinSpeed * Time.deltaTime);   // só visual
    }

    private void FixedUpdate()
    {
        if (!IsBoomerang) return;

        if (!returning && Vector2.Distance(startPosition, rb.position) >= boomerangRange)
            returning = true;

        if (returning)
        {
            if (returnTarget == null)
            {
                Destroy(gameObject);
                return;
            }

            Vector2 toOwner = (Vector2)returnTarget.position - rb.position;
            if (toOwner.magnitude < 0.5f)
            {
                Destroy(gameObject);   // voltou pro Pitcher
                return;
            }
            rb.linearVelocity = toOwner.normalized * speed;   // Unity 2022: rb.velocity
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger) return;
        if (!string.IsNullOrEmpty(ownerTag) && other.CompareTag(ownerTag)) return;

        Health health = other.GetComponentInParent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);

            if (StunDuration > 0f)
            {
                Stunnable stunnable = other.GetComponentInParent<Stunnable>();
                if (stunnable != null) stunnable.Stun(StunDuration);
            }

            if (!IsBoomerang) Destroy(gameObject);   // o bumerangue atravessa inimigos
            return;
        }

        // Bateu em algo sólido (parede, móvel...).
        if (IsBoomerang) returning = true;
        else Destroy(gameObject);
    }
}
