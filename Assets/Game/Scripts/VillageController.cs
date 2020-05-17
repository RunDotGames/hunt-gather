using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

[Serializable]
public class VillagerConfig {
  public GameObject prefab;
  public float speed;
  public float arrivalDistance;
  public RandomFloatRange restRange;
}

[Serializable]
public class VillageConfig {
  public VillagerAllocation[] startingAllocations;
  public GameObject prefab;
  public float hutRadius;
}

[Serializable]
public class VillagerAllocation {
  public VillagerType type;
  public int count;
}

public enum VillagerType {
  Gatherer, Hunter, Builder
}

public enum HutType {
  Storage, Housing
}

public delegate void OnValueChange<T>(T oldValue, T newValue);

public class VillageController: MonoBehaviour {
  public static readonly int villagerTypecount = Enum.GetNames(typeof(VillagerType)).Length;
  public Transform villageCenter;
  public VillageConfig villageConfig;
  public VillagerConfig villagerConfig;
  private VillageHut primaryHut;
  private List<VillageHut> huts = new List<VillageHut>();
  private List<Villager> villagers = new List<Villager>();
  private Dictionary<VillagerType, List<Villager>> villagersByType;
  private Dictionary<HutType, List<VillageHut>> hutsByType = new Dictionary<HutType, List<VillageHut>>();

  
  public event OnValueChange<Dictionary<VillagerType, int>> OnVillagerAllocationChange;
  public event OnValueChange<Dictionary<HutType, int>> OnHutAllocationChange;
  public event Action<Villager> OnVillagerSpawned;
  
  public void SpawnVillage(){
    hutsByType[HutType.Housing] = new List<VillageHut>();
    hutsByType[HutType.Storage] = new List<VillageHut>();
    primaryHut = SpawnHut(villageCenter.transform.position, HutType.Storage);
    villagersByType = new Dictionary<VillagerType, List<Villager>>();
    villageConfig.startingAllocations.Select((item)=>{
      var type = item.type;
      villagersByType[type] = new List<Villager>();
      for(var i = 0; i < item.count; i++){
        SpawnVillager(type, primaryHut);
      }
      return item.count;
    }).ToArray();
  }

  public Dictionary<HutType, int> GetCurrentHutAllocation(){
    return GetAllocation(hutsByType);
  }
  public Dictionary<VillagerType, int> GetCurrentVillagerAllocation(){
    return GetAllocation(villagersByType);
  }

  public IEnumerable<Villager> GetVillagers(){
    return villagers;
  }

  private Dictionary<K, int> GetAllocation<K, V>(Dictionary<K, List<V>> map){
    return map?.Keys.Aggregate(new Dictionary<K, int>(), (result, item) => {
      result[item] = map[item].Count;
      return result;
    }) ?? new Dictionary<K, int>();
  }

  public VillageHut GetNearestHut(Vector2 from){
    return DistanceUtility.GetNearest(from, huts);
  }
  public VillageHut GetPrimaryHut(){
    return primaryHut;
  }

  public VillageHut SpawnHut(Vector2 position, HutType type){
    var priorAllocation = GetCurrentHutAllocation();
    var hut = GameObject.Instantiate(villageConfig.prefab).GetComponent<VillageHut>();
    hut.transform.position = position;
    huts.Add(hut);
    hut.SetIsStorage(type == HutType.Storage);
    hutsByType[type].Add(hut);
    OnHutAllocationChange?.Invoke(priorAllocation, GetCurrentHutAllocation());
    return hut;
  }

  public void SpawnVillager(){
    SpawnVillager(VillagerType.Gatherer, primaryHut);
  }

  private void SpawnVillager(VillagerType type, VillageHut hut){
    var priorAllocation = GetCurrentVillagerAllocation();
    var villager = GameObject.Instantiate(villagerConfig.prefab).GetComponent<Villager>();
    villager.Init(villagerConfig, type);
    villagers.Add(villager);
    villager.transform.position = (UnityEngine.Random.insideUnitCircle.normalized * villageConfig.hutRadius) + (Vector2)hut.transform.position;
    villager.OnDeath += HandleVillagerDeath;
    villagersByType[type].Add(villager);
    OnVillagerAllocationChange?.Invoke(priorAllocation, GetCurrentVillagerAllocation());
    OnVillagerSpawned?.Invoke(villager);
  }

  private void HandleVillagerDeath(CombatTarget target){
    var villager = (Villager)target;
    var priorAllocation = GetCurrentVillagerAllocation();
    villager.OnDeath -= HandleVillagerDeath;
    villagers.Remove(villager);
    var type = villager.GetVillagerType();
    villagersByType[type].Remove(villager);
    OnVillagerAllocationChange?.Invoke(priorAllocation, GetCurrentVillagerAllocation());
  }

  public Villager getNearestVillager(Vector2 from){
    return DistanceUtility.GetNearest(from, villagers);
  }


  private int CompareVillager(Villager a, Villager b){
    return a.GetAvailability() - b.GetAvailability();
  }

  private int CompareHut(VillageHut a, VillageHut b){
    return 0;
  }

  private void RassignHut(VillageHut hut, HutType type){
    hut.SetIsStorage(type == HutType.Storage);
  }

  private void ReassignVillager(Villager villager, VillagerType type) {
    villager.SetVillagerType(type);
  }

  public void ReAllocateVillagers(Dictionary<VillagerType, int> newAllocation){
    var oldAllocation = GetCurrentVillagerAllocation();
    Reallocate(oldAllocation, newAllocation, villagersByType, CompareVillager, ReassignVillager);
    OnVillagerAllocationChange?.Invoke(oldAllocation, GetCurrentVillagerAllocation());
  }

  public void ReAllocateHuts(Dictionary<HutType, int> newAllocation){
    var oldAllocation = GetCurrentHutAllocation();
    var storage = newAllocation.ContainsKey(HutType.Storage) ? newAllocation[HutType.Storage] : 0;
    if(storage > 0){
      Reallocate(oldAllocation, newAllocation, hutsByType, CompareHut, RassignHut);
    }
    OnHutAllocationChange?.Invoke(oldAllocation, GetCurrentHutAllocation());
  }

  private static void Reallocate<T, V>(
    Dictionary<T, int> oldAllocation,
    Dictionary<T, int> newAllocation,
    Dictionary<T, List<V>> basis,
    Func<V, V, int> comparer,
    Action<V, T> onReassign
  ){
    var released = newAllocation.Keys.Aggregate(new List<V>(), (result, item) =>{
      var dif = oldAllocation[item] - newAllocation[item];
      if(dif <= 0){
        return result;
      }
      basis[item].Sort((V first, V second) => comparer(first, second));
      for(int i = 0; i < dif; i++){
        var basisItem = basis[item][0];
        basis[item].RemoveAt(0);
        result.Add(basisItem);
      }
      return result;
    });
    var unaccounted = newAllocation.Keys.Aggregate(released, (result, item) =>{
      var dif = newAllocation[item] - oldAllocation[item];
      if(dif <= 0){
        return result;
      }
      if(result.Count == 0) {
        Debug.LogError("reallocation error state, no released items for re-assignment");
        return result;
      }
      for(int i = 0; i < dif; i++){
        var basisItem = result[0];
        basis[item].Add(basisItem);
        onReassign(basisItem, item);
        result.RemoveAt(0);
      }
      return result;
    });
    if(unaccounted.Count > 0){
      Debug.LogError("reallocation error state, released villagers not reassigned");
    }
  }
}