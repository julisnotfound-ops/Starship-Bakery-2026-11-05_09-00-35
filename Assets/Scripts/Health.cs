using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Vida genérica. Serve pro Pitcher (3 cerejas = maxHealth 3) e pros inimigos.
/// Os eventos permitem ligar HUD, animações e sons sem mexer neste script.
/// </summary>
public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [Tooltip("Inimigos: ligado. Pitcher: desligado (a tela de Game Over cuida da morte).")]
    [SerializeField] private bool destroyOnDeath = true;

    public int Current { get; private set; }
    public int Max => maxHealth;
    public bool IsDead { get; private set; }

    public UnityEvent<int, int> OnHealthChanged;   // (vida atual, vida máxima)
    public UnityEvent OnDamaged;
    public UnityEvent OnDied;

    private void Awake()
    {
        Current = maxHealth;
    }

    private void Start()
    {
        // Avisa a HUD do valor inicial.
        OnHealthChanged?.Invoke(Current, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0) return;

        Current = Mathf.Max(0, Current - amount);
        OnHealthChanged?.Invoke(Current, maxHealth);
        OnDamaged?.Invoke();

        if (Current == 0)
        {
            IsDead = true;
            OnDied?.Invoke();
            if (destroyOnDeath) Destroy(gameObject);
        }
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0) return;

        Current = Mathf.Min(maxHealth, Current + amount);
        OnHealthChanged?.Invoke(Current, maxHealth);
    }

    /// <summary>Volta a vida ao máximo (usado ao reiniciar o jogo).</summary>
    public void ResetHealth()
    {
        IsDead = false;
        Current = maxHealth;
        OnHealthChanged?.Invoke(Current, maxHealth);
    }
}
