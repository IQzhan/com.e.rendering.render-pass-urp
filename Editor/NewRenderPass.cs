using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using UnityEngine;

namespace E.Rendering.Editor
{
    public class NewRenderPass : EditorWindow
    {
        private const string Key = "03A2E43A95E8D9B1";

        private const string SearchFilter_Templete = "t:TextAsset PassTemplete_" + Key;

        private const string SearchFilter_DataTemplete = "t:TextAsset PassDataTemplete_" + Key;

        private const string ComponentMenuPathReplaceStr = "#COMPONENT_MENU_PATH#";

        private const string PassNameReplaceStr = "#PASS_NAME#";

        private const string NamespaceStartReplaceStr = "#NAMESPACE_START#";

        private const string NamespaceEndReplaceStr = "#NAMESPACE_END#";

        private const string DefaultComponentMenuPath = "Custom/New Render Pass";

        private const string DefaultPassName = "New Render Pass";

        private string componentMenuPath;

        private string passName;

        [MenuItem("Assets/Create/Render Pass", false, 201)]
        private static void OpenWindow()
        {
            float width = 400;
            float height = 65;
            Rect rect = new Rect(Screen.width * 0.5f - width, Screen.height * 0.5f - height, width, height);
            NewRenderPass window = GetWindowWithRect<NewRenderPass>(rect, true, DefaultPassName);
            window.Reset();
            window.Show();
        }

        private void Reset()
        {
            componentMenuPath = DefaultComponentMenuPath;
            passName = DefaultPassName;
            PlayerPrefs.DeleteKey(Key);
        }

        private void OnGUI()
        {
            componentMenuPath = EditorGUILayout.TextField("Volume Component Menu Path", componentMenuPath);
            passName = EditorGUILayout.TextField("Render Pass Name", passName);
            if (GUILayout.Button("Create"))
            {
                if (CheckValues())
                {
                    try
                    {
                        CreateVolume();
                        Close();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                else
                {
                    Debug.LogError("Render Pass Name format error");
                }
            }
        }

        private bool CheckValues()
        {
            Regex regex0 = new Regex(@"\b[a-z]");
            Regex regex1 = new Regex(@" +");
            componentMenuPath = regex0.Replace(componentMenuPath, (Match match) => { return match.Value.ToUpper(); });
            componentMenuPath = regex1.Replace(componentMenuPath.Trim(), " ");
            passName = regex0.Replace(passName, (Match match) => { return match.Value.ToUpper(); });
            passName = regex1.Replace(passName, string.Empty);
            return !string.IsNullOrWhiteSpace(componentMenuPath)
                && !string.IsNullOrWhiteSpace(passName);
        }

        private void CreateVolume()
        {
            if (TryGetCurrentFolderPath(out string currPath))
            {
                ReadTemplete(out string volumeText, out string volumeDataText);
                ReplaceText(currPath, ref volumeText, ref volumeDataText);
                CraeteFiles(currPath, volumeText, volumeDataText);
                Debug.Log("Create render pass <" + passName + "> succeed");
            }
            else
            {
                throw new System.IO.DirectoryNotFoundException("No folder selected");
            }
        }

        private void ReadTemplete(out string volumeText, out string volumeDataText)
        {
            string volumeTempletePath =
                AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(SearchFilter_Templete)[0]);
            string volumeDataTempletePath =
                AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(SearchFilter_DataTemplete)[0]);
            volumeText = AssetDatabase.LoadAssetAtPath<TextAsset>(volumeTempletePath).text;
            volumeDataText = AssetDatabase.LoadAssetAtPath<TextAsset>(volumeDataTempletePath).text;
        }

        private void ReplaceText(in string currPath, ref string volumeText, ref string volumeDataText)
        {
            string namespaceStr = CompilationPipeline.GetAssemblyRootNamespaceFromScriptPath(currPath + "/");
            ReplaceText(ref volumeText, namespaceStr);
            ReplaceText(ref volumeDataText, namespaceStr);
        }

        private void ReplaceText(ref string text, in string namespaceStr)
        {
            text = InsertOrRemoveNamespace(text
                .Replace(ComponentMenuPathReplaceStr, componentMenuPath)
                .Replace(PassNameReplaceStr, passName), namespaceStr);
        }

