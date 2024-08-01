using Editor.MyControl;
using Editor.MyControl.CustomTabControl;
using Editor.Utils;
using Microsoft.Win32;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Editor
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private bool isCtrlDown = false;

        private readonly ActionTabViewModal customTabView;

        private Thread loadSaveThread;
        private bool isFormClosing;

        public MainWindow()
        {
            InitializeComponent();

            customTabView = new ActionTabViewModal();
            customTabView.Bind(tabControl);

            infoBox.Visibility = Visibility.Collapsed;

            isFormClosing = false;
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenPack();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            CreatePackControl();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            PreSavePack();
        }

        private void PreSavePack()
        {
            var selected = tabControl.SelectedContent;

            if (selected == null)
            {
                return;
            }

            var packageControl = (PackageControl)((ActionTabItem)selected).Content;

            SavePack(packageControl);
        }

        private void SavePack(PackageControl packageControl)
        {
            var pack = packageControl.GetData();

            if (!packageControl.IsPathAvailable)
            {
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "pack files |*" + DataStore.Utils.PackUtils.PackManager.MyPackExtension,
                    FileName = TextUtils.Transliteration(pack.Name),
                };

                if (sfd.ShowDialog() != true)
                {
                    return;
                }

                packageControl.PackPath = sfd.FileName;
            }



            this.IsEnabled = false;
            infoBox.Visibility = Visibility.Visible;
            loadSaveThread = new Thread(() =>
            {
                try
                {
                    DataStore.Utils.PackUtils.PackConverter.SavePack(
                        pack,
                        packageControl.PackPath,
                        (text) =>
                        {
                            infoText.Dispatcher.Invoke(new Action(() =>
                            {
                                infoText.Text = string.Format("Сохранение \n {0}", text);
                            }));
                        });
                }
                catch (DataStore.Exceptions.BadFileException ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    if (!isFormClosing)
                    {
                        try
                        {
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                this.IsEnabled = true;
                                infoBox.Visibility = Visibility.Collapsed;
                            }));
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            });
            loadSaveThread.Start();
        }

        private void OpenPack()
        {
            if (!CheckTabCount())
            {
                return;
            }

            OpenFileDialog fd = new OpenFileDialog
            {
                Filter = "pack files |*" + DataStore.Utils.PackUtils.PackManager.SiqPackExtension + "; *" + DataStore.Utils.PackUtils.PackManager.MyPackExtension
            };

            if (fd.ShowDialog() == true)
            {
                CreatePackControl(fd.FileName);
                GC.Collect();
            }
        }

        private bool CheckTabCount()
        {
            if (customTabView.TabCount > 4)
            {
                new DialogForm.MessageBox("Открыто максимальное количество вкладок").Show();
                return false;
            }
            return true;
        }

        private void CreatePackControl(string packPath = "")
        {
            if (!CheckTabCount())
            {
                return;
            }

            var packageControl = new PackageControl();

            var header = new TextBlock
            {
                Text = "новый",
                Width = 100
            };

            packageControl.PackageNameChanged += () =>
            {
                packageControl.Dispatcher.Invoke(new Action(() =>
                {
                    var text = packageControl.PackName;

                    if (text.Length > 10)
                    {
                        text = text.Substring(0, 10) + "...";
                    }

                    if (!packageControl.IsSaved)
                    {
                        text += " *";
                    }

                    var currentText = header.Text;

                    if (!currentText.Equals(text))
                    {
                        header.Text = text;
                    }
                }));
            };

            packageControl.ContentChanged += () =>
            {
                packageControl.PackageNameChanged?.Invoke();
            };

            var tabId = customTabView.Add(header, packageControl);
            tabControl.SelectedIndex = tabControl.Items.Count - 1;

            if (packPath.Equals(""))
            {
                packageControl.ForceUpdate();
            }
            else
            {
                this.IsEnabled = false;
                infoBox.Visibility = Visibility.Visible;


                loadSaveThread = new Thread(() =>
                {
                    try
                    {
                        packageControl.ParsePack(packPath, (text) =>
                        {
                            infoText.Dispatcher.Invoke(new Action(() =>
                            {
                                infoText.Text = string.Format("Загрузка \n {0}", text);
                            }));
                        });
                    }
                    catch (DataStore.Exceptions.BadFileException ex)
                    {
                        Console.WriteLine(ex.ToString());
                        if (!isFormClosing)
                        {
                            try
                            {
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    customTabView.Remove(tabId);
                                    new DialogForm.MessageBox("Ошибка при открытии").Show();
                                }));
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                    finally
                    {
                        if (!isFormClosing)
                        {
                            try
                            {
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    this.IsEnabled = true;
                                    infoBox.Visibility = Visibility.Collapsed;
                                }));
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                });
                loadSaveThread.Start();
            }

        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                isCtrlDown = false;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                isCtrlDown = true;

            }
            else if (isCtrlDown)
            {
                if (e.Key == Key.S)
                {
                    PreSavePack();
                    isCtrlDown = false;
                }
                else if (e.Key == Key.O)
                {
                    OpenPack();
                    isCtrlDown = false;
                }
                else if (e.Key == Key.N)
                {
                    CreatePackControl();
                    isCtrlDown = false;
                }
            }
        }

        private void OnCloseTab(PackageControl control)
        {
            if (!control.IsSaved)
            {
                var name = control.PackName;
                if (name.Length > 20)
                {
                    name = name.Substring(0, 20) + "...";
                }

                var dialog = new DialogForm.YesNoDialog("Изменения не сохранены. Сохранить?", name);

                if (dialog.ShowDialog() == true)
                {
                    if (dialog.Result == DialogForm.Utils.DialogResult.Yes)
                    {
                        SavePack(control);
                    }
                }
            }

            control.Clear();
        }

        private void BtnCloseTab_Click(object sender, RoutedEventArgs e)
        {
            var id = ((Button)sender).DataContext.ToString();
            var control = (PackageControl)customTabView.GetUserControl(id);

            OnCloseTab(control);

            customTabView.Remove(id);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isFormClosing = true;
            loadSaveThread?.Abort();
            loadSaveThread?.Join();

            foreach (PackageControl control in customTabView.GetAllUserControls())
            {
                OnCloseTab(control);
            }
        }

    }
}
