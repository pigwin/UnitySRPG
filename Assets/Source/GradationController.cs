using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

/*************************************************************/
/*GradationController gradationcontroller グラデーション     */
/*  グラデーションをコントロールする関数                     */
/*************************************************************/

public class GradationController : BaseMeshEffect
{
    public Color colorTop = Color.white;
    public Color colorBottom = Color.white;

    [SerializeField]
    private bool isTrans = false;
    [SerializeField]
    [Header("Text Only")]
    private bool followalpha = false;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;

        List<UIVertex> vertices = new List<UIVertex>();

        vh.GetUIVertexStream(vertices);

        if (followalpha)
        {
            colorTop.a = this.GetComponent<Text>().color.a;
            colorBottom.a = this.GetComponent<Text>().color.a;
        }
        Gradation(vertices);

        vh.Clear();
        vh.AddUIVertexTriangleStream(vertices);
    }

    private void Gradation(List<UIVertex> vertices)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            UIVertex newVertex = vertices[i];

            if(!isTrans)
                newVertex.color = (i % 6 == 0 || i % 6 == 1 || i % 6 == 5) ? colorTop : colorBottom;
            else
                newVertex.color = (i % 6 == 1 || i % 6 == 2 || i % 6 == 3) ? colorTop : colorBottom;

            vertices[i] = newVertex;
        }
    }
}
