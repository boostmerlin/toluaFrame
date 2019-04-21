
using UnityEngine;

public class UnScaleTimeParticle : MonoBehaviour
{
    private ParticleSystem particle;
    private float deltaTime;
    private float lastTime;

    void Start()
    {
        particle = GetComponent<ParticleSystem>();
        lastTime = Time.realtimeSinceStartup;
    }

    void Update()
    {
        if (particle == null) return;
        float now = Time.realtimeSinceStartup;
        deltaTime = now - lastTime;
        lastTime = now;
        if (Time.timeScale < 1e-6)
        {
            particle.Simulate(deltaTime, true, false);
        }
    }
}