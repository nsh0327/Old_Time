using UnityEngine;

[RequireComponent(typeof(TimeField))]
public class TimeFieldRenderer : MonoBehaviour
{
    [Header("Field Visual")]
    [SerializeField] private Color _fieldColor = new Color(1f, 0.9f, 0.3f, 0.15f);
    [SerializeField] private Color _fieldEdgeColor = new Color(1f, 0.9f, 0.3f, 0.8f);
    [SerializeField] private int _circleSegments = 64;
    [SerializeField] private float _edgeWidth = 0.05f;
    [SerializeField] private float _centerOffsetY = -0.4f;    

    private TimeField _timeField;
    private GameObject _fillObject;
    private GameObject _edgeObject;
    private Mesh _fillMesh;
    private LineRenderer _edgeRenderer;

   
    private void Awake()
    {
        _timeField = GetComponent<TimeField>();
        CreateFillCircle();
        CreateEdgeCircle();
    }

    private void LateUpdate()
    {
        UpdateFillCircle();
        UpdateEdgeCircle();
    }

    private void CreateFillCircle()
    {
        _fillObject = new GameObject("TimeFieldFill");
        _fillObject.transform.SetParent(transform);
        _fillObject.transform.localPosition = new Vector3(0f, _centerOffsetY, 0.1f);

        var mf = _fillObject.AddComponent<MeshFilter>();
        var mr = _fillObject.AddComponent<MeshRenderer>();

        mr.sortingOrder = -1;

        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = _fieldColor;
        mr.material = mat;

        _fillMesh = new Mesh();
        mf.mesh = _fillMesh;

        BuildFillMesh(_timeField.FieldRadius);
    }

    private void UpdateFillCircle()
    {
        BuildFillMesh(_timeField.FieldRadius);
    }

    private void BuildFillMesh(float radius)
    {
        var vertices = new Vector3[_circleSegments + 1];
        var triangles = new int[_circleSegments * 3];

        vertices[0] = Vector3.zero;

        for (int i = 0; i < _circleSegments; i++)
        {
            float angle = 2f * Mathf.PI * i / _circleSegments;
            vertices[i + 1] = new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );
        }

        for (int i = 0; i < _circleSegments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 1) % _circleSegments + 1;
        }

        _fillMesh.Clear();
        _fillMesh.vertices = vertices;
        _fillMesh.triangles = triangles;
        _fillMesh.RecalculateNormals();
    }


    private void CreateEdgeCircle()
    {
        _edgeObject = new GameObject("TimeFieldEdge");
        _edgeObject.transform.SetParent(transform);
        _edgeObject.transform.localPosition = new Vector3(0f,_centerOffsetY, 0.1f);

        _edgeRenderer = _edgeObject.AddComponent<LineRenderer>();
        _edgeRenderer.loop = true;
        _edgeRenderer.useWorldSpace = false;
        _edgeRenderer.startWidth = _edgeWidth;
        _edgeRenderer.endWidth = _edgeWidth;
        _edgeRenderer.sortingOrder = 0;

        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = _fieldEdgeColor;
        _edgeRenderer.material = mat;

        UpdateEdgeCircle();
    }

    private void UpdateEdgeCircle()
    {
        float radius = _timeField.FieldRadius;
        _edgeRenderer.positionCount = _circleSegments;

        for (int i = 0; i < _circleSegments; i++)
        {
            float angle = 2f * Mathf.PI * i / _circleSegments;
            _edgeRenderer.SetPosition(i, new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            ));
        }
    }
}