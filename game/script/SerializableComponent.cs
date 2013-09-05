#define JA2_LOG_SERIALIZATION
using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine;


//! Implements serialization.
[Serializable, ExecuteInEditMode]
public abstract class SerializableComponent : MonoBehaviourEx
{
#region Attributes
	//! Fields that need to be serialized.
	[SerializeField, HideInInspector]
	private string[] m_FieldToSerialize;
	//! Bytes of serialized data.
	[SerializeField, HideInInspector]
	private byte[] m_Buffer;
#endregion

#region Operations
	//! Deserialize on load.
	public void Awake()
	{
		// Retrieve all fields that need to be serialized.
		LoadFieldsToSerialize();

		Deserialize();
	}
	
	//! Serialize data.
	public void Serialize()
	{
#if JA2_LOG_SERIALIZATION
		Debug.Log("---SERIALIZE---");
#endif

		IFormatter formatter = new BinaryFormatter();
		MemoryStream stream = new MemoryStream();

		// Be sure we've processed all fields that need to be serialized.
		// We're checking it also here because it could be null/empty because
		// Awake() was not yet called.
		LoadFieldsToSerialize();

		formatter.Serialize(stream, m_FieldToSerialize.Length);
		
		// Now serialize objects
		Type actual_type = this.GetType();
		foreach (var field in m_FieldToSerialize)
		{
			// Store the field name as first
			formatter.Serialize(stream, field);
			formatter.Serialize(stream, GetFieldInfo(actual_type, field).GetValue(this));
		}

		stream.Close();
		m_Buffer = stream.GetBuffer();
	}

	//! Deserialize data.
	public void Deserialize()
	{
#if JA2_LOG_SERIALIZATION
		Debug.Log("---DESERIALIZE---");
#endif
		if (m_Buffer != null)
		{
			IFormatter formatter = new BinaryFormatter();
			MemoryStream stream = new MemoryStream(m_Buffer);
			
			// First number of objects
			int number_ofobjects = (int)formatter.Deserialize(stream);
			// Now all objects
			Type actual_type = this.GetType();
			for (int i = 0; i < number_ofobjects; ++i)
			{
				// Get field name
				string field = (string)formatter.Deserialize(stream);
				// Actual object
				GetFieldInfo(actual_type, field).SetValue(this, formatter.Deserialize(stream));
			}
			stream.Close();

			// Let child hanlde deserialization if any
			DoProcessDeserialization();
		}
	}

	//! Get field info.
	private FieldInfo GetFieldInfo(Type Type_, string Name)
	{
		return Type_.GetField(Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
	}

	//! Get fields to serialize.
	private void LoadFieldsToSerialize()
	{
		// If not initialized field, do it now for once
		if (m_FieldToSerialize == null || m_FieldToSerialize.Length == 0)
		{
			Debug.Log("--- Init fields ---");

			Type actual_type = this.GetType();
			Attribute[] attributes = System.Attribute.GetCustomAttributes(actual_type);
			foreach (var attr in attributes)
			{
				// Got [Serializable]
				if (attr.GetType() == typeof(System.SerializableAttribute))
				{
					// Get all fields
					FieldInfo[] fields = actual_type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					m_FieldToSerialize = new string[fields.Length];
					for (int i = 0; i < fields.Length; ++i)
						m_FieldToSerialize[i] = fields[i].Name;

					break;
				}
			}
		}
	}
	//! Process deserialized objects in derived class.
	protected virtual void DoProcessDeserialization()
	{}
#endregion
}
