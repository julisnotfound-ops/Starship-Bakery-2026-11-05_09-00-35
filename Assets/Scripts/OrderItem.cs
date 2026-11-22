using UnityEngine;

/// <summary>
/// Um item que o cliente pode pedir (e que o Pitcher entrega no balcão).
/// Botão direito > Create > Starship Bakery > Item de pedido.
/// </summary>
[CreateAssetMenu(fileName = "NovoItem", menuName = "Starship Bakery/Item de pedido")]
public class OrderItem : ScriptableObject
{
    public string displayName = "Item";
    public Sprite icon;   // aparece no balãozinho do cliente
}
