
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class DeliveryCounter : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private OrderItem[] menu;

    [Header("Pitcher")]
    [SerializeField] private PlayerController player;
    [SerializeField] private PlayerWallet wallet;
    [SerializeField] private SkillChoicePanel skillPanel;

    [Header("Interação")]
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private float deliveryDistance = 1.5f;

    [Tooltip("Ponto de onde se mede a distância da entrega.")]
    [SerializeField] private Transform servicePoint;

    public OrderItem[] Menu => menu;

    // Eventos utilizados pelo tutorial da Irene.
    public event Action<OrderItem> OnOrderPickedUp;
    public event Action<CustomerNPC> OnOrderDelivered;

    private InputAction interactAction;
    private bool carryingOrder;
    private bool pickupJustHappened;
    private OrderItem pickedUpItem;

    private void Awake()
    {
        interactAction = new InputAction(
            "Interact",
            InputActionType.Button
        );

        interactAction.AddBinding("<Keyboard>/e");
        interactAction.AddBinding("<Gamepad>/buttonNorth");

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
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

    public void TryPickupOrder(OrderItem foodItem)
    {
        if (carryingOrder)
        {
            Debug.Log("Você já está carregando um pedido!");
            return;
        }

        if (CustomerNPC.Waiting == null)
        {
            Debug.Log("Não há cliente esperando!");
            return;
        }

        if (foodItem == null)
        {
            Debug.LogError("Comida não identificada!");
            return;
        }

        carryingOrder = true;
        pickedUpItem = foodItem;
        pickupJustHappened = true;

        Debug.Log("Você pegou: " + foodItem.displayName);

        // Avisa o tutorial que a comida foi pega.
        OnOrderPickedUp?.Invoke(foodItem);
    }

    private void Update()
    {
        if (pickupJustHappened)
        {
            pickupJustHappened = false;
            return;
        }

        CustomerNPC customer = CustomerNPC.Waiting;

        Vector2 reference = servicePoint != null
            ? (Vector2)servicePoint.position
            : (customer != null
                ? (Vector2)customer.transform.position
                : Vector2.zero);

        float distance = (customer != null && player != null)
            ? Vector2.Distance(player.transform.position, reference)
            : -1f;

        bool nearCustomer =
            customer != null &&
            player != null &&
            distance <= deliveryDistance;

        bool canDeliver = carryingOrder && nearCustomer;

        if (interactPrompt != null)
            interactPrompt.SetActive(canDeliver);

        if (!canDeliver)
        {
            if (interactAction.WasPressedThisFrame())
            {
                string motivo;

                if (!carryingOrder)
                {
                    motivo = "você não está carregando nenhuma comida.";
                }
                else if (customer == null)
                {
                    motivo = "não há cliente esperando.";
                }
                else
                {
                    motivo = "longe demais do ponto de entrega: distância "
                        + distance.ToString("0.00")
                        + " e o máximo é "
                        + deliveryDistance;
                }

                Debug.Log("[Entrega] Não entregou: " + motivo);
            }

            return;
        }

        if (interactAction.WasPressedThisFrame())
        {
            DeliverOrder(customer);
        }
    }

    private void DeliverOrder(CustomerNPC customer)
    {
        if (customer == null || !carryingOrder)
            return;

        if (pickedUpItem != customer.Order)
        {
            Debug.Log("Pedido errado! Irene continua esperando.");

            carryingOrder = false;
            pickedUpItem = null;

            return;
        }

        carryingOrder = false;
        pickedUpItem = null;

        if (wallet != null)
            wallet.Add(customer.Reward);

        customer.Serve();

        // Avisa o tutorial que o pedido foi entregue.
        OnOrderDelivered?.Invoke(customer);

        Debug.Log(
            "Pedido correto! Você ganhou "
            + customer.Reward
            + " moedas."
        );

        if (customer.GrantsSkillChoice && skillPanel != null)
            skillPanel.Show(customer.Leave);
        else
            customer.Leave();
    }
}
