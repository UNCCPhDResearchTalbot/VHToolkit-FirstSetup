using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using System.Linq;

/// <summary>
/// The purpose of this class is to add menu items to the unity menu toolbar.
/// Clicking these menu items will perform some functionality describe in this file.
/// Non-EditorWindow MenuItems should go in this class to keep them in a common place
/// EditorWindow MenuItems go in their own specific classes (like SBMWindow.cs)
/// </summary>
public class VHMenuItems : MonoBehaviour
{
    [MenuItem("VH/Scene Reporting/Check For Duplicate Materials")]
    static void CheckForDuplicateMaterials()
    {
        //Object[] allProjectMaterials = Component.FindObjectsOfTypeIncludingAssets(typeof(Material));
        List<string> allProjectMaterials = Utils.GetFilesRecursive(Application.dataPath, "*.mat");
        List<string> duplicateMaterials = new List<string>();

        if (allProjectMaterials == null || allProjectMaterials.Count <= 0)
        {
            Debug.Log("Couldn't find any materials in the project");
            return;
        }

        Debug.Log("Num materials in project: " + allProjectMaterials.Count);

        // format the paths for unity to load assets properly
        for (int i = 0; i < allProjectMaterials.Count; i++)
        {
            // All asset names & paths in Unity use forward slashes, paths using backslashes will not work.
            allProjectMaterials[i] = allProjectMaterials[i].Replace('\\', '/');

            // remove everything before Unity's "Assets" folder, otherwise the load fails
            allProjectMaterials[i] = Utils.RemovePathUpTo("Assets/", allProjectMaterials[i]);
        }

        // O(n^2)
        Material matI, matJ;
        StringBuilder dupMaterialString = new StringBuilder();
        for (int i = 0; i < allProjectMaterials.Count; i++)
        {
            //Debug.Log("allProjectMaterials[i]: " + allProjectMaterials[i]);
            matI = (Material)AssetDatabase.LoadAssetAtPath(allProjectMaterials[i], typeof(Material));
            for (int j = i + 1; j < allProjectMaterials.Count; j++)
            {
                matJ = (Material)AssetDatabase.LoadAssetAtPath(allProjectMaterials[j], typeof(Material));

                if (matI.shader.name == matJ.shader.name && matI.mainTexture == matJ.mainTexture
                    && matI.color == matJ.color)
                {
                    // this appears to be a duplicate
                    dupMaterialString.Append(", " + matJ.name);
                    allProjectMaterials.RemoveAt(j--);
                }
            }

            if (dupMaterialString.Length > 0)
            {
                // duplicate material names have been stored
                duplicateMaterials.Add("Potential duplicate materials using " + matI.shader.name + ": " + matI.name + dupMaterialString);

                // clear string
                dupMaterialString = dupMaterialString.Remove(0, dupMaterialString.Length);
            }
        }

        // show the duplicates
        for (int i = 0; i < duplicateMaterials.Count; i++)
        {
            Debug.LogWarning(duplicateMaterials[i]);
        }

        Resources.UnloadUnusedAssets();
    }

    static Quaternion CreateQuat(float x, float y, float z, float degrees)
    {
        Quaternion q = new Quaternion();
        float radians = Mathf.PI * degrees / 180.0f;
        //float radians = degrees * DEG_TO_RAD;

        // normalize axis:
        q.x = x;
        q.y = y;
        q.z = z;

        float f = x * x + y * y + z * z;

        if (f > 0)
        {
            f = Mathf.Sqrt(f);
            q.x /= f;
            q.y /= f;
            q.z /= f;
        }

        // set the quaternion:
        radians /= 2;
        f = Mathf.Sin(radians);
        q.x *= f;
        q.y *= f;
        q.z *= f;
        q.w = Mathf.Cos(radians);
        return q;
    }

