using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;


public class Hand : MonoBehaviour
{
    // Physics Movement
    [Space]
    [SerializeField] private ActionBasedController controller; // follow controller
    [SerializeField] private float followSpeed = 30f; 
    [SerializeField] private float rotateSpeed = 100f;
    [Space]
    [SerializeField] private Vector3 positionOffset;
    [SerializeField] private Vector3 rotationOffset;
    [Space]
    [SerializeField] private Transform palm; // gameObject attached on the palm
    [SerializeField] float reachDistance = 0.05f, jointDistance = 0.01f;
    [SerializeField] private LayerMask grabbableLayer;

    private Transform followTarget;
    private Rigidbody body;

    // Non-physics hand
    [Space]
    public bool show_nonPhysicalHand = true;
    public Renderer nonPhysicalHand;
    public float showNonPhysicalHandDistance = 0.05f;

    // disable collider when grab objects
    private Collider[] handColliders;

    private bool isGrabbing;
    public GameObject heldObject;
    private Transform grabPoint;
    private FixedJoint joint1, joint2;

    // fingerBones
    public Transform[] fingerBones;
    public Transform[] straightState;
    public Transform[] bentState;
    private FixedJoint[] fingerBonesJoints;
    public float deltaDeg = 0.2f;
    public float distanceTar = 0.3f; // distance between target object and fingertip
    public float degree = 0.0f;

    // hingeJoint
    public bool hinge = false;
    public Vector3 hingeJointPos;

    // cue
    public GameObject cue;

    // Start is called before the first frame update
    void Start()
    {
        // Physics Movement
        followTarget = controller.gameObject.transform;
        body = GetComponent<Rigidbody>();
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.mass = 20f;
        body.maxAngularVelocity = 20f;

        // disable collider when grab objects
        handColliders = GetComponentsInChildren<Collider>(); // find all the hand colliders

        // Input Setup
        controller.selectAction.action.started += Grab;
        controller.selectAction.action.canceled += Release;

        // Teleport hands
        body.position = followTarget.position;
        body.rotation = followTarget.rotation;
    }

    public void EnableHandCollider()
    {
        Debug.Log("enable collider");

        // foreach (var item in handColliders)
        // {
        //     item.enabled = true;
        // }
    }

    public void EnableHandColliderDelay(float delay)
    {
        Invoke("EnableHandCollider", delay);
    }

    public void DisableHandCollider()
    {
        Debug.Log("disable collider");

        // foreach (var item in handColliders)
        // {
        //     item.enabled = false;
        // }
    }

