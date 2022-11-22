/*
Diógenes Grajales Corona A01653251
José Ernesto Gómez Aguilar A01658889
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.FilePathAttribute;

public class RayCaster : MonoBehaviour
{
    public Renderer renderer;
    public Texture2D background;
    public int numberSpheres = 20;

    Vector3 Ia;
    Vector3 Id;
    Vector3 Is;
    Vector3 n;
    Vector3 LIGHT;
    Vector3 CAMERA;
    float ALPHA;
     
    Camera mainCam;
    Vector2 CameraResolution;

    float frusttumHeight;
    float frustumWidth;

    float pixelWidth;
    float pixelHeight;

    Texture2D texture;

    Vector3 topLeft;

    struct Esfera
    {

        public float kdr;
        public float kdg;
        public float kdb;

        public Vector3 kd;
        public Vector3 ka;
        public Vector3 ks;


        public Vector3 SC;
        public float SR;

    };

    List<Esfera> esferas;


    // Start is called before the first frame update
    void Start()
    {
        
        esferas = new List<Esfera>();

        //Sets plane
        renderer.transform.localRotation = Quaternion.Euler(90, 0, 0);
        renderer.transform.localPosition = new Vector3(0, 5, -2);

        //Sets camera
        CameraResolution = new Vector2(640,480);
        CAMERA = new Vector3(0f, 4f, 5.5f);
        mainCam = Camera.main;
        mainCam.transform.position = CAMERA;
        mainCam = Camera.main;
        mainCam.transform.position = CAMERA;

        //Sets light variable
        LIGHT = new Vector3(0f, 7.5f, 3f);
        Ia = new Vector3(0.7f, 0.7f, 0.7f);
        Id = new Vector3(0.8f, 0.8f, 1f);
        Is = new Vector3(1f, 1f, 1f);

        // Create pointLight
        GameObject pointLight = new GameObject("ThePointLight");
        Light lightComp = pointLight.AddComponent<Light>();
        pointLight.transform.position = LIGHT;
        lightComp.type = LightType.Point;
        lightComp.color = new Color(Id.x, Id.y, Id.z);
        lightComp.intensity = 4;

        //Sets Frustrum size 
        frusttumHeight = 2.0f * mainCam.nearClipPlane * Mathf.Tan(mainCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        frustumWidth = frusttumHeight * mainCam.aspect;

        //Sets pixels size
        pixelWidth = frustumWidth / (float)CameraResolution.x;
        pixelHeight = frusttumHeight / (float)CameraResolution.y;

        //Get the pixel at the top left corner from the frustrum
        topLeft = FindTopLeftFrusrtumNear();

        //Creates and sorts the spheres depending on its z position
        setSpheres(numberSpheres);
        esferas.Sort((s1, s2) => s1.SC.z.CompareTo(s2.SC.z));

        //Creates the texture
        texture = new Texture2D(Mathf.RoundToInt(CameraResolution.x), Mathf.RoundToInt(CameraResolution.y), TextureFormat.ARGB32, false);


        for (int y = 0; y < CameraResolution.y; y++)
        {
            for (int x = 0; x < CameraResolution.x; x++)
            {
                Color bg = background.GetPixel(x, y);
                texture.SetPixel(x, y, bg);
            }
        }
        texture.Apply();

        for (int s = esferas.Count-1; s >= 0; s--) {

            for (int y = 0; y < CameraResolution.y; y++)
            {
                for (int x = 0; x < CameraResolution.x; x++)
                {
                    Color color = GetPixel(new Vector3(x, y, 0f), esferas[s]);

                    if (color != Color.clear) texture.SetPixel(x, -y, color);

                }
            }

        }
        
        texture.Apply();


        texture.filterMode = FilterMode.Point;
        Renderer rend2 = renderer.GetComponent<Renderer>();
        Shader shader = Shader.Find("Unlit/Texture");
        rend2.material.shader = shader;
        rend2.material.mainTexture = texture;

        SaveRender(texture);

    }

    //Gets the coordinates of a specific pixel center 
    Vector3 Cast(Vector3 coords)
    {

        Vector3 center = topLeft;

        center += (pixelWidth / 2f) * mainCam.transform.right;
        center -= (pixelHeight / 2f) * mainCam.transform.up;
        center += (pixelWidth) * mainCam.transform.right * coords.x;
        center -= (pixelHeight) * mainCam.transform.up * coords.y;


        return center;
    }

    //Checks whenever there is a sphere in a specific pixel and gets its color
    Color GetPixel(Vector3 coords, Esfera esfera)
    {

        Vector3 center = Cast(coords);

        Vector3 u = (center - CAMERA);
        u = u.normalized;
        Vector3 oc = CAMERA - esfera.SC;

        float nabla = (Vector3.Dot(u, oc) * Vector3.Dot(u, oc)) - ((oc.magnitude * oc.magnitude) - (esfera.SR * esfera.SR));

        if (nabla < 0)
        {

            return Color.clear;
        }

        float dpos = -1 * Vector3.Dot(u, oc) + Mathf.Sqrt(nabla);
        float dneg = -1 * Vector3.Dot(u, oc) - Mathf.Sqrt(nabla);

        Vector3 color = new Vector3();


        if (Mathf.Abs(dpos) < Mathf.Abs(dneg))
        {

            color = Illumination(CAMERA + dpos * u, esfera);
        }
        else
        {
            color = Illumination(CAMERA + dneg * u, esfera);
        }


        return new Color(color.x, color.y, color.z);
    }

    //Gets a vector with the rgb attributes of a specific point
    Vector3 Illumination(Vector3 PoI2, Esfera esfera)
    {
        Vector3 A = new Vector3(esfera.ka.x * Ia.x, esfera.ka.y * Ia.y, esfera.ka.z * Ia.z);
        Vector3 D = new Vector3(esfera.kd.x * Id.x, esfera.kd.y * Id.y, esfera.kd.z * Id.z);
        Vector3 S = new Vector3(esfera.ks.x * Is.x, esfera.ks.y * Is.y, esfera.ks.z * Is.z);

        Vector3 l = LIGHT - PoI2;
        Vector3 v = CAMERA - PoI2;

        n = PoI2 - esfera.SC;

        float dotNuLu = Vector3.Dot(n.normalized, l.normalized);
        float dotNuL = Vector3.Dot(n.normalized, l);

        Vector3 lp = n.normalized * dotNuL;
        Vector3 lo = l - lp;
        Vector3 r = lp - lo;
        D *= dotNuLu;
        float w = Mathf.Pow(Vector3.Dot(v.normalized, r.normalized), ALPHA);
        if (w is float.NaN) w = 0;
        S *= w;
        return A + D + S;
    }

    //Gets the pixel at the top left corner of the camera frustrum
    Vector3 FindTopLeftFrusrtumNear()
    {
        //localizar camara
        Vector3 o = CAMERA;
        //mover hacia adelante
        Vector3 p = o + mainCam.transform.forward * mainCam.nearClipPlane;
        //mover hacia arriba, media altura
        p += mainCam.transform.up * frusttumHeight / 2.0f;
        //mover a la izquierda, medio ancho
        p -= mainCam.transform.right * frustumWidth / 2.0f;
        return p;

    }

    //Creates numSpheres Spheres with random attributes
    void setSpheres(int numSpheres)
    {


        for (int i = 0; i < numSpheres; i++)
        {
            Esfera esfera = new Esfera();

            esfera.kdr = Random.Range(0.5f, 1.0f);
            esfera.kdg = Random.Range(0.5f, 1.0f);
            esfera.kdb = Random.Range(0.5f, 1.0f);

            esfera.kd = new Vector3(esfera.kdr, esfera.kdg, esfera.kdb);
            esfera.ka = new Vector3(esfera.kdr / 5.0f, esfera.kdg / 5.0f, esfera.kdb / 5.0f);
            esfera.ks = new Vector3(esfera.kdr / 3.0f, esfera.kdg / 3.0f, esfera.kdb / 3.0f);

            esfera.SC = new Vector3(Random.Range(-2.0f, 2.0f), Random.Range(2.0f, 6.0f), Random.Range(8.0f, 10.0f));
            esfera.SR = Random.Range(0.1f, 0.35f);

            // Create sphere
            GameObject sph = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sph.transform.position = esfera.SC;
            sph.transform.localScale = new Vector3(esfera.SR * 2f, esfera.SR * 2f, esfera.SR * 2f);
            Renderer rend = sph.GetComponent<Renderer>();
            rend.material.shader = Shader.Find("Specular");
            rend.material.SetColor("_Color", new Color(esfera.kd.x, esfera.kd.y, esfera.kd.z));
            rend.material.SetColor("_SpecColor", new Color(esfera.ks.x, esfera.ks.y, esfera.ks.z));

            esferas.Add(esfera);
        }


    }

    //Saves a texture as png
    void SaveRender(Texture2D texture) 
    {
        byte[] bytes = texture.EncodeToPNG();
        var dirPath = Application.dataPath + "/Scripts/";
        if (!System.IO.Directory.Exists(dirPath))
        {
            System.IO.Directory.CreateDirectory(dirPath);
        }
        System.IO.File.WriteAllBytes(dirPath + "render" + ".png", bytes);

    }

    

}
