#if UNITY_EDITOR

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kogane.Internal
{
    /// <summary>
    /// エージングテストのタグをポップアップで選択できるようにする PropertyDrawer
    /// </summary>
    [CustomPropertyDrawer( typeof( PopupAttribute ) )]
    internal sealed class PopupDrawer : PropertyDrawer
    {
        //================================================================================
        // 変数
        //================================================================================
        private static string[] m_emptyTags;

        //================================================================================
        // 関数
        //================================================================================
        /// <summary>
        /// 描画する時に呼び出されます
        /// </summary>
        public override void OnGUI
        (
            Rect               position,
            SerializedProperty property,
            GUIContent         label
        )
        {
            m_emptyTags ??= Array.Empty<string>();

            var tags          = ( UIAgingButton.Tags ?? m_emptyTags ).Prepend( UIAgingButton.ANY ).ToArray();
            var selectedIndex = Array.IndexOf( tags, property.stringValue ) + 1;
            var tagsWithNone  = tags.Prepend( UIAgingButton.UNTAGGED ).ToArray();
            var index         = EditorGUI.Popup( position, property.displayName, selectedIndex, tagsWithNone ) - 1;

            property.stringValue = tags.ElementAtOrDefault( index );
        }
    }
}

#endif