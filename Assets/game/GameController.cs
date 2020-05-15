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
        var foodController = GameObject.FindObjectOfType<FoodController>();
        var forestController = GameObject.FindObjectOfType<ForestController>();
        var constructionController = GameObject.FindObjectOfType<ConstructionController>();
        var predatorController = GameObject.FindObjectOfType<PredatorController>();
        
        foodController.Init(villageController);
        forestController.SpawnForest(villageController);
        constructionController.Init(villageController, forestController);
        predatorController.Init(villageController);
        villageController.SpawnVillage();

    }

}