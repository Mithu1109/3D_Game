using UnityEngine;
using System.Collections;

public class MagicCandle : MonoBehaviour
{
    private ParticleSystem[] flameParticles;
    private Light[] candleLights;

    void Start()
    {
        flameParticles = GetComponentsInChildren<ParticleSystem>(true);
        candleLights = GetComponentsInChildren<Light>(true);

        Debug.Log($"Candle {gameObject.name} initialized with {flameParticles.Length} particles and {candleLights.Length} lights.");
    }

    public void SnuffOut(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(TempOff(duration));
    }

    IEnumerator TempOff(float duration)
    {
    
        foreach (var ps in flameParticles) ps.Stop();
        foreach (var light in candleLights) light.enabled = false;

        yield return new WaitForSeconds(duration);
    }
}