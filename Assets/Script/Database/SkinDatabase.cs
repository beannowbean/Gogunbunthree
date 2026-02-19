using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkinDatabase", menuName = "Game Data/Skin Database")]
public class SkinDatabase : ScriptableObject
{
    [Header("Player Skins")]
    public List<Sprite> playerSkins;

    [Header("Beanie Skins")]
    public List<Sprite> beanieSkins;

    [Header("Bag Skins")]
    public List<Sprite> bagSkins;

    [Header("Hook Skins")]
    public List<Sprite> hookSkins;

    [Header("Helicopter Skins")]
    public List<Sprite> helicopterSkins;


}