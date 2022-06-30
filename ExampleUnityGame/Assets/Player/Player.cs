using RomAssetExtractor.Pokemon.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ExampleUnityGame
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Player : GridWalker
    {
        public AnimatedSpriteSheet[] replacementSpriteSheets;
        public MapBuilder mapGrid;

        private SpriteRenderer[] localRenderers;

        protected override void Awake()
        {
            base.Awake();

            localRenderers = GetComponentsInChildren<SpriteRenderer>();
            OnTestTileColliderHit += Player_OnTestTileColliderHit;
        }

        private bool? Player_OnTestTileColliderHit(UnityEngine.Tilemaps.TilemapCollider2D collider, ref Vector3 targetPosition, UnityEngine.Tilemaps.TileBase tile, UnityEngine.Tilemaps.Tilemap tilemap)
        {
            var metatile = mapGrid.GetMetatileForTilePosition(targetPosition);

            if (metatile.IsSurfOnly)
                // TODO: Exception for surfing player
                return true; // false if surfing

            // Allow jumping if we're going the right direction
            var currentDirection = TranslateDirection(desiredMoveDirectionVector);
            if (
                (metatile.IsJumpNorth && currentDirection == Direction.North)
                || (metatile.IsJumpEast && currentDirection == Direction.East)
                || (metatile.IsJumpSouth && currentDirection == Direction.South)
                || (metatile.IsJumpWest && currentDirection == Direction.West))
            {
                targetPosition = GridSystem.Instance.GetNearestValidStepPosition(targetPosition + (Vector3)desiredMoveDirectionVector, desiredMoveDirectionVector);
                return false; // disable collision
            }

            // Continue with default behavior:
            return null;
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
        }

        protected void LateUpdate()
        {
            foreach (var renderer in localRenderers)
            {
                if (renderer.sprite == null)
                    continue;

                var spriteName = renderer.sprite.name;

                foreach (var replacementSpriteSheet in replacementSpriteSheets)
                {
                    var sprite = replacementSpriteSheet.GetSprite(spriteName);

                    if (sprite != null)
                        renderer.sprite = sprite;
                }
            }
        }

        public void OnMovement(InputAction.CallbackContext value)
        {
            var directionVector = value.ReadValue<Vector2>();

            // Normalize the vector and disallow diagonal movement
            directionVector.x = Mathf.RoundToInt(directionVector.x);
            directionVector.y = Mathf.RoundToInt(directionVector.y);

            if (directionVector.x != 0)
                directionVector.y = 0;
            else if (directionVector.y != 0)
                directionVector.x = 0;

            SetDesiredMoveDirectionVector(directionVector);

            if (IsMovingToNextPosition)
                return;

            // Only process moving if input vector is in any direction
            if (!(directionVector.sqrMagnitude != 0))
                return;

            Move(directionVector);
        }

        public void OnHopOnBike(InputAction.CallbackContext value)
        {
            if (!value.performed)
                return;

            SetBiking(!isOnBike);
        }
    }
}
