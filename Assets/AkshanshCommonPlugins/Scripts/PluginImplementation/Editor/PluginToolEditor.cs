using AkshanshKanojia.Controllers;
using AkshanshKanojia.Controllers.CameraController;
using AkshanshKanojia.Controllers.ObjectManager;
using AkshanshKanojia.Inputs.Mobile;
using AkshanshKanojia.Animations;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace AkshanshKanojia.PluginManager
{
    public class PluginToolEditor : EditorWindow
    {
        [SerializeField] List<string> customFolderNames;

        [MenuItem("Akshansh/Open Plugin Manager")]
        static void GeneratePluginManager()
        {
            EditorWindow _window = GetWindow<PluginToolEditor>();
            _window.titleContent = new GUIContent("Common Plugins Manager");
        }

        private void OnGUI()
        {
            //EditorGUILayout.LabelField("");
            if (GUILayout.Button("Generate My Custome Folders", GUILayout.Width(200), GUILayout.Height(20)))
            {
                AssetDatabase.CreateFolder("Assets", "Akshansh");
                AssetDatabase.CreateFolder("Assets/Akshansh", "Scripts");
                AssetDatabase.CreateFolder("Assets/Akshansh", "Scenes");
                AssetDatabase.CreateFolder("Assets/Akshansh", "Audio");
                AssetDatabase.CreateFolder("Assets/Akshansh", "Models");
                AssetDatabase.CreateFolder("Assets/Akshansh", "Animations");
                AssetDatabase.CreateFolder("Assets/Akshansh", "Prefabs");
                AssetDatabase.CreateFolder("Assets/Akshansh", "Textures");
                AssetDatabase.CreateFolder("Assets/Akshansh", "Sprites");
                AssetDatabase.CreateFolder("Assets/Akshansh", "Materials");
            }
            if (GUILayout.Button("Generate Mobile Input", GUILayout.Width(150), GUILayout.Height(20)))
            {
                GameObject _tempObj = new GameObject("Mobile Input manager");
                _tempObj.AddComponent<MobileInputManager>();
                Debug.Log("Generated Mobile Input Manager Dummy Object!");
            }
            if (GUILayout.Button("Generate Character Manager", GUILayout.Width(200), GUILayout.Height(20)))
            {
                if (FindObjectOfType<CharacterSetupManager>())
                {
                    Debug.LogWarning("Character Setup Manager already exist!");
                    return;
                }
                var _tempObj = new GameObject("Character Manager");
                _tempObj.AddComponent<CharacterSetupManager>();
            }
            if (GUILayout.Button("Generate Camera Manager", GUILayout.Width(200), GUILayout.Height(20)))
            {
                if (FindObjectOfType<Camera>())
                {
                    if (FindObjectOfType<Camera>().GetComponent<CameraManager>())
                    {
                        Debug.LogWarning("Camera already have camera manager component!");
                        return;
                    }
                }
                if (FindObjectOfType<Camera>())
                {
                    FindObjectOfType<Camera>().gameObject.AddComponent<CameraManager>();
                    Debug.Log("Added Camera Manager to existing camera: " + FindObjectOfType<CameraManager>().name);
                }
                else
                {
                    var _tempObj = new GameObject("Camera Manager");
                    _tempObj.AddComponent<Camera>();
                    _tempObj.AddComponent<CameraManager>();
                    Debug.Log("Added Camera Manager Object");

                }
            }
            if (GUILayout.Button("Generate Object Controller", GUILayout.Width(200), GUILayout.Height(20)))
            {
                GameObject _tempObj = new GameObject("Object Controller");
                _tempObj.AddComponent<ObjectController>();
                Debug.Log("Generated Object Controller!");
            }
            if (GUILayout.Button("Generate Transform Sequencer", GUILayout.Width(200), GUILayout.Height(20)))
            {
                GameObject _tempObj = new GameObject("Transform Sequence Manager");
                _tempObj.AddComponent<TransformSequencer>();
                Debug.Log("Generated Transform Sequencer!");
            }
            if (GUILayout.Button("Close", GUILayout.Width(100), GUILayout.Height(20)))
            {
                GetWindow<PluginToolEditor>().Close();
            }
            EditorGUILayout.LabelField("Still Under Very Early Development!");
        }
    }
}
