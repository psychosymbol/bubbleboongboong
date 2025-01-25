using UnityEditor.SpeedTree.Importer;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(EdgeCollider2D))]
[RequireComponent(typeof(WaterTriggerHandler))]
public class InteractableWater : MonoBehaviour
{
    [Header("Springs")]
    [SerializeField] private float _spriteConstant = 1.4f;
    [SerializeField] private float _damping = 1.1f;
    [SerializeField] private float _spread = 6.5f;
    [SerializeField, Range(1, 10)] private int _wavePropogationIterations = 8;
    [SerializeField, Range(0f,20f)] private float _speedMult = 5.5f;

    [Header("Force")]
    public float ForceMultiplier = .2f;
    [Range(1f, 50f)] public float MaxForce = 5f;

    [Header("Collision")]
    [SerializeField, Range(1f, 10f)] private float _playerCollisionRadiusMult = 4.15f;

    [Header("Mesh Generation")]
    [Range(2, 500)] public int numOfXVertices = 70;
    public float width = 10f;
    public float height = 4f;
    public Material waterMaterial;
    private const int NUM_OF_Y_VERTICES = 2;

    [Header("Gizmo")]
    public Color gizmoColor = Color.white;

    private Mesh _mesh;
    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    private Vector3[] _vertices;
    private int[] _topVerticesIndex;
    private EdgeCollider2D _edgeCollider;

    private class WaterPoint
    {
        public float velocity, pos, targetHeight;
    }

    private List<WaterPoint> _waterPoints = new List<WaterPoint>();

    private void Start()
    {
        _edgeCollider = GetComponent<EdgeCollider2D>();
        GenerateMesh();
        CreateWaterPoints();
    }

    private void Reset()
    {
        _edgeCollider = GetComponent<EdgeCollider2D>();
        _edgeCollider.isTrigger = true;
    }

    private void FixedUpdate()
    {
        //update all spring positions
        for (int i = 1; i < _waterPoints.Count-1; i++)
        {
            WaterPoint point = _waterPoints[i];
            float x = point.pos - point.targetHeight;
            float acceleration = -_spriteConstant * x - _damping * point.velocity;
            point.pos += point.velocity * _speedMult * Time.fixedDeltaTime;
            _vertices[_topVerticesIndex[i]].y = point.pos;
            point.velocity += acceleration * _speedMult * Time.fixedDeltaTime;
        }

        //wave propogation
        for (int j = 0; j < _wavePropogationIterations; j++)
        {
            for (int i = 1; i < _waterPoints.Count-1; i++)
            {
                float leftDelta = _spread * (_waterPoints[i].pos - _waterPoints[i - 1].pos) * _speedMult * Time.fixedDeltaTime;
                _waterPoints[i - 1].velocity += leftDelta;

                float rightDelta = _spread * (_waterPoints[i].pos - _waterPoints[i + 1].pos) * _speedMult * Time.fixedDeltaTime;
                _waterPoints[i + 1].velocity += rightDelta;
            }
        }

        //update the mesh
        _mesh.vertices = _vertices;
    }
    
    public void Splash(Collider2D collision, float force)
    {
        float radius = collision.bounds.extents.x * _playerCollisionRadiusMult;
        Vector2 center = collision.transform.position;

        for (int i = 0; i < _waterPoints.Count; i++)
        {
            Vector2 vertexWorldPos = transform.TransformPoint(_vertices[_topVerticesIndex[i]]);

            if(IsPointInsideCircle(vertexWorldPos,center,radius))
            {
                _waterPoints[i].velocity = force;
            }
        }
    }

    private bool IsPointInsideCircle(Vector2 point, Vector2 center, float radius)
    {
        float distanceSquare = (point - center).sqrMagnitude;
        return distanceSquare <= radius * radius;
    }

    public void ResetEdgeCollider()
    {
        _edgeCollider = GetComponent<EdgeCollider2D>();

        Vector2[] newPoints = new Vector2[2];

        Vector2 firstPoint = new Vector2(_vertices[_topVerticesIndex[0]].x, _vertices[_topVerticesIndex[0]].y);
        newPoints[0] = firstPoint;

        Vector2 secondPoint = new Vector2(_vertices[_topVerticesIndex.Length - 1].x, _vertices[_topVerticesIndex.Length - 1].y);
        newPoints[1] = secondPoint;

        _edgeCollider.offset = Vector2.zero;
        _edgeCollider.points = newPoints;
    }

