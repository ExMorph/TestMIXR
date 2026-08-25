using UnityEngine;

/// <summary>
/// Связывает HeightMapData с отображающим Mesh.
/// Обновляет Y-координаты вершин меша на основе данных высот.
/// Переиспользует массивы - без аллокаций в рантайме.
/// </summary>
public class MeshUpdater
{
    private readonly Mesh _mesh;
    private readonly Vector3[] _vertices;
    private readonly int _resolutionX;
    private readonly int _resolutionZ;

    /// <summary>
    /// Создать MeshUpdater для заданного меша.
    /// Запоминает ссылку на массив вершин.
    /// </summary>
    public MeshUpdater(Mesh mesh, int resolutionX, int resolutionZ)
    {
        _mesh = mesh;
        _resolutionX = resolutionX;
        _resolutionZ = resolutionZ;

        // Копируем массив вершин из меша — работаем с копией,
        // потом записываем обратно одним вызовом mesh.vertices = ...
        _vertices = mesh.vertices;
    }

    /// <summary>
    /// Обновить все вершины меша на основе текущих данных высот.
    /// После обновления пересчитывает нормали для правильного освещения.
    /// 
    /// Порядок вершин в _vertices совпадает с порядком в HeightMapData:
    /// index = z * resolutionX + x.
    /// </summary>
    public void UpdateMesh(HeightMapData heightMap)
    {
        for (int z = 0; z < _resolutionZ; z++)
        {
            for (int x = 0; x < _resolutionX; x++)
            {
                int idx = z * _resolutionX + x;
                _vertices[idx].y = heightMap.Heights[idx];
            }
        }

        // Записываем обновлённый массив вершин обратно в меш
        _mesh.vertices = _vertices;

        // Пересчитываем нормали — без них освещение будет некорректным
        // (тени, блики не будут реагировать на изменение формы)
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
    }
}
