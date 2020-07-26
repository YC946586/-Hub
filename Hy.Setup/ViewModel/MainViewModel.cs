using GalaSoft.MvvmLight;
using HY.MAIN.Properties;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace Hy.Setup.ViewModel
{

    public class MainViewModel : baseViewModel
    {
        public MainViewModel()
        {
            ////���.net�汾 ���û�а�װ ��ȥ����
            if (!Tool.CheckNetLanguage())
            {
                Process.Start(@"https://dotnet.microsoft.com/download/thank-you/net452?survey=false");
                MessageBox.Show("��⵽����δ��װ.NET4.5,���Ȱ�װ.NET4.5", "��ʾ", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            //���VC++2013
            //if (!Tool.CheckVc2013())
            //{
            //    Process.Start(@"https://www.microsoft.com/en-us/download/confirmation.aspx?id=40784");

            //    MessageBox.Show("��⵽����δ��װVC++2013,���Ȱ�װVC++2013", "��ʾ", MessageBoxButton.OK, MessageBoxImage.Information);
            //    return;
            //}
            //��ȡ�����̷�
            var drive = DriveInfo.GetDrives();
            if (drive.Length != 0)
            {
                var driveDate = drive.Where(s => s.IsReady).ToList();
                if (driveDate.Any())
                {
                    if (driveDate.Count > 1)
                    {
                        PageCollection.StrupPath = driveDate[1].Name + "HyInstallPackage";
                    }
                    else
                    {
                        PageCollection.StrupPath = driveDate[0].Name + "HyInstallPackage";
                    }
                }
            }
        }

        /// <summary>
        /// ѡ��װĿ¼
        /// </summary>
        public override void Browse()
        {
            try
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "��ѡ��װ·��";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    PageCollection.StrupPath = fbd.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// ��ʼ��ѹ ʵ�ְ�װ
        /// </summary>
        public override void GetAllDirFiles()
        {
            try
            {
                //�������ѹ���û�ָ��Ŀ¼
                var filesPath = Resources.Release;
                Extract(filesPath);
                Adddesktop();
            }
            catch (Exception ex)
            {
                _notsucces = "ʧ��";
                Msg = ex.Message;
                MessageBox.Show(ex.ToString(), "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// ��������ͼ��
        /// </summary>
        private void Adddesktop()
        {
            try
            {
                //ɾ��ע�������
                DeleteStartMenuShortcuts(PageCollection.StrupPath);
                //��ӿ�ʼ�˵���ݷ�ʽ
                RegistryKey hkeyCurrentUser =
                    Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders");

                if (hkeyCurrentUser != null)
                {

                    string programsPath = hkeyCurrentUser.GetValue("Programs").ToString(); //��ȡ��ʼ�˵������ļ���·��
                    Directory.CreateDirectory(programsPath + @"\��ӥHub"); //�ڳ����ļ����д�����ݷ�ʽ���ļ���

                    PageCollection.Message = "��ӿ�ʼ�˵���ݷ�ʽ";

                    Tool.CreateShortcut(programsPath + @"\��ӥHub.lnk", PageCollection.StrupPath, appName);

                    PageCollection.Message = "���ж��Ŀ¼";
                    Tool.CreateShortcut(programsPath + @"\��ӥHub.lnk", PageCollection.StrupPath,
                        uninstallName); //����ж�ؿ�ݷ�ʽ
                    //��������ݷ�ʽ
                    string desktopPath = hkeyCurrentUser.GetValue("Desktop").ToString(); //��ȡ�����ļ���·��
                    PageCollection.Message = "�������ͼ��";
                    Tool.CreateShortcut(desktopPath + @"\��ӥHub.lnk", PageCollection.StrupPath, appName); //������ݷ�ʽ

                    PageCollection.Schedule = 100;
                    PageCollection.Plah = "100";
                    PageCollection.GridHide = 3;
                    PageCollection.Message = "��ӥHub��װ���";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// ɾ����ʼ����ݷ�ʽ
        /// </summary>
        /// <param name="bBase"></param>
        /// <returns></returns>
        public override void DeleteStartMenuShortcuts(string stpPath)
        {
            try
            {
                RegistryKey cuKey = Registry.LocalMachine;
                RegistryKey cnctkjptKey = cuKey.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall", true);

                //��ȡAutoCAD�����ݷ�ʽ�������ϵ�·��
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string pathCom = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

                //����������*.lnk�ļ��ļ���
                string[] items = Directory.GetFiles(path, "*.lnk");
                string[] itemsCom = Directory.GetFiles(pathCom, "*.lnk");

                #region ɾ��Uninstallע���

                if (cnctkjptKey != null)
                {
                    foreach (string aimKey in cnctkjptKey.GetSubKeyNames())
                    {
                        if (aimKey == "HyInstallPackage")
                            cnctkjptKey.DeleteSubKeyTree("HyInstallPackage");
                    }
                    cnctkjptKey.Close();
                }

                cuKey.Close();


                #endregion

                #region ɾ�������ݷ�ʽ

                foreach (string item in items)
                {
                    Console.WriteLine(item);
                    if (item.Contains("��ӥHub") && item.Contains(".lnk"))
                    {
                        File.Delete(item);
                    }
                    else if (item.Contains("ж�غ�ӥHub") && item.Contains(".lnk"))
                    {
                        File.Delete(item);
                    }
                }
                foreach (string item in itemsCom)
                {
                    Console.WriteLine(item);
                    if (item.Contains("��ӥHub") && item.Contains(".lnk"))
                    {
                        File.Delete(item);
                    }
                    else if (item.Contains("ж�غ�ӥHub") && item.Contains(".lnk"))
                    {
                        File.Delete(item);
                    }

                }

                #endregion


            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}