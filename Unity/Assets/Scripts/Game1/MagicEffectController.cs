using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicEffectController : MonoBehaviour
{
    private ParticleSystem myParticleSystem;

    public int strengthLevel = 1;

    public float scaleMultiplier = 0.5f;

    public float initStartSpeed = -5.0f;
    public float speedMultiplier = 1.0f;

    public float initRadius = 1.0f;
    public float radiusMultiplier = 0.5f;

    public float initEmission = 15.0f;
    public float emissionMultiplier = 2.0f;

    public float initStartSize = 0.04f;
    public float sizeMultiplier = 0.2f;

    private float initLifetime;

    private ParticleSystem.MainModule mainModule;
    private ParticleSystem.ShapeModule shapeModule;
    private ParticleSystem.EmissionModule emissionModule;


    // Start is called before the first frame update
    void Start()
    {
        myParticleSystem = GetComponentInChildren<ParticleSystem>();
        mainModule = myParticleSystem.main;
        shapeModule = myParticleSystem.shape;
        emissionModule = myParticleSystem.emission;
        shapeModule.shapeType = ParticleSystemShapeType.Circle;

        initLifetime = initRadius / initStartSpeed;
        mainModule.startLifetime = Mathf.Abs(initLifetime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f) * (1.0f + strengthLevel * scaleMultiplier);
        mainModule.startSpeed = initStartSpeed * (1 + strengthLevel * speedMultiplier);
        shapeModule.radius = initRadius * (1 + strengthLevel * radiusMultiplier);
        emissionModule.rateOverTime = initEmission * (1 + strengthLevel * emissionMultiplier);
        mainModule.startSize = initStartSize * (1 + strengthLevel * sizeMultiplier);
        mainModule.startLifetime = Mathf.Abs(shapeModule.radius / mainModule.startSpeed.constant);
    }
}
