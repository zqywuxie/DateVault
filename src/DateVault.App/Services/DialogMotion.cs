using Wpf = System.Windows;
using WpfAnimation = System.Windows.Media.Animation;
using WpfMedia = System.Windows.Media;

namespace DateVault.App.Services;

public static class DialogMotion
{
    public static void Attach(Wpf.Window window)
    {
        window.Opacity = 0;

        window.ContentRendered += (_, _) =>
        {
            var opacityAnimation = new WpfAnimation.DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(180),
                EasingFunction = new WpfAnimation.CubicEase
                {
                    EasingMode = WpfAnimation.EasingMode.EaseOut
                }
            };

            window.BeginAnimation(Wpf.UIElement.OpacityProperty, opacityAnimation);

            if (window.Content is not Wpf.FrameworkElement content)
            {
                return;
            }

            content.RenderTransformOrigin = new Wpf.Point(0.5, 0.5);

            var scaleTransform = new WpfMedia.ScaleTransform(0.985, 0.985);
            var translateTransform = new WpfMedia.TranslateTransform(0, 8);
            var transformGroup = new WpfMedia.TransformGroup();
            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(translateTransform);
            content.RenderTransform = transformGroup;

            var slideAnimation = new WpfAnimation.DoubleAnimation
            {
                From = 8,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(180),
                EasingFunction = new WpfAnimation.CubicEase
                {
                    EasingMode = WpfAnimation.EasingMode.EaseOut
                }
            };

            translateTransform.BeginAnimation(WpfMedia.TranslateTransform.YProperty, slideAnimation);

            var scaleAnimation = new WpfAnimation.DoubleAnimation
            {
                From = 0.985,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(180),
                EasingFunction = new WpfAnimation.CubicEase
                {
                    EasingMode = WpfAnimation.EasingMode.EaseOut
                }
            };

            scaleTransform.BeginAnimation(WpfMedia.ScaleTransform.ScaleXProperty, scaleAnimation);
            scaleTransform.BeginAnimation(WpfMedia.ScaleTransform.ScaleYProperty, scaleAnimation);
        };
    }
}
