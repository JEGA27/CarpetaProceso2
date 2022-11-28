using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EjercicioRubric : MonoBehaviour
{
    public List<MeshFilter> Cubos = new List<MeshFilter>();
    public List<Vector3> positions = new List<Vector3>();

    Vector3[] originales;
    public List<Matrix4x4> mOriginales = new List<Matrix4x4>();
    public List<Matrix4x4> matrices = new List<Matrix4x4>();

    float rotZ;
    float rotX;
    float rotY;

    // Start is called before the first frame update
    void Start()
    {
        int doub = 0;
        for(int i = 0; i < 8; i++){
            Vector3 y = doub < 2? Vector3.up : Vector3.down;
            Vector3 z = i < 4? Vector3.forward : Vector3.back;

            if(i%2 == 0){
            positions.Add((Vector3.left + y + z)/2);
            }
            else if(i%2 != 0){
                positions.Add((Vector3.right + y + z)/2);
            }

            doub++;
            if(doub >= 4){
                doub = 0;
            }

            GameObject Cubo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Cubos.Add(Cubo.GetComponent<MeshFilter>());
            mOriginales.Add(Transformaciones.Translate(positions[i].x, positions[i].y, positions[i].z));
            matrices.Add(mOriginales[i]);
        }

        originales = Cubos[0].mesh.vertices;
    }

    // Update is called once per frame
    void Update()
    {
        if(rotZ < 360.0f *2f){
            rotZ += 0.1f;
            for(int i = 4; i < 8; i++){
                matrices[i] = Transformaciones.RotateZ(rotZ)* mOriginales[i];
            }
        }
        else if(rotY < 360.0f)
        {
            rotY += 0.1f;
            matrices[2] = Transformaciones.RotateY(rotY) * mOriginales[2];
            matrices[3] = Transformaciones.RotateY(rotY) * mOriginales[3];
            matrices[6] = Transformaciones.RotateY(rotY) * mOriginales[6];
            matrices[7] = Transformaciones.RotateY(rotY) * mOriginales[7];
        }
        else{
            rotZ = rotY = 0;
            for(int i = 4; i < 8; i++){
                matrices[i] = mOriginales[i];
            }
        }

        for(int i = 0; i< 8; i++)
            Cubos[i].mesh.vertices = Transformaciones.Transform(matrices[i],originales);
    }
}
