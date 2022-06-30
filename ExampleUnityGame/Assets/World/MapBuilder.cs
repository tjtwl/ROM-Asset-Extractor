using Newtonsoft.Json;
using RomAssetExtractor.Pokemon.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEditor;
using Tile = RomAssetExtractor.Pokemon.Entities.Tile;

namespace ExampleUnityGame
{
    [RequireComponent(typeof(Grid))]
    public class MapBuilder : MonoBehaviour
    {
        const int BLOCKS_PER_TILESET_ROW = 8;
        private readonly Color DEBUG_MARKER_COLOR = new Color(255, 0, 0, 255);

        public Tilemap bottomTilemap; // Below the player/characters
        public Tilemap colliderTilemap; // Below the player/characters, but with a TilemapCollider2D
        public Tilemap topTilemap; // Above the player/characters

        public Sprite collisionSprite;

        public string pathToMap = ".RomCache/maps";
        public string pathToMapTiles = ".RomCache/maps/tiles";
        public string defaultMap = "3.0";

        private string loadedMap;
        private MapModel map;
        private MetatileModel[] mapTiles;

        #region Tile Debug Info
        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            var mousePositionOnScreen = Mouse.current.position.ReadValue();
            var mousePosition = Camera.main.ScreenToWorldPoint(mousePositionOnScreen);
            var nearestCellPosition = GridSystem.Instance.GetValidCellPosition(mousePosition);
            var cellWorldPosition = GridSystem.Instance.CellToWorld(nearestCellPosition);

            ExtDebug.DrawBox(cellWorldPosition, GridWalker.FULL_TILE, Quaternion.identity, DEBUG_MARKER_COLOR, 0.01f);

            var metatileId = TranslateMapPositionToMetatileId((int)(nearestCellPosition.x * .5f), (int)(nearestCellPosition.y * .5));
            var metatile = (metatileId < mapTiles.Length && metatileId >= 0) ? mapTiles[metatileId] : null;

            var bottomTile = bottomTilemap.GetTile(nearestCellPosition);
            var bottomTileText = bottomTile != null ? "Y" : "";
            var colliderTile = colliderTilemap.GetTile(nearestCellPosition);
            var colliderTileText = colliderTile != null ? "Y" : "";
            var topTile = topTilemap.GetTile(nearestCellPosition);
            var topTileText = topTile != null ? "Y" : "";

            var style = new GUIStyle();
            style.normal.textColor = DEBUG_MARKER_COLOR;

            if (metatile == null)
            {
                Handles.Label(mousePosition, nearestCellPosition.ToString(), style);
                return;
            }

            string metatileText;

            if (metatile.Attributes.Count > 0)
            {
                metatileText = string.Empty;
                foreach (var attribute in metatile.Attributes)
                {
                    metatileText += $"{attribute.Key} ({attribute.Value})\n";
                }
            }
            else
            {
                metatileText = "No Attributes\n";
            }

            var debugText = $"Map Metatile = {metatileId}\n" +
                $"(Cell = {nearestCellPosition})\n" +
                $"(Tile = {metatile.Id})\n" +
                $"======== Colliders  =======\n" +
                $"Bottom\t| Collider\t| Top\n" +
                $" {bottomTileText}\t| {colliderTileText}\t| {topTileText}\n" +
                $"======== Attributes =======\n" +
                $"Raw A: {metatile.RawMapAttributes.ToString("X")}\n" +
                $"Raw B: {metatile.RawBehavior.ToString("X")}\n" +
                $"{metatileText ?? "Not part of map"}";

            Handles.Label(mousePosition, debugText, style);
        }
        #endregion Tile Debug Info

        void Update()
        {
            if (loadedMap != defaultMap)
                ReloadMap();
        }

        public MetatileModel GetMetatileForTilePosition(Vector3 position)
        {
            var nearestCellPosition = GridSystem.Instance.GetValidCellPosition(position);
            var metatileId = TranslateMapPositionToMetatileId((int)(nearestCellPosition.x * .5f), (int)(nearestCellPosition.y * .5));
            return (metatileId < mapTiles.Length && metatileId >= 0) ? mapTiles[metatileId] : null;
        }

