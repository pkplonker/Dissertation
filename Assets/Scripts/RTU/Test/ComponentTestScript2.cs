using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentTestScript2 : MonoBehaviour
{
    [Serializable]
    public class NestedComplexClass
    {
        public int nestedInt;
        public GameObject nestedGameobject;
        public Transform nestedTransform;
        public List<Transform> nestedTransforms;
    }

    public NestedComplexClass nestedClassComplexField;

    [SerializeField]
    private NestedComplexClass privateNestedClassComplexField;

    [field: SerializeField]
    public NestedComplexClass serializedNestedComplexClass { get; private set; }
 
}
