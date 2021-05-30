using Facebook.Yoga;
using ReactUnity.Helpers;
using ReactUnity.Styling.Internal;
using ReactUnity.Types;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace ReactUnity.Styling
{
    public class BorderAndBackground : MonoBehaviour
    {
        public RectTransform Root { get; private set; }
        public RectTransform Border { get; private set; }
        public RectTransform Background { get; private set; }
        public RectTransform ShadowRoot { get; private set; }
        public RectTransform Shadow { get; private set; }

        public RoundedBorderMaskImage RootGraphic;
        public Mask RootMask;

        public BasicBorderImage BorderGraphic;

        public BoxShadowImage ShadowGraphic;

        public static BorderAndBackground Create(GameObject go)
        {
            var cmp = go.AddComponent<BorderAndBackground>();

            var root = new GameObject("[MaskRoot]", typeof(RectTransform), typeof(RoundedBorderMaskImage));
            var border = new GameObject("[BorderImage]", typeof(RectTransform), typeof(BasicBorderImage));
            var bg = new GameObject("[BackgroundImage]", typeof(RectTransform), typeof(Image));


            cmp.RootGraphic = root.GetComponent<RoundedBorderMaskImage>();

            cmp.RootMask = root.AddComponent<Mask>();
            cmp.RootMask.showMaskGraphic = false;

            cmp.BorderGraphic = border.GetComponent<BasicBorderImage>();

            var bgImage = bg.GetComponent<Image>();
            bgImage.type = Image.Type.Sliced;
            bgImage.pixelsPerUnitMultiplier = 1;

            var sr = new GameObject("[Shadows]", typeof(RectTransform));

            var sd = new GameObject("[Shadow]", typeof(RectTransform), typeof(BoxShadowImage));
            cmp.ShadowGraphic = sd.GetComponent<BoxShadowImage>();

            cmp.Root = root.transform as RectTransform;
            cmp.ShadowRoot = sr.transform as RectTransform;
            cmp.Shadow = sd.transform as RectTransform;
            cmp.Border = border.transform as RectTransform;
            cmp.Background = bg.transform as RectTransform;

            FullStretch(cmp.ShadowRoot, cmp.Root);
            FullStretch(cmp.Shadow, cmp.ShadowRoot);
            FullStretch(cmp.Background, cmp.Root);
            FullStretch(cmp.Border, cmp.Root);
            FullStretch(cmp.Root, cmp.transform as RectTransform);
            cmp.Root.SetAsFirstSibling();

            return cmp;
        }

        public void SetBorderSize(YogaNode layout)
        {
            var bidiLeft = layout.LayoutDirection == YogaDirection.LTR ? layout.BorderStartWidth : layout.BorderEndWidth;
            var bidiRight = layout.LayoutDirection == YogaDirection.RTL ? layout.BorderStartWidth : layout.BorderEndWidth;

            var borderLeft = GetFirstDefinedSize(bidiLeft, layout.BorderLeftWidth, layout.BorderWidth);
            var borderRight = GetFirstDefinedSize(bidiRight, layout.BorderRightWidth, layout.BorderWidth);
            var borderTop = GetFirstDefinedSize(layout.BorderTopWidth, layout.BorderWidth);
            var borderBottom = GetFirstDefinedSize(layout.BorderBottomWidth, layout.BorderWidth);


            var min = new Vector2(-borderLeft, -borderBottom);
            var max = new Vector2(borderRight, borderTop);

            Root.offsetMin = -min;
            Root.offsetMax = -max;

            Border.offsetMin = min;
            Border.offsetMax = max;

            Background.offsetMin = min;
            Background.offsetMax = max;

            ShadowRoot.offsetMin = min;
            ShadowRoot.offsetMax = max;

            BorderGraphic.enabled = borderLeft > 0 || borderRight > 0 || borderBottom > 0 || borderTop > 0;
            BorderGraphic.BorderSize = new Vector4(borderTop, borderRight, borderBottom, borderLeft);
            BorderGraphic.SetMaterialDirty();
        }

        public void SetBorderRadius(float tl, float tr, float br, float bl)
        {
            var v = new Vector4(tl, tr, br, bl);

            RootGraphic.BorderRadius = v;
            RootGraphic.SetMaterialDirty();
            MaskUtilities.NotifyStencilStateChanged(RootMask);

            BorderGraphic.BorderRadius = v;
            BorderGraphic.SetMaterialDirty();

            ShadowGraphic.BorderRadius = v;
            ShadowGraphic.SetMaterialDirty();
        }

        public void SetBorderColor(Color top, Color right, Color bottom, Color left)
        {
            BorderGraphic.TopColor = top;
            BorderGraphic.RightColor = right;
            BorderGraphic.BottomColor = bottom;
            BorderGraphic.LeftColor = left;
            BorderGraphic.SetMaterialDirty();
        }

        public void SetBackgroundColorAndImage(Color? color, Sprite sprite)
        {
            var bg = Background.GetComponent<Image>();
            bg.color = color.HasValue ? color.Value : (sprite ? Color.white : Color.clear);
            bg.sprite = sprite;
        }

        public void SetBoxShadow(BoxShadow shadow)
        {
            Shadow.gameObject.SetActive(shadow != null);

            if (shadow == null) return;

            ShadowGraphic.Shadow = shadow;

            Shadow.sizeDelta = (shadow.spread + shadow.blur) * 2;
            Shadow.anchoredPosition = new Vector2(shadow.offset.x, -shadow.offset.y);

            ShadowGraphic.color = shadow.color;
            ShadowGraphic.SetMaterialDirty();
        }

        static void FullStretch(RectTransform child, RectTransform parent)
        {
            child.transform.SetParent(parent, false);
            child.anchorMin = new Vector2(0, 0);
            child.anchorMax = new Vector2(1, 1);
            child.anchoredPosition = Vector2.zero;
            child.pivot = new Vector2(0.5f, 0.5f);
            child.sizeDelta = Vector2.zero;
        }

        private float GetFirstDefinedSize(params float[] fallbacks)
        {
            for (int i = 0; i < fallbacks.Length; i++)
            {
                var f = fallbacks[i];

                if (!float.IsNaN(f)) return f;
            }

            return 0;
        }
    }
}
