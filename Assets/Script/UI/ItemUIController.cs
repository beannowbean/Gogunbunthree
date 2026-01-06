using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * 아이템에 닿였을 때 아래의 함수 호출 필요
 * ActivateNextAvailableItem(float duration, Sprite icon = null)
 * duration:    아이템 지속 시간 (초 단위)
 * icon:        아이템 아이콘 (Sprite 형식, 기본값 null)
 */

public class ItemUIController : MonoBehaviour
{
    public static ItemUIController Instance;

    [System.Serializable]
    public class ItemSlot
    {
        public GameObject rootObject;   // Item1~Item4 루트 오브젝트
        public Image itemIcon;          // 아이콘
        public Slider durationSlider;   // 슬라이더

        // 인스펙터에서는 안 보이지만 코드로 제어할 변수들
        [HideInInspector]
        public float maxDuration;
        [HideInInspector]
        public float currentDuration;
        [HideInInspector]
        public bool isActive;

        // 슬롯 초기화 (숨기기)
        public void Initialize()
        {
            if (rootObject != null) rootObject.SetActive(false);
            isActive = false;
        }

        // 매 프레임 실행될 로직 (시간 감소 및 슬라이더 갱신)
        public void UpdateSlot()
        {
            if (!isActive) return;

            currentDuration -= Time.deltaTime;

            if (durationSlider != null)
            {
                durationSlider.value = currentDuration / maxDuration;
            }

            if (currentDuration <= 0)
            {
                Deactivate();
            }
        }

        // 다른 슬롯의 데이터를 복사해오기
        public void CopyDataFrom(ItemSlot otherSlot)
        {
            // 1. 데이터 복사
            this.maxDuration = otherSlot.maxDuration;
            this.currentDuration = otherSlot.currentDuration;
            this.isActive = otherSlot.isActive;

            // 2. UI 갱신 (아이콘 복사)
            if (this.itemIcon != null && otherSlot.itemIcon != null)
            {
                this.itemIcon.sprite = otherSlot.itemIcon.sprite;
            }

            // 3. 켜져있는 상태라면 UI도 켜기
            if (this.isActive && rootObject != null)
            {
                rootObject.SetActive(true);
            }
            else if (!this.isActive && rootObject != null)
            {
                rootObject.SetActive(false);
            }
        }

        // 아이템 활성화
        public void Activate(float duration, Sprite iconSprite = null)
        {
            maxDuration = duration;
            currentDuration = duration;
            isActive = true;

            if (itemIcon != null && iconSprite != null)
                itemIcon.sprite = iconSprite;

            if (durationSlider != null)
                durationSlider.value = 1f;

            if (rootObject != null)
                rootObject.SetActive(true);
        }

        // 아이템 비활성화
        public void Deactivate()
        {
            isActive = false;
            if (rootObject != null) rootObject.SetActive(false);
        }
    }

    // ItemSlot 클래스를 배열로 선언
    public ItemSlot[] itemSlots;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 시작 시 모든 슬롯 초기화 (숨기기)
        foreach (var slot in itemSlots)
        {
            slot.Initialize();
        }
    }

    void Update()
    {
        // 배열을 순회하며 업데이트
        for (int i = 0; i < itemSlots.Length; i++)
        {
            // 활성화된 슬롯만 시간 감소
            if (itemSlots[i].isActive)
            {
                itemSlots[i].currentDuration -= Time.deltaTime;

                // 슬라이더 갱신
                if (itemSlots[i].durationSlider != null)
                {
                    itemSlots[i].durationSlider.value = itemSlots[i].currentDuration / itemSlots[i].maxDuration;
                }

                // 시간이 다 된 슬롯은 제거 및 당기기
                if (itemSlots[i].currentDuration <= 0)
                {
                    RemoveAndShift(i);
                    // 당기고 나면 현재 i번째 슬롯에 뒷녀석이 들어왔으므로,
                    // 다음 프레임에 다시 i번부터 검사하도록(혹은 꼬임 방지) i를 하나 줄임
                    i--;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.T)) // 테스트용: T 키를 누르면 아이템 활성화
        {
            ActivateNextAvailableItem(5f);
        }
    }

    // 특정 인덱스의 슬롯을 제거하고 뒤의 슬롯들을 앞으로 당기는 함수
    public void RemoveAndShift(int indexToRemove)
    {
        // 1. 지울 위치(indexToRemove)부터 끝 바로 전까지 반복
        for (int i = indexToRemove; i < itemSlots.Length - 1; i++)
        {
            // 현재 슬롯(i)에 뒷 슬롯(i+1)의 데이터를 덮어씌움
            itemSlots[i].CopyDataFrom(itemSlots[i + 1]);
        }

        // 2. 맨 마지막 슬롯은 이제 데이터가 앞으로 갔으니 비활성화 (초기화)
        itemSlots[itemSlots.Length - 1].Deactivate();
    }

    // --- 외부에서 호출할 함수들 ---

    /// <summary>
    /// 비어있는 슬롯을 자동으로 찾아 아이템을 표시합니다.
    /// 같은 아이템(아이콘이 같은 경우)이 이미 있으면 지속시간을 초기화합니다.
    /// </summary>
    public void ActivateNextAvailableItem(float duration, Sprite icon = null)
    {
        // 먼저 같은 아이템이 이미 활성화되어 있는지 확인
        foreach (var slot in itemSlots)
        {
            if (slot.isActive && slot.itemIcon != null && slot.itemIcon.sprite == icon && icon != null)
            {
                // 같은 아이템이 있으면 지속시간만 초기화
                slot.maxDuration = duration;
                slot.currentDuration = duration;
                if (slot.durationSlider != null)
                    slot.durationSlider.value = 1f;
                return;
            }
        }

        // 같은 아이템이 없으면 빈 슬롯을 찾아서 새로 활성화
        foreach (var slot in itemSlots)
        {
            // 활성화되지 않은(비어있는) 슬롯을 찾으면
            if (!slot.isActive)
            {
                slot.Activate(duration, icon);
                return; // 하나 켰으니 종료
            }
        }
    }
}
