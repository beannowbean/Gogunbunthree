using UnityEngine;

public class Heli : MonoBehaviour
{
    public float rotateSpeed = 300.0f;

    // 헬리콥터 생성하기 위한 선언
    public GameObject helicopterPrefab;

    // 헬리 아이템을 먹을 시 나오는 이펙트
    public GameObject heliEffectPrefab;

    public GameObject heliAuroraEffectPrefab; // 가시성 확보를 위한 오로라 이펙트 추가

    private GameObject currentAurora;   //오오라 이펙트 관리

    void OnEnable()
    {
        if(currentAurora == null)
        {
            // 아이템 생성 시 오오라 이펙트 추가
            currentAurora = Instantiate(heliAuroraEffectPrefab, transform.position, Quaternion.Euler(-90f, 0f, 0f));
            currentAurora.transform.SetParent(transform); // 아이템의 자식으로 설정
        }
        currentAurora.SetActive(true); // 활성화
    }

    void Update()
    {
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
        currentAurora.transform.position = transform.position;  // 오오라 이펙트는 회전 방지
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 헬리콥터 이펙트 생성 로직
            if (heliEffectPrefab != null)
            {
                // 아이템 위치에 이펙트 생성
                GameObject effect = Instantiate(heliEffectPrefab, transform.position, Quaternion.identity);

                // 2초 뒤에 이펙트 삭제
                Destroy(effect, 2.0f);
            }

            if (helicopterPrefab != null)
            {
                // 헬리콥터 생성
                Instantiate(helicopterPrefab);
            }

            SFXManager.Instance.Play("HeliSound");  // 헬기 아이템 먹을 시 획득 사운드	
            SFXManager.Instance.Play("Helicopter");    // 헬리콥터 내려오는 사운드
            currentAurora.SetActive(false); // 헬기 아래 이펙트 비활성화

            // 아이템 비활성화
            gameObject.SetActive(false);
        }
    }
}