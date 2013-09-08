#define JA2_LOG_SERIALIZATION
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.IO;
using UnityEngine;

using StringList_t = System.Collections.Generic.List<string>;
using MappingType_t = System.Collections.Generic.Dictionary<System.Type, string[]>;

//! Main serialization class for level.
/*!
	This class is responsible for serialization of all the class that are marked
	by [Serializable] attribute. It save all the MonoBehaviors that needs to
	be serialized and also save the fields which each object want to serialize.
	These fields are saved in map, where for each object type field list is
	saved. This is only needed in edit mode for saving the data. When the data
	are saved, all fileds name are saved also so we know which fields to
	deserialize. The object references that need to be serialized are serialized
	by unity, which simplifies it a bit (no need to handle it explicitly).
*/
[HideInInspector]
[ExecuteInEditMode]
public class SerializationManager : MonoBehaviour
{
#region  Attributes
	//! Mapping for data type to fields serialize.
	private MappingType_t m_TypeMap;
	//! All objects that need to be serialized.
	[SerializeField]
	[HideInInspector]
	private MonoBehaviour[] m_SerializedObjects;
#endregion

#region Serialized Data
	//! Buffer for Type map.
	[SerializeField]
	[HideInInspector]
	private byte[] m_TypeMapBuffer;
	//! Buffer for holding serialized data.
	[SerializeField]
	[HideInInspector]
	private byte[] m_Buffer;
#endregion

#region Public Methods
	//! Reload.
	public void Reload()
	{
		Debug.Log("--- Init fields ---");

		m_TypeMap = new MappingType_t();

		// MonoBehavior objects that need to be serialized
		var object_to_serialize_set = new HashSet<MonoBehaviour>();

		// Traverse all all objects and find if they need to be serialized
		MonoBehaviour[] all_obejcts = (MonoBehaviour[])GameObject.FindObjectsOfType(typeof(MonoBehaviour));
		foreach (var obj in all_obejcts)
		{
			Type object_type = obj.GetType();
			// Find if we've got [Serializable] for all class
			if(FindCustomAttribute(System.Attribute.GetCustomAttributes(object_type), typeof(System.SerializableAttribute)))
			{
				// Get all fields
				FieldInfo[] fields = object_type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				StringList_t fields_to_serialize = new StringList_t();
				for (int i = 0; i < fields.Length; ++i)
				{
					// Only if not mark with [SerializeField], because these are
					// serialized by unity
					if(!FindCustomAttribute(System.Attribute.GetCustomAttributes(fields[i]), typeof(UnityEngine.SerializeField)))
						fields_to_serialize.Add(fields[i].Name);
				}
				// Save field to map for given type
				m_TypeMap[object_type] = fields_to_serialize.ToArray();
				// Save object for serialization
				object_to_serialize_set.Add(obj);
			}
		}
		m_SerializedObjects = new MonoBehaviour[object_to_serialize_set.Count];
		object_to_serialize_set.CopyTo(m_SerializedObjects);
		// Need to serialize type map to not to reload it on every player to
		// editor change. It is needed only for "saving serialization".
		SerializeTypeMap();
	}

	//! Try to serialize all game objects.
	public void Serialize()
	{
#if JA2_LOG_SERIALIZATION
		Debug.Log("---SERIALIZE---");
#endif
		// Find if we need to deserialize previous type map.
		// This could happen when we're in editor and get back from player to
		// editor. Type map is then set to null, because it isn't deserialized
		// in Awake() (because it isn't needed there, no need to lower
		// performance) so we can deserialize it. If reload was in progress,
		// then it is already loaded
		DeserializeTypeMapOptional();
		
		IFormatter formatter = new BinaryFormatter();
		MemoryStream stream = new MemoryStream();
		// Traverse each object need to be serialized
		foreach (var obj in m_SerializedObjects)
		{
			Type actual_type = obj.GetType();
			// Find fields to serialize
			var serialize_fields = m_TypeMap[actual_type];

			formatter.Serialize(stream, serialize_fields.Length);
			// Now serialize fields
			foreach (var field in serialize_fields)
			{
				// Store the field name as first
				formatter.Serialize(stream, field);
				formatter.Serialize(stream, GetFieldInfo(actual_type, field).GetValue(obj));
			}
		}
		stream.Close();

		m_Buffer = stream.GetBuffer();
	}

	public void Deserialize()
	{
#if JA2_LOG_SERIALIZATION
		Debug.Log("---DESERIALIZE---");
#endif
		if (m_Buffer != null && m_Buffer.Length > 0)
		{
			IFormatter formatter = new BinaryFormatter();
			MemoryStream stream = new MemoryStream(m_Buffer);

			// Traverse all objects to deserialize
			foreach (var obj in m_SerializedObjects)
			{
				Type actual_type = obj.GetType();

				// If obj has been destroyed,
				if (obj == null)
					Debug.Log("Serialization: Object has been destroyed, cannot deserialize.");
				// First number of objects
				int number_ofobjects = (int)formatter.Deserialize(stream);
				// Now all fields
				for (int i = 0; i < number_ofobjects; ++i)
				{
					// Get field name
					string field = (string)formatter.Deserialize(stream);
					// Actual object
					GetFieldInfo(actual_type, field).SetValue(obj, formatter.Deserialize(stream));
				}
			}
			stream.Close();
			// Let child hanlde deserialization if any
//			DoProcessDeserialization();
		}
	}
#endregion

#region Private Methods
	//! Find custom attribute
	private bool FindCustomAttribute(Attribute[] Attributes, Type AttributeType)
	{
		foreach (var attr in Attributes)
		{
			// Got attribute
			if (attr.GetType() == AttributeType)
				return true;
		}

		return false;
	}
	//! Get field info.
	private FieldInfo GetFieldInfo(Type Type_, string Name)
	{
		return Type_.GetField(Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
	}

	//! Serialize type map.
	private void SerializeTypeMap()
	{
		IFormatter formatter = new BinaryFormatter();
		var type_map_stream = new MemoryStream();

		// As first save type map to be able to know which fields to deserialize
		formatter.Serialize(type_map_stream, m_TypeMap);
		m_TypeMapBuffer = type_map_stream.GetBuffer();
		
		type_map_stream.Close();
	}

	//! Deserialize type map.
	private void DeserializeTypeMap()
	{
		IFormatter formatter = new BinaryFormatter();
		MemoryStream type_map_stream = new MemoryStream(m_TypeMapBuffer);

		m_TypeMap = (MappingType_t)formatter.Deserialize(type_map_stream);
		
		type_map_stream.Close();
	}

	//! Remove deleted object from serialized list.
	/*!
		\return true if at least deleted object was found and removed.
	*/
	private bool PurgeDeleted()
	{
		bool was_found = false;

		var new_go = new List<MonoBehaviour>();

		// Find if some objects were removed
		foreach (var go in m_SerializedObjects)
		{
			// At least one is out, need to remove it
			if (go == null)
				was_found = true;
			else
				new_go.Add(go);
		}
		// Need to make new array of objects
		if (was_found)
			m_SerializedObjects = new_go.ToArray();

		return was_found;
	}

	//! Desrialize type map if empty
	private void DeserializeTypeMapOptional()
	{
		// If something change
		if (PurgeDeleted())
			Reload();
		
		if(m_TypeMap == null)
		   DeserializeTypeMap();
	}
#endregion


#region Messages
	void Awake()
	{
		Deserialize();
	}
#endregion
}
