using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;


public sealed class Board : MonoBehaviour
{
    public static Board Instance { get; private set; }
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioSource audioSource;

    public Row[] rows;
    public Tile[,] Tiles { get; private set; }
    public int Width => Tiles.GetLength(dimension: 0);
    public int Height => Tiles.GetLength(dimension: 1);
    
    private readonly List<Tile> _selection = new List<Tile>();
    private const float TweenDuration = 0.25f;

    private void Awake() => Instance =this;
    private void Start()
    {
        Tiles = new Tile[rows.Max(selector: row => row.tiles.Length), rows.Length];
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var tile = rows[y].tiles[x];
                tile.x = x;
                tile.y = y;
                tile.Item = ItemDatabase.Items[UnityEngine.Random.Range(0, ItemDatabase.Items.Length)];
                Tiles[x, y] = tile;
            }
        }

    }
  
    public async void Select(Tile tile)
    {

        if (!_selection.Contains(tile))
        {
        if (_selection.Count>0)
        {
                if (Array.IndexOf(_selection[0].Neigbours, tile) != -1) 
                {
                    _selection.Add(tile);
                }
        }
            else
            {
                _selection.Add(tile); 
            }
                    
            
        
        }
        

        if (_selection.Count < 2) return;
        Debug.Log(message: $"Selected tiles at({_selection[0].x},{_selection[0].y},'and',{_selection[1].x},{_selection[1].y})");
        await Swap(_selection[0], _selection[1]);
        if (CanPop())
        {
            Pop();
        }
        else
        {
            await Swap(_selection[0], _selection[1]);
        }
         _selection.Clear();
    }
    public async Task Swap(Tile tile1,Tile tile2)
    {
        var icon1 = tile1.icon;
        var icon2 = tile2.icon;
        var icon1transform = icon1.transform;
        var icon2transform = icon2.transform;
        var sequance = DOTween.Sequence();
        sequance.Join(icon1transform.DOMove(icon2transform.position, TweenDuration)).Join(icon2transform.DOMove(icon1transform.position, TweenDuration));
        await sequance.Play().AsyncWaitForCompletion();
        icon1transform.SetParent(tile2.transform);
        icon2transform.SetParent(tile1.transform);
        tile1.icon = icon2;
        tile2.icon = icon1;
        var tile1item = tile1.Item;
        tile1.Item = tile2.Item;
        tile2.Item = tile1item;
    }
    private bool CanPop()
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                if (Tiles[x,y].GetConnectedTiles().Skip(1).Count()>=2)
                {
                    return true;
                }
            }
        }
        return false;
    }
    private async void Pop()
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var tile = Tiles[x, y];
                var connectedTiles = tile.GetConnectedTiles();
                if (connectedTiles.Skip(1).Count() < 2) continue;
                var deflateSequence = DOTween.Sequence();
                foreach (var connectedTile in connectedTiles)
                { deflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.zero, TweenDuration)); }

                audioSource.PlayOneShot(collectSound);
                ScoreCounter.Instance.Score += tile.Item.value * connectedTiles.Count;
                await deflateSequence.Play().AsyncWaitForCompletion();
                var inflateSequence = DOTween.Sequence();
                foreach (var connectedTile in connectedTiles)
                {
                    connectedTile.Item = ItemDatabase.Items[UnityEngine.Random.Range(0, ItemDatabase.Items.Length)];
                    inflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.one, TweenDuration));
                }
                await inflateSequence.Play().AsyncWaitForCompletion();
                x = 0;y = 0;
            }
        }
    }

}
