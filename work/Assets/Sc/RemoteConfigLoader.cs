using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class RemoteConfigLoader : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string configUrl = "https://your-url.com/config.csv"; 
    [SerializeField] private ConfigType configType = ConfigType.CSV;
    [SerializeField] private bool loadOnStart = true;

    [Header("Default Values")]
    [SerializeField]
    private List<WeaponData> defaultWeapons = new List<WeaponData>()
    {
        new WeaponData(1, 25f, 0.5f),
        new WeaponData(2, 50f, 1.0f),
        new WeaponData(3, 100f, 2.0f)
    };

    [Header("References")]
    [SerializeField] private List<Weapon> weapons = new List<Weapon>();

    private string localFilePath;
    private List<WeaponData> currentConfig = new List<WeaponData>();

    public enum ConfigType
    {
        CSV,
        JSON
    }

    public event Action<List<WeaponData>> OnConfigLoaded;
    public event Action<string> OnConfigError;

    private void Awake()
    {
        localFilePath = Path.Combine(Application.persistentDataPath, "weapon_config.json");
        Debug.Log($"Local config path: {localFilePath}");
    }

    private void Start()
    {
        if (loadOnStart)
        {
            LoadConfig();
        }
    }

    public void LoadConfig()
    {
        StartCoroutine(LoadConfigCoroutine());
    }

    private IEnumerator LoadConfigCoroutine()
    {
        Debug.Log("Starting remote config load...");

        using (UnityWebRequest request = UnityWebRequest.Get(configUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Remote config downloaded successfully");
                string downloadedData = request.downloadHandler.text;

                List<WeaponData> parsedData = ParseConfig(downloadedData);

                if (ValidateConfig(parsedData))
                {
                    currentConfig = parsedData;
                    SaveLocalCopy();
                    ApplyConfigToWeapons();
                    OnConfigLoaded?.Invoke(currentConfig);
                    Debug.Log("Remote config applied successfully");
                }
                else
                {
                    Debug.LogError("Remote config validation failed");
                    LoadLocalCopy();
                }
            }
            else
            {
                Debug.LogError($"Failed to download config: {request.error}");
                LoadLocalCopy();
            }
        }
    }

    private List<WeaponData> ParseConfig(string rawData)
    {
        List<WeaponData> result = new List<WeaponData>();

        try
        {
            if (configType == ConfigType.CSV)
            {
                result = ParseCSV(rawData);
            }
            else
            {
                result = ParseJSON(rawData);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Parse error: {ex.Message}");
            OnConfigError?.Invoke($"Parse error: {ex.Message}");
        }

        return result;
    }

    private List<WeaponData> ParseCSV(string csvContent)
    {
        List<WeaponData> weapons = new List<WeaponData>();

        string[] lines = csvContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 2)
        {
            Debug.LogError("CSV file has no data rows");
            return weapons;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            string[] values = line.Split(',');

            if (values.Length >= 3)
            {
                try
                {
                    int id = int.Parse(values[0].Trim());
                    float damage = float.Parse(values[1].Trim());
                    float cooldown = float.Parse(values[2].Trim());

                    weapons.Add(new WeaponData(id, damage, cooldown));
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to parse line {i}: {line}. Error: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"Invalid CSV line {i}: {line}");
            }
        }

        Debug.Log($"Parsed {weapons.Count} weapons from CSV");
        return weapons;
    }

    private List<WeaponData> ParseJSON(string jsonContent)
    {
        WeaponData[] weaponsArray = JsonUtility.FromJson<WeaponDataWrapper>(jsonContent)?.weapons;

        if (weaponsArray == null)
        {
            try
            {
                string wrappedJson = "{\"weapons\":" + jsonContent + "}";
                weaponsArray = JsonUtility.FromJson<WeaponDataWrapper>(wrappedJson)?.weapons;
            }
            catch
            {
                Debug.LogError("Failed to parse JSON array");
            }
        }

        List<WeaponData> weapons = weaponsArray?.ToList() ?? new List<WeaponData>();
        Debug.Log($"Parsed {weapons.Count} weapons from JSON");
        return weapons;
    }

    [System.Serializable]
    private class WeaponDataWrapper
    {
        public WeaponData[] weapons;
    }

    private bool ValidateConfig(List<WeaponData> config)
    {
        if (config == null || config.Count == 0)
        {
            Debug.LogError("Config is empty");
            return false;
        }

        bool isValid = true;

        foreach (var weapon in config)
        {
            if (!weapon.IsValid())
            {
                Debug.LogError($"Invalid weapon data: ID={weapon.id}, {weapon.GetValidationError()}");
                isValid = false;
            }
        }

        return isValid;
    }

    private void SaveLocalCopy()
    {
        try
        {
            WeaponSaveData saveData = new WeaponSaveData
            {
                weapons = currentConfig.ToArray()
            };

            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(localFilePath, json);
            Debug.Log($"Local copy saved to {localFilePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save local copy: {ex.Message}");
        }
    }

    private void LoadLocalCopy()
    {
        if (File.Exists(localFilePath))
        {
            try
            {
                string json = File.ReadAllText(localFilePath);
                WeaponSaveData saveData = JsonUtility.FromJson<WeaponSaveData>(json);

                if (saveData?.weapons != null)
                {
                    currentConfig = saveData.weapons.ToList();

                    if (ValidateConfig(currentConfig))
                    {
                        ApplyConfigToWeapons();
                        OnConfigLoaded?.Invoke(currentConfig);
                        Debug.Log("Local config loaded successfully");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load local copy: {ex.Message}");
            }
        }

        Debug.LogWarning("No valid local config found. Using default values.");
        UseDefaultConfig();
    }

    private void UseDefaultConfig()
    {
        currentConfig = new List<WeaponData>(defaultWeapons);

        if (!ValidateConfig(currentConfig))
        {
            Debug.LogError("Default config is also invalid! Check your default values.");
        }

        ApplyConfigToWeapons();
        OnConfigLoaded?.Invoke(currentConfig);
        OnConfigError?.Invoke("Using default config due to errors");
    }

    private void ApplyConfigToWeapons()
    {
        if (weapons == null || weapons.Count == 0)
        {
            Debug.LogWarning("No weapons assigned to apply config");
            return;
        }

        foreach (var weapon in weapons)
        {
            var weaponData = currentConfig.Find(w => w.id == weapon.Id);

            if (weaponData != null)
            {
                weapon.ApplyStats(weaponData.damage, weaponData.cooldown);
            }
            else
            {
                Debug.LogWarning($"No config found for weapon ID {weapon.Id}");
            }
        }
    }

    public WeaponData GetWeaponConfig(int id)
    {
        return currentConfig?.Find(w => w.id == id);
    }

    public List<WeaponData> GetAllConfigs()
    {
        return new List<WeaponData>(currentConfig);
    }

    [System.Serializable]
    private class WeaponSaveData
    {
        public WeaponData[] weapons;
    }
}

[System.Serializable]
public class SerializableWeaponData
{
    public int id;
    public float damage;
    public float cooldown;

    public SerializableWeaponData(WeaponData data)
    {
        id = data.id;
        damage = data.damage;
        cooldown = data.cooldown;
    }

    public WeaponData ToWeaponData()
    {
        return new WeaponData(id, damage, cooldown);
    }
}