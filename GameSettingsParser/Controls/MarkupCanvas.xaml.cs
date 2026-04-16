using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GameSettingsParser.Adorners;
using GameSettingsParser.Model;
using GameSettingsParser.Utility;
using Point = System.Windows.Point;
using DrawableRectangle = System.Windows.Shapes.Rectangle;

namespace GameSettingsParser.Controls
{
    public partial class MarkupCanvas
    {
        public static readonly DependencyProperty ImageInstanceProperty =
            DependencyProperty.Register(nameof(ImageInstance),
                typeof(ImageInstanceModel), typeof(MarkupCanvas), new PropertyMetadata(OnImageInstanceChanged));

        private static void OnImageInstanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MarkupCanvas)d;
            control.HandleImageInstanceChanged(((ImageInstanceModel)e.OldValue), ((ImageInstanceModel)e.NewValue));
        }

        private void HandleImageInstanceChanged(ImageInstanceModel eOldValue, ImageInstanceModel eNewValue)
        {
            if (eNewValue != null)
            {
                ImageInstance.MarkupInstances.CollectionChanged += MarkupInstancesOnCollectionChanged;
                if (ImageInstance is { Image: not null } && File.Exists(ImageInstance.Image.Path))
                {
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri(ImageInstance.Image.Path);
                    bitmapImage.EndInit();
                    MainImage.Source = bitmapImage;
                }
                else
                {
                    MainImage.Source = null;
                }

                MainImage.InvalidateMeasure();
                MainImage.InvalidateArrange();
                MainImage.UpdateLayout();
                UpdateCanvasTransform();
                CreateInitialDisplays();
            }
        }

        public ImageInstanceModel ImageInstance
        {
            get => (ImageInstanceModel)GetValue(ImageInstanceProperty);
            set
            {
                if(ImageInstance != value)
                {
                    SetValue(ImageInstanceProperty, value);
                    if (ImageInstance != null)
                    {
                        ImageInstance.MarkupInstances.CollectionChanged += MarkupInstancesOnCollectionChanged;
                        CreateInitialDisplays();
                        if (ImageInstance.Image != null && File.Exists(ImageInstance.Image.Path))
                        {
                            BitmapImage bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.UriSource = new Uri(ImageInstance.Image.Path);
                            bitmapImage.EndInit();

                            MainImage.Source = bitmapImage;
                        }
                    }
                }
            }
        }
        
        public static readonly DependencyProperty SelectedMarkupTypeProperty =
            DependencyProperty.Register(nameof(SelectedMarkupType),
                typeof(MarkupTypeModel), typeof(MarkupCanvas), new PropertyMetadata(null, null));


        public MarkupTypeModel SelectedMarkupType
        {
            get => (MarkupTypeModel)GetValue(SelectedMarkupTypeProperty);
            set => SetValue(SelectedMarkupTypeProperty, value);
        }

        public static readonly DependencyProperty SelectedBrushProperty =
            DependencyProperty.Register(nameof(SelectedBrush),
                typeof(Brush), typeof(MarkupCanvas), new PropertyMetadata(Brushes.White, null));

        public Brush SelectedBrush
        {
            get => (Brush)GetValue(SelectedBrushProperty);
            set => SetValue(SelectedBrushProperty, value);
        }

        
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(nameof(StrokeThickness),
                typeof(double), typeof(MarkupCanvas), new PropertyMetadata(3.0, null));
        
        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }
        
        public static readonly DependencyProperty FillOpacityProperty =
            DependencyProperty.Register(nameof(FillOpacity),
                typeof(double), typeof(MarkupCanvas), new PropertyMetadata(0.25, null));
        
        public double FillOpacity
        {
            get => (double)GetValue(FillOpacityProperty);
            set => SetValue(FillOpacityProperty, value);
        }

        private void MarkupInstancesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddMarkupInstances(e.NewItems);
                    break;
                
                case NotifyCollectionChangedAction.Remove:
                    RemoveMarkupInstances(e.OldItems);
                    break;
                
                case NotifyCollectionChangedAction.Replace:
                    RemoveMarkupInstances(e.OldItems);
                    AddMarkupInstances(e.NewItems);
                    break;
                
                case NotifyCollectionChangedAction.Reset:
                    ClearMarkupDisplays();
                    break;
            }
        }

        private void AddMarkupInstances(IList? markupInstances)
        {
            if (markupInstances == null) 
                return;

            foreach (MarkupInstanceModel addedItem in markupInstances)
            {
                CreateMarkupDisplays(addedItem);
            }
        }
        
        private void RemoveMarkupInstances(IList? markupInstances)
        {
            if (markupInstances == null) 
                return;
            
            foreach (MarkupInstanceModel removedItem in markupInstances)
            {
                if (!_markupRectangles.TryGetValue(removedItem, out var value)) 
                    continue;
                            
                MainCanvas.Children.Remove(value);
                _markupRectangles.Remove(removedItem);
            }
        }
        
        public bool HasImageInstance => ImageInstance != null;
        public bool HasSelectedMarkupType => SelectedMarkupType != null;
        
        private bool _isDragActive;
        private Point _dragStartPoint;
        private Point _dragEndPoint;
        private readonly BidirectionalDictionary<MarkupInstanceModel, DrawableRectangle> _markupRectangles = [];
        private readonly Dictionary<DrawableRectangle, RectangleTransformAdorner> _rectangleTransformAdorners = [];
        private DrawableRectangle? _activeDragRectangle;
        private DrawableRectangle? _selectedRectangle;

        public MarkupCanvas()
        {
            InitializeComponent();
            if (ImageInstance != null)
            {
                UpdateCanvasTransform();
                CreateInitialDisplays();
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!HasImageInstance || !HasSelectedMarkupType || _isDragActive)
            {
                ClearDrag();
                return;
            }

            if (!IsMouseOver || IsExistingDisplayUnderMouse()) 
                return;
            
            _isDragActive = true;
            _dragStartPoint = TransformPointToImageSpace(e.GetPosition(MainImage));
            _dragEndPoint = _dragStartPoint;
            _activeDragRectangle = CreateDrawableRectangle(SelectedMarkupType.Color, PointsToValidRect(_dragStartPoint, _dragEndPoint), false);
            e.Handled = true;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!HasImageInstance || !HasSelectedMarkupType)
            {
                ClearDrag();
                return;
            }

            if (_isDragActive && _activeDragRectangle != null)
            {
                _dragEndPoint = TransformPointToImageSpace(e.GetPosition(MainImage));
                UpdateRectangleTransform(_activeDragRectangle, PointsToValidRect(_dragStartPoint, _dragEndPoint));
                e.Handled = true;
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!HasImageInstance || !HasSelectedMarkupType)
            {
                ClearDrag();
                return;
            }
            
            if (_selectedRectangle != null)
            {
                Deselect();
            }
            
            if (_isDragActive)
            {
                CreateMarkupInstance(_dragStartPoint, TransformPointToImageSpace(e.GetPosition(MainImage)));

                MainCanvas.Children.Remove(_activeDragRectangle);
                _activeDragRectangle = null;
            }

            _isDragActive = false;
            e.Handled = true;
        }
        
        private void ClearDrag()
        {
            _dragStartPoint = new Point(double.NaN, double.NaN);
            _dragEndPoint = new Point(double.NaN, double.NaN);
            _isDragActive = false;
        }

        private void SelectRectangle(DrawableRectangle? rectangle)
        {
            Deselect();
            _selectedRectangle = rectangle;
            _selectedRectangle!.Stroke = SelectedBrush;
        }

        private void Deselect()
        {
            if (_selectedRectangle == null || !_markupRectangles.Inverse.TryGetValue(_selectedRectangle, out var markupInstance))
                return;

            if (markupInstance.Type == null)
                return;
            
            var color = markupInstance.Type.Color;
            _selectedRectangle.Stroke = new SolidColorBrush(color);
            _selectedRectangle = null;
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            ClearDrag();
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            //TODO: Fix not firing
            if(e.Key == Key.Delete && _selectedRectangle != null)
                RemoveDrawableRectangle(_selectedRectangle);
        }

        private bool IsExistingDisplayUnderMouse()
        {
            return _markupRectangles.Any(markupInstance => markupInstance.Value.IsMouseOver);
        }

        private Point TransformPointToImageSpace(Point point)
        {
            if (MainImage.Source is not BitmapSource bitmap)
                throw new InvalidOperationException("MainImage.Source is not a BitmapSource");

            var imageSize = new Size(bitmap.PixelWidth, bitmap.PixelHeight);
            
            var xScale = point.X / MainImage.ActualWidth;
            var yScale = point.Y / MainImage.ActualHeight;

            return new Point(imageSize.Width * xScale, imageSize.Height * yScale);
        }

        private void CreateMarkupInstance(Point startPoint, Point endPoint)
        {
            if (!HasImageInstance || !HasSelectedMarkupType)
                return;
            
            var newInstance = new MarkupInstanceModel()
            {
                Rect = new Rect(startPoint, endPoint),
                Type = SelectedMarkupType
            };

            // TODO: Fix the source of this issue, it appears to occur when alt-tabbing but may be development only.
            if (newInstance.Rect.Width == 0 || newInstance.Rect.Height == 0)
                return;
            
            ImageInstance.MarkupInstances.Add(newInstance);
        }

        private void CreateMarkupDisplays(MarkupInstanceModel markupInstance)
        {
            if (markupInstance == null || markupInstance.Type == null || _markupRectangles.ContainsKey(markupInstance))
                return;
            
            var drawableRectangle = CreateDrawableRectangle(markupInstance.Type.Color, markupInstance.Rect);
            _markupRectangles[markupInstance] = drawableRectangle;
        }

        private DrawableRectangle CreateDrawableRectangle(Color color, Rect imageSpaceRect, bool addAdorner = true)
        {
            var fillBrush = new SolidColorBrush(color)
            {
                Opacity = FillOpacity
            };

            var drawableRectangle = new DrawableRectangle
            {
                Stroke = new SolidColorBrush(color),
                Fill = fillBrush,
                StrokeThickness = StrokeThickness,
                IsManipulationEnabled = true,
            };

            MainCanvas.Children.Add(drawableRectangle);
            
            UpdateRectangleTransform(drawableRectangle, imageSpaceRect);
            if (addAdorner)
            {
                var adorner = new RectangleTransformAdorner(drawableRectangle, new Thickness(6.0));
                adorner.OnTransformChanged += OnAdornerTransformChanged;
                adorner.OnSelected += OnAdornerSelected;
                _rectangleTransformAdorners.Add(drawableRectangle, adorner);
            }

            return drawableRectangle;
        }

        private void UpdateRectangleTransform(DrawableRectangle rectangle, Rect imageSpaceRect)
        {
            if (imageSpaceRect.Width < 0)
            {
                imageSpaceRect.X -= Math.Abs(imageSpaceRect.Width);
                imageSpaceRect.Width = Math.Max(Math.Abs(imageSpaceRect.Width), 1);
            }

            if (imageSpaceRect.Height < 0)
            {
                imageSpaceRect.Y -= Math.Abs(imageSpaceRect.Height);
                imageSpaceRect.Height = Math.Max(Math.Abs(imageSpaceRect.Height), 1);
            }
            
            rectangle.SetValue(Canvas.LeftProperty, imageSpaceRect.X);
            rectangle.SetValue(Canvas.TopProperty, imageSpaceRect.Y);
            rectangle.SetValue(Canvas.WidthProperty, imageSpaceRect.Width);
            rectangle.SetValue(Canvas.HeightProperty, imageSpaceRect.Height);
        }

        private Rect PointsToValidRect(Point pointA, Point pointB)
        {
            return new Rect(Math.Min(pointA.X, pointB.X), Math.Min(pointA.Y, pointB.Y), Math.Abs(pointA.X - pointB.X), Math.Abs(pointA.Y - pointB.Y));       
        }

        private void CreateInitialDisplays()
        {
            ClearMarkupDisplays();
            
            foreach (var markupInstance in ImageInstance.MarkupInstances)
            {
                CreateMarkupDisplays(markupInstance);
            }
        }

        private void ClearMarkupDisplays()
        {
            foreach (var markupRectangle in _markupRectangles.Values)
                RemoveDrawableRectangle(markupRectangle);
            
            _markupRectangles.Clear();
        }

        private void RemoveDrawableRectangle(DrawableRectangle drawableRectangle)
        {
            if(_selectedRectangle == drawableRectangle)
                Deselect();
            
            if (_rectangleTransformAdorners.TryGetValue(drawableRectangle, out var adorner))
            {
                adorner.OnTransformChanged -= OnAdornerTransformChanged;
                adorner.OnSelected -= OnAdornerSelected;
                adorner.Destroy();
                _rectangleTransformAdorners.Remove(drawableRectangle);
            }
            
            _markupRectangles.Inverse.Remove(drawableRectangle);
            MainCanvas.Children.Remove(drawableRectangle);
        }

        private void OnAdornerTransformChanged(object? sender, RectangleTransformAdorner.RectangleOperationEventArgs e)
        {
            if (_markupRectangles.Inverse.TryGetValue(e.Rectangle, out var markupInstance))
            {
                Point startPoint = new Point((double)e.Rectangle.GetValue(Canvas.LeftProperty), (double)e.Rectangle.GetValue(Canvas.TopProperty));
                Point endPoint = new Point(startPoint.X + e.Rectangle.ActualWidth, startPoint.Y + e.Rectangle.ActualHeight);
                var rect = new Rect(startPoint, endPoint);
                
                if(rect.Width == 0 || rect.Height == 0)
                    throw new InvalidOperationException("Cannot create markup instance with zero width or height");
                
                markupInstance.Rect = rect;
            }
        }

        private void OnAdornerSelected(object? sender, RectangleTransformAdorner.RectangleOperationEventArgs e)
        {
            SelectRectangle(e.Rectangle);
            Focus();
        }

        private void OnMainImageSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateCanvasTransform();
        }

        private void UpdateCanvasTransform()
        {
            if (MainImage.Source is not BitmapSource bitmap)
                return;

            var imageSize = new Size(bitmap.PixelWidth, bitmap.PixelHeight);
            var viewSize = new Size(MainGrid.ActualWidth, MainGrid.ActualHeight);

            MainCanvas.RenderTransform = OverlayTransformHelper.CreateImageToViewTransform(
                imageSize,
                viewSize,
                MainImage.Stretch);
        }
    }
}