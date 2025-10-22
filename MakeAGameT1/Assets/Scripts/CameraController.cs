using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    private CinemachineCamera cvc;
    private CinemachineBasicMultiChannelPerlin cvcPerlin;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        cvc = GetComponent<CinemachineCamera>();
        cvcPerlin = cvc.GetComponent<CinemachineBasicMultiChannelPerlin>();

    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CameraShake(float intensity)
    {
        cvcPerlin.AmplitudeGain = intensity;
        StartCoroutine(ShakeTimer());
    }
    
    public IEnumerator ShakeTimer()
    {
        yield return new WaitForSeconds(1f);
        cvcPerlin.AmplitudeGain = 0f;
    }
}
