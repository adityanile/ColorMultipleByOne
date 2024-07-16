using System.Collections.Generic;
using UnityEngine;

namespace AkshanshKanojia.LevelEditors
{
    //edit size values to avoid -ve numbers
    public class ObjectSpwaner : MonoBehaviour
    {
        #region Public Fields
        public enum AvailableGenerationModes
        {
            RandomGrid, OrderedGrid, RandomInRadius,
            RandomBetweenVectors, RemoveRandomObjects
        }
        public AvailableGenerationModes GenerationMode;//determines how generation algorithm takes place
        //grid properties
        [HideInInspector] public float GridCellSize = 1f;
        [HideInInspector] public int GridXSize = 10, GridZSize = 10;
        [HideInInspector] public bool RandomizeInsideCell = false, MatchSurfaceHeight = false;
        //Generation Properties
        [HideInInspector] public bool AvoidObjectOverlaps, SkipOnOverlap, ApplyParentRotation, AdjustHeight, RotateAlongNormal;
        [HideInInspector] public int MaxOverlapItteration = 10;
        [HideInInspector] public float SurfaceRayDist = 2f;

        [System.Serializable]
        public struct SpwanableObjectHolder
        {
            public GameObject ObjectPrefab;
            public float ObjectAvoidanceRadius;
        }
        public SpwanableObjectHolder[] SpwanablePrefabs;
        [HideInInspector] public string HolderName = "Generated props";
        //debug properties
        [HideInInspector] public bool ShowGrid;
        [HideInInspector] public float GridVertSize = 0.1f;

        #endregion

        #region Serialized Fields
        [SerializeField, Tooltip("Total number of objects to generate/Remove")]
        int MaxObjectsToGenerate = 10;
        #endregion

        #region Private Fields
        [System.Serializable]
        struct GeneratedObjectData
        {
            public GameObject TargetObj;
            public Vector3 SpawnPos;
        }

        List<GeneratedObjectData> CurtActiveObjects;
        GridManager gridMang;
        enum ObjectGenrationFilters { FixedInRadius, RandomInRadius }
        #endregion

        void Initalize()//generates basic componenets required for generation
        {
            gridMang = GetComponent<GridManager>();
            if (!gridMang)
                gridMang = gameObject.AddComponent<GridManager>();
            gridMang.hideFlags = HideFlags.HideInInspector;
            gridMang.vertColor = Color.red;
            gridMang.SetCellSize(GridCellSize);
            gridMang.SetGridSize(GridXSize, GridZSize);
            gridMang.vertSize = GridVertSize;
            gridMang.updateWithObjectTransform = true;
            gridMang.GenerateGrid();
        }

        /// <summary>
        /// Generates a single object depending onnvalues passed initilly on list
        /// </summary>
        bool GenerateSingleObject(ObjectGenrationFilters _filter, int _gridIndex,bool _avoidOverlaps)
        {
            int _genIndex = Random.Range(0, SpwanablePrefabs.Length);
            Transform _tempObj = Instantiate(SpwanablePrefabs[_genIndex].ObjectPrefab).transform;
            _tempObj.position = gridMang.cells[_gridIndex].midPos;

            //if apply rot
            if (ApplyParentRotation)
            {
                _tempObj.eulerAngles = transform.eulerAngles;
            }

            if (MatchSurfaceHeight)
            {
                if (GetCollidingSurface(_tempObj.transform.position, -_tempObj.transform.up) != null)
                {
                    RaycastHit _hit = (RaycastHit)GetCollidingSurface(_tempObj.transform.position, -_tempObj.transform.forward);
                    if (RotateAlongNormal)
                    {
                        _tempObj.transform.rotation = Quaternion.LookRotation(transform.position - _hit.point);
                    }
                    _tempObj.position = _hit.point;
                }
            }
            //raycast to get height
            if (_filter == ObjectGenrationFilters.RandomInRadius)
            {
                //randomize pos
                _tempObj.position += new Vector3(Random.Range(-GridCellSize / 2, GridCellSize / 2), 0, Random.Range(-GridCellSize / 2, GridCellSize / 2));
                //match surface
                if (MatchSurfaceHeight)
                {
                    if (GetCollidingSurface(_tempObj.transform.position, -_tempObj.transform.up) != null)
                    {
                        RaycastHit _hit = (RaycastHit)GetCollidingSurface(_tempObj.transform.position, -_tempObj.transform.forward);
                        if (RotateAlongNormal)
                        {
                            _tempObj.transform.rotation = Quaternion.LookRotation(transform.position - _hit.point);
                        }
                        _tempObj.position = _hit.point;
                    }
                }
            }

            if (_avoidOverlaps)//check for overlaps
            {
                foreach (var v in CurtActiveObjects)
                {
                    if (Vector3.Distance(v.TargetObj.transform.position, _tempObj.transform.position) <
                        SpwanablePrefabs[_genIndex].ObjectAvoidanceRadius)
                    {
                        DestroyImmediate(_tempObj.gameObject);
                        return false;
                    }
                }
            }
            CurtActiveObjects.Add(new GeneratedObjectData
            {
                TargetObj = _tempObj.gameObject,
            });
            return true;
        }

        /// <summary>
        /// checks if a possible colliding surface exists
        /// </summary>
        /// <returns>raycast hit (nullable)</returns>
        RaycastHit? GetCollidingSurface(Vector3 _origin, Vector3 _dir)
        {
            if (Physics.Raycast(_origin, _dir, out RaycastHit _hit, SurfaceRayDist))
            {
                print("yes");
                return _hit;
            }
            return null;
        }

        public void GenerateObjects()//generates object based on generation type
        {
            Initalize();
            CurtActiveObjects = new List<GeneratedObjectData>();
            switch (GenerationMode)
            {
                case AvailableGenerationModes.OrderedGrid:
                    GenerateGridBased(ObjectGenrationFilters.FixedInRadius);
                    break;
                case AvailableGenerationModes.RandomGrid:
                    GenerateGridBased(ObjectGenrationFilters.RandomInRadius);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Runs the algorithm for grid based generations.
        /// </summary>
        /// <param name="_filter"></param>
        private void GenerateGridBased(ObjectGenrationFilters _filter)
        {
            int _tempObjGenerated = 0, _tempGridIndex = 0;
            //generation
            while (_tempObjGenerated < MaxObjectsToGenerate)
            {
                if (gridMang.cells.Count == 0)
                {
                    break;//if no possible cells available
                }
                GenerateSingleObject(_filter, _tempGridIndex, AvoidObjectOverlaps);

                //try avoiding overlaps
                if (AvoidObjectOverlaps)
                {
                    int _tempItteration = 0;
                    do
                    {
                        if (GenerateSingleObject(_filter, _tempGridIndex, AvoidObjectOverlaps))
                        {
                            break;
                        }
                        _tempItteration++;
                        if (_tempItteration == MaxOverlapItteration)
                        {
                            //last itteration
                            if (!SkipOnOverlap)
                            {
                                GenerateSingleObject(_filter, _tempGridIndex, false);//generate object regardless
                            }
                        }
                    }
                    while (_tempItteration < MaxOverlapItteration);
                }
                _tempGridIndex++;
                if (_tempGridIndex == gridMang.cells.Count)
                {
                    _tempGridIndex = 0;
                }
                _tempObjGenerated++;
            }
        }

        public void SetGridDebug()
        {
            Initalize();
            gridMang.showInEditor = ShowGrid;
            gridMang.vertColor = Color.red;
        }
    }
}
