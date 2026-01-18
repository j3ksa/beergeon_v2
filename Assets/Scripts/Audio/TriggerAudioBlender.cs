using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

//Designed for 1 target object to enter the area at a time.
public class TriggerBoxAudioBlender : MonoBehaviour
{
    private float progressThroughTrigger; // The progress of the desired object through the trigger box. Field.

    public float ProgressThroughTrigger // The property of the 'progressThroughTrigger' field that you can access from other scripts.
    {
        get { return progressThroughTrigger; }
    }

    //public fields
    public enum TagOrGameObject { Tag, GameObject };
    [Header("Choose Object Identification Method")]
    public TagOrGameObject identifyObjectBy;
    public string checkForTag = "Player"; // The tag of the desired object you wish to track.
    public GameObject targetObject;
    [Header("Choose Response Type")]
    public bool multiAxis = false; // Can the desired object enter from the sides of the trigger box?
    [Range(1f, 99f)] public float halfWayMark = 50; // The halfway point between the enterance and exit of the trigger box.
    [Header("Call Functions With Trigger")]
    public UnityEvent onEnterPointA; // Call functions from other scripts when the object enters the trigger box from point A.
    public UnityEvent onEnterPointB; // Call functions from other scripts when the object enters the trigger box from point B.
    public UnityEvent onStay; // Call functions from other scripts when the object stays inside the trigger box.
    public UnityEvent onExitPointA; // Call functions from other scripts when the object exits the trigger box from point A.
    public UnityEvent onExitPointB; // Call functions from other scripts when the object exits the trigger box from point B.
    [Header("Debug Tools")]
    public bool showGizmos = true; // Show the entrance point, exit point and progress of desired object through the trigger in the scene view.
    public GUIStyle textSettings;

    //private fields
    private BoxCollider boxCollider; // The box collider component that must be attached to the same gameObject as this script.
    private bool trackingPosition; // Enabled when the desired object enters the trigger box.
    private Vector3 PointA; // The entrance of the trigger box.
    private Vector3 PointB; // The exit of the trigger box.
    private Vector3 PointAB; // The point on line AB that's closest to the desired object.
    private float ProgressAB; // How far away pointAB is from pointA as a percentage.
    private Vector3 target_global_position_Last_Frame; // The position of the desired object within the trigger box during the last frame.
    private Vector3 trigger_box_position_last_frame; // The position of the trigger box during the last frame.
    private Quaternion trigger_box_rotation_last_frame; // The rotation of the trigger box during the last frame.
    private Vector3 trigger_box_scale_last_frame; // The scale of the trigger box during the last frame.

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (showGizmos)
        {
            if (GetComponent<BoxCollider>() != null)
                boxCollider = GetComponent<BoxCollider>();
            Vector3 center = boxCollider.center;
            Vector3 size = boxCollider.size;

            Gizmos.matrix = this.transform.localToWorldMatrix;

            //show green sqaure (point A)
            Color newColor = new Vector4(Color.green.r, Color.green.g, Color.green.b, 0.4f);
            Gizmos.color = newColor;
            Gizmos.DrawCube(new Vector3(-0.5f * size.x, 0f, 0f) + center, new Vector3(0.01f * size.x, 1f * size.y, 1f * size.z));

            //show red square (point B)
            newColor = new Vector4(Color.red.r, Color.red.g, Color.red.b, 0.4f);
            Gizmos.color = newColor;
            Gizmos.DrawCube(new Vector3(0.5f * size.x, 0f, 0f) + center, new Vector3(0.01f * size.x, 1f * size.y, 1f * size.z));

            //show yellow square (half way point)
            if (multiAxis)
            {
                newColor = new Vector4(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.4f);
                Gizmos.color = newColor;
                float x = halfWayMark / 100 - 0.5f;
                Gizmos.DrawCube(new Vector3(x * size.x, 0f, 0f) + center, new Vector3(0.01f * size.x, 1f * size.y, 1f * size.z));
            }

            //show outline of the entire trigger box
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Vector3.zero + center, new Vector3(1f * size.x, 1f * size.y, 1f * size.z));

            //progress text and lines between enterance and exit areas.
            if (trackingPosition)
            {
                GUI.color = Color.white;
                Handles.Label(PointAB, "Progress Through Trigger:" + "\n" + progressThroughTrigger.ToString("F2") + "%", textSettings);

                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(PointA, PointB);

                if (multiAxis)
                    Gizmos.DrawLine(transform.TransformPoint(new Vector3(over_100(ProgressAB) - 0.5f * size.x, 0, -0.5f * size.z) + center),
                        transform.TransformPoint(new Vector3(over_100(ProgressAB) - 0.5f * size.x, 0, 0.5f * size.z) + center));
            }
        }
    }
