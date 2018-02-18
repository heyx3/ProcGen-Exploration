using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class WaterColorTest : MonoBehaviour
{
    public Color BackgroundCol = new Color();
    public Color Col = Color.red;

    public int NBlotches = 30;

    public Renderer ResultRenderer;
    public RenderTexture RendTarget;

    public WaterColorBrush Brush = new WaterColorBrush();

    private ScreenQuadRenderer screenRenderer = new ScreenQuadRenderer();

    
    private void Start()
    {
        Brush.GenerateBaseBlotch();
        Brush.GenerateInstanceBlotch();
        RenderToTex();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            RenderToTex();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Brush.GenerateBaseBlotch();
            RenderToTex();
        }
    }


    public void RenderToTex()
    {
        var oldActive = RenderTexture.active;

        //Clear the texture.
        RenderTexture.active = RendTarget;
        GL.Clear(true, true, BackgroundCol);

        //Set up the screen-space renderer.
        screenRenderer.TurnOffTests();
        screenRenderer.UseAlphaBlending();
        screenRenderer.UseTintTex(Brush.InstanceShapeRender, Col);

        //Render the watercolor images.
        for (int i = 0; i < NBlotches; ++i)
        {
            Brush.GenerateInstanceBlotch();
            screenRenderer.Draw(Vector2.zero, 0.0f, 1.0f);
        }

        RenderTexture.active = oldActive;

        ResultRenderer.material.mainTexture = RendTarget;
    }
}