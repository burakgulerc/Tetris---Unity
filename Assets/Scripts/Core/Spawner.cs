using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Shape[] m_allShapes;

    Shape GetRandomShape()
    {
        int i = Random.Range(0,m_allShapes.Length);
        if(m_allShapes[i] != null)
        {
            return m_allShapes[i];
        } else
        {
            Debug.Log("Warning! Invalid shape");
            return null;
        }

    }

    public Shape SpawnShape()
    {
        Shape shape = null;
        shape = Instantiate(GetRandomShape(),transform.position,Quaternion.identity) as Shape;

        if(shape != null)
        {
            return shape;
        }
        else
        {
            Debug.Log("Warning! Invalid shape in spawner");
            return null;

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
