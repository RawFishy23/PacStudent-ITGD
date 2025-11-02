using UnityEngine;
using TMPro;

public class FogOfWar : MonoBehaviour
{
    public Transform player;     
    public Transform visionMask;    
    public float baseRadius = 1f;  
    public float pelletIncrease = 0.2f;
    public float shrinkRate = 0.4f;
    public float maxRadius = 8f;    
    
    [Header("UI References")]
    public TextMeshProUGUI visionLabel;  

    [HideInInspector]
    public float currentRadius;

    void Start()
    {
        currentRadius = baseRadius;
        UpdateMask();
    }

    void Update()
    {
        if (player != null)
            visionMask.position = player.position;

        currentRadius -= shrinkRate * Time.deltaTime;
        currentRadius = Mathf.Max(currentRadius, baseRadius);

        UpdateMask();

        if (visionLabel != null)
        {
            if (currentRadius >= (maxRadius-0.1f))
                visionLabel.text = "MAX";
            else
                visionLabel.text = $"{currentRadius:F2}";
        }
    }

    public void EatPellet()
    {
        currentRadius += pelletIncrease;
        currentRadius = Mathf.Min(currentRadius, maxRadius);
        UpdateMask();
    }

    private void UpdateMask()
    {
        visionMask.localScale = new Vector3(currentRadius, currentRadius, 1f);
    }
}
