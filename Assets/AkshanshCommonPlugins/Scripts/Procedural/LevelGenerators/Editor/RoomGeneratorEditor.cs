using UnityEditor;

namespace AkshanshKanojia.LevelEditors
{
    [CustomEditor(typeof(RoomGenerator))]
    public class RoomGeneratorEditor : Editor
    {
        private void OnSceneGUI()
        {
            DrawDefaultInspector();
        }
    }
}
