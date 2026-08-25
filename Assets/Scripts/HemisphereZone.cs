using UnityEngine;

/// <summary>
/// Полусферическая зона воздействия.
/// Хранит позицию, вычисляет текущий радиус (с пульсацией по AnimationCurve)
/// и определяет высоту материала в заданной точке.
/// </summary>
[System.Serializable]
public class HemisphereZone
{
    [Header("Movement")]
    [Tooltip("Скорость перемещения зоны по поверхности")]
    public float MoveSpeed = 5f;

    [Header("Radius")]
    [Tooltip("Базовый радиус полусферы")]
    public float BaseRadius = 1f;

    [Tooltip("Амплитуда изменения радиуса (0 = без пульсации)")]
    public float RadiusAmplitude = 0.3f;

    [Tooltip("Частота изменения радиуса (период в секундах)")]
    public float RadiusFrequency = 2f;

    [Tooltip("Кривая изменения радиуса в пределах одного периода.\n" +
             "X = время (0..1 в пределах периода), Y = множитель амплитуды (-1..1).\n" +
             "Значение Y=0 в начале и конце создаёт плавное увеличение-уменьшение.")]
    public AnimationCurve RadiusCurve = AnimationCurve.EaseInOut(0f, -1f, 1f, -1f);

    /// <summary>Текущая позиция центра зоны в мировых координатах (только X, Z).</summary>
    public Vector2 Position { get; set; }

    /// <summary>Текущий вычисленный радиус с учётом пульсации.</summary>
    public float CurrentRadius { get; private set; }

    /// <summary>Предыдущая позиция зоны (для интерполяции траектории).</summary>
    public Vector2 PreviousPosition { get; set; }

    private float _time;

    /// <summary>
    /// Обновить радиус зоны. Вызывается каждый кадр.
    /// Радиус = BaseRadius + Amplitude * Curve(t % period).
    /// </summary>
    public void UpdateRadius(float deltaTime)
    {
        _time += deltaTime;

        if (RadiusFrequency > 0f)
        {
            // Вычисляем нормализованное время в пределах одного периода [0, 1)
            float period = 1f / RadiusFrequency;
            float normalizedTime = (_time % period) / period;

            // AnimationCurve возвращает значение Y для заданного X
            float curveValue = RadiusCurve.Evaluate(normalizedTime);
            CurrentRadius = BaseRadius + RadiusAmplitude * curveValue;
        }
        else
        {
            CurrentRadius = BaseRadius;
        }

        // Радиус не может быть отрицательным
        if (CurrentRadius < 0f)
            CurrentRadius = 0f;
    }

    /// <summary>
    /// Вычислить высоту полусферы в точке (pointX, pointZ) относительно центра зоны.
    /// Возвращает 0, если точка за пределами радиуса.
    /// 
    /// Формула полусферы: y = sqrt(R² - d²)
    /// где d — расстояние от центра до точки в плоскости XZ.
    /// </summary>
    /// <param name="pointX">X координата проверяемой точки</param>
    /// <param name="pointZ">Z координата проверяемой точки</param>
    /// <returns>Высота полусферы в данной точке (0 если за пределами)</returns>
    public float GetHemisphereHeight(float pointX, float pointZ)
    {
        float dx = pointX - Position.x;
        float dz = pointZ - Position.y; // Position.y хранит Z координату мира
        float distSq = dx * dx + dz * dz;
        float rSq = CurrentRadius * CurrentRadius;

        if (distSq >= rSq)
            return 0f;

        // Высота полусферы: sqrt(R² - d²)
        return Mathf.Sqrt(rSq - distSq);
    }

    /// <summary>
    /// Проверить, находится ли точка внутри зоны (в проекции на плоскость XZ).
    /// </summary>
    public bool IsInside(float pointX, float pointZ)
    {
        float dx = pointX - Position.x;
        float dz = pointZ - Position.y;
        float distSq = dx * dx + dz * dz;
        return distSq < CurrentRadius * CurrentRadius;
    }

    /// <summary>
    /// Расстояние от точки до центра зоны в плоскости XZ (без квадратного корня для оптимизации).
    /// Используется для быстрой проверки расстояния.
    /// </summary>
    public float GetDistanceSq(float pointX, float pointZ)
    {
        float dx = pointX - Position.x;
        float dz = pointZ - Position.y;
        return dx * dx + dz * dz;
    }
}
