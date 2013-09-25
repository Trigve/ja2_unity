#define JA2_LOG_SERIALIZATION
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.IO;
using UnityEngine;

using StringList_t = System.Collections.Generic.List<string>;
using StringSet_t = System.Collections.Generic.HashSet<string>;
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

	We need to store 2 list for object that are processed by
	serialization/deserialization. One for serialization, another by
	deserialization. This because when the serialization was run, all objects
	were valid. Then some objects were deleted but no serialization was done
	and assemblies has been reloaded for instance. And if to-be serialized
	obejcts were only reduced, then buffer holding the serialized data now 
	has more objects store than actual is. When deserializing we could end
	desrializing the object data to wrong object (because the one which was
	serialized was removed from list for isntance). Therefor we must store
	object instances which need to be deserialized and then check if some of
	the instance is null and o appropriate steps.
*/
[HideInInspector]
[ExecuteInEditMode]
public class SerializationManager : MonoBehaviour
{
#region Inner classes
	//! Helper class, because unity couldn't serialize array of array.
	[Serializable]
	internal sealed class BufferHolder
	{
		public byte[] m_Buffer;
	}
#endregion
#region  Attributes
	//! Mapping for data type to fields serialize.
	private MappingType_t m_TypeMap;
	//! All objects that need to be serialized.
	[SerializeField]
	[HideInInspector]
	private MonoBehaviour[] m_SerializedObjects;
	//! All objects that need to be deserialized.
	[SerializeField]
	[HideInInspector]
	private MonoBehaviour[] m_DeserializedObjects;
#endregion

#region Serialized Data
	//! Buffer for holding serialized data.
	[SerializeField]
	[HideInInspector]
	private BufferHolder[] m_Buffer;
#endregion

#region Interface
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

			// The type is [Serializable], add object
			if (FindAllFields(obj.GetType(), object_type))
				object_to_serialize_set.Add(obj);
		}
		m_SerializedObjects = new MonoBehaviour[object_to_serialize_set.Count];
		object_to_serialize_set.CopyTo(m_SerializedObjects);
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
		// Create new list of buffer
		m_Buffer = new BufferHolder[m_SerializedObjects.Length];

		IFormatter formatter = new BinaryFormatter();
		
		// Traverse each object need to be serialized
		int i = 0;
		foreach (var obj in m_SerializedObjects)
		{
			MemoryStream stream = new MemoryStream();

			SerializeObject(formatter, stream, obj, obj.GetType());
			
			stream.Close();
			// Save to solo buffer
			m_Buffer[i] = new BufferHolder();
			m_Buffer[i].m_Buffer = stream.GetBuffer();
			++i;
		}
		
		// Make the copy to deserialized list
		m_DeserializedObjects = (MonoBehaviour[])m_SerializedObjects.Clone();
	}

	public void Deserialize()
	{
#if JA2_LOG_SERIALIZATION
		Debug.Log("---DESERIALIZE---");
#endif
		if (m_Buffer != null && m_Buffer.Length > 0)
		{
			IFormatter formatter = new BinaryFormatter();

			int j = 0;
			// Traverse all objects to deserialize
			foreach (var obj in m_DeserializedObjects)
			{
				// If obj has been destroyed,
				if (obj == null)
					Debug.LogWarning("Serialization: Object has been destroyed, cannot deserialize.");
				// Get type only if valid
				else
				{
					MemoryStream stream = new MemoryStream(m_Buffer[j].m_Buffer);

					DeserializeObject(formatter, stream, obj, obj.GetType());

					stream.Close();

					// Let child handle deserialization post process
					var post_process = obj as ja2.script.ISerializePostProcessable;
					if (post_process != null)
						post_process.PostProcess();
				}
				++j;
			}
		}
	}
#endregion

