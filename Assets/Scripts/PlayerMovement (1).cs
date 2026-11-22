using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Movimento top-down 2D em 8 direções, para usar com o componente Player Input
/// (Behavior: Invoke Unity Events). A ação "Move" chama OnMove().
/// Atualiza o Animator com os MESMOS parâmetros do vídeo do tutorial:
/// isWalking (Bool), InputX, InputY, LastInputX, LastInputY (Float).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Animator")]
    [Tooltip("Se ficar vazio, procura um Animator neste objeto ou nos filhos.")]
    [SerializeField] private Animator animator;
    [SerializeField] private string isWalkingParam = "isWalking";   // Bool
    [SerializeField] private string inputXParam = "InputX";         // Float
    [SerializeField] private string inputYParam = "InputY";         // Float
    [SerializeField] private string lastInputXParam = "LastInputX"; // Float
    [SerializeField] private string lastInputYParam = "LastInputY"; // Float

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastDirection = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        CheckAnimatorParameters();
    }

    /// <summary>Avisa no Console se algum parâmetro não existe no Animator (ajuda a achar o erro).</summary>
    private void CheckAnimatorParameters()
    {
        if (animator == null)
        {
            Debug.LogWarning("PlayerMovement: nenhum Animator encontrado. O Pitcher vai andar, mas sem animação.", this);
            return;
        }
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("PlayerMovement: o Animator está sem Controller.", this);
            return;
        }

        string[] needed = { isWalkingParam, inputXParam, inputYParam, lastInputXParam, lastInputYParam };
        foreach (string name in needed)
        {
            if (!HasParameter(name))
                Debug.LogWarning($"PlayerMovement: o parâmetro '{name}' não existe no Animator.", this);
        }
    }

    private bool HasParameter(string paramName)
    {
        foreach (var p in animator.parameters)
            if (p.name == paramName) return true;
        return false;
    }

    /// <summary>Ligar em: Player Input > Events > (mapa) > Move.</summary>
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = Vector2.ClampMagnitude(context.ReadValue<Vector2>(), 1f);

        if (moveInput.sqrMagnitude > 0.01f)
            lastDirection = SnapToFourDirections(moveInput);
    }

    /// <summary>Mesmo que OnMove. Existe porque o vídeo do tutorial usa o nome "Move" na ligação do evento.</summary>
    public void Move(InputAction.CallbackContext context) => OnMove(context);

    private void Update()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;

        bool walking = moveInput.sqrMagnitude > 0.01f;
        Vector2 dir = walking ? SnapToFourDirections(moveInput) : Vector2.zero;

        SetBool(isWalkingParam, walking);
        SetFloat(inputXParam, dir.x);
        SetFloat(inputYParam, dir.y);
        SetFloat(lastInputXParam, lastDirection.x);
        SetFloat(lastInputYParam, lastDirection.y);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;   // Unity 2022: use rb.velocity
    }

    // A sprite sheet só tem 4 direções: a diagonal vira o eixo dominante.
    private static Vector2 SnapToFourDirections(Vector2 v)
    {
        return Mathf.Abs(v.x) > Mathf.Abs(v.y)
            ? new Vector2(Mathf.Sign(v.x), 0f)
            : new Vector2(0f, Mathf.Sign(v.y));
    }

    private void SetBool(string paramName, bool value)
    {
        if (HasParameter(paramName)) animator.SetBool(paramName, value);
    }

    private void SetFloat(string paramName, float value)
    {
        if (HasParameter(paramName)) animator.SetFloat(paramName, value);
    }
}
