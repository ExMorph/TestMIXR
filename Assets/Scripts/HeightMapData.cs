using System;

/// <summary>
/// Хранит высоту накопленного материала в каждой точке регулярной сетки.
/// Представляет собой плоский float[] размером resolutionX * resolutionZ.
/// </summary>
public class HeightMapData
{
    public int ResolutionX { get; }
    public int ResolutionZ { get; }
    public float[] Heights { get; }

    private readonly int _totalSize;

    public HeightMapData(int resolutionX, int resolutionZ)
    {
        ResolutionX = resolutionX;
        ResolutionZ = resolutionZ;
        _totalSize = resolutionX * resolutionZ;
        Heights = new float[_totalSize];
    }

    /// <summary>
    /// Преобразует 2D координаты сетки (x, z) в индекс плоского массива.
    /// Сетка хранится по строкам: индекс = z * resolutionX + x.
    /// </summary>
    public int GetIndex(int x, int z)
    {
        return z * ResolutionX + x;
    }

    /// <summary>
    /// Получить высоту в точке сетки.
    /// </summary>
    public float GetHeight(int x, int z)
    {
        return Heights[GetIndex(x, z)];
    }

    /// <summary>
    /// Установить высоту в точке сетки.
    /// </summary>
    public void SetHeight(int x, int z, float value)
    {
        Heights[GetIndex(x, z)] = value;
    }

    /// <summary>
    /// Сбросить все высоты в ноль.
    /// </summary>
    public void Reset()
    {
        Array.Clear(Heights, 0, _totalSize);
    }

    /// <summary>
    /// Получить высоту с ограничением по границам сетки.
    /// За пределами возвращает 0.
    /// </summary>
    public float GetHeightClamped(int x, int z)
    {
        if (x < 0 || x >= ResolutionX || z < 0 || z >= ResolutionZ)
            return 0f;
        return Heights[GetIndex(x, z)];
    }
}
