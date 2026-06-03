#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using PlinkoPinball.Gameplay.Upgrade;

public static class UpgradeCsvImporter
{
    private const string CsvPath = "Assets/Data/PlinkoPinball_UpgradeSample.csv";
    private const string OutputFolder = "Assets/Data";
    private const string DatabasePath = "Assets/Data/UpgradeDatabase.asset";

    [MenuItem("PlinkoPinball/Upgrades/Import CSV")]
    public static void Import()
    {
        EnsureFolder("Assets/Data");

        if (!File.Exists(CsvPath))
        {
            Debug.LogError($"[UpgradeCsvImporter] CSV not found: {CsvPath}");
            return;
        }

        string[] lines = File.ReadAllLines(CsvPath);

        if (lines.Length <= 1)
        {
            Debug.LogError("[UpgradeCsvImporter] CSV is empty.");
            return;
        }

        Dictionary<string, UpgradeDefinition> created = new();
        Dictionary<UpgradeDefinition, string> pendingPrerequisites = new();

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] columns = SplitCsvLine(line);

            string upgradeId = columns[0];
            string displayName = columns[1];
            int baseCost = int.Parse(columns[3]);
            float costMultiplier = float.Parse(columns[4]);
            int maxLevel = int.Parse(columns[5]);
            UpgradeType upgradeType = System.Enum.Parse<UpgradeType>(columns[6]);
            float valuePerLevel = float.Parse(columns[7]);
            string prerequisiteId = columns[8];
            int gridX = int.Parse(columns[9]);
            int gridY = int.Parse(columns[10]);
            string description = columns.Length > 11 ? columns[11] : string.Empty;

            UpgradeDefinition definition = ScriptableObject.CreateInstance<UpgradeDefinition>();

            definition.upgradeId = upgradeId;
            definition.displayName = displayName;
            definition.description = description;
            definition.baseCost = baseCost;
            definition.costMultiplier = costMultiplier;
            definition.maxLevel = maxLevel;
            definition.upgradeType = upgradeType;
            definition.valuePerLevel = valuePerLevel;
            definition.gridPosition = new Vector2Int(gridX, gridY);

            string assetPath = $"{OutputFolder}/{upgradeId}.asset";

            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.CreateAsset(definition, assetPath);

            created.Add(upgradeId, definition);
            pendingPrerequisites.Add(definition, prerequisiteId);
        }

        foreach (var pair in pendingPrerequisites)
        {
            UpgradeDefinition definition = pair.Key;
            string prerequisiteId = pair.Value;

            if (string.IsNullOrWhiteSpace(prerequisiteId))
            {
                definition.prerequisites = new UpgradeDefinition[0];
                continue;
            }

            string[] prerequisiteIds = prerequisiteId.Split('|');

            List<UpgradeDefinition> prerequisites = new();

            foreach (string id in prerequisiteIds)
            {
                string trimmedId = id.Trim();

                if (created.TryGetValue(trimmedId, out UpgradeDefinition prerequisite))
                {
                    prerequisites.Add(prerequisite);
                }
                else
                {
                    Debug.LogWarning($"[UpgradeCsvImporter] Missing prerequisite '{trimmedId}' for '{definition.upgradeId}'");
                }
            }

            definition.prerequisites = prerequisites.ToArray();
            EditorUtility.SetDirty(definition);
        }

        UpgradeDatabase database = AssetDatabase.LoadAssetAtPath<UpgradeDatabase>(DatabasePath);

        if (database == null)
        {
            database = ScriptableObject.CreateInstance<UpgradeDatabase>();
            AssetDatabase.CreateAsset(database, DatabasePath);
        }

        database.upgrades = new List<UpgradeDefinition>(created.Values).ToArray();
        EditorUtility.SetDirty(database);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[UpgradeCsvImporter] Imported {created.Count} upgrades.");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        string folder = Path.GetFileName(path);

        AssetDatabase.CreateFolder(parent, folder);
    }

    private static string[] SplitCsvLine(string line)
    {
        return line.Split(',');
    }
}
#endif