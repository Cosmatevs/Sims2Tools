﻿/*
 * Log Watcher - a utility for monitoring Sims 2 ObjectError logs
 *
 * William Howard - 2020-2021
 *
 * Permission granted to use this code in any way, except to claim it as your own or sell it
 */

using LogWatcher.Controls;
using Sims2Tools;
using Sims2Tools.Updates;
using Sims2Tools.Utils.Persistence;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace LogWatcher
{
    public partial class LogWatcherForm : Form
    {
#pragma warning disable IDE0052 // Remove unread private members
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#pragma warning restore IDE0052 // Remove unread private members

        private MruList MyMruList;
        private Updater MyUpdater;

        private String logsDir;

        public LogWatcherForm()
        {
            InitializeComponent();
            this.Text = LogWatcherApp.AppName;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            RegistryTools.LoadAppSettings(LogWatcherApp.RegistryKey, LogWatcherApp.AppVersionMajor, LogWatcherApp.AppVersionMinor);
            RegistryTools.LoadFormSettings(LogWatcherApp.RegistryKey, this);

            logsDir = $"{Sims2ToolsLib.Sims2HomePath}\\Logs";

            MyMruList = new MruList(LogWatcherApp.RegistryKey, menuItemRecentLogs, Properties.Settings.Default.MruSize);
            MyMruList.FileSelected += MyMruList_FileSelected;

            MyUpdater = new Updater(LogWatcherApp.RegistryKey, menuHelp);
            MyUpdater.CheckForUpdates();

            String optOpenAtStart = (String)RegistryTools.GetSetting(LogWatcherApp.RegistryKey + @"\Options", "OpenAtStart", "None");
            menuItemOpenAll.Checked = (optOpenAtStart.Equals("All"));
            menuItemOpenRecent.Checked = (optOpenAtStart.Equals("Recent"));

            menuItemAutoOpen.Checked = ((int)RegistryTools.GetSetting(LogWatcherApp.RegistryKey + @"\Options", "AutoOpen", 1) != 0);
            menuItemAutoUpdate.Checked = ((int)RegistryTools.GetSetting(LogWatcherApp.RegistryKey + @"\Options", "AutoUpdate", 1) != 0);
            menuItemAutoClose.Checked = ((int)RegistryTools.GetSetting(LogWatcherApp.RegistryKey + @"\Options", "AutoClose", 0) != 0);

            if (Directory.Exists(logsDir))
            {
                if (menuItemOpenAll.Checked || menuItemOpenRecent.Checked)
                {
                    foreach (String logFile in Directory.GetFiles(logsDir, "ObjectError_*.txt"))
                    {
                        if (menuItemOpenRecent.Checked && File.GetLastWriteTime(logFile) < DateTime.Now.AddHours(-Properties.Settings.Default.RecentHours))
                        {
                            continue;
                        }

                        LoadErrorLog(logFile);
                    }
                }

                logDirWatcher.Path = logsDir;
                logDirWatcher.EnableRaisingEvents = true;
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            RegistryTools.SaveAppSettings(LogWatcherApp.RegistryKey, LogWatcherApp.AppVersionMajor, LogWatcherApp.AppVersionMinor);
            RegistryTools.SaveFormSettings(LogWatcherApp.RegistryKey, this);
        }

        private void OnFileOpening(object sender, EventArgs e)
        {
            menuItemCloseTab.Enabled = tabControl.SelectedTab != null;
            menuItemCloseTabAndDelete.Enabled = tabControl.SelectedTab != null;
        }

        private void OnExitClicked(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OnHelpClicked(object sender, EventArgs e)
        {
            new Sims2ToolsAboutDialog(LogWatcherApp.AppProduct).ShowDialog();
        }

        private void OnConfigurationClicked(object sender, EventArgs e)
        {
            Form config = new Sims2ToolsConfigDialog();

            if (config.ShowDialog() == DialogResult.OK)
            {
                // Perform any reload necessary after changing the objects.package location
            }
        }

        private void MyMruList_FileSelected(String logFilePath)
        {
            LoadErrorLog(logFilePath);
        }

        private void OnSelectClicked(object sender, EventArgs e)
        {
            selectFileDialog.InitialDirectory = logsDir;
            selectFileDialog.FileName = "ObjectError_*.txt";
            if (selectFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (String fileName in selectFileDialog.FileNames)
                {
                    LoadErrorLog(fileName);
                    MyMruList.AddFile(fileName);
                }
            }
        }

        private void LoadErrorLog(String logFilePath)
        {
            foreach (TabPage tab in tabControl.TabPages)
            {
                if (tab is LogTab logTab)
                {
                    if (logFilePath.Equals(logTab.LogFilePath))
                    {
                        tabControl.SelectedTab = logTab;
                        return;
                    }
                }
            }

            tabControl.Controls.Add(new LogTab(logFilePath));
            tabControl.SelectedIndex = tabControl.TabCount - 1;
        }

        private void OnTabChanged(object sender, TabControlEventArgs e)
        {
            if (tabControl.SelectedTab == null)
            {
                this.Text = $"{LogWatcherApp.AppName}";
            }
            else
            {
                this.Text = $"{LogWatcherApp.AppName} - {tabControl.SelectedTab.Text}";
            }
        }

        private void LogWatcher_DragEnter(object sender, DragEventArgs e)
        {
            DataObject data = e.Data as DataObject;

            if (data.ContainsFileDropList())
            {
                string[] rawFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (rawFiles != null)
                {
                    bool allOk = true;

                    foreach (string rawFile in rawFiles)
                    {
                        if (!Path.GetFileName(rawFile).StartsWith("ObjectError_"))
                        {
                            allOk = false;
                            break;
                        }
                    }

                    if (allOk)
                    {
                        e.Effect = DragDropEffects.Copy;
                    }
                }
            }
        }

        private void LogWatcher_DragDrop(object sender, DragEventArgs e)
        {
            DataObject data = e.Data as DataObject;

            if (data.ContainsFileDropList())
            {
                string[] rawFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (rawFiles != null)
                {
                    foreach (string rawFile in rawFiles)
                    {
                        LoadErrorLog(rawFile);
                    }
                }
            }
        }

        private void OnCloseTab(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab != null)
            {
                tabControl.TabPages.Remove(tabControl.SelectedTab);
            }
        }

        private void OnCloseTabAndDelete(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab != null)
            {
                String logFilePath = null;

                if (tabControl.SelectedTab is LogTab logTab)
                {
                    logFilePath = logTab.LogFilePath;
                }

                tabControl.TabPages.Remove(tabControl.SelectedTab);

                if (logFilePath != null)
                {
                    try
                    {
                        File.Delete(logFilePath);
                        MyMruList.RemoveFile(logFilePath);
                    }
                    catch (Exception) { }
                }
            }
        }

        private void OnOpenAllClicked(object sender, EventArgs e)
        {
            menuItemOpenRecent.Checked = false;

            RegistryTools.SaveSetting(LogWatcherApp.RegistryKey + @"\Options", "OpenAtStart", menuItemOpenAll.Checked ? "All" : menuItemOpenRecent.Checked ? "Recent" : "None");
        }

        private void OnOpenRecentClicked(object sender, EventArgs e)
        {
            menuItemOpenAll.Checked = false;

            RegistryTools.SaveSetting(LogWatcherApp.RegistryKey + @"\Options", "OpenAtStart", menuItemOpenAll.Checked ? "All" : menuItemOpenRecent.Checked ? "Recent" : "None");
        }

        private void OnLogFileCreated(object sender, FileSystemEventArgs e)
        {
            if (menuItemAutoOpen.Checked)
            {
                LoadErrorLog(e.FullPath);
            }
        }

        private void OnLogFileDeleted(object sender, FileSystemEventArgs e)
        {
            if (menuItemAutoClose.Checked)
            {
                foreach (TabPage tabPage in tabControl.TabPages)
                {
                    if (tabPage is LogTab logTab)
                    {
                        if (logTab.LogFilePath.Equals(e.FullPath))
                        {
                            tabControl.TabPages.Remove(logTab);
                            return;
                        }
                    }
                }
            }
        }

        private void OnLogFileChanged(object sender, FileSystemEventArgs e)
        {
            if (menuItemAutoUpdate.Checked)
            {
                foreach (TabPage tabPage in tabControl.TabPages)
                {
                    if (tabPage is LogTab logTab)
                    {
                        if (logTab.LogFilePath.Equals(e.FullPath))
                        {
                            // TODO - do we reload, open in a new tab, or what?
                            logTab.Reload();
                            tabControl.SelectedTab = logTab;
                            return;
                        }
                    }
                }
            }
        }

        private void OnLogFileRenamed(object sender, RenamedEventArgs e)
        {
            // TODO - do what with a renamed file?
        }

        private void OnAutoOpenClicked(object sender, EventArgs e)
        {
            RegistryTools.SaveSetting(LogWatcherApp.RegistryKey + @"\Options", "AutoOpen", menuItemAutoOpen.Checked ? 1 : 0);
        }

        private void OnAutoUpdateClicked(object sender, EventArgs e)
        {
            RegistryTools.SaveSetting(LogWatcherApp.RegistryKey + @"\Options", "AutoUpdate", menuItemAutoUpdate.Checked ? 1 : 0);
        }

        private void OnAutoCloseClicked(object sender, EventArgs e)
        {
            RegistryTools.SaveSetting(LogWatcherApp.RegistryKey + @"\Options", "AutoClose", menuItemAutoClose.Checked ? 1 : 0);
        }

        private void OnTabControlMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.menuContextTab.Show(this.tabControl, e.Location);
            }
        }

        private void OnTabContextMenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Point p = this.tabControl.PointToClient(Cursor.Position);

            for (int i = 0; i < this.tabControl.TabCount; i++)
            {
                Rectangle r = this.tabControl.GetTabRect(i);
                if (r.Contains(p))
                {
                    this.tabControl.SelectedIndex = i;
                    return;
                }
            }

            e.Cancel = true;
        }
    }
}