    public void GenerateMesh()
    {
        _mesh = new Mesh();

        //add vertices
        _vertices = new Vector3[numOfXVertices * NUM_OF_Y_VERTICES];
        _topVerticesIndex = new int[numOfXVertices];
        for (int y = 0; y < NUM_OF_Y_VERTICES; y++)
        {
            for (int x = 0; x < numOfXVertices; x++)
            {
                float xPos = (x / (float)(numOfXVertices - 1)) * width - width / 2;
                float yPos = (y / (float)(NUM_OF_Y_VERTICES - 1) * height - height / 2);
                _vertices[y * numOfXVertices + x] = new Vector3(xPos, yPos, 0);

                if(y == NUM_OF_Y_VERTICES-1)
                {
                    _topVerticesIndex[x] = y * numOfXVertices + x;
                }
            }
        }

        //construct triangles
        int[] triangles = new int[(numOfXVertices - 1) * (NUM_OF_Y_VERTICES - 1) * 6];
        int index = 0;
        for (int y = 0; y < NUM_OF_Y_VERTICES-1; y++)
        {
            for (int x = 0; x < numOfXVertices - 1; x++)
            {
                int bottomLeft = y * numOfXVertices + x;
                int bottomRight = bottomLeft + 1;
                int topLeft = bottomLeft + numOfXVertices;
                int topRight = topLeft + 1;

                //first triangle
                triangles[index++] = bottomLeft;
                triangles[index++] = topLeft;
                triangles[index++] = bottomRight;

                //second triangle
                triangles[index++] = bottomRight;
                triangles[index++] = topLeft;
                triangles[index++] = topRight;
            }
        }

        //UVs
        Vector2[] uvs = new Vector2[_vertices.Length];
        for (int i = 0; i < _vertices.Length; i++)
        {
            uvs[i] = new Vector2((_vertices[i].x + width / 2) / width, (_vertices[i].y + height / 2) / height);
        }

        if(_meshRenderer == null)
            _meshRenderer = GetComponent<MeshRenderer>();

        if(_meshFilter == null)
            _meshFilter = GetComponent<MeshFilter>();

        _meshRenderer.material = waterMaterial;

        _mesh.vertices = _vertices;
        _mesh.triangles = triangles;
        _mesh.uv = uvs;

        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        _meshFilter.mesh = _mesh;
    }

    private void CreateWaterPoints()
    {
        _waterPoints.Clear();
        for (int i = 0; i < _topVerticesIndex.Length; i++)
        {
            _waterPoints.Add(new WaterPoint
            {
                pos = _vertices[_topVerticesIndex[i]].y,
                targetHeight = _vertices[_topVerticesIndex[i]].y,
            });
        }
    }
}
[CustomEditor(typeof(InteractableWater))]
public class InteractableWaterEditor : Editor
{
    private InteractableWater _water;

    private void OnEnable()
    {
        _water = (InteractableWater)target;
    }

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();
        InspectorElement.FillDefaultInspector(root, serializedObject, this);
        root.Add(new VisualElement { style = { height = 10 } });
        Button generateMeshButton = new Button(() => _water.GenerateMesh())
        {
            text = "Generate Mesh"
        };
        root.Add(generateMeshButton);
        Button placeEdgeColliderButton = new Button(() => _water.ResetEdgeCollider())
        {
            text = "Place Edge Collider"
        };
        root.Add(placeEdgeColliderButton);
        return root;
    }

    private void ChangeDimensions(ref float width, ref float height, float calculatedWidthMax, float calculatedHeightMax)
    {
        width = Mathf.Max(.1f, calculatedWidthMax);
        height = Mathf.Max(.1f, calculatedHeightMax);
    }

    private void OnSceneGUI()
    {
        //Draw the wireframe box
        Handles.color = _water.gizmoColor;
        Vector3 center = _water.transform.position;
        Vector3 size = new Vector3(_water.width, _water.height, .1f);
        Handles.DrawWireCube(center, size);

        //Handles for width and height
        float handleSize = HandleUtility.GetHandleSize(center) * .1f;
        Vector3 snap = Vector3.one * .1f;

        //Corner Handles
        Vector3[] corners = new Vector3[4];
        corners[0] = center + new Vector3(-_water.width / 2, -_water.height / 2, 0); //bottom left
        corners[1] = center + new Vector3(_water.width / 2, -_water.height / 2, 0); //bottom right
        corners[2] = center + new Vector3(-_water.width / 2, _water.height / 2, 0); //top left
        corners[3] = center + new Vector3(_water.width / 2, _water.height / 2, 0); //top right

        //Handle for each corner
        EditorGUI.BeginChangeCheck();
        Vector3 newBottomLeft = Handles.FreeMoveHandle(corners[0], handleSize, snap, Handles.CubeHandleCap);
        if(EditorGUI.EndChangeCheck())
        {
            ChangeDimensions(ref _water.width, ref _water.height, corners[1].x - newBottomLeft.x, corners[3].y - newBottomLeft.y);
            _water.transform.position += new Vector3((newBottomLeft.x - corners[0].x) / 2, (newBottomLeft.y - corners[0].y) / 2, 0);
        }

        EditorGUI.BeginChangeCheck();
        Vector3 newBottomRight = Handles.FreeMoveHandle(corners[1], handleSize, snap, Handles.CubeHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            ChangeDimensions(ref _water.width, ref _water.height, newBottomRight.x - corners[0].x, corners[3].y - newBottomRight.y);
            _water.transform.position += new Vector3((newBottomRight.x - corners[1].x) / 2, (newBottomRight.y - corners[1].y) / 2, 0);
        }

        EditorGUI.BeginChangeCheck();
        Vector3 newTopLeft = Handles.FreeMoveHandle(corners[2], handleSize, snap, Handles.CubeHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            ChangeDimensions(ref _water.width, ref _water.height, corners[3].x - newTopLeft.x, newTopLeft.y - corners[0].y);
            _water.transform.position += new Vector3((newTopLeft.x - corners[2].x) / 2, (newTopLeft.y - corners[2].y) / 2, 0);
        }

        EditorGUI.BeginChangeCheck();
        Vector3 newTopRight = Handles.FreeMoveHandle(corners[3], handleSize, snap, Handles.CubeHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            ChangeDimensions(ref _water.width, ref _water.height, newTopRight.x - corners[2].x, newTopRight.y - corners[1].y);
            _water.transform.position += new Vector3((newTopRight.x - corners[3].x) / 2, (newTopRight.y - corners[3].y) / 2, 0);
        }
        //Update mesh if the handles are moved
        if(GUI.changed)
        {
            _water.GenerateMesh();
        }
    }
}
