using UnityEngine;
using System.Collections;

public class SuperManController : MonoBehaviour {

    private Rigidbody rb;
    [SerializeField]
    private float speed;

    private Cloth cloak;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
        cloak = GetComponentInChildren<Cloth>();

    }
	
	// Update is called once per frame
	void Update () {
        rb.velocity = new Vector3(-Input.GetAxisRaw("Horizontal"), 0, -Input.GetAxisRaw("Vertical")) * Time.deltaTime * speed;
        cloak.movementForce = -rb.velocity * 0.03f;
	}
}
