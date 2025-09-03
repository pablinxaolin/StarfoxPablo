using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;               // O avião
    public Vector3 offset = new Vector3(0, 2, -6);
    public float smoothSpeed = 5f;         // Suavidade do movimento

    void LateUpdate()
    {
        if (!target) return;

        // Posição desejada (atrás do avião)
        Vector3 desiredPosition = target.position + target.rotation * offset;

        // Movimento suave até a posição desejada
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Faz a câmera olhar para o avião
        transform.LookAt(target);
    }
}
