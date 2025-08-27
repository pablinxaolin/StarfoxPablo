using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 2.0f, -6.5f);
    [SerializeField] private float positionSmoothTime = 0.12f;
    [SerializeField] private float rotationLerpSpeed = 8f;
    [SerializeField] private float lookAhead = 30f;

    private Vector3 velocity;

    void LateUpdate()
    {
        if (!target) return;

        // Posição desejada no espaço da nave (fica “atrás e acima”)
        Vector3 desiredPos = target.TransformPoint(localOffset);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, positionSmoothTime);

        // Olha para uma posição à frente da nave (evita olhar direto no pivot)
        Vector3 lookTarget = target.position + target.forward * lookAhead;
        Quaternion desiredRot = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotationLerpSpeed * Time.deltaTime);
    }
}
