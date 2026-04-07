using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GameSettingsParser.Adorners;
using GameSettingsParser.Model;
using Point = System.Windows.Point;
using DrawableRectangle = System.Windows.Shapes.Rectangle;

namespace GameSettingsParser.Controls
{
    public partial class MarkupCanvas : UserControl
    {
        public static readonly DependencyProperty ImageInstanceProperty =
            DependencyProperty.Register(nameof(ImageInstance),
                typeof(ParsingProfileModel.ImageInstance), typeof(MarkupCanvas), new PropertyMetadata(OnImageInstanceChanged));

        private static void OnImageInstanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MarkupCanvas)d;
            control.HandleImageInstanceChanged(((ParsingProfileModel.ImageInstance)e.OldValue), ((ParsingProfileModel.ImageInstance)e.NewValue));
        }

        private void HandleImageInstanceChanged(ParsingProfileModel.ImageInstance eOldValue, ParsingProfileModel.ImageInstance eNewValue)
        {
            if (eNewValue != null)
            {
                ImageInstance.MarkupInstances.CollectionChanged += MarkupInstancesOnCollectionChanged;
                BitmapImage bitmapImage =  new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(ImageInstance.Image.Path);
                bitmapImage.EndInit();
                MainImage.Source = bitmapImage;
                MainImage.InvalidateMeasure();
                MainImage.InvalidateArrange();
                MainImage.UpdateLayout();
                CreateInitialDisplays();
            }
        }

        public ParsingProfileModel.ImageInstance ImageInstance
        {
            get => (ParsingProfileModel.ImageInstance)GetValue(ImageInstanceProperty);
            set
            {
                if(!ImageInstance.Equivalent(value))
                {
                    SetValue(ImageInstanceProperty, value);
                    if (ImageInstance != null)
                    {
                        ImageInstance.MarkupInstances.CollectionChanged += MarkupInstancesOnCollectionChanged;
                        CreateInitialDisplays();
                        BitmapImage bitmapImage =  new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.UriSource = new Uri(ImageInstance.Image.Path);
                        bitmapImage.EndInit();
                        MainImage.Source = bitmapImage;
                    }
                }
            }
        }
        
        public static readonly DependencyProperty SelectedMarkupTypeProperty =
            DependencyProperty.Register(nameof(SelectedMarkupType),
                typeof(MarkupTypeModel?), typeof(MarkupCanvas), new PropertyMetadata(null, null));


        public MarkupTypeModel? SelectedMarkupType
        {
            get => (MarkupTypeModel?)GetValue(SelectedMarkupTypeProperty);
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
            }
        }

        private void AddMarkupInstances(IList? markupInstances)
        {
            if (markupInstances == null) 
                return;

            foreach (ParsingProfileModel.MarkupInstance addedItem in markupInstances)
            {
                CreateMarkupDisplays(addedItem);
            }
        }
        
        private void RemoveMarkupInstances(IList? markupInstances)
        {
            if (markupInstances == null) 
                return;
            
            foreach (ParsingProfileModel.MarkupInstance removedItem in markupInstances)
            {
                if (!_markupRectangles.TryGetValue(removedItem, out var value)) 
                    continue;
                            
                MainCanvas.Children.Remove(value);
                _markupRectangles.Remove(removedItem);
            }
        }
        
        public bool HasImageInstance => ImageInstance != null;
        public bool HasSelectedMarkupType => SelectedMarkupType != null;
        
        private bool _isDragActive = false;
        private bool _isManipulationActive = false;
        private Point _dragStartPoint;
        private Point _dragEndPoint;
        private BidirectionalDictionary<ParsingProfileModel.MarkupInstance, DrawableRectangle> _markupRectangles = [];
        private Dictionary<DrawableRectangle, RectangleTransformAdorner> _rectangleTransformAdorners = [];
        private DrawableRectangle? _activeDragRectangle = null;
        private DrawableRectangle? _selectedRectangle = null;
        private RectangleTransformAdorner? _selectedRectangleTransformAdorner = null;

        public MarkupCanvas()
        {
            InitializeComponent();
            if(ImageInstance != null)
                CreateInitialDisplays();
            
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!HasImageInstance || !HasSelectedMarkupType || _isManipulationActive || _isDragActive)
            {
                ClearDrag();
                return;
            }

            if (!IsMouseOver || IsExistingDisplayUnderMouse()) 
                return;
            
            _isDragActive = true;
            _dragStartPoint = e.GetPosition(MainCanvas);
            _dragEndPoint = _dragStartPoint;
            _activeDragRectangle = CreateDrawableRectangle(SelectedMarkupType!.Value.Color, PointsToValidRect(_dragStartPoint, _dragEndPoint), false);
            e.Handled = true;
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!HasImageInstance || !HasSelectedMarkupType || _isManipulationActive)
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
                CreateMarkupInstance(_dragStartPoint, e.GetPosition(this));

                MainCanvas.Children.Remove(_activeDragRectangle);
                _activeDragRectangle = null;
            }

            _isDragActive = false;
            e.Handled = true;
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
            
            var color = markupInstance.Type.Color;
            _selectedRectangle.Stroke = new SolidColorBrush(color);
            _selectedRectangle = null;
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
                _dragEndPoint = e.GetPosition(MainCanvas);
                UpdateRectangleTransform(_activeDragRectangle, PointsToValidRect(_dragStartPoint, _dragEndPoint));
                e.Handled = true;
            }
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            ClearDrag();
        }

        private bool IsExistingDisplayUnderMouse()
        {
            return _markupRectangles.Any(markupInstance => markupInstance.Value.IsMouseOver);
        }
        
        private void ClearDrag()
        {
            _dragStartPoint = new Point(double.NaN, double.NaN);
            _dragEndPoint = new Point(double.NaN, double.NaN);
            _isDragActive = false;
        }

        private Point TransformPointToImageSpace(Point point)
        {
            var xScale = point.X / MainImage.ActualWidth;
            var yScale = point.Y / MainImage.ActualHeight;

            return new Point(MainImage.Source.Width * xScale, MainImage.Source.Height * yScale);
        }

        private void CreateMarkupInstance(Point startPoint, Point endPoint)
        {
            if (!HasImageInstance || !HasSelectedMarkupType)
                return;
            
            var newInstance = new ParsingProfileModel.MarkupInstance()
            {
                Rect = new Rect(TransformPointToImageSpace(startPoint), TransformPointToImageSpace(endPoint)),
                Type = SelectedMarkupType!.Value
            };
            
            ImageInstance.MarkupInstances.Add(newInstance);
        }

        private void CreateMarkupDisplays(ParsingProfileModel.MarkupInstance markupInstance)
        {
            if (_markupRectangles.ContainsKey(markupInstance))
                return;

            var transformedRect = markupInstance.Rect;
            
            var xScale = MainImage.ActualWidth / MainImage.Source.Width;
            var yScale = MainImage.ActualHeight / MainImage.Source.Height;
            
            transformedRect.X *= xScale;
            transformedRect.Y *= yScale;
            transformedRect.Width *= xScale;
            transformedRect.Height *= yScale;
            
            var color = markupInstance.Type.Color;

            var drawableRectangle = CreateDrawableRectangle(color, transformedRect);

            _markupRectangles[markupInstance] = drawableRectangle;
        }

        private DrawableRectangle CreateDrawableRectangle(Color color, Rect widgetSpaceRect, bool addAdorner = true)
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
            
            UpdateRectangleTransform(drawableRectangle, widgetSpaceRect);
            if (addAdorner)
            {
                var adorner = new RectangleTransformAdorner(drawableRectangle, new Thickness(6.0));
                adorner.OnTransformChanged += OnAdornerTransformChanged;
                adorner.OnSelected += OnAdornerSelected;
                _rectangleTransformAdorners.Add(drawableRectangle, adorner);
            }

            return drawableRectangle;
        }

        private void OnAdornerTransformChanged(object? sender, RectangleTransformAdorner.RectangleOperationEventArgs e)
        {
            if (_markupRectangles.Inverse.TryGetValue(e.Rectangle, out var markupInstance))
            {
                Point startPoint = new Point((double)e.Rectangle.GetValue(Canvas.LeftProperty), (double)e.Rectangle.GetValue(Canvas.TopProperty));
                Point endPoint = new Point(startPoint.X + e.Rectangle.ActualWidth, startPoint.Y + e.Rectangle.ActualHeight);
                markupInstance.Rect = new Rect(TransformPointToImageSpace(startPoint), TransformPointToImageSpace(endPoint));
            }
        }

        private void UpdateRectangleTransform(DrawableRectangle rectangle, Rect widgetSpaceRect)
        {
            if (widgetSpaceRect.Width < 0)
            {
                widgetSpaceRect.X -= Math.Abs(widgetSpaceRect.Width);
                widgetSpaceRect.Width = Math.Max(Math.Abs(widgetSpaceRect.Width), 1);
            }

            if (widgetSpaceRect.Height < 0)
            {
                widgetSpaceRect.Y -= Math.Abs(widgetSpaceRect.Height);
                widgetSpaceRect.Height = Math.Max(Math.Abs(widgetSpaceRect.Height), 1);
            }
            
            rectangle.SetValue(Canvas.LeftProperty, widgetSpaceRect.X);
            rectangle.SetValue(Canvas.TopProperty, widgetSpaceRect.Y);
            rectangle.SetValue(Canvas.WidthProperty, widgetSpaceRect.Width);
            rectangle.SetValue(Canvas.HeightProperty, widgetSpaceRect.Height);
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

        private void OnAdornerSelected(object? sender, RectangleTransformAdorner.RectangleOperationEventArgs e)
        {
            SelectRectangle(e.Rectangle);
            Focus();
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            //TODO: Fix not firing
            if(e.Key == Key.Delete && _selectedRectangle != null)
                RemoveDrawableRectangle(_selectedRectangle);
        }
    }
}