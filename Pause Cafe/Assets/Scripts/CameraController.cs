using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Characters;

/// Created by Timothé MIEL - L3L1;
/// 
/// La caméra possède différents mouvements :
/// Click and drag, flèches directionnelles et zoom via la molette, depuis l'écran de jeu, si la caméra n'est pas verrouillée.
/// La caméra est verrouillée automatiquement en partie CPU vs CPU, et peut l'être manuellement depuis le menu Pause.
/// 
/// Retournement à 180° depuis le menu Pause ou automatiquement à chaque changement de tour.
public class CameraController : MonoBehaviour
{
    Vector3 mousePos;
    Vector3 mousePosOld;

    public Vector3 cameraPosGoal;
    bool cameraLocked;
    public bool cameraMoved;

    Vector3 cameraRotGoal;
    int ralentisseurRotationCamera = 15;
    bool sensInitial;
    bool cameraRotating;
    bool savePosition;

    Character oldCharFocused;
    float characterOffset = 8f;

    float forceCamX;
    float forceCamY;
    float forceCamZ;

    float scrollWheelStrengh = 7.0f;
    float dragDecreaser = 0.003f;
    int limiteGauche = -14;
    int limiteDroite = 14;
    int limiteAvant = 9;
    int limiteArrière = -24;
    int limiteHaut = 20;
    int limiteBas = 8;
    float partAnciennePositionCamera = 0.85f;
    float partNouvellePositionCamera = 0.15f;

    public Toggle verrouillage;

    int sensActuel;

    bool UIlock;

    // Start is called before the first frame update
    void Start()
    {
        cameraLocked = GameObject.Find("Gestion").GetComponent<MainGame>().IsOnlyCPUGame();
        if (cameraLocked) // On affiche la case "Verrouiller la caméra" comme cochée si partie CPU
        {
            verrouillage.isOn = true;
        }
        cameraPosGoal = transform.position;
        sensInitial = true;
        sensActuel = 0;
    }

