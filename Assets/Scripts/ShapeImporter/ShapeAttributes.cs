using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeAttributes : MonoBehaviour {

    [SerializeField]
    private StringStringDictionary attributesStore = StringStringDictionary.New<StringStringDictionary>();
    public Dictionary<string, string> attributes
    {
        get { return attributesStore.dictionary; }
        set { attributesStore.dictionary = value; }
    }
}
