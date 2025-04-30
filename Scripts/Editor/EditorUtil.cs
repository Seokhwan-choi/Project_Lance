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
        [MenuItem("Lance/계정 지우기")]
        static void ResetAccount()
        {
            ObscuredPrefs.DeleteKey("3094kdfjskd");

            UnityEngine.Debug.Log("계정 삭제됨");
        }

        const string SavedDataSheetPath = "Assets/Resources/DataSheet/SavedDataSheet.asset";

        [MenuItem("Lance/데이터 시트 TextAsset 저장")]
        static void DataSheetToTextAsset()
        {
            // 앞서 저장된 데이터는 그냥 지워주자
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
                Debug.Log("정상적으로 파싱 성공");

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

            //    // Resources 폴더 내 모든 프리팹을 로드합니다.
            //    string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Resources/UI" });
            //    foreach (string prefabPath in prefabPaths)
            //    {
            //        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabPath));
            //        GameObject newPrefab = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            //        // 모든 TMPro 폰트 및 폰트 매터리얼 변경
            //        TextMeshProUGUI[] textComponents = newPrefab.GetComponentsInChildren<TextMeshProUGUI>(true);
            //        foreach (TextMeshProUGUI textComponent in textComponents)
            //        {
            //            textComponent.font = newFont;
            //            textComponent.fontSharedMaterial = newFontMaterial;
            //        }

            //        // 경로를 읽어오자
            //        string newPrefabPath = AssetDatabase.GenerateUniqueAssetPath(AssetDatabase.GetAssetPath(prefab));
            //        // 기존 프리팹은 지워주자
            //        AssetDatabase.DeleteAsset(prefabPath);
            //        // 생성된 프리팹을 저장하고
            //        PrefabUtility.SaveAsPrefabAsset(newPrefab, newPrefabPath);

            //        DestroyImmediate(newPrefab);

            //        AssetDatabase.ImportAsset(newPrefabPath, ImportAssetOptions.ForceUpdate);

            //        AssetDatabase.Refresh();
            //    }
            //}
        }
    }
}