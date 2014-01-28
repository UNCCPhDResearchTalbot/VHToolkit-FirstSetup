using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class ImportSettingsWindow : EditorWindow {

    #region Constants
    const string SavedWindowPosXKey = "ImportSettingsWindowX";
    const string SavedWindowPosYKey = "ImportSettingsWindowY";
    const string SavedWindowWKey = "ImportSettingsWindowW";
    const string SavedWindowHKey = "ImportSettingsWindowH";
    #endregion

    #region Variables
    // Material Name
    int m_nMaterialNameIndex = 0;
    string[] m_MaterialNameOptions =
    {
        ModelImporterMaterialName.BasedOnMaterialName.ToString(),
        ModelImporterMaterialName.BasedOnModelNameAndMaterialName.ToString(),
        ModelImporterMaterialName.BasedOnTextureName.ToString(),
    };

    // Material Search
    int m_nMaterialSearchIndex = 0;
    string[] m_MaterialSearchOptions =
    {
        ModelImporterMaterialSearch.Everywhere.ToString(),
        ModelImporterMaterialSearch.Local.ToString(),
        ModelImporterMaterialSearch.RecursiveUp.ToString(),
    };

    // Import Priority
    int m_nImportPriority = -10;
    #endregion

    #region Functions
    [MenuItem("VH/Import Settings")]
    static void Init()
    {
        ImportSettingsWindow window = (ImportSettingsWindow)EditorWindow.GetWindow(typeof(ImportSettingsWindow));
        window.autoRepaintOnSceneChange = true;
        window.position = new Rect(PlayerPrefs.GetFloat(SavedWindowPosXKey, 0),
            PlayerPrefs.GetFloat(SavedWindowPosYKey, 0), PlayerPrefs.GetFloat(SavedWindowWKey, 215),
            PlayerPrefs.GetFloat(SavedWindowHKey, 145));

        window.Show();
    }

    // Update is called once per frame
    void OnGUI()
    {
        // Material Name
        EditorGUILayout.LabelField("Material Name");
        m_nMaterialNameIndex = EditorGUILayout.Popup(m_nMaterialNameIndex, m_MaterialNameOptions, GUILayout.Width(200));

        // Material Search
        EditorGUILayout.LabelField("Material Search");
        m_nMaterialSearchIndex = EditorGUILayout.Popup(m_nMaterialSearchIndex, m_MaterialSearchOptions, GUILayout.Width(200));

        // Import Priority
        m_nImportPriority = EditorGUILayout.IntField("Import Priority", m_nImportPriority, GUILayout.Width(200));

        EditorGUILayout.BeginHorizontal("Button", GUILayout.Width(200));
        // Create Import Settings
        if (GUILayout.Button("Create Settings"))
        {
            CreateImportSettingsFile();
        }
        EditorGUILayout.EndHorizontal();
    }

    void OnDestroy()
    {
        SaveLocation();
    }

    void SaveLocation()
    {
        PlayerPrefs.SetFloat(SavedWindowPosXKey, position.x);
        PlayerPrefs.SetFloat(SavedWindowPosYKey, position.y);
        PlayerPrefs.SetFloat(SavedWindowWKey, position.width);
        PlayerPrefs.SetFloat(SavedWindowHKey, position.height);
    }

    void CreateImportSettingsFile()
    {
        string directory = Application.dataPath + "/Editor";
        string path = directory + "/ImportSettings.cs";

        try
        {
            if (!Directory.Exists(directory))
            {
                AssetDatabase.CreateFolder("Assets", "Editor");
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            StreamWriter sw = File.CreateText(path);
            sw.WriteLine("using UnityEngine;");
            sw.WriteLine("using UnityEditor;");
            sw.WriteLine("using System.Collections;");
            sw.WriteLine();
            sw.WriteLine("class MaterialImportSettings : AssetPostprocessor");
            sw.WriteLine("{0}", "{");
            sw.WriteLine("    public override int GetPostprocessOrder() {0} return {1}; {2}", "{", m_nImportPriority, "}");
            sw.WriteLine();
            sw.WriteLine("    void OnPreprocessModel()");
            sw.WriteLine("    {0}", "{");
            sw.WriteLine("        ModelImporter modelImporter = (ModelImporter)assetImporter;");
            sw.WriteLine();
            sw.WriteLine("        // MATERIAL NAME");
            sw.WriteLine("        //     ModelImporterMaterialName.BasedOnMaterialName");
            sw.WriteLine("        //     ModelImporterMaterialName.BasedOnModelNameAndMaterialName");
            sw.WriteLine("        //     ModelImporterMaterialName.BasedOnTextureName");
            sw.WriteLine("        modelImporter.materialName = ModelImporterMaterialName.{0};", m_MaterialNameOptions[m_nMaterialNameIndex]);
            sw.WriteLine();
            sw.WriteLine("        // MATERIAL SEARCH");
            sw.WriteLine("        //     ModelImporterMaterialSearch.Everywhere");
            sw.WriteLine("        //     ModelImporterMaterialSearch.Local");
            sw.WriteLine("        //     ModelImporterMaterialSearch.RecursiveUp");
            sw.WriteLine("        modelImporter.materialSearch = ModelImporterMaterialSearch.{0};", m_MaterialSearchOptions[m_nMaterialSearchIndex]);
            sw.WriteLine("    {0}", "}");
            sw.WriteLine("{0}", "}");
            sw.Close();

            Debug.Log("Saved settings to: " + path);

            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error creating ImportSettings.cs. Exception: " + e.Message);
        }
    }
    #endregion
}
