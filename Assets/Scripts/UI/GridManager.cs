using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public int gridSize = 10;
    public TMP_InputField gridInput;

    public RectTransform gridContainer;

    public TMP_Text errorText;

    [Header("Road Tile Prefab")]
    public GameObject tileRoad;

    public static int2[] Path;

    private GridLayoutGroup _gridLayout;
    private List<UiRoadTile> _path;
    private UiRoadTile[,] _tiles;
    private bool _drawing;

    private Coroutine _errorRoutine;

    private void Start()
    {
        gridInput.text = gridSize.ToString();
        _gridLayout = gridContainer.GetComponent<GridLayoutGroup>();
        _drawing = false;

        updateGridTiles();
    }

    public void GridSizeChanged()
    {
        gridSize = int.Parse(gridInput.text);

        updateGridTiles();
    }

    private void updateGridTiles()
    {
        foreach (var c in gridContainer.GetComponentsInChildren<RectTransform>())
        {
            if (c != gridContainer)
                DestroyImmediate(c.gameObject);
        }
        _path = new List<UiRoadTile>();
        _tiles = new UiRoadTile[gridSize, gridSize];
        _drawing = false;

        for (var y = 0; y < gridSize; y++)
        {
            for (var x = 0; x < gridSize; x++)
            {
                var g = GameObject.Instantiate(tileRoad, gridContainer);
                var tile = g.GetComponent<UiRoadTile>();

                tile.pos = new Vector2Int(x, y);
                _tiles[y, x] = tile;
            }
        }

        var cellSize = new Vector2(gridContainer.rect.width, gridContainer.rect.height);
        cellSize.x -= _gridLayout.spacing.x * gridSize;
        cellSize.y -= _gridLayout.spacing.y * gridSize;

        _gridLayout.cellSize = cellSize / gridSize;
    }

    public void ResetPath()
    {
        foreach (var tile in _path)
        {
            tile.GetComponent<Image>().color = Color.white;
        }
        
        _path = new List<UiRoadTile>();
        _drawing = false;
    }

    public void StartSimulation()
    {
        if (_path.Count < 2)
        {
            if (_errorRoutine == null)
            {
                errorText.enabled = true;
                errorText.text = "Fail to start simulation: Not enough tiles.";
                _errorRoutine = StartCoroutine(ErrorCoroutine(2f));
            }
            return;
        }
        
        var path = new int2[_path.Count];
        
        for (var i = 0; i < _path.Count; i++)
        {
            path[i] = new int2(_path[i].pos.x, _path[i].pos.y);
            if (i + 1 == _path.Count) continue;
            
            var dist = Vector2Int.Distance(_path[i].pos, _path[i + 1].pos);
            if (dist > 1f)
            {
                if (_errorRoutine == null)
                {
                    errorText.enabled = true;
                    errorText.text = "Fail to start simulation: Some tiles aren't correctly connected!";
                    _errorRoutine = StartCoroutine(ErrorCoroutine(2f));
                }
                return;
            }
        }
        Path = path;
        SceneManager.LoadScene(1);
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator ErrorCoroutine(float second)
    {
        yield return new WaitForSeconds(second);
        errorText.enabled = false;
        _errorRoutine = null;
    }

    private List<RaycastResult> GetMouseOver()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }
    
    private void Update()
    {
        var raysastResults = GetMouseOver();
        
        if (raysastResults.FindIndex(result => result.gameObject == gridContainer.gameObject) != -1
            && Input.GetMouseButtonDown(0))
        {
            ResetPath();
            _drawing = true;
        }
        else if (Input.GetMouseButtonUp(0))
            _drawing = false;
        
        if (_drawing)
        {
            if (raysastResults.Count == 0)
                return;
            var roadTile = raysastResults.First().gameObject.GetComponent<UiRoadTile>();
            if (!roadTile || _path.Contains(roadTile))
                return;

            _path.Add(roadTile);
            roadTile.GetComponent<Image>().color = Color.yellow;
        }
    }
}