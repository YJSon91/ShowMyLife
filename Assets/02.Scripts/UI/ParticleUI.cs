using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleUI : UiBase
{
    [SerializeField] private ParticleType _particleType;
    public ParticleType ParticleType => _particleType;

    private ParticleSystem _particleSystem;
    
    public override void Init()
    {        
        _particleSystem = GetComponent<ParticleSystem>();
    }

    public override void Show(bool show)
    {
        if (show)
        {
            gameObject.SetActive(true);
            _particleSystem.Play();
        }
        else
        {
            _particleSystem.Stop();
            gameObject.SetActive(false);
        }
    }
}
