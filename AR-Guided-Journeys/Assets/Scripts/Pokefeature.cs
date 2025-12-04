using UnityEngine;

/// <summary>
/// Simple poke detector for Meta XR.
/// Detects when the user's hand or controller touches the cube.
/// Attach this alongside the InteractiveCube component.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class SimplePokeDetector : MonoBehaviour
{
    [Header("Poke Settings")]
    [SerializeField] private float pokeDistance = 0.15f; // Detection radius
    [SerializeField] private float cooldownTime = 1f; // Prevent rapid re-triggering
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject pokeIndicator; // Optional visual feedback
    
    private InteractiveCube interactiveCube;
    private SphereCollider pokeCollider;
    private float lastPokeTime = 0f;
    
    private void Awake()
    {
        interactiveCube = GetComponent<InteractiveCube>();
        
        // Setup poke collider
        pokeCollider = GetComponent<SphereCollider>();
        pokeCollider.isTrigger = true;
        pokeCollider.radius = pokeDistance;
        
        if (pokeIndicator != null)
        {
            pokeIndicator.SetActive(false);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if cooldown has passed
        if (Time.time - lastPokeTime < cooldownTime)
            return;
        
        // Check if it's a hand or controller
        bool isHand = other.CompareTag("Hand") || 
                      other.gameObject.layer == LayerMask.NameToLayer("Hands") ||
                      other.name.Contains("Hand") ||
                      other.name.Contains("Index"); // Meta hand tracking uses Index finger
        
        bool isController = other.CompareTag("Controller") || 
                           other.name.Contains("Controller");
        
        if (isHand || isController)
        {
            OnPoke();
        }
    }
    
    private void OnPoke()
    {
        lastPokeTime = Time.time;
        
        // Visual feedback
        if (pokeIndicator != null)
        {
            pokeIndicator.SetActive(true);
            Invoke(nameof(HidePokeIndicator), 0.3f);
        }
        
        // Trigger the minigame
        if (interactiveCube != null)
        {
            interactiveCube.OnPokeDetected();
        }
        
        Debug.Log("Poke detected!");
    }
    
    private void HidePokeIndicator()
    {
        if (pokeIndicator != null)
        {
            pokeIndicator.SetActive(false);
        }
    }
}