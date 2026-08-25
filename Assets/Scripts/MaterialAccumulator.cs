using UnityEngine;

/// <summary>
/// Логика накопления материала на поверхности.
/// Решает три ключевые задачи:
/// 1. Интерполяция позиции между кадрами (непрерывный след)
/// 2. Заполнение высоты по формуле полусферы
/// 3. Суммирование проходов (материал только добавляется)
/// </summary>
[System.Serializable]
public class MaterialAccumulator
{
    [Tooltip("Скорость накопления материала (единиц высоты в секунду)")]
    public float AccumulationSpeed = 0.5f;

    /// <summary>
    /// Накопить материал вдоль траектории от previousPos к currentPos.
    /// 
    /// Алгоритм:
    /// 1. Вычисляем вектор перемещения delta = currentPos - previousPos
    /// 2. Разбиваем отрезок на шаги размером = min(cellSizeX, cellSizeZ)
    /// 3. В каждой промежуточной точке "рисуем" полусферу
    /// 4. Для каждой вершины сетки: если полусфера выше текущей высоты — поднимаем
    /// 
    /// Это гарантирует непрерывный след даже при высокой скорости.
    /// </summary>
    public void Accumulate(
        HeightMapData heightMap,
        HemisphereZone zone,
        float cellSizeX,
        float cellSizeZ,
        float worldOffsetX,
        float worldOffsetZ,
        float deltaTime)
    {
        float heightDelta = AccumulationSpeed * deltaTime;
        if (heightDelta <= 0f)
            return;

        Vector2 prev = zone.PreviousPosition;
        Vector2 curr = zone.Position;

        Vector2 delta = curr - prev;
        float distance = delta.magnitude;

        // Минимальный размер ячейки — гарантирует перекрытие всех вершин
        float minCellSize = Mathf.Min(cellSizeX, cellSizeZ);

        if (distance < 0.0001f)
        {
            AccumulateAtPosition(heightMap, zone, curr.x, curr.y, cellSizeX, cellSizeZ, worldOffsetX, worldOffsetZ, heightDelta);
            return;
        }

        int steps = Mathf.CeilToInt(distance / minCellSize);
        if (steps < 1) steps = 1;

        Vector2 stepDir = delta / steps;

        for (int i = 0; i <= steps; i++)
        {
            Vector2 samplePos = prev + stepDir * i;
            AccumulateAtPosition(heightMap, zone, samplePos.x, samplePos.y, cellSizeX, cellSizeZ, worldOffsetX, worldOffsetZ, heightDelta);
        }
    }

    /// <summary>
    /// В одной точке пространства: для каждой вершины сетки в радиусе зоны
    /// вычисляем высоту полусферы и добавляем материал.
    /// </summary>
    private void AccumulateAtPosition(
        HeightMapData heightMap,
        HemisphereZone zone,
        float centerX,
        float centerZ,
        float cellSizeX,
        float cellSizeZ,
        float worldOffsetX,
        float worldOffsetZ,
        float heightDelta)
    {
        float radius = zone.CurrentRadius;

        // Bounding box в индексах сетки — оптимизация, чтобы не перебирать все вершины
        int minGridX = Mathf.Max(0, Mathf.FloorToInt((centerX - radius - worldOffsetX) / cellSizeX));
        int maxGridX = Mathf.Min(heightMap.ResolutionX - 1, Mathf.CeilToInt((centerX + radius - worldOffsetX) / cellSizeX));
        int minGridZ = Mathf.Max(0, Mathf.FloorToInt((centerZ - radius - worldOffsetZ) / cellSizeZ));
        int maxGridZ = Mathf.Min(heightMap.ResolutionZ - 1, Mathf.CeilToInt((centerZ + radius - worldOffsetZ) / cellSizeZ));

        for (int z = minGridZ; z <= maxGridZ; z++)
        {
            for (int x = minGridX; x <= maxGridX; x++)
            {
                float worldX = worldOffsetX + x * cellSizeX;
                float worldZ = worldOffsetZ + z * cellSizeZ;

                float hemisphereHeight = zone.GetHemisphereHeight(worldX, worldZ);
                if (hemisphereHeight <= 0f)
                    continue;

                int idx = heightMap.GetIndex(x, z);
                float currentHeight = heightMap.Heights[idx];

                // Материал только добавляется, ограничивается высотой полусферы
                float newHeight = currentHeight + heightDelta;
                if (newHeight > hemisphereHeight)
                    newHeight = hemisphereHeight;

                if (newHeight > currentHeight)
                    heightMap.Heights[idx] = newHeight;
            }
        }
    }
}
