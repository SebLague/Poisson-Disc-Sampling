using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PoissonDiscSampling
{
    public static List<Vector3> GeneratePointsOfDifferentSize(Vector2 radiusRange, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30)
    {
        // use the min of the radius range for our cell size
        float cellSize = radiusRange.x / Mathf.Sqrt(2);

        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
        List<Vector2> points = new List<Vector2>(); // where the points will actually be
        List<Vector2> spawnPoints = new List<Vector2>(); // a place we'll try to spawn points around
        List<Vector3> pointsPlusRadius = new List<Vector3>(); // what we'll use to keep track of the points radii

        spawnPoints.Add(sampleRegionSize / 2); // add first spawn point in the center
        while (spawnPoints.Count > 0)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Count); // pick random guy to spawn
            Vector2 spawnCentre = spawnPoints[spawnIndex];
            bool candidateAccepted = false;

            // Pick a random radius, try it n times
            float radius = Random.Range(radiusRange.x, radiusRange.y);

            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                float angle = Random.value * Mathf.PI * 2; // pick random direction to try
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)); // get dir from angle
                Vector2 candidate = spawnCentre + dir * Mathf.Sqrt(Random.Range(radius * radius, 4 * radius * radius));
                if (IsVaryingRadiusCandidateValid(candidate, sampleRegionSize, cellSize, radius, points, grid, pointsPlusRadius))
                {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    pointsPlusRadius.Add(new Vector3(candidate.x, candidate.y, radius));
                    grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
                    candidateAccepted = true;
                    break;
                }
            }
            if (!candidateAccepted)
            {
                spawnPoints.RemoveAt(spawnIndex);
            }
        }

        return pointsPlusRadius;
    }
    
    static bool IsVaryingRadiusCandidateValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid, List<Vector3> pointsPlusRadius)
    {
        // Only look if candidate is within the bounds of the grid, taking its radius into account
        if (candidate.x - radius + 1 >= 0 &&
            candidate.x + radius - 1 < sampleRegionSize.x &&
            candidate.y - radius + 1 >= 0 &&
            candidate.y + radius - 1 < sampleRegionSize.y)
        {
            // Get cell indecies
            int cellX = (int)(candidate.x / cellSize);
            int cellY = (int)(candidate.y / cellSize);

            // Characterize our search area
            int searchAreaSide = (int)(2f * radius) + 3;
            int searchAddition = (searchAreaSide - 1) / 2;

            // Get starting and ending positions for where we will search with additional range to compensate for our radius
            int searchStartX = Mathf.Max(0, cellX - searchAddition);
            int searchEndX = Mathf.Min(cellX + searchAddition, grid.GetLength(0) - 1);
            int searchStartY = Mathf.Max(0, cellY - searchAddition);
            int searchEndY = Mathf.Min(cellY + searchAddition, grid.GetLength(1) - 1);

            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    int pointIndex = grid[x, y] - 1; // cause its a zero based array
                    if (pointIndex != -1)
                    {
                        // Check the distance between our candidate point and the radius of the point we've encountered
                        float sqrDst = (candidate - points[pointIndex]).sqrMagnitude;
                        if(sqrDst < pointsPlusRadius[pointIndex].z * pointsPlusRadius[pointIndex].z)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        return false;
    }

    #region Static Radius

    public static List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30)
    {
        float cellSize = radius / Mathf.Sqrt(2);

        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
        List<Vector2> points = new List<Vector2>(); // where the points will actually be
        List<Vector2> spawnPoints = new List<Vector2>(); // a place we'll try to spawn points around

        spawnPoints.Add(sampleRegionSize / 2); // add first spawn point in the center
        while (spawnPoints.Count > 0)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Count); // pick random guy to spawn
            Vector2 spawnCentre = spawnPoints[spawnIndex]; //
            bool candidateAccepted = false;

            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                float angle = Random.value * Mathf.PI * 2; // pick random direction to try
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)); // get dir from angle
                Vector2 candidate = spawnCentre + dir * Random.Range(radius, 2 * radius); // 
                if (IsValid(candidate, sampleRegionSize, cellSize, radius, points, grid))
                {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
                    candidateAccepted = true;
                    break;
                }
            }
            if (!candidateAccepted)
            {
                spawnPoints.RemoveAt(spawnIndex);
            }
        }

        return points;
    }

    static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid)
    {
        if (candidate.x >= 0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 && candidate.y < sampleRegionSize.y)
        {
            // Get cell indecies
            int cellX = (int)(candidate.x / cellSize);
            int cellY = (int)(candidate.y / cellSize);

            // Get starting and ending positions for where we will search
            int searchStartX = Mathf.Max(0, cellX - 2);
            int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
            int searchStartY = Mathf.Max(0, cellY - 2);
            int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    int pointIndex = grid[x, y] - 1; // cause its zero based?
                    if (pointIndex != -1)
                    {
                        // find distance between candidate point and the point we discovered was near us
                        float sqrDst = (candidate - points[pointIndex]).sqrMagnitude;
                        if (sqrDst < radius * radius)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        return false;
    }

    #endregion Static Radius
}
