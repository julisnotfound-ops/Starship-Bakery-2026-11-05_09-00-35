using UnityEngine;

/// <summary>
/// Envia pro Animator os valores de movimento do PlayerController.
/// Usa os mesmos parâmetros do Animator do vídeo:
/// isWalking (Bool), InputX, InputY, LastInputX, LastInputY (Float).
/// Não precisa de Player Input nem de ligação de eventos.
/// </summary>
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("Animator")]
    [Tooltip("Se ficar vazio, procura um Animator neste objeto ou nos filhos.")]
    [SerializeField] private Animator animator;
    [SerializeField] private string isWalkingParam = "isWalking";    // Bool
    [SerializeField] private string inputXParam = "InputX";          // Float
    [SerializeField] private string inputYParam = "InputY";          // Float
    [SerializeField] private string lastInputXParam = "LastInputX";  // Float
    [SerializeField] private string lastInputYParam = "LastInputY";  // Float

    [Header("Extras (opcionais)")]
    [SerializeField] private string isSleepingParam = "IsSleeping";  // Bool, ignorado se não existir
    [SerializeField] private float timeToSleep = 8f;
    [SerializeField] private float dashAnimSpeed = 2f;

    private Rigidbody2D rb;
    private PlayerController player;
    private float idleTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<PlayerController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        CheckAnimatorParameters();
    }

    /// <summary>Avisa no Console se algum parâmetro não existe no Animator.</summary>
    private void CheckAnimatorParameters()
    {
        if (animator == null)
        {
            Debug.LogWarning("PlayerAnimator: nenhum Animator encontrado.", this);
            return;
        }
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("PlayerAnimator: o Animator está sem Controller.", this);
            return;
        }

        string[] needed = { isWalkingParam, inputXParam, inputYParam, lastInputXParam, lastInputYParam };
        foreach (string name in needed)
        {
            if (!HasParameter(name))
                Debug.LogWarning($"PlayerAnimator: o parâmetro '{name}' não existe no Animator.", this);
        }
    }

    private bool HasParameter(string paramName)
    {
        foreach (var p in animator.parameters)
            if (p.name == paramName) return true;
        return false;
    }

    private void Update()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;

        bool walking = rb.linearVelocity.sqrMagnitude > 0.01f;   // Unity 2022: rb.velocity
        Vector2 current = walking ? SnapToFourDirections(rb.linearVelocity) : Vector2.zero;
        Vector2 last = SnapToFourDirections(player.FacingDirection);

        idleTimer = walking ? 0f : idleTimer + Time.deltaTime;

        SetBool(isWalkingParam, walking);
        SetFloat(inputXParam, current.x);
        SetFloat(inputYParam, current.y);
        SetFloat(lastInputXParam, last.x);
        SetFloat(lastInputYParam, last.y);
        SetBool(isSleepingParam, idleTimer >= timeToSleep);

        animator.speed = player.IsDashing ? dashAnimSpeed : 1f;
    }

    /// <summary>Chame ao atirar, interagir ou levar dano pra o Pitcher não dormir no meio da ação.</summary>
    public void ResetIdleTimer() => idleTimer = 0f;

    // A sprite sheet só tem 4 direções: a diagonal vira o eixo dominante.
    private static Vector2 SnapToFourDirections(Vector2 v)
    {
        if (v.sqrMagnitude < 0.0001f) return Vector2.down;
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
