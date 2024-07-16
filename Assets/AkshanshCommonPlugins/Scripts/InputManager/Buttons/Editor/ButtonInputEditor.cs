using UnityEditor;

namespace AkshanshKanojia.Inputs.Button
{
    [CustomEditor(typeof(ButtonInputManager))]
    public class ButtonInputEditor : Editor
    {
        private void OnSceneGUI()
        {
            DrawDefaultInspector();
        }
    }
}
