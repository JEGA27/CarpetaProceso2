
//Gomez Aguilar Jose Ernesto - A01658889

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmQuiz : MonoBehaviour
{
    [SerializeField]
    float coeficient;

    Matrix4x4 matrix;
    Matrix4x4 originM;
    Vector3[] originalVertices;
    List<MeshFilter> cubes = new List<MeshFilter>();

    bool liftFlag;
    float rotationZ;
    int numberOfCubes;

    // Start is called before the first frame update
    void Start()
    {
        liftFlag = true;
        numberOfCubes = 3;
        originM = Transformaciones.Translate(0.5f, 0, 0);

        for(int i=0; i<numberOfCubes; i++)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            MeshFilter mesh = cube.GetComponent<MeshFilter>();
            cubes.Add(mesh);
        }

        originalVertices = cubes[0].mesh.vertices;
    }

    // Update is called once per frame
    void Update()
    {

        MoveArm();
    }

    private void MoveArm()
    {
        if(rotationZ > 45.0f || rotationZ < -45.0f)
        {
            liftFlag = !liftFlag;
        }

        if(liftFlag)
        {
            rotationZ += coeficient;
        }
        else
        {
            rotationZ += -coeficient;
        }

        matrix = Transformaciones.RotateZ(rotationZ)*originM;

        Matrix4x4 fm1 = Transformaciones.RotateZ(rotationZ)*Transformaciones.Translate(0.5f, 0, 0);
        Matrix4x4 fm2 = Transformaciones.RotateZ(rotationZ)*Transformaciones.Translate(0.5f, 0, 0)*Transformaciones.Translate(0.5f, 0, 0)*Transformaciones.RotateZ(rotationZ)*Transformaciones.Translate(0.5f, 0, 0);
        Matrix4x4 fm3 = Transformaciones.RotateZ(rotationZ)*Transformaciones.Translate(0.5f, 0, 0)*Transformaciones.Translate(0.5f, 0, 0)*Transformaciones.RotateZ(rotationZ)*Transformaciones.Translate(0.5f, 0, 0)*Transformaciones.Translate(0.5f, 0, 0)*Transformaciones.RotateZ(rotationZ)*Transformaciones.Translate(0.5f, 0, 0);

        cubes[0].mesh.vertices = Transformaciones.Transform(fm1 * Transformaciones.Scale(1, 0.5f, 0.5f), originalVertices);
        cubes[1].mesh.vertices = Transformaciones.Transform(fm2 * Transformaciones.Scale(1, 0.5f, 0.5f), originalVertices);
        cubes[2].mesh.vertices = Transformaciones.Transform(fm3 * Transformaciones.Scale(1, 0.5f, 0.5f), originalVertices);
    }
}
