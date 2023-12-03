using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabHandPose : MonoBehaviour
{
    public float posTransitionDuration = 0.2f;

    public HandData rightHandPose;
    public HandData leftHandPose;

    public Hand physicsRightHand;
    public Hand physicsLeftHand;

    private Vector3 startingHandPosition;
    private Vector3 finalHandPosition;
    private Quaternion startingHandRotation;
    private Quaternion finalHandRotation;

    private Quaternion[] startingFingerRotations;
    private Quaternion[] finalFingerRotations;

    void Start()
    {
        XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();

        grabInteractable.selectEntered.AddListener(SetupPose);
        grabInteractable.selectExited.AddListener(UnSetPose);

        rightHandPose.gameObject.SetActive(false);
    }

    public void SetupPose(BaseInteractionEventArgs arg)
    {
        // HandData handData;

        // // left hand or right hand
        // if(controller.CompareTag("Right Hand"))
        // {
        //     handData = physicsRightHand.transform.GetComponent<HandData>();
        //     handData.animator.enabled = false;
        //     SetHandDataValues(handData, rightHandPose);
        // }
        // else
        // {
        //     handData = physicsLeftHand.transform.GetComponent<HandData>();
        //     handData.animator.enabled = false;
        //     SetHandDataValues(handData, leftHandPose);
        // }

        if(arg.interactorObject is XRDirectInteractor)
        {
            HandData handData = arg.interactorObject.transform.GetComponentInChildren<HandData>();
            handData.animator.enabled = false;

            SetHandDataValues(handData, rightHandPose);

            StartCoroutine(SetHandDataRoutine(handData, finalHandPosition, finalHandRotation, finalFingerRotations, startingHandPosition, startingHandRotation, startingFingerRotations));
        }
    }

    public void SetHandDataValues(HandData h1, HandData h2)
    {
        startingHandPosition = new Vector3(
            h1.root.localPosition.x/h1.root.localScale.x, 
            h1.root.localPosition.y/h1.root.localScale.y,
            h1.root.localPosition.z/h1.root.localScale.z);
        finalHandPosition = new Vector3(
            h2.root.localPosition.x/h2.root.localScale.x, 
            h2.root.localPosition.y/h2.root.localScale.y,
            h2.root.localPosition.z/h2.root.localScale.z);

        startingHandRotation = h1.root.localRotation;
        finalHandRotation = h2.root.localRotation;

        startingFingerRotations = new Quaternion[h1.fingerBones.Length];
        finalFingerRotations = new Quaternion[h2.fingerBones.Length];

        for(int i=0; i<h1.fingerBones.Length; i++)
        {
            startingFingerRotations[i] = h1.fingerBones[i].localRotation;
            finalFingerRotations[i] = h2.fingerBones[i].localRotation;
        }
    }

    public void SetHandData(HandData h, Vector3 newPosition, Quaternion newRotation, Quaternion[] newBonesRotations)
    {
        h.root.localPosition  = newPosition;
        h.root.localRotation = newRotation;

        for(int i=0; i<newBonesRotations.Length; i++)
        {
            h.fingerBones[i].localRotation = newBonesRotations[i];
        }
    }

    public IEnumerator SetHandDataRoutine(HandData h, Vector3 newPosition, Quaternion newRotation, Quaternion[] newBonesRotations, Vector3 startingPosition, Quaternion startingRotation, Quaternion[] startingBonesRotation)
    {
        float timer = 0;

        while(timer < posTransitionDuration)
        {
            Vector3 p = Vector3.Lerp(startingPosition, newPosition, timer/posTransitionDuration);
            Quaternion r = Quaternion.Lerp(startingRotation, newRotation, timer/posTransitionDuration);

            h.root.localPosition = p;
            h.root.localRotation = r;

            for(int i=0; i<newBonesRotations.Length; i++)
            {
                h.fingerBones[i].localRotation = Quaternion.Lerp(startingBonesRotation[i], newBonesRotations[i], timer/posTransitionDuration);
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }

    public void UnSetPose(BaseInteractionEventArgs arg)
    {
        if(arg.interactorObject is XRDirectInteractor)
        {
            HandData handData = arg.interactorObject.transform.GetComponentInChildren<HandData>();
            handData.animator.enabled = true;

            StartCoroutine(SetHandDataRoutine(handData, startingHandPosition, startingHandRotation, startingFingerRotations, finalHandPosition, finalHandRotation, finalFingerRotations));
        }
    }

    // public void UnSetPose()
    // {
    //     // if(arg.interactorObject is XRDirectInteractor)
    //     // {
    //         // HandData handData = arg.interactorObject.transform.GetComponent<HandData>();

    //         Debug.Log("Unset!!");
    //         HandData handData = physicsRightHand.transform.GetComponent<HandData>();
    //         handData.animator.enabled = false;

    //         StartCoroutine(SetHandDataRoutine(handData, startingHandPosition, startingHandRotation, startingFingerRotations, finalHandPosition, finalHandRotation, finalFingerRotations));
    //     // }
    // }
}
