using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Tela de escolha da habilidade do casal Jack/Ennis (1 entre 3).
/// Coloque este script num objeto que fica SEMPRE ativo (ex.: o Canvas) e
/// arraste o painel (que começa desligado) em "Panel".
/// Enquanto aberto, o jogo pausa e o Pitcher não atira.
/// Também dá pra escolher com as teclas 1, 2 e 3.
/// </summary>
public class SkillChoicePanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button speedButton;    // andar mais rápido
    [SerializeField] private Button damageButton;   // mais dano
    [SerializeField] private Button moneyButton;    // mais dinheiro

    [Header("Pitcher")]
    [SerializeField] private PlayerController player;
    [SerializeField] private PlayerShooting shooting;
    [SerializeField] private PlayerWallet wallet;

    [Header("Valores")]
    [SerializeField] private float speedMultiplier = 1.5f;
    [SerializeField] private float damageMultiplier = 1.5f;
    [Tooltip("0.25 = ganha 25% a mais de dinheiro em cada cliente.")]
    [SerializeField] private float moneyBonus = 0.25f;

    private Action onClosed;

    private void Awake()
    {
        if (panel != null) panel.SetActive(false);
        if (speedButton != null) speedButton.onClick.AddListener(ChooseSpeed);
        if (damageButton != null) damageButton.onClick.AddListener(ChooseDamage);
        if (moneyButton != null) moneyButton.onClick.AddListener(ChooseMoney);
    }

    public void Show(Action whenClosed)
    {
        onClosed = whenClosed;
        panel.SetActive(true);
        Time.timeScale = 0f;
        PlayerShooting.InputBlocked = true;
    }

    private void Update()
    {
        if (panel == null || !panel.activeSelf) return;

        Keyboard kb = Keyboard.current;
        if (kb == null) return;
        if (kb.digit1Key.wasPressedThisFrame) ChooseSpeed();
        else if (kb.digit2Key.wasPressedThisFrame) ChooseDamage();
        else if (kb.digit3Key.wasPressedThisFrame) ChooseMoney();
    }

    private void ChooseSpeed()
    {
        if (player != null) player.PermanentSpeedMultiplier *= speedMultiplier;
        Close();
    }

    private void ChooseDamage()
    {
        if (shooting != null) shooting.DamageMultiplier *= damageMultiplier;
        Close();
    }

    private void ChooseMoney()
    {
        if (wallet != null) wallet.MoneyMultiplier += moneyBonus;
        Close();
    }

    private void Close()
    {
        panel.SetActive(false);
        Time.timeScale = 1f;
        PlayerShooting.InputBlocked = false;

        Action callback = onClosed;
        onClosed = null;
        callback?.Invoke();
    }
}
