using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;

namespace Lab12Avalonia
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private double _time = 0;
        
        // Масиви для електронів та кутів нахилу орбіт
        private Ellipse[] _electrons;
        private double[] _orbitAngles = { 0, Math.PI / 3, 2 * Math.PI / 3 }; 
        
        // Параметри орбіт та центру екрану
        private double _a = 180; 
        private double _b = 60;  
        private double _centerX = 400; 
        private double _centerY = 260; 
        
        private Canvas _myCanvas;

        public MainWindow()
        {
            InitializeComponent();
            
            // Надійна прив'язка до Canvas
            _myCanvas = this.FindControl<Canvas>("MyCanvas") ?? this.Find<Canvas>("MyCanvas");
            
            DrawAtom();
            StartAnimation();
        }

        private void DrawAtom()
        {
            // Малюємо ядро атома
            Ellipse nucleus = new Ellipse
            {
                Width = 50,
                Height = 50,
                Fill = Brushes.OrangeRed,
                Stroke = Brushes.DarkRed,
                StrokeThickness = 2
            };
            Canvas.SetLeft(nucleus, _centerX - 25);
            Canvas.SetTop(nucleus, _centerY - 25);
            _myCanvas.Children.Add(nucleus);

            _electrons = new Ellipse[3];

            // Створюємо орбіти та електрони
            for (int i = 0; i < 3; i++)
            {
                Ellipse orbit = new Ellipse
                {
                    Width = _a * 2,
                    Height = _b * 2,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 2
                };

                orbit.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
                orbit.RenderTransform = new RotateTransform(_orbitAngles[i] * 180 / Math.PI); 
                
                Canvas.SetLeft(orbit, _centerX - _a);
                Canvas.SetTop(orbit, _centerY - _b);
                _myCanvas.Children.Add(orbit);

                _electrons[i] = new Ellipse
                {
                    Width = 16,
                    Height = 16,
                    Fill = Brushes.RoyalBlue
                };
                _myCanvas.Children.Add(_electrons[i]);
            }
        }

        private void StartAnimation()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(20);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _time += 0.04; 

            for (int i = 0; i < 3; i++)
            {
                double t = _time + i * (2 * Math.PI / 3);

                double x0 = _a * Math.Cos(t);
                double y0 = _b * Math.Sin(t);

                double angle = _orbitAngles[i];
                double x = _centerX + x0 * Math.Cos(angle) - y0 * Math.Sin(angle);
                double y = _centerY + x0 * Math.Sin(angle) + y0 * Math.Cos(angle);

                Canvas.SetLeft(_electrons[i], x - 8); 
                Canvas.SetTop(_electrons[i], y - 8);
            }
        }

        // Кнопка збереження 
        public void BtnSave_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;

            try
            {
                var bounds = _myCanvas.Bounds;
                if (bounds.Width <= 0 || bounds.Height <= 0)
                {
                    btn.Content = "Помилка: Розмір вікна = 0";
                    return;
                }

                double scaling = this.RenderScaling;
                var pixelSize = new PixelSize((int)(bounds.Width * scaling), (int)(bounds.Height * scaling));
                var dpi = new Vector(96 * scaling, 96 * scaling);

                // Отримуємо шлях до робочого столу
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                
                // ЯВНО вказуємо System.IO.Path, щоб компілятор не плутав його з фігурами XAML
                string fullPath = System.IO.Path.Combine(desktopPath, "atom_output.png");

                using (var rtb = new RenderTargetBitmap(pixelSize, dpi))
                {
                    rtb.Render(_myCanvas);
                    
                    // Явно вказуємо System.IO.File
                    using (var stream = System.IO.File.Create(fullPath))
                    {
                        rtb.Save(stream);
                    }
                }

                btn.Content = "Збережено на Робочий стіл!";
                btn.Background = Brushes.Green;
                btn.Foreground = Brushes.White;

                ToolTip.SetTip(btn, $"Шлях: {fullPath}");
            }
            catch (Exception ex)
            {
                btn.Content = "Помилка!";
                btn.Background = Brushes.Red;
                btn.Foreground = Brushes.White;
                ToolTip.SetTip(btn, $"Причина: {ex.Message}");
            }
        }
    }
}