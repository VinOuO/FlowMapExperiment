using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
public class FlowMapBaker : MonoBehaviour
{
    public RenderTexture RenderMap;
    public Texture2D Result;
    public Vector3 Origin;
    public Vector2 WidthHeight;
    private void Start()
    {
        StartCoroutine(BakingFlowMap());
    }

    void Update()
    {
        //Debug.DrawLine(Path.corners[i], Path.corners[i + 1], Color.red);
    }

    void TexToPNG(Texture2D tex)
    {
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/ResultMap.png", bytes);
    }

    void InitMap()
    {
        Result = new Texture2D(256, 256, TextureFormat.RGB24, false);
        //RenderTexture.active = RenderMap;
        //Result.ReadPixels(new Rect(0, 0, RenderMap.width, RenderMap.height), 0, 0);
        //Result.Apply();
    }

    Vector3 PixelToLocation(int texX, int texY)
    {
        return Origin + ((float)texX / (float)Result.width * WidthHeight.x) * Vector3.right + ((float)texY / (float)Result.height * WidthHeight.y) * Vector3.forward;
    }

    public List<PointCondition> PointConditions = new List<PointCondition>();
    IEnumerator BakingFlowMap()
    {
        YieldInstruction wait = new WaitForEndOfFrame();
        List<PointCondition> pointConditions = new List<PointCondition>();

        InitMap();
        for (int texX = 0; texX < Result.width; texX++)
        {
            for (int texY = 0; texY < Result.height; texY++)
            {
                pointConditions.Add(new PointCondition(PixelToLocation(texX, texY), PixelToLocation(Result.width, texY), new Vector2Int(texX, texY)));
            }
        }
        PointConditions = pointConditions;
        Debug.Log("Finished Condition");
        yield return wait;
        foreach (PointCondition condition in pointConditions)
        {
            condition.GetConditionAnswer();
            Result.SetPixel(condition.PixelPos.x, condition.PixelPos.y, condition.Col);
        }
        Debug.Log("Finished Color");
        yield return wait;
        TexToPNG(Result);
        Debug.Log("Finished Export");
    }
}

[System.Serializable]
public class PointCondition
{
    public Vector3 Origin;
    public Vector3 Goal;
    public Vector2Int PixelPos;
    public Vector3 Dir;
    public Color Col;
    public PointCondition(Vector3 origin, Vector3 goal, Vector2Int pixelPos)
    {
        Origin = origin;
        Goal = goal;
        PixelPos = pixelPos;
    }

    public void GetConditionAnswer()
    {
        NavMeshPath tmpPath = new NavMeshPath();
        NavMesh.CalculatePath(Origin, Goal, NavMesh.AllAreas, tmpPath);
        if (tmpPath.corners.Length >= 2)
        {
            Dir = tmpPath.corners[1] - tmpPath.corners[0];
        }
        else
        {
            Dir = Vector2.zero;
        }
        Dir = Dir.normalized * 0.5f;
        Col =  new Color(Dir.x + 0.5f, Dir.y + 0.5f, 0);
    }
}