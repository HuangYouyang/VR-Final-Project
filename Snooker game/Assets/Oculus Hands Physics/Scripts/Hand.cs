using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

// [RequireComponent(typeof(Animator))]
public class Hand : MonoBehaviour
{
    // Animation
    // public float animationSpeed;
    // Animator animator;
    // private float gripTarget;
    // private float triggerTarget;
    // private float gripCurrent;
    // private float triggerCurrent;
    // private string animatorGripParam = "Grip";
    // private string animatorTriggerParam = "Trigger";

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
    [SerializeField] float reachDistance = 0.1f, jointDistance = 0.05f;
    [SerializeField] private LayerMask grabbableLayer;

    private Transform followTarget;
    private Rigidbody body;

    private bool isGrabbing;
    private GameObject heldObject;
    private Transform grabPoint;
    private FixedJoint joint1, joint2;

    // Start is called before the first frame update
    void Start()
    {
        // Animation
        // animator = GetComponent<Animator>();

        // Physics Movement
        followTarget = controller.gameObject.transform;
        body = GetComponent<Rigidbody>();
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.mass = 20f;
        body.maxAngularVelocity = 20f;

        // Input Setup
        controller.selectAction.action.started += Grab;
        controller.selectAction.action.canceled += Release;

        // Teleport hands
        body.position = followTarget.position;
        body.rotation = followTarget.rotation;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // AnimateHand(); // animation

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
        body.angularVelocity = axis * (angle * Mathf.Deg2Rad * rotateSpeed);
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

        // Create a grab point
        grabPoint = new GameObject().transform;
        grabPoint.position = collider.ClosestPoint(palm.position);
        grabPoint.parent = heldObject.transform;

        // Move hand to grab point
        followTarget = grabPoint;

        // Wait for hand to reach grab point
        while(grabPoint!=null && Vector3.Distance(grabPoint.position, palm.position) > jointDistance && isGrabbing)
        {
            yield return new WaitForEndOfFrame();
        }

        // Freeze
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        targetBody.velocity = Vector3.zero;
        targetBody.angularVelocity = Vector3.zero;

        targetBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        targetBody.interpolation = RigidbodyInterpolation.Interpolate;

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
        joint2.enablePreprocessing = false;

        // Reset follow target
        followTarget = controller.gameObject.transform;
    }

    private void Release(InputAction.CallbackContext context)
    {   
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

    // Animation

    // internal void SetGrip(float v)
    // {
    //     gripTarget = v;
    // }

    // internal void SetTrigger(float v)
    // {
    //     triggerTarget = v;
    // }

    // void AnimateHand()
    // {
    //     if(gripCurrent!=gripTarget)
    //     {
    //         gripCurrent = Mathf.MoveTowards(gripCurrent, gripTarget, Time.deltaTime * animationSpeed);
    //         animator.SetFloat(animatorGripParam, gripCurrent);
    //     }
    //     if(triggerCurrent!=triggerTarget)
    //     {
    //         triggerCurrent = Mathf.MoveTowards(triggerCurrent, triggerTarget, Time.deltaTime * animationSpeed);
    //         animator.SetFloat(animatorTriggerParam, triggerCurrent);
    //     }
    // }
}
