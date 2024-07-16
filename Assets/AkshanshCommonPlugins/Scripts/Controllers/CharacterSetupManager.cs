using AkshanshKanojia.Controllers.PointClick;
using UnityEngine;

namespace AkshanshKanojia.Controllers
{
    public class CharacterSetupManager : MonoBehaviour
    {
        public bool GenerateOverSelectedObject;
        [HideInInspector] public GameObject CharacterParent;
        [SerializeField] enum AvailableControllers { PointClick }
        [SerializeField] AvailableControllers controllerToGenerate;

        public void GenerateController()
        {
            switch (controllerToGenerate)
            {
                case AvailableControllers.PointClick:
                    var _tempParent = (!GenerateOverSelectedObject) ? new GameObject("Point Click Controller") : CharacterParent;
                    if (_tempParent.TryGetComponent<PointClickManager>(out _))
                    {
                        Debug.LogWarning(_tempParent.name + " already contain a Point Click Manager!");
                    }
                    else
                    {
                        _tempParent.AddComponent<PointClickManager>();
                    }
                    if (_tempParent.TryGetComponent<PointClickInput>(out _))
                    {
                        Debug.LogWarning(_tempParent.name + " already contain a Point Click Input!");
                    }
                    else
                    {
                        _tempParent.AddComponent<PointClickInput>();
                    }
                    break;
            }
        }
    }
}
