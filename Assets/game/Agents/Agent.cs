
using UnityEngine;

delegate void AgentUpdate();

public interface Agent{
  void Update();
  bool IsBusy();
  void Release();
  void Resume();
  int GetAvailability();
}

public class AgentConfigCommon {
  public float speed;
  public Transform transform;
  public float arrivalDistance;
  public SeasonalTimeRange restRange;
}

public class AgentPather{

  public Transform transform;
  public float speed;
  public float arrivalDistance;

  public bool ToPoint(Vector2 point){
    var path = point - (Vector2)transform.position;
    var distance = path.magnitude;
    if(distance <= arrivalDistance){
      return true;
    }
    var delta = Vector2.ClampMagnitude(Time.deltaTime * speed * path.normalized, distance);
    transform.position = new Vector3(delta.x + transform.position.x, delta.y + transform.position.y, transform.position.z);
    return false;
  }
}