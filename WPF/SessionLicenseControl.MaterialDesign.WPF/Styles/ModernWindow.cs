using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using MathCore.Annotations;
using SessionLicenseControl.WPF.Commands;
using SessionLicenseControl.WPF.Enums;
using SessionLicenseControl.WPF.Extensions;
using License = SessionLicenseControl.Licenses.License;

namespace SessionLicenseControl.WPF.Styles
{
    internal partial class ModernWindow
    {
        private void OnSizeSouth(object sender, MouseButtonEventArgs e) => OnSize(sender, SizingAction.South);
        private void OnSizeNorth(object sender, MouseButtonEventArgs e) => OnSize(sender, SizingAction.North);
        private void OnSizeEast(object sender, MouseButtonEventArgs e) => OnSize(sender, SizingAction.East);
        private void OnSizeWest(object sender, MouseButtonEventArgs e) => OnSize(sender, SizingAction.West);
        private void OnSizeNorthWest(object sender, MouseButtonEventArgs e) => OnSize(sender, SizingAction.NorthWest);
        private void OnSizeNorthEast(object sender, MouseButtonEventArgs e) => OnSize(sender, SizingAction.NorthEast);
        private void OnSizeSouthEast(object sender, MouseButtonEventArgs e) => OnSize(sender, SizingAction.SouthEast);
        private void OnSizeSouthWest(object sender, MouseButtonEventArgs e) => OnSize(sender, SizingAction.SouthWest);

