using UnityEngine;

[CreateAssetMenu(fileName = "New Bait", menuName = "Items/Bait")]
public class Bait : ScriptableObject
{
    public string name = "Bait";
    public string desc = "Basic bait";
    public int price = 1;
    public int stench = 5; //strength of smell
    public int rarity = 1;
}
