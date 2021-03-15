using QuickResolutionSwitcher.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickResolutionSwitcher
{
    public class ApplicationContext : System.Windows.Forms.ApplicationContext
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            private const int CCHDEVICENAME = 0x20;
            private const int CCHFORMNAME = 0x20;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("User32.dll")]
        public static extern int ChangeDisplaySettings(ref DEVMODE lpDevMode, uint dwflags);

        #region === Data Members ===

        private NotifyIcon _notifyIcon;
        private readonly ContextMenuStrip _contextMenu = new ContextMenuStrip();

        #endregion

        #region === Construction ===

        public ApplicationContext()
        {
            // get resolutions
            var resolutions = ((ResolutionConfigurationSection)ConfigurationManager.GetSection("resolutions"))
                .Items
                .OfType<ResolutionConfigurationElement>()
                .ToList();

            // loop through resolutions
            for (var i = 0; i < resolutions.Count; i++)
            {
                // add context item
                _contextMenu.Items.Add(new ToolStripMenuItem(resolutions[i].Value, null, ContextMenuStrip_ResolutionClicked)
                {
                    ShortcutKeyDisplayString = $"Win+Ctrl+Alt+{Keys.F1 + i}"
                });

                // registry hotkey
                HotKeyManager.RegisterHotKey(Keys.F1 + i, KeyModifiers.Control | KeyModifiers.Alt | KeyModifiers.Windows);
            }

            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add("Exit", null, (o, e) => { Application.Exit(); });

            // create notifyicon
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Visible = true;
            _notifyIcon.Icon = Properties.Resources.icons8_monitor_tray;
            _notifyIcon.ContextMenuStrip = _contextMenu;

            // add subscriptions
            _contextMenu.Opening += ContextMenuStrip_Opening;
            HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;
        }

        #endregion

        #region === Event Handlers ===

        private void ContextMenuStrip_Opening(object sender, EventArgs e)
        {
            try
            {
                // loop through items
                foreach (var item in
                    ((ContextMenuStrip)sender)
                        .Items
                        .OfType<ToolStripMenuItem>()
                        .ToList())
                {
                    item.Checked = (string.Equals(item.Text, GetResolution()));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
            }
        }

        private void ContextMenuStrip_ResolutionClicked(object sender, EventArgs e)
        {
            try
            {
                // get resolution
                var resolution = ((ToolStripMenuItem)sender).Text;

                // get regex
                var match = Regex.Match(resolution, @"(?<Width>\d+)x(?<Height>\d+)");

                // get dimensions
                var width = Convert.ToInt32(match.Groups["Width"].Value);
                var height = Convert.ToInt32(match.Groups["Height"].Value);

                // set resolution
                SetResolution(width, height);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
            }
        }

        private void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e)
        {
            try
            {
                if ((e.Modifiers.HasFlag(KeyModifiers.Control)) && (e.Modifiers.HasFlag(KeyModifiers.Alt)) && (e.Modifiers.HasFlag(KeyModifiers.Windows)))
                {
                    // get resolutions
                    var resolutions = ((ResolutionConfigurationSection)ConfigurationManager.GetSection("resolutions"))
                        .Items
                        .OfType<ResolutionConfigurationElement>()
                        .ToList();

                    // get index
                    var index = e.Key - Keys.F1;

                    if ((index >= 0) && (index < resolutions.Count))
                    {
                        // get resolution
                        var resolution = resolutions[index];

                        // get regex
                        var match = Regex.Match(resolution.Value, @"(?<Width>\d+)x(?<Height>\d+)");

                        // get dimensions
                        var width = Convert.ToInt32(match.Groups["Width"].Value);
                        var height = Convert.ToInt32(match.Groups["Height"].Value);

                        // set resolution
                        SetResolution(width, height);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
            }
        }

        #endregion

        #region === Properties ===



        #endregion

        #region === Operations ===

        public void SetResolution(int width, int height)
        {
            // get old resolution
            var oldResolution = new DEVMODE();
            EnumDisplaySettings(null, -1, ref oldResolution);

            // get new resolution
            DEVMODE newMode = oldResolution;
            newMode.dmDisplayFixedOutput = 0;
            newMode.dmPelsWidth = width;
            newMode.dmPelsHeight = height;
            newMode.dmBitsPerPel = oldResolution.dmBitsPerPel;

            // Capturing the operation result  
            int result = ChangeDisplaySettings(ref newMode, 0);
            if (result == 0) //DISP_CHANGE_SUCCESSFUL)
                Console.WriteLine("Succeeded.");
            else if (result == -2) //DISP_CHANGE_BADMODE)
                Console.WriteLine("Mode not supported.");
            else if (result == 1) //DISP_CHANGE_RESTART)
                Console.WriteLine("Restart required.");
            else
                Console.WriteLine("Failed. Error code = {0}", result);
        }

        public static string GetResolution()
        {
            var mode = new DEVMODE();
            if (EnumDisplaySettings(null, -1, ref mode))
            {
                return $"{mode.dmPelsWidth}x{mode.dmPelsHeight}";
            }
            return null;
        }

        #endregion
    }
}
