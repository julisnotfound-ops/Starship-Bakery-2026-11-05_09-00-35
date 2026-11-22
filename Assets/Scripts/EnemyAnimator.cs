using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Liga o movimento do inimigo ao Animator (igual ao que o PlayerAnimator faz
/// com o Pitcher). Lê a velocidade do Rigidbody2D e preenche os parâmetros
/// do Animator de TODOS os filhos (ex.: Irene Rosa cliente e Irene Rosa Zumbi).
///
/// Coloque no objeto PRINCIPAL (o que tem o Rigidbody2D), não nos filhos.
/// Os nomes dos parâmetros podem ser mudados no Inspector, caso o seu
/// Animator use outros nomes. Parâmetros que não existirem são ignorados.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAnimator : MonoBehaviour
{
    [Header("Nomes dos parâmetros no Animator")]
    [SerializeField] private string isWalkingParam = "isWalking";   // bool
    [SerializeField] private string inputXParam = "InputX";         // float
    [SerializeField] private string inputYParam = "InputY";         // float
    [SerializeField] private string lastInputXParam = "LastInputX"; // float
    [SerializeField] private string lastInputYParam = "LastInputY"; // float

    [Header("Ajustes")]
    [Tooltip("Velocidade mínima para contar como 'andando'.")]
    [SerializeField] private float walkThreshold = 0.05f;
    [Tooltip("Direção em que ele olha antes de se mover pela primeira vez (0,-1 = para baixo/frente).")]
    [SerializeField] private Vector2 startFacing = new Vector2(0f, -1f);

    private Rigidbody2D rb;
    private Animator[] animators;
    private readonly Dictionary<Animator, HashSet<string>> parameterNames =
        new Dictionary<Animator, HashSet<string>>();
    private Vector2 lastDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animators = GetComponentsInChildren<Animator>(true);   // inclui filhos desligados
        lastDirection = startFacing.sqrMagnitude > 0f ? startFacing.normalized : Vector2.down;
    }

    private void Update()
    {
        Vector2 velocity = rb.linearVelocity;   // Unity 2022: rb.velocity
        bool walking = velocity.magnitude > walkThreshold;
        Vector2 input = walking ? velocity.normalized : Vector2.zero;
        if (walking) lastDirection = input;

        foreach (Animator animator in animators)
        {
            // Animator de filho desligado (ex.: o zumbi de dia) não aceita parâmetros.
            if (animator == null || !animator.gameObject.activeInHierarchy) continue;

            HashSet<string> names = GetParameterNames(animator);

            SetBool(animator, names, isWalkingParam, walking);
            SetFloat(animator, names, inputXParam, input.x);
            SetFloat(animator, names, inputYParam, input.y);
            SetFloat(animator, names, lastInputXParam, lastDirection.x);
            SetFloat(animator, names, lastInputYParam, lastDirection.y);
        }
    }

    // Guarda a lista de parâmetros de cada Animator na primeira vez que ele está ativo.
    private HashSet<string> GetParameterNames(Animator animator)
    {
        if (!parameterNames.TryGetValue(animator, out HashSet<string> names))
        {
            names = new HashSet<string>();
            foreach (AnimatorControllerParameter parameter in animator.parameters)
                names.Add(parameter.name);
            parameterNames[animator] = names;
        }
        return names;
    }

    private static void SetBool(Animator animator, HashSet<string> names, string param, bool value)
    {
        if (!string.IsNullOrEmpty(param) && names.Contains(param)) animator.SetBool(param, value);
    }

    private static void SetFloat(Animator animator, HashSet<string> names, string param, float value)
    {
        if (!string.IsNullOrEmpty(param) && names.Contains(param)) animator.SetFloat(param, value);
    }
}
