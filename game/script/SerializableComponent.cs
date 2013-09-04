using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


//! Implements serialization.
[Serializable, ExecuteInEditMode]
public abstract class SerializableComponent : MonoBehaviourEx
{
#region Attributes
	//! Bytes of serialized data.
	[SerializeField, HideInInspector]
	private byte[] m_Buffer;
#endregion

#region Operations
	//! Deserialize on load.
	public void Awake()
	{
		Deserialize();
	}
	
	//! Serialize data.
	public void Serialize()
	{
		Debug.Log("SerializableComponent: Serialize");

		IFormatter formatter = new BinaryFormatter();
		MemoryStream stream = new MemoryStream();

		var serialized_objects = DoGetSerializedObjects();
		// First save number of objects
		formatter.Serialize(stream, serialized_objects.Length);
		// Now serialize objects
		foreach(var obj in serialized_objects)
			formatter.Serialize(stream, obj);

		stream.Close();
		m_Buffer = stream.GetBuffer();
	}

	//! Deserialize data.
	public void Deserialize()
	{
		if (m_Buffer != null)
		{
			Debug.Log("SerializableComponent: Deserialize");

			IFormatter formatter = new BinaryFormatter();
			MemoryStream stream = new MemoryStream(m_Buffer);
			
			// First number of objects
			int number_ofobjects = (int)formatter.Deserialize(stream);
			// Now all objects
			var all_objects = new object[number_ofobjects];
			for(int i = 0; i < number_ofobjects; ++i)
				all_objects[i] = formatter.Deserialize(stream);
			
			// Let child process the objects
			DoProcessDeserialization(all_objects);

			stream.Close();
		}
	}
	//! Process deserialized objects in derived class.
	protected abstract void DoProcessDeserialization(object[] SerializedObjects);
	//! Get all objects to serialize.
	/*!
		Implement in derived class and returns objects that are needed for
		serialization.
	*/
	protected abstract object[] DoGetSerializedObjects();

#endregion
}
