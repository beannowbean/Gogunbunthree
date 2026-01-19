using UnityEngine;


// [추가] 스킨들 리스트로 저장하기 위해
using System.Collections.Generic; 

public class Customize : MonoBehaviour
{
    [Header("PlayerSkin")]
    // assets/Customize/sweater에 있는 이미지들을 리스트로 다 대입
    // 인스펙터에서 해줘야 됨
    // [추가] 스킨 등록 
    public List<Texture> playerSkins = new List<Texture>();

    // [각 스킨마다 개별 호출] 이 함수로 번호 호출해서 착용시키도록
    public void EquipPlayerSkinNumber(int index)
    {
        if (index >= 0 && index < playerSkins.Count)
        {
            Player.Instance.ChangeSkinTexture(playerSkins[index]);
        }
    }


    /*
     */


    [Header("PlayerRope")]
    // [추가] 로프 머터리얼 등록
    public List<Material> ropeSkins = new List<Material>();

    // [각 로프 머터리얼마다 개별 호출] 이 함수로 번호 호출해서 색깔 바꾸도록
    public void EquipRopeSkinNumber(int index)
    {
        if (index >= 0 && index < ropeSkins.Count)
        {
            Player.Instance.ChangeRopeMaterial(ropeSkins[index]);
        }
    }


    /*
     */


    [Header("PlayerHook")]
    // [추가] 갈고리 머터리얼 등록
    public List<Material> hookSkins = new List<Material>();

    // [각 로프 머터리얼마다 개별 호출] 이 함수로 번호 호출해서 색깔 바꾸도록
    public void EquipHookSkinNumber(int index)
    {
        if (index >= 0 && index < hookSkins.Count)
        {
            Hook.currentSkin = hookSkins[index];
        }
    }


    /*
     */


    [Header("Helicopter")]
    public List<Texture> helicopterSkins = new List<Texture>();

    public void EquipHelicopterSkinNumber(int index)
    {
        if (index >= 0 && index < helicopterSkins.Count)
        {
            Helicopter.currentSkin = helicopterSkins[index];
        }
    }

    /*
     */


    [Header("beanie")]
    public GameObject beaniePrefab; 
    public List<Texture> beanieSkins = new List<Texture>();

    public void EquipBeanie()
    {
        if (Player.Instance != null && beaniePrefab != null)
        {
            Player.Instance.EquipBeanie(beaniePrefab);
        }
    }

    public void UnequipBeanie()
    {
        if (Player.Instance != null)
        {
            Player.Instance.UnequipBeanie();
        }
    }

    public void ChangeBeanieSkinNumber(int index)
    {
        if (index >= 0 && index < beanieSkins.Count)
        {
            Player.Instance.ChangeBeanieSkin(beanieSkins[index]);
        }
    }

    /*
     */


    [Header("bag")]
    public GameObject bagPrefab;
    public List<Texture> bagSkins = new List<Texture>();

    public void EquipBag()
    {
        if (Player.Instance != null && bagPrefab != null)
        {
            Player.Instance.EquipBag(bagPrefab);
        }
    }

    public void UnequipBag()
    {
        if (Player.Instance != null)
        {
            Player.Instance.UnequipBag();
        }
    }

    public void EquipBagSkinNumber(int index)
    {
        if (index >= 0 && index < bagSkins.Count)
        {
            Player.Instance.ChangeBagSkin(bagSkins[index]);
        }
    }
}