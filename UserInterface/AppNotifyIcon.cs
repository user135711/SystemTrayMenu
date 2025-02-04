﻿// <copyright file="AppNotifyIcon.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu.UserInterface
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using SystemTrayMenu.Helper;
    using SystemTrayMenu.Utilities;
    using Timer = System.Windows.Forms.Timer;

    internal class AppNotifyIcon : IDisposable
    {
        private readonly Timer load = new();
        private readonly NotifyIcon notifyIcon = new();
        private bool threadsLoading;
        private int rotationAngle;

        public AppNotifyIcon()
        {
            notifyIcon.Icon = Resources.StaticResources.LoadingIcon;
            load.Tick += Load_Tick;
            load.Interval = 15;
            notifyIcon.Text = Translator.GetText("SystemTrayMenu");
            notifyIcon.Visible = true;
            notifyIcon.Icon = Config.GetAppIcon();

            AppContextMenu contextMenus = new();

            contextMenus.ClickedOpenLog += ClickedOpenLog;
            void ClickedOpenLog()
            {
                OpenLog?.Invoke();
            }

            notifyIcon.ContextMenuStrip = contextMenus.Create();
            notifyIcon.MouseClick += NotifyIcon_MouseClick;
            void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
            {
                VerifyClick(e);
            }

            notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;
            void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
            {
                VerifyClick(e);
            }
        }

        public event EventHandlerEmpty Click;

        public event EventHandlerEmpty OpenLog;

        public void Dispose()
        {
            notifyIcon.Icon = null;
            notifyIcon.Dispose();
            load.Dispose();
        }

        public void LoadingStart()
        {
            threadsLoading = true;
            load.Start();
        }

        public void LoadingStop()
        {
            Cursor.Current = Cursors.Default;
            threadsLoading = false;
        }

        private void VerifyClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Click?.Invoke();
            }
        }

        private void Load_Tick(object sender, EventArgs e)
        {
            if (threadsLoading)
            {
                rotationAngle += 5;
                using Bitmap bitmapLoading = Resources.StaticResources.LoadingIcon.ToBitmap();
                using Bitmap bitmapLoadingRotated = new(ImagingHelper.RotateImage(bitmapLoading, rotationAngle));
                IntPtr hIcon = bitmapLoadingRotated.GetHicon();
                notifyIcon.Icon = (Icon)Icon.FromHandle(hIcon).Clone();
                DllImports.NativeMethods.User32DestroyIcon(hIcon);
            }
            else
            {
                notifyIcon.Icon = Config.GetAppIcon();
                load.Stop();
            }
        }
    }
}