        private string InsertOrRemoveNamespace(string text, in string namespaceStr)
        {
            if (!string.IsNullOrWhiteSpace(namespaceStr))
            {
                Regex classBodyReg = new Regex(@"(?<=" + NamespaceStartReplaceStr + @")[\s\S]*(?=" + NamespaceEndReplaceStr + @")");
                Regex lineStartReg = new Regex(@"[\n\r]{1,2}(?=.+)");
                string lineStartReplaceStr = NewLine + "    ";
                string start = "namespace " + namespaceStr + NewLine + "{";
                string end = "}";
                text = classBodyReg
                    .Replace(text, match => lineStartReg.Replace(match.Value, lineStartReplaceStr))
                    .Replace(NamespaceStartReplaceStr, start)
                    .Replace(NamespaceEndReplaceStr, end);
            }
            else
            {
                text = text
                    .Replace(NamespaceStartReplaceStr, string.Empty)
                    .Replace(NamespaceEndReplaceStr, string.Empty);
            }
            return text;
        }

        private string NewLine { get { return System.Environment.NewLine; } }

        private bool TryGetCurrentFolderPath(out string currPath)
        {
            currPath = null;
            string[] selGUIDs = Selection.assetGUIDs;
            if (selGUIDs.Length > 0)
            {
                currPath = AssetDatabase.GUIDToAssetPath(selGUIDs[0]);
                if (!AssetDatabase.IsValidFolder(currPath))
                {
                    currPath = System.IO.Path.GetDirectoryName(currPath);
                    Regex regex = new Regex(@"\\");
                    currPath = regex.Replace(currPath, "/");
                }
                return true;
            }
            return false;
        }

        private void CraeteFiles(in string currPath, in string volumeText, in string volumeDataText)
        {
            string rootFolderPath = currPath + "/" + passName;
            string resourcesFolderPath = rootFolderPath + "/Resources";
            string volumeCSPath = rootFolderPath + "/" + passName + ".cs";
            string volumeDataCSPath = rootFolderPath + "/" + passName + "Data.cs";
            string volumeDataAssetPath = resourcesFolderPath + "/" + passName + "Data.asset";
            CreateFolder(currPath, passName, rootFolderPath);
            CreateFolder(rootFolderPath, "Resources", resourcesFolderPath);
            CreateTextAsset(volumeText, volumeCSPath);
            CreateTextAsset(volumeDataText, volumeDataCSPath);
            SetDataAssetPath(volumeDataCSPath, volumeDataAssetPath);
            AssetDatabase.Refresh();
        }

        private void CreateFolder(string parentPath, string folderName, string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(parentPath, folderName);
            }
        }

        private static string GetProjectBaseDirectory()
        {
            return System.Environment.CurrentDirectory;
        }

        private void CreateTextAsset(string text, string assetPath)
        {
            string filePath = GetProjectBaseDirectory() + "/" + assetPath;
            if (!System.IO.File.Exists(filePath))
            {
                System.IO.File.WriteAllText(filePath, text);
            }
        }

        [DidReloadScripts]
        private static void CreateDataAsset()
        {
            if (HasDataAssetToSave())
            {
                GetDataAssetPath(out string volumeDataCSPath, out string volumeDataAssetPath);
                CreateDataAsset(volumeDataCSPath, volumeDataAssetPath);
                AssetDatabase.Refresh();
            }
        }

        private static void CreateDataAsset(string scriptPath, string assetPath)
        {
            string filePath = GetProjectBaseDirectory() + "/" + assetPath;
            if (!System.IO.File.Exists(filePath))
            {
                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
                System.Type type = monoScript.GetClass();
                ScriptableObject scriptableObject = CreateInstance(type);
                AssetDatabase.CreateAsset(scriptableObject, assetPath);
                AssetDatabase.SaveAssets();
            }
        }

        private static bool HasDataAssetToSave()
        {
            return PlayerPrefs.HasKey(Key);
        }

        private static void SetDataAssetPath(in string scriptPath, in string assetPath)
        {
            PlayerPrefs.SetString(Key, scriptPath + ";" + assetPath);
        }

        private static void GetDataAssetPath(out string scriptPath, out string assetPath)
        {
            string[] paths = PlayerPrefs.GetString(Key).Split(';');
            scriptPath = paths[0];
            assetPath = paths[1];
            PlayerPrefs.DeleteKey(Key);
        }
    }
}