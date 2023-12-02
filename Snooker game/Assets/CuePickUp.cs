using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;
public class CuePickUp : MonoBehaviour
{
    [SerializeField] private ActionBasedController controller; // controller
    [SerializeField] private ActionBasedController rightHandcontroller; 
    public Hand rightHand;
    public Hand leftHand;
    public GameObject table;
    private Rigidbody rb;
    private Transform rbTransform;
    private Vector3 leftPrePos;
    private HingeJoint hingeJoint;
    public bool press = false;
    public GameObject cueHead;
    public GameObject[] Balls;

    public GameObject circleProjector;

    // lock cue
    public GameObject player;
    public  Vector3 lockPos;
    public Quaternion lockRot;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>(); // rb of the ball
        rbTransform = GetComponent<Transform>(); // transform of the ball
        hingeJoint = GetComponent<HingeJoint>(); // hingeJoint connected the table and the cue

        controller.selectAction.action.started += cueStick;
        controller.selectAction.action.canceled += Release;
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

        float downAngle = 10f;

        // lock pos
        lockPos = gameObject.GetComponent<Transform>().localPosition;
        lockRot = gameObject.GetComponent<Transform>().localRotation;
    }

    void FixedUpdate()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        float leftForwardDistance = leftForward();

        // LayerMask layer;

        if(press)
        {
            // lock cue
            gameObject.GetComponent<Transform>().localPosition = lockPos;
            gameObject.GetComponent<Transform>().localRotation = lockRot;

            // ray
            var ray = new Ray(cueHead.GetComponent<Transform>().position, cueHead.GetComponent<Transform>().up);
            RaycastHit hit;

            // show circle on the ball if hit
            if(Physics.Raycast(ray, out hit, 3))
            {   
                if(hit.transform.gameObject.CompareTag("ball"))
                {
                    circleProjector.SetActive(true);
                    circleProjector.GetComponent<Transform>().position = hit.point;
                }
            }

            rightHand.GetComponent<Transform>().parent = gameObject.GetComponent<Transform>();

            if(leftForwardDistance>0.02 || leftForwardDistance<-0.02) {

                rbTransform.position  = rbTransform.position + rbTransform.up * leftForwardDistance;

                if(Physics.Raycast(ray, out hit, 3))
                {   
                    Debug.DrawRay(cueHead.GetComponent<Transform>().position, cueHead.GetComponent<Transform>().up, Color.green);

                    if(hit.transform.gameObject.CompareTag("ball"))
                    {
                        GameObject ball = hit.transform.gameObject;
                        ball.GetComponent<Rigidbody>().AddForceAtPosition(-1 * (hit.normal) * 50, hit.point); // give force
                    }
                }
            }
        }
    }

    private void Release(InputAction.CallbackContext context)
    {   
        press = false;

        rightHand.GetComponent<Transform>().parent = player.GetComponent<Transform>();

        circleProjector.SetActive(false); // disable circle
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
