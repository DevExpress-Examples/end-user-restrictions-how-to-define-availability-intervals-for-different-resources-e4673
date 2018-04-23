﻿using System;
using System.Windows.Media;
using DevExpress.XtraScheduler;

namespace SchedulerResourcesAvailabilitiesWpf {
    public class CellColorConverter : System.Windows.Markup.MarkupExtension, System.Windows.Data.IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            DateTime intervalStart = System.Convert.ToDateTime(values[0]);
            DateTime intervalEnd = System.Convert.ToDateTime(values[1]);
            string resourceId = System.Convert.ToString(values[2]);
            Brush defaultBrush = (Brush)values[3];

            if (intervalStart > intervalEnd) {
                DateTime temp = intervalStart;
                intervalStart = intervalEnd;
                intervalEnd = temp;
            }

            bool available = ResourcesAvailabilities.IsIntervalAvailableForResource(
                resourceId, new TimeInterval(intervalStart, intervalEnd));

            if (available)
                return defaultBrush;
            else
                return defaultBrush.SetBrightness(0.85);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException("DebugConverterFormatter::ConvertBack");
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }

    public class HSBColor {
        public double H { get; set; }
        public double S { get; set; }
        public double B { get; set; }
        public byte A { get; set; }

        public static HSBColor FromColor(Color rgbColor) {
            HSBColor result = new HSBColor();

            // preserve alpha
            result.A = rgbColor.A;

            // convert R, G, B to numbers from 0 to 1
            double r = rgbColor.R / 255d;
            double g = rgbColor.G / 255d;
            double b = rgbColor.B / 255d;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));

            // hue
            if (max == min)
                result.H = 0;
            else if (max == r)
                result.H = (60 * (g - b) / (max - min) + 360) % 360;
            else if (max == g)
                result.H = 60 * (b - r) / (max - min) + 120;
            else
                result.H = 60 * (r - g) / (max - min) + 240;

            // saturation
            if (max == 0)
                result.S = 0;
            else
                result.S = 1 - min / max;

            // brightness
            result.B = max;

            return result;
        }

        public Color ToColor() {
            Color result = new Color();

            result.A = this.A;

            int hi = (int)Math.Floor(this.H / 60) % 6;
            double f = this.H / 60 - Math.Floor(this.H / 60);

            double p = this.B * (1 - this.S);
            double q = this.B * (1 - f * this.S);
            double t = this.B * (1 - (1 - f) * this.S);

            switch (hi) {
                case 0:
                    result.R = (byte)(this.B * 255);
                    result.G = (byte)(t * 255);
                    result.B = (byte)(p * 255);
                    break;
                case 1:
                    result.R = (byte)(q * 255);
                    result.G = (byte)(this.B * 255);
                    result.B = (byte)(p * 255);
                    break;
                case 2:
                    result.R = (byte)(p * 255);
                    result.G = (byte)(this.B * 255);
                    result.B = (byte)(t * 255);
                    break;
                case 3:
                    result.R = (byte)(p * 255);
                    result.G = (byte)(q * 255);
                    result.B = (byte)(this.B * 255);
                    break;
                case 4:
                    result.R = (byte)(t * 255);
                    result.G = (byte)(p * 255);
                    result.B = (byte)(this.B * 255);
                    break;
                case 5:
                    result.R = (byte)(this.B * 255);
                    result.G = (byte)(p * 255);
                    result.B = (byte)(q * 255);
                    break;
            }

            return result;
        }
    }

    public static class HelperExtensions {
        public static Brush SetBrightness(this Brush original, double brightness) {
            if (brightness < 0 || brightness > 1)
                throw new ArgumentOutOfRangeException("brightness", "brightness should be between 0 and 1");

            Brush result;

            if (original is SolidColorBrush) {
                HSBColor hsb = HSBColor.FromColor(((SolidColorBrush)original).Color);
                hsb.B = brightness;
                result = new SolidColorBrush(hsb.ToColor());
            }
            else if (original is GradientBrush) {
                result = original.Clone();
                // change brightness of every gradient stop
                foreach (GradientStop gs in ((GradientBrush)result).GradientStops) {
                    HSBColor hsb = HSBColor.FromColor(gs.Color);
                    hsb.B = brightness;
                    gs.Color = hsb.ToColor();
                }
            }
            else {
                result = original.Clone();
            }

            return result;
        }
    }
}