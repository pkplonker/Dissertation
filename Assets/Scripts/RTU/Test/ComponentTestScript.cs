using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ComponentTestScript : MonoBehaviour
{
	public int intField;

	[SerializeField]
	private float floatField;

	[field: SerializeField]
	public double doubleField { get; private set; }

	public string stringField;

	[SerializeField]
	private string privateStringField;

	[field: SerializeField]
	public string serializedStringProperty { get; private set; }

	public Vector3 vector3Field;

	[SerializeField]
	private Quaternion quaternionField;

	[field: SerializeField]
	public Color colorProp { get; private set; }

	public int[] intArray;

	[SerializeField]
	private string[] stringArray;

	[field: SerializeField]
	public Vector3[] vector3Array { get; private set; }

	public List<int> intList;

	[SerializeField]
	private List<string> stringList;

	[field: SerializeField]
	public List<Color> colorList { get; private set; }

	public Dictionary<string, int> dictionaryField;

	[SerializeField]
	private Dictionary<int, string> privateDictionaryField;

	[field: SerializeField]
	public Dictionary<string, Vector3> serializedDictionary { get; private set; }

	[Serializable]
	public class NestedClass
	{
		public int nestedInt;
		public string nestedString;
	}

	

	public NestedClass nestedClassField;

	[SerializeField]
	private NestedClass privateNestedClassField;

	[field: SerializeField]
	public NestedClass serializedNestedClass { get; private set; }

	public enum TestEnum
	{
		ValueA,
		ValueB,
		ValueC
	}

	public TestEnum enumField;

	[SerializeField]
	private TestEnum privateEnumField;

	[field: SerializeField]
	public TestEnum serializedEnumProperty { get; private set; }

	[Serializable]
	public struct CustomStruct
	{
		public int structInt;
		public float structFloat;
	}

	public CustomStruct structField;

	[SerializeField]
	private CustomStruct privateStructField;

	[field: SerializeField]
	public CustomStruct serializedStructProperty { get; private set; }

	public List<List<int>> nestedList;

	[SerializeField]
	private Dictionary<string, List<Vector3>> complexDictionary;

	[field: SerializeField]
	public HashSet<string> hashSetProperty { get; private set; }

	public int? nullableInt;

	[SerializeField]
	private float? nullableFloat;

	[field: SerializeField]
	public bool? nullableBool { get; private set; }

	public GameObject gameObjectField;

	[SerializeField]
	private Transform transformField;

	[field: SerializeField]
	public Rigidbody rigidbdyProperty { get; private set; }

	public GameObject[] gameObjectArray;

	[SerializeField]
	private List<Transform> transformList;

	[SerializeField]
	private List<NestedClass> classList;

	[field: SerializeField]
	public List<Rigidbody> rigidbodyList { get; private set; }

	private int[] cachedIntArray = null;
	private string[] cachedStringArray = null;

	// private void Update()
	// {
	// 	if (cachedIntArray != intArray)
	// 	{
	// 		if (intArray?.Any() ?? false)
	// 		{
	// 			foreach (var i in intArray)
	// 			{
	// 				Debug.Log(i);
	// 			}
	// 		}
	// 	}
	//
	// 	if (cachedStringArray != stringArray)
	// 	{
	// 		if (stringArray?.Any() ?? false)
	// 		{
	// 			foreach (var i in stringArray)
	// 			{
	// 				Debug.Log(i);
	// 			}
	// 		}
	// 	}
	//
	// 	cachedStringArray = stringArray;
	// 	cachedIntArray = intArray;
	// }

	private void Reset()
	{
		classList = new List<NestedClass>();
		intField = 10;
		floatField = 20.5f;
		doubleField = 30.123;

		stringField = "Test String";
		privateStringField = "Private Test String";
		serializedStringProperty = "Serialized Property String";

		vector3Field = Vector3.one;
		quaternionField = Quaternion.identity;
		colorProp = Color.red;

		intArray = new[] {1, 2, 3};
		stringArray = new[] {"A", "B", "C"};
		vector3Array = new[] {Vector3.up, Vector3.down, Vector3.left};

		intList = new List<int> {1, 2, 3};
		stringList = new List<string> {"X", "Y", "Z"};
		colorList = new List<Color> {Color.green, Color.blue, Color.yellow};

		dictionaryField = new Dictionary<string, int> {{"1", 1}, {"2", 2}};
		privateDictionaryField = new Dictionary<int, string> {{1, "1"}, {2, "2"}};
		serializedDictionary = new Dictionary<string, Vector3> {{"a", Vector3.zero}, {"b", Vector3.one}};

		nestedClassField = new NestedClass {nestedInt = 10, nestedString = "Nested"};
		privateNestedClassField = new NestedClass {nestedInt = 20, nestedString = "Private Nested"};
		serializedNestedClass = new NestedClass {nestedInt = 30, nestedString = "Serialized Nested "};

		enumField = TestEnum.ValueA;
		privateEnumField = TestEnum.ValueB;
		serializedEnumProperty = TestEnum.ValueC;

		structField = new CustomStruct {structInt = 10, structFloat = 10.5f};
		privateStructField = new CustomStruct {structInt = 20, structFloat = 20.5f};
		serializedStructProperty = new CustomStruct {structInt = 30, structFloat = 30.5f};

		nestedList = new List<List<int>> {new List<int> {1, 2}, new List<int> {3, 4}};
		complexDictionary = new Dictionary<string, List<Vector3>> {{"KeyA", new List<Vector3> {Vector3.forward}}};
		hashSetProperty = new HashSet<string> {"Value1", "Value2"};

		nullableInt = 5;
		nullableFloat = 10.5f;
		nullableBool = true;

		gameObjectField = this.gameObject;
		transformField = this.transform;
		rigidbdyProperty = this.GetComponent<Rigidbody>();

		gameObjectArray = new[] {this.gameObject};
		transformList = new List<Transform> {this.transform};
		rigidbodyList = new List<Rigidbody> {this.GetComponent<Rigidbody>()};
	}
}