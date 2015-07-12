using System;
using System.Drawing;
using System.Windows.Forms;

namespace Downtify.GUI
{
    public class TextProgressBar : ProgressBar
    {
        public override string Text { get; set; }
        public int TotalTracks { get; set; }
        public int CurrentTrack { get; set; }
        public bool ShowText { get; set; }

        public TextProgressBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            Text = "";
            ShowText = false;
            TotalTracks = 0;
            CurrentTrack = 0;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = ClientRectangle;

            ProgressBarRenderer.DrawHorizontalBar(e.Graphics, rect);
            rect.Inflate(-3, -3);
            if (Value > 0)
            {
                Rectangle clip = new Rectangle(rect.X, rect.Y, (int)Math.Round(((float)Value / Maximum) * rect.Width), rect.Height);
                ProgressBarRenderer.DrawHorizontalChunks(e.Graphics, clip);
            }
            if (ShowText)
            {
                string text = String.Format(Text, CurrentTrack, TotalTracks);
                float center_x = this.Width / 2.0f - (e.Graphics.MeasureString(text, SystemFonts.DefaultFont).Width / 2.0f);
                float center_y = this.Height / 2.0f - (e.Graphics.MeasureString(text, SystemFonts.DefaultFont).Height / 2.0f);
                e.Graphics.DrawString(text, SystemFonts.DefaultFont, Brushes.Black, new PointF(center_x, center_y));
            }
        }
    }
}