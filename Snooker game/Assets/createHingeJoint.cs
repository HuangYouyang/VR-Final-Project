using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// create hinge joint for cue to put on
public class createHingeJoint : MonoBehaviour
{
    public Hand hand;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cue"))
        {
            hand.hinge = true;
            hand.hingeJointPos = other.ClosestPoint(transform.position);
        }
    }

    // Called when another Collider exits this trigger
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Cue"))
        {
            hand.hinge = false;
            hand.hingeJointPos = new Vector3(0, 0, 0);
        }
    }
}
