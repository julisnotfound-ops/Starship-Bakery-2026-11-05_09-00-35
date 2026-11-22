using UnityEngine;

/// <summary>
/// Controla se o Pitcher está com a arma na mão e qual animação usar.
///  - DE DIA: sem arma (animação padrão do Animator, de atender), não atira nem troca de arma.
///  - DE NOITE: com a arma equipada (animação do Pitcher Animator da arma), atira normalmente.
///  - Sem nenhuma arma comprada: sempre sem arma.
/// As armas entram na lista do PlayerShooting pela loja da Queijoca (AddWeapon).
/// Coloque no Pitcher, junto com o PlayerShooting. Ouve o TimeOfDay.
/// </summary>
[RequireComponent(typeof(PlayerShooting))]
public class PlayerWeaponAnimator : MonoBehaviour
{
    [Tooltip("Se ficar vazio, procura o Animator neste objeto ou nos filhos.")]
    [SerializeField] private Animator animator;
    [Tooltip("Desmarque se quiser a arma na mão também de dia.")]
    [SerializeField] private bool holsterDuringDay = true;

    private PlayerShooting shooting;
    private RuntimeAnimatorController defaultController;   // Pitcher SEM arma
    private bool armed;

    private void Awake()
    {
        shooting = GetComponent<PlayerShooting>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
            defaultController = animator.runtimeAnimatorController;
        else
            Debug.LogWarning("PlayerWeaponAnimator: nenhum Animator encontrado.", this);
    }

    private void OnEnable()
    {
        shooting.OnWeaponChanged += OnWeaponChanged;
        TimeOfDay.OnNightChanged += OnNightChanged;
        OnNightChanged(TimeOfDay.IsNight);
    }

    private void OnDisable()
    {
        shooting.OnWeaponChanged -= OnWeaponChanged;
        TimeOfDay.OnNightChanged -= OnNightChanged;
    }

    private void OnNightChanged(bool night)
    {
        armed = night || !holsterDuringDay;
        Refresh();
    }

    private void OnWeaponChanged(WeaponData weapon) { Refresh(); }

    private void Refresh()
    {
        WeaponData weapon = shooting.CurrentWeapon;
        bool holding = armed && weapon != null;

        shooting.CanShoot = holding;

        if (animator == null) return;

        RuntimeAnimatorController target =
            holding && weapon.pitcherAnimator != null
                ? weapon.pitcherAnimator
                : defaultController;

        if (target == null || animator.runtimeAnimatorController == target) return;

        // A animação recomeça do estado inicial; o PlayerAnimator reenvia os
        // parâmetros (andando, direção) já no próximo frame.
        animator.runtimeAnimatorController = target;
    }
}
