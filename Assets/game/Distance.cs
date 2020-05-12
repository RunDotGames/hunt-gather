using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

public class DistanceUtility{
  public static T GetNearest<T>(Vector2 from, List<T> list) where T : MonoBehaviour {
    var nearest = list.Aggregate(null as Tuple<T, float>, (result, item) => {
        var itemPosition = new Vector2(item.transform.position.x, item.transform.position.y);
        var itemDistance = (from-itemPosition).magnitude;
        if(result == null){
            return new Tuple<T, float>(item, itemDistance);
        }
        if(itemDistance < result.Item2){
            return new Tuple<T, float>(item, itemDistance);
        }
        return result;
    });
    if (nearest == null){
        return null;
    }
    return nearest.Item1;
  }
}