using UnityEngine;
using System.Collections;

public class FloatingObject : MonoBehaviour
{
    [Header("Float Settings")]
    public bool isFloating = false;
    public float floatForce = 15f;
    public float amplitude = 0.5f; 
    public float frequency = 1f;   
    public float floatDuration = 5f; 

    [Header("Return Settings")]
    public float returnSpeed = 0.2f;

    private Rigidbody rb;
    private Vector3 homePosition;
    private Quaternion homeRotation;
    private bool isReturning = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Save the exact spot where the dagger starts (e.g., on a table)
        homePosition = transform.position;
        homeRotation = transform.rotation;
    }

    void FixedUpdate()
    {
        // Only apply physics forces if floating and NOT currently sliding home
        if (isFloating && !isReturning)
        {
            // 1. Basic Lift
            rb.AddForce(Vector3.up * floatForce, ForceMode.Acceleration);

            // 2. Up/Down Bobbing
            float hoverVariation = Mathf.Sin(Time.time * frequency) * amplitude;
            rb.AddForce(Vector3.up * hoverVariation, ForceMode.Acceleration);
            
            // 3. Gentle Tilting
            Quaternion targetRotation = Quaternion.Euler(Mathf.Sin(Time.time) * 5, 0, Mathf.Cos(Time.time) * 5);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime));

            // 4. Subtle Drift
            Vector3 drift = new Vector3(Mathf.Cos(Time.time * 0.5f), 0, Mathf.Sin(Time.time * 0.5f)) * 0.2f; 
            rb.AddForce(drift, ForceMode.Acceleration);
        }
    }

    public void StartFloating()
    {
        // Don't restart if it's already floating or landing
        if (isFloating || isReturning) return;

        isFloating = true;
        isReturning = false;
        rb.useGravity = false;
        rb.isKinematic = false;

        // Start the automatic sequence: Float -> Wait -> Land
        StartCoroutine(FloatSequence());
    }

    IEnumerator FloatSequence()
    {
        // Wait for the duration you set (5s)
        yield return new WaitForSeconds(floatDuration);
        
        // Begin landing phase
        isFloating = false;
        isReturning = true;

        // Turn off physics so we can move it back home without interference
        rb.useGravity = false;
        rb.isKinematic = true;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float elapsed = 0;

        // Smoothly slide back to the home position
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * returnSpeed;
            
            transform.position = Vector3.Lerp(startPos, homePosition, elapsed);
            transform.rotation = Quaternion.Slerp(startRot, homeRotation, elapsed);
            
            yield return null;
        }

        // Snap to exact home values and reset for next time
        transform.position = homePosition;
        transform.rotation = homeRotation;
        
        rb.isKinematic = false;
        rb.useGravity = true;
        isReturning = false;

        Debug.Log("Viento spell finished. Dagger returned.");
    }
}