using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PaperPlaneController : MonoBehaviour
{
    [Header("Movimentação")]
    public float moveForce = 100f;     // Intensidade da força lateral/vertical
    public float forwardForce = 10f;  // Força constante para frente

    private Rigidbody rb;
    private float inputZ;
    private float inputY;


    public bool forwardForceInput;



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

        if (Input.GetButton("Action") && forwardForceInput == false)
        {

            StartCoroutine(ApplyForwardForce());

        }





    }


    IEnumerator ApplyForwardForce()
    {
        forwardForceInput = true;
            rb.AddRelativeForce(Vector3.left* forwardForce, ForceMode.Impulse);
        rb.useGravity = true;

        yield return new WaitForSeconds(0.3f);
        forwardForceInput = false;

        
    }






    void FixedUpdate()
    {

        //rb.AddRelativeForce(Vector3.left* forwardForce, ForceMode.Force);
        // Força constante para frente (eixo Z local do avião)
        //  rb.AddRelativeForce(Vector3.forward * forwardForce, ForceMode.Force);

        // Movimento em Y (vertical) e X/Z (horizontal)
        Vector3 moveDir = new Vector3(0f, inputY, inputZ);
        rb.AddRelativeForce(moveDir * moveForce, ForceMode.Force);
    }
}
