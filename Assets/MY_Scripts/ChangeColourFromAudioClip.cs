using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ChangeColourFromAudioClip : MonoBehaviour
{

    public AudioSource source;
    public Vector3 minScale;
    public Vector3 maxScale;
    public AudioDetection detector;

    public float loudnessSensibility = 100;
    public float threshold = 0.1f;

    public Color startColor;
    public Color endColor;
    public float fadetime = 1f;
    private Renderer rend;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material.color = startColor;
    }

    // Update is called once per frame
    void Update()
    {
        float loudness = detector.GetLoudnessFromMicrophone() * loudnessSensibility;
        Vector3 randomVector = new Vector3
            (
            Random.Range(minScale.x, maxScale.x),
            Random.Range(minScale.y, maxScale.y),
            Random.Range(minScale.z, maxScale.z)
            );

        if (loudness < threshold)
            loudness = 0;

        //lerp value from minScale to maxScale
        //transform.localScale = Vector3.Lerp(minScale, randomVector, loudness);

        if (loudness <= threshold)
        {
            rend.material.DOColor(startColor, fadetime);
        }
        else if (loudness > threshold)
        {
            rend.material.DOColor(endColor, fadetime);
            transform.localScale = Vector3.Lerp(randomVector, randomVector, loudness);
        }

    }
}
