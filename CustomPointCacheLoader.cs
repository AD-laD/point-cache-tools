using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Experimental.Rendering;
using System;
using System.Collections.Generic;

/// <summary>
/// Classe contenant un nom de texture lié à une texture2D
/// </summary>

[System.Serializable]
public class PCacheTexture
{
    public string m_name;
    public Texture2D m_texture;
}

/// <summary>
/// permet de charger dynamiquement une texture2D liée à un fichier .pcache si la texture n'a pas déjà été créée, et de la relier au VFX Graph via l'attribute map position ;
///créée une liste dynamique de textures ;
///si la texture a déjà été créée, la relie simplement au VFX graph 
/// </summary>
public class CustomPointCacheLoader : MonoBehaviour
{
    public VisualEffect vfxGraph; //référence au VFX graph à glisser dans l'inspecteur
    private int textureSize = 64;
    public List<PCacheTexture> pCacheTextures = new List<PCacheTexture>(); // Liste des textures
    bool textureExists = false; //booléen permettant de savoir si une texture existe déjà ou non

    public void LoadCustomPointCache(string pCacheFilePath)
    {
        if (System.IO.File.Exists(pCacheFilePath))
        {
            Texture2D positionTexture = null;
            //on parcourt la liste pour savoir si la texture existe déjà
            foreach (PCacheTexture texture in pCacheTextures)
            {
                if (texture.m_name ==System.IO.Path.GetFileNameWithoutExtension(pCacheFilePath)+ "position")
                {
                    textureExists = true;
                    positionTexture = texture.m_texture;
                    break;
                } else textureExists = false;
            }

            if (!textureExists) // si la texture n'existe pas
            {
                positionTexture = CreateTextureFromPCache(pCacheFilePath);
                if (positionTexture != null){
                    string positionTextureName = System.IO.Path.GetFileNameWithoutExtension(pCacheFilePath) + "position";
                    pCacheTextures.Add(new PCacheTexture { m_name = positionTextureName, m_texture = positionTexture });
                    Debug.Log("il y a une position texture");
                }
            }
            // Affecte la texture au module Point Cache du VFX Graph
            string attributeName = "PCacheTexture";
            vfxGraph.SetTexture(attributeName, positionTexture);
        } 
        else if (pCacheFilePath == "delete") //déconnecte la texture visuellement (à l'aide d'une texture transparente)
        { 
            Texture2D transparentTexture = GenerateTransparentTexture(64, 64);
            vfxGraph.SetTexture("PCacheTexture", transparentTexture);
        } 
        else
        {
            Debug.LogError("Le fichier de cache specifie n'existe pas : " + pCacheFilePath);
        }
    }

/// <summary>
/// création d'une texture position à partir d'un .pcache (attribute map)
/// par lecture du .pcache et conversion des coordonnées liées à la position en couleurs
/// </summary>

    public Texture2D CreateTextureFromPCache(string pCacheFilePath){
        string[] lines = System.IO.File.ReadAllLines(pCacheFilePath);
        int startIndex = 12;

        //creation de la texture position avec les bons paramètres
        Texture2D positionTexture = new Texture2D(textureSize, textureSize, GraphicsFormat.R16G16B16A16_SFloat, TextureCreationFlags.None);
        positionTexture.filterMode = FilterMode.Bilinear;

        Color[] colors = new Color[textureSize * textureSize];
        int index = 0;

        for(int i=startIndex; i<lines.Length && index<colors.Length; i++){
            string[] values = lines[i].Split(' ');
            // Convertit les valeurs de position en couleurs pour chaque pixel de la texture
            string values0= values[0].Replace(" ","");
            float floatX = float.Parse(values[0], System.Globalization.CultureInfo.InvariantCulture);//converti la string en float
            string values1= values[1].Replace(" ","");
            float floatY = float.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture);
            string values2= values[2].Replace(" ","");
            float floatZ = float.Parse(values[2], System.Globalization.CultureInfo.InvariantCulture);
            colors[index] = new Color(floatX, floatY, floatZ, 1.0f);
            index++;
        }
        //Applique les couleurs à la texture
        if(positionTexture != null){
            positionTexture.SetPixels(colors);
            positionTexture.Apply();
            Debug.Log("les pixels sont appliqués");
        } else {
            Debug.Log("la texture est vide ou n'existe pas");
        }
        return positionTexture;
    }

/// <summary>
/// renvoie une texture2D Transparent si l'on veut faire "disparaitre" le VFX
/// </summary>

    public Texture2D GenerateTransparentTexture(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);

        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear; // Utilisation de Color.clear pour définir des pixels transparents
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return texture;
    }

}