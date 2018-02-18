using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class BugTest : MonoBehaviour
{
    public MeshRenderer OutputMesh;

    private ScreenQuadRenderer screenRenderer = new ScreenQuadRenderer();
    private RenderTexture startTex, endTex;


    public void Start()
    {
        startTex = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        startTex.Create();

        endTex = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        endTex.Create();
    }
    public void Update()
    {
        //If the user presses enter, do the rendering job.

        if (!Input.GetKey(KeyCode.Return))
            return;

        //Clear the final result render target.
        RenderTexture.active = endTex;
        GL.Clear(true, true, new Color(1.0f, 0.0f, 1.0f, 0.05f));

        //Also clear the temp render target.
        RenderTexture.active = startTex;
        GL.Clear(true, true, new Color(0.0f, 0.0f, 0.0f, 0.0f));

        //Use alpha blending, don't use culling/depth testing.
        screenRenderer.TurnOffTests();
        screenRenderer.UseAlphaBlending();

        //Draw some rectangles into the temp render target.
        for (int i = 0; i < 5; ++i)
        {
            RenderTexture.active = startTex;

            //Draw a rectangle on the render target.
            screenRenderer.Color = new Color(UnityEngine.Random.value,
                                             UnityEngine.Random.value,
                                             UnityEngine.Random.value,
                                             0.8f);
            screenRenderer.Tex = null;
            screenRenderer.Draw(new Vector2(Mathf.Lerp(-1.0f, 1.0f, UnityEngine.Random.value),
                                            Mathf.Lerp(-1.0f, 1.0f, UnityEngine.Random.value)),
                                UnityEngine.Random.value * 360.0f,
                                new Vector2(UnityEngine.Random.value,
                                            UnityEngine.Random.value));

            //Draw the current state of the temp render target into the final one,
            //    with a small random offset.
            RenderTexture.active = endTex;
            screenRenderer.Color = new Color(1.0f, 1.0f, 1.0f, 0.8f);
            screenRenderer.Tex = startTex;
            screenRenderer.Draw(new Vector2(Mathf.Lerp(-0.075f, 0.075f, UnityEngine.Random.value),
                                            Mathf.Lerp(-0.075f, 0.075f, UnityEngine.Random.value)),
                                0.0f, 0.5f);
        }
        RenderTexture.active = null;

        //Display the final texture.
        OutputMesh.material.mainTexture = endTex;
        return;
    }
}