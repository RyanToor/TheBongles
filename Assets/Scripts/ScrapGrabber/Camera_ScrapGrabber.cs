using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_ScrapGrabber : MonoBehaviour
{
    public ParallaxLayerSet[] parallaxLayerSets;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public struct ParallaxLayer
    {
        public float parallaxMultiplier;
        public Sprite sprite;
    }

    public struct ParallaxLayerSet
    {
        public ParallaxLayer[] parallaxLayers;
    }
}
