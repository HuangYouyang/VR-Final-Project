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

    public bool press = false;
    private bool checkHit = false;
    public GameObject cueHead;
    public GameObject[] Balls;

    public GameObject circleProjector;

    // lock cue
    public GameObject player;
    public  Vector3 lockPos;
    public Quaternion lockRot;
    private float leftForwardDistance;

    // audio
    private AudioSource cueHitBallAudio;

    // game manager
    GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rbTransform = GetComponent<Transform>(); 

        controller.selectAction.action.started += cueStick;
        controller.selectAction.action.canceled += Release;

        leftForwardDistance = 0;
    }

    // distance: left hand moves back and forth 
    float leftForward()
    {
        Vector3 displacement = leftHand.transform.localPosition - leftPrePos;
        float distanceMovedForward = Vector3.Dot(displacement, leftHand.transform.forward);
        leftPrePos = leftHand.transform.localPosition;

        return distanceMovedForward;
    }

    private void cueStick(InputAction.CallbackContext context) 
    {
        leftForwardDistance = 0;

        press = true;

        // lock pos & rot
        lockPos = gameObject.GetComponent<Transform>().localPosition;
        lockRot = gameObject.GetComponent<Transform>().localRotation;
    }

    void FixedUpdate()
    {
        leftForwardDistance = leftForwardDistance + leftForward();

        if(press)
        {
            // freeze cue & hands
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rightHand.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

            // lock cue
            gameObject.GetComponent<Transform>().localPosition = lockPos + rbTransform.up * leftForwardDistance * 20; 
            gameObject.GetComponent<Transform>().localRotation = lockRot;
        
            // ray
            var ray = new Ray(cueHead.GetComponent<Transform>().position, cueHead.GetComponent<Transform>().up);
            RaycastHit hit;

            // show circle on the ball if hit
            if(Physics.Raycast(ray, out hit, 1.0f))
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

            rightHand.GetComponent<Transform>().parent = gameObject.GetComponent<Transform>();

            Debug.DrawRay(cueHead.GetComponent<Transform>().position, cueHead.GetComponent<Transform>().up, Color.green);

            if(Physics.Raycast(ray, out hit, 1.0f))
            {   
                if(hit.transform.gameObject.CompareTag("ball"))
                {
                    GameObject ball = hit.transform.gameObject;
                    Debug.Log(leftForwardDistance);
                    if(leftForwardDistance>0)
                    {
                        ball.GetComponent<Rigidbody>().AddForceAtPosition(-hit.normal * Mathf.Abs(leftForwardDistance) * 200, hit.point); // give force
                        gameManager.IsShot();
                        cueHitBallAudio = gameObject.GetComponent<AudioSource>(); // audio
                        cueHitBallAudio.time = 2f; // start playing from 1s 
                        cueHitBallAudio.Play();
                        Invoke("StopAudio", 2f); // duration: 2s
                    }
                }
            }
        }
    }

    void StopAudio()
    {
        cueHitBallAudio.Stop();
    }

    private void Release(InputAction.CallbackContext context)
    {   
        press = false;

        rightHand.GetComponent<Transform>().parent = player.GetComponent<Transform>();

        circleProjector.SetActive(false); // disable circle

        rb.constraints = RigidbodyConstraints.None;
        rightHand.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
    }
}
