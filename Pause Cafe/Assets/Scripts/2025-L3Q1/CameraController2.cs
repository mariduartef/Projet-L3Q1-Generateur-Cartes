using System.Collections;
using System.IO;
using UnityEngine;

//created by Chen Christophe L3Q1
//Edited by Mariana Duarte 04/2025
public class CameraController2 : MonoBehaviour
{
    public Camera terrainCamera;
    [Tooltip("Référence au RectTransform du RawImage qui affiche le RenderTexture")]
    public RectTransform rawImageRectTransform;

    [Header("Zoom Settings")]
    public float zoomSpeed = 2f;
    public float minZoom = 5f;
    public float maxZoom = 20f;

    [Header("Pan Settings")]
    public float panSpeed = 0.5f;
    private Vector3 dragOrigin;

    //Added by Mariana Duarte L3Q1 04/2025
    private static RenderTexture renderTexture;

    //Edited by Mariana Duarte L3Q1 04/2025
    void Start()
    {
        renderTexture = terrainCamera.targetTexture;
    }

    void Update()
    {
        HandleZoom();
        HandlePan();
        
    }
    //Author :Chen Chrisophe L3Q1 2025
    //gestion du zoom sur le terrain
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // Vérifier si le curseur est dans la zone du RawImage
            bool isCursorOverRawImage = RectTransformUtility.RectangleContainsScreenPoint(
                rawImageRectTransform,
                Input.mousePosition
            );

            if (isCursorOverRawImage)
            {
                terrainCamera.orthographicSize -= scroll * zoomSpeed;
                terrainCamera.orthographicSize = Mathf.Clamp(terrainCamera.orthographicSize, minZoom, maxZoom);
            }
        }
    }

    //Author :Chen Chrisophe L3Q1 2025
    //gestion du deplacement de la caméra 
    void HandlePan()
    {
        
        if (Input.GetMouseButtonDown(1))
        {
            dragOrigin = GetMouseWorldPosition();
        }
        if (Input.GetMouseButton(1))
        {
            Vector3 currentMousePos = GetMouseWorldPosition();
            Vector3 difference = dragOrigin - currentMousePos;
            terrainCamera.transform.position += difference;
        }
    }

    //Author :Chen Chrisophe L3Q1 2025
    //convertir la position du curseur sur l'interface graphique en positon monde
    Vector3 GetMouseWorldPosition()
    {
        Vector2 localPoint;
        // Convertir la position de l'écran dans l'espace local du RawImage
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rawImageRectTransform,
            Input.mousePosition,
            null, // Canvas en Screen Space Overlay
            out localPoint))
        {
            // On normalise les coordonnées locales (0 à 1)
            Rect rect = rawImageRectTransform.rect;
            float normalizedX = (localPoint.x - rect.x) / rect.width;
            float normalizedY = (localPoint.y - rect.y) / rect.height;
            Vector2 viewportPoint = new Vector2(normalizedX, normalizedY);

            // Créer un rayon à partir du viewport point
            Ray ray = terrainCamera.ViewportPointToRay(viewportPoint);
            // Définir un plan horizontal à y = 0 (où se trouve le terrain)
            Plane terrainPlane = new Plane(Vector3.up, Vector3.zero);
            if (terrainPlane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }
            if (RectTransformUtility.RectangleContainsScreenPoint(rawImageRectTransform, Input.mousePosition, null))
            {
                return ray.GetPoint(distance);
            }
        }


        return Vector3.zero;
    }

    //Author :Chen Chrisophe L3Q1 2025
    //utilisé pour le debogage
    void DebugMousePosition()
    {
        Vector3 pos = GetMouseWorldPosition();
        Debug.Log("Mouse Position on Terrain: " + pos);
    }

    //Author : Mariana Duarte L3Q1 04/2025
    //Captures a screenshot of the map that is finished and being saved and saves it as a png file
    public static IEnumerator captureScreenshot(string fileName)
    {
        if (!Directory.Exists(Application.persistentDataPath + "/Save/MapScreenshots"))
            Directory.CreateDirectory(Application.persistentDataPath + "/Save/MapScreenshots");

        yield return new WaitForEndOfFrame();

        Texture2D screenshotTexture = new Texture2D(460, 600, TextureFormat.RGB24, false);

        RenderTexture.active = renderTexture;
        screenshotTexture.ReadPixels(new Rect(270, 200, 460, 600), 0, 0);
        screenshotTexture.Apply();
        RenderTexture.active = null; 

        byte[] bytes = screenshotTexture.EncodeToPNG();
        string filePath = Application.persistentDataPath + "/Save/MapScreenshots/" + fileName + ".png"; 
        File.WriteAllBytes(filePath, bytes);

        Debug.Log("Screenshot saved to: " + filePath);

        Destroy(screenshotTexture);
    }

}
