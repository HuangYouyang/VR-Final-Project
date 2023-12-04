using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;
public class CuePickUp : MonoBehaviour
{
    [SerializeField] private ActionBasedController controller; // controller
    [SerializeField] public ActionBasedController rightHandcontroller; 
    public GameObject rightHand;
    public GameObject leftHand;
    public GameObject table;
    private Rigidbody rb;
    private Transform rbTransform;
    private Vector3 leftPrePos;

    public bool press = false;
    private bool checkHit = false;
    public GameObject cueHead;
    public GameObject[] Balls;

    public GameObject circleProjector;

    // lock cue
    public  Vector3 lockPos;
    public Quaternion lockRot;
    private float leftForwardDistance;
    private XRController xrController;

    // audio
    private AudioSource cueHitBallAudio;

    // game manager
    GameManager gameManager;

    // hit
    private float previousTime;

    // projection
    [SerializeField] private Transform[] points;
    [SerializeField] private LineController line;
    private LineRenderer myLineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rbTransform = GetComponent<Transform>(); 

        controller.selectAction.action.started += cueStick;
        controller.selectAction.action.canceled += Release;

        leftForwardDistance = 0;

        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();

        myLineRenderer = GetComponent<LineRenderer>();
    }
    
    // distance: left hand moves back and forth 
    float leftForward()
    {
        Vector3 displacement = controller.GetComponent<Transform>().position - leftPrePos;
        float distanceMovedForward = Vector3.Dot(displacement, controller.transform.forward);
        leftPrePos = controller.GetComponent<Transform>().position;

        return distanceMovedForward;
    }


    private void cueStick(InputAction.CallbackContext context) 
    {
        leftForwardDistance = 0;

        press = true;

        // lock pos & rot
        lockPos = gameObject.GetComponent<Transform>().localPosition;
        lockRot = gameObject.GetComponent<Transform>().localRotation;

        XRGrabInteractable grabInteractable = gameObject.GetComponent<XRGrabInteractable>();
        grabInteractable.movementType = XRBaseInteractable.MovementType.VelocityTracking;

        rightHandcontroller.enableInputTracking = false;

        var ray = new Ray(cueHead.GetComponent<Transform>().position, cueHead.GetComponent<Transform>().up);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, 0.12f))
        {   
            float ballCueDis = Vector3.Distance(hit.transform.position, cueHead.GetComponent<Transform>().position);
            Debug.Log(ballCueDis);
            if(hit.transform.gameObject.CompareTag("ball") && ballCueDis<=0.12f)
            {
                gameObject.layer = LayerMask.NameToLayer("Non-Physics collision");
                // lock pos & rot
                lockPos = gameObject.GetComponent<Transform>().localPosition - rbTransform.up * (0.12f-ballCueDis);
                lockRot = gameObject.GetComponent<Transform>().localRotation;
            }
        }
    }

    void FixedUpdate()
    {
        float deltaTime = Time.time - previousTime;
        previousTime = Time.time;
        float disLeftForward = leftForward();
        leftForwardDistance = leftForwardDistance + disLeftForward;
        float speed = Mathf.Abs(disLeftForward) / previousTime;

        if(press && !checkHit)
        {
            // freeze cue & hands
            rb.constraints = RigidbodyConstraints.FreezeAll;

            // lock cue
            gameObject.GetComponent<Transform>().localPosition = lockPos + rbTransform.up * leftForwardDistance;  
            gameObject.GetComponent<Transform>().localRotation = lockRot;
        
            // ray
            var ray = new Ray(cueHead.GetComponent<Transform>().position, cueHead.GetComponent<Transform>().up);
            RaycastHit hit;

            // show circle on the ball if hit
            if(Physics.Raycast(ray, out hit, 0.12f))
            {   
                if(hit.transform.gameObject.CompareTag("ball"))
                {
                    myLineRenderer.enabled = true;
                    points[0] = cueHead.GetComponent<Transform>();
                    points[1] = hit.transform;
                    // GameObject newObject = new GameObject("MyObjectWithoutTransform");
                    // newObject.transform.position = hit.transform.position + cueHead.GetComponent<Transform>().up * 2;
                    // points[1] = newObject.transform;

                    line.SetUpLine(points);
                    // Destroy(newObject.GetComponent<Transform>());

                    float ballCueDis = Vector3.Distance(hit.transform.position, cueHead.GetComponent<Transform>().position);
                    float circleSize = Mathf.Lerp(0.02f, 0.04f, ballCueDis);
                    DecalProjector decalProjector = circleProjector.GetComponent<DecalProjector>();
                    decalProjector.size = new Vector3(circleSize, circleSize, circleSize);
                    circleProjector.SetActive(true);
                    circleProjector.GetComponent<Transform>().position = hit.point;

                    // GameObject ball = hit.transform.gameObject;
                    // _projection.SimulateTrajectory(ball, ball.GetComponent<Transform>().position, speed, hit.normal, hit.point);
                }
            }

            Debug.DrawRay(cueHead.GetComponent<Transform>().position, cueHead.GetComponent<Transform>().up, Color.green);

            if(Physics.Raycast(ray, out hit, 0.05f))
            {   
                if(hit.transform.gameObject.CompareTag("ball"))
                {
                    GameObject ball = hit.transform.gameObject;
                    if((Mathf.Abs(speed) * 1000000/1.5f)>1)
                    {
                        cueHitBallAudio = gameObject.GetComponent<AudioSource>(); // audio
                        cueHitBallAudio.time = 2f; // start playing from 1s 
                        cueHitBallAudio.Play();
                        Invoke("StopAudio", 1f); // duration: 2s
                        Debug.Log("hit");
                        Debug.Log(-hit.normal * Mathf.Abs(speed) * 1000000/1.5f);
                        Debug.Log(ball);

                        // ball.GetComponentInChildren<Rigidbody>().AddForceAtPosition(-hit.normal * Mathf.Abs(speed) * 1000000/1.5f, hit.point); // give force
                        // ball.GetComponentInChildren<Rigidbody>().AddForceAtPosition(-hit.normal * Mathf.Abs(speed) * 1000000/1.5f, ball.transform.position); // give force
                        ball.GetComponentInChildren<Rigidbody>().AddForceAtPosition(ray.direction * Mathf.Abs(speed) * 1000000/1.5f, hit.point); // give force
                        gameManager.IsShot();
                        checkHit = true;
                    }
                }
            }
        }
    }

    void StopAudio()
    {
        cueHitBallAudio.Stop();
    }

    void onCollisionEnter(Collision col)
    {
        Rigidbody rb = col.gameObject.GetComponent<Rigidbody>(); 
        Debug.Log("collision!!");
        Vector3 forceDirection = (col.contacts[0].point - cueHead.GetComponent<Transform>().position).normalized;
        rb.AddForce(forceDirection * rb.GetComponent<Rigidbody>().velocity.magnitude);
    }

    private void Release(InputAction.CallbackContext context)
    {   
        // gameObject.layer = LayerMask.NameToLayer("Grabbable");

        press = false;

        circleProjector.SetActive(false); // disable circle

        rb.constraints = RigidbodyConstraints.None;

        rightHandcontroller.enableInputTracking = true;

        checkHit = false;

        myLineRenderer.enabled = false;
    }
}
