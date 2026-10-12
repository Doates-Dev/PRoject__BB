using UnityEngine;

[CreateAssetMenu(menuName = "Tops/Top Data", fileName = "NewTopData")]
public class TopData : ScriptableObject
{
    [Header("Identity")]
    public string displayName = "Top";

    [Header("Health")]
    public float maxHealth = 100f;

    [Header("Movement")]
    public float moveForce = 15f;
    public float maxSpeed = 8f;

    [Header("Spin")]
    public float startSpin = 200f;
    public float maxSpin = 300f;        // must be above startSpin and any collision boost
    public float spinLossPerSecond = 3f;

    [Header("Combat")]
    public float attack = 1f;    // multiplier on the damage this top deals
    public float defense = 1f;   // higher = takes less damage
}