using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public class GamePrefabs {
    public GameObject ui;
}

public class GameController : MonoBehaviour {
    
    public GamePrefabs prefabs;
    

    void Start() {
        GameObject.Instantiate(prefabs.ui);

        var villageController = GameObject.FindObjectOfType<VillageController>();
        villageController.SpawnVillage();

        var forestController = GameObject.FindObjectOfType<ForestController>();
        forestController.SpawnForest(villageController.villageCenter.position);

    }

    

}