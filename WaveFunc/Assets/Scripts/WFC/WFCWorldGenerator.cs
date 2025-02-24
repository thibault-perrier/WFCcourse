using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WFCWorldGenerator : MonoBehaviour
{
    [Header("Map Parameters")]
    [SerializeField] private int _mapWidth;
    [SerializeField] private int _mapHeight;
    [SerializeField] private List<Tile> _tileRules = new();
    private Dictionary<(int, int), Tile> _placedTiles;
    private Dictionary<(int, int), Tile> _tilesToPlace;

    private WFCGenerateGrid _gridGenerator;


    [ContextMenu("GenerateWorld")]
    public void Init()
    {
        _gridGenerator = GetComponent<WFCGenerateGrid>();

        for (int i = 0; i < _tileRules.Count; i++)
            _tileRules[i].ID = i;

        DeleteWorld();

        _placedTiles = new();
        _tilesToPlace = new();

        Tile fstTile = new Tile(_tileRules[Random.Range(0, _tileRules.Count)]);
        fstTile.position = new Vector2Int(Random.Range(0, _mapWidth), Random.Range(0, _mapHeight));
        fstTile.TileOptions = _tileRules.ToArray();

        _tilesToPlace[(fstTile.position.x, fstTile.position.y)] = fstTile;

        WFCWorldGen();
    }

    #region Basic Wave Function Collapse
    private void WFCWorldGen()
    {
        while (_tilesToPlace.Count > 0)
        {
            CheckEntropy();
        }
    }

    private void CheckEntropy()
    {
        if (_tilesToPlace.Count == 0)
            return;

        int minNbTileOptions = -1;
        minNbTileOptions = _tilesToPlace.Values.Where(t => t.TileOptions != null).Min(t => t.TileOptions.Length);

        if (minNbTileOptions == -1)
        {
            Debug.Log("Error, all tile to place have a 0 length tile options array");
            _tilesToPlace.Clear();
            return;
        }

        Tile randTile = _tilesToPlace.Values.FirstOrDefault(t => t.TileOptions != null && t.TileOptions.Length == minNbTileOptions);

        CollapseTileByRand(randTile);
    }

    private void CollapseTileByRand(Tile tileToCollapse)
    {

        if (tileToCollapse.TileOptions.Length == 0)
        {
            HandleConstraintIssue(tileToCollapse);
            Debug.Log($"Tile {tileToCollapse.position} has now {tileToCollapse.TileOptions.Length} options.");

            return;
        }

        _tilesToPlace.Remove((tileToCollapse.position.x, tileToCollapse.position.y));

        int randID = -1;
        try
        {
            randID = tileToCollapse.TileOptions[Random.Range(0, tileToCollapse.TileOptions.Length)].ID;
        }
        catch
        {
            Debug.Log($"Couldn't find a solution for tile {tileToCollapse.position}");
        }
        tileToCollapse.TileOptions = tileToCollapse
                                    .TileOptions
                                    .Where(t => t.ID == randID)
                                    .ToArray();
        tileToCollapse.ID = randID;

        // Tile chosenTile = _tileRules.FirstOrDefault(t => t.ID == tileToCollapse.ID);
        // if (chosenTile != null)
        //     tileToCollapse.GetNeighboursFrom(chosenTile);

        tileToCollapse.Collapsed = true;
        _placedTiles[(tileToCollapse.position.x, tileToCollapse.position.y)] = tileToCollapse;

        _gridGenerator.PlaceTile(tileToCollapse);

        UpdatePlacedTileSurroundings(tileToCollapse);
    }

    public void UpdatePlacedTileSurroundings(Tile centerTile)
    {
        Tile centerTileRules = _tileRules[centerTile.ID];

        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                if (!(xOffset == 0 ^ yOffset == 0))
                    continue;

                int nX = centerTile.position.x + xOffset;
                int nY = centerTile.position.y + yOffset;

                if (nX < 0 || nY < 0 || nX >= _mapWidth || nY >= _mapHeight)
                    continue;

                if (_placedTiles.ContainsKey((nX, nY)))
                    continue;

                Tile[] centerTileDirOptions = new Tile[] { };

                // Here we want to get the neighbour possibilities of the center tile
                // if tile is not directional, we don't care about the dir of the neighbour
                if (!centerTileRules.IsDirectional)
                {
                    centerTileDirOptions = centerTileRules.Neighbours;
                }
                // case left neighbour
                else if (xOffset == -1)
                {
                    centerTileDirOptions = centerTileRules.LeftNeighbours;
                }
                // case right neighbour
                else if (xOffset == 1)
                {
                    centerTileDirOptions = centerTileRules.RightNeighbours;
                }
                // case up neighbour
                else if (yOffset == 1)
                {
                    centerTileDirOptions = centerTileRules.UpNeighbours;
                }
                // case down neighbour
                else if (yOffset == -1)
                {
                    centerTileDirOptions = centerTileRules.DownNeighbours;
                }

                // if the tile was already seen, we just need to keep the options that are also in the possible neighbours of the current center tile 
                if (_tilesToPlace.ContainsKey((nX, nY)))
                {
                    Tile t = _tilesToPlace[(nX, nY)];
                    t.TileOptions = t.TileOptions.Intersect(centerTileDirOptions).ToArray();
                }
                else
                {
                    _tilesToPlace[(nX, nY)] = new Tile(centerTileDirOptions, new Vector2Int(nX, nY), false);
                }
            }
        }

        // if (_placedTiles.Count == _mapWidth * _mapHeight)
        // {
        //     _gridGenerator.GenerateGrid(_placedTiles, _mapWidth, _mapHeight);
        // }
    }

    #endregion

    #region Error handling

    private void HandleConstraintIssue(Tile problematicTile)
    {
        Debug.Log($"Tile at {problematicTile.position} has no valid options. Attempting constraint removal.");

        RemoveMostConstraining(problematicTile);

        if (problematicTile.TileOptions.Length == 0)
        {
            Debug.LogWarning("Constraint issue not resolved. Escalating to RemoveAllConstraints().");
            RemoveAllConstraints(problematicTile);
        }
    }


    private void RemoveAllConstraints(Tile centerTile)
    {
        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                if (!(xOffset == 0 ^ yOffset == 0))
                    continue;

                int nX = centerTile.position.x + xOffset;
                int nY = centerTile.position.y + yOffset;

                bool wasPlaced = _placedTiles.ContainsKey((nX, nY));
                bool isWaiting = _tilesToPlace.ContainsKey((nX, nY));

                if (!(wasPlaced || isWaiting))
                    continue;

                Tile toRemove;
                if (wasPlaced)
                {
                    toRemove = _placedTiles[(nX, nY)];
                    _placedTiles.Remove((toRemove.position.x, toRemove.position.y));
                }
                else
                {
                    toRemove = _tilesToPlace[(nX, nY)];
                    _tilesToPlace.Remove((toRemove.position.x, toRemove.position.y));
                }

                int savedID = toRemove.ID;

                ReevaluateTileOptions(toRemove, true);

                if (toRemove.TileOptions.Length == 0)
                {
                    if (wasPlaced)
                        toRemove.ID = savedID;
                    RemoveAllConstraints(toRemove);
                }

                _tilesToPlace[(toRemove.position.x, toRemove.position.y)] = toRemove;
            }
        }
        ReevaluateTileOptions(centerTile);
    }

    private void RemoveMostConstraining(Tile centerTile)
    {
        Tile mostConstraining = new Tile();

        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                if (!(xOffset == 0 ^ yOffset == 0))
                    continue;

                int nX = centerTile.position.x + xOffset;
                int nY = centerTile.position.y + yOffset;

                if (!_placedTiles.ContainsKey((nX, nY)))
                    continue;

                if (mostConstraining.ID == -1)
                {
                    mostConstraining = _placedTiles[(nX, nY)];
                    continue;
                }

                Tile[] dirOptions = GetDirOptions(_placedTiles[(nX, nY)], xOffset, yOffset);
                Tile[] mostConstrainingOptions =
                    GetDirOptions(mostConstraining, centerTile.position.x - mostConstraining.position.x, centerTile.position.y - mostConstraining.position.y);

                if (dirOptions.Length < mostConstrainingOptions.Length)
                {
                    mostConstraining = _placedTiles[(nX, nY)];
                }
            }
        }

        if (mostConstraining.ID == -1)
            return;

        _placedTiles.Remove((mostConstraining.position.x, mostConstraining.position.y));
        ReevaluateTileOptions(mostConstraining);
        _tilesToPlace[(mostConstraining.position.x, mostConstraining.position.y)] = mostConstraining;
        ReevaluateTileOptions(centerTile);
    }

    public void ReevaluateTileOptions(Tile centerTile, bool wasCenterTile = false)
    {
        if (wasCenterTile)
            centerTile.TileOptions = _tileRules.Where(t => t.ID != centerTile.ID).ToArray();
        else
            centerTile.TileOptions = _tileRules.ToArray();
        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                if (!(xOffset == 0 ^ yOffset == 0))
                    continue;

                int nX = centerTile.position.x + xOffset;
                int nY = centerTile.position.y + yOffset;

                if (!_placedTiles.ContainsKey((nX, nY)))
                    continue;

                Tile[] NeighbourDirOptions = GetDirOptions(_placedTiles[(nX, nY)], xOffset, yOffset);
                centerTile.TileOptions = centerTile.TileOptions.Intersect(NeighbourDirOptions).ToArray();
            }
        }
    }

    private void UpdateRemovedTileSurroundings(Tile centerTile)
    {

    }

    Tile[] GetDirOptions(Tile targetTile, int xOffset, int yOffset)
    {
        Tile tileRule = _tileRules[targetTile.ID];
        // Here we want to get the neighbour possibilities of the center tile's neighbour tiles (it gets tricky ik) 
        // if tile is not directional, we don't care about the dir of the neighbour
        if (!tileRule.IsDirectional)
        {
            return tileRule.Neighbours;
        }
        // case left neighbour
        else if (xOffset == -1)
        {
            return tileRule.RightNeighbours;
        }
        // case right neighbour
        else if (xOffset == 1)
        {
            return tileRule.LeftNeighbours;
        }
        // case up neighbour
        else if (yOffset == 1)
        {
            return tileRule.DownNeighbours;
        }
        // case down neighbour
        else if (yOffset == -1)
        {
            return tileRule.UpNeighbours;
        }

        return null;
    }

    #endregion

    [ContextMenu("DeleteWorld")]
    public void DeleteWorld()
    {
        List<Transform> tiles = GetComponentsInChildren<Transform>().ToList();
        for (int i = tiles.Count - 1; i >= 0; i--)
        {
            if (tiles[i] != transform)
                DestroyImmediate(tiles[i].gameObject);
        }
    }
}