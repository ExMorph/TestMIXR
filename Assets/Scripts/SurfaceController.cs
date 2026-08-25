using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Оркестратор: связывает все компоненты воедино.
/// Управляет вводом, движением зоны, накоплением и обновлением меша.
/// Это единственный MonoBehaviour в системе.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SurfaceController : MonoBehaviour
{
    [Header("Surface")]
    [Tooltip("Ширина плоскости (по оси X)")]
    public float SurfaceWidth = 20f;

    [Tooltip("Глубина плоскости (по оси Z)")]
    public float SurfaceDepth = 20f;

    [Tooltip("Количество сегментов (ячеек) по X. Вершин будет на 1 больше.")]
    public int ResolutionX = 100;

    [Tooltip("Количество сегментов (ячеек) по Z. Вершин будет на 1 больше.")]
    public int ResolutionZ = 100;

    [Header("Zone Settings")]
    public HemisphereZone Zone = new HemisphereZone();

    [Header("Accumulation")]
    public MaterialAccumulator Accumulator = new MaterialAccumulator();

    private HeightMapData _heightMap;
    private MeshUpdater _meshUpdater;
    private Mesh _mesh;

    private float _cellSizeX;
    private float _cellSizeZ;
    private float _offsetX;
    private float _offsetZ;

    private void Start()
    {
        // Размер ячейки сетки — расстояние между соседними вершинами
        _cellSizeX = SurfaceWidth / ResolutionX;
        _cellSizeZ = SurfaceDepth / ResolutionZ;

        // Смещение: первая вершина находится в (-halfWidth, 0, -halfDepth)
        _offsetX = -SurfaceWidth * 0.5f;
        _offsetZ = -SurfaceDepth * 0.5f;

        // Количество вершин = segments + 1 (границы сетки включены)
        int vertexCountX = ResolutionX + 1;
        int vertexCountZ = ResolutionZ + 1;

        // Data layer хранит ровно столько же, сколько вершин в меше
        _heightMap = new HeightMapData(vertexCountX, vertexCountZ);

        _mesh = MeshGenerator.Generate(ResolutionX, ResolutionZ, SurfaceWidth, SurfaceDepth);
        GetComponent<MeshFilter>().mesh = _mesh;

        // MeshUpdater оперирует количеством вершин — индексы совпадают 1:1
        _meshUpdater = new MeshUpdater(_mesh, vertexCountX, vertexCountZ);

        // Зона стартует в центре плоскости
        Vector2 center = new Vector2(0f, 0f);
        Zone.Position = center;
        Zone.PreviousPosition = center;
    }

    private void Update()
    {
        HandleInput();
        UpdateZone();
        HandleAccumulation();
        _meshUpdater.UpdateMesh(_heightMap);
    }

    /// <summary>
    /// Чтение ввода: WASD для движения, R для сброса.
    /// Используем Input System API напрямую — Keyboard.current.
    /// </summary>
    private void HandleInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Сброс поверхности по клавише R
        if (keyboard.rKey.wasPressedThisFrame)
        {
            _heightMap.Reset();
        }
    }

    /// <summary>
    /// Движение зоны по WASD. Вызывается каждый кадр.
    /// </summary>
    private void UpdateZone()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        Zone.PreviousPosition = Zone.Position;

        // Ввод по осям: X = A/D, Z = W/S (мир)
        // В нашем случае Vector2.x = мир X, Vector2.y = мир Z
        float inputX = 0f;
        float inputZ = 0f;
        if (keyboard.aKey.isPressed) inputX -= 1f;
        if (keyboard.dKey.isPressed) inputX += 1f;
        if (keyboard.wKey.isPressed) inputZ += 1f;
        if (keyboard.sKey.isPressed) inputZ -= 1f;

        Vector2 input = new Vector2(inputX, inputZ);
        if (input.sqrMagnitude > 1f)
            input.Normalize();

        // Перемещение
        Vector2 movement = input * (Zone.MoveSpeed * Time.deltaTime);
        Vector2 newPos = Zone.Position + movement;

        // Ограничение в пределах плоскости
        float halfWidth = SurfaceWidth * 0.5f;
        float halfDepth = SurfaceDepth * 0.5f;
        newPos.x = Mathf.Clamp(newPos.x, -halfWidth, halfWidth);
        newPos.y = Mathf.Clamp(newPos.y, -halfDepth, halfDepth);

        Zone.Position = newPos;

        // Обновляем радиус (пульсация)
        Zone.UpdateRadius(Time.deltaTime);
    }

    /// <summary>
    /// Накопление материала при зажатом пробеле.
    /// </summary>
    private void HandleAccumulation()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.spaceKey.isPressed)
        {
            Accumulator.Accumulate(_heightMap, Zone, _cellSizeX, _cellSizeZ, _offsetX, _offsetZ, Time.deltaTime);
        }
    }

    /// <summary>
    /// Публичный метод сброса — можно вызвать из UI или другого скрипта.
    /// </summary>
    public void ResetSurface()
    {
        if (_heightMap != null)
            _heightMap.Reset();
    }
}
