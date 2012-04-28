﻿using System;
using System.Drawing;
using KangaModeling.Graphics.GdiPlus.Utilities;
using KangaModeling.Graphics.Primitives;
using Point = KangaModeling.Graphics.Primitives.Point;
using Size = KangaModeling.Graphics.Primitives.Size;
using Color = KangaModeling.Graphics.Primitives.Color;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using KangaModeling.Graphics.GdiPlus.Resources;
using System.Collections.Generic;

namespace KangaModeling.Graphics.GdiPlus
{
    public sealed class GdiPlusGraphicContext : IGraphicContext, IDisposable
    {
        private readonly System.Drawing.Graphics m_Graphics;
        private readonly PrivateFontCollection m_FontCollection = new PrivateFontCollection();
        private readonly Random m_Random = new Random();

        public GdiPlusGraphicContext(System.Drawing.Graphics graphics)
        {
            if (graphics == null) throw new ArgumentNullException("graphics");

            m_Graphics = graphics;

            FillFontCollection();
        }

        #region IGraphicContext Members

        public void DrawRectangle(Point location, Size size, Color color)
        {
            var rectangle = new RectangleF(location.ToPointF(), size.ToSizeF());

            using (var pen = new Pen(color.ToColor()))
            {
                Point topLeft = location;
                Point topRight = location.Offset(size.Width, 0);
                Point bottomLeft = location.Offset(0, size.Height);
                Point bottomRight = location.Offset(size.Width, size.Height);

                DrawLineCore(topLeft, topRight, pen.Width, p => { });
                DrawLineCore(topRight, bottomRight, pen.Width, p => { });
                DrawLineCore(bottomRight, bottomLeft, pen.Width, p => { });
                DrawLineCore(bottomLeft, topLeft, pen.Width, p => { });

                //m_Graphics.DrawRectangle(pen, location.X, location.Y, size.Width, size.Height);
            }
        }

        public void FillRectangle(Point location, Size size, Color color)
        {
            var rectangle = new RectangleF(location.ToPointF(), size.ToSizeF());

            using (var brush = new SolidBrush(color.ToColor()))
            {
                m_Graphics.FillRectangle(brush, rectangle);
            }
        }

        public void DrawText(string text, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, Point location, Size size)
        {
            using (var font = CreateFont())
            {
                var rectangle = new RectangleF(location.ToPointF(), size.ToSizeF());

                using (var stringFormat = new StringFormat())
                {
                    stringFormat.Alignment = horizontalAlignment.ToStringAlignment();
                    stringFormat.LineAlignment = verticalAlignment.ToStringAlignment();

                    string stringToDraw = ReplaceLineBreaks(text);
                    m_Graphics.DrawString(stringToDraw, font, Brushes.Black, rectangle, stringFormat);
                }
            }
        }

        private static string ReplaceLineBreaks(string text)
        {
            return text.Replace("\\n", Environment.NewLine);
        }

        public void DrawLine(Point from, Point to, float width)
        {
            DrawLineCore(from, to, width, pen => { });
        }

        public void DrawDashedLine(Point from, Point to, float width)
        {
            DrawLineCore(from, to, width, pen =>
            {
                pen.DashStyle = DashStyle.Dash;
            });
        }

        public void DrawArrow(Point from, Point to, float width, float arrowCapWidth, float arrowCapHeight)
        {
            DrawLineCore(from, to, width, pen =>
            {
                pen.CustomEndCap = new AdjustableArrowCap(arrowCapWidth, arrowCapHeight, false);
            });
        }

        public void DrawDashedArrow(Point from, Point to, float width, float arrowCapWidth, float arrowCapHeight)
        {
            DrawLineCore(from, to, width, pen =>
            {
                pen.DashStyle = DashStyle.Dash;
                pen.CustomEndCap = new AdjustableArrowCap(arrowCapWidth, arrowCapHeight, false);
            });
        }

        public Size MeasureText(string text)
        {
            using (var font = CreateFont())
            {
                string stringToDraw = ReplaceLineBreaks(text);
                SizeF size = m_Graphics.MeasureString(stringToDraw, font);
                return new Size(size.Width, size.Height);
            }
        }

        public IDisposable ApplyOffset(float dx, float dy)
        {
            var graphicsContainer = m_Graphics.BeginContainer();

            m_Graphics.TranslateTransform(dx, dy);

            return new DoOnDispose(() => m_Graphics.EndContainer(graphicsContainer));
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            m_FontCollection.Dispose();
        }

        #endregion

        #region Private Methods

        private void DrawLineCore(Point from, Point to, float width, Action<Pen> initializePen)
        {
            using (var pen = new Pen(Brushes.Black, width))
            {
                initializePen(pen);

                pen.Alignment = PenAlignment.Left;

                m_Graphics.SmoothingMode = SmoothingMode.HighQuality;
                
                int minimumOff = -5;
                int maximumOff = 5;
                
                float firstControlPointPosition = (float)m_Random.Next(20, 80) / 100;
                Point firstControlPoint = new Point(
                    from.X + (to.X - from.X) * firstControlPointPosition + m_Random.Next(minimumOff, maximumOff),
                    from.Y + (to.Y - from.Y) * firstControlPointPosition + m_Random.Next(minimumOff, maximumOff));

                float secondControlPointPosition = (float)m_Random.Next(20, 80) / 100;
                Point secondControlPoint = new Point(
                    from.X + (to.X - from.X) * secondControlPointPosition + m_Random.Next(minimumOff, maximumOff),
                    from.Y + (to.Y - from.Y) * secondControlPointPosition + m_Random.Next(minimumOff, maximumOff));

                m_Graphics.DrawBezier(pen, from.ToPointF(), firstControlPoint.ToPointF(), secondControlPoint.ToPointF(), to.ToPointF());
            }
        }

        private void FillFontCollection()
        {
            byte[] fontData = Fonts.BuxtonSketch;
            IntPtr fontMemory = Marshal.AllocCoTaskMem(fontData.Length);
            try
            {
                Marshal.Copy(fontData, 0, fontMemory, fontData.Length);
                m_FontCollection.AddMemoryFont(fontMemory, fontData.Length);
            }
            finally
            {
                Marshal.FreeCoTaskMem(fontMemory);
            }
        }

        private Font CreateFont()
        {
            return new Font(m_FontCollection.Families[0], 15);
        }

        #endregion
    }
}