using UnityEngine;

public class Heli : MonoBehaviour
{
    public float rotateSpeed = 300.0f;

    [Header("Skin Settings")]
    public Renderer[] itemPartsRenderers; 
 

    [Header("Spawn Settings")]
    public GameObject helicopterPrefab;
    public GameObject heliEffectPrefab;
    public GameObject heliAuroraEffectPrefab;

    private GameObject currentAurora;

    void OnEnable()
    {
        if (Helicopter.currentSkin != null)
        {
            ApplyItemSkin(Helicopter.currentSkin);
        }

        // 오로라 이펙트 로직
        if (currentAurora == null)
        {
            if (heliAuroraEffectPrefab != null)
            {
                currentAurora = Instantiate(heliAuroraEffectPrefab, transform.position, Quaternion.Euler(-90f, 0f, 0f));
                currentAurora.transform.SetParent(transform);
            }
        }

        if (currentAurora != null)
        {
            currentAurora.SetActive(true);
        }
    }

    void Update()
    {
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);

        if (currentAurora != null)
        {
            currentAurora.transform.position = transform.position;
        }
    }

    // 스킨 적용 함수
    void ApplyItemSkin(Texture newTexture)
    {
        if (itemPartsRenderers != null)
        {
            foreach (Renderer r in itemPartsRenderers)
            {
                if (r != null)
                {
                    Material[] mats = r.materials;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        mats[i].mainTexture = newTexture;
                    }
                    r.materials = mats;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (heliEffectPrefab != null)
            {
                GameObject effect = Instantiate(heliEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2.0f);
            }

            if (helicopterPrefab != null)
            {
                Instantiate(helicopterPrefab);
            }

            SFXManager.Instance.Play("HeliSound");
            SFXManager.Instance.Play("Helicopter");

            if (currentAurora != null)
            {
                currentAurora.SetActive(false);
            }

            gameObject.SetActive(false);
        }
    }
}