using System;
using System.Linq;
using UnityEditor;

namespace Kogane.Internal
{
	/// <summary>
	/// UIAgingButton の Inspector を拡張するクラス
	/// </summary>
	[CustomEditor( typeof( UIAgingButton ) )]
	internal sealed class UIAgingButtonInspector : Editor
	{
		//================================================================================
		// 定数(static readonly)
		//================================================================================
		private static readonly string[] EMPTY_TAGS = new string[0];
		
		//================================================================================
		// 変数
		//================================================================================
		private SerializedProperty m_tagProperty;
		private SerializedProperty m_graphicProperty;
		private SerializedProperty m_isOnceProperty;
		private SerializedProperty m_randomHeightProperty;
		private SerializedProperty m_delayFrameProperty;
		
		//================================================================================
		// 関数
		//================================================================================
		/// <summary>
		/// 有効になった時に呼び出されます
		/// </summary>
		private void OnEnable()
		{
			m_tagProperty          = serializedObject.FindProperty( "m_tag" );
			m_graphicProperty      = serializedObject.FindProperty( "m_graphic" );
			m_isOnceProperty       = serializedObject.FindProperty( "m_isOnce" );
			m_randomHeightProperty = serializedObject.FindProperty( "m_randomWeight" );
			m_delayFrameProperty   = serializedObject.FindProperty( "m_delayFrame" );
		}

		/// <summary>
		/// Inspector を描画する時に呼び出されます
		/// </summary>
		public override void OnInspectorGUI()
		{
			var tags          = UIAgingButton.Tags ?? EMPTY_TAGS;
			var selectedIndex = Array.IndexOf( tags, m_tagProperty.stringValue ) + 1;
			var tagsWithNone  = tags.Prepend( UIAgingButton.UNTAGGED ).ToArray();
			var index         = EditorGUILayout.Popup( m_tagProperty.displayName, selectedIndex, tagsWithNone ) - 1;

			m_tagProperty.stringValue = tags.ElementAtOrDefault( index );

			EditorGUILayout.PropertyField( m_graphicProperty );
			EditorGUILayout.PropertyField( m_isOnceProperty );
			EditorGUILayout.PropertyField( m_randomHeightProperty );
			EditorGUILayout.PropertyField( m_delayFrameProperty );

			serializedObject.ApplyModifiedProperties();
		}
	}
}