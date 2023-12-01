using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tableCheck : MonoBehaviour
{
    void Update()
    {
        // Assuming this script is attached to the table GameObject

        // Get the rotation of the table
        Quaternion tableRotation = transform.rotation;

        // Extract the angle around the up (y) axis
        float tiltAngleY = tableRotation.eulerAngles.y;

        // You can set a threshold for what you consider "horizontally smooth"
        float horizontalThreshold = 5.0f; // Adjust this value based on your needs

        // Check if the table is horizontally smooth based on the threshold
        bool isHorizontallySmooth = Mathf.Abs(tiltAngleY) < horizontalThreshold;

        // Now, you can use the isHorizontallySmooth variable for your logic
        if (isHorizontallySmooth)
        {
            Debug.Log("The table is horizontally smooth.");
        }
        else
        {
            Debug.Log("The table is not horizontally smooth.");
        }
    }
}
