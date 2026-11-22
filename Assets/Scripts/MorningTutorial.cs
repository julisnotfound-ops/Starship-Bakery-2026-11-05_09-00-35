
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MorningTutorial : MonoBehaviour
{
    [Header("Personagens")]
    [SerializeField] private CustomerNPC irene;
    [SerializeField] private DeliveryCounter deliveryCounter;

    [Header("Painel de diálogo")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;

    [Header("Retratos")]
    [SerializeField] private Sprite irenePortrait;
    [SerializeField] private Sprite pitcherPortrait;

    [Header("Botão")]
    [SerializeField] private Button continueButton;

    private bool canContinue;
    private bool foodPickedUp;
    private bool foodDelivered;
    private bool tutorialStarted;

    private void OnEnable()
    {
        if (irene != null)
            irene.Arrived += OnIreneArrived;

        if (deliveryCounter != null)
        {
            deliveryCounter.OnOrderPickedUp += OnFoodPickedUp;
            deliveryCounter.OnOrderDelivered += OnFoodDelivered;
        }
    }

    private void OnDisable()
    {
        if (irene != null)
            irene.Arrived -= OnIreneArrived;

        if (deliveryCounter != null)
        {
            deliveryCounter.OnOrderPickedUp -= OnFoodPickedUp;
            deliveryCounter.OnOrderDelivered -= OnFoodDelivered;
        }
    }

    private void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueDialogue);
    }

    private void OnIreneArrived(CustomerNPC customer)
    {
        if (customer != irene || tutorialStarted)
            return;

        tutorialStarted = true;
        StartCoroutine(RunTutorial());
    }

    private IEnumerator RunTutorial()
    {
        // APRESENTAÇÃO DA IRENE
        yield return ShowLine(
            "Irene Rosa",
            "Olá! Tudo bem? Meu nome é Irene Rosa! Vim conhecer a padaria nova. Estava bastante curiosa para saber como é aqui!",
            irenePortrait
        );

        // RESPOSTA DO PITCHER
        yield return ShowLine(
            "Pitcher",
            "Pipipipiii! Piiip! Brrrrr pi pi pi!",
            pitcherPortrait
        );

        // IRENE NÃO ENTENDE
        yield return ShowLine(
            "Irene Rosa",
            "Hã...? Desculpa, acho que não entendi nadinha do que você disse!",
            irenePortrait
        );

        yield return ShowLine(
            "Irene Rosa",
            "Bom... não importa! Eu adoraria experimentar alguma coisa daqui. Você poderia preparar um pedido para mim?",
            irenePortrait
        );

        yield return ShowLine(
            "Irene Rosa",
            "Ah, quase esqueci! Meu pedido é este aqui. Espero que seja uma das especialidades da casa!",
            irenePortrait
        );

        // ENSINAR A PEGAR A COMIDA
        foodPickedUp = false;

        ShowInstruction(
            "Tutorial",
            "Aproxime-se do balcão de comida e aperte E para pegar o pedido."
        );

        yield return new WaitUntil(() => foodPickedUp);

        // IRENE REAGE
        yield return ShowLine(
            "Irene Rosa",
            "Oba! Estou ansiosa para experimentar!",
            irenePortrait
        );

        // ENSINAR A ENTREGAR
        foodDelivered = false;

        ShowInstruction(
            "Tutorial",
            "Leve o pedido até Irene e aperte E para entregar."
        );

        yield return new WaitUntil(() => foodDelivered);

        // FINALIZAÇÃO
        yield return ShowLine(
            "Irene Rosa",
            "Muito obrigada! Estava uma delícia! Acho que vou gostar bastante desta padaria. Até mais!",
            irenePortrait
        );

        dialoguePanel.SetActive(false);

        Debug.Log("Tutorial da manhã da Irene concluído!");
    }

    private IEnumerator ShowLine(
        string speaker,
        string message,
        Sprite portrait
    )
    {
        dialoguePanel.SetActive(true);

        speakerText.text = speaker;
        dialogueText.text = message;
        portraitImage.sprite = portrait;

        continueButton.gameObject.SetActive(true);

        canContinue = false;

        yield return new WaitUntil(() => canContinue);
    }

    private void ShowInstruction(string speaker, string message)
    {
        dialoguePanel.SetActive(true);

        speakerText.text = speaker;
        dialogueText.text = message;

        portraitImage.sprite = null;

        continueButton.gameObject.SetActive(false);
    }

    private void ContinueDialogue()
    {
        canContinue = true;
    }

    private void OnFoodPickedUp(OrderItem item)
    {
        foodPickedUp = true;
    }

    private void OnFoodDelivered(CustomerNPC customer)
    {
        if (customer == irene)
            foodDelivered = true;
    }
}
