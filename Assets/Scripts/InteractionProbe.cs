using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Ferramenta TEMPORÁRIA de teste (não mexe em nenhum outro script).
/// Coloque no objeto que o Pitcher deve alcançar (ex.: a "caixa").
/// Escreve no Console quando o Pitcher chega perto / se afasta e quando o E é apertado,
/// dizendo a distância. Na Scene, desenha o círculo de alcance (amarelo = longe, verde = perto).
/// Deixe "Radius" igual ao "Delivery Distance" do DeliveryCounter. Apague depois de testar.
/// </summary>
public class InteractionProbe : MonoBehaviour
{
    [SerializeField] private float radius = 1.5f;

    private Transform player;
    private bool wasNear;

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        else Debug.LogWarning("InteractionProbe: nenhum objeto com a tag Player na cena.", this);
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool near = distance <= radius;

        if (near != wasNear)
        {
            wasNear = near;
            Debug.Log("[" + name + "] Pitcher " + (near ? "PERTO" : "LONGE") + " (distância " + distance.ToString("0.00") + ")", this);
        }

        Keyboard kb = Keyboard.current;
        if (kb != null && kb.eKey.wasPressedThisFrame)
        {
            Debug.Log("[" + name + "] E apertado. Distância " + distance.ToString("0.00") + " / alcance " + radius + " -> " + (near ? "DENTRO" : "FORA"), this);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = wasNear ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
