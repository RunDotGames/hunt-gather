
using UnityEngine;

public class ForgroundIndexed : MonoBehaviour {

  private ForgroundConfig config;
  public void Start(){
    config =GameObject.FindObjectOfType<ForgroundConfig>();
  }

  public void Update(){
    var max = Camera.main.pixelHeight;
    var cameraPos = Camera.main.WorldToScreenPoint(transform.position).y;
    var adjusted = config.max * (cameraPos/max);
    transform.position = new Vector3(transform.position.x, transform.position.y, adjusted);
  }
}