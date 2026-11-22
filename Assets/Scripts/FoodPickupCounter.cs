using UnityEngine;
using UnityEngine.InputSystem;

public class FoodPickupCounter : MonoBehaviour
{
    [Header("Comida deste balcão")]
    [SerializeField] private OrderItem foodItem;

    [Header("Sistema de entrega")]
    [SerializeField] private DeliveryCounter deliveryCounter;

    private InputAction interactAction;
    private bool playerInside;

    private void Awake()
    {
        interactAction = new InputAction(
            "RetirarComida",
            InputActionType.Button
        );

        interactAction.AddBinding("<Keyboard>/e");
        interactAction.AddBinding("<Gamepad>/buttonNorth");

        // Teste: o Collider2D com Is Trigger precisa estar NESTE mesmo objeto, senão o Pitcher nunca é detectado.
        Collider2D col = GetComponent<Collider2D>();
        if (col == null || !col.isTrigger)
            Debug.LogWarning(name + ": falta um Collider2D com 'Is Trigger' marcado neste mesmo objeto.", this);

        if (foodItem == null)
            Debug.LogWarning(name + ": o campo Food Item está vazio.", this);
    }

    private void OnEnable()
    {
        interactAction.Enable();
    }

    private void OnDisable()
    {
        interactAction.Disable();
    }

    private void OnDestroy()
    {
        interactAction.Dispose();
    }

    private bool IsPlayer(Collider2D other)
    {
        return other.CompareTag("Player") ||
               other.transform.root.CompareTag("Player");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            playerInside = true;
            Debug.Log("Pitcher entrou na área de: " + name, this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            playerInside = false;
            Debug.Log("Pitcher saiu da área de: " + name, this);
        }
    }

    private void Update()
    {
        if (!playerInside)
            return;

        if (!interactAction.WasPressedThisFrame())
            return;

        if (foodItem == null)
        {
            Debug.LogError("Esta comida não tem OrderItem configurado! (objeto: " + name + ")", this);
            return;
        }

        if (deliveryCounter != null)
            deliveryCounter.TryPickupOrder(foodItem);
        else
            Debug.LogError("Delivery Counter não está conectado! (objeto: " + name + ")", this);
    }
}
