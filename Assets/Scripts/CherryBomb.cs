using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bomba de cereja do Lord Flan. É lançada até um ponto, mostra um círculo
/// de aviso no chão, espera o pavio e explode causando dano em área.
///
/// Prefab esperado:
///   CherryBomb (este script)
///     Visual   (sprite da cereja)
///     Aviso    (sprite de círculo vermelho transparente, 1 unidade de diâmetro)
/// </summary>
public class CherryBomb : MonoBehaviour
{
    [Header("Voo")]
    [SerializeField] private float flightTime = 0.8f;
    [Tooltip("Altura do arco do arremesso (só visual).")]
    [SerializeField] private float arcHeight = 1.5f;

    [Header("Explosão")]
    [Tooltip("Tempo parada no chão antes de explodir.")]
    [SerializeField] private float fuseTime = 0.6f;
    [SerializeField] private float radius = 2f;
    [SerializeField] private int damage = 1;

    [Header("Visual")]
    [SerializeField] private Transform visual;
    [Tooltip("Círculo de aviso. Fica no ponto de impacto, com o tamanho da explosão.")]
    [SerializeField] private Transform warningCircle;
    [Tooltip("Efeito/animação da explosão (opcional).")]
    [SerializeField] private GameObject explosionEffect;

    public void Launch(Vector2 targetPosition)
    {
        StartCoroutine(Run(transform.position, targetPosition));
    }

    private IEnumerator Run(Vector2 start, Vector2 target)
    {
        // O círculo de aviso se solta da bomba e fica parado no ponto de impacto.
        if (warningCircle != null)
        {
            warningCircle.SetParent(null, true);
            warningCircle.position = target;
            warningCircle.localScale = Vector3.one * radius * 2f;
            warningCircle.gameObject.SetActive(true);
        }

        // Voo em arco.
        float elapsed = 0f;
        while (elapsed < flightTime)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / flightTime);
            transform.position = Vector2.Lerp(start, target, k);
            if (visual != null)
                visual.localPosition = new Vector3(0f, Mathf.Sin(k * Mathf.PI) * arcHeight, 0f);
            yield return null;
        }

        transform.position = target;
        if (visual != null) visual.localPosition = Vector3.zero;

        // Pavio.
        yield return new WaitForSeconds(fuseTime);

        Explode(target);
    }

    private void Explode(Vector2 center)
    {
        HashSet<Health> alreadyHit = new HashSet<Health>();

        foreach (Collider2D hit in Physics2D.OverlapCircleAll(center, radius))
        {
            Health health = hit.GetComponentInParent<Health>();
            if (health == null || !health.CompareTag("Player")) continue;
            if (!alreadyHit.Add(health)) continue;   // uma vez só por explosão

            health.TakeDamage(damage);
        }

        if (explosionEffect != null)
            Instantiate(explosionEffect, center, Quaternion.identity);

        if (warningCircle != null) Destroy(warningCircle.gameObject);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
