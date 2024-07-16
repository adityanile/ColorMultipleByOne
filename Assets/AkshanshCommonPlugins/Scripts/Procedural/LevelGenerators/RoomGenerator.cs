using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AkshanshKanojia.LevelEditors
{
    public class RoomGenerator : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Genral Properties")]
        [SerializeField] string roomContainerName = "Room";
        [SerializeField] int gridXSize = 5, gridYSize = 5;
        [SerializeField] float floorSize = 1f;

        [Header("Floor Properties")]
        [SerializeField] FloorDataHolder[] AvailableFloors;
        [System.Serializable]
        struct FloorDataHolder
        {
            public GameObject FloorPrefab;
            public FloorPlacementOptions PlacementType;
        }

        [System.Serializable]
        class TempFloorData
        {
            public GameObject FloorObject;
            public int xIndex, zIndex;
        }

        #endregion

        #region Public Fields
        public enum FloorPlacementOptions { Centere,Sides,Fill}
        #endregion

        #region Private Fields
        #endregion

        void GenerateFloor(GridManager _grid)
        {
            if(AvailableFloors.Length==0)
            {
                Debug.LogWarning("No floor data available! Check Available floors list");
                return;
            }
            var _tempVert = _grid.GetVerticeData();

        }
        public void GenerateRoom()
        {
            GridManager _grid = GetComponent<GridManager>();
            _grid = (_grid) ? _grid : gameObject.AddComponent<GridManager>();
            _grid.SetCellSize(floorSize);
            _grid.SetGridSize(gridXSize, gridYSize);
            _grid.GenerateGrid();
            GenerateFloor(_grid);
        }
    }
}
