using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace languageFlashCards
{
    public class OutlinedLabel : Label
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color OutlineColor { get; set; } = Color.White;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float OutlineWidth { get; set; } = 4f;

        protected override void OnPaint(PaintEventArgs e)
        {
            // Do NOT call base.OnPaint(e); or it will paint default text
            e.Graphics.Clear(this.BackColor);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            using var path = new GraphicsPath();

            float emSize = e.Graphics.DpiY * Font.Size / 72f;

            var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            path.AddString(
                Text,
                Font.FontFamily,
                (int)Font.Style,
                emSize,
                ClientRectangle,
                format
            );

            using var pen = new Pen(OutlineColor, OutlineWidth)
            {
                LineJoin = LineJoin.Round
            };

            using var brush = new SolidBrush(ForeColor);

            e.Graphics.DrawPath(pen, path);
            e.Graphics.FillPath(brush, path);
        }
    }
}