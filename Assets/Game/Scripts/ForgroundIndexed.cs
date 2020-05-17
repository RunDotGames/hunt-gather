
using UnityEngine;

public class ForgroundIndexed : MonoBehaviour {

  private static ForgroundConfig config;
  
  public void Start(){
    if(config == null){
      config =GameObject.FindObjectOfType<ForgroundConfig>();
      if(config == null){
        var obj = new GameObject();
        config = obj.AddComponent<ForgroundConfig>();
      }
    }
    
  }

  public void Update(){
    var max = Camera.main.pixelHeight;
    var cameraPos = Camera.main.WorldToScreenPoint(transform.position).y;
    var adjusted = config.max * (cameraPos/max);
    transform.position = new Vector3(transform.position.x, transform.position.y, adjusted);
  }
}