        private void OnSize(object sender, SizingAction action)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed) return;
            sender.ForWindowFromTemplate(w =>
            {
                if (w.WindowState == WindowState.Normal)
                    DragSize(w.GetWindowHandle(), action);
            });
        }

        private void MaxButtonClick(object sender, RoutedEventArgs e) => sender.ForWindowFromTemplate(w => w.WindowState = w.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized);

        private void TitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
                MaxButtonClick(sender, e);
            else if (e.LeftButton == MouseButtonState.Pressed)
                sender.ForWindowFromTemplate(w => w.DragMove());
        }

        private void IconMouseLeftButtonDown(object Sender, MouseButtonEventArgs E)
        {
            if (E.ClickCount > 1)
                Sender.ForWindowFromTemplate(w => w.Close());
            else
                Sender.ForWindowFromTemplate(w => w.SendMessage(WM.SYSCOMMAND, SC.KEYMENU));
        }

        private void MinButtonClick(object Sender, RoutedEventArgs E) => Sender.ForWindowFromTemplate(w => w.WindowState = WindowState.Minimized);

        private void CloseButtonClick(object Sender, RoutedEventArgs E) => Sender.ForWindowFromTemplate(w => w.Close());

        private void TitleBarMouseMove(object sender, [NotNull] MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            sender.ForWindowFromTemplate(w =>
            {
                if (w.WindowState != WindowState.Maximized) return;
                w.BeginInit();
                const double adjustment = 40.0;
                var mouse1 = e.MouseDevice.GetPosition(w);
                var width1 = Math.Max(w.ActualWidth - 2 * adjustment, adjustment);
                w.WindowState = WindowState.Normal;
                var width2 = Math.Max(w.ActualWidth - 2 * adjustment, adjustment);
                w.Left = (mouse1.X - adjustment) * (1 - width2 / width1);
                w.Top = -7;
                w.EndInit();
                w.DragMove();
            });
        }

        #region P/Invoke

        private void DragSize(IntPtr handle, SizingAction SizingAction)
        {
            var _ = handle.SendMessage(WM.SYSCOMMAND, (IntPtr)((int)SC.SIZE + SizingAction), IntPtr.Zero);
            _ = handle.SendMessage(WM.LBUTTONUP, IntPtr.Zero, IntPtr.Zero);
        }

        #endregion

        public static Action OnLicenseLoaded;
        private static SessionLicenseController __LicenseController;
        public static SessionLicenseController LicenseController
        {
            get => __LicenseController;
            set
            {
                __LicenseController = value;
                OnLicenseLoaded?.Invoke();
            }
        }

        private static readonly byte[] __Data = { 82, 101, 97, 100, 121, 46 };
        public static DateTime? GetTrialDate(string LicenseFilePath,string Secret)
        {
            if (LicenseController is null)
            {
                LoadStyle("Check", LicenseFilePath,Secret,false,null);
            }

            return LicenseController.License.ExpirationDate;
        }

        public static string LoadStyle(string Status, string FilePath,string Secret, bool NeedStartNewSession, [CanBeNull] string UserName)
        {
            var c = new string(new byte[] { 82, 101, 97, 100, 121, 46 }.Select(b => (char)b).ToArray());
            if (!new string(__Data.Select(b => (char)b).ToArray()).Equals(Status, StringComparison.InvariantCultureIgnoreCase))
                return Status;

            var app = Application.Current;

            try
            {
                //Если есть файл лицензии
                if (File.Exists(FilePath))
                {
                    LicenseController = new SessionLicenseController(FilePath, Secret, NeedStartNewSession, UserName);
                    if (!LicenseController.ValidateLicense())
                        throw new LicenseException(typeof(License),LicenseController,$"Wrong license data: Expiration date - {LicenseController?.License?.ExpirationDate}, HDD - {LicenseController?.License?.HDDid}");
                    return Status;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            Action<string> l_checker = null;

            void Checker(ContentControl w, bool r)
            {
                if (!r)
                {
                    if ((w.Content as Grid)?.Name != "GrId") return;
                    var cc = ((Grid)w.Content).Children[0];
                    ((Grid)w.Content).Children.Remove(cc);
                    w.Content = cc;
                    ((UIElement)w.Content).Effect = null;
                    ((UIElement)w.Content).IsEnabled = true;
                    return;
                }

                if (w.Content is null) return;
                var content = (UIElement)w.Content;
                var grid = new Grid { Name = "GrId" };
                w.Content = grid;
                grid.Children.Add(content);
                content.Effect = new BlurEffect { Radius = 5 };
                content.IsEnabled = false;
                var panel = new GroupBox
                {
                    Header = "Input license row",
                    Width = 300,
                    Height = 200,
                    //Padding = new Thickness(5),
                    //Margin = new Thickness(5),
                    Style = Application.Current.FindResource("MaterialDesignCardGroupBox") as Style

                };
                grid.Children.Add(new Border
                {
                    Child = panel,
                    CornerRadius = new CornerRadius(3),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Effect = new DropShadowEffect(),
                    Background = Application.Current.FindResource("MaterialDesignPaper") as Brush
                });
                var fields = new DockPanel();
                panel.Content = fields;
                var license_text = new TextBox
                {
                    Margin = new Thickness(left: 0, top: 5, right: 0, bottom: 5),
                    TextWrapping = TextWrapping.Wrap,
                    Style = Application.Current.FindResource("MaterialDesignFilledTextBox") as Style,
                    VerticalAlignment = VerticalAlignment.Bottom
                };
                fields.Children.Add(ObjectEx.Init(
                    ObjectEx.Init(
                        ObjectEx.Init(
                            new StackPanel(),
                            p => DockPanel.SetDock(p, Dock.Top)),
                        p => p.Children.Add(new TextBlock
                        {
                            Text = "ID:",
                            Style = Application.Current.FindResource("MaterialDesignTextBlock") as Style
                        })),
                    p => p.Children.Add(
                        new TextBox
                        {
                            IsReadOnly = true,
                            Text = SessionLicenseControl.Licenses.License.GetThisPcHddSerialNumber(),
                            Style = Application.Current.FindResource("MaterialDesignTextBox") as Style,
                        })));
                fields.Children.Add(
                    ObjectEx.Init(
                            ObjectEx.Init(
                                new Button
                                {
                                    Style = Application.Current.FindResource("MaterialDesignPaperButton") as Style,
                                    Content = /*new PackIcon(){Kind = PackIconKind.PresenceEnter, Width = 20,Height = 20},*/
                                    new TextBlock()
                                    {
                                        Text = "Enter",
                                        Foreground = new SolidColorBrush(Colors.DarkGoldenrod),
                                        Style = Application.Current.FindResource("MaterialDesignTextBlock") as Style
                                    },
                                    Margin = new Thickness(left: 0, top: 5, right: 0, bottom: 5),
                                    //Padding = new Thickness(top: 15, bottom: 15, left: 0, right: 0),
                                    Command = new LamdaCommand(o => l_checker((string)o), o => !string.IsNullOrWhiteSpace(o as string))
                                }, b => DockPanel.SetDock(b, Dock.Bottom)), b => b.SetBinding(ButtonBase.CommandParameterProperty, new Binding("Text") { Source = license_text })));
                fields.Children.Add(license_text);
            }

            l_checker = s =>
            {
                //Сохраняем файл лицензии
                try
                {
                    LicenseController = new SessionLicenseController(s, Secret, FilePath, NeedStartNewSession, UserName);
                    if (!LicenseController.ValidateLicense())
                        throw new LicenseException(typeof(License), LicenseController, $"Wrong license data: Expiration date - {LicenseController?.License?.ExpirationDate}, HDD - {LicenseController?.License?.HDDid}");
                }
                catch
                {
                    return;
                }
                foreach (var window in app.GetValue(a => a.Windows).OfType<Window>())
                    window.Invoke(w => Checker(w, false));
            };

            foreach (var window in app.GetValue(a => a.Windows).OfType<Window>())
                window.Invoke(w => Checker(w, true));
            return Status;
        }
    }
}
