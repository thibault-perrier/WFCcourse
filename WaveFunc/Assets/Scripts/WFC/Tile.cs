using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tile : MonoBehaviour
{
    public bool IsDirectional = false;

    public Tile[] Neighbours;

    public Tile[] UpNeighbours;
    public Tile[] RightNeighbours;
    public Tile[] DownNeighbours;
    public Tile[] LeftNeighbours;

    public int ID;

    public bool Collapsed = false;
    public Tile[] TileOptions;
    public Vector2Int position;

    public Tile()
    {
        ID = -1;
    }

    public Tile(Tile[] tiles, Vector2Int pos, bool collapseState)
    {
        Collapsed = collapseState;
        TileOptions = tiles;
        position = pos;
        ID = -1;
    }

    public Tile(Tile tileToCopy)
    {
        ID = tileToCopy.ID;
        Collapsed = tileToCopy.Collapsed;
        TileOptions = (Tile[])tileToCopy.TileOptions.Clone();
        position = tileToCopy.position;

        IsDirectional = tileToCopy.IsDirectional;
        Neighbours = (Tile[])tileToCopy.Neighbours.Clone();
        UpNeighbours = (Tile[])tileToCopy.UpNeighbours.Clone();
        RightNeighbours = (Tile[])tileToCopy.RightNeighbours.Clone();
        DownNeighbours = (Tile[])tileToCopy.DownNeighbours.Clone();
        LeftNeighbours = (Tile[])tileToCopy.LeftNeighbours.Clone();
    }

    public void GetNeighboursFrom(Tile tile)
    {
        IsDirectional = tile.IsDirectional;
        Neighbours = Neighbours = (Tile[])tile.Neighbours.Clone();
        UpNeighbours = UpNeighbours = (Tile[])tile.UpNeighbours.Clone();
        RightNeighbours = RightNeighbours = (Tile[])tile.RightNeighbours.Clone();
        DownNeighbours = DownNeighbours = (Tile[])tile.DownNeighbours.Clone();
        LeftNeighbours = LeftNeighbours = (Tile[])tile.LeftNeighbours.Clone();
    }
}