    // LateUpdate is called once per frame after Update call
    void LateUpdate()
    {
        // Rotation caméra
        transform.RotateAround(Vector3.zero, Vector3.up, cameraRotGoal.y / ralentisseurRotationCamera);
        cameraRotGoal.y -= cameraRotGoal.y / ralentisseurRotationCamera;

        if (cameraRotGoal.y <= 1) // Mise à 0 manuelle
        {
            cameraRotGoal.y = 0;
            int rotation = sensInitial ? 0 : 180;
            transform.rotation = Quaternion.Euler(60, rotation, 0);
            cameraRotating = false;

            if (savePosition)
            {
                cameraPosGoal = transform.position;
            }
        }

        if (verrouillage.isOn)
        {
            cameraLocked = true;
        }
        else
        {
            cameraLocked = false;
        }

        if (!cameraLocked && !UIlock)
        {

            // Mise à jour de la position de la souris

            mousePosOld = mousePos;
            mousePos = Input.mousePosition;

            forceCamX = 0.0f;
            forceCamY = 0.0f;
            forceCamZ = 0.0f;

            // Zoom : molette

            if (Input.GetAxis("Mouse ScrollWheel") != 0.0f)
            {
                forceCamY -= Input.GetAxis("Mouse ScrollWheel") * scrollWheelStrengh;
            }

            // Déplacement de la caméra : click & drag gauche

            if (Input.GetMouseButton(0))
            {
                forceCamX += (mousePosOld.x - mousePos.x) * dragDecreaser * cameraPosGoal.y;
                forceCamZ += (mousePosOld.y - mousePos.y) * dragDecreaser * cameraPosGoal.y;
            }

            // Déplacement de la caméra : flèches directionnelles

            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Z))
            {
                forceCamZ += 1;
            }
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.Q))
            {
                forceCamX -= 1;
            }
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                forceCamZ -= 1;
            }
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                forceCamX += 1;
            }

            if (!sensInitial)
            {
                forceCamX = -forceCamX;
                forceCamZ = -forceCamZ;
            }

            // Nouveau vecteur

            Vector3 newCameraPosGoal = new Vector3(
                cameraPosGoal.x + forceCamX,
                cameraPosGoal.y + forceCamY,
                cameraPosGoal.z + forceCamZ
            );

            cameraMoved = false;

            // Vérification des limites

            if (newCameraPosGoal.x > limiteGauche && newCameraPosGoal.x < limiteDroite)
            {
                cameraMoved = true;
                cameraPosGoal.x = newCameraPosGoal.x;
            }

            if (newCameraPosGoal.y > limiteBas && newCameraPosGoal.y < limiteHaut)
            {
                cameraMoved = true;
                cameraPosGoal.y = newCameraPosGoal.y;
                newCameraPosGoal.z += Input.GetAxis("Mouse ScrollWheel") * scrollWheelStrengh;
            }

            if (newCameraPosGoal.z > limiteArrière && newCameraPosGoal.z < limiteAvant)
            {
                cameraMoved = true;
                cameraPosGoal.z = newCameraPosGoal.z;
            }
            
            if (forceCamX == 0.0f && forceCamY == 0.0f && forceCamZ == 0.0f)
            {
                cameraMoved = false;
            }

            // Déplacement caméra

            transform.position = new Vector3(
            transform.position.x * partAnciennePositionCamera + cameraPosGoal.x * partNouvellePositionCamera,
            transform.position.y * partAnciennePositionCamera + cameraPosGoal.y * partNouvellePositionCamera,
            transform.position.z * partAnciennePositionCamera + cameraPosGoal.z * partNouvellePositionCamera
            );
        }
        
    }

    // Added by Timothé MIEL - L3L1
    // Appelée à la fin de nextTurn;
    // On retourne instantanément si méthode appelée avant début de partie
    // quand premier personnage contrôlé est de l'équipe 1 (côté opposé)
    public void GoToCharacter(Character c, bool beforeGameStart)
    {
        cameraPosGoal = new Vector3(
        c.go.transform.position.x,
        cameraPosGoal.y,
        c.go.transform.position.z
        );

        if ((c.team != oldCharFocused.team && c.team != sensActuel)|| beforeGameStart)
        {
            ReverseCamera(false, beforeGameStart);
        }
        Debug.Log("Sens initial : " + sensInitial);
        if (sensInitial)
        {
            cameraPosGoal.z -= characterOffset;
        }
        else
        {
            cameraPosGoal.z += characterOffset;
        }
        if (beforeGameStart)
        {
            transform.position = cameraPosGoal;
        }
        oldCharFocused = c;
    }

    // Added by Timothé MIEL - L3L1
    // Permet de retourner la caméra de 180°
    // Le fait sans animation si instantaneous == true
    public void ReverseCamera(bool stayAtDestination, bool instantaneous)
    {
        if (cameraRotating == false)
        {
            
            int buffer = -limiteArrière;
            limiteArrière = -limiteAvant;
            limiteAvant = buffer;

            sensInitial = !sensInitial;
            cameraRotating = true;

            savePosition = stayAtDestination;

            sensActuel = sensActuel == 0 ? 1 : 0; // Inversion du sens actuel à chaque rotation

            if (instantaneous)
            {
                sensActuel = 1; // Si rotation instantanée en début de partie;
                int rotation = sensInitial ? 0 : 180;
                transform.rotation = Quaternion.Euler(60, rotation, 0);
            }
            else
            {
                cameraRotGoal.y = 180;
            }
        }
    }

    // Added by Timothé MIEL - L3L1
    // À utiliser en début de partie pour éviter une NullReferenceException
    public void setOldCharFocused(Character c)
    {
        oldCharFocused = c;
    }

    // Added by Timothé MIEL - L3L1
    // Empêche les mouvements de caméra lorsque une UI (menu Pause, menu Aide) est ouverte
    public void IsUIOpen(bool opened)
    {
        UIlock = opened;
    }
}

