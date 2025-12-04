using UnityEngine;
using Oculus.Interaction;

/// <summary>
/// Interactive cube for Meta XR that can be grabbed and moved in AR space.
/// When poked/tapped, it triggers the tic-tac-toe minigame UI.
/// Works with Meta XR's Grab And Locate script.
/// </summary>
public class InteractiveCube : MonoBehaviour
{
    [Header("Minigame Settings")]
    [SerializeField] private GameObject ticTacToeUI;
    [SerializeField] private float pokeForceThreshold = 0.5f; // Minimum velocity to trigger game
    
    [Header("Visual Feedback")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material hoveredMaterial;
    [SerializeField] private Material activeMaterial;
    
    private Renderer cubeRenderer;
    private Rigidbody rb;
    private bool isGameActive = false;
    private bool isGrabbed = false;
    
    private void Awake()
    {
        cubeRenderer = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();
        
        // If no rigidbody, add one
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.linearDamping = 5f;
        }
        
        // Make sure UI is hidden at start
        if (ticTacToeUI != null)
        {
            ticTacToeUI.SetActive(false);
        }
        
        // Set normal material if available
        if (normalMaterial != null && cubeRenderer != null)
        {
            cubeRenderer.material = normalMaterial;
        }
    }
    
    // Called when object is grabbed (you can call this from Grab And Locate events)
    public void OnGrabbed()
    {
        isGrabbed = true;
        
        // Change material when grabbed
        if (activeMaterial != null && cubeRenderer != null)
        {
            cubeRenderer.material = activeMaterial;
        }
        
        Debug.Log("Cube grabbed!");
    }
    
    // Called when object is released
    public void OnReleased()
    {
        isGrabbed = false;
        
        // Return to normal material
        if (normalMaterial != null && cubeRenderer != null)
        {
            cubeRenderer.material = normalMaterial;
        }
        
        Debug.Log("Cube released!");
    }
    
    // Called when hand/controller hovers over cube
    public void OnHoverStart()
    {
        if (hoveredMaterial != null && !isGrabbed && cubeRenderer != null)
        {
            cubeRenderer.material = hoveredMaterial;
        }
    }
    
    // Called when hand/controller stops hovering
    public void OnHoverEnd()
    {
        if (normalMaterial != null && !isGrabbed && cubeRenderer != null)
        {
            cubeRenderer.material = normalMaterial;
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Check if poked with sufficient force (finger poke or controller poke)
        if (collision.relativeVelocity.magnitude > pokeForceThreshold)
        {
            // Check if it's from a hand or controller
            if (collision.gameObject.CompareTag("Hand") || 
                collision.gameObject.CompareTag("Controller") ||
                collision.gameObject.layer == LayerMask.NameToLayer("Hands") ||
                collision.gameObject.name.Contains("Hand") ||
                collision.gameObject.name.Contains("Controller"))
            {
                TriggerMinigame();
            }
        }
    }
    
    /// <summary>
    /// Alternative method: Call this directly from a button press or trigger event
    /// Can be called from Meta XR controller button events
    /// </summary>
    public void OnPokeDetected()
    {
        TriggerMinigame();
    }
    
    /// <summary>
    /// Public method that can be called from anywhere to open the game
    /// </summary>
    public void TriggerMinigame()
    {
        if (ticTacToeUI != null && !isGameActive)
        {
            isGameActive = true;
            ticTacToeUI.SetActive(true);
            
            // Position UI in front of the cube
            PositionUIInFrontOfCube();
            
            Debug.Log("Tic-Tac-Toe game activated!");
        }
    }
    
    private void PositionUIInFrontOfCube()
    {
        // Position the UI panel in front of the cube, facing the camera
        Camera mainCamera = Camera.main;
        if (mainCamera != null && ticTacToeUI != null)
        {
            Vector3 directionToCamera = (mainCamera.transform.position - transform.position).normalized;
            ticTacToeUI.transform.position = transform.position + directionToCamera * 0.5f;
            ticTacToeUI.transform.rotation = Quaternion.LookRotation(directionToCamera);
        }
    }
    
    /// <summary>
    /// Call this to close the minigame UI
    /// </summary>
    public void CloseMinigame()
    {
        if (ticTacToeUI != null)
        {
            ticTacToeUI.SetActive(false);
            isGameActive = false;
        }
    }
    
    // Optional: Add a simple button press trigger for testing
    private void Update()
    {
        // For testing: Press 'T' key to trigger game (useful in Unity Editor)
        if (Input.GetKeyDown(KeyCode.T))
        {
            TriggerMinigame();
        }
    }
}