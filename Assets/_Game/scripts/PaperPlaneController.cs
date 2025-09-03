using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PaperPlaneController : MonoBehaviour
{
    [Header("Movimentação")]
    public float moveForce = 100f;     // Intensidade da força lateral/vertical
    public float forwardForce = 20f;  // Força constante para frente

    private Rigidbody rb;
    private float inputZ;
    private float inputY;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Deixar um pouco mais realista (opcional)
        rb.useGravity = false;
        rb.drag = 0.5f;        // resistência do ar
        rb.angularDrag = 2f;   // resistência à rotação
    }

    void Update()
    {
        // WASD / Setas
        inputZ = Input.GetAxis("Horizontal"); // A/D ou ←/→
        inputY = Input.GetAxis("Vertical");   // W/S ou ↑/↓
    }

    void FixedUpdate()
    {
        // Força constante para frente (eixo Z local do avião)
      //  rb.AddRelativeForce(Vector3.forward * forwardForce, ForceMode.Force);

        // Movimento em Y (vertical) e X/Z (horizontal)
        Vector3 moveDir = new Vector3(0f, inputY, inputZ);
        rb.AddRelativeForce(moveDir * moveForce, ForceMode.Force);
    }
}
