using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[System.Serializable]
public struct CameraKeyframe
{
    public Vector3 position;
    public Vector3 lookAtOffset; // Offset from sphereCenter to look at
    public float duration;       // Time in seconds to reach this keyframe from the previous one
}

public class CameraSphericalPath : MonoBehaviour
{
    public List<CameraKeyframe> keyframes = new List<CameraKeyframe>();

    public Key triggerKey = Key.Space;
    public Vector3 sphereCenter = Vector3.zero;
    public bool loopPath = true; // Added loop option

    private int currentKeyframeIndex;
    private float segmentTime; // Time elapsed in the current keyframe segment
    private bool isMoving;

    void Reset()
    {
        // Set a default sphereCenter, assuming the black hole is at or near the origin
        sphereCenter = Vector3.zero; 

        // Clear existing keyframes
        keyframes.Clear();

        // Add default keyframes for cinematic movement
        // Phase 1: Swoop in from afar, maintaining relatively straight pitch
        // Keyframe 1: Very far, high up, slightly to the side, looking at center
        keyframes.Add(new CameraKeyframe
        {
            position = new Vector3(400f, 150f, 600f),
            lookAtOffset = new Vector3(0f, 0f, 0f), // Looking directly at center (straight pitch from afar)
            duration = 15f 
        });

        // Keyframe 2: Approaching, descending, still far, pitch straight
        keyframes.Add(new CameraKeyframe
        {
            position = new Vector3(250f, 75f, 350f),
            lookAtOffset = new Vector3(0f, 0f, 0f), // Maintain straight pitch
            duration = 12f 
        });

        // Phase 2: Go towards black hole level, circle rings at an angle
        // Keyframe 3: Closer, almost at ring level, starting angled circle, subtle pitch
        keyframes.Add(new CameraKeyframe
        {
            position = new Vector3(100f, 20f, 150f), // Closer, near ring plane (assuming Y=0 is ring plane)
            lookAtOffset = new Vector3(0f, -5f, 0f), // Look slightly down into the ring
            duration = 10f 
        });

        // Keyframe 4: Circling rings, more to the side, maintaining angle
        keyframes.Add(new CameraKeyframe
        {
            position = new Vector3(-120f, 15f, 80f), // Orbiting
            lookAtOffset = new Vector3(0f, -7f, 0f), // Maintain look into ring
            duration = 12f 
        });

        // Keyframe 5: Continue circling, from another angle, slightly closer
        keyframes.Add(new CameraKeyframe
        {
            position = new Vector3(-50f, 10f, -100f), // Orbiting
            lookAtOffset = new Vector3(0f, -3f, 0f), // Slight adjustment
            duration = 10f 
        });
        
        // Keyframe 6: Final close approach before swooping out
        keyframes.Add(new CameraKeyframe
        {
            position = new Vector3(80f, 10f, -50f), // Orbiting
            lookAtOffset = new Vector3(0f, -5f, 0f), // Look into ring
            duration = 8f 
        });

        // Phase 3: Zoom out, ascending, pitch straight
        // Keyframe 7: Moving away, ascending, straightening pitch
        keyframes.Add(new CameraKeyframe
        {
            position = new Vector3(300f, 100f, -400f),
            lookAtOffset = new Vector3(0f, 0f, 0f), // Straightening pitch
            duration = 15f 
        });

        // Keyframe 8 (Loop back to start): Return to the initial very far position
        keyframes.Add(new CameraKeyframe
        {
            position = new Vector3(400f, 150f, 600f),
            lookAtOffset = new Vector3(0f, 0f, 0f), // Straight pitch
            duration = 15f 
        });

        // Ensure looping by default
        loopPath = true;
    }

    void Start()
    {
        if (keyframes.Count >= 2)
        {
            currentKeyframeIndex = 0;
            segmentTime = 0f;
            // No longer need to precompute segment lengths
        }
    }

    void Update()
    {
        if (Keyboard.current[triggerKey].wasPressedThisFrame && keyframes.Count >= 2)
        {
            currentKeyframeIndex = 0;
            segmentTime = 0f;
            isMoving = true;
        }

        if (!isMoving || keyframes.Count < 2) return;

        CameraKeyframe currentKeyframe = keyframes[currentKeyframeIndex];
        CameraKeyframe nextKeyframe = keyframes[(currentKeyframeIndex + 1) % keyframes.Count];

        segmentTime += Time.deltaTime;

        float normalizedTime = segmentTime / nextKeyframe.duration;

        if (normalizedTime >= 1f)
        {
            normalizedTime = 1f;
            currentKeyframeIndex++;
            if (currentKeyframeIndex >= keyframes.Count)
            {
                if (loopPath)
                {
                    currentKeyframeIndex = 0;
                }
                else
                {
                    isMoving = false;
                    return;
                }
            }
            segmentTime = 0f; // Reset segment time for the new segment
            currentKeyframe = keyframes[currentKeyframeIndex];
            nextKeyframe = keyframes[(currentKeyframeIndex + 1) % keyframes.Count];
        }

        // Interpolate position
        Vector3 startPosRelative = currentKeyframe.position - sphereCenter;
        Vector3 endPosRelative = nextKeyframe.position - sphereCenter;

        Vector3 currentPosRelative = SphericalInterpolate(startPosRelative.normalized, endPosRelative.normalized, normalizedTime) * Mathf.Lerp(startPosRelative.magnitude, endPosRelative.magnitude, normalizedTime);
        transform.position = sphereCenter + currentPosRelative;

        // Interpolate lookAtOffset
        Vector3 currentLookAtOffset = Vector3.Lerp(currentKeyframe.lookAtOffset, nextKeyframe.lookAtOffset, normalizedTime);
        Vector3 lookTarget = sphereCenter + currentLookAtOffset;

        // Set camera rotation to look at the target
        transform.rotation = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);
    }

        Vector3 SphericalInterpolate(Vector3 a, Vector3 b, float t)

        {

            float dot = Mathf.Clamp(Vector3.Dot(a, b), -1f, 1f);

            float theta = Mathf.Acos(dot) * t;

            Vector3 relativeVec = (b - a * dot).normalized;

            return a * Mathf.Cos(theta) + relativeVec * Mathf.Sin(theta);

        }

    }
