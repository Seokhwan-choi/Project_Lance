using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Text;
using ExcelToObject;
using CodeStage.AntiCheat.Storage;
using TMPro;
using Assets.SimpleZip;

namespace Lance
{
    public class EditorUtil : EditorWindow
    {
        [MenuItem("Lance/���� �����")]
        static void ResetAccount()
        {
            ObscuredPrefs.DeleteKey("3094kdfjskd");

            UnityEngine.Debug.Log("���� ������");
        }

        const string SavedDataSheetPath = "Assets/Resources/DataSheet/SavedDataSheet.asset";

        [MenuItem("Lance/������ ��Ʈ TextAsset ����")]
        static void DataSheetToTextAsset()
        {
            // �ռ� ����� �����ʹ� �׳� ��������
            Object savedTextAsset = AssetDatabase.LoadAssetAtPath<Object>(SavedDataSheetPath);
            if (savedTextAsset != null)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(savedTextAsset));

                AssetDatabase.Refresh();
            }

            GameData gameData = new GameData();

            IEnumerable<byte[]> dataBytes = GameDataReader.GetGameDataBytes("DataSheet");

            var tableList = new TableList(dataBytes);

            tableList.MapInto(gameData);

            string json = JsonUtil.ToJsonNewton(gameData);

            //Debug.Log(json);

            string base64 = JsonUtil.JsonToBase64String(json);

            //Debug.Log(base64);

            TextAsset text = new TextAsset(base64);

            AssetDatabase.CreateAsset(text, SavedDataSheetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            TextAsset text2 = Resources.Load("DataSheet/SavedDataSheet") as TextAsset;
            if (text2 != null)
            {
                Debug.Log("���������� �Ľ� ����");

                gameData = JsonUtil.Base64FromJsonNewton<GameData>(text2.text);

                File.WriteAllText("SaveDataSheet/DataSheet.txt", base64);
            }
        }

        [MenuItem("Lance/ModifyPrefab")]
        static void Init()
        {
            EditorUtil window = (EditorUtil)EditorWindow.GetWindow(typeof(EditorUtil));

            window.Show();
        }

        void OnGUI()
        {
            //if (GUILayout.Button("ChangeFont Maplestory Bold SDF"))
            //{
            //    TMP_FontAsset newFont = Resources.Load<TMP_FontAsset>("Fonts/Maplestory Bold SDF - Outline");
            //    Material newFontMaterial = Resources.Load<Material>("Fonts/Maplestory Bold SDF - NoOutline");

            //    // Resources ���� �� ��� �������� �ε��մϴ�.
            //    string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Resources/UI" });
            //    foreach (string prefabPath in prefabPaths)
            //    {
            //        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabPath));
            //        GameObject newPrefab = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            //        // ��� TMPro ��Ʈ �� ��Ʈ ���͸��� ����
            //        TextMeshProUGUI[] textComponents = newPrefab.GetComponentsInChildren<TextMeshProUGUI>(true);
            //        foreach (TextMeshProUGUI textComponent in textComponents)
            //        {
            //            textComponent.font = newFont;
            //            textComponent.fontSharedMaterial = newFontMaterial;
            //        }

            //        // ��θ� �о����
            //        string newPrefabPath = AssetDatabase.GenerateUniqueAssetPath(AssetDatabase.GetAssetPath(prefab));
            //        // ���� �������� ��������
            //        AssetDatabase.DeleteAsset(prefabPath);
            //        // ������ �������� �����ϰ�
            //        PrefabUtility.SaveAsPrefabAsset(newPrefab, newPrefabPath);

            //        DestroyImmediate(newPrefab);

            //        AssetDatabase.ImportAsset(newPrefabPath, ImportAssetOptions.ForceUpdate);

            //        AssetDatabase.Refresh();
            //    }
            //}
        }
    }
}