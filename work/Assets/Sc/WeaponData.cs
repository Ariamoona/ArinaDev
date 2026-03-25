using System;

[System.Serializable]
public class WeaponData
{
    public int id;
    public float damage;
    public float cooldown;

    public WeaponData(int id, float damage, float cooldown)
    {
        this.id = id;
        this.damage = damage;
        this.cooldown = cooldown;
    }

    public bool IsValid()
    {
        return damage >= 0 && cooldown > 0;
    }

    public string GetValidationError()
    {
        if (damage < 0)
            return $"Damage ({damage}) is negative";
        if (cooldown <= 0)
            return $"Cooldown ({cooldown}) is not positive";
        return null;
    }
}