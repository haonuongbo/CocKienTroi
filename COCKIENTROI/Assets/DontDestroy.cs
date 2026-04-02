using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class DontDestroy : MonoBehaviour
{
    private static  GameObject[] persistantObject = new GameObject[3];
    public int objectIndex;
    
 void Awake()
    {
        if (persistantObject[objectIndex] == null)
        {
            persistantObject[objectIndex] = this.gameObject;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (persistantObject[objectIndex] != this.gameObject)
        {
            Destroy(this.gameObject);
        }   
    }
}
   
