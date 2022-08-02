using System.Diagnostics;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class StressTestManager : MonoBehaviour
{
    [SerializeField] private InstanciationMode instanciationMode;
    [SerializeField] private int numberToSpawn;
    [SerializeField] private bool outputTime = true;

    [Header("Movement")]
    [SerializeField] private bool useJob;
    [SerializeField] private bool burstJob;
    [SerializeField] private float movementSpeed;

    public float MovementSpeed => movementSpeed;

    [Header("UI")]
    [SerializeField] private UIManager uiManager;

    [Header("Spawn Boundaries")]
    [SerializeField] private float topBounds;
    [SerializeField] private float bottomBounds;
    [SerializeField] private float leftBounds;
    [SerializeField] private float rightBounds;
    [SerializeField] private float heightRange = 10;

    public float HeightRange => heightRange;

    [Header("Conversion Setting")]
    [SerializeField] private bool convertionOnCreate = true;

    [Header("Components & Prefabs")]
    [SerializeField] private Mesh unitMesh;
    [SerializeField] private Material unitMaterial;
    [SerializeField] private float unitScale = 0.0f;
    [Space]
    [SerializeField] private GameObject classicPrefab;

    private int totalInstanciated;
    private EntityArchetype entityArchetype;
    private Entity conversionEnity;

    StressTestStatistics testStats = new StressTestStatistics(1);
    Stopwatch stopWatch = new Stopwatch();

    private EntityManager entityManager;
    private World defaultWorld;

    public static StressTestManager globalManager;

    // Start is called before the first frame update
    void Start()
    {
        globalManager = this;

        if (uiManager == null)
            uiManager = GameObject.FindObjectOfType<UIManager>();

        switch (instanciationMode)
        {
            case InstanciationMode.ECSPure:
                SetupPureECS();
                break;
            case InstanciationMode.ECSConversion:
                SetupECSConversion();
                break;

            default:
                break;
        }

        SetMovementMode();

        uiManager.DisplayInstanceMode(instanciationMode);
        uiManager.DisplayTotalInstanced(totalInstanciated);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            stopWatch.Reset();
            if (outputTime)
                stopWatch.Start();

            SpawnUnit(numberToSpawn);

            if (outputTime)
            {
                stopWatch.Stop();
                testStats.AddValue(stopWatch.Elapsed.TotalMilliseconds);

                UnityEngine.Debug.Log($"{totalInstanciated} units instanced. Average instance time is {testStats.MeanTime}ms +/- {testStats.SigmaDeviation}ms");
                UnityEngine.Debug.Log("Total time elapsed: " + stopWatch.Elapsed.TotalMilliseconds + " ms");
            }
        }
    }

    private void SetupPureECS()
    {
        defaultWorld = World.DefaultGameObjectInjectionWorld;
        entityManager = defaultWorld.EntityManager;

        entityArchetype = entityManager.CreateArchetype
        (
            typeof(Translation),
            typeof(Rotation),
            typeof(LocalToWorld),
            typeof(NonUniformScale),

            typeof(RenderMesh),
            typeof(RenderBounds),

            typeof(MovementComponent)
        );
    }

    private void SetupECSConversion()
    {
        defaultWorld = World.DefaultGameObjectInjectionWorld;
        entityManager = defaultWorld.EntityManager;

        if (convertionOnCreate)
            return;

        GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(defaultWorld, null);
        conversionEnity = GameObjectConversionUtility.ConvertGameObjectHierarchy(classicPrefab, settings);
    }

    private void SpawnUnit(int numberToSpawn)
    {
        totalInstanciated += numberToSpawn;
        uiManager.DisplayTotalInstanced(totalInstanciated);

        switch (instanciationMode)
        {
            case InstanciationMode.Classic:
                SpawnClassic();
                break;
            case InstanciationMode.ECSConversion:
                SpawnECSConversion();
                break;
            case InstanciationMode.ECSPure:
                SpawnECSPure();
                break;
        }
    }

    private void SpawnClassic()
    {
        for (int i = 0; i < numberToSpawn; i++)
        {
            GameObject instance = Instantiate(classicPrefab);
            instance.transform.position = GetRandomPosition();
            instance.transform.localScale = new float3(GetRandomScale(unitScale));
        }
    }

    private void SpawnECSConversion()
    {
        for (int i = 0; i < numberToSpawn; i++)
        {
            Entity myEntity;

            if (convertionOnCreate)
            {
                GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(defaultWorld, null);
                Entity conversion = GameObjectConversionUtility.ConvertGameObjectHierarchy(classicPrefab, settings);
                myEntity = entityManager.Instantiate(conversion);
            }
            else
            {
                myEntity = entityManager.Instantiate(conversionEnity);
            }

            entityManager.SetComponentData(myEntity, new Translation { Value = GetRandomPosition() });
            entityManager.SetComponentData(myEntity, new NonUniformScale { Value = GetRandomScale(unitScale) });
            entityManager.AddComponentData(myEntity, new MovementComponent { MoveSpeed = this.movementSpeed });
        }
    }

    private void SpawnECSPure()
    {
        for (int i = 0; i < numberToSpawn; i++)
        {
            Entity myEntity = entityManager.CreateEntity(entityArchetype);

            entityManager.SetComponentData(myEntity, new Translation { Value = GetRandomPosition() });
            entityManager.SetComponentData(myEntity, new NonUniformScale { Value = GetRandomScale(unitScale) });
            entityManager.SetComponentData(myEntity, new MovementComponent { MoveSpeed = this.movementSpeed });

            entityManager.SetSharedComponentData(myEntity, new RenderMesh
            {
                mesh = unitMesh,
                material = unitMaterial,
                layerMask = 1
            });
        }
    }

    private void SetMovementMode()
    {
        defaultWorld = World.DefaultGameObjectInjectionWorld;

        defaultWorld.GetOrCreateSystem<MovementSystem>();
        defaultWorld.GetOrCreateSystem<MovementSystemJobs>();
        defaultWorld.GetOrCreateSystem<MovementSystemJobsBurst>();

        if (instanciationMode != InstanciationMode.Classic)
        {
            if (!useJob)
                World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<MovementSystem>().Enabled = true;
            else if (burstJob)
                World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<MovementSystemJobsBurst>().Enabled = true;
            else if (!burstJob)
                World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<MovementSystemJobs>().Enabled = true;
        }
    }

    private float3 GetRandomPosition()
    {
        float randomX = UnityEngine.Random.Range(leftBounds, rightBounds);
        float randomY = UnityEngine.Random.Range(topBounds, bottomBounds);
        float randomZ = UnityEngine.Random.Range(0f, 1f) * heightRange;
        return new float3(randomX, randomY, randomZ);
    }

    private float GetRandomScale(float scaleMax)
    {
        float scaleMin = 0.1f;
        return UnityEngine.Random.Range(scaleMin, scaleMax);
    }
}
