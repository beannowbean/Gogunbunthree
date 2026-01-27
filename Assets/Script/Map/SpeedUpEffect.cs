using System.Collections;
using System.Collections.Generic;
using CartoonFX;
using UnityEngine;

/// <summary>
/// 레벨 업 이펙트 스크립트
/// </summary>
public class SpeedUpEffect : MonoBehaviour
{
    public static SpeedUpEffect Instance;   // 싱글톤 인스턴스

    // 내부 변수
    CFXR_ParticleText particleText; // 이펙트 스크립트
    ParticleSystem ps;  // 이펙트 파티클 시스템

    void Awake()
    {
        // 참조 변수
        Instance= this;
        particleText = GetComponent<CFXR_ParticleText>();
        ps = particleText.GetComponent<ParticleSystem>();
    }
    
    // Speed Up! 이펙트 재생
    public void StartSpeedUpEffect()
    {
        ps.Clear();
        ps.Play(true);
    }
}
