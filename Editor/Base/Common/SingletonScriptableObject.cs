using UnityEditor;
using UnityEngine;

public class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
{
    private static T instance;

    public static T Get()
    {
        if (instance != null) { return instance; }

        string typeSearchString = $" t:{typeof(T).Name}";
        string[] guids = AssetDatabase.FindAssets(typeSearchString);
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T preferences = AssetDatabase.LoadAssetAtPath<T>(path);
            if (preferences != null)
            {
                instance = preferences;
                return preferences;
            }
        }
        Debug.LogError("未找到资产：" + typeof(T));
        return default(T);
    }
}