#endif

    private void Start()
    {
        if (GetComponent<BoxCollider>() == null) // Checks to see if the 'Box Collider' component that should attached to the game object is missing.
            Debug.LogError("HEY! You've attached a 'Trigger Box Audio Blender' component to the" + gameObject.name + " game object but you haven't attached " +
                "a 'Box Collider' component. The 'Trigger Box Audio Blender' won't work without one, so get on it already and add a 'Box Collider to '" +
                gameObject.name + "!");
        else
        {
            boxCollider = GetComponent<BoxCollider>();
            if (!boxCollider.isTrigger)
                boxCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other) // If something enters the Trigger Box..
    {
        if (ObjectCheck(other)) // Check to see if that object was our target player object.
        {
            trackingPosition = true;
            CalculateTriggerProgress(other); // Find out how far into the trigger box the player entered.
            if (progressThroughTrigger <= halfWayMark) // See if they entered before or after the half way mark to know if they entered from point A or B.
                onEnterPointA.Invoke();
            else
                onEnterPointB.Invoke();
        }
    }

    private void OnTriggerStay(Collider other) // This runs continously every frame whilst our target player object is inside of the box.
    {
        if (ObjectCheck(other) && trackingPosition) // Whilst the player is still inside of the trigger box...
        {
            CalculateTriggerProgress(other); // Continue to calculate how far into the box they are, how much progress have they made from point A to B.
            onStay.Invoke();
        }
    }

    private void OnTriggerExit(Collider other) // If the player exits the trigger box.
    {
        if (ObjectCheck(other)) // Check that it was the player that left and if it was...
        {
            trackingPosition = false;
            CalculateTriggerProgress(other); // ...find out from which side they left, point A or B.
            if (progressThroughTrigger <= halfWayMark)
                onExitPointA.Invoke();
            else
                onExitPointB.Invoke();
        }
    }

    private bool ObjectCheck(Collider other) // This is used to see if the object that entered was the player object.
    {
        bool objectIdentified = false;
        if (identifyObjectBy == TagOrGameObject.Tag) // If we're comparing by tag, check the objects tag.
        {
            if (!string.IsNullOrEmpty(checkForTag))
            {
                if (other.CompareTag(checkForTag))
                    objectIdentified = true;
                else
                    objectIdentified = false;
            }
            else
                Debug.LogWarning("Hey, so you've set the 'Identify Object By' field on the 'Trigger Box Audio Blender' component attached to your '" + gameObject.name + "' object to 'Tag', " +
                    "but the 'Check For Tag' field is empty. In order for the trigger box to work, you need to enter the name of the tag you've given to the game object " +
                    "that you want to activate this trigger box with.");
        }
        else // If not, then we must be checking for a specific game object.
        {
            if (targetObject != null)
            {
                if (other.gameObject == targetObject)
                    objectIdentified = true;
                else
                    objectIdentified = false;
            }
            else
                Debug.LogWarning("Hey, so you've set the 'Identify Object By' field on the 'Trigger Box Audio Blender' component attached to your '" + gameObject.name + "' object to 'GameObject', " +
                    "but the 'Target Object' field is empty. In order for the trigger box to work, you need to reference the game object that you want to activate " +
                    "it with in this field.");
        }
        return objectIdentified;
    }

    private void CalculateTriggerProgress(Collider other) // This function calculates how much progress the player has made between point A and B.
    {
        Vector3 target_global_position = other.transform.position; // The target objects global position.
        Vector3 target_local_position = transform.InverseTransformPoint(target_global_position); // The target objects local position relative to the trigger box itself.

        if (target_global_position != target_global_position_Last_Frame // If the target object or the trigger box moves, then we need to calculate it's new progress value.
            || transform.position != trigger_box_position_last_frame
            || transform.rotation != trigger_box_rotation_last_frame
            || transform.localScale != trigger_box_scale_last_frame)
        {
            Vector3 pointA = new Vector3(-0.5f, 0, 0); // Get point A.
            Vector3 pointB = new Vector3(0.5f, 0, 0); // Get point B.
            Vector3 pointAB = GetIntersectionPoint(pointA, pointB, 1, target_local_position); // Get the intersection point between A and B (point AB).

            float line_A_to_AB = Vector3.Distance(pointA, pointAB); // Get the distance between point A and point AB.
            float progress_AB = line_A_to_AB * 100; // Take that distance and turn it into a percentage %.
            progress_AB = Mathf.Clamp(progress_AB, 0f, 100f); // Clamp the variable so that the value of it stays within the range of 0 - 100. 

            if (multiAxis) // If you've selected multi position in the inspector tab...
            {
                Vector3 pointC = new Vector3(pointAB.x, 0, 0.5f); // Get point C.
                Vector3 pointD = new Vector3(pointAB.x, 0, -0.5f); // Get point D.
                Vector3 pointCD = GetIntersectionPoint(pointC, pointD, 1, target_local_position); // Get the intersection point between C and D (point CD).

                float distance_from_AB_to_CD = Vector3.Distance(pointAB, pointCD); // Get the distance between intersection points AB and CD.
                float distance_from_CD_to_line_C = 0.5f - distance_from_AB_to_CD; // Get the distance between intersection point CD and the closest edge of the trigger box. 0.5 = half the box width.

                if (progress_AB <= halfWayMark) // If the target hasn't yet crossed the half way mark coming from point A...
                {
                    float progress_to_halfwayMark = X_as_a_percentage_of_Y(progress_AB, halfWayMark);    // How close is the target object to the halfway mark as a percentage %.
                    float distance_from_line_AB_to_triangle_hypotenuse = X_percent_of_Y(progress_to_halfwayMark, 0.5f);       // Take the 'progress to halway mark' percentage of half the width of the box. This gives us the 
                                                                                                                              // distance from the line AB to the hypotenuse of the progress triangle.
                    float distance_from_line_C_to_triangle_hypotenuse = 0.5f - distance_from_line_AB_to_triangle_hypotenuse; // half of the box width, minus the distance from line AB to the hypotenuse gives us the distance
                                                                                                                             // from line C to the hypotenuse.
                    float progressCH = X_as_a_percentage_of_Y(distance_from_CD_to_line_C, distance_from_line_C_to_triangle_hypotenuse);
                    // How close is the target object to crossing the hypotenuse as it aproaches from line C. 
                    progressCH = Mathf.Clamp(progressCH, 0f, 100);                    // Clamp the variable so that the value of it stays within the range of 0 - 100. 
                    progressThroughTrigger = X_percent_of_Y(progressCH, progress_AB);             // What is 'progressCH' percent of 'progressAB'. That is our final progress value as a percentage %.
                }
                else // If the target has crossed the hald way mark coming from point A, do the same as above. This time however, progress needs to be 100 when the player is at the edge of the box instead of 0.
                {
                    float progress_from_halfwayMark = X_as_a_percentage_of_Y(100 - progress_AB, 100 - halfWayMark);
                    float distance_from_line_AB_to_triangle_hypotenuse = X_percent_of_Y(progress_from_halfwayMark, 0.5f);
                    float distance_from_line_C_to_triangle_hypotenuse = 0.5f - distance_from_line_AB_to_triangle_hypotenuse;
                    float progressCH = 100 - X_as_a_percentage_of_Y(distance_from_CD_to_line_C, distance_from_line_C_to_triangle_hypotenuse);
                    progressCH = Mathf.Clamp(progressCH, 0f, 100);
                    float progress_left = 100 - progress_AB;
                    progressThroughTrigger = progress_AB + X_percent_of_Y(progressCH, progress_left);
                }
                progressThroughTrigger = Mathf.Clamp(progressThroughTrigger, 0f, 100f); // Finally clamp the progress value between 0 and 100 (just to make sure it is never below or above them respectively)
            }
            else
                progressThroughTrigger = progress_AB; // If Multi-Axis wasn't used, we can just uses progress_AB as our final value/

            PointA = transform.TransformPoint(pointA); // Convert point A into global coordinates to be shown in the editor.
            PointB = transform.TransformPoint(pointB);  // Convert point B into global coordinates to be shown in the editor.
            PointAB = transform.TransformPoint(pointAB);  // Convert point AB into global coordinates to be shown in the editor.
            ProgressAB = progress_AB;  // ProgressAB will show progressAB in the editor.
        }

        // Now that we've finished calculating the players progres, get the current position rotation and scale of the target object and the trigger box to be checked during the next frame.
        target_global_position_Last_Frame = target_global_position;
        trigger_box_position_last_frame = transform.position;
        trigger_box_rotation_last_frame = transform.rotation;
        trigger_box_scale_last_frame = transform.localScale;
    }

    // Credit goes to Cheddar Game Dev on YouTube for this function.
    // www.youtube.com/watch?v=CiAwRA6IQi0&list=LL&index=10&ab_channel=CheddarGameDev
    // This function takes two points, a distance between the two and a thrid point in space.
    // It then returns a new point (the intersection point) on the line between the first two points that the third point is closest to.
    private Vector3 GetIntersectionPoint(Vector3 firstPoint, Vector3 secondPoint, float distance, Vector3 target)
    {
        float t = ((target.x - secondPoint.x) * (firstPoint.x - secondPoint.x) +
            (target.y - secondPoint.y) * (firstPoint.y - secondPoint.y) +
            (target.z - secondPoint.z) * (firstPoint.z - secondPoint.z)) / (distance * distance);

        t = Mathf.Clamp(t, 0f, 1f);

        Vector3 intersection = new Vector3();
        intersection.x = secondPoint.x + t * (firstPoint.x - secondPoint.x);
        intersection.y = secondPoint.y + t * (firstPoint.y - secondPoint.y);
        intersection.z = secondPoint.z + t * (firstPoint.z - secondPoint.z);

        return intersection;
    }

    private float over_100(float number)
    {
        return number / 100f;
    }

    private float X_as_a_percentage_of_Y(float x, float y) // returns a %.
    {
        return (x / y) * 100f;
    }

    private float X_percent_of_Y(float x, float y) // returns a value.
    {
        return (y / 100f) * x;
    }
}