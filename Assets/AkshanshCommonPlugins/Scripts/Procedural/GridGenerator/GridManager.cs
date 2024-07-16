using System.Collections.Generic;
using UnityEngine;

namespace AkshanshKanojia.LevelEditors
{
    public class GridManager : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Grid Properties")]
        [SerializeField] float cellSize = 1f;
        [SerializeField,Range(2,10000)]
        uint xSize = 10, zSize = 10;

        
        #endregion

        #region Private Fields
        Vector3[] gridVertices;
        #endregion

        #region Public Fields
        [Header("Editor Properties")]
        public Color vertColor;
        public float vertSize = 1f;

        public bool showInEditor = true, showMidPoint = false, updateWithObjectTransform;
        #endregion


        //can be used to genrate faces if needed later
        [System.Serializable]
        public class CellHolder
        {
            public Vector3 bottomLeft, topLeft, bottomRight, topRight, midPos;
            public int EntitiesInCell = 0;//assign this manually (for game need)
        }
        public List<CellHolder> cells;

        #region private methods
        private void Start()
        {
            GenerateGrid();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = vertColor;
            if (showInEditor)
            {
                try
                {
                    foreach (var v in gridVertices)
                    {
                        if (v != null)
                        {
                            Gizmos.DrawSphere(v, vertSize);
                        }
                    }
                    if (showMidPoint)
                    {
                        foreach (var v in cells)
                        {
                            Gizmos.DrawSphere(v.midPos, vertSize);
                        }
                    }
                }
                catch
                {
                    GenerateGrid();
                }
            }

        }
        // gets information of cells formed by 4 vertices of grid
        void GetCellData()
        {
            cells.Clear();
            int _zFixIndex = 0;//with each itteration z value falls behind 1 to fix the index this is used
            for (int i = 0; i < (xSize - 1) * (zSize - 1); i++)
            {
                if (i % (zSize - 1) == 0 && i != 0)// ie one row is finished since it is stored in 1d array
                {
                    _zFixIndex++;
                }
                CellHolder _temp = new CellHolder()
                {
                    bottomLeft = gridVertices[i + _zFixIndex],
                    topLeft = gridVertices[i + 1 + _zFixIndex],
                    topRight = gridVertices[i + zSize + 1 + _zFixIndex],
                    bottomRight = gridVertices[i + zSize + _zFixIndex]
                };
                _temp.midPos = RotatePointAroundPivot(_temp.midPos, transform.position, transform.eulerAngles);
                cells.Add(_temp);
            }

            //calculate Midpoint
            foreach(var v in cells)
            {
                v.midPos = CalculateMidPoint(v.topRight, v.bottomLeft);
            }
        }

        Vector3 CalculateMidPoint(Vector3 _top,Vector3 _bottom)
        {
            return _top + (_bottom - _top) / 2;
        }

        #endregion

        #region public methods
        public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            return Quaternion.Euler(angles) * (point - pivot) + pivot;
        }
        // generates the grid vertices
        public void GenerateGrid()
        {
            gridVertices = new Vector3[xSize * zSize];
            if(cells!=null)
            cells.Clear();
            //generate all vertices
            int _tempVertIndex = 0;
            Vector3 _tempTransform = (updateWithObjectTransform) ? transform.position : Vector3.zero;
            for (int i = 0; i < xSize; i++)
            {
                for (int j = 0; j < zSize; j++)
                {
                    gridVertices[_tempVertIndex] = new Vector3(i, 0, j) * cellSize + _tempTransform;
                    if (updateWithObjectTransform)
                    {
                        gridVertices[_tempVertIndex] = RotatePointAroundPivot(gridVertices[_tempVertIndex],
                            transform.position, transform.eulerAngles);//apply rotation on vector
                    }
                    _tempVertIndex++;
                }
            }
            if (xSize >= 1 && zSize >= 1)
                GetCellData();

        }
        public void ClearGrid()
        {
            xSize = 0;
            zSize = 0;
            cells.Clear();
        }

        public int GetNearestCellIndex(Vector3 _pos)
        {
            //can be optimized by adding algorithms, following linear search now
            for (int i = 0; i < cells.Count; i++)
            {
                _pos.y = transform.position.y;//exclude y distance for snap
                if (Vector3.Distance(_pos, cells[i].midPos) < cellSize / 2)
                {
                    return i;
                }
            }
            return -1;
        }

        public void SetGridSize(int _x, int _z)
        {
            _x = (_x < 0) ? -_x : _x;
            _z = (_z< 0) ? -_z : _z;
            xSize = (uint)_x;
            zSize = (uint)_z;
            GenerateGrid();
        }
        public void SetCellSize(float _size)
        {
            cellSize = _size;
            GenerateGrid();
        }
        public Vector3[] GetVerticeData()
        {
            return gridVertices;
        }
        #endregion
    }
}