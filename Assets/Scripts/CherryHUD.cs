using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD das cerejas de vida. Ligue o método UpdateCherries ao evento
/// OnHealthChanged do Health do Pitcher (na parte "Dynamic int").
/// </summary>
public class CherryHUD : MonoBehaviour
{
    [Tooltip("As imagens das cerejas, da esquerda para a direita.")]
    [SerializeField] private Image[] cherries;
    [SerializeField] private Sprite fullCherry;
    [SerializeField] private Sprite emptyCherry;

    // Ligado ao evento OnHealthChanged do Health
    public void UpdateCherries(int current, int max)
    {
        for (int i = 0; i < cherries.Length; i++)
        {
            if (cherries[i] == null) continue;

            cherries[i].enabled = i < max;
            cherries[i].sprite = i < current ? fullCherry : emptyCherry;
        }
    }
}
