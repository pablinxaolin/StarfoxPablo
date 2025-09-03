using UnityEngine;
using System.Collections;


[RequireComponent(typeof(Rigidbody))]
public class Fly : MonoBehaviour
{
    [Header("Movimentação")]

    public float forwardForce = 100f;  // Força constante para frente

    private Rigidbody rb;
    public bool forwardForceInput;



    void Start()
    {
        rb = GetComponent<Rigidbody>();


    }


  



    void Update()
    {
 




        if (Input.GetButton("Action")&&forwardForceInput==false)
        {

          
          StartCoroutine(ApplyForwardForce2());

        }
            }






    IEnumerator ApplyForwardForce2()
    {
        forwardForceInput = true;
        rb.AddRelativeForce(Vector3.left * forwardForce, ForceMode.Force);
        rb.useGravity = true;

        yield return new WaitForSeconds(0.3f);
        forwardForceInput = false;


    }






}
