﻿//////////////////////////////////////////////
// 2006 - Microsoft 
//        https://blogs.msdn.microsoft.com/wpfsdk/2006/10/26/uncommon-dialogs-font-chooser-color-picker-dialogs/ 
//
// 2019 - Modified and adapted by Derek Tremblay (derektremblay666@gmail.com)
//////////////////////////////////////////////

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace Microsoft.Samples.CustomControls
{
    public class SpectrumSlider : Slider
    {
        #region Private Fields
        private const string _spectrumDisplayName = "PART_SpectrumDisplay";
        private Rectangle _spectrumDisplay;
        private LinearGradientBrush _pickerBrush;
        #endregion

        static SpectrumSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SpectrumSlider),
                new FrameworkPropertyMetadata(typeof(SpectrumSlider)));
        }
        
        #region Public Properties
        public Color SelectedColor
        {
            get => (Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }
        #endregion

        #region Dependency Property Fields
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(SpectrumSlider),
                new PropertyMetadata(Colors.Transparent));
        #endregion
        
        #region Public Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _spectrumDisplay = GetTemplateChild(_spectrumDisplayName) as Rectangle;
            UpdateColorSpectrum();
            OnValueChanged(Double.NaN, Value);
        }
        #endregion

        #region Protected Methods
        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            Color theColor = ColorUtilities.ConvertHsvToRgb(360 - newValue, 1, 1);
            SetValue(SelectedColorProperty, theColor);
        }
        #endregion

        #region Private Methods

        private void UpdateColorSpectrum()
        {
            if (_spectrumDisplay == null) return;
            CreateSpectrum();
        }
               
        private void CreateSpectrum()
        {
            _pickerBrush = new LinearGradientBrush();
            _pickerBrush.StartPoint = new Point(0.5, 0);
            _pickerBrush.EndPoint = new Point(0.5, 1);
            _pickerBrush.ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation;

            var colorsList = ColorUtilities.GenerateHsvSpectrum();
            var stopIncrement = (double)1 / colorsList.Count;

            int i;
            for (i = 0; i < colorsList.Count; i++)
                _pickerBrush.GradientStops.Add(new GradientStop(colorsList[i], i * stopIncrement));

            _pickerBrush.GradientStops[i - 1].Offset = 1.0;
            _spectrumDisplay.Fill = _pickerBrush;
        }
        #endregion
    }
}