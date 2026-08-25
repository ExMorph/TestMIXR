using UnityEngine;

/// <summary>
/// Генерирует регулярную сетку (плоскость) через API Mesh.
/// </summary>
public static class MeshGenerator
{
    /// <summary>
    /// Создаёт Mesh плоскости с заданным количеством сегментов.
    /// Вершины располагаются равномерно в диапазоне [-halfWidth..halfWidth] по X
    /// и [-halfDepth..halfDepth] по Z. Y = 0 для всех вершин.
    /// </summary>
    /// <param name="segmentsX">Количество сегментов по X (количество ячеек)</param>
    /// <param name="segmentsZ">Количество сегментов по Z</param>
    /// <param name="width">Общая ширина плоскости (по X)</param>
    /// <param name="depth">Общая глубина плоскости (по Z)</param>
    /// <returns>Готовый Mesh с вершинами, треугольниками и UV</returns>
    public static Mesh Generate(int segmentsX, int segmentsZ, float width, float depth)
    {
        // Количество вершин = (segments + 1) в каждом направлении
        // Например: 10 сегментов → 11 вершин (от 0 до 10)
        int vertCountX = segmentsX + 1;
        int vertCountZ = segmentsZ + 1;
        int totalVertices = vertCountX * vertCountZ;

        // Количество треугольников: каждый сегмент = 2 треугольника, каждый треугольник = 3 индекса
        int totalTriangles = segmentsX * segmentsZ * 6;

        Vector3[] vertices = new Vector3[totalVertices];
        Vector2[] uv = new Vector2[totalVertices];
        int[] triangles = new int[totalTriangles];

        float halfWidth = width * 0.5f;
        float halfDepth = depth * 0.5f;
        float stepX = width / segmentsX;
        float stepZ = depth / segmentsZ;

        // Генерация вершин и UV
        // Вершины нумеруются по строкам: сначала вся строка Z=0, потом Z=1 и т.д.
        // Это совпадает с порядком индексов в HeightMapData: index = z * resX + x
        for (int z = 0; z < vertCountZ; z++)
        {
            for (int x = 0; x < vertCountX; x++)
            {
                int idx = z * vertCountX + x;

                // Позиция вершины: от (-halfWidth, 0, -halfDepth) до (+halfWidth, 0, +halfDepth)
                float posX = -halfWidth + x * stepX;
                float posZ = -halfDepth + z * stepZ;
                vertices[idx] = new Vector3(posX, 0f, posZ);

                // UV координаты от (0,0) до (1,1) — для текстурирования
                uv[idx] = new Vector2(
                    (float)x / segmentsX,
                    (float)z / segmentsZ
                );
            }
        }

        // Генерация треугольников
        // Каждая ячейка сетки (x, z) → (x+1, z+1) делится на 2 треугольника:
        //
        //  (z+1)*vertCountX+x --- (z+1)*vertCountX+x+1
        //          |              / |
        //          |            /   |
        //          |          /     |
        //     z*vertCountX+x --- z*vertCountX+x+1
        //
        int triIdx = 0;
        for (int z = 0; z < segmentsZ; z++)
        {
            for (int x = 0; x < segmentsX; x++)
            {
                int bl = z * vertCountX + x;       // bottom-left
                int br = bl + 1;                     // bottom-right
                int tl = (z + 1) * vertCountX + x; // top-left
                int tr = tl + 1;                     // top-right

                // Первый треугольник (bl, tl, tr)
                triangles[triIdx++] = bl;
                triangles[triIdx++] = tl;
                triangles[triIdx++] = tr;

                // Второй треугольник (bl, tr, br)
                triangles[triIdx++] = bl;
                triangles[triIdx++] = tr;
                triangles[triIdx++] = br;
            }
        }

        Mesh mesh = new Mesh
        {
            name = "SurfaceMesh",
            vertices = vertices,
            uv = uv,
            triangles = triangles
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
