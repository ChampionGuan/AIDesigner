using UnityEngine;

namespace AIDesigner
{
    public static class EditorZoomArea
    {
        private static Matrix4x4 _prevGuiMatrix;

        private static Rect groupRect = default(Rect);

        public static Rect Begin(Rect screenCoordsArea, float zoomScale)
        {
            GUI.EndGroup();
            Rect val = screenCoordsArea.ScaleSizeBy(1f / zoomScale, screenCoordsArea.TopLeft());
            val.y = val.y + 21f;
            GUI.BeginGroup(val);
            _prevGuiMatrix = GUI.matrix;
            Matrix4x4 val2 = Matrix4x4.TRS((Vector2) (val.TopLeft()), Quaternion.identity, Vector3.one);
            Vector3 one = Vector3.one;
            one.x = one.y = zoomScale;
            Matrix4x4 val3 = Matrix4x4.Scale(one);
            GUI.matrix = val2 * val3 * val2.inverse * GUI.matrix;
            return val;
        }

        public static void End()
        {
            GUI.matrix = _prevGuiMatrix;
            GUI.EndGroup();
            groupRect.y = 21f;
            groupRect.width = (float) Screen.width;
            groupRect.height = (float) Screen.height;
            GUI.BeginGroup(groupRect);
        }
    }
}