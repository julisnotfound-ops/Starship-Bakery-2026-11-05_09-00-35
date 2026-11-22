using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Tiro do Pitcher com armas (WeaponData) e mira automática.
/// Ataque normal: botão esquerdo do mouse / X no controle (segurar atira em sequência).
/// Ataque especial: botão direito do mouse / A no controle.
/// Trocar de arma: UM botão só (Q no teclado / RB no controle), passa pra próxima arma.
/// A bala vai pro inimigo mais próximo dentro de um cone na direção em que o Pitcher olha.
/// Sem inimigo no cone, vai reto pra frente.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerShooting : MonoBehaviour
{
    [Header("Armas")]
    [Tooltip("Armas que o Pitcher tem. Se ficar vazio, usa um tiro simples padrão.")]
    [SerializeField] private List<WeaponData> weapons = new List<WeaponData>();

    [Header("Tiro")]
    [Tooltip("Bala usada quando a arma não define uma própria.")]
    [SerializeField] private Bullet bulletPrefab;
    [Tooltip("Distância extra à frente da arma onde a bala nasce. Com a posição da arma ajustada, 0.1 a 0.3 costuma bastar.")]
    [SerializeField] private float spawnOffset = 0.6f;

    [Header("Posição da arma (de onde sai a bala)")]
    [Tooltip("Deslocamento a partir do pivô do Pitcher até a ponta da arma, por direção em que ele olha. Selecione o Pitcher e veja as bolinhas ciano na Scene.")]
    [SerializeField] private Vector2 muzzleDown = new Vector2(0f, 0.4f);
    [SerializeField] private Vector2 muzzleUp = new Vector2(0f, 0.4f);
    [SerializeField] private Vector2 muzzleLeft = new Vector2(-0.3f, 0.4f);
    [SerializeField] private Vector2 muzzleRight = new Vector2(0.3f, 0.4f);

    [Header("Mira automática")]
    [SerializeField] private float autoAimRange = 8f;
    [Tooltip("Abertura total do cone, em graus. 90 = 45 pra cada lado.")]
    [SerializeField, Range(10f, 180f)] private float autoAimConeAngle = 90f;

    /// <summary>Ligado enquanto um menu está aberto (balcão, escolha de habilidade): não atira nem troca de arma.</summary>
    public static bool InputBlocked { get; set; }

    /// <summary>Falso = Pitcher sem arma na mão (de dia): não atira nem troca de arma. O PlayerWeaponAnimator controla isso.</summary>
    public bool CanShoot { get; set; } = true;

    /// <summary>Multiplicador de dano (habilidade do casal: 1.5). O dano é inteiro, então arredonda pra cima (1 vira 2).</summary>
    public float DamageMultiplier { get; set; } = 1f;

    private PlayerController player;
    private PlayerAnimator playerAnimator;   // opcional
    private InputAction normalAction;
    private InputAction specialAction;
    private readonly FireMode fallbackMode = new FireMode();

    private int currentIndex;
    private float normalCooldownLeft;
    private float specialCooldownLeft;

    /// <summary>Arma equipada agora (null se a lista estiver vazia).</summary>
    public WeaponData CurrentWeapon => weapons.Count > 0 ? weapons[currentIndex] : null;

    /// <summary>Avisa quando a arma muda (a HUD vai usar isso).</summary>
    public event System.Action<WeaponData> OnWeaponChanged;

    /// <summary>Avisa a cada disparo (o PlayerWeaponAnimator usa isso pra mostrar a arma na mão).</summary>
    public event System.Action OnFired;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        playerAnimator = GetComponent<PlayerAnimator>();

        normalAction = new InputAction("Attack", InputActionType.Button);
        normalAction.AddBinding("<Mouse>/leftButton");
        normalAction.AddBinding("<Gamepad>/buttonWest");

        specialAction = new InputAction("SpecialAttack", InputActionType.Button);
        specialAction.AddBinding("<Mouse>/rightButton");
        specialAction.AddBinding("<Gamepad>/buttonSouth");
    }

    private void OnEnable()
    {
        normalAction.Enable();
        specialAction.Enable();
    }

    private void OnDisable()
    {
        normalAction.Disable();
        specialAction.Disable();
    }

    private void Start()
    {
        OnWeaponChanged?.Invoke(CurrentWeapon);
    }

    // ---------------------------------------------------------------- Loja / inventário

    /// <summary>A loja da Queijoca chama isso quando o jogador compra uma arma.</summary>
    public void AddWeapon(WeaponData weapon, bool equip = true)
    {
        if (weapon == null || weapons.Contains(weapon)) return;
        weapons.Add(weapon);
        if (equip) Equip(weapons.Count - 1);
    }

    public void Equip(int index)
    {
        if (weapons.Count == 0) return;
        currentIndex = ((index % weapons.Count) + weapons.Count) % weapons.Count;
        OnWeaponChanged?.Invoke(CurrentWeapon);
    }

    // ---------------------------------------------------------------- Loop principal

    private void Update()
    {
        if (InputBlocked || !CanShoot) return;

        HandleWeaponSwitch();

        if (normalCooldownLeft > 0f) normalCooldownLeft -= Time.deltaTime;
        if (specialCooldownLeft > 0f) specialCooldownLeft -= Time.deltaTime;

        // Ataque normal (segurar atira em sequência)
        if (normalAction.IsPressed() && normalCooldownLeft <= 0f)
        {
            FireMode mode = CurrentWeapon != null ? CurrentWeapon.normal : fallbackMode;
            if (Fire(mode)) normalCooldownLeft = mode.cooldown;
        }

        // Ataque especial (um clique por vez)
        WeaponData weapon = CurrentWeapon;
        if (weapon != null && weapon.hasSpecial
            && specialAction.WasPressedThisFrame() && specialCooldownLeft <= 0f)
        {
            if (Fire(weapon.special)) specialCooldownLeft = weapon.special.cooldown;
        }
    }

    private void HandleWeaponSwitch()
    {
        Keyboard kb = Keyboard.current;
        Gamepad pad = Gamepad.current;

        bool pressed = (kb != null && kb.qKey.wasPressedThisFrame)
                    || (pad != null && pad.rightShoulder.wasPressedThisFrame);

        if (pressed) Equip(currentIndex + 1);   // volta pra primeira depois da última
    }

    // ---------------------------------------------------------------- Disparo

    /// <returns>true se atirou.</returns>
    private bool Fire(FireMode mode)
    {
        Bullet prefab = mode.bulletPrefab != null ? mode.bulletPrefab : bulletPrefab;
        if (prefab == null)
        {
            Debug.LogWarning("PlayerShooting: falta arrastar o prefab da bala no Inspector.", this);
            return false;
        }

        Vector2 bodyPos = transform.position;
        Vector2 origin = GetMuzzlePosition();   // a bala sai da altura da arma
        Vector2 baseDirection = player.FacingDirection;
        if (TryFindTarget(bodyPos, baseDirection, out Vector2 targetPos))
        {
            Vector2 toTarget = targetPos - origin;
            if (toTarget.sqrMagnitude > 0.0001f) baseDirection = toTarget.normalized;
        }

        int damage = Mathf.Max(1, Mathf.CeilToInt(mode.damage * DamageMultiplier - 0.001f));
        int count = Mathf.Max(1, mode.projectiles);
        float startAngle = -mode.spreadAngle * 0.5f;
        float step = count > 1 ? mode.spreadAngle / (count - 1) : 0f;

        for (int i = 0; i < count; i++)
        {
            float angle = count > 1 ? startAngle + step * i : 0f;
            Vector2 direction = (Vector2)(Quaternion.Euler(0f, 0f, angle) * (Vector3)baseDirection);

            Bullet bullet = Instantiate(prefab, origin + direction * spawnOffset, Quaternion.identity);
            bullet.Init(direction, mode.bulletSpeed, damage, "Player");
            bullet.StunDuration = mode.stunDuration;
            if (mode.boomerang) bullet.SetBoomerang(transform, mode.boomerangRange);
            ApplyBulletLook(bullet, mode);
        }

        if (playerAnimator != null) playerAnimator.ResetIdleTimer();
        OnFired?.Invoke();
        return true;
    }

    /// <summary>Ponta da arma no mundo, conforme a direção em que o Pitcher olha (4 direções, como na sprite sheet).</summary>
    private Vector2 GetMuzzlePosition()
    {
        Vector2 f = player.FacingDirection;
        Vector2 offset;
        if (Mathf.Abs(f.x) > Mathf.Abs(f.y)) offset = f.x > 0f ? muzzleRight : muzzleLeft;
        else offset = f.y > 0f ? muzzleUp : muzzleDown;
        return (Vector2)transform.position + offset;
    }

    private static void ApplyBulletLook(Bullet bullet, FireMode mode)
    {
        if (!Mathf.Approximately(mode.bulletScale, 1f))
            bullet.transform.localScale *= mode.bulletScale;

        if (mode.tintBullet)
        {
            SpriteRenderer sr = bullet.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = mode.bulletColor;
        }
    }

    /// <summary>
    /// Inimigo = qualquer coisa com Health que não tenha a tag Player.
    /// (Quando os inimigos de verdade existirem, dá pra trocar por um componente Enemy.)
    /// </summary>
    private bool TryFindTarget(Vector2 origin, Vector2 facing, out Vector2 targetPos)
    {
        targetPos = default;
        float bestSqrDist = float.MaxValue;
        bool found = false;

        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, autoAimRange);
        foreach (Collider2D hit in hits)
        {
            if (hit.isTrigger || hit.CompareTag("Player")) continue;

            Health health = hit.GetComponentInParent<Health>();
            if (health == null || health.IsDead) continue;

            Vector2 center = hit.bounds.center;
            Vector2 toTarget = center - origin;

            if (Vector2.Angle(facing, toTarget) > autoAimConeAngle * 0.5f) continue;

            float sqrDist = toTarget.sqrMagnitude;
            if (sqrDist < bestSqrDist)
            {
                bestSqrDist = sqrDist;
                targetPos = center;
                found = true;
            }
        }
        return found;
    }

    // Mostra o alcance e o cone na janela Scene quando o Pitcher está selecionado.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 facing = Application.isPlaying && player != null ? player.FacingDirection : Vector2.down;
        float half = autoAimConeAngle * 0.5f;

        Vector3 left = Quaternion.Euler(0, 0, half) * facing * autoAimRange;
        Vector3 right = Quaternion.Euler(0, 0, -half) * facing * autoAimRange;
        Gizmos.DrawLine(transform.position, transform.position + left);
        Gizmos.DrawLine(transform.position, transform.position + right);
        Gizmos.DrawWireSphere(transform.position, autoAimRange);

        // Pontos de saída da bala (um por direção).
        Gizmos.color = Color.cyan;
        Vector3 p = transform.position;
        Gizmos.DrawSphere(p + (Vector3)muzzleDown, 0.06f);
        Gizmos.DrawSphere(p + (Vector3)muzzleUp, 0.06f);
        Gizmos.DrawSphere(p + (Vector3)muzzleLeft, 0.06f);
        Gizmos.DrawSphere(p + (Vector3)muzzleRight, 0.06f);
    }
}
