using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLTest : MonoBehaviour
{
    [SerializeField] Material mat;

    void OnPostRender() {

        if (!mat) {
            Debug.LogError("Assign material on inspector");
        }

        GL.PushMatrix();
        mat.SetPass(0);
        GL.LoadOrtho();
        GL.Begin(GL.TRIANGLE_STRIP);
        GL.Color(new Color(0, 0, 0, 1));
        GL.Vertex3(0.25f, 0.5f, 0);
        GL.Vertex3(0, 0.5f, 0);
        GL.Vertex3(0.25f, 0.25f, 0);
        GL.Vertex3(0, 0.25f, 0);
        GL.End();
        GL.PopMatrix();
    }
}
