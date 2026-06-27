using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections; 

public class PlayerManager : MonoBehaviour
{
    private Volume postProcessing;
    private Vignette vignette;

    public GameObject bloodPrefab;
    public GameObject bloodSpawnLocation;
    private Rigidbody rb;

    private Coroutine injuryRoutine;

    void Start()
    {
        postProcessing = GameObject.Find("Global Volume").GetComponent<Volume>();
        rb = GetComponent<Rigidbody>();
        StartMovement();

        if (postProcessing.profile.TryGet<Vignette>(out Vignette activeVignette))
        {
            vignette = activeVignette;

            vignette.color.Override(Color.black);
            vignette.intensity.Override(0.5f);
        }
        else
        {
            Debug.LogWarning("Vignette component not found on Global Volume Profile!");
        }
    }

    private void Update()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    public void SpawnBlood()
    {
        
        if (injuryRoutine != null)
        {
            StopCoroutine(injuryRoutine);
        }

       
        injuryRoutine = StartCoroutine(RedFlashRoutine());

        GameObject newBlood = Instantiate(bloodPrefab);
        newBlood.transform.SetParent(bloodSpawnLocation.transform, false);
        newBlood.transform.localPosition = new Vector3(0.6f, -0.17f, 2.87f);
    }

    private IEnumerator RedFlashRoutine()
    {
        if (vignette != null)
        {
            
            vignette.color.Override(Color.red);
            vignette.intensity.Override(0.65f); 

           
            yield return new WaitForSeconds(1.0f);

           
            float elapsedTime = 0f;
            float fadeDuration = 0.5f;

            Color startColor = vignette.color.value;
            float startIntensity = vignette.intensity.value;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float lerpPercent = elapsedTime / fadeDuration;

                
                vignette.color.Override(Color.Lerp(startColor, Color.black, lerpPercent));
                vignette.intensity.Override(Mathf.Lerp(startIntensity, 0.5f, lerpPercent));

                yield return null; 
            }

            
            vignette.color.Override(Color.black);
            vignette.intensity.Override(0.5f);
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    public void StopMovement()
    {
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
    }

    public void StartMovement()
    {
        rb.constraints = RigidbodyConstraints.None;
    }
}