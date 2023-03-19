using System;
using System.Collections.Generic;
using System.Linq;
using Kogane.Internal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

#pragma warning disable 0414

namespace Kogane
{
    /// <summary>
    /// エージングテスト時に自動でボタンをクリックするためのコンポーネント
    /// </summary>
    public sealed class UIAgingButton : MonoBehaviour
    {
        //================================================================================
        // 定数
        //================================================================================
        /// <summary>
        /// 無効なタグの名前
        /// </summary>
        public const string UNTAGGED = "Untagged";

        //================================================================================
        // 変数(SerializeField)
        //================================================================================
        [SerializeField][Popup]           private string     m_tag = string.Empty; // タグ
        [SerializeField]                  private Graphic    m_graphic;            // ゲームオブジェクトにアタッチされている Graphic
        [SerializeField]                  private Collider2D m_collider2D;         // ゲームオブジェクトにアタッチされている Collider2D
        [SerializeField]                  private bool       m_isOnce;             // 1度だけクリックする場合 true
        [SerializeField][Range( 0, 100 )] private float      m_randomWeight;       // クリックするかどうかの乱数の重み（ 0 から 100、0 か 100 の場合は必ずクリック ）
        [SerializeField]                  private int        m_delayFrame;         // ゲームオブジェクトがアクティブになってからボタンを押すまでの遅延フレーム数

        //================================================================================
        // 変数(static)
        //================================================================================
        private static Dictionary<string, bool> m_isEnableTableCache;

        //================================================================================
        // プロパティ(static)
        //================================================================================
        /// <summary>
        /// エージングテストが有効かどうかを取得または設定します
        /// </summary>
        private static Dictionary<string, bool> IsEnableTable => m_isEnableTableCache ??= new Dictionary<string, bool>();

        /// <summary>
        /// エージングテストで使用できるタグを取得または設定します
        /// </summary>
        public static string[] Tags { get; set; }

        /// <summary>
        /// クリックできるかどうか確認する時に呼び出されるデリゲートを設定します
        /// </summary>
        public static Func<bool> CanClick { private get; set; }

        /// <summary>
        /// クリックされる時に呼び出されるデリゲートを設定します
        /// </summary>
        public static Action<IEventSystemHandler, PointerEventData> OnClick { private get; set; }

        /// <summary>
        /// クリックする時に対象のゲームオブジェクトを無視するかどうかを確認する時に呼び出されるデリゲートを設定します
        /// </summary>
        public static Func<GameObject, bool> IsClick { private get; set; }

#if !KOGANE_DISABLE_UI_AGING_BUTTON

        //================================================================================
        // 変数
        //================================================================================
        private bool  m_isClicked;  // 既に1度クリックされた場合 true
        private float m_frameCount; // ゲームオブジェクトがアクティブになってから経過したフレーム数

        //================================================================================
        // 関数
        //================================================================================
        /// <summary>
        /// アタッチした時に呼び出されます
        /// </summary>
        private void Reset()
        {
            m_graphic    = GetComponent<Graphic>();
            m_collider2D = GetComponent<Collider2D>();
        }

        /// <summary>
        /// 更新される時に呼び出されます
        /// </summary>
        private void Update()
        {
            if ( !GetEnable( m_tag ) ) return;

            m_frameCount++;

            // クリックできない場合は何もしません
            if ( m_isOnce && m_isClicked ) return;
            if ( m_randomWeight != 0 && m_randomWeight < Random.Range( 0, 101 ) ) return;
            if ( m_delayFrame != 0 && m_frameCount - 1 <= m_delayFrame ) return;
            if ( CanClick != null && !CanClick() ) return;
            if ( !EventSystemUtils.CanClick( m_graphic, IsClick ) && !EventSystemUtils.CanClick( m_collider2D, IsClick ) ) return;

            // GC Alloc 対策のため処理をローカル関数に格納しています
            void Impl()
            {
                // クリックできる場合はクリックします
                var eventData = new PointerEventData( EventSystem.current );

                ExecuteEvents.Execute<IEventSystemHandler>
                (
                    target: gameObject,
                    eventData: eventData,
                    functor: ( eventSystemHandler, data ) =>
                    {
                        if ( OnClick != null )
                        {
                            OnClick.Invoke( eventSystemHandler, eventData );
                        }
                        else
                        {
                            ( eventSystemHandler as IPointerClickHandler )?.OnPointerClick( eventData );
                        }

                        m_isClicked = true;
                    }
                );
            }

            Impl();
        }

        //================================================================================
        // 関数(static)
        //================================================================================
        /// <summary>
        /// 指定されたタグに紐づくエージングテストが有効かどうかを返します
        /// </summary>
        public static bool GetEnable( string tag )
        {
            if ( tag is null or UNTAGGED )
            {
                tag = string.Empty;
            }

            return IsEnableTable.ContainsKey( tag ) && IsEnableTable[ tag ];
        }

        /// <summary>
        /// 指定されたタグに紐づくエージングテストが有効かどうかを設定します
        /// </summary>
        public static void SetEnable( string tag, bool isEnable )
        {
            if ( tag is null or UNTAGGED )
            {
                tag = string.Empty;
            }

            IsEnableTable[ tag ] = isEnable;
        }

