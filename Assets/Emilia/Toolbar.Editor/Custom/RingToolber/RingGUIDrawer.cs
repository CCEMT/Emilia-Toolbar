using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Emilia.Toolbar.Editor
{
    /// <summary>
    /// 专门用于在GUI中绘制圆环的工具类
    /// </summary>
    public static class RingGUIDrawer
    {
        public static void DrawRing(Rect rect, float innerRadius, float outerRadius, int segments, Color[] colors, float angleOffset, Color centerColor)
        {
            Vector2 center = new Vector2(rect.x + rect.width * 0.5f, rect.y + rect.height * 0.5f);
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
            float actualInnerRadius = radius * Mathf.Clamp01(innerRadius);
            float actualOuterRadius = radius * Mathf.Clamp01(outerRadius);

            float anglePerSegment = 360f / segments;

            for (int i = 0; i < segments; i++)
            {
                float startAngle = (i * anglePerSegment + angleOffset) * Mathf.Deg2Rad;
                float endAngle = ((i + 1) * anglePerSegment + angleOffset) * Mathf.Deg2Rad;

                Color color = colors[i];

                Color oldColor = GUI.color;
                GUI.color = color;
                DrawFilledSegment(center, actualInnerRadius, actualOuterRadius, startAngle, endAngle, color);
                GUI.color = oldColor;
            }

            if (centerColor.a > 0f) DrawFilledCircle(center, actualInnerRadius + 1f, centerColor);
        }

        private static Material coloredMaterial;
        private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
        private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
        private static readonly int Cull = Shader.PropertyToID("_Cull");
        private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");

        private static Material GetColoredMaterial()
        {
            if (coloredMaterial) return coloredMaterial;
            Material mat = new Material(Shader.Find("Hidden/Internal-Colored"));
            mat.hideFlags = HideFlags.HideAndDontSave;
            mat.SetInt(SrcBlend, (int) BlendMode.SrcAlpha);
            mat.SetInt(DstBlend, (int) BlendMode.OneMinusSrcAlpha);
            mat.SetInt(Cull, (int) CullMode.Off);
            mat.SetInt(ZWrite, 0);
            coloredMaterial = mat;
            return mat;
        }

        private static void DrawFilledSegment(Vector2 center, float innerRadius, float outerRadius, float startAngle, float endAngle, Color color)
        {
            Material mat = GetColoredMaterial();
            Matrix4x4 savedMatrix = GUI.matrix;

            int subdivisions = 36;
            float angleStep = (endAngle - startAngle) / subdivisions;

            mat.SetPass(0);
            GL.PushMatrix();
            GL.LoadPixelMatrix();
            GL.Begin(GL.TRIANGLES);
            GL.Color(color);

            for (int i = 0; i < subdivisions; i++)
            {
                float currentAngle = startAngle + i * angleStep;
                float nextAngle = startAngle + (i + 1) * angleStep;

                Vector3 innerStart = new Vector3(
                    center.x + Mathf.Cos(currentAngle) * innerRadius,
                    center.y + Mathf.Sin(currentAngle) * innerRadius,
                    0
                );

                Vector3 outerStart = new Vector3(
                    center.x + Mathf.Cos(currentAngle) * outerRadius,
                    center.y + Mathf.Sin(currentAngle) * outerRadius,
                    0
                );

                Vector3 innerEnd = new Vector3(
                    center.x + Mathf.Cos(nextAngle) * innerRadius,
                    center.y + Mathf.Sin(nextAngle) * innerRadius,
                    0
                );

                Vector3 outerEnd = new Vector3(
                    center.x + Mathf.Cos(nextAngle) * outerRadius,
                    center.y + Mathf.Sin(nextAngle) * outerRadius,
                    0
                );

                // 绘制两个三角形
                GL.Vertex(innerStart);
                GL.Vertex(outerStart);
                GL.Vertex(outerEnd);

                GL.Vertex(innerStart);
                GL.Vertex(outerEnd);
                GL.Vertex(innerEnd);
            }

            GL.End();
            GL.PopMatrix();

            GUI.matrix = savedMatrix;
        }

        private static void DrawFilledCircle(Vector2 center, float radius, Color color)
        {
            Material mat = GetColoredMaterial();
            Matrix4x4 savedMatrix = GUI.matrix;

            int subdivisions = 36;
            float angleStep = 2f * Mathf.PI / subdivisions;

            mat.SetPass(0);
            GL.PushMatrix();
            GL.LoadPixelMatrix();
            GL.Begin(GL.TRIANGLES);
            GL.Color(color);

            for (int i = 0; i < subdivisions; i++)
            {
                float currentAngle = i * angleStep;
                float nextAngle = (i + 1) * angleStep;

                Vector3 centerPoint = new Vector3(center.x, center.y, 0);
                Vector3 currentPoint = new Vector3(
                    center.x + Mathf.Cos(currentAngle) * radius,
                    center.y + Mathf.Sin(currentAngle) * radius,
                    0
                );
                Vector3 nextPoint = new Vector3(
                    center.x + Mathf.Cos(nextAngle) * radius,
                    center.y + Mathf.Sin(nextAngle) * radius,
                    0
                );

                GL.Vertex(centerPoint);
                GL.Vertex(currentPoint);
                GL.Vertex(nextPoint);
            }

            GL.End();
            GL.PopMatrix();

            GUI.matrix = savedMatrix;
        }

        /// <summary>
        /// 在GUI中绘制自定义GUI内容
        /// </summary>
        public static void DrawCustomGUI(Rect rect, float innerRadius, float outerRadius, int segments, Action<int, Rect> guiDrawAction, float angleOffset = 0f)
        {
            if (guiDrawAction == null) return;

            Vector2 center = new Vector2(rect.x + rect.width * 0.5f, rect.y + rect.height * 0.5f);
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f;

            float actualInnerRadius = radius * Mathf.Clamp01(innerRadius);
            float actualOuterRadius = radius * Mathf.Clamp01(outerRadius);
            float midRadius = (actualInnerRadius + actualOuterRadius) * 0.5f;

            float anglePerSegment = 360f / segments;

            for (int i = 0; i < segments; i++)
            {
                float midAngle = (i * anglePerSegment + anglePerSegment * 0.5f + angleOffset) * Mathf.Deg2Rad;
                float x = center.x + Mathf.Cos(midAngle) * midRadius;
                float y = center.y + Mathf.Sin(midAngle) * midRadius;

                Vector2 guiPosition = new Vector2(x, y);
                Rect guiRect = new Rect(guiPosition.x - 50, guiPosition.y - 25, 100, 50);
                guiDrawAction(i, guiRect);
            }
        }

        /// <summary>
        /// 获取圆环分段的中心点
        /// </summary>
        public static Vector2 GetSegmentCenter(Rect rect, float innerRadius, float outerRadius, int segmentIndex, int segments, float angleOffset = 0f)
        {
            Vector2 center = new Vector2(rect.x + rect.width * 0.5f, rect.y + rect.height * 0.5f);
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f;

            float actualInnerRadius = radius * Mathf.Clamp01(innerRadius);
            float actualOuterRadius = radius * Mathf.Clamp01(outerRadius);
            float midRadius = (actualInnerRadius + actualOuterRadius) * 0.5f;

            float anglePerSegment = 360f / segments;
            float midAngle = (segmentIndex * anglePerSegment + anglePerSegment * 0.5f + angleOffset) * Mathf.Deg2Rad;

            float x = center.x + Mathf.Cos(midAngle) * midRadius;
            float y = center.y + Mathf.Sin(midAngle) * midRadius;

            return new Vector2(x, y);
        }

        /// <summary>
        /// 获取指定坐标所在的圆环片段索引
        /// </summary>
        public static int GetRingSegmentIndex(Vector2 point, Rect rect, float innerRadius, float outerRadius, int segments, float angleOffset = 0f)
        {
            Vector2 center = new Vector2(rect.x + rect.width * 0.5f, rect.y + rect.height * 0.5f);
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
            float actualInnerRadius = radius * Mathf.Clamp01(innerRadius);
            float actualOuterRadius = radius * Mathf.Clamp01(outerRadius);

            Vector2 direction = point - center;
            float distance = direction.magnitude;

            if (distance < actualInnerRadius || distance > actualOuterRadius) return -1;

            float pointAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (pointAngle < 0) pointAngle += 360f;

            float adjustedAngle = (pointAngle - angleOffset) % 360f;
            if (adjustedAngle < 0) adjustedAngle += 360f;

            float anglePerSegment = 360f / segments;
            int segmentIndex = Mathf.FloorToInt(adjustedAngle / anglePerSegment);

            return Mathf.Clamp(segmentIndex, 0, segments - 1);
        }
    }
}