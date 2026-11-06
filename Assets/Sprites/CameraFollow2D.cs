using UnityEngine;

/// <summary>
/// Câmera top-down 2D que segue um alvo suavemente.
/// Versão simples para o protótipo; dá pra trocar por Cinemachine depois.
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.12f;
    [SerializeField] private Vector2 offset = Vector2.zero;

    private Vector3 velocity;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z);   // mantém o Z da câmera (-10)

        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }
}
