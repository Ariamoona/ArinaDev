using UnityEngine;

[System.Serializable]
public class Weapon
{
    [SerializeField] private int id;
    [SerializeField] private float damage;
    [SerializeField] private float cooldown;

    public int Id => id;
    public float Damage => damage;
    public float Cooldown => cooldown;

    public Weapon(int id, float damage, float cooldown)
    {
        this.id = id;
        this.damage = damage;
        this.cooldown = cooldown;
    }

    public void ApplyStats(float newDamage, float newCooldown)
    {
        damage = newDamage;
        cooldown = newCooldown;
        Debug.Log($"[Weapon {id}] Stats updated: Damage={damage}, Cooldown={cooldown}");
    }

    public void DebugStats()
    {
        Debug.Log($"[Weapon {id}] Current stats - Damage: {damage}, Cooldown: {cooldown}");
    }
}