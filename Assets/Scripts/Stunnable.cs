using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Deixa um inimigo "parado" por alguns segundos (usado pela arma que paralisa).
/// Enquanto está parado: o corpo congela, a animação pausa, o sprite fica azulado
/// e os scripts listados em "Disable While Stunned" (a IA do inimigo) ficam desligados.
/// Coloque este componente em todo inimigo que pode ser paralisado.
/// </summary>
public class Stunnable : MonoBehaviour
{
    [Header("Efeito de parado")]
    [Tooltip("Scripts desligados enquanto o inimigo está parado (ex.: o script de IA/ataque dele).")]
    [SerializeField] private MonoBehaviour[] disableWhileStunned;
    [SerializeField] private Color stunTint = new Color(0.55f, 0.85f, 1f);
    [Tooltip("Marque nos bosses se eles não puderem ser paralisados.")]
    [SerializeField] private bool immune = false;

    public bool IsStunned { get; private set; }

    public UnityEvent OnStunned;
    public UnityEvent OnRecovered;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer[] renderers;

    private Color[] originalColors;
    private bool[] wasEnabled;
    private RigidbodyConstraints2D originalConstraints;
    private float originalAnimatorSpeed = 1f;
    private float timeLeft;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        renderers = GetComponentsInChildren<SpriteRenderer>();
    }

    /// <summary>Deixa o inimigo parado. Acertar de novo renova o tempo, sem somar.</summary>
    public void Stun(float duration)
    {
        if (immune || duration <= 0f) return;

        timeLeft = Mathf.Max(timeLeft, duration);
        if (!IsStunned) BeginStun();
    }

    private void Update()
    {
        if (!IsStunned) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f) EndStun();
    }

    private void BeginStun()
    {
        IsStunned = true;

        if (rb != null)
        {
            originalConstraints = rb.constraints;
            rb.linearVelocity = Vector2.zero;   // Unity 2022: rb.velocity
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        if (animator != null)
        {
            originalAnimatorSpeed = animator.speed;
            animator.speed = 0f;
        }

        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            originalColors[i] = renderers[i].color;
            renderers[i].color = stunTint;
        }

        wasEnabled = new bool[disableWhileStunned.Length];
        for (int i = 0; i < disableWhileStunned.Length; i++)
        {
            if (disableWhileStunned[i] == null) continue;
            wasEnabled[i] = disableWhileStunned[i].enabled;
            disableWhileStunned[i].enabled = false;
        }

        OnStunned?.Invoke();
    }

    private void EndStun()
    {
        IsStunned = false;
        timeLeft = 0f;

        if (rb != null) rb.constraints = originalConstraints;
        if (animator != null) animator.speed = originalAnimatorSpeed;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null) renderers[i].color = originalColors[i];
        }

        for (int i = 0; i < disableWhileStunned.Length; i++)
        {
            if (disableWhileStunned[i] != null && wasEnabled[i])
                disableWhileStunned[i].enabled = true;
        }

        OnRecovered?.Invoke();
    }
}
