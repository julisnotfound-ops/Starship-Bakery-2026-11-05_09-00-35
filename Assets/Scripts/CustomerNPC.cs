
using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CustomerNPC : MonoBehaviour
{
    public enum State { Idle, Entering, Waiting, Served, Leaving, Gone }

    [Header("Movimento")]
    [SerializeField] private float walkSpeed = 1.8f;
    [SerializeField] private float arriveDistance = 0.1f;

    [Tooltip("Desloca o ponto onde ESTE cliente para.")]
    [SerializeField] private Vector2 counterOffset = Vector2.zero;

    [Header("Pedido")]
    [SerializeField] private OrderItem fixedOrder;
    [SerializeField, Min(0)] private int reward = 10;
    [SerializeField] private GameObject orderBubble;
    [SerializeField] private SpriteRenderer orderIcon;

    [Header("Cliente especial")]
    [SerializeField] private bool grantsSkillChoice = false;
    [SerializeField] private bool unlocksDash = false;

    [Header("Ao sair")]
    [SerializeField] private bool destroyWhenLeft = true;

    [Header("Física e teste")]
    [SerializeField] private bool ghostBody = false;
    [SerializeField] private bool logSteps = true;

    public static CustomerNPC Waiting { get; private set; }

    public State Current { get; private set; } = State.Idle;
    public OrderItem Order { get; private set; }

    public int Reward => reward;
    public bool GrantsSkillChoice => grantsSkillChoice;
    public bool UnlocksDash => unlocksDash;

    public event Action<CustomerNPC> Arrived;
    public event Action<CustomerNPC> Left;

    private Rigidbody2D rb;
    private Transform counterPoint;
    private Transform exitPoint;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (ghostBody)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;

            foreach (Collider2D c in GetComponentsInChildren<Collider2D>())
                c.enabled = false;
        }

        if (orderBubble != null)
            orderBubble.SetActive(false);
    }

    private void Log(string message)
    {
        if (logSteps)
            Debug.Log("[Cliente " + name + "] " + message, this);
    }

    private void OnDestroy()
    {
        if (Waiting == this)
            Waiting = null;
    }

    public void Arrive(
        Transform entry,
        Transform counter,
        Transform exit,
        OrderItem[] menu)
    {
        counterPoint = counter;
        exitPoint = exit;

        if (entry != null)
        {
            transform.position = entry.position;
            rb.position = entry.position;
        }

        if (Waiting == this)
            Waiting = null;

        Order = fixedOrder;

        if (Order == null && menu != null && menu.Length > 0)
            Order = menu[UnityEngine.Random.Range(0, menu.Length)];

        if (Order == null)
            Debug.LogError("NPC sem pedido configurado!", this);

        if (orderBubble != null)
            orderBubble.SetActive(false);

        Current = State.Entering;

        Log("Entrou. Pedido: " +
            (Order != null ? Order.displayName : "NENHUM"));
    }

    public void Serve()
    {
        if (Current != State.Waiting)
            return;

        if (Waiting == this)
            Waiting = null;

        if (orderBubble != null)
            orderBubble.SetActive(false);

        Current = State.Served;

        Log("Pedido entregue.");
    }

    public void Leave()
    {
        if (Current == State.Served)
            Current = State.Leaving;
    }

    private void FixedUpdate()
    {
        switch (Current)
        {
            case State.Entering:
                if (MoveTo(counterPoint, counterOffset))
                {
                    Current = State.Waiting;
                    Waiting = this;

                    ShowOrder();

                    Log("Chegou ao balcão.");

                    Arrived?.Invoke(this);
                }
                break;

            case State.Leaving:
                if (MoveTo(exitPoint, Vector2.zero))
                {
                    Current = State.Gone;

                    rb.linearVelocity = Vector2.zero;

                    Log("Saiu da loja.");

                    Left?.Invoke(this);

                    if (destroyWhenLeft)
                        Destroy(gameObject);
                }
                break;

            default:
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }

    private void ShowOrder()
    {
        if (Order == null)
        {
            Debug.LogError("Não existe pedido para mostrar!", this);
            return;
        }

        if (orderIcon != null)
        {
            orderIcon.sprite = Order.icon;
            orderIcon.enabled = Order.icon != null;
        }

        if (orderBubble != null)
            orderBubble.SetActive(true);
    }

    private bool MoveTo(Transform point, Vector2 offset)
    {
        if (point == null)
        {
            rb.linearVelocity = Vector2.zero;
            return true;
        }

        Vector2 toPoint = ((Vector2)point.position + offset) - rb.position;

        if (toPoint.magnitude <= arriveDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return true;
        }

        rb.linearVelocity = toPoint.normalized * walkSpeed;

        return false;
    }
}
