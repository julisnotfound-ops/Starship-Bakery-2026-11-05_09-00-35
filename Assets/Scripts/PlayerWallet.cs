using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Dinheiro do Pitcher. Coloque no Pitcher. A loja da Queijoca usa TrySpend(preço).
/// O bônus de dinheiro (habilidade do casal) mexe no MoneyMultiplier.
/// </summary>
public class PlayerWallet : MonoBehaviour
{
    [SerializeField] private int startMoney = 0;

    public int Money { get; private set; }
    public float MoneyMultiplier { get; set; } = 1f;

    public UnityEvent<int> OnMoneyChanged;   // (dinheiro atual) - ligue na HUD

    private void Awake() { Money = startMoney; }
    private void Start() { OnMoneyChanged?.Invoke(Money); }

    /// <summary>Ganha dinheiro (já aplica o bônus).</summary>
    public void Add(int baseAmount)
    {
        Money += Mathf.RoundToInt(baseAmount * MoneyMultiplier);
        OnMoneyChanged?.Invoke(Money);
    }

    public bool TrySpend(int price)
    {
        if (price > Money) return false;
        Money -= price;
        OnMoneyChanged?.Invoke(Money);
        return true;
    }
}
