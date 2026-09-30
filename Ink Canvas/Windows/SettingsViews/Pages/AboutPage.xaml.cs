using Ink_Canvas.Helpers;
using Ink_Canvas.Windows.FeedbackPages;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace Ink_Canvas.Windows.SettingsViews.Pages
{
    public class AvatarItem
    {
        public string AvatarPath { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
    }

    public partial class AboutPage : iNKORE.UI.WPF.Modern.Controls.Page
    {
        public AboutPage()
        {
            InitializeComponent();
            Loaded += AboutPage_Loaded;
            InitializeAvatarData();
        }

        private void InitializeAvatarData()
        {
            var developers = new ObservableCollection<AvatarItem>
            {
                new AvatarItem { AvatarPath = "/Resources/DeveloperAvatars/CJKmkp.jpg", Name = "CJK_mkp", Role = LocalizationHelper.GetString("About_Dev_ICCCE") },
                new AvatarItem { AvatarPath = "/Resources/DeveloperAvatars/dubi906w.jpg", Name = "Dubi906w", Role = LocalizationHelper.GetString("About_Dev_ICC") },
                new AvatarItem { AvatarPath = "/Resources/DeveloperAvatars/ChangSakura.png", Name = "ChangSakura", Role = LocalizationHelper.GetString("About_Dev_ICA") },
                new AvatarItem { AvatarPath = "/Resources/DeveloperAvatars/WXRIW.png", Name = "WXRIW", Role = LocalizationHelper.GetString("About_Dev_InkCanvas") }
            };

            var contributors = new ObservableCollection<AvatarItem>
            {
                new AvatarItem { AvatarPath = "/Resources/DeveloperAvatars/RaspberryKan.jpg", Name = "Raspberry Kan" },
                new AvatarItem { AvatarPath = "/Resources/DeveloperAvatars/kengwang.png", Name = "Kengwang" },
                new AvatarItem { AvatarPath = "/Resources/DeveloperAvatars/jiajiaxd.jpg", Name = "Charles Jia" },
                new AvatarItem { AvatarPath = "/Resources/DeveloperAvatars/clover-yan.png", Name = "clover_yan" },
                new AvatarItem { AvatarPath = "/Resources/DeveloperAvatars/NetheriteBowl.png", Name = "Netherite_Bowl" },
                new AvatarItem { AvatarPath = "/Resources/DeveloperAvatars/NotYoojun.png", Name = "Yoojun Zhou" },
                new AvatarItem { AvatarPath = "/Resources/DeveloperAvatars/yuwenhui2020.png", Name = "YuWenHui2020" },
                new AvatarItem { AvatarPath = "/Resources/DeveloperAvatars/STBBRD.png", Name = "ZongziTEK" },
                new AvatarItem { AvatarPath = "/Resources/DeveloperAvatars/aaaaaaccd.jpg", Name = "Aesthed" },
                new AvatarItem { AvatarPath = "/Resources/DeveloperAvatars/wwei.png", Name = "Wei" },
                new AvatarItem { AvatarPath = "/Resources/DeveloperAvatars/Alan-CRL.png", Name = "Alan-CRL" },
                new AvatarItem { AvatarPath = "/Resources/DeveloperAvatars/PrefacedCorg.jpg", Name = "PrefacedCorg" },
                new AvatarItem { AvatarPath = "/Resources/DeveloperAvatars/PANDA-JSR.jpg", Name = "PANDA-JSR" },
                new AvatarItem { AvatarPath = "/Resources/DeveloperAvatars/Super-Yyt.png", Name = "zhaishis(Super-Yyt)" },
                new AvatarItem { AvatarPath = "/Resources/DeveloperAvatars/CreeperAWA.png", Name = "CreeperAWA" },
                new AvatarItem { AvatarPath = "/Resources/DeveloperAvatars/AstrZero.png", Name = "AstrZero" }
            };

            DeveloperItemsControl.ItemsSource = developers;
            ContributorItemsControl.ItemsSource = contributors;
        }

        private void AboutPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                AppVersionTextBlock.Text = version.Major + "." + version.Minor + "." + version.Build + "." + version.Revision;
                var informationalVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                if (informationalVersion != null)
                {
                    string infoVersion = informationalVersion.InformationalVersion;
                    int lastDotIndex = infoVersion.LastIndexOf('.');
                    if (lastDotIndex >= 0 && lastDotIndex < infoVersion.Length - 7)
                    {
                        AppVersionTextBlock.Text += " (" + infoVersion.Substring(lastDotIndex + 1) + ")";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"加载关于页面设置时出错: {ex.Message}");
            }
        }

        private void BtnReportIssue_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var feedbackWindow = new FeedbackWindow();
                feedbackWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"打开反馈窗口失败: {ex.Message}");
            }
        }

    }
}