    static void QuatToAxisAngle(Quaternion q, out Vector4 aa, bool normalizeAxis)
    {
        aa = new Vector4();
        float ang = 2.0f * Mathf.Acos(q.w);
        //q.y *= -1;
        //q.z *= -1;
        float norm = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z);
        if (norm == 0 || ang == 0)
        {
            aa[0] = 0;
            aa[1] = 0;
            aa[2] = 0;
        }
        else
        {
            aa[0] = (q[0] / norm);
            aa[1] = (q[1] / norm);
            aa[2] = (q[2] / norm);

            if (!normalizeAxis)
            {
                aa[0] *= ang;
                aa[1] *= ang;
                aa[2] *= ang;
            }
        }

        aa[3] = ang * 180.0f / Mathf.PI;
    }

    [MenuItem("VH/Load Clips")]
    static void SetupClips()
    {
        AudioBank bank = (AudioBank)GameObject.FindObjectOfType(typeof(AudioBank));
        if (bank == null)
        {
            return;
        }

        bank.m_Clips.Clear();

        string[] audioFiles = Directory.GetFiles(string.Format("{0}/StreamingAssets/Sounds", Application.dataPath), "*.wav");
        foreach (string filename in audioFiles)
        {
            string s = filename.Replace(Application.dataPath + "/", "Assets/");
            s = s.Replace("\\", "/");
            AudioClip clip = (AudioClip)AssetDatabase.LoadAssetAtPath(s, typeof(AudioClip));
            if (clip != null)
            {
                bank.m_Clips.Add(clip);
            }
        }

        EditorUtility.SetDirty(bank);
    }


    [MenuItem("VH/print")]
    static void Print()
    {
        //Quaternion quat = Quaternion.Euler(-8.1600246f, -2.2186675f, 7.1426268f);
        //Quaternion quat = Quaternion.Euler(-21.326f, 35.073f, 14.833f);
        Quaternion quat = new Quaternion();
        Vector4 axis;
        /*QuatToAxisAngle(quat, out axis, false);*/

        /*Vector4 axis;
        QuatToAxisAngle(quat, out axis, false);*/

        Quaternion qx, qy, qz;
        qx = CreateQuat(1, 0, 0, quat.eulerAngles.x);
        qy = CreateQuat(0, 1, 0, quat.eulerAngles.y);
        qz = CreateQuat(0, 0, 1, quat.eulerAngles.z);

        Quaternion qzyx;
        Quaternion qyx;
        qyx = qy * qx;
        qzyx = qz * qyx;
        QuatToAxisAngle(qzyx, out axis, false);

        Debug.Log(string.Format("qx: {0} {1} {2} {3}", qx.x.ToString("f6"), qx.y.ToString("f6"), qx.z.ToString("f6"), qx.w.ToString("f6")));
        Debug.Log(string.Format("qy: {0} {1} {2} {3}", qy.x.ToString("f6"), qy.y.ToString("f6"), qy.z.ToString("f6"), qy.w.ToString("f6")));
        Debug.Log(string.Format("qz: {0} {1} {2} {3}", qz.x.ToString("f6"), qz.y.ToString("f6"), qz.z.ToString("f6"), qz.w.ToString("f6")));
        Debug.Log(string.Format("qyx: {0} {1} {2} {3}", qyx.x.ToString("f6"), qyx.y.ToString("f6"), qyx.z.ToString("f6"), qyx.w.ToString("f6")));
        Debug.Log(string.Format("qzyx: {0} {1} {2} {3}", qzyx.x.ToString("f6"), qzyx.y.ToString("f6"), qzyx.z.ToString("f6"), qzyx.w.ToString("f6")));

        Debug.Log(string.Format("{0} {1} {2}", axis.x.ToString("f6"), axis.y.ToString("f6"), axis.z.ToString("f6")));
    }

    [MenuItem("VH/print2")]
    static void Print2()
    {
        Vector4 axis;
        Quaternion quat = new Quaternion(0.002321f, -0.002234f, -0.002321f, 0.002234f);
        QuatToAxisAngle(quat, out axis, false);
        Debug.Log(string.Format("{0} {1} {2}", axis.x.ToString("f6"), axis.y.ToString("f6"), axis.z.ToString("f6")));
    }
}
