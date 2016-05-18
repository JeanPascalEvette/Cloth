using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    private Rigidbody rb;

    [SerializeField]
    private float moveSpeed = 1.0f;
    [SerializeField]
    private float depthSpeed = 3.0f;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {

        float depth = 0.0f;
        if (Input.GetKey(KeyCode.Q))
            depth = -1.0f;
        else if (Input.GetKey(KeyCode.E))
            depth = 1.0f;

            rb.velocity = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical"),
            depth * depthSpeed / moveSpeed) * Time.deltaTime * moveSpeed;
	}
}
