using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

[Serializable]
public class ForestConfig {
    public GameObject treePrefab;
    public float deadZoneSize;
    public float treeMinDistance;
    public RandomIntRange treeRange;
    public int maxRetries;
    public float xMargin;
    public float yMargin;
    public float startingFruitPercent;
    public int fruitFood;
}

[Serializable]
public class TreeConfig {
    public RandomFloatRange fruitRange;
}

public class ForestController : MonoBehaviour {

  private List<Tree> forest = new List<Tree>();
  private List<Tree> nonFruitTrees = new List<Tree>();
  private List<Tree> fruitTrees = new List<Tree>();
  private List<Tree> targetedTrees = new List<Tree>();
  public ForestConfig config;
  public TreeConfig treeConfig;

  public void SpawnForest(Vector3 deadZone){
        var targetCount = config.treeRange.GetRangeValue();
        var hutPosition = new Vector2(deadZone.x, deadZone.y);
        var retries = 0;
        while(forest.Count < targetCount){
            if(retries > config.maxRetries){
                Debug.LogError("failed to place tree after max retries, forest max pop not reached");
                break;
            }
            var x = UnityEngine.Random.Range(config.xMargin, Screen.width - config.xMargin);
            var y = UnityEngine.Random.Range(config.yMargin, Screen.height - config.yMargin);
            var position = (Vector2)Camera.main.ScreenToWorldPoint(new Vector2(x, y));
            
            
            if((position - hutPosition).magnitude < config.deadZoneSize) {
                retries++;
                continue;
            }
            var tooClose = forest.Find((aTree) => {
                var treePosition = new Vector2(aTree.transform.position.x, aTree.transform.position.y);
                return (position - treePosition).magnitude < config.treeMinDistance;
            });
            if(tooClose != null) {
                retries++;
                continue;
            }
            retries = 0;
            var tree = GameObject.Instantiate(config.treePrefab).GetComponent<Tree>();
            tree.transform.position = position;
            forest.Add(tree);
            nonFruitTrees.Add(tree);
        }

        var fruitTotal = Mathf.RoundToInt(nonFruitTrees.Count * Mathf.Min(config.startingFruitPercent, 1.0f));
        Debug.Log("forest init with " + fruitTotal + " trees of " + forest.Count + " fruiting to start");
        while(fruitTrees.Count < fruitTotal){
            var index = UnityEngine.Random.Range(0, nonFruitTrees.Count);
            var tree = nonFruitTrees[index];
            nonFruitTrees.Remove(tree);
            tree.Init(treeConfig, true, HandleTreeFruited);
        }
        for(int i = 0; i < nonFruitTrees.Count; i++){
            nonFruitTrees[i].Init(treeConfig, false, HandleTreeFruited);
        }
    }

    public Tree GetNearestFruitTreeTarget(Vector2 from){
        var nearest = DistanceUtility.GetNearest(from, fruitTrees);
        if(nearest == null){
            return null;
        }
        fruitTrees.Remove(nearest);
        targetedTrees.Add(nearest);
        return nearest;
    }

    public Tree GetNearestTree(Vector2 from){
        return DistanceUtility.GetNearest(from, forest);
    }

    public void ReleaseTarget(Tree targeted){
        var targetIndex = targetedTrees.IndexOf(targeted);
        if(targetIndex > -1){
            targetedTrees.RemoveAt(targetIndex);
            fruitTrees.Add(targeted);
        }
    }

    public int DefruitTree(Tree defruited) {
        var targetIndex = targetedTrees.IndexOf(defruited);
        if(targetIndex > -1){
            targetedTrees.RemoveAt(targetIndex);
        }
        var fruitIndex = fruitTrees.IndexOf(defruited);
        if(fruitIndex > -1){
            fruitTrees.RemoveAt(fruitIndex);
        }
        nonFruitTrees.Add(defruited);
        defruited.Defruit();
        return config.fruitFood;
    }

    private void HandleTreeFruited(Tree tree){
        fruitTrees.Add(tree);
        nonFruitTrees.Remove(tree);
    }
}