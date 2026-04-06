using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
                CreateInitialDisplays();
                BitmapImage bitmapImage =  new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(ImageInstance.Image.Path);
                bitmapImage.EndInit();
                MainImage.Source = bitmapImage;
                MainImage.InvalidateMeasure();
                MainImage.InvalidateArrange();
                MainImage.UpdateLayout();
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
        private DrawableRectangle? _activeDragRectangle = null;
        private DrawableRectangle? _selectedRectangle = null;

        public MarkupCanvas()
        {
            InitializeComponent();
            if(ImageInstance != null)
                CreateInitialDisplays();
            
            Control.ManipulationStarting += OnManipulationStarting;
            Control.ManipulationDelta += OnManipulationDelta;
            Control.ManipulationCompleted += OnManipulationCompleted;
        }

        private void OnManipulationStarting(object? sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = Control;
            e.Handled = true;
            _isManipulationActive = true;
        }

        private void OnManipulationDelta(object? sender, ManipulationDeltaEventArgs e)
        {
            if (_isDragActive)
                return;
            
            DrawableRectangle? rectangle = e.OriginalSource as DrawableRectangle;
            if (rectangle == null)
                return;
            
            Matrix rectsMatrix = ((MatrixTransform)rectangle.RenderTransform).Matrix;

            // Rotate the Rectangle.
            rectsMatrix.RotateAt(e.DeltaManipulation.Rotation,
                e.ManipulationOrigin.X,
                e.ManipulationOrigin.Y);

            // Resize the Rectangle.  Keep it square
            // so use only the X value of Scale.
            rectsMatrix.ScaleAt(e.DeltaManipulation.Scale.X,
                e.DeltaManipulation.Scale.X,
                e.ManipulationOrigin.X,
                e.ManipulationOrigin.Y);

            // Move the Rectangle.
            rectsMatrix.Translate(e.DeltaManipulation.Translation.X,
                e.DeltaManipulation.Translation.Y);

            // Apply the changes to the Rectangle.
            rectangle.RenderTransform = new MatrixTransform(rectsMatrix);
            
            Rect containingRect =
                new Rect(((FrameworkElement)e.ManipulationContainer).RenderSize);

            Rect shapeBounds =
                rectangle.RenderTransform.TransformBounds(
                    new Rect(rectangle.RenderSize));

            // Check if the rectangle is completely in the window.
            // If it is not and intertia is occuring, stop the manipulation.
            if (e.IsInertial && !containingRect.Contains(shapeBounds))
            {
                e.Complete();
            }

            Point topLeftWidgetSpace = new Point((double)rectangle.GetValue(Canvas.LeftProperty), (double)rectangle.GetValue(Canvas.TopProperty));
            Point bottomRightWidgetSpace = new Point(topLeftWidgetSpace.X + rectangle.ActualWidth, topLeftWidgetSpace.Y + rectangle.ActualHeight);
            Point topLeftImageSpace = TransformPointToImageSpace(topLeftWidgetSpace);
            Point bottomRightImageSpace = TransformPointToImageSpace(bottomRightWidgetSpace);
            
            var markupInstance = _markupRectangles.Inverse[rectangle];
            markupInstance.Rect = new Rect(topLeftImageSpace.X, topLeftImageSpace.Y, bottomRightImageSpace.X - topLeftImageSpace.X, bottomRightImageSpace.Y - topLeftImageSpace.Y);
            
            e.Handled = true;
        }

        private void OnManipulationCompleted(object? sender, ManipulationCompletedEventArgs e)
        {
            e.Handled = true;
            _isManipulationActive = false;
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
            Console.WriteLine($"Down: Drag start: {_dragStartPoint}");
            _dragEndPoint = _dragStartPoint;
            _activeDragRectangle = CreateDrawableRectangle(SelectedMarkupType!.Value.Color, PointsToValidRect(_dragStartPoint, _dragEndPoint));
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!HasImageInstance || !HasSelectedMarkupType || _isManipulationActive)
            {
                ClearDrag();
                return;
            }
            
            if (_isDragActive)
            {
                CreateMarkupInstance(_dragStartPoint, e.GetPosition(this));

                MainCanvas.Children.Remove(_activeDragRectangle);
                _activeDragRectangle = null;
            }
            else
            {
                if (TryGetExistingDisplayUnderMouse(out var rectangle))
                {
                    _selectedRectangle = rectangle;
                    _selectedRectangle!.Stroke = SelectedBrush;
                }
                else
                {
                    if (_selectedRectangle != null)
                    {
                        var color = _markupRectangles.Inverse[_selectedRectangle].Type.Color;
                        _selectedRectangle.Stroke = new SolidColorBrush(color);
                    }

                    _selectedRectangle = null;
                }
            }

            Console.WriteLine($"Up: Drag start: {_dragStartPoint} | Drag end: {_dragEndPoint}");
            _isDragActive = false;
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
            }
            
            Console.WriteLine($"Move: Drag start: {_dragStartPoint} | Drag end: {_dragEndPoint}");
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            ClearDrag();
        }

        private bool IsExistingDisplayUnderMouse()
        {
            return _markupRectangles.Any(markupInstance => markupInstance.Value.IsMouseDirectlyOver);
        }

        private bool TryGetExistingDisplayUnderMouse(out DrawableRectangle? rectangle)
        {
            rectangle = _markupRectangles.FirstOrDefault(markupInstance => markupInstance.Value.IsMouseDirectlyOver).Value;
            return rectangle != null;
        }
        
        private void ClearDrag()
        {
            Console.WriteLine($"Clear: Drag start: {_dragStartPoint} | Drag end: {_dragEndPoint}");;
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

        private DrawableRectangle CreateDrawableRectangle(Color color, Rect widgetSpaceRect)
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
            
            return drawableRectangle;
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
            foreach (var markupInstance in ImageInstance.MarkupInstances)
            {
                CreateMarkupDisplays(markupInstance);
            }
        }

        private void ClearInstancedDisplays()
        {
            foreach (var markupRectangle in _markupRectangles.Values)
                MainCanvas.Children.Remove(markupRectangle);
            
            _markupRectangles.Clear();
        }
    }
}