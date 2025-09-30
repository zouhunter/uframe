using System.Collections;
using System.Collections.Generic;
using UFrame.Decision;
using UnityEngine;

public class DecisionTreeTest : MonoBehaviour
{
    public DecisionTree tree;
    public int gdp;
    public bool child;
    public bool wife;
    public bool pet;

    void Start()
    {
        var result = tree.Evaluate(new Dictionary<string, object> { { "GDP", gdp }, { "Child", child }, { "Wife", wife }, { "Pet", pet } });
        Debug.Log("result:" + result);
    }
}
