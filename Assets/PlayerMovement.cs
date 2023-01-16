using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController cc;
    public float speed;
    public float rotateSpeed;

    float horiz;
    float vert;
// Start is called before the first frame update
void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        horiz = Input.GetAxisRaw("Horizontal");
        vert = Input.GetAxisRaw("Vertical");
    }
    private void FixedUpdate()
    {
        Vector3 move = new Vector3(horiz, 0, vert);
        cc.SimpleMove(move* speed);
    }
}
