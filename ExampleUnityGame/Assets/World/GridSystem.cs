using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExampleUnityGame
{
    [RequireComponent(typeof(Grid))]
    public class GridSystem : MonoBehaviour
    {
        public static GridSystem Instance { get; private set; }

        public bool drawGridHelper = true;
        public int gridHelperSize = 500;

        private Grid grid;

        public Vector3Int WorldToCell(Vector3 worldPosition) => grid.WorldToCell(worldPosition);
        public Vector3 CellToWorld(Vector3Int cellPosition) => grid.CellToWorld(cellPosition);

        public Vector3 GetCellCenterWorld(Vector3Int position) => grid.GetCellCenterWorld(position);

        void Awake()
        {
            Instance = this;

            grid = GetComponent<Grid>();
        }

        // Draw 16x16 grid for reference
        void OnDrawGizmos()
        {
            if (!drawGridHelper)
                return;

            Gizmos.color = new Color(255, 0, 0, .3f);

            var gridStart = transform.position - (Vector3.left * (gridHelperSize * .5f)) - (Vector3.down * (gridHelperSize * .5f));

            // TODO: Test if this makes in all cases (seems very specific to current (non rotated)
            // situation)
            for (int x = 1; x < gridHelperSize; x += 2)
            {
                var gridLine = gridStart + (Vector3.down * x);
                Gizmos.DrawLine(gridLine + (Vector3.left * gridHelperSize), gridLine);
            }
            for (int y = 1; y < gridHelperSize; y += 2)
            {
                var gridLine = gridStart + (Vector3.left * y);
                Gizmos.DrawLine(gridLine, gridLine + (Vector3.down * gridHelperSize));
            }
        }

        public Vector3Int GetValidCellPosition(Vector3 nearPosition, Vector2? inMoveDirection = null)
        {
            Vector3Int targetPosition;

            if (inMoveDirection != null)
                targetPosition = grid.WorldToCell(nearPosition + (Vector3)inMoveDirection);
            else
                targetPosition = grid.WorldToCell(nearPosition);

            // Our tiles for drawing are 8x8 whilst movement is calculated 16x16
            if (targetPosition.x % 2 != 0)
                if (inMoveDirection is Vector2 moveDirection)
                    targetPosition.x += (int)moveDirection.x;
                else
                    targetPosition.x += 1;


            if (targetPosition.y % 2 != 0)
                if (inMoveDirection is Vector2 moveDirection)
                    targetPosition.y += (int)moveDirection.y;
                else
                    targetPosition.y += 1;

            return targetPosition;
        }

        public Vector3 GetNearestValidStepPosition(Vector3 stepPosition, Vector2? inMoveDirection = null)
        {
            var targetPosition = GetValidCellPosition((Vector3)stepPosition, inMoveDirection);

            return grid.CellToWorld(targetPosition);
        }
    }
}