        void ReloadMap()
        {
            loadedMap = defaultMap;

            bottomTilemap.ClearAllTiles();
            colliderTilemap.ClearAllTiles();
            topTilemap.ClearAllTiles();

            var mapJson = File.ReadAllText(Path.Combine(Application.dataPath, pathToMap, $"{defaultMap}.json"));
            var mapTilesJson = File.ReadAllText(Path.Combine(Application.dataPath, pathToMapTiles, $"{defaultMap}.json"));

            map = JsonConvert.DeserializeObject<MapModel>(mapJson);
            mapTiles = JsonConvert.DeserializeObject<MetatileModel[]>(mapTilesJson);

            // Load the tileset image as a texture
            var bottomTilesetPixels = File.ReadAllBytes(Path.Combine(Application.dataPath, pathToMapTiles, $"{map.BankId}.{map.Id}_tileset.png"));

            // TODO: Calculate or store these sizes somewhere?
            // TODO: Optimize below code if possible
            var bottomTilesetTexture = new Texture2D(0, 0); // size will get calculated from pixels
            bottomTilesetTexture.filterMode = FilterMode.Point;
            bottomTilesetTexture.LoadImage(bottomTilesetPixels);

            DrawTilemapLayer(bottomTilemap, bottomTilesetTexture);

            var topTilesetPixels = File.ReadAllBytes(Path.Combine(Application.dataPath, pathToMapTiles, $"{map.BankId}.{map.Id}_tileset_overlays.png"));
            var topTilesetTexture = new Texture2D(0, 0); // size will get calculated from pixels
            topTilesetTexture.filterMode = FilterMode.Point;
            topTilesetTexture.LoadImage(topTilesetPixels);

            DrawTilemapLayer(topTilemap, topTilesetTexture);

            colliderTilemap.size = topTilemap.size = bottomTilemap.size;
            colliderTilemap.origin = topTilemap.origin = bottomTilemap.origin;
        }

        private void TranslateMetatileIdToMapPosition(int metatileId, out int metatileX, out int metatileY)
        {
            metatileX = metatileId % map.Layout.Width;
            // Unity works bottom left to top right, so we want to subtract from the height to match how the map is laid out in our data (top left to bottom right)
            metatileY = map.Layout.Height - (metatileId / map.Layout.Width);
        }

        // Supposed to reverse TranslateMetatileIdToMapPosition
        private int TranslateMapPositionToMetatileId(int metatileX, int metatileY)
        {
            // I'm 101% sure this math is funky.
            metatileX -= 1;
            metatileY *= -map.Layout.Width;
            metatileX += metatileY;
            var metatileId = mapTiles.Length + metatileX;
            return metatileId;
        }

        private void DrawTilemapLayer(Tilemap tilemap, Texture2D tilesetTexture)
        {
            for (int metatileId = 0; metatileId < mapTiles.Length; metatileId++)
            {
                var metatile = mapTiles[metatileId];

                // 4 tiles in a metatile
                // TODO: Top and bottom layers
                for (var tileMetatileIndex = 0; tileMetatileIndex < 8; tileMetatileIndex++)
                {
                    var tilemapTileX = metatile.Id % BLOCKS_PER_TILESET_ROW * MetatileModel.SIZE_IN_PIXELS;
                    var tilemapTileY = metatile.Id / BLOCKS_PER_TILESET_ROW * MetatileModel.SIZE_IN_PIXELS;

                    // World positions to place the tile:
                    GetTileOffset(tileMetatileIndex, out int tileX, out int tileY);

                    tilemapTileX += tileX * Tile.SIZE_IN_PIXELS;
                    tilemapTileY += tileY * Tile.SIZE_IN_PIXELS;

                    //Debug.Log($"{metatileId} ({metatile.Id} + ({tileX}, {tileY}) = {tilemapTileX}, {tilemapTileY}");

                    var gridTile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
                    gridTile.sprite = Sprite.Create(tilesetTexture,
                        // The Y axis is bottom to top
                        new Rect(tilemapTileX, tilesetTexture.height - tilemapTileY - Tile.SIZE_IN_PIXELS, Tile.SIZE_IN_PIXELS, Tile.SIZE_IN_PIXELS), // section of texture to use
                        new Vector2(.5f, .5f), // final sprite pivot in the center
                        8, // pixels per unity tile grid unit
                        0,
                        SpriteMeshType.FullRect,
                        Vector4.zero
                    );

                    // Times 2 because we are placing 4 tiles for every 1 metatile. So double in both X and Y directions.
                    // Also Bottom to top (instead of top to bottom like our input data) so we subtract Y
                    // We add 1 to X otherwise we start drawing on a half tile
                    TranslateMetatileIdToMapPosition(metatileId, out var metatileX, out var metatileY);
                    var metatilePosition = new Vector3Int(1 + (metatileX * 2) + tileX, (metatileY * 2) - tileY, 0);

                    tilemap.SetTile(metatilePosition, gridTile);

                    if (metatile.IsImpassable || metatile.IsSurfOnly)
                    {
                        var collisionTile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
                        collisionTile.sprite = collisionSprite;
                        colliderTilemap.SetTile(metatilePosition, collisionTile);
                    }
                }
            }
        }

        private static void GetTileOffset(int tileMetatileIndex, out int tileX, out int tileY)
        {
            switch (tileMetatileIndex)
            {
                case 0:
                case 4:
                    tileX = 0;
                    tileY = 0;
                    break;
                case 1:
                case 5:
                    tileX = 1;
                    tileY = 0;
                    break;
                case 2:
                case 6:
                    tileX = 0;
                    tileY = 1;
                    break;
                case 3:
                case 7:
                    tileX = 1;
                    tileY = 1;
                    break;
                default:
                    throw new NotImplementedException("More than 2 layers are not yet supported for metatiles");
            }
        }
    }
}