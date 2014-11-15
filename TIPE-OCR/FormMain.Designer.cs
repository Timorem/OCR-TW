namespace TIPE_OCR
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBoxPreProcessing = new System.Windows.Forms.GroupBox();
            this.buttonGrayScaling = new System.Windows.Forms.Button();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.imagePanel2 = new TIPE_OCR.ImagePanel();
            this.imagePanel1 = new TIPE_OCR.ImagePanel();
            this.imagePanelInput = new TIPE_OCR.ImagePanel();
            this.groupBoxPreProcessing.SuspendLayout();
            this.tableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxPreProcessing
            // 
            this.groupBoxPreProcessing.Controls.Add(this.imagePanel2);
            this.groupBoxPreProcessing.Controls.Add(this.imagePanel1);
            this.groupBoxPreProcessing.Controls.Add(this.buttonGrayScaling);
            this.groupBoxPreProcessing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxPreProcessing.Location = new System.Drawing.Point(3, 489);
            this.groupBoxPreProcessing.Name = "groupBoxPreProcessing";
            this.groupBoxPreProcessing.Size = new System.Drawing.Size(938, 87);
            this.groupBoxPreProcessing.TabIndex = 1;
            this.groupBoxPreProcessing.TabStop = false;
            this.groupBoxPreProcessing.Text = "Pre Processing";
            // 
            // buttonGrayScaling
            // 
            this.buttonGrayScaling.Location = new System.Drawing.Point(6, 19);
            this.buttonGrayScaling.Name = "buttonGrayScaling";
            this.buttonGrayScaling.Size = new System.Drawing.Size(75, 23);
            this.buttonGrayScaling.TabIndex = 0;
            this.buttonGrayScaling.Text = "Gray scaling";
            this.buttonGrayScaling.UseVisualStyleBackColor = true;
            this.buttonGrayScaling.Click += new System.EventHandler(this.buttonGrayScaling_Click);
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel.ColumnCount = 1;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Controls.Add(this.groupBoxPreProcessing, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.imagePanelInput, 0, 0);
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 2;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(944, 579);
            this.tableLayoutPanel.TabIndex = 2;
            // 
            // imagePanel2
            // 
            this.imagePanel2.CanvasSize = new System.Drawing.Size(60, 40);
            this.imagePanel2.Image = null;
            this.imagePanel2.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            this.imagePanel2.Location = new System.Drawing.Point(145, 0);
            this.imagePanel2.Name = "imagePanel2";
            this.imagePanel2.Size = new System.Drawing.Size(150, 150);
            this.imagePanel2.TabIndex = 2;
            this.imagePanel2.Zoom = 1F;
            // 
            // imagePanel1
            // 
            this.imagePanel1.CanvasSize = new System.Drawing.Size(60, 40);
            this.imagePanel1.Image = null;
            this.imagePanel1.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            this.imagePanel1.Location = new System.Drawing.Point(314, 38);
            this.imagePanel1.Name = "imagePanel1";
            this.imagePanel1.Size = new System.Drawing.Size(150, 150);
            this.imagePanel1.TabIndex = 1;
            this.imagePanel1.Zoom = 1F;
            // 
            // imagePanelInput
            // 
            this.imagePanelInput.CanvasSize = new System.Drawing.Size(60, 40);
            this.imagePanelInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imagePanelInput.Image = null;
            this.imagePanelInput.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            this.imagePanelInput.Location = new System.Drawing.Point(3, 3);
            this.imagePanelInput.Name = "imagePanelInput";
            this.imagePanelInput.Size = new System.Drawing.Size(938, 480);
            this.imagePanelInput.TabIndex = 2;
            this.imagePanelInput.Zoom = 1F;
            this.imagePanelInput.DragDrop += new System.Windows.Forms.DragEventHandler(this.imagePanelInput_DragDrop);
            this.imagePanelInput.DragEnter += new System.Windows.Forms.DragEventHandler(this.imagePanelInput_DragEnter);
            // 
            // FormMain
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(944, 579);
            this.Controls.Add(this.tableLayoutPanel);
            this.MinimumSize = new System.Drawing.Size(500, 300);
            this.Name = "FormMain";
            this.Text = "TIPE";
            this.groupBoxPreProcessing.ResumeLayout(false);
            this.tableLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxPreProcessing;
        private ImagePanel imagePanel2;
        private ImagePanel imagePanel1;
        private System.Windows.Forms.Button buttonGrayScaling;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private ImagePanel imagePanelInput;

    }
}

