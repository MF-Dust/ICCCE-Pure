using Ink_Canvas.Helpers;
using Ink_Canvas.Windows.SettingsViews.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace Ink_Canvas.Windows.SettingsViews.Pages
{
    public partial class PowerPointPage : Page
    {
        private bool _isLoaded = false;
        private DelayAction _sliderDelayAction = new DelayAction();

        public PowerPointPage()
        {
            InitializeComponent();
            Loaded += PowerPointPage_Loaded;
            Unloaded += PowerPointPage_Unloaded;
        }

        private void PowerPointPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
            _isLoaded = true;
            UpdateAllSliderTexts();
            SliderTouchHelper.AddTouchSupportToAllSliders(this);
        }

        private void UpdateAllSliderTexts()
        {
        }

        private void UpdateSliderText(Slider slider, TextBlock textBlock, string format)
        {
            if (slider == null || textBlock == null) return;
            textBlock.Text = string.Format(format, slider.Value);
        }

        private void PowerPointPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = false;
        }

        private void LoadSettings()
        {
            _isLoaded = false;
            var ppt = SettingsManager.Settings.PowerPointSettings;

            CardSupportPowerPoint.IsOn = ppt.PowerPointSupport;
            CardSkipAnimationsWhenGoNext.IsOn = ppt.SkipAnimationsWhenGoNext;
            CardSupportWPS.IsOn = ppt.IsSupportWPS;
            CardEnableWppProcessKill.IsOn = ppt.EnableWppProcessKill;

            CardEnablePPTButtonPageClickable.IsOn = ppt.EnablePPTButtonPageClickable;
            ToggleSwitchEnablePPTButtonEnhancedPreview.IsOn = ppt.EnablePPTButtonEnhancedPreview;
            ToggleSwitchPPTEnhancedPreviewLoadingAnimation.IsOn = ppt.ShowPPTEnhancedPreviewLoadingAnimation;
            CardEnablePPTButtonLongPressPageTurn.IsOn = ppt.EnablePPTButtonLongPressPageTurn;

            CardShowCanvasAtNewSlideShow.IsOn = ppt.IsShowCanvasAtNewSlideShow;
            CardEnableSmartMode.IsOn = ppt.EnableSmartMode;

            CardEnableTwoFingerGestureInPresentationMode.IsOn = ppt.IsEnableTwoFingerGestureInPresentationMode;
            CardEnableFingerGestureSlideShowControl.IsOn = ppt.IsEnableFingerGestureSlideShowControl;
            CardShowPPTSidebarByDefault.IsOn = ppt.ShowPPTSidebarByDefault;
            CardShowPPTModePrompt.IsOn = ppt.ShowPPTModePrompt;

            CardAutoSaveScreenShotInPowerPoint.IsOn = ppt.IsAutoSaveScreenShotInPowerPoint;
            CardAutoSaveStrokesInPowerPoint.IsOn = ppt.IsAutoSaveStrokesInPowerPoint;

            CardNotifyPreviousPage.IsOn = ppt.IsNotifyPreviousPage;
            CardAlwaysGoToFirstPageOnReenter.IsOn = ppt.IsAlwaysGoToFirstPageOnReenter;
            CardNotifyHiddenPage.IsOn = ppt.IsNotifyHiddenPage;
            CardNotifyAutoPlayPresentation.IsOn = ppt.IsNotifyAutoPlayPresentation;

            _isLoaded = true;
        }

        #region PPT Basic

        private void ToggleSwitchSupportPowerPoint_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            var ppt = SettingsManager.Settings.PowerPointSettings;
            ppt.PowerPointSupport = CardSupportPowerPoint.IsOn;
            if (!ppt.PowerPointSupport && ppt.IsSupportWPS)
            {
                ppt.IsSupportWPS = false;
                CardSupportWPS.IsOn = false;
            }
            SettingsManager.SaveSettingsToFile();
            SettingsActionHub.OnPPTSupportChanged(CardSupportPowerPoint.IsOn);
        }

        private void ToggleSwitchSkipAnimationsWhenGoNext_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            SettingsManager.Settings.PowerPointSettings.SkipAnimationsWhenGoNext = CardSkipAnimationsWhenGoNext.IsOn;
            SettingsManager.SaveSettingsToFile();
            SettingsActionHub.OnSkipAnimationsWhenGoNextChanged(CardSkipAnimationsWhenGoNext.IsOn);
        }

        private void ToggleSwitchSupportWPS_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            var ppt = SettingsManager.Settings.PowerPointSettings;
            ppt.IsSupportWPS = CardSupportWPS.IsOn;
            if (ppt.IsSupportWPS)
            {
                if (!ppt.PowerPointSupport)
                {
                    ppt.PowerPointSupport = true;
                    CardSupportPowerPoint.IsOn = true;
                }
            }
            SettingsManager.SaveSettingsToFile();
            SettingsActionHub.OnSupportWPSChanged();
        }

        private void ToggleSwitchEnableWppProcessKill_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            SettingsManager.Settings.PowerPointSettings.EnableWppProcessKill = CardEnableWppProcessKill.IsOn;
            SettingsManager.SaveSettingsToFile();
        }

        #endregion

        #region PPT Flip Buttons

        private void OnCardClicked(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is SettingsWindow settingsWindow)
            {
                settingsWindow.NavigateToPage("PPTPageFlipPreviewPage");
            }
        }

        private void ToggleSwitchShowPPTSidebarByDefault_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            SettingsManager.Settings.PowerPointSettings.ShowPPTSidebarByDefault = CardShowPPTSidebarByDefault.IsOn;
            SettingsManager.SaveSettingsToFile();
            SettingsActionHub.OnShowPPTSidebarByDefaultChanged();
        }

        private void ToggleSwitchShowPPTModePrompt_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            SettingsManager.Settings.PowerPointSettings.ShowPPTModePrompt = CardShowPPTModePrompt.IsOn;
            SettingsManager.SaveSettingsToFile();
        }

        private void ToggleSwitchEnablePPTButtonPageClickable_OnToggled(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            SettingsManager.Settings.PowerPointSettings.EnablePPTButtonPageClickable = CardEnablePPTButtonPageClickable.IsOn;
            SettingsManager.SaveSettingsToFile();
        }

        private void ToggleSwitchEnablePPTButtonEnhancedPreview_OnToggled(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            SettingsManager.Settings.PowerPointSettings.EnablePPTButtonEnhancedPreview = ToggleSwitchEnablePPTButtonEnhancedPreview.IsOn;
            SettingsManager.SaveSettingsToFile();
        }

        private void ToggleSwitchPPTEnhancedPreviewLoadingAnimation_OnToggled(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            SettingsManager.Settings.PowerPointSettings.ShowPPTEnhancedPreviewLoadingAnimation = ToggleSwitchPPTEnhancedPreviewLoadingAnimation.IsOn;
            SettingsManager.SaveSettingsToFile();
        }

        private void ToggleSwitchEnablePPTButtonLongPressPageTurn_OnToggled(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            SettingsManager.Settings.PowerPointSettings.EnablePPTButtonLongPressPageTurn = CardEnablePPTButtonLongPressPageTurn.IsOn;
            SettingsManager.SaveSettingsToFile();
        }

        #endregion



        #region PPT SlideShow Entry & Gesture

        private void ToggleSwitchEnableSmartMode_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            SettingsManager.Settings.PowerPointSettings.EnableSmartMode = CardEnableSmartMode.IsOn;
            SettingsManager.SaveSettingsToFile();
        }

        private void ToggleSwitchShowCanvasAtNewSlideShow_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            SettingsManager.Settings.PowerPointSettings.IsShowCanvasAtNewSlideShow = CardShowCanvasAtNewSlideShow.IsOn;
            SettingsManager.SaveSettingsToFile();
        }

        private void ToggleSwitchEnableTwoFingerGestureInPresentationMode_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            SettingsManager.Settings.PowerPointSettings.IsEnableTwoFingerGestureInPresentationMode = CardEnableTwoFingerGestureInPresentationMode.IsOn;
            SettingsManager.SaveSettingsToFile();
        }

        private void ToggleSwitchEnableFingerGestureSlideShowControl_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            SettingsManager.Settings.PowerPointSettings.IsEnableFingerGestureSlideShowControl = CardEnableFingerGestureSlideShowControl.IsOn;
            SettingsManager.SaveSettingsToFile();
        }

        #endregion

        #region PPT Auto Save & Notifications

        private void ToggleSwitchAutoSaveScreenShotInPowerPoint_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            SettingsManager.Settings.PowerPointSettings.IsAutoSaveScreenShotInPowerPoint = CardAutoSaveScreenShotInPowerPoint.IsOn;
            SettingsManager.SaveSettingsToFile();
        }

        private void ToggleSwitchAutoSaveStrokesInPowerPoint_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            SettingsManager.Settings.PowerPointSettings.IsAutoSaveStrokesInPowerPoint = CardAutoSaveStrokesInPowerPoint.IsOn;
            SettingsManager.SaveSettingsToFile();
        }

        private void ToggleSwitchNotifyPreviousPage_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            SettingsManager.Settings.PowerPointSettings.IsNotifyPreviousPage = CardNotifyPreviousPage.IsOn;
            SettingsManager.SaveSettingsToFile();
        }

        private void ToggleSwitchAlwaysGoToFirstPageOnReenter_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            SettingsManager.Settings.PowerPointSettings.IsAlwaysGoToFirstPageOnReenter = CardAlwaysGoToFirstPageOnReenter.IsOn;
            SettingsManager.SaveSettingsToFile();
        }

        private void ToggleSwitchNotifyHiddenPage_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            SettingsManager.Settings.PowerPointSettings.IsNotifyHiddenPage = CardNotifyHiddenPage.IsOn;
            SettingsManager.SaveSettingsToFile();
        }

        private void ToggleSwitchNotifyAutoPlayPresentation_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            SettingsManager.Settings.PowerPointSettings.IsNotifyAutoPlayPresentation = CardNotifyAutoPlayPresentation.IsOn;
            SettingsManager.SaveSettingsToFile();
        }

        #endregion
    }
}
