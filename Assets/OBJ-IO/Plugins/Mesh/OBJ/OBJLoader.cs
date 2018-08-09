
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using UnityExtension;
using System.Threading.Tasks;

/*
 * Currently only supports Triangluar Meshes
 */

public class OBJLoader
{
    //------------------------------------------------------------------------------------------------------------
    private static OBJData m_OBJData = null;

    //------------------------------------------------------------------------------------------------------------
    private static OBJMaterial m_CurrentMaterial = null;
    private static OBJGroup m_CurrentGroup = null;

    #region PROCESSORS

    //------------------------------------------------------------------------------------------------------------
    private static readonly Dictionary<string, Action<string>> m_ParseOBJActionDictionary = new Dictionary<string, Action<string>>
    {
        { "mtllib", (lEntry) => { /*Load MTL*/ } }, 
        { "usemtl",  (lEntry) => { PushOBJGroupIfNeeded(); m_CurrentGroup.m_Material = m_OBJData.m_Materials.SingleOrDefault((lX) => { return lX.m_Name.EqualsInvariantCultureIgnoreCase(lEntry); }); } },
        { "v", (lEntry) => { m_OBJData.m_Vertices.Add(Utils.ParseVector3String(lEntry)); } },
        { "vn", (lEntry) => { m_OBJData.m_Normals.Add(Utils.ParseVector3String(lEntry)); } },
        { "vt", (lEntry) => { m_OBJData.m_UVs.Add(Utils.ParseVector2String(lEntry)); } },
        { "vt2", (lEntry) => { m_OBJData.m_UV2s.Add(Utils.ParseVector2String(lEntry)); } },
        { "vc", (lEntry) => { m_OBJData.m_Colors.Add(Utils.ParseVector4String(lEntry).ToColor()); } },
		{ "f", PushOBJFace },
        { "g", PushOBJGroup },
    };

    #endregion

    #region PUBLIC_INTERFACE

    //------------------------------------------------------------------------------------------------------------
    public static OBJData LoadOBJ(Stream lStream)
    {
        m_OBJData = new OBJData();

        StreamReader lLineStreamReader = new StreamReader(lStream);

        Action<string> lAction = null;
        string lCurrentLine = null;
        string[] lFields = null;
        string lKeyword = null;
        string lData = null;

        while (!lLineStreamReader.EndOfStream)
        {
            lCurrentLine = lLineStreamReader.ReadLine();

            if (StringExt.IsNullOrWhiteSpace(lCurrentLine) || lCurrentLine[0] == '#')
            {
                continue;
            }

            lFields = lCurrentLine.Trim().Split(null, 2);
            if (lFields.Length < 2)
            {
                continue;
            }

            lKeyword = lFields[0].Trim();
            lData = lFields[1].Trim();

            lAction = null;
            m_ParseOBJActionDictionary.TryGetValue(lKeyword.ToLowerInvariant(), out lAction);

            if (lAction != null)
            {
                lAction(lData);
            }
        }

        var lOBJData = m_OBJData;

        m_OBJData = null;
        m_CurrentMaterial = null;
        m_CurrentGroup = null;

        return lOBJData;
    }

    //------------------------------------------------------------------------------------------------------------
    public static void ExportOBJ(OBJData lData, Stream lStream)
    {
        StreamWriter lLineStreamWriter = new StreamWriter(lStream);

        lLineStreamWriter.WriteLine(string.Format("# File exported by Unity3D version {0}", Application.unityVersion));

        for (int lCount = 0; lCount < lData.m_Vertices.Count; ++lCount)
        {
            lLineStreamWriter.WriteLine(string.Format("v {0} {1} {2}",
                lData.m_Vertices[lCount].x.ToString("n8"),
                lData.m_Vertices[lCount].y.ToString("n8"),
                lData.m_Vertices[lCount].z.ToString("n8")));
        }

        for (int lCount = 0; lCount < lData.m_UVs.Count; ++lCount)
        {
            lLineStreamWriter.WriteLine(string.Format("vt {0} {1}",
                lData.m_UVs[lCount].x.ToString("n5"),
                lData.m_UVs[lCount].y.ToString("n5")));
        }

        for (int lCount = 0; lCount < lData.m_UV2s.Count; ++lCount)
        {
            lLineStreamWriter.WriteLine(string.Format("vt2 {0} {1}",
                lData.m_UVs[lCount].x.ToString("n5"),
                lData.m_UVs[lCount].y.ToString("n5")));
        }

        for (int lCount = 0; lCount < lData.m_Normals.Count; ++lCount)
        {
            lLineStreamWriter.WriteLine(string.Format("vn {0} {1} {2}",
                lData.m_Normals[lCount].x.ToString("n8"),
                lData.m_Normals[lCount].y.ToString("n8"),
                lData.m_Normals[lCount].z.ToString("n8")));
        }

        for (int lCount = 0; lCount < lData.m_Colors.Count; ++lCount)
        {
            lLineStreamWriter.WriteLine(string.Format("vc {0} {1} {2} {3}",
                lData.m_Colors[lCount].r.ToString("n8"),
                lData.m_Colors[lCount].g.ToString("n8"),
                lData.m_Colors[lCount].b.ToString("n8"),
                lData.m_Colors[lCount].a.ToString("n8")));
        }

        for (int lGroup = 0; lGroup < lData.m_Groups.Count; ++lGroup)
        {
            lLineStreamWriter.WriteLine(string.Format("g {0}", lData.m_Groups[lGroup].m_Name));

            for (int lFace = 0; lFace < lData.m_Groups[lGroup].Faces.Count; ++lFace)
            {
                lLineStreamWriter.WriteLine(string.Format("f {0} {1} {2}",
                    lData.m_Groups[lGroup].Faces[lFace].ToString(0),
                    lData.m_Groups[lGroup].Faces[lFace].ToString(1),
                    lData.m_Groups[lGroup].Faces[lFace].ToString(2)));
            }
        }

        lLineStreamWriter.Flush();
    }

    #endregion

    //------------------------------------------------------------------------------------------------------------
    private static void PushOBJMaterial(string lMaterialName)
    {
        m_CurrentMaterial = new OBJMaterial(lMaterialName);
        m_OBJData.m_Materials.Add(m_CurrentMaterial);
    }

    //------------------------------------------------------------------------------------------------------------
    private static void PushOBJGroup(string lGroupName)
    {
        m_CurrentGroup = new OBJGroup(lGroupName);
        m_OBJData.m_Groups.Add(m_CurrentGroup);
    }

    //------------------------------------------------------------------------------------------------------------
    private static void PushOBJGroupIfNeeded()
    {
        if (m_CurrentGroup == null)
        {
            PushOBJGroup("default");
        }
    }
    
    //------------------------------------------------------------------------------------------------------------
    private static void PushOBJFace(string lFaceLine)
    {
		PushOBJGroupIfNeeded();

        var vertices = lFaceLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        var face = new OBJFace();

        foreach (var vertexString in vertices)
        {
            face.ParseVertex(vertexString);
        }

        m_CurrentGroup.AddFace(face);
    }
}