    private void Update()
    {
        float distance = Vector3.Distance(transform.position, followTarget.position);

        // show non PhysicalHand
        if(show_nonPhysicalHand==true) showNonPhysicsHand(distance);

        // when hands stuck 
        if(distance > 0.5)
        {
            body.detectCollisions = false;
        }
        else
        {   
            body.detectCollisions = true;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(cue.GetComponent<CuePickUp>().press == false)
            PhysicsMove();
        else if(controller.CompareTag("Left Hand"))
            PhysicsMove();
    }

    private void PhysicsMove()
    {
        // Position
        var positionWithOffset = followTarget.TransformPoint(positionOffset);
        var distance = Vector3.Distance(positionWithOffset, transform.position);
        body.velocity = (followTarget.position - transform.position).normalized * (followSpeed * distance);

        // Rotation
        var rotationWithOffset = followTarget.rotation * Quaternion.Euler(rotationOffset);
        var q = rotationWithOffset * Quaternion.Inverse(body.rotation);
        q.ToAngleAxis(out float angle, out Vector3 axis);
        if (Mathf.Abs(axis.magnitude) != Mathf.Infinity)
        {
            if (angle > 180.0f) { angle -= 360.0f; }
            body.angularVelocity = axis * (angle * Mathf.Deg2Rad * rotateSpeed);
        }
    }

    private void Grab(InputAction.CallbackContext context) 
    {
        // check if hand has already grabbed sth.
        if(isGrabbing || heldObject) return; 

        // get the grabbable objects
        Collider[] grabbableColliders = Physics.OverlapSphere(palm.position, reachDistance, grabbableLayer);
        if(grabbableColliders.Length < 1) return; 

        var objectToGrab = grabbableColliders[0].transform.gameObject;

        // get the rigidbody
        var objectBody = objectToGrab.GetComponent<Rigidbody>();

        if(objectBody != null)
        {
            heldObject = objectBody.gameObject;
        }
        else // child object
        {
            objectBody = objectToGrab.GetComponentInParent<Rigidbody>();
            if(objectBody != null)
            {
                heldObject = objectBody.gameObject;
            }
            else return;
        }

        StartCoroutine(GrabObject(grabbableColliders[0], objectBody));
    }  

    private IEnumerator GrabObject(Collider collider, Rigidbody targetBody)
    {
        isGrabbing = true;

        // 1.Create a grab point
        grabPoint = new GameObject().transform;
        grabPoint.position = collider.ClosestPoint(palm.position);
        grabPoint.parent = heldObject.transform;

        // 2.Move hand to grab point
        followTarget = grabPoint;

        // Wait for hand to reach grab point
        while(grabPoint!=null && Vector3.Distance(grabPoint.position, palm.position) > jointDistance && isGrabbing)
        {
            yield return new WaitForEndOfFrame();
        }

        // 3.Freeze
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        targetBody.velocity = Vector3.zero;
        targetBody.angularVelocity = Vector3.zero;

        targetBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        targetBody.interpolation = RigidbodyInterpolation.Interpolate;

        heldObject.GetComponent<Rigidbody>().isKinematic = true;
        heldObject.layer = LayerMask.NameToLayer("Non-physics collision");
        Transform[] heldObjectChildren = heldObject.GetComponentsInChildren<Transform>();
        foreach (var heldItem in heldObjectChildren)
        {
            heldItem.gameObject.layer = LayerMask.NameToLayer("Non-physics collision");
        }

        // Reset follow target
        followTarget = controller.gameObject.transform;

        foreach (var item in handColliders)
        {
            item.enabled = true;
        }

        // pre-defined gesture
        GrabHandPose s2 = heldObject.GetComponent<GrabHandPose>();
        s2.SetupPose(controller);

        // Attach joints
        // hand to object
        joint1 = gameObject.AddComponent<FixedJoint>();
        joint1.connectedBody = targetBody;
        joint1.breakForce = float.PositiveInfinity;
        joint1.breakTorque = float.PositiveInfinity;

        joint1.connectedMassScale = 1;
        joint1.massScale = 1;
        joint1.enableCollision = false;
        joint1.enablePreprocessing = false;

        // object to hand
        joint2 = heldObject.AddComponent<FixedJoint>();
        joint2.connectedBody = body;
        joint2.breakForce = float.PositiveInfinity;
        joint2.breakTorque = float.PositiveInfinity;

        joint2.connectedMassScale = 1;
        joint2.massScale = 1;
        joint2.enableCollision = false;
        joint2.enablePreprocessing = false;

        // HandData handData;
        // handData = GetComponent<HandData>();

        // fingerMove(collider);

        heldObject.GetComponent<Rigidbody>().isKinematic = false;
        // foreach (var heldItem in heldObjectChildren)
        // {
        //     heldItem.GetComponent<Rigidbody>().isKinematic = false;
        // }
    }

    private void showNonPhysicsHand(float distance)
    {
        if(distance > showNonPhysicalHandDistance)
        {
            nonPhysicalHand.enabled = true;
        }
        else nonPhysicalHand.enabled = false;
    }

    private void Release(InputAction.CallbackContext context)
    {   
        GrabHandPose s2 = heldObject.GetComponent<GrabHandPose>();
        s2.UnSetPose();

        heldObject.layer = LayerMask.NameToLayer("Grabbable");

        if(joint1 != null)
            Destroy(joint1);
        if(joint2 != null)
            Destroy(joint2);
        if(grabPoint != null)
            Destroy(grabPoint.gameObject);

        if(heldObject != null)
        {
            var targetBody = heldObject.GetComponent<Rigidbody>();
            targetBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            targetBody.interpolation = RigidbodyInterpolation.None;
            heldObject = null;
        }

        isGrabbing = false;
        followTarget = controller.gameObject.transform;
    }

    // Finger Bending - deleted
    // private void fingerMove(Collider heldObjectCollider)
    // {
    //     for(int i=0;i<15;i++)
    //     {
    //         // fingerBones[i].position = straightState[i].position;
    //         fingerBones[i].localRotation = straightState[i].localRotation;
    //     }
            
    //     Transform[] copyFingerBones = fingerBones;

    //     for(int i=0; i<5; i++)
    //     {
    //         // initialize
    //         deltaDeg = 0.2f;
    //         distanceTar = 0.3f;
    //         float distanceHo = Mathf.Infinity;
    //         float degree = 0.0f;

    //         while(degree<1 && distanceHo>=distanceTar)
    //         {
    //             int numberOfFingerJoints = 3;
    //             if(i==2) numberOfFingerJoints = 4;

    //             for(int j=0;j<numberOfFingerJoints;j++)
    //             {
    //                 // fingerBones[i*3+j].position = fingerBones[i*3+j].position + (bentState[i*3+j].position - straightState[i*3+j].position) * degree;  
    //                 // fingerBones[i*3+j].localRotation = fingerBones[i*3+j].localRotation + (bentState[i*3+j].localRotation - straightState[i*3+j].localRotation) * degree

    //                 Quaternion q =  bentState[i*3+j].localRotation * Quaternion.Inverse(straightState[i*3+j].localRotation);
    //                 Quaternion q2 = Quaternion.Slerp(Quaternion.identity, q, degree);
    //                 Quaternion q3 = copyFingerBones[i*3+j].localRotation * q2; 
    //                 fingerBones[i*3+j].localRotation = q3;

    //                 Vector3 closestPoint = heldObjectCollider.ClosestPoint(fingerBones[i*3+numberOfFingerJoints-1].position);
    //                 distanceHo = Vector3.Distance(fingerBones[i*3+numberOfFingerJoints-1].position, closestPoint);

    //                 if(distanceHo<distanceTar)
    //                 {  
    //                     if(distanceTar==0.3f)
    //                     {
    //                         deltaDeg = 0.02f;
    //                         distanceTar = 0.03f;
    //                     }
    //                     else 
    //                     {
    //                         distanceHo = -1;
    //                         j = numberOfFingerJoints;
    //                     }
    //                 }
    //             }
    //             degree += deltaDeg;
    //         }
    //     }
    // } 
}
