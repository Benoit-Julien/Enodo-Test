using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoadsManager : MonoBehaviour
{
    public static RoadsManager Instance { get; private set; }
    
    [Header("Prefabs")]
    public GameObject roadTilePrefab;
    public GameObject carPrefab;

    public Transform cameraTarget;

    [Header("UI")]
    public Slider minSpeedSlider;
    public TMP_Text minValueLabel;
    public Slider maxSpeedSlider;
    public TMP_Text maxValueLabel;
    public Slider safeDistanceSlider;
    public TMP_Text safeDistanceLabel;

    [HideInInspector] public int2[] Path;
    [HideInInspector] public Queue<Entity> Cars;
    
    private EntityManager _manager;
    private Entity _roadTileEntityPrefab;
    private Entity _carEntityPrefab;
    
    private float _minSpeed = 0.5f;
    private float _maxSpeed = 2f;
    private float _safeDistance = 1f;

    private void Start()
    {
        Instance = this;
        
        _manager = World.Active.EntityManager;
        _roadTileEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(roadTilePrefab, World.Active);
        _carEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(carPrefab, World.Active);
        
        Path = GridManager.Path;
        Cars = new Queue<Entity>();

        var tiles = new NativeArray<Entity>(Path.Length, Allocator.TempJob);
        _manager.Instantiate(_roadTileEntityPrefab, tiles);
        
        var bound = new Bounds();
        
        for (var i = 0; i < tiles.Length; i++)
        {
            var pos = new float3(Path[i].x, 0, Path[i].y);
            
            _manager.SetComponentData(tiles[i], new Translation { Value = pos });
            _manager.AddComponentData(tiles[i], new AsPriority { Value = true });
            
            bound.Encapsulate(new Vector3(pos.x, pos.y, pos.z));
        }

        cameraTarget.position = bound.center;

        tiles.Dispose();

        minSpeedSlider.value = _minSpeed;
        minValueLabel.text = _minSpeed.ToString("0.0");
        maxSpeedSlider.value = _maxSpeed;
        maxValueLabel.text = _maxSpeed.ToString("0.0");
        safeDistanceSlider.value = _safeDistance;
        safeDistanceLabel.text = _safeDistance.ToString("0.0");
        
        FindObjectOfType<MouseOrbitImproved>().UpdateCameraPosition();
    }

    public void SpawnCar()
    {
        if (Cars.Count > 0)
        {
            var prevCar = Cars.Last();
            var previousCarPos = _manager.GetComponentData<Translation>(prevCar);
            var distance = math.length(previousCarPos.Value - new float3(Path[0].x, previousCarPos.Value.y, Path[0].y));

            if (distance - _safeDistance <= 0)
                return;
        }

        var car = _manager.Instantiate(_carEntityPrefab);

        // Place Car to correct position and correct rotation
        var t = new Translation { Value = new float3(Path.First().x, 0, Path.First().y) };
        _manager.SetComponentData(car, t);
            
        var target = Path[1];
        var heading = new float3(target.x, 0, target.y) - t.Value;
        heading.y = 0;
        _manager.SetComponentData(car, new Rotation { Value = quaternion.LookRotation(heading, math.up()) });
        
        //
        var p = _manager.GetComponentData<PathHelper>(car);
        p.CurrentIndexInRoad = Cars.Count;
        _manager.SetComponentData(car, p);

        //
        var s = _manager.GetComponentData<MoveSpeed>(car);
        var speed = UnityEngine.Random.Range(_minSpeed, _maxSpeed);
        s.CurrentSpeed = s.OriginalSpeed = speed;
        s.SafeDistance = _safeDistance;
        _manager.SetComponentData(car, s);
            
        Cars.Enqueue(car);
    }

    public void SetMinSpeed()
    {
        _minSpeed = minSpeedSlider.value;
        minValueLabel.text = _minSpeed.ToString("0.0");
    }

    public void SetMaxSpeed()
    {
        _maxSpeed = maxSpeedSlider.value;
        maxValueLabel.text = _maxSpeed.ToString("0.0");
    }

    public void SetSafeDistance()
    {
        _safeDistance = safeDistanceSlider.value;
        safeDistanceLabel.text = _safeDistance.ToString("0.0");
    }

    public void Back()
    {
        _manager.DestroyEntity(_manager.GetAllEntities());
        SceneManager.LoadScene(0);
    }
    
    /*private void Update()
    {
        foreach (var car in Cars)
        {
            var t = _manager.GetComponentData<Translation>(car);
            var p = _manager.GetComponentData<PathHelper>(car);
            var s = _manager.GetComponentData<MoveSpeed>(car);
            Debug.Log("index in queue = " + p.CurrentIndexInRoad + " pos = " + t.Value + " speed = " + s.CurrentSpeed);
        }
    }*/
}
