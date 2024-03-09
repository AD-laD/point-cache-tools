using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System.IO;//pour utitiliser getFileWithoutExtension

/// <summary>
/// Main script permettant de crééer des fichiers .pcache dynamiquement
/// </summary>
public class ChangePointCache : MonoBehaviour
{
    public VisualEffect visualEffect; // Faire glisser le VFX GameObject ici dans l'Inspector
    public string targetTag = "ObjectQuete"; // remplacer par le tag spécifique que vous souhaitez cibler si besoin
    public PCacheListGenerator pCacheList; // Faire glisser le script PCacheListGenerator
    public CustomPointCacheLoader pointCacheLoader; //Faire glisser le script CustomPointCacheLoader (permet de charger la texture du point cache)
    public PointCacheBakeTool pointCacheTool; //faire glisser le script PointCacheBakeTool  (permet de Bake les .pcache)

/// <summary>
/// Appelle toutes les fonctions permettant de récuperer le mesh lié à un objet, crééer un fichier .pcache, créer la map attribute
/// </summary>
    public void CreatePointCache(GameObject obj){
        //resetRotation();
        GameObject objetClique = obj;
        // Mise à jour du GameObject target dans le MyVFXTransformBinder

        GameObject parent = GameObject.Find("Parent");
        ResetParentTransform(parent);
        //SetParent(objetClique);
        MyVFXTransformBinder myVFXBinder = visualEffect.GetComponent<MyVFXTransformBinder>(); //on récupere le transform du vfx
        myVFXBinder.SetTarget(objetClique); //on met le game object en target du vfxPropertyBinder
        Mesh objetMesh = FindMeshFromGameObject(objetClique);//création du mesh associé au game object

        // if((objetClique.transform.rotation != Quaternion.identity)&&(objetMesh.name != "Sphere")){ //si l'objet est en rotation et est différent d'une sphere
        //     parent = SetParent(objetClique);
        // }

        if (objetMesh != null && objetClique !=null && pCacheList != null){
            if (!pCacheList.pCachealreadyExists(objetClique)){
                DynamicGenerator(objetMesh, objetClique);//génère un fichier .pcache à partir du mesh associé au gameobject
            }   
        }
        pCacheList.LoadPCacheFiles(); //update la liste
        PCacheObject pCacheObject = FindPCacheFile(objetClique); // Trouve le fichier .pcache dans la liste associé à l'objet touché
        
        // Si le fichier .pcache est trouvé,le charge dans le point cache du VFX Graph
        if (pCacheObject != null)
        {
            pointCacheLoader.LoadCustomPointCache(pCacheObject.filePath); //relier le point cache correspondant au vfx graph
        }
    }

