using DirectShowLib;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;



namespace CamLoader
{
    public partial class Form1 : Form
    {
        // Importieren Sie die benötigten Funktionen
        [DllImport("olepro32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int OleCreatePropertyFrame(IntPtr hwndOwner, int x, int y,
            string lpszCaption, int cObjects, [In, MarshalAs(UnmanagedType.Interface)] ref object ppUnk,
            int cPages, IntPtr lpPageClsID, int lcid, int dwReserved, IntPtr pvReserved
        );




        // ATTRIBUTES

        private const string settingsFilePath = "cameraSettings.json";

        private BindingList<DsDevice> Cameras { get; set; } = new BindingList<DsDevice>();

        private IMediaControl VideoStream;





        // METHODS

        public Form1()
        {
            InitializeComponent();

            CameraComboBox.DataSource = Cameras;
            CameraComboBox.DisplayMember = "Name";

            // NotifyIcon erstellen
            notifyIcon1 = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Visible = false,
                Text = Text
            };

            // ContextMenuStrip erstellen
            contextMenuStrip1 = new ContextMenuStrip();
            var openMenuItem = new ToolStripMenuItem("Open", null, OnOpenMenuItemClick);
            var exitMenuItem = new ToolStripMenuItem("Exit", null, OnExitMenuItemClick);
            contextMenuStrip1.Items.Add(openMenuItem);
            contextMenuStrip1.Items.Add(exitMenuItem);

            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.DoubleClick += NotifyIcon_DoubleClick;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            LoadAvailableCameras();
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            var selectedCamera = (DsDevice)CameraComboBox.SelectedItem;
            if (selectedCamera != null)
            {
                Size cameraResolution = GetCameraResolution(selectedCamera);
                AdjustPictureBoxSize(PictureBoxCamera, cameraResolution);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            HideApplication();
            base.OnClosing(e);
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowApplication();
        }

        private void OnOpenMenuItemClick(object sender, EventArgs e)
        {
            ShowApplication();
        }

        private void OnExitMenuItemClick(object sender, EventArgs e)
        {
            StopVideoStream();
            notifyIcon1.Visible = false;
            Application.Exit();
        }

        private void CameraComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            StopVideoStream();

            var selectedCamera = (DsDevice)CameraComboBox.SelectedItem;
            if (selectedCamera != null)
            {
                Size cameraResolution = GetCameraResolution(selectedCamera);
                AdjustPictureBoxSize(PictureBoxCamera, cameraResolution);

                OpenCameraSettingsDialog();
            }
        }

        private void ButtonSettings_Click(object sender, EventArgs e)
        {
            OpenCameraSettingsDialog();
        }




        private void ShowApplication()
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void HideApplication()
        {
            StopVideoStream();
            Hide();
            notifyIcon1.Visible = true;
        }

        private void ShowTrayNotification(string title, string text, ToolTipIcon icon, int duration)
        {
            notifyIcon1.BalloonTipTitle = title;
            notifyIcon1.BalloonTipText = text;
            notifyIcon1.BalloonTipIcon = icon;
            notifyIcon1.ShowBalloonTip(duration);
        }


        private void StopVideoStream()
        {
            if (VideoStream != null)
            {
                VideoStream.Stop();
                VideoStream = null;
            }
        }

        private void LoadAvailableCameras()
        {
            Cameras.Clear();
            DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            foreach (DsDevice device in devices)
            {
                Cameras.Add(device);
            }


            if (File.Exists(settingsFilePath))
            {
                string jsonContent = File.ReadAllText(settingsFilePath);
                var savedSettings = JsonConvert.DeserializeObject<CameraSettings>(jsonContent);

                var counter = ApplySettingsToCameras(savedSettings);
                if (counter > 0)
                {
                    HideApplication();
                    ShowTrayNotification("Camera-Settings", $"Settings loaded for {counter} camera(s)", ToolTipIcon.Info, 5000);
                }
            }
        }

        private Size GetCameraResolution(DsDevice cameraDevice)
        {
            IBaseFilter cameraFilter;
            Guid baseFilterGuid = typeof(IBaseFilter).GUID;
            cameraDevice.Mon.BindToObject(null, null, ref baseFilterGuid, out object filterObject);
            cameraFilter = (IBaseFilter)filterObject;

            IPin pPin = null;
            pPin = DsFindPin.ByCategory(cameraFilter, PinCategory.Capture, 0);

            IAMStreamConfig streamConfig = pPin as IAMStreamConfig;
            AMMediaType mediaType = null;
            streamConfig.GetFormat(out mediaType);

            VideoInfoHeader videoInfoHeader = (VideoInfoHeader)Marshal.PtrToStructure(mediaType.formatPtr, typeof(VideoInfoHeader));
            Size resolution = new Size(videoInfoHeader.BmiHeader.Width, videoInfoHeader.BmiHeader.Height);

            Marshal.ReleaseComObject(cameraFilter);
            Marshal.ReleaseComObject(pPin);

            return resolution;
        }

