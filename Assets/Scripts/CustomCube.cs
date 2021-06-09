using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CustomCube : MonoBehaviour
{

	public int xSize, ySize, zSize;

	private Mesh mesh;
	private Vector3[] vertices;
	private Vector2[] uv;
	private Vector4[] tangents;
	private Vector4 tangent;

	public Texture2D originalTexture;

	private void Awake()
	{
		//CreateColliders();

		Generate();

        mesh.Optimize();

		//mesh.RecalculateUVDistributionMetrics();
		//mesh.RecalculateTangents();
		//mesh.RecalculateNormals();

		SetTexture();

	}

	private void Generate()
	{
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.Clear();
		mesh.name = "Custom Cube";
		
		

		CreateVertices();
		CreateTriangles();

		
	}

	private void CreateColliders()
    {
		Destroy(gameObject.GetComponent<MeshCollider>());
		gameObject.AddComponent<MeshCollider>();
	}

	private void SetTexture()
    {
		Texture2D copyTexture = new Texture2D(originalTexture.width, originalTexture.height);
		copyTexture.SetPixels(originalTexture.GetPixels());
		copyTexture.Apply();
		Renderer rend = gameObject.GetComponent<Renderer>();
		rend.material.mainTexture = copyTexture;
	}


	private void CreateVertices()
	{
		int cornerVertices = 8;
		int edgeVertices = (xSize + ySize + zSize - 3) * 4;
		int faceVertices = (
			(xSize - 1) * (ySize - 1) +
			(xSize - 1) * (zSize - 1) +
			(ySize - 1) * (zSize - 1)) * 2;
		vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
        Vector3[] normals = new Vector3[vertices.Length];
        uv = new Vector2[vertices.Length];
		tangents = new Vector4[vertices.Length];
		tangent = new Vector4(1f, 0f, 0f, -1f);

		int v = 0;
		for (int y = 0; y <= ySize; y++)
		{
            for (int x = 0; x <= xSize; x++)
            {
                //vertices[v] = new Vector3(x, y, 0);
                //tangents[v] = tangent;
                //uv[v++] = new Vector2((float)x / xSize, (float)y / ySize);
				SetVertex(new Vector3(x, y, 0), v++,  x, y, xSize, ySize);

			}
            for (int z = 1; z <= zSize; z++)
            {
                //vertices[v] = new Vector3(xSize, y, z);
                //tangents[v] = tangent;
                //uv[v++] = new Vector2((float)z / zSize, (float)y / ySize);
				SetVertex(new Vector3(xSize, y, z), v++,  z, y, zSize, ySize);
			}
            for (int x = xSize - 1; x >= 0; x--)
			{
				//vertices[v] = new Vector3(x, y, zSize);
				//tangents[v] = tangent;
				//uv[v++] = new Vector2((float)x / xSize, (float)y / ySize);
				SetVertex(new Vector3(x, y, zSize), v++,  x, y, xSize, ySize);
			}
			for (int z = zSize - 1; z > 0; z--)
			{
				//vertices[v] = new Vector3(0, y, z);
				//tangents[v] = tangent;
				//uv[v++] = new Vector2((float)z / zSize, (float)y / ySize);
				SetVertex(new Vector3(0, y, z), v++,  z, y, zSize, ySize);
			}
		}

		for (int z = 1; z < zSize; z++)
		{
			for (int x = 1; x < xSize; x++)
			{
				//vertices[v] = new Vector3(x, ySize, z);
				//tangents[v] = tangent;
				//uv[v++] = new Vector2((float)x / xSize, (float)z / zSize);
				SetVertex(new Vector3(x, ySize, z), v++,  x, z, xSize, zSize);
			}
		}
		for (int z = 1; z < zSize; z++)
		{
			for (int x = 1; x < xSize; x++)
			{
				//vertices[v] = new Vector3(x, 0, z);
				//tangents[v] = tangent;
				//uv[v++] = new Vector2((float)x / xSize, (float)z / zSize);
				SetVertex(new Vector3(x, 0, z), v++,  x, z, xSize, zSize);
			}
		}
		mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.tangents = tangents;
        mesh.normals = normals;

    }

	private void SetVertex(Vector3 coord, int v, int coordUvX, int coordUvY, float coordUvSizeX, float coordUvSizeY)
    {
		vertices[v] = coord;
		tangents[v] = tangent;
		uv[v] = new Vector2(coordUvY / coordUvSizeY, coordUvX / coordUvSizeX);
	}

	private void CreateTriangles()
	{
		int quads = (xSize * ySize + xSize * zSize + ySize * zSize) * 2;
		int[] triangles = new int[quads * 6];

		int ring = (xSize + zSize) * 2;
		int t = 0, v = 0;

		for (int y = 0; y < ySize; y++, v++)
		{
			for (int q = 0; q < ring - 1; q++, v++)
			{
				t = SetQuad(triangles, t, v, v + 1, v + ring, v + ring + 1);
			}
			t = SetQuad(triangles, t, v, v - ring + 1, v + ring, v + 1);
		}

		t = CreateTopFace(triangles, t, ring);
		t = CreateBottomFace(triangles, t, ring);
		mesh.triangles = triangles;
	}

	private int CreateBottomFace(int[] triangles, int t, int ring)
	{
		int v = 1;
		int vMid = vertices.Length - (xSize - 1) * (zSize - 1);
		t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
		for (int x = 1; x < xSize - 1; x++, v++, vMid++)
		{
			t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
		}
		t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

		int vMin = ring - 2;
		vMid -= xSize - 2;
		int vMax = v + 2;

		for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++)
		{
			t = SetQuad(triangles, t, vMin, vMid + xSize - 1, vMin + 1, vMid);
			for (int x = 1; x < xSize - 1; x++, vMid++)
			{
				t = SetQuad(
					triangles, t,
					vMid + xSize - 1, vMid + xSize, vMid, vMid + 1);
			}
			t = SetQuad(triangles, t, vMid + xSize - 1, vMax + 1, vMid, vMax);
		}

		int vTop = vMin - 1;
		t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
		for (int x = 1; x < xSize - 1; x++, vTop--, vMid++)
		{
			t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
		}
		t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

		return t;
	}

	private int CreateTopFace(int[] triangles, int t, int ring)
	{
		int v = ring * ySize;
		for (int x = 0; x < xSize - 1; x++, v++)
		{
			t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
		}
		t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);


		int vMin = ring * (ySize + 1) - 1;
		int vMid = vMin + 1;
		int vMax = v + 2;

		for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++)
		{
			t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + xSize - 1);
			for (int x = 1; x < xSize - 1; x++, vMid++)
			{
				t = SetQuad(
					triangles, t,
					vMid, vMid + 1, vMid + xSize - 1, vMid + xSize);
			}
			t = SetQuad(triangles, t, vMid, vMax, vMid + xSize - 1, vMax + 1);
		}


		int vTop = vMin - 2;
		t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
		for (int x = 1; x < xSize - 1; x++, vTop--, vMid++)
		{
			t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
		}
		t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);


		return t;
	}

	private static int SetQuad(int[] triangles, int i, int v00, int v10, int v01, int v11)
	{
		triangles[i] = v00;
		triangles[i + 1] = triangles[i + 4] = v01;
		triangles[i + 2] = triangles[i + 3] = v10;
		triangles[i + 5] = v11;
		return i + 6;
	}

	//private void OnDrawGizmos()
	//{
	//	if (vertices == null)
	//	{
	//		return;
	//	}
	//	for (int i = 0; i < vertices.Length; i++)
	//	{
	//		Gizmos.color = Color.black;
	//		Gizmos.DrawSphere(vertices[i], 0.1f);
	//		Gizmos.color = Color.yellow;
	//		Gizmos.DrawRay(vertices[i], uv[i]);
	//	}
	//}
}