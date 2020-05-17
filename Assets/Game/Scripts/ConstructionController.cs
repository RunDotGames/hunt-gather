using UnityEngine;
using System;

[Serializable]
public class ConstructionConfig{
  public GameObject prefab;
  public float projectRadius;
}


public class ConstructionController: MonoBehaviour {

  public ConstructionConfig config;
  public ConstructionProjectConfig projectConfig;
  
  private ForestController forest;
  private VillageController village;
  private ConstructionProject active;
  
  public event Action<float> OnProgressChange;

  public void Init(VillageController village, ForestController forest){
    this.village = village;
    this.forest = forest;
  }

  public ConstructionProject GetActiveProject(){
    if(active != null){
      return active;
    }
    var hut = village.GetPrimaryHut();
    var starting = UnityEngine.Random.Range(0, 360);
    var rotation = 0;
    var valid = true;
    Vector2 targetLocation = Vector2.zero;
    do{
      if(rotation > 360){
        valid = false;
        break;
      }
      var targetDirection = Quaternion.AngleAxis(rotation + starting, Vector3.forward) * (Vector2.up * config.projectRadius);
      targetLocation = hut.transform.position + targetDirection;
      rotation = rotation+5;
      var viewPoint = Camera.main.WorldToViewportPoint(targetLocation);
      if (viewPoint.x < 0 || viewPoint.x > 1 || viewPoint.y < 0 || viewPoint.y > 1){
        continue;
      }
      var nearest = forest.GetNearestTree(targetLocation);
      var distance = nearest != null ?((Vector2)nearest.transform.position - targetLocation).magnitude : float.MaxValue;
      if(distance < config.projectRadius) {
        continue;
      }
      var nearestHut = village.GetNearestHut(targetLocation);
      var nearestHutDistance = nearestHut != null ?((Vector2)nearestHut.transform.position - targetLocation).magnitude : float.MaxValue;
      if(nearestHutDistance >= config.projectRadius) {
        break;
      }
    } while(true);
    if(!valid){
      return null;
    }
    active = GameObject.Instantiate(config.prefab).GetComponent<ConstructionProject>();
    active.Init(projectConfig, HandleProjectProgress);
    active.OnComplete += HandleWorkDone;
    active.transform.position = targetLocation;
    return active;
  }

  private void HandleWorkDone(ConstructionProject project){
    if(active == project){
      active = null;
      village.SpawnHut(project.transform.position, HutType.Storage);
      OnProgressChange?.Invoke(0);
    }
  }

  private void HandleProjectProgress(float value){
    OnProgressChange?.Invoke(value);
  }

  
}