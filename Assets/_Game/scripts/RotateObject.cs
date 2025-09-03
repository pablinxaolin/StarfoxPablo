using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RotateObject : MonoBehaviour
{
    public float rotationSpeed = 100f;
    private Rigidbody rb;

    private float pitch = 0f; // rotação no eixo X local (frente/trás)
    private float yaw = 0f;   // rotação no eixo Y global (esquerda/direita)

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        yaw += horizontalInput * rotationSpeed * Time.deltaTime;
        pitch -= verticalInput * rotationSpeed * Time.deltaTime; // negativo para inclinar corretamente

        // limitar pitch para não dar looping (opcional)
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        // cria a rotação combinada
        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0f);
        rb.MoveRotation(targetRotation);
    }
}
