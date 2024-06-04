using UnityEngine;
using System.Collections;
using UnityEditor;

namespace SwipeEffect
{
	[CustomEditor(typeof(SwipeEffect))]
	[CanEditMultipleObjects]
	[System.Serializable]
	public class ImageLoader : Editor {
		
		private	SwipeEffect 			swipeEffectScript;
		private	SerializedProperty		spimagesFolderPath;
		private SerializedProperty		spimagesWebLoadSettings;
		private SerializedProperty		spurls;
		private SerializedObject 		serObj;
		private bool 					loadSettings = true;

		private static readonly string[] _dontIncludeMe = new string[]{"m_Script"};

		void OnEnable() {
			// Setup serialized property
			swipeEffectScript = (SwipeEffect)target;
		}
		

		public override void OnInspectorGUI()
		{
			// Folders array
			serObj = new SerializedObject (target);


			loadSettings = EditorGUILayout.Foldout(loadSettings, "Required Image Download Settings");
			if(loadSettings) {
				EditorGUILayout.HelpBox("1. Set \"Images Folder Path\". \n" +
				                        "2. Click button \"Load Images from Folder\".\n" +
				                        "or\n1'. Set some direct web urls in the \"Urls\" array.\n" +
				                        "2'. Set \"Folder For Web Images\".\n" +
				                        "3'. Click button \"Download Images from URL\".\n", MessageType.Info);
				EditorGUILayout.BeginHorizontal("Box");
				if (GUILayout.Button("Load Images from Folder")) // loading from folders
				{
					string [] folders = new string[swipeEffectScript.imagesFolderPath.Length];
					for(int idx=0; idx < swipeEffectScript.imagesFolderPath.Length; ++idx)
						folders[idx] = ("Assets/" + swipeEffectScript.imagesFolderPath[idx]);
					
					string[] guids = AssetDatabase.FindAssets ("t:Texture" ,folders);
					Texture[] imageContent = new Texture[guids.Length];
					for(int idx=0; idx < guids.Length; ++idx)
						imageContent[idx] = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[idx]), typeof(Texture)) as Texture;
					// sending created array to main script
					swipeEffectScript.LoadImageContent(imageContent);
				}
				
				if (GUILayout.Button("Download Images from URL")) // loading from folders
				{
					swipeEffectScript.LoadWithWebClient();
				}
				/*if (GUILayout.Button("Load prefabs from Folder")) // loading from folders
			{
			}*/
				
				EditorGUILayout.EndHorizontal();


				EditorGUI.indentLevel++;
				spimagesFolderPath = serObj.FindProperty("imagesFolderPath");
				Show(spimagesFolderPath);
				//EditorGUILayout.PropertyField(spimagesFolderPath, true);
		
				spurls = serObj.FindProperty("imageDownloadSettings.urls");
				Show(spurls);
				//EditorGUILayout.PropertyField(spurls, true);

				spimagesWebLoadSettings = serObj.FindProperty("imageDownloadSettings.folderForWebImages");
				EditorGUILayout.PropertyField(spimagesWebLoadSettings);
				EditorGUI.indentLevel--;
			}

			if (GUI.changed)
				serObj.ApplyModifiedProperties();


			// This draws the default inspector for MySettingsClass
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, _dontIncludeMe);
			serializedObject.ApplyModifiedProperties();
		}

		/** */
		public static void Show (SerializedProperty list) {
			//EditorGUILayout.PropertyField(list);

			list.isExpanded = EditorGUILayout.Foldout(list.isExpanded, list.displayName);
			EditorGUI.indentLevel += 1;
			if (list.isExpanded) {
				EditorGUILayout.PropertyField(list.FindPropertyRelative("Array.size"));
				for (int i = 0; i < list.arraySize; i++) {
				EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), true);
				}
			}
			EditorGUI.indentLevel -= 1;
		}
	}
}