using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class PCacheObject
{
    public string filePath;
}

/// <summary>
/// génère une liste contenant les chemins d'acces vers le .pcache
/// </summary>
public class PCacheListGenerator : MonoBehaviour
{

    public string pCacheFolderPath = "Assets/cPoints"; // Chemin du dossier contenant les fichiers .pcache
    public List<PCacheObject> pCacheObjects = new List<PCacheObject>(); // Liste des fichiers .pcache

    void Start()
    {
        LoadPCacheFiles();
    }

    public void LoadPCacheFiles()
    {
        // Vérifie si le dossier existe
        if (Directory.Exists(pCacheFolderPath))
        {
            // Si la liste n'est pas vide, la vider
            if (pCacheObjects != null){
                if (pCacheObjects.Count > 0)
                {
                    pCacheObjects.Clear();
                }
            }
            
            // Obtient tous les fichiers .pcache dans le dossier
            string[] files = Directory.GetFiles(pCacheFolderPath,"*.pcache");

            foreach (string file in files)
            {
                string assetPath = file.Replace(Application.dataPath, "").Replace('\\', '/');
                Debug.Log(assetPath); 
                if (assetPath != null)
                {
                    // Ajoute l'association à la liste
                    pCacheObjects.Add(new PCacheObject { filePath = assetPath });
                }
            }
        }
        else
        {
            Debug.LogError("Le dossier spécifié n'existe pas : " + pCacheFolderPath);
        }
    }

    public bool pCachealreadyExists(GameObject targetObject){ //vérifie si le point cache n'existe pas déjà
        if (pCacheObjects.Count >0){
            foreach (PCacheObject Pcache in pCacheObjects) //on parcourt la liste de .pcache
            {
                string pcache = Path.GetFileNameWithoutExtension(Pcache.filePath);//supprime la terminaison .pcache et le chemin
                if (pcache== targetObject.name){ //si le nom de l'objet existe déjà dans la liste de .pcache
                    return true; //alors l'objet a déjà un fichier .pcache
                }
            }
        }
        return false;
    }

/// <summary>
/// suppression des fichiers .pcache créés dynamiquement dans le dossier .pcache
/// </summary>
    public void DeletePCacheFiles()
    {
        foreach (PCacheObject pCacheObject in pCacheObjects)
        {
            // string filePath = Path.Combine(pCacheFolderPath, pCacheObject.filePath);
            if (File.Exists(pCacheObject.filePath))
            {
                try
                {
                    File.Delete(pCacheObject.filePath);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error deleting file {pCacheObject.filePath}: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"File {pCacheObject.filePath} does not exist or cannot be found.");
            }
        }
    }

}