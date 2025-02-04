﻿// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu
{
    using System;
    using System.Reflection;
    using System.Windows.Forms;
    using SystemTrayMenu.Utilities;

    internal static class Program
    {
        private static bool isStartup = true;

        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                bool killOtherInstances = true;
                if (args != null && args.Length > 0 &&
                    (args[0] == "-h" || args[0] == "-hotkey"))
                {
                    args = null;
                    killOtherInstances = false;
                }

                Log.Initialize();
                Translator.Initialize();
                Config.SetFolderByWindowsContextMenu(args);
                Config.LoadOrSetByUser();
                Config.Initialize();

                if (SingleAppInstance.Initialize(killOtherInstances))
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.ThreadException += (sender, e) => AskUserSendError(e.Exception);
                    Scaling.Initialize();
                    FolderOptions.Initialize();

                    using (new App())
                    {
                        isStartup = false;
                        Log.WriteApplicationRuns();
                        Application.Run();
                    }
                }

                Config.Dispose();
            }
            catch (Exception ex)
            {
                AskUserSendError(ex);
            }
            finally
            {
                Log.Close();
            }

            static void AskUserSendError(Exception ex)
            {
                Log.Error("Application Crashed", ex);

                if (MessageBox.Show(
                    "A problem has been encountered and the application needs to restart. " +
                    "Reporting this error will help us make our product better. " +
                    "Press yes to open your standard email app.",
                    "SystemTrayMenu BugSplat",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Log.ProcessStart("mailto:" + "markus@hofknecht.eu" +
                        "?subject=SystemTrayMenu Bug reported " +
                        Assembly.GetEntryAssembly().GetName().Version +
                        "&body=" + ex.ToString());
                }

                if (!isStartup)
                {
                    AppRestart.ByThreadException();
                }
            }
        }
    }
}