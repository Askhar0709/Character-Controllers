using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using System;
public class Too : MonoBehaviour
{
    public static T GetData<T>(Data key)
    {
        object res = null;
        if (GameController.Datas.ContainsKey(key))
        {
            if (Type.Equals(GameController.Datas[key].GetType(), typeof(bool)))
            {
                res = PlayerPrefs.GetInt("" + key, (bool)GameController.Datas[key] ? 1 : 0) == 1;
            }
            else if (Type.Equals(GameController.Datas[key].GetType(), typeof(int)))
            {
                res = PlayerPrefs.GetInt("" + key, (int)GameController.Datas[key]);
            }
            else if (Type.Equals(GameController.Datas[key].GetType(), typeof(float)))
            {
                res = PlayerPrefs.GetFloat("" + key, (float)GameController.Datas[key]);
            }
            else if (Type.Equals(GameController.Datas[key].GetType(), typeof(string)))
            {
                res = PlayerPrefs.GetString("" + key, (string)GameController.Datas[key]);
            }
            else if (Type.Equals(GameController.Datas[key].GetType(), typeof(Vector3)))
            {
                res = Too.String2Vector3(PlayerPrefs.GetString("" + key, "" + (Vector3)GameController.Datas[key]));
            }
        }
        return (T)res;
    }
    public static void SetData(Data key, object value)
    {
        if (GameController.Datas.ContainsKey(key))
        {
            if (Type.Equals(GameController.Datas[key].GetType(), typeof(bool)))
            {
                PlayerPrefs.SetInt("" + key, (bool)value ? 1 : 0);
            }
            else if (Type.Equals(GameController.Datas[key].GetType(), typeof(int)))
            {
                PlayerPrefs.SetInt("" + key, (int)value);
            }
            else if (Type.Equals(GameController.Datas[key].GetType(), typeof(float)))
            {
                PlayerPrefs.SetFloat("" + key, (float)value);
            }
            else if (Type.Equals(GameController.Datas[key].GetType(), typeof(string)))
            {
                PlayerPrefs.SetString("" + key, (string)value);
            }
            else if (Type.Equals(GameController.Datas[key].GetType(), typeof(Vector3)))
            {
                PlayerPrefs.SetString("" + key, "" + (Vector3)value);
            }
        }
    }
    public static GameController GetGameController()
    {
        return GameObject.Find("GameController").GetComponent<GameController>();
    }
    public static string GetProjectName()
    {
        string[] s = Application.dataPath.Split('/');
        return s[s.Length - 2];
    }
    public static void Swap<T>(ref T a, ref T b)
    {
        T t = a;
        a = b;
        b = t;
    }
    public static bool IsBetween(float num, float min, float max)
    {
        return (min > max) ? (num >= max && num <= min) : (num >= min && num <= max);
    }
    public static float Resize(float num, float minA, float maxA, float minB, float maxB)
    {
        return (num - minA) / (maxA - minA) * (maxB - minB) + minB;
    }
    public static Vector3 Player2TargetVel(Transform playerTf, Vector3 targetPos, float angle)
    {
        Vector3 projectileXZPos = new Vector3(playerTf.position.x, 0.0f, playerTf.position.z);
        Vector3 targetXZPos = new Vector3(targetPos.x, 0.0f, targetPos.z);
        playerTf.LookAt(targetXZPos);
        float distanceXZ = Vector3.Distance(projectileXZPos, targetXZPos);
        float gravity = Physics.gravity.y;
        float tanAlpha = Mathf.Tan(angle * Mathf.Deg2Rad);
        float height = targetPos.y - playerTf.position.y;
        float velocityZ = Mathf.Sqrt(Mathf.Abs(gravity * distanceXZ * distanceXZ / (2f * (height - distanceXZ * tanAlpha))));
        float velocityY = tanAlpha * velocityZ;
        Vector3 localVelocity = new Vector3(0, velocityY, velocityZ);
        Vector3 globalVelocity = playerTf.TransformDirection(localVelocity);
        return globalVelocity;
    }
    public static bool IsProbability(float p)
    {
        return UnityEngine.Random.Range(0f, 100f) <= p;
    }
    public static float RandPercent()
    {
        return UnityEngine.Random.Range(0f, 100f);
    }
    public static bool RandBool()
    {
        return UnityEngine.Random.Range(0, 2) == 0;
    }
    public static int BoolSign(bool isPositive)
    {
        return isPositive ? 1 : -1;
    }
    public static T RandEnum<T>()
    {
        var values = System.Enum.GetValues(typeof(T));
        int random = UnityEngine.Random.Range(0, values.Length);
        return (T)values.GetValue(random);
    }
    public static Vector3 String2Vector3(string strVec)
    {
        if (strVec.StartsWith("(") && strVec.EndsWith(")"))
        {
            strVec = strVec.Substring(1, strVec.Length - 2);
        }
        string[] sArray = strVec.Split(',');
        return new Vector3(float.Parse(sArray[0]), float.Parse(sArray[1]), float.Parse(sArray[2]));
    }
    public static void CreateDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
    public static List<T> Add2List<T>(List<T> a, List<T> b)
    {
        List<T> res = a;
        foreach (T e in b)
        {
            res.Add(e);
        }
        return res;
    }
    public static List<Vector3> CreateBetweenPoints(Vector3 a, Vector3 b, int betweenCount, bool isA, bool isB)
    {
        List<Vector3> res = new List<Vector3>();
        float dis = Vector3.Distance(a, b), deltaDis = dis / (betweenCount + 1);
        if (isA)
        {
            res.Add(a);
        }
        for (int i = 1; i <= betweenCount; i++)
        {
            res.Add(Vector3.Lerp(a, b, (i * deltaDis) / dis));
        }
        if (isB)
        {
            res.Add(b);
        }
        return res;
    }
    public static bool IsBetweenIdx(int idx, int minIdx, int maxIdx, int count)
    {
        idx = Too.LoopIdx(idx, count);
        return (minIdx < 0) ?
            ((maxIdx >= count) ? true : ((maxIdx >= count + minIdx) ? true : (!(maxIdx <= idx && idx <= count + minIdx)))) :
            ((maxIdx >= count) ? (minIdx <= idx) : (minIdx <= idx && idx <= maxIdx));
    }
    public static int LoopIdx(int idx, int count)
    {
        return Mathf.RoundToInt(LoopNum(idx, 0, count));
    }
    public static float LoopNum(float num, float min, float max)
    {
        if (max == min)
        {
            return min;
        }
        bool isInc = max > min;
        num = BoolSign(isInc) * (num - min);
        max = BoolSign(isInc) * (max - min);
        while (num >= max)
        {
            num -= max;
        }
        while (num < 0)
        {
            num += max;
        }
        return BoolSign(isInc) * num + min;
    }
    public static float PingPong(float num, float min, float max)
    {
        if (max == min)
        {
            return min;
        }
        int i = 0;
        bool isInc = max > min;
        num = BoolSign(isInc) * (num - min);
        max = BoolSign(isInc) * (max - min);
        for (; num >= max; i++)
        {
            num -= max;
        }
        for (; num < 0; i++)
        {
            num += max;
        }
        return BoolSign(isInc) * (i % 2 != 0 ? max - num : num) + min;
    }
    public static float AngleLerp(float a, float b, float t)
    {
        return Mathf.Lerp(a = LoopNum(a, -180, 180), LoopNum(b, a - 180, a + 180), t);
    }
    public static float AngleDirLerp(float a, float b, float t, bool isClockWise)
    {
        return Mathf.Lerp(a = LoopNum(a, -180, 180), LoopNum(b, a, a + BoolSign(isClockWise) * 360), t);
    }
    public static float AngleXZ2Pos(Vector3 a, Vector3 b)
    {
        return Angle2UnityReal(Mathf.Atan2(b.z - a.z, b.x - a.x) * Mathf.Rad2Deg, true);
    }
    public static float Angle2UnityReal(float angle, bool isUnity)
    {
        return LoopNum(isUnity ? (540 - LoopNum(angle + 90, 0, 360)) : (360 - LoopNum(angle - 90, 0, 360)), 0, 360);
    }
    public static float NumDis(float fromNum, float toNum, float min, float max)
    {
        fromNum = LoopNum(fromNum, min, max);
        toNum = LoopNum(toNum, min, max);
        bool isInc = toNum > fromNum;
        float dis1 = Mathf.Abs(toNum - fromNum),
            dis2 = isInc ? (fromNum - min + max - toNum) : (toNum - min + max - fromNum);
        return (dis1 < dis2) ? (toNum - fromNum) : isInc ? (toNum + min - max - fromNum) : (toNum + max - min - fromNum);
    }
    public static Vector3[] MakeSmoothCurve(Vector3[] arrayToCurve, float smoothness)
    {
        List<Vector3> points;
        List<Vector3> curvedPoints;
        int pointsLength = 0, curvedLength = 0;
        if (smoothness < 1.0f)
        {
            smoothness = 1.0f;
        }
        pointsLength = arrayToCurve.Length;
        curvedLength = (pointsLength * Mathf.RoundToInt(smoothness)) - 1;
        curvedPoints = new List<Vector3>(curvedLength);
        float t = 0.0f;
        for (int pointInTimeOnCurve = 0; pointInTimeOnCurve < curvedLength + 1; pointInTimeOnCurve++)
        {
            t = Mathf.InverseLerp(0, curvedLength, pointInTimeOnCurve);
            points = new List<Vector3>(arrayToCurve);
            for (int j = pointsLength - 1; j > 0; j--)
            {
                for (int i = 0; i < j; i++)
                {
                    points[i] = (1 - t) * points[i] + t * points[i + 1];
                }
            }
            curvedPoints.Add(points[0]);
        }
        return (curvedPoints.ToArray());
    }
    public static Color String2Color(string strCol)
    {
        if (Regex.IsMatch(strCol.ToUpper(), "#?(([0-9A-F]{6})|([0-9A-F]{8}))"))
        {
            if (strCol[0] == '#')
            {
                strCol = strCol.Substring(1);
            }
            if (strCol.Length == 6)
            {
                byte r = byte.Parse("" + strCol[0] + strCol[1], System.Globalization.NumberStyles.HexNumber),
                    g = byte.Parse("" + strCol[2] + strCol[3], System.Globalization.NumberStyles.HexNumber),
                    b = byte.Parse("" + strCol[4] + strCol[5], System.Globalization.NumberStyles.HexNumber);
                return new Color32(r, g, b, 255);
            }
            else if (strCol.Length == 8)
            {
                byte r = byte.Parse("" + strCol[0] + strCol[1], System.Globalization.NumberStyles.HexNumber),
                    g = byte.Parse("" + strCol[2] + strCol[3], System.Globalization.NumberStyles.HexNumber),
                    b = byte.Parse("" + strCol[4] + strCol[5], System.Globalization.NumberStyles.HexNumber),
                    a = byte.Parse("" + strCol[4] + strCol[5], System.Globalization.NumberStyles.HexNumber);
                return new Color32(r, g, b, a);
            }
        }
        return Color.black;
    }
    public static void ChangeRenderMode(Material standardShaderMaterial, BlendMode blendMode)
    {
        switch (blendMode)
        {
            case BlendMode.Opaque:
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                standardShaderMaterial.SetInt("_ZWrite", 1);
                standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.renderQueue = -1;
                break;
            case BlendMode.Cutout:
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                standardShaderMaterial.SetInt("_ZWrite", 1);
                standardShaderMaterial.EnableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.renderQueue = 2450;
                break;
            case BlendMode.Fade:
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                standardShaderMaterial.SetInt("_ZWrite", 0);
                standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.EnableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.renderQueue = 3000;
                break;
            case BlendMode.Transparent:
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                standardShaderMaterial.SetInt("_ZWrite", 0);
                standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.renderQueue = 3000;
                break;
        }
    }
}