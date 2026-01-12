using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour  // 낮과 밤 주기 스크립트
{
    public float sunsetDuration = 15f;
    public float sunriseDuration = 15f;

    public Light directionalLight;
    private Material skyboxMaterial;

    public bool isNight = false; // 밤인지 확인

    void Start()
    {
        // Skybox 머테리얼 가져오기
        skyboxMaterial = RenderSettings.skybox;

        // 시작 시 낮으로 설정
        StartDay();
    }

    // 낮으로 강제 설정
    void StartDay()
    {
        StopAllCoroutines();
        isNight = false;
        directionalLight.intensity = 1f;
        directionalLight.transform.rotation = Quaternion.Euler(50f, 0f, 0f);
        skyboxMaterial.SetFloat("_Exposure", 1f);
        skyboxMaterial.SetFloat("_Blend", 0f);
    }

    // 낮에서 밤으로 변경
    public void DayToNight()
    {
        StopAllCoroutines();
        StartCoroutine(TimeChange(50f, 230f, sunsetDuration, 1f, 0f, 1f, 0.8f, 0f, 1f));
        isNight = true;
    }

    // 밤에서 낮으로 변경
    public void NightToDay()
    {
        StopAllCoroutines();
        StartCoroutine(TimeChange(230f, 410f, sunriseDuration, 0f, 1f, 0.8f, 1f, 1f, 0f));
        isNight = false;

    }

    // 시간에 따른 배경 변경 (light, skybox, shader blend)
    IEnumerator TimeChange(float startAngle, float endAngle, float duration,
        float startIntensity, float endIntensity, float startExposure, float endExposure, float startBlend, float endBlend)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float angle = Mathf.Lerp(startAngle, endAngle, t);
            float intensity = Mathf.Lerp(startIntensity, endIntensity, t);
            float exposure = Mathf.Lerp(startExposure, endExposure, t);
            float blend = Mathf.Lerp(startBlend, endBlend, t);

            directionalLight.transform.rotation = Quaternion.Euler(angle, 0f, 0f);
            directionalLight.intensity = intensity;
            skyboxMaterial.SetFloat("_Exposure", exposure);
            skyboxMaterial.SetFloat("_Blend", blend);

            elapsed += Time.deltaTime;
            yield return null;
        }
        directionalLight.transform.rotation = Quaternion.Euler(endAngle, 0f, 0f);
        directionalLight.intensity = endIntensity;
        skyboxMaterial.SetFloat("_Exposure", endExposure);
        skyboxMaterial.SetFloat("_Blend", endBlend);
    }
}
