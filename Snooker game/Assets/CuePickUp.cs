using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
public class CuePickUp : MonoBehaviour
{
    [SerializeField] private ActionBasedController controller; // controller
    public Hand rightHand;
    public Hand leftHand;
    public GameObject table;
    private Rigidbody rb;
    private Transform rbTransform;
    private Vector3 leftPrePos;
    private HingeJoint hingeJoint;
    bool press = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>(); // rb of the ball
        rbTransform = GetComponent<Transform>(); // transform of the ball
        hingeJoint = GetComponent<HingeJoint>(); // hingeJoint connected the table and the cue

        controller.selectAction.action.started += cueStick;
    }

    // distance: left hand moves back and forth 
    float leftForward()
    {
        float distance = leftHand.transform.localPosition.x - leftPrePos.x;
        leftPrePos = leftHand.transform.localPosition;
        return distance;
    }

    private void cueStick(InputAction.CallbackContext context) 
    {
        Debug.Log("leftpress!!");
        float leftForwardDistance = leftForward();

        press = true;

        // check cue pick up
        if(rightHand.heldObject!=null && rightHand.heldObject.CompareTag("Cue"))
        {
            // check cue put on the table
            if(rightHand.hinge == true)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                RigidbodyConstraints currentConstraints = rb.constraints;

                if(hingeJoint == null)
                {
                    // add hingeJoint
                    hingeJoint = gameObject.AddComponent<HingeJoint>();

                    hingeJoint.connectedBody = table.GetComponent<Rigidbody>();
                    hingeJoint.anchor = new Vector3(0, 0, 0); // Set the anchor point
                    hingeJoint.axis = new Vector3(0, 1, 0);
                }
            }
        }
    }

    void FixedUpdate()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        float leftForwardDistance = leftForward();

        if(press)
        {
            if(leftForwardDistance>0.02 || leftForwardDistance<-0.02) rbTransform.position  = rbTransform.position + rbTransform.up * leftForwardDistance;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // // check cue pick up
        // if(rightHand.heldObject!=null && rightHand.heldObject.CompareTag("Cue"))
        // {
        //     // check cue put on the table
        //     if(rightHand.hinge == true)
        //     {
        //         // add hingeJoint
        //         HingeJoint hingeJoint = gameObject.AddComponent<HingeJoint>();

        //         hingeJoint.connectedBody = GetComponent<Rigidbody>();
        //         hingeJoint.anchor = new Vector3(0, 0, 0); // Set the anchor point
        //         hingeJoint.axis = new Vector3(0, 1, 0);
        //     }
        // }

        // rb.velocity = Vector3.zero;
        // rb.angularVelocity = Vector3.zero;

        // if(leftForwardDistance>0.02 || leftForwardDistance<-0.02) rbTransform.position  = rbTransform.position + rbTransform.up * leftForwardDistance;

        // // check cue put on the table
        // if(rightHand.hinge == true)
        // {
        //     if(hingeJoint == null)
        //     {
        //         // add hingeJoint
        //         hingeJoint = gameObject.AddComponent<HingeJoint>();

        //         hingeJoint.connectedBody = table.GetComponent<Rigidbody>();
        //         hingeJoint.anchor = rightHand.hingeJointPos;
        //         hingeJoint.axis = new Vector3(0, 1, 0);

        //         hingeJoint.breakForce = 500;
        //         hingeJoint.breakTorque = 500;
        //     }
        // }
    }
}
