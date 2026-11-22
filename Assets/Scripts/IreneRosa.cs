using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Irene Rosa: de DIA é a primeira cliente (quem a faz andar até o balcão e sair
/// é o CustomerNPC, comandado pelo DayManager); de NOITE volta como o primeiro
/// zumbi (persegue o Pitcher com o EnemyChase) e serve de "cobaia" pro tutorial de tiro.
///
/// Estrutura esperada:
///   IreneRosa (Rigidbody2D, Collider2D, Health, EnemyChase, CustomerNPC, EnemyAnimator, este script)
///     Cliente  (sprite/animação normal)
///     Zumbi    (sprite/animação de zumbi)
/// No CustomerNPC dela, desmarque "Destroy When Left".
/// Ouve o TimeOfDay para saber se é dia ou noite.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Health), typeof(CustomerNPC))]
public class IreneRosa : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private GameObject clientVisual;
    [SerializeField] private GameObject zombieVisual;

    [Header("Componentes")]
    [Tooltip("O EnemyChase deste mesmo objeto (ligado só à noite).")]
    [SerializeField] private EnemyChase chase;
    [Tooltip("O collider do corpo (desligado de dia, para os tiros atravessarem).")]
    [SerializeField] private Collider2D bodyCollider;

    [Header("Noite (zumbi)")]
    [Tooltip("Onde ela reaparece como zumbi. Vazio = onde saiu da loja.")]
    [SerializeField] private Transform zombieSpawnPoint;
    [SerializeField] private bool resetHealthAtNight = true;

    [Header("Eventos (diálogo, animação, tutorial...)")]
    public UnityEvent OnBecameClient;
    public UnityEvent OnBecameZombie;

    private Rigidbody2D rb;
    private Health health;
    private CustomerNPC customer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        customer = GetComponent<CustomerNPC>();
    }

    private void OnEnable()
    {
        TimeOfDay.OnNightChanged += SetNight;
        customer.Left += OnCustomerLeft;
        SetNight(TimeOfDay.IsNight);
    }

    private void OnDisable()
    {
        TimeOfDay.OnNightChanged -= SetNight;
        customer.Left -= OnCustomerLeft;
    }

    // Saiu da loja: some até a noite.
    private void OnCustomerLeft(CustomerNPC c)
    {
        if (clientVisual != null) clientVisual.SetActive(false);
        rb.linearVelocity = Vector2.zero;   // Unity 2022: rb.velocity
        rb.simulated = false;
    }

    private void SetNight(bool night)
    {
        if (night)
        {
            customer.enabled = false;   // o CustomerNPC não pode brigar com o EnemyChase
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;

            if (zombieSpawnPoint != null)
            {
                transform.position = zombieSpawnPoint.position;
                rb.position = zombieSpawnPoint.position;
            }

            if (clientVisual != null) clientVisual.SetActive(false);
            if (zombieVisual != null) zombieVisual.SetActive(true);
            if (chase != null) chase.enabled = true;
            if (bodyCollider != null) bodyCollider.enabled = true;

            if (resetHealthAtNight) health.ResetHealth();
            OnBecameZombie?.Invoke();
        }
        else
        {
            customer.enabled = true;

            bool gone = customer.Current == CustomerNPC.State.Gone;
            if (clientVisual != null) clientVisual.SetActive(!gone);
            if (zombieVisual != null) zombieVisual.SetActive(false);
            if (chase != null) chase.enabled = false;
            if (bodyCollider != null) bodyCollider.enabled = false;

            OnBecameClient?.Invoke();
        }
    }
}
