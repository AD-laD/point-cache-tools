using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Script à poser sur un gameobject sur lequel on veut appliquer la création dynamique de Point Cache
/// </summary>
public class MouseDown : MonoBehaviour
{
    public VisualEffect visualEffect; //glisser le VFX
    private GameObject myVFX;
    private ChangePointCache mainScript;
    private bool isClicked =false;

    
    // Start is called before the first frame update

    void OnMouseDown(){
        //lors du clic
        //Debug.Log("test");
        mainScript.CreatePointCache(this.gameObject);
        isClicked = true;
    }

    void Start()
    {
        //Debug.Log("start");
        myVFX=GameObject.Find("/PointCacheVFX");
        mainScript = visualEffect.GetComponent<ChangePointCache>();
        
    }

    void Update()
    {
        if (isClicked == true){
            //a faire ici : update la rotation du vfx à l'aide de l'objet parent
            //ou autre piste : update la rotation en rajoutant des notes rotation sur le VFX et en définissant les rotations avec du code
        }
        
    }
}
