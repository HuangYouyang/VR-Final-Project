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

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rbTransform = GetComponent<Transform>(); 

        controller.selectAction.action.started += cueStick;
        controller.selectAction.action.canceled += Release;

        leftForwardDistance = 0;

        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
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

        if(Physics.Raycast(ray, out hit, 0.1f))
        {   
            float ballCueDis = Vector3.Distance(hit.transform.position, cueHead.GetComponent<Transform>().position);
            Debug.Log(ballCueDis);
            if(hit.transform.gameObject.CompareTag("ball") && ballCueDis<=0.1f)
            {
                // lock pos & rot
                lockPos = gameObject.GetComponent<Transform>().localPosition - rbTransform.up * (0.1f-ballCueDis);
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
            if(Physics.Raycast(ray, out hit, 0.1f))
            {   
                if(hit.transform.gameObject.CompareTag("ball"))
                {
                    float ballCueDis = Vector3.Distance(hit.transform.position, cueHead.GetComponent<Transform>().position);
                    float circleSize = Mathf.Lerp(0.03f, 0.06f, ballCueDis);
                    DecalProjector decalProjector = circleProjector.GetComponent<DecalProjector>();
                    decalProjector.size = new Vector3(circleSize, circleSize, circleSize);
                    circleProjector.SetActive(true);
                    circleProjector.GetComponent<Transform>().position = hit.point;
                }
            }

            Debug.DrawRay(cueHead.GetComponent<Transform>().position, cueHead.GetComponent<Transform>().up, Color.green);

            if(Physics.Raycast(ray, out hit, 0.02f))
            {   
                if(hit.transform.gameObject.CompareTag("ball"))
                {
                    GameObject ball = hit.transform.gameObject;
                    if((Mathf.Abs(speed) * 1000000/2)>2)
                    {
                        Debug.Log("hit");
                        Debug.Log(-hit.normal * Mathf.Abs(speed) * 1000000/2);
                        Debug.Log(ball);
                        ball.GetComponentInChildren<Rigidbody>().AddForceAtPosition(-hit.normal * Mathf.Abs(speed) * 1000000/2, hit.point); // give force
                        gameManager.IsShot();
                        // cueHitBallAudio = gameObject.GetComponent<AudioSource>(); // audio
                        // cueHitBallAudio.time = 1f; // start playing from 1s 
                        // cueHitBallAudio.Play();
                        // Invoke("StopAudio", 2f); // duration: 2s
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
        press = false;

        circleProjector.SetActive(false); // disable circle

        rb.constraints = RigidbodyConstraints.None;

        rightHandcontroller.enableInputTracking = true;

        checkHit = false;
    }
}
