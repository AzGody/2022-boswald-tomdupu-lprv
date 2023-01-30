using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class HTTPRequest : MonoBehaviour
{

    public Color c1 = Color.red;
    public Color c2 = Color.red;

    public GameObject moniteur;

    private LineRenderer lineRenderer;
    private ArrayList datas = new ArrayList();

 
    void Start()
    {
        moniteur = GameObject.Find("Moniteur");
   
        lineRenderer = moniteur.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = 0.005f;
        lineRenderer.useWorldSpace = true;
   
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(c1, 0.0f), new GradientColorKey(c2, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        lineRenderer.colorGradient = gradient;
        
        Debug.Log("Requête");
        string uri = "https://www.labri.fr/perso/pecher/RV/rv_advanced.php";
        StartCoroutine(getRequest(uri));
    }

    void Update()
    {
        int nbPoints =  datas.Count;
        lineRenderer.positionCount = nbPoints;
        var points = new Vector3[nbPoints];

        float somme = 0f;
        float max = 0f;
        float min = 200f;
        for (int i=0; i<nbPoints; i++){
            float val = (int) datas[i];
            somme +=  val;
            if (val > max)
                max = val;
            if (val < min)
                min = val;
        }
        float moyenne = somme / nbPoints;
   
        Renderer rend = moniteur.GetComponent<Renderer>(); 
        float xmin = rend.bounds.min.x; float xmax = rend.bounds.max.x;
        float ymin = rend.bounds.min.y; float ymax = rend.bounds.max.y;
        float zmin = rend.bounds.min.z; float zmax = rend.bounds.max.z;
        for (int i=0; i<nbPoints; i++){
            var val = (int) datas[i];
            var x = xmin + ((xmax-xmin) * i)/nbPoints;
            var y = ymin + (ymax-ymin) * ((val-min)/(max-min));
            var z = zmin + ((zmax-zmin) * i)/nbPoints;
            points[i] = new Vector3(x,y,z);
        }
      
        lineRenderer.SetPositions(points);
        
        string time = System.DateTime.UtcNow.ToLocalTime().ToString("HH:mm:ss");
        string affichage = time + " : " + (int) moyenne + " BPM";
      
    }

     IEnumerator getRequest(string uri)
    {       
        UnityWebRequest uwr;
            while(true){
                uwr = UnityWebRequest.Get(uri);
                yield return uwr.SendWebRequest();
                if (uwr.result == UnityWebRequest.Result.ConnectionError){
                   Debug.Log("Errreur " + uwr.error);
                }
                else{
                    string contenu = uwr.downloadHandler.text;
                    Debug.Log("Reçu " + contenu);
                    datas = new ArrayList();
                    
                    char[] delims = {',','[',']',':',' ','\n'};
                    string[] tokens = contenu.Split(delims);
                    
                    for (int i=3; i<tokens.Length; i++){
                        try{
                            float val = float.Parse(tokens[i]);
                            datas.Add( (int) val);
                        } catch (FormatException) {}
                    }
                    
                }
                yield return new WaitForSeconds(5.0f);
            }    
    }

}