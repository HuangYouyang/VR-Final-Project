using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    Rigidbody rb;
    float velThreshold = 0.02f;

    public bool isRed = true;
    public bool is8Ball = false;
    public bool isCueBall = false;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void FixedUpdate()
    {
        if (rb.velocity.y > 0)
        {
            Vector3 newVelocity = rb.velocity;
            newVelocity.y = 0.0f;
            rb.velocity = newVelocity;
        }
        if (rb.velocity.magnitude < velThreshold)
        {
            rb.velocity = Vector3.zero;
        }
    }
    public bool IsBallRed()
    {
        return isRed;
    }
    public bool IsCueBall()
    {
        return isCueBall;
    }
    public bool IsEightBall()
    {
        return is8Ball;
    }
}
