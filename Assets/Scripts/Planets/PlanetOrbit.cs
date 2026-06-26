using UnityEngine;

public class PlanetOrbit : MonoBehaviour
{
    [Header("Orbit Settings")]
    public Transform sunTarget;          
    public float orbitalPeriodInDays;    

    
    public static float timeScaleMultiplier = 10f;

    void Start()
    {
        if (sunTarget == null)
        {
            
            GameObject sun = GameObject.Find("Sun Sphere");
            if (sun != null) sunTarget = sun.transform;
        }
    }

    void Update()
    {
        if (sunTarget == null) return;

        
        float speed = (360f / orbitalPeriodInDays) * timeScaleMultiplier;

        
        transform.RotateAround(sunTarget.position, Vector3.up, speed * Time.deltaTime);
    }
}