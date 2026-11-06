using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Movimento top-down 2D do Pitcher (8 direções) + dash.
/// O dash começa BLOQUEADO: é uma habilidade obrigatória dada por Ennis Lemon / Jack Vanilla.
/// Chame UnlockDash() quando o jogador entregar o pedido especial.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Dash")]
    [SerializeField] private bool dashUnlocked = false;
    [SerializeField] private float dashSpeed = 14f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.8f;

    /// <summary>Última direção para onde o Pitcher olhou (usada depois pela mira automática).</summary>
    public Vector2 FacingDirection { get; private set; } = Vector2.down;
    public bool IsDashing { get; private set; }
    public bool DashUnlocked => dashUnlocked;

    /// <summary>Multiplicador de velocidade (a poça de chantilly da Mochaπ vai usar isso: 0.5 por 1,5 s).</summary>
    public float SpeedMultiplier { get; set; } = 1f;

    private Rigidbody2D rb;
    private InputAction moveAction;
    private InputAction dashAction;

    private Vector2 moveInput;
    private Vector2 dashDirection;
    private float dashTimeLeft;
    private float cooldownLeft;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // Movimento: WASD, direcional e analógico esquerdo do controle.
        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        moveAction.AddBinding("<Gamepad>/leftStick");
        moveAction.AddBinding("<Gamepad>/dpad");

        // Dash: Shift no teclado, B (botão leste) no controle Xbox.
        dashAction = new InputAction("Dash", InputActionType.Button);
        dashAction.AddBinding("<Keyboard>/leftShift");
        dashAction.AddBinding("<Gamepad>/buttonEast");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        dashAction.Enable();
        dashAction.performed += OnDashPerformed;
    }

    private void OnDisable()
    {
        dashAction.performed -= OnDashPerformed;
        moveAction.Disable();
        dashAction.Disable();
    }

    private void Update()
    {
        // ClampMagnitude garante que diagonais não sejam mais rápidas.
        moveInput = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);

        if (moveInput.sqrMagnitude > 0.01f)
            FacingDirection = moveInput.normalized;

        if (cooldownLeft > 0f)
            cooldownLeft -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (IsDashing)
        {
            dashTimeLeft -= Time.fixedDeltaTime;
            rb.linearVelocity = dashDirection * dashSpeed;   // Unity 2022: use rb.velocity
            if (dashTimeLeft <= 0f)
                IsDashing = false;
            return;
        }

        rb.linearVelocity = moveInput * moveSpeed * SpeedMultiplier;   // Unity 2022: use rb.velocity
    }

    private void OnDashPerformed(InputAction.CallbackContext ctx)
    {
        if (!dashUnlocked || IsDashing || cooldownLeft > 0f) return;

        // Dash na direção do movimento; parado, usa a direção que está olhando.
        dashDirection = moveInput.sqrMagnitude > 0.01f ? moveInput.normalized : FacingDirection;
        IsDashing = true;
        dashTimeLeft = dashDuration;
        cooldownLeft = dashCooldown;
    }

    /// <summary>Chamado quando Ennis ou Jack dão a habilidade.</summary>
    public void UnlockDash() => dashUnlocked = true;
}