        /// <summary>
        /// ゲーム実行時に呼び出されます
        /// </summary>
        [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSceneLoad )]
        private static void RuntimeInitializeOnLoadMethod()
        {
            m_isEnableTableCache?.Clear();
            m_isEnableTableCache = null;

            CanClick = null;
            OnClick  = null;
            IsClick  = null;
        }

        //================================================================================
        // クラス(static)
        //================================================================================
        /// <summary>
        /// EventSystem 関連の汎用的な機能を管理するクラス
        /// </summary>
        private static class EventSystemUtils
        {
            //================================================================================
            // 変数(static)
            //================================================================================
            private static List<RaycastResult> m_raycastResults;

            //================================================================================
            // 関数(static)
            //================================================================================
            /// <summary>
            /// 指定された UI オブジェクトがクリックできる場合 true を返します
            /// </summary>
            public static bool CanClick( Graphic graphic, Func<GameObject, bool> isClick )
            {
                m_raycastResults ??= new List<RaycastResult>();

                if ( graphic == null ) return false;

                var gameObject = graphic.gameObject;

                if ( !gameObject.activeInHierarchy || !graphic || !EventSystem.current ) return false;

                var canvas        = graphic.canvas;
                var rectTransform = graphic.rectTransform;
                var position      = RectTransformUtils.RectTransformToScreenPoint( canvas, rectTransform );

                var pointerEventData = new PointerEventData( EventSystem.current )
                {
                    position = position,
                };

                m_raycastResults.Clear();

                EventSystem.current.RaycastAll( pointerEventData, m_raycastResults );

                if ( m_raycastResults.Count <= 0 )
                {
                    m_raycastResults.Clear();
                    return false;
                }

                var go = m_raycastResults.FirstOrDefault( x => isClick == null || isClick( x.gameObject ) ).gameObject;

                m_raycastResults.Clear();

                if ( go == null ) return false;

                return
                    go == gameObject ||
                    go.GetComponent<Graphic>() &&
                    go.transform.IsChildOf( gameObject.transform )
                    // go.GetComponentInParent<Selectable>().gameObject == gameObject
                    ;
            }

            /// <summary>
            /// 指定された Collider2D がクリックできる場合 true を返します
            /// </summary>
            public static bool CanClick( Collider2D collider2D, Func<GameObject, bool> isClick )
            {
                if ( collider2D == null ) return false;

                var gameObject = collider2D.gameObject;

                if ( !gameObject.activeInHierarchy || !collider2D || !EventSystem.current ) return false;

                var position       = collider2D.transform.position;
                var hitCollider2Ds = Physics2D.OverlapBoxAll( position, Vector3.one * 0.1f, 0 );

                if ( hitCollider2Ds.Length <= 0 ) return false;

                var hitCollider2D = hitCollider2Ds
                        .Where( x => x.GetComponent<UIAgingButtonIgnore>() == null )
                        .FirstOrDefault( x => isClick == null || isClick( x.gameObject ) )
                    ;

                if ( hitCollider2D == null ) return false;

                var hitGameObject = hitCollider2D.gameObject;

                if ( hitGameObject == null ) return false;

                return
                    hitGameObject == gameObject ||
                    hitGameObject.GetComponent<Collider2D>() &&
                    hitGameObject.transform.IsChildOf( gameObject.transform )
                    ;
            }
        }

        /// <summary>
        /// RectTransform 関連の汎用的な機能を管理するクラス
        /// </summary>
        private static class RectTransformUtils
        {
            //================================================================================
            // 変数(static)
            //================================================================================
            private static Vector3[] m_fourCornersArray;

            //================================================================================
            // 関数(static)
            //================================================================================
            /// <summary>
            /// 指定された RectTransform のスクリーン座標を返します
            /// </summary>
            public static Vector2 RectTransformToScreenPoint( Canvas canvas, RectTransform rectTransform )
            {
                if ( m_fourCornersArray == null )
                {
                    m_fourCornersArray = new Vector3[ 4 ];
                }

                if ( canvas.renderMode == RenderMode.ScreenSpaceOverlay )
                {
                    var position = rectTransform.position;
                    var pivot    = rectTransform.pivot;
                    var size     = Vector2.Scale( rectTransform.rect.size, rectTransform.lossyScale );
                    var rect     = new Rect( position.x, position.y, size.x, size.y );

                    rect.x -= rectTransform.pivot.x * size.x;
                    rect.y =  rect.y - ( 1f - pivot.y ) * size.y + rect.height;

                    return rect.center;
                }

                var worldCamera = canvas.worldCamera;
                var camera      = worldCamera != null ? worldCamera : Camera.main;

                if ( !camera ) return Vector2.zero;

                rectTransform.GetWorldCorners( m_fourCornersArray );

                var worldPoint = ( m_fourCornersArray[ 0 ] + m_fourCornersArray[ 2 ] ) / 2;

                return RectTransformUtility.WorldToScreenPoint( camera, worldPoint );
            }
        }
#endif
    }
}