    private void  OnDisable()
    {
        //pCacheList.DeletePCacheFiles(); //suppression des fichiers .pcache lorsque l'on arrête le jeu
        DeletePCacheFiles();
    }

/// <summary>
/// renvoie le mesh lié à un gameObject
/// </summary>
    Mesh FindMeshFromGameObject(GameObject targetObject){
        //si l'objet sur lequel on clique a un mesh, retourne ce mesh
        if (targetObject != null)
        {
            Mesh mesh = targetObject.GetComponent<MeshFilter>()?.sharedMesh;
            if (mesh != null)
            {
                // Si le GameObject a un MeshFilter avec un Mesh attaché
                mesh.MarkDynamic();//permet d'accèder aux propriétés du mesh
                return mesh;
            }
            else
            {
                // Si le Mesh est attaché directement au GameObject
                mesh = targetObject.GetComponent<Mesh>();
                if (mesh != null)
                {
                    mesh.MarkDynamic();
                    return mesh;
                }
                else
                {
                    Debug.LogWarning("Le GameObject ne contient pas de composant Mesh.");
                    return null;
                }
            }
        }
        else
        {
            Debug.LogWarning("Le GameObject est null.");
            return null;
        }
    }

/// <summary>
/// trouve un pCacheObject à partir d'un game object dans la liste de .pcache
/// </summary>
    PCacheObject FindPCacheFile(GameObject targetObject)
    {
        // Parcourt la liste pCacheList pour trouver le fichier .pcache associé à l'objet
        string targetObjectName = targetObject.name;
        foreach (PCacheObject pCacheObject in pCacheList.pCacheObjects)
        {
            string pCacheName = Path.GetFileNameWithoutExtension(pCacheObject.filePath);
            if (pCacheName == targetObjectName){
                return pCacheObject;
            }
        }
        // Si aucun fichier .pcache n'est trouvé, retourne null
        return null;
    }

/// <summary>
/// création du fichier .pcache à partir du mesh et du gameObject
/// </summary>
    public void DynamicGenerator (Mesh mesh, GameObject targetObject)
    {
        pointCacheTool = GetComponent<PointCacheBakeTool>();
        string targetObjectName = targetObject.name;
        if (pointCacheTool != null && mesh !=null) { 
            pointCacheTool.m_Mesh = mesh;
            string meshName = targetObjectName;
            pointCacheTool.filePath = "Assets/cPoints/" + meshName + ".pcache"; //le nom du fichier .pcache sera le même que celui du mesh
            pointCacheTool.MeshToPcache(); //création du fichier pcache
            
        } else {
            Debug.Log("pas de mesh associé au gameobject");
        }
    }

/// <summary>
/// permet de recopier les rotations d'un objet fixe sur l'objet parent du vfx graph
/// </summary>

    public GameObject SetParent(GameObject targetObject){
        GameObject parentObject = GameObject.Find("Parent");
        if (parentObject != null) {
            // Assignez le point cache au parent existant
            visualEffect.transform.parent = parentObject.transform;
            Quaternion objectRotation = targetObject.transform.rotation;
            Vector3 eulerRotation = objectRotation.eulerAngles;
            float rotationAroundX = eulerRotation.x;
            float rotationAroundY = eulerRotation.y;
            float rotationAroundZ = eulerRotation.z;

            MeshFilter meshFilter = targetObject.GetComponent<MeshFilter>();

            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                // Obtenir le centre du mesh en utilisant les informations de la bounding box
                Vector3 center = meshFilter.sharedMesh.bounds.center;

                // Convertir le centre du mesh de l'espace local à l'espace mondial si nécessaire
                Vector3 worldCenter = targetObject.transform.TransformPoint(center);
                Debug.Log(eulerRotation);
                parentObject.transform.RotateAround(worldCenter, Vector3.right, rotationAroundX);
                parentObject.transform.RotateAround(worldCenter, Vector3.up, rotationAroundY);
                parentObject.transform.RotateAround(worldCenter, Vector3.down, rotationAroundZ);

            }
            return parentObject;

        } else {
            Debug.LogError("L'objet parent n'a pas été trouvé !");
            return null;
        }
    }

/// <summary>
/// a faire : update en temps réel la rotation de l'objet parent sur le vfx graph
/// </summary>
    public void UpdateParentRotation(GameObject targetObject){//s'update a chaque frame
        GameObject parentObject = GameObject.Find("Parent");
        if (parentObject != null) {
            //but : effectuer la rotation du mesh sur le visual effect a chaque frame
        }
    }

/// <summary>
/// permet de reset la position d'un objet
/// </summary>
    void ResetParentTransform(GameObject obj)
    {
        // Réinitialise complètement le transform : position, rotation et échelle
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one; // Échelle par défaut (1, 1, 1)
    }

/// <summary>
/// suppression des fichiers .pcache créés dynamiquement dans le dossier .pcache
/// </summary>
    void DeletePCacheFiles()
    {
        string directoryPath = pCacheList.pCacheFolderPath;
        if (Directory.Exists(directoryPath))
        {
            string[] pcacheFiles = Directory.GetFiles(directoryPath, "*.pcache");

            foreach (string file in pcacheFiles)
            {
                File.Delete(file);
            }
        }
        else
        {
            Debug.LogWarning("Le répertoire spécifié n'existe pas ou n'a pas été trouvé.");
        }
    }
   
}
