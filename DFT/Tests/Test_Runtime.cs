using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(MeshRenderer))]
public class Test_Runtime : MonoBehaviour
{
    public Material DFTMat;
    public Texture2D SourceTex;

    private Texture2D intermediateTex;
    private RenderTexture destTex;


    private void Start()
    {
        intermediateTex = new Texture2D(SourceTex.width, SourceTex.height,
                                        TextureFormat.RGBAFloat, false, true);
        intermediateTex.filterMode = FilterMode.Point;
        intermediateTex.wrapMode = TextureWrapMode.Clamp;

        destTex = new RenderTexture(SourceTex.width, SourceTex.height, 16,
                                    RenderTextureFormat.ARGBFloat,
                                    RenderTextureReadWrite.Default);
        destTex.filterMode = FilterMode.Point;
        destTex.wrapMode = TextureWrapMode.Clamp;

        GetComponent<MeshRenderer>().material.mainTexture = destTex;
    }
    private void Update()
    {
        DFTMat.EnableKeyword("DFT_FORWARD");
        DFTMat.DisableKeyword("DFT_INVERSE");

        DFTMat.EnableKeyword("DFT_HORZ");
        DFTMat.DisableKeyword("DFT_VERT");

        DFTMat.SetInt("u_SamplesSizeX", SourceTex.width);
        DFTMat.SetInt("u_SamplesSizeY", SourceTex.height);
        DFTMat.SetVector("_MainTex_TexelSize", SourceTex.texelSize);
        DFTMat.SetTexture("_MainTex", SourceTex);

        Graphics.Blit(SourceTex, destTex, DFTMat);
        DFT_GPU.CopyTo(destTex, intermediateTex);

        //DFTMat.EnableKeyword("DFT_VERT");
        //DFTMat.DisableKeyword("DFT_HORZ");
        //DFTMat.SetTexture("_MainTex", destTex);

        //Graphics.Blit(intermediateTex, destTex, DFTMat);
    }
}