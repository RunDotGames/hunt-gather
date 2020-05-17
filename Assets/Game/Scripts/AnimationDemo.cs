using UnityEngine;


public class AnimationDemo: MonoBehaviour {
  public Villager villager;

  public VillagerConfig config;
  
  public void Start(){
    GameObject.FindObjectOfType<SeasonController>().Init();
    villager.Init(config, VillagerType.Builder);
  }

  public void Update(){
    if(Input.GetKeyUp(KeyCode.Alpha1)){
      villager.HandleEntityEvent(EntityEventType.Walk);
    }

    if(Input.GetKeyUp(KeyCode.Alpha2)){
      villager.HandleEntityEvent(EntityEventType.Idle);
    }

    if(Input.GetKeyUp(KeyCode.Alpha3)){
      villager.HandleEntityEvent(EntityEventType.Rest);
    }

    if(Input.GetKeyUp(KeyCode.Alpha4)){
      villager.HandleEntityEvent(EntityEventType.Die);
    }

    if(Input.GetKeyUp(KeyCode.Alpha5)){
      villager.HandleEntityEvent(EntityEventType.Attack);
    }

    if(Input.GetKeyUp(KeyCode.Alpha6)){
      villager.HandleEntityEvent(EntityEventType.Construct);
    }

    if(Input.GetKeyUp(KeyCode.Alpha7)){
      villager.HandleEntityEvent(EntityEventType.Gather);
    }

    if(Input.GetKeyUp(KeyCode.Z)){
      villager.SetVillagerType(VillagerType.Builder);
    }

    if(Input.GetKeyUp(KeyCode.X)){
      villager.SetVillagerType(VillagerType.Gatherer);
    }

    if(Input.GetKeyUp(KeyCode.C)){
      villager.SetVillagerType(VillagerType.Hunter);
    }



  }

}