#region Operations
	//! Get all fields that need to be serialized.
	/*!
		Also process all bases that are marked for serialization. RootType is
		the most derived type to which later fields will be set.
	*/
	private bool FindAllFields(Type ActualType, Type RootType)
	{
		bool is_serializble = false;
		// Type is already serialized
		if (m_TypeMap.ContainsKey(ActualType))
		{
			is_serializble = true;
		}
		else
		{
			// If the class has some base, process it as first. We don't want to
			// proces MonoBehaviour and up
			Type base_type = ActualType.BaseType;
			if (base_type != null && !base_type.Equals(typeof(MonoBehaviour)))
				FindAllFields(base_type, RootType);

			// The class is serializable and isn't serialized already
			if (FindCustomAttribute(System.Attribute.GetCustomAttributes(ActualType), typeof(System.SerializableAttribute)))
			{
				// Get all fields
				FieldInfo[] fields = ActualType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
				StringList_t fields_to_serialize = new StringList_t();

				for (int i = 0; i < fields.Length; ++i)
				{
					// Only if not mark with [SerializeField]( because these are
					// serialized by unity) or [NonSerialized] (cannot use both
					// at same time because otherwise unity wouldn't serializa
					// the field).
					if (!FindCustomAttribute(System.Attribute.GetCustomAttributes(fields[i]), typeof(NonSerializedAttribute)) &&
						!FindCustomAttribute(System.Attribute.GetCustomAttributes(fields[i]), typeof(UnityEngine.SerializeField)))
						fields_to_serialize.Add(fields[i].Name);
				}
				// Save field to map for given type
				m_TypeMap[ActualType] = fields_to_serialize.ToArray();

				is_serializble = true;
			}
		}

		return is_serializble;
	}

	//! Serialize the whole hierarchy of object.
	private void SerializeObject(IFormatter Formatter, Stream Stream_, MonoBehaviour Object, Type ObjectType)
	{
		// Process all bases
		Type base_type = ObjectType.BaseType;
		if (base_type != null && !base_type.Equals(typeof(MonoBehaviour)))
			SerializeObject(Formatter, Stream_, Object, base_type);
		
		// Write the actual type
		Formatter.Serialize(Stream_, ObjectType);

		// Here could be the base type that isn't serialized, so serialize only
		// types that shoud be
		if (m_TypeMap.ContainsKey(ObjectType))
		{
			// Find fields to serialize
			var serialize_fields = m_TypeMap[ObjectType];

			Formatter.Serialize(Stream_, serialize_fields.Length);
			// Now serialize fields
			//\FIXME need to handle when some of the fields types are not marked
			// as serializable and excpetion is thrown, therefor nothing is
			// saved
			foreach (var field in serialize_fields)
			{
				// Store the field name as first
				Formatter.Serialize(Stream_, field);
				Formatter.Serialize(Stream_, GetFieldInfo(ObjectType, field).GetValue(Object));
			}
		}
		// Write length of 0, that means that type isn't serialized
		else
		{
			Formatter.Serialize(Stream_, 0);
		}
	}

	//! Deserialize whole hierarchy of object.
	private void DeserializeObject(IFormatter Formatter, Stream Stream_, MonoBehaviour Object, Type ObjectType)
	{
		// Process all bases
		Type base_type = ObjectType.BaseType;
		if (base_type != null && !base_type.Equals(typeof(MonoBehaviour)))
			DeserializeObject(Formatter, Stream_, Object, base_type);

		// Read the type, must match
		Type actual_type = (Type)Formatter.Deserialize(Stream_);
		if (!ObjectType.Equals(actual_type))
			throw new SerializationException("Deserialize: Actual type " + ObjectType.ToString() + " isn't equal to expected " + actual_type.ToString() + " when deserializing object " + Object.ToString());
		// First number of objects
		int number_ofobjects = (int)Formatter.Deserialize(Stream_);
		// Now all fields
		for (int i = 0; i < number_ofobjects; ++i)
		{
			// Get field name
			string field = (string)Formatter.Deserialize(Stream_);
			// Deserialize object
			object deserialized_obj = Formatter.Deserialize(Stream_);
			// Set value
			GetFieldInfo(ObjectType, field).SetValue(Object, deserialized_obj);
		}
	}

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
		// Need to always reload because there could be situations that some
		// object were added to be serialized but they wasn't added on init
		// in to-be serialized list and therefor no change there and we're
		// skipping them
		Reload();
	}
#endregion


#region Messages
	void Awake()
	{
		Deserialize();
	}
#endregion
}
