using UnityEngine;
using PDollarGestureRecognizer;
using System.IO;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using FSMC.Runtime;
public class DrawHandler : MonoBehaviour
{
    private Vector2 currMousePosition;

    private List<Gesture> trainingSet = new List<Gesture>();
    public Vector2 CurrMousePosition{ 
        get
        {
            Vector2 returnValue = currMousePosition;
            currMousePosition = Vector2.positiveInfinity;
            return returnValue;
        } set
        {
            currMousePosition = value;
        } 
    }

    private List<LineRenderer> gestureLinesRenderer = new List<LineRenderer>();
    private LineRenderer currentGestureLineRenderer;
    private int vertexCount = 0;


    public Transform gestureOnScreenPrefab;
    public PlayerAttack playerAttack;

    public int StrokeId { get; set; }
    public List<Point> DrawedPoints { get; set; } = new List<Point>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("GestureSet/10-stylus-MEDIUM/");
        foreach (TextAsset gestureXml in gesturesXml)
            trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));

        //Load user custom gestures
        string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.xml");
        foreach (string filePath in filePaths)
            trainingSet.Add(GestureIO.ReadGestureFromFile(filePath));


    }

    public Result RecognizePointCloud(List<Point> pointClouds)
    {
        Gesture candidate = new Gesture(pointClouds.ToArray());

        Result gestureResult = PointCloudRecognizer.Classify(candidate, trainingSet.ToArray());

        return gestureResult;
    }

    public void CleanseData()
    {
        StrokeId = -1;
        DrawedPoints.Clear();


        foreach (LineRenderer lineRenderer in gestureLinesRenderer)
        {

            lineRenderer.positionCount = 0;
            Destroy(lineRenderer.gameObject);
        }

        gestureLinesRenderer.Clear();
    }

    public void PrepareForNewStroke()
    {
        ++StrokeId;

        Transform tmpGesture = Instantiate(gestureOnScreenPrefab, transform.position, transform.rotation);
        currentGestureLineRenderer = tmpGesture.GetComponent<LineRenderer>();

        gestureLinesRenderer.Add(currentGestureLineRenderer);

        vertexCount = 0;
    }
    public void AddPointToLine(Vector2 point)
    {
        Vector2 currentPos = point;

        DrawedPoints.Add(new Point(point.x,-point.y, StrokeId));
        currentGestureLineRenderer.positionCount = ++vertexCount;
        var screenCoords = Camera.main.ScreenToWorldPoint(new Vector3(currentPos.x, currentPos.y, 10f));
        currentGestureLineRenderer.SetPosition(vertexCount-1,screenCoords );
    }
}
