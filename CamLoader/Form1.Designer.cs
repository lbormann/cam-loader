namespace CamLoader
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            CameraComboBox = new ComboBox();
            PictureBoxCamera = new PictureBox();
            ButtonSettings = new Button();
            LabelCameraState = new Label();
            notifyIcon1 = new NotifyIcon(components);
            contextMenuStrip1 = new ContextMenuStrip(components);
            ((System.ComponentModel.ISupportInitialize)PictureBoxCamera).BeginInit();
            SuspendLayout();
            // 
            // CameraComboBox
            // 
            CameraComboBox.BackColor = Color.FromArgb(64, 64, 64);
            CameraComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            CameraComboBox.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point);
            CameraComboBox.ForeColor = Color.White;
            CameraComboBox.FormattingEnabled = true;
            CameraComboBox.Location = new Point(12, 49);
            CameraComboBox.Name = "CameraComboBox";
            CameraComboBox.Size = new Size(633, 38);
            CameraComboBox.TabIndex = 0;
            CameraComboBox.SelectionChangeCommitted += CameraComboBox_SelectionChangeCommitted;
            // 
            // PictureBoxCamera
            // 
            PictureBoxCamera.BackColor = Color.Silver;
            PictureBoxCamera.Location = new Point(12, 111);
            PictureBoxCamera.Name = "PictureBoxCamera";
            PictureBoxCamera.Size = new Size(760, 638);
            PictureBoxCamera.TabIndex = 1;
            PictureBoxCamera.TabStop = false;
            // 
            // ButtonSettings
            // 
            ButtonSettings.BackColor = Color.White;
            ButtonSettings.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            ButtonSettings.ForeColor = Color.Black;
            ButtonSettings.Location = new Point(651, 49);
            ButtonSettings.Name = "ButtonSettings";
            ButtonSettings.Size = new Size(121, 38);
            ButtonSettings.TabIndex = 2;
            ButtonSettings.Text = "Open Settings";
            ButtonSettings.UseVisualStyleBackColor = false;
            ButtonSettings.Click += ButtonSettings_Click;
            // 
            // LabelCameraState
            // 
            LabelCameraState.AutoSize = true;
            LabelCameraState.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point);
            LabelCameraState.Location = new Point(12, 16);
            LabelCameraState.Name = "LabelCameraState";
            LabelCameraState.Size = new Size(228, 30);
            LabelCameraState.TabIndex = 3;
            LabelCameraState.Text = "Please select a camera";
            // 
            // notifyIcon1
            // 
            notifyIcon1.Text = "notifyIcon1";
            notifyIcon1.Visible = true;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(61, 4);
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            ClientSize = new Size(784, 761);
            Controls.Add(LabelCameraState);
            Controls.Add(ButtonSettings);
            Controls.Add(PictureBoxCamera);
            Controls.Add(CameraComboBox);
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form1";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CamLoader";
            Shown += Form1_Shown;
            ResizeEnd += Form1_ResizeEnd;
            ((System.ComponentModel.ISupportInitialize)PictureBoxCamera).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox CameraComboBox;
        private PictureBox PictureBoxCamera;
        private Button ButtonSettings;
        private Label LabelCameraState;
        private NotifyIcon notifyIcon1;
        private ContextMenuStrip contextMenuStrip1;
    }
}