using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Ciclo dia/noite AUTOMÁTICO.
/// DIA: os clientes entram um de cada vez; quando o último sai, anoitece.
///   Dia 1: Irene Rosa (tutorial) + casal Jack/Ennis + NPCs aleatórios até fechar 4.
///   Outros dias: só NPCs aleatórios.
/// NOITE: o NightSpawner faz nascer os inimigos; quando todos (e o boss) morrem, amanhece.
/// Coloque em UM objeto vazio da cena. Precisa de TimeOfDay e NightSpawner na cena.
/// (Desmarque "Enable Debug Key" do TimeOfDay se não quiser mais a tecla N.)
/// </summary>
public class DayManager : MonoBehaviour
{
    public static int DayNumber { get; private set; } = 1;

    [Header("Clientes")]
    [Tooltip("Quantos clientes por dia (a Irene conta, o casal conta como 1).")]
    [SerializeField, Min(1)] private int customersPerDay = 4;
    [Tooltip("Irene Rosa (objeto da CENA). Só aparece como cliente no dia 1.")]
    [SerializeField] private CustomerNPC tutorialCustomer;
    [Tooltip("Casal Jack Vanilla + Ennis Lemon (PREFAB). Aparece no dia 1.")]
    [SerializeField] private CustomerNPC specialCustomerPrefab;
    [Tooltip("NPCs aleatórios (PREFABS) que completam o dia.")]
    [SerializeField] private CustomerNPC[] randomCustomerPrefabs;

    [Header("Loja")]
    [SerializeField] private Transform entryPoint;     // porta de entrada
    [SerializeField] private Transform counterPoint;   // onde o cliente espera, de frente pro balcão
    [SerializeField] private Transform exitPoint;      // saída
    [SerializeField] private DeliveryCounter counter;

    [Header("Tempos")]
    [SerializeField] private float startDelay = 1.5f;
    [SerializeField] private float gapBetweenCustomers = 1f;
    [SerializeField] private float nightStartDelay = 1f;

    public UnityEvent<int> OnDayStarted;   // (número do dia) - ligue na HUD

    private void Awake() { DayNumber = 1; }

    private void OnEnable()  { NightSpawner.OnNightCleared += HandleNightCleared; }
    private void OnDisable() { NightSpawner.OnNightCleared -= HandleNightCleared; }

    private void Start()
    {
        StartCoroutine(RunDay());
    }

    private void HandleNightCleared()
    {
        DayNumber++;
        TimeOfDay.SetNight(false);
        StopAllCoroutines();
        StartCoroutine(RunDay());
    }

    private IEnumerator RunDay()
    {
        OnDayStarted?.Invoke(DayNumber);
        yield return new WaitForSeconds(startDelay);

        foreach (CustomerNPC source in BuildQueue())
        {
            // Objeto da cena (Irene) é usado direto; prefab é instanciado na porta.
            CustomerNPC customer = source.gameObject.scene.IsValid()
                ? source
                : Instantiate(source, entryPoint.position, Quaternion.identity);

            bool left = false;
            System.Action<CustomerNPC> onLeft = _ => left = true;
            customer.Left += onLeft;

            customer.Arrive(entryPoint, counterPoint, exitPoint, counter.Menu);
            yield return new WaitUntil(() => left);

            customer.Left -= onLeft;
            yield return new WaitForSeconds(gapBetweenCustomers);
        }

        yield return new WaitForSeconds(nightStartDelay);
        TimeOfDay.SetNight(true);
    }

    private List<CustomerNPC> BuildQueue()
    {
        List<CustomerNPC> queue = new List<CustomerNPC>();

        if (DayNumber == 1)
        {
            if (tutorialCustomer != null) queue.Add(tutorialCustomer);
            if (specialCustomerPrefab != null) queue.Add(specialCustomerPrefab);
        }

        if (randomCustomerPrefabs != null && randomCustomerPrefabs.Length > 0)
        {
            while (queue.Count < customersPerDay)
                queue.Add(randomCustomerPrefabs[UnityEngine.Random.Range(0, randomCustomerPrefabs.Length)]);
        }

        return queue;
    }
}
