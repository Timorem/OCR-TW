using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TIPE_OCR
{
    public partial class ImagePanel : UserControl
    {
        private Size m_canvasSize = new Size(60, 40);
        private Image m_image;
        private InterpolationMode m_interMode = InterpolationMode.HighQualityBilinear;
        private int m_viewRectHeight; // view window width and height
        private int m_viewRectWidth; // view window width and height

        private float m_zoom = 1.0f;

        public ImagePanel()
        {
            InitializeComponent();

            // Set the value of the double-buffering style bits to true.
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
        }

        public float Zoom
        {
            get { return m_zoom; }
            set
            {
                if (value < 0.001f) value = 0.001f;
                m_zoom = value;

                DisplayScrollbar();
                SetScrollbarValues();
                Invalidate();
            }
        }

        public Size CanvasSize
        {
            get { return m_canvasSize; }
            set
            {
                m_canvasSize = value;
                DisplayScrollbar();
                SetScrollbarValues();
                Invalidate();
            }
        }

        public Image Image
        {
            get { return m_image; }
            set
            {
                m_image = value;
                DisplayScrollbar();
                SetScrollbarValues();
                Invalidate();
            }
        }

        public InterpolationMode InterpolationMode
        {
            get { return m_interMode; }
            set { m_interMode = value; }
        }

        protected override void OnLoad(EventArgs e)
        {
            DisplayScrollbar();
            SetScrollbarValues();
            base.OnLoad(e);
        }

        protected override void OnResize(EventArgs e)
        {
            DisplayScrollbar();
            SetScrollbarValues();
            base.OnResize(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            //draw image
            if (m_image != null)
            {
                Rectangle srcRect, distRect;
                var pt = new Point((int) (hScrollBar1.Value/m_zoom), (int) (vScrollBar1.Value/m_zoom));
                if (m_canvasSize.Width*m_zoom < m_viewRectWidth && m_canvasSize.Height*m_zoom < m_viewRectHeight)
                    srcRect = new Rectangle(0, 0, m_canvasSize.Width, m_canvasSize.Height); // view all image
                else
                    srcRect = new Rectangle(pt, new Size((int) (m_viewRectWidth/m_zoom), (int) (m_viewRectHeight/m_zoom)));
                        // view a portion of image

                distRect = new Rectangle(-srcRect.Width/2, -srcRect.Height/2, srcRect.Width, srcRect.Height);
                    // the center of apparent image is on origin

                var mx = new Matrix(); // create an identity matrix
                mx.Scale(m_zoom, m_zoom); // zoom image
                mx.Translate(m_viewRectWidth/2.0f, m_viewRectHeight/2.0f, MatrixOrder.Append);
                    // move image to view window center

                Graphics g = e.Graphics;
                g.InterpolationMode = m_interMode;
                g.Transform = mx;
                g.DrawImage(m_image, distRect, srcRect, GraphicsUnit.Pixel);
            }
        }

        private void DisplayScrollbar()
        {
            m_viewRectWidth = Width;
            m_viewRectHeight = Height;

            if (m_image != null) m_canvasSize = m_image.Size;

            // If the zoomed image is wider than view window, show the HScrollBar and adjust the view window
            if (m_viewRectWidth > m_canvasSize.Width*m_zoom)
            {
                hScrollBar1.Visible = false;
                m_viewRectHeight = Height;
            }
            else
            {
                hScrollBar1.Visible = true;
                m_viewRectHeight = Height - hScrollBar1.Height;
            }

            // If the zoomed image is taller than view window, show the VScrollBar and adjust the view window
            if (m_viewRectHeight > m_canvasSize.Height*m_zoom)
            {
                vScrollBar1.Visible = false;
                m_viewRectWidth = Width;
            }
            else
            {
                vScrollBar1.Visible = true;
                m_viewRectWidth = Width - vScrollBar1.Width;
            }

            // Set up scrollbars
            hScrollBar1.Location = new Point(0, Height - hScrollBar1.Height);
            hScrollBar1.Width = m_viewRectWidth;
            vScrollBar1.Location = new Point(Width - vScrollBar1.Width, 0);
            vScrollBar1.Height = m_viewRectHeight;
        }

        private void SetScrollbarValues()
        {
            // Set the Maximum, Minimum, LargeChange and SmallChange properties.
            vScrollBar1.Minimum = 0;
            hScrollBar1.Minimum = 0;

            // If the offset does not make the Maximum less than zero, set its value. 
            if ((m_canvasSize.Width*m_zoom - m_viewRectWidth) > 0)
            {
                hScrollBar1.Maximum = (int) (m_canvasSize.Width*m_zoom) - m_viewRectWidth;
            }
            // If the VScrollBar is visible, adjust the Maximum of the 
            // HSCrollBar to account for the width of the VScrollBar.  
            if (vScrollBar1.Visible)
            {
                hScrollBar1.Maximum += vScrollBar1.Width;
            }
            hScrollBar1.LargeChange = hScrollBar1.Maximum/10;
            hScrollBar1.SmallChange = hScrollBar1.Maximum/20;

            // Adjust the Maximum value to make the raw Maximum value 
            // attainable by user interaction.
            hScrollBar1.Maximum += hScrollBar1.LargeChange;

            // If the offset does not make the Maximum less than zero, set its value.    
            if ((m_canvasSize.Height*m_zoom - m_viewRectHeight) > 0)
            {
                vScrollBar1.Maximum = (int) (m_canvasSize.Height*m_zoom) - m_viewRectHeight;
            }

            // If the HScrollBar is visible, adjust the Maximum of the 
            // VSCrollBar to account for the width of the HScrollBar.
            if (hScrollBar1.Visible)
            {
                vScrollBar1.Maximum += hScrollBar1.Height;
            }
            vScrollBar1.LargeChange = vScrollBar1.Maximum/10;
            vScrollBar1.SmallChange = vScrollBar1.Maximum/20;

            // Adjust the Maximum value to make the raw Maximum value 
            // attainable by user interaction.
            vScrollBar1.Maximum += vScrollBar1.LargeChange;
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            Invalidate();
        }
    }
}