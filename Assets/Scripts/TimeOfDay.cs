using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Relógio dia/noite PROVISÓRIO. Guarda se é dia ou noite e avisa quem estiver
/// ouvindo (Irene Rosa, spawner de inimigos...). Enquanto o ciclo de verdade
/// não existe, aperte a tecla de teste (N) para alternar entre dia e noite.
/// Quando o ciclo real for feito, é só ele chamar TimeOfDay.SetNight(true/false).
/// Coloque em UM objeto vazio da cena.
/// </summary>
public class TimeOfDay : MonoBehaviour
{
    public static bool IsNight { get; private set; }
    public static event Action<bool> OnNightChanged;   // true = anoiteceu, false = amanheceu

    [SerializeField] private bool startAtNight = false;
    [SerializeField] private bool enableDebugKey = true;
    [SerializeField] private Key debugKey = Key.N;

    private void Start()
    {
        SetNight(startAtNight);
    }

    private void Update()
    {
        if (!enableDebugKey) return;

        Keyboard kb = Keyboard.current;
        if (kb != null && kb[debugKey].wasPressedThisFrame)
            SetNight(!IsNight);
    }

    public static void SetNight(bool night)
    {
        IsNight = night;
        OnNightChanged?.Invoke(night);
    }
}
