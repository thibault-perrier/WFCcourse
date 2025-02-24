using System.Collections.Generic;
using UnityEngine;

public class WFCGenerateGrid : MonoBehaviour
{
    [SerializeField] List<GameObject> _tilePrefabs = new();
    public void GenerateGrid(Dictionary<(int, int), Tile> Cells, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < width; y++)
            {
                Tile t = Cells[(x, y)];
                Instantiate(_tilePrefabs[t.ID], new Vector3(t.position.x, t.position.y, 0), Quaternion.identity, transform);

            }
        }
    }

    public void PlaceTile(Tile t)
    {
        Instantiate(_tilePrefabs[t.ID], new Vector3(t.position.x, t.position.y, 0), Quaternion.identity, transform);
    }
}