        private void AdjustPictureBoxSize(PictureBox pictureBox, Size cameraResolution)
        {
            double aspectRatio = (double)cameraResolution.Width / cameraResolution.Height;

            int newWidth = pictureBox.Width;
            int newHeight = (int)(newWidth / aspectRatio);

            pictureBox.Size = new Size(newWidth, newHeight);
        }

        private void OpenCameraSettingsDialog()
        {
            StopVideoStream();

            var selectedCamera = (DsDevice)CameraComboBox.SelectedItem;
            if (selectedCamera != null)
            {
                // Erstellen Sie eine neue FilterGraph Instanz
                IFilterGraph2 graphBuilder = (IFilterGraph2)new FilterGraph();

                // Erstellen Sie das VideoRenderElement zum Anzeigen des Videostreams
                IVideoWindow videoWindow = (IVideoWindow)graphBuilder;

                // Erstellen Sie den CaptureGraphBuilder und verknüpfen Sie ihn mit dem FilterGraph
                ICaptureGraphBuilder2 captureGraphBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
                captureGraphBuilder.SetFiltergraph(graphBuilder);

                // Fügen Sie die ausgewählte Kamera zum FilterGraph hinzu
                IBaseFilter baseFilter;
                Guid baseFilterGuid = typeof(IBaseFilter).GUID;
                selectedCamera.Mon.BindToObject(null, null, ref baseFilterGuid, out object filterObject);
                baseFilter = (IBaseFilter)filterObject;
                graphBuilder.AddFilter(baseFilter, selectedCamera.Name);

                // Rendern Sie den Videostream auf das PictureBox-Steuerelement
                captureGraphBuilder.RenderStream(null, null, baseFilter, null, null);
                videoWindow.put_Owner(PictureBoxCamera.Handle);
                videoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipSiblings | WindowStyle.ClipChildren);
                videoWindow.SetWindowPosition(0, 0, PictureBoxCamera.Width, PictureBoxCamera.Height);
                videoWindow.put_Visible(OABool.True);

                // Starten Sie den Videostream
                VideoStream = (IMediaControl)graphBuilder;
                VideoStream.Run();


                // Zeige den Standard-Kameradialog zum Einstellen der Eigenschaften an
                IAMCameraControl cameraControl = baseFilter as IAMCameraControl;
                ISpecifyPropertyPages propertyPages = baseFilter as ISpecifyPropertyPages;
                if (propertyPages != null)
                {
                    DsCAUUID caGUID;
                    propertyPages.GetPages(out caGUID);
                    object baseFilterObj = baseFilter;
                    OleCreatePropertyFrame(this.Handle, 0, 0, "Camera-Settings", 1, ref baseFilterObj, 0, IntPtr.Zero, 0, 0, IntPtr.Zero);
                    SaveCameraSettings(selectedCamera);
                }
            }
        }

        private (int VendorID, int ProductID) ExtractVendorAndProductID(string devicePath)
        {
            var regex = new Regex(@"vid_(\w+)&pid_(\w+)", RegexOptions.IgnoreCase);
            var match = regex.Match(devicePath);

            if (match.Success)
            {
                int.TryParse(match.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int vendorID);
                int.TryParse(match.Groups[2].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int productID);

                return (vendorID, productID);
            }

            return (0, 0);
        }

        private CameraSettings GetCurrentCameraSettings(DsDevice cameraDevice)
        {
            IBaseFilter cameraFilter;
            Guid baseFilterGuid = typeof(IBaseFilter).GUID;
            cameraDevice.Mon.BindToObject(null, null, ref baseFilterGuid, out object filterObject);
            cameraFilter = (IBaseFilter)filterObject;

            // Zugriff auf die IAMCameraControl-Schnittstelle
            var cameraControl = cameraFilter as IAMCameraControl;
            // Zugriff auf die IAMVideoProcAmp-Schnittstelle
            var videoProcAmp = cameraFilter as IAMVideoProcAmp;


            // Extrahiere VendorID und ProductID
            var (vendorID, productID) = ExtractVendorAndProductID(cameraDevice.DevicePath);
            var currentSettings = new CameraSettings
            {
                DeviceName = cameraDevice.Name,
                DevicePath = cameraDevice.DevicePath,
                VendorID = vendorID,
                ProductID = productID
            };

            if (cameraControl != null)
            {
                foreach (DirectShowLib.CameraControlProperty property in Enum.GetValues(typeof(DirectShowLib.CameraControlProperty)))
                {
                    int hr = cameraControl.GetRange(property, out int minValue, out int maxValue, out int step, out int defaultValue, out DirectShowLib.CameraControlFlags flags);

                    if (hr == 0) // Erfolgreich
                    {
                        cameraControl.Get(property, out int currentValue, out flags);
                        currentSettings.CameraControlProperties.Add(new CameraControlPropertyValue
                        {
                            Property = property,
                            Value = currentValue,
                            Flags = flags,
                            MinValue = minValue,
                            MaxValue = maxValue,
                            Step = step,
                            DefaultValue = defaultValue
                        });
                    }
                }
            }

            if (videoProcAmp != null)
            {
                foreach (VideoProcAmpProperty property in Enum.GetValues(typeof(VideoProcAmpProperty)))
                {
                    int hr = videoProcAmp.GetRange(property, out int minValue, out int maxValue, out int step, out int defaultValue, out VideoProcAmpFlags flags);

                    if (hr == 0) // Erfolgreich
                    {
                        videoProcAmp.Get(property, out int currentValue, out flags);
                        currentSettings.VideoProcAmpProperties.Add(new VideoProcAmpPropertyValue
                        {
                            Property = property,
                            Value = currentValue,
                            Flags = flags,
                            MinValue = minValue,
                            MaxValue = maxValue,
                            Step = step,
                            DefaultValue = defaultValue
                        });
                    }
                }
            }


            Marshal.ReleaseComObject(cameraFilter);

            return currentSettings;
        }

        private void SaveCameraSettings(DsDevice currentCamera)
        {
            var currentCameraSettings = GetCurrentCameraSettings(currentCamera);
            string json = JsonConvert.SerializeObject(currentCameraSettings);
            File.WriteAllText(settingsFilePath, json);
            LabelCameraState.Text = "Saved camera settings!";

            ApplySettingsToCameras(currentCameraSettings);
        }

        private int ApplySettingsToCameras(CameraSettings settings)
        {
            if (settings == null) throw new Exception($"Failed to parse camera settings-file {settingsFilePath}");

            var counter = 0;
            foreach (var device in Cameras)
            {
                var (vendorID, productID) = ExtractVendorAndProductID(device.DevicePath);
                if (vendorID == settings.VendorID && productID == settings.ProductID)
                {
                    ApplyCameraSettings(device, settings);
                    counter++;
                }
            }

            if (counter > 0)
            {
                LabelCameraState.Text = $"Loaded settings for {counter} camera(s) from vendor ({settings.VendorID}) - productID ({settings.ProductID})";
            }

            return counter;
        }

        private void ApplyCameraSettings(DsDevice cameraDevice, CameraSettings settings)
        {
            IBaseFilter cameraFilter;
            Guid baseFilterGuid = typeof(IBaseFilter).GUID;
            cameraDevice.Mon.BindToObject(null, null, ref baseFilterGuid, out object filterObject);
            cameraFilter = (IBaseFilter)filterObject;

            // Zugriff auf die IAMCameraControl-Schnittstelle
            var cameraControl = cameraFilter as IAMCameraControl;
            // Zugriff auf die IAMVideoProcAmp-Schnittstelle
            var videoProcAmp = cameraFilter as IAMVideoProcAmp;

            if (cameraControl != null)
            {
                foreach (CameraControlPropertyValue propertyValue in settings.CameraControlProperties)
                {
                    cameraControl.Set(propertyValue.Property, propertyValue.Value, propertyValue.Flags);
                }
            }

            if (videoProcAmp != null)
            {
                foreach (VideoProcAmpPropertyValue propertyValue in settings.VideoProcAmpProperties)
                {
                    videoProcAmp.Set(propertyValue.Property, propertyValue.Value, propertyValue.Flags);
                }
            }

            Marshal.ReleaseComObject(cameraFilter);
        }


    }




    // SOME CLASSES FOR THE SETTINGS ;)

    public class CameraSettings
    {
        public string DeviceName { get; set; }
        public string DevicePath { get; set; }
        public int VendorID { get; set; }
        public int ProductID { get; set; }

        public List<CameraControlPropertyValue> CameraControlProperties { get; set; } = new List<CameraControlPropertyValue>();
        public List<VideoProcAmpPropertyValue> VideoProcAmpProperties { get; set; } = new List<VideoProcAmpPropertyValue>();
    }


    public class CameraControlPropertyValue
    {
        public DirectShowLib.CameraControlProperty Property { get; set; }
        public string PropertyName => Property.ToString();
        public int Value { get; set; }
        public DirectShowLib.CameraControlFlags Flags { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public int Step { get; set; }
        public int DefaultValue { get; set; }
    }

    public class VideoProcAmpPropertyValue
    {
        public VideoProcAmpProperty Property { get; set; }
        public string PropertyName => Property.ToString();
        public int Value { get; set; }
        public VideoProcAmpFlags Flags { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public int Step { get; set; }
        public int DefaultValue { get; set; }
    }

}