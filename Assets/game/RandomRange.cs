using System;
using UnityEngine;

interface RandomRange<T> {
  T GetRangeValue();
}

[Serializable]
public class RandomIntRange : RandomRange<int> {
    public int min;
    public int max;

    public int GetRangeValue() {
        return UnityEngine.Random.Range(min, max);
    }
}

[Serializable]
public class RandomFloatRange : RandomRange<float> {
    public float min;
    public float max;

    public float GetRangeValue() {
        return UnityEngine.Random.Range(min, max);
    }
}