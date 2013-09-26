﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ja2.script
{
	//! Interface for asset database.
	/*!
		We need to use interface for asset database because we cannot use
		UnityEditor assembly in this assembly as it would be used at runtime. We
		don't want to make separate assembly only for "editor" scripts because
		we wan't to be able to serialize and use this editor classes at runtime
		but withou edit functionality.
	*/
	public interface IAssetDatabase
	{
		string GetAssetPath(UnityEngine.Object AssetObject);
		void CreateAsset(UnityEngine.Object Asset, string Path);
	}
} /*ja2.script*/