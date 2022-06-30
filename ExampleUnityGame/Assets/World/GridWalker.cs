using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using System;

namespace ExampleUnityGame
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class GridWalker : MonoBehaviour
    {
        public enum Direction
        {
            Stationary = 0,
            North = 1,
            East = 2,
            South = 3,
            West = 4
        }
        private static readonly Vector2[] directionTranslations = new Vector2[]{
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(0, -1),
            new Vector2(-1, 0),
        };

        public delegate bool? OnColliderHitTest(Collider2D collider, ref Vector3 targetPosition);
        public delegate bool? OnTileColliderHitTest(TilemapCollider2D collider, ref Vector3 targetPosition, TileBase tile, Tilemap tilemap);
        public delegate void OnTileStepEventHandler(Vector3Int tilePosition, TileBase tile, Tilemap tilemap);

        internal static readonly Vector3 FULL_TILE = new Vector3(1f, 1f, 0f);
        internal static readonly Vector3 HALF_TILE = new Vector3(.5f, .5f, 0f);

        public event OnColliderHitTest OnTestColliderHit;
        public event OnTileColliderHitTest OnTestTileColliderHit;
        public event OnTileStepEventHandler OnSteppingIntoTile;
        public event OnTileStepEventHandler OnSteppedOnTile;

        [Header("Speeds")]
        public float tileStepSpeed = 5f;
        public float speedWalking = 1.5f;
        public float speedBiking = 4f;
        public float speedJumping = 2f;
        public float speedSprintModifier = 2f;

        [Header("States")]
        
        public bool isOnBike;
        
        public bool isSprinting;
        public bool isJumping;

        [Header("Movement")]
        // Where we will be when we arrive at our desired destination
        public Vector3? nextPlayerPosition;

        // The normalized vector indicating in which direction we want to be moving
        public Vector2 desiredMoveDirectionVector = Vector2.zero;

        // The normalized vector indicating in which direction we are looking
        public Vector2 lookDirectionVector = Vector2.zero;

        public bool IsMovingToNextPosition
          => nextPlayerPosition != null && nextPlayerPosition != transform.position;

        // How fast we're moving in a direction
        public float speedMultiplier = 1f;

        private Tilemap[] tilemaps;
        private BoxCollider2D localCollider;

        private Animator localAnimator;

        protected virtual void Awake()
        {
        }

        protected virtual void Start()
        {
            tilemaps = GridSystem.Instance.GetComponentsInChildren<Tilemap>();
            localCollider = GetComponent<BoxCollider2D>();
            localAnimator = GetComponent<Animator>();

            TeleportTo(GetNearestValidPlayerPosition());
        }

        protected virtual void Update()
        {
            if (IsMovingToNextPosition)
            {
                localAnimator.SetFloat("Speed", speedMultiplier > speedWalking ? 2 : 1);
            }
            else
            {
                localAnimator.SetFloat("Speed", 0);
            }

            localAnimator.SetFloat("Horizontal", lookDirectionVector.x);
            localAnimator.SetFloat("Vertical", lookDirectionVector.y);

            localAnimator.SetBool("IsJumping", isJumping);
            localAnimator.SetBool("IsOnBike", isOnBike);
        }

        protected void FixedUpdate()
        {
            Vector3 position;

            if (!IsMovingToNextPosition)
                return;

            position = (Vector3) nextPlayerPosition;

            var desiredSpeed = speedWalking;

            if (isJumping)
                desiredSpeed = speedJumping;
            else if (isOnBike)
                desiredSpeed = speedBiking;
            else if (isSprinting)
                desiredSpeed *= speedSprintModifier;

            speedMultiplier = desiredSpeed;

            var step = tileStepSpeed * desiredSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, position, step);

            if (IsMovingToNextPosition)
                return;

            // Once we arrive at our destination let everyone know
            var targetPosition = GridSystem.Instance.WorldToCell(TranslatePlayerToStepPosition(position));

            foreach (var tilemap in tilemaps)
            {
                if (tilemap.HasTile(targetPosition))
                    OnSteppedOnTile?.Invoke(targetPosition, tilemap.GetTile(targetPosition), tilemap);
            }

            DebugMarkStepPosition();

            // Let the player input decide whether to continue moving in our desired direction
            if (desiredMoveDirectionVector.sqrMagnitude > 0)
                Move(desiredMoveDirectionVector);
            else
                nextPlayerPosition = null;
        }

        // Returns whether we can move in the specified direction
        public Vector3? GetNextValidPosition(Vector2 moveDirectionVector)
        {
            var playerPosition = GetStepPosition();
            var nextStepPosition = GridSystem.Instance.GetNearestValidStepPosition(playerPosition, moveDirectionVector);

            // If there's a collision stop moving
            if (!GetDirectionIsWalkable(playerPosition, ref nextStepPosition))
            {
                ExtDebug.DrawBoxCastBox(playerPosition, FULL_TILE, Quaternion.identity, moveDirectionVector, 2f, Color.magenta);

                return null;
            }

            return nextStepPosition;
        }

        public void SetDesiredMoveDirectionVector(Vector2 moveDirectionVector)
          => desiredMoveDirectionVector = moveDirectionVector;

        public void Move(Vector2 moveDirectionVector)
        {
            if (IsMovingToNextPosition)
                return;

            var possibleNextStepPosition = GetNextValidPosition(moveDirectionVector);

            lookDirectionVector = moveDirectionVector;

            // Stop moving if the position we want to move in is invalid
            if (possibleNextStepPosition == null)
            {
                StopMoving();
                return;
            }

            ForceMove((Vector3)possibleNextStepPosition);
        }

        public void SetSprinting(bool isSprinting)
        {
            this.isSprinting = isSprinting;
        }

        public void SetBiking(bool isBiking)
        {
            isOnBike = isBiking;
        }

        public void ForceMove(Vector3 nextStepPosition)
        {
            // Ensure we move the player to the correct position, since movement checking is relative to
            // their collider, but we move the player relative to their anchor
            this.nextPlayerPosition = TranslateStepToPlayerPosition(nextStepPosition);
            
            // Inform event listeners that we're stepping into this tile
            var targetTile = GridSystem.Instance.WorldToCell(nextStepPosition);

            foreach (var tilemap in tilemaps)
            {
                if (tilemap.HasTile(targetTile))
                    OnSteppingIntoTile?.Invoke(targetTile, tilemap.GetTile(targetTile), tilemap);
            }
        }

        public void StopMoving()
        {
            desiredMoveDirectionVector = TranslateDirection(Direction.Stationary);
        }

        // Step into the direction without asking any event handlers for other options
        public void ForceStepTowards(Direction direction)
        {
            var playerPosition = GetStepPosition();
            var nextStepPosition = GridSystem.Instance.GetNearestValidStepPosition(playerPosition, TranslateDirection(direction));

            ForceMove(nextStepPosition);
        }

        public void TeleportTo(Vector3 position)
        {
            var stepPosition = GridSystem.Instance.GetNearestValidStepPosition(position);
            transform.position = (Vector3) (nextPlayerPosition = TranslateStepToPlayerPosition(stepPosition));
        }

        public bool GetDirectionIsWalkable(Vector3 currentPosition, ref Vector3 targetPosition)
        {
            var direction = (targetPosition - currentPosition).normalized;
            var hits = Physics2D.BoxCastAll(currentPosition, FULL_TILE, 0, direction, 2f);

            if (hits.Length > 0)
            {
                var targetCellPosition = GridSystem.Instance.GetValidCellPosition(targetPosition);

                foreach (var hit in hits)
                {
                    if (hit.collider == localCollider)
                        continue;

                    bool? eventHitResult = default;

                    if (hit.collider is TilemapCollider2D tilemapCollider)
                    {
                        var tilemap = tilemapCollider.GetComponent<Tilemap>();

                        // Let the event handlers adjust where we will move to through `ref targetPosition`
                        eventHitResult = OnTestTileColliderHit?.Invoke(tilemapCollider, ref targetPosition, tilemap.GetTile(targetCellPosition), tilemap);
                    }
                    else if (hit.collider is Collider2D collider)
                    {
                        eventHitResult = OnTestColliderHit?.Invoke(collider, ref targetPosition);
                    }

                    if (eventHitResult != null && eventHitResult == false)
                        continue;

                    if (eventHitResult == null && hit.collider.isTrigger)
                        continue;

                    return false;
                }
            }

            return true;
        }

        public static Vector2 TranslateDirection(Direction direction)
        {
            return directionTranslations[(int)direction];
        }

        public static Direction TranslateDirection(Vector2 directionVector)
        {
            for (int i = 0; i < directionTranslations.Length; i++)
            {
                if (directionTranslations[i] == directionVector)
                    return (Direction)i;
            }

            Debug.LogWarning("Invalid direction vector given to TranslateDirection");
            return Direction.Stationary;
        }

        public T GetObjectInFront<T>() where T : Component
          => GetObjectInFront(typeof(T)) as T;

        public object GetObjectInFront(Type objectType)
        {
            var playerPosition = GetStepPosition();
            var hits = Physics2D.BoxCastAll(playerPosition, FULL_TILE, 0, lookDirectionVector, 2f);

            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    if (hit.collider == localCollider)
                        continue;

                    if (objectType.IsAssignableFrom(hit.collider.GetType()))
                        return hit.collider;

                    if (hit.collider.gameObject is GameObject hitGameObject)
                    {
                        if (objectType.IsAssignableFrom(hitGameObject.GetType()))
                            return hitGameObject;

                        Component found;

                        if (found = hitGameObject.GetComponent(objectType))
                            return found;

                        if (found = hitGameObject.GetComponentInChildren(objectType))
                            return found;
                    }
                }
            }

            return null;
        }

        public Vector3 TranslateStepToPlayerPosition(Vector3 stepPosition)
        {
            // TODO: This may break? Should I use Matrix or TransformPoint instead?
            return stepPosition - (Vector3)localCollider.offset;
        }

        public Vector3 TranslatePlayerToStepPosition(Vector3 playerPosition)
        {
            // TODO: This may break? Should I use Matrix or TransformPoint instead?
            return playerPosition + (Vector3)localCollider.offset;
        }

        public Direction GetDirection()
          => TranslateDirection(desiredMoveDirectionVector);

        public Vector2 GetDirectionVector()
          => desiredMoveDirectionVector;

        public Direction GetLookDirection()
          => TranslateDirection(lookDirectionVector);

        public Vector3 GetStepPosition()
        {
            return GridSystem.Instance.GetNearestValidStepPosition(localCollider.transform.TransformPoint(localCollider.offset));
        }

        public Vector3 GetNearestValidPlayerPosition(Vector2? inMoveDirection = null)
        {
            var nearestValidPosition = GridSystem.Instance.GetNearestValidStepPosition(GetStepPosition(), inMoveDirection);
            return TranslateStepToPlayerPosition(nearestValidPosition);
        }

        public void DebugMarkStepPosition()
        {
            var currentPosition = GridSystem.Instance.WorldToCell(GetStepPosition());

            ExtDebug.DrawBox(GridSystem.Instance.GetCellCenterWorld(currentPosition) - HALF_TILE, FULL_TILE, Quaternion.identity, Color.black);
        }
    }
}
