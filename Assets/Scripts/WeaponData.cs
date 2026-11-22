using UnityEngine;

/// <summary>
/// Configuração de UM tipo de disparo (usado pelo ataque normal e pelo especial).
/// </summary>
[System.Serializable]
public class FireMode
{
    [Min(0)] public int damage = 1;
    [Tooltip("Segundos entre um disparo e o próximo.")]
    [Min(0.05f)] public float cooldown = 0.35f;
    public float bulletSpeed = 12f;

    [Header("Vários projéteis (espingarda, leque...)")]
    [Min(1)] public int projectiles = 1;
    [Tooltip("Abertura total do leque, em graus. Só faz efeito com 2 ou mais projéteis.")]
    [Range(0f, 180f)] public float spreadAngle = 0f;

    [Header("Efeitos")]
    [Tooltip("Segundos que o inimigo fica parado ao ser acertado. 0 = nenhum. O inimigo precisa do componente Stunnable.")]
    [Min(0f)] public float stunDuration = 0f;

    [Header("Bumerangue")]
    [Tooltip("A bala vai até a distância máxima, volta pro Pitcher e atravessa inimigos.")]
    public bool boomerang = false;
    [Min(1f)] public float boomerangRange = 5f;

    [Header("Visual (opcional)")]
    [Tooltip("Deixe vazio pra usar a bala padrão do PlayerShooting.")]
    public Bullet bulletPrefab;
    public float bulletScale = 1f;
    public bool tintBullet = false;
    public Color bulletColor = Color.white;
}

/// <summary>
/// Dados de uma arma. Cada arma é um arquivo no projeto:
/// botão direito > Create > Starship Bakery > Arma.
/// Dá pra ajustar números e trocar o visual sem mexer em código.
/// </summary>
[CreateAssetMenu(fileName = "NovaArma", menuName = "Starship Bakery/Arma")]
public class WeaponData : ScriptableObject
{
    [Header("Informações")]
    public string displayName = "Arma";
    public Sprite icon;                 // ícone pra loja e inventário
    [TextArea] public string description;
    [Min(0)] public int price = 0;      // preço na loja da Queijoca

    [Header("Animação do Pitcher")]
    [Tooltip("Animator Override Controller com as animações do Pitcher segurando esta arma. Vazio = animação padrão do Pitcher.")]
    public RuntimeAnimatorController pitcherAnimator;

    [Header("Ataque normal (botão esquerdo / X no controle)")]
    public FireMode normal = new FireMode();

    [Header("Ataque especial (botão direito / A no controle)")]
    public bool hasSpecial = true;
    public FireMode special = new FireMode
    {
        damage = 1,
        cooldown = 4f,
        projectiles = 5,
        spreadAngle = 50f
    };
}
