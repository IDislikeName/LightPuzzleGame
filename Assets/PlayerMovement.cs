using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;
    public float speed;

    float horiz;
    float vert;
// Start is called before the first frame update
void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        horiz = Input.GetAxisRaw("Horizontal");
        vert = Input.GetAxisRaw("Vertical");
        transform.rotation = Quaternion.LookRotation(Vector3.forward, transform.up);
    }
    private void FixedUpdate()
    {
        rb.velocity = new Vector3(horiz*speed,rb.velocity.y,vert*speed);
    }
}
