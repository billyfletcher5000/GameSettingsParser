using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Rectangle = System.Windows.Shapes.Rectangle;

// Heavily based upon NigelWGMajor's ResizeAdorner: https://github.com/NigelWGMajor/ResizeAdorner/tree/master
namespace GameSettingsParser.Adorners
{
    public class RectangleTransformAdorner : Adorner
    {
        public class TransformChangedEventArgs : EventArgs
        {
            public required Rectangle Rectangle { get; init; }
        }
        
        public event EventHandler<TransformChangedEventArgs>? OnTransformChanged;
        
        private readonly Rectangle _rectangle;
        private Thickness _thickness;
        private Brush _brush = new SolidColorBrush(Colors.LightPink);

        private readonly VisualCollection _visualCollection;
        private Thumb? _moveThumb;
        private Thumb _leftThumb;
        private Thumb _topThumb;
        private Thumb _rightThumb;
        private Thumb _bottomThumb;
        private Thumb _topLeftThumb;
        private Thumb _topRightThumb;
        private Thumb _bottomLeftThumb;
        private Thumb _bottomRightThumb;
        
        public RectangleTransformAdorner(UIElement adornedElement, Thickness thickness) : base(adornedElement)
        {
            if(adornedElement is not Rectangle rectangle)
                throw new ArgumentException("The adorned element must be a rectangle!");
            
            _rectangle = rectangle;
            _thickness = thickness;

            _visualCollection = new VisualCollection(this);
            
            AdornerLayer.GetAdornerLayer(rectangle)?.Add(this);
            
            CreateThumbs();
        }

        private void CreateThumbs()
        {
            _moveThumb = new Thumb
            {
                Width = Math.Abs(Width - _thickness.Left - _thickness.Right - 2.0),
                Height = Math.Abs(Height - _thickness.Top - _thickness.Bottom - 2.0),
                Background = Brushes.HotPink,
                Opacity = 0.5,
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(1)
            };
            _moveThumb.DragDelta += OnMoveDragDelta;
            _moveThumb.Cursor = Cursors.Hand;
            _visualCollection.Add(_moveThumb);
            
            if(_thickness.Left > 0 && _thickness.Top > 0)
            {
                _topLeftThumb = new Thumb
                {
                    Width = Math.Abs(_thickness.Left),
                    Height = Math.Abs(_thickness.Top),
                    Background = Brushes.White,
                    Opacity = 0.5,
                    BorderBrush = new SolidColorBrush(Colors.Black),
                    BorderThickness = new Thickness(1)
                };
                _topLeftThumb.DragDelta += OnTopLeftDragDelta;
                _topLeftThumb.Cursor = Cursors.SizeNWSE;
                _visualCollection.Add(_topLeftThumb);
            }

            if (_thickness.Left > 0 && _thickness.Bottom > 0)
            {
                _bottomLeftThumb = new Thumb
                {
                    Width = Math.Abs(_thickness.Left),
                    Height = Math.Abs(_thickness.Bottom),
                    Background = Brushes.White,
                    Opacity = 0.5,
                    BorderBrush = new SolidColorBrush(Colors.Black),
                    BorderThickness = new Thickness(1)
                };
                _bottomLeftThumb.DragDelta += OnBottomLeftDragDelta;
                _bottomLeftThumb.Cursor = Cursors.SizeNESW;
                _visualCollection.Add(_bottomLeftThumb);
            }

            if (_thickness.Right > 0 && _thickness.Top > 0)
            {
                _topRightThumb = new Thumb
                {
                    Width = Math.Abs(_thickness.Right),
                    Height = Math.Abs(_thickness.Top),
                    Background = Brushes.White,
                    Opacity = 0.5,
                    BorderBrush = new SolidColorBrush(Colors.Black),
                    BorderThickness = new Thickness(1)
                };
                _topRightThumb.DragDelta += OnTopRightDragDelta;
                _topRightThumb.Cursor = Cursors.SizeNESW;
                _visualCollection.Add(_topRightThumb);
            }

            if (_thickness.Right > 0 && _thickness.Bottom > 0)
            {
                _bottomRightThumb = new Thumb()
                {
                    Width = Math.Abs(_thickness.Right),
                    Height = Math.Abs(_thickness.Bottom),
                    Background = Brushes.White,
                    Opacity = 0.5,
                    BorderBrush = new SolidColorBrush(Colors.Black),
                    BorderThickness = new Thickness(1)
                };
                _bottomRightThumb.DragDelta += OnBottomRightDragDelta;
                _bottomRightThumb.Cursor = Cursors.SizeNWSE;
                _visualCollection.Add(_bottomRightThumb);
            }
        }

        private void OnBottomRightDragDelta(object sender, DragDeltaEventArgs e)
        {
            double width = (double) _rectangle.GetValue(Canvas.WidthProperty);
            _rectangle.SetValue(Canvas.WidthProperty, width + e.HorizontalChange);
            double height = (double) _rectangle.GetValue(Canvas.HeightProperty);
            _rectangle.SetValue(Canvas.HeightProperty, height + e.VerticalChange);
            OnTransformChanged?.Invoke(this, new TransformChangedEventArgs { Rectangle = _rectangle });
        }

        private void OnTopRightDragDelta(object sender, DragDeltaEventArgs e)
        {
            double y = (double) _rectangle.GetValue(Canvas.TopProperty);
            _rectangle.SetValue(Canvas.TopProperty, y + e.VerticalChange);
            double width = (double) _rectangle.GetValue(Canvas.WidthProperty);
            _rectangle.SetValue(Canvas.WidthProperty, width + e.HorizontalChange);
            double height = (double) _rectangle.GetValue(Canvas.HeightProperty);
            _rectangle.SetValue(Canvas.HeightProperty, height - e.VerticalChange);
            OnTransformChanged?.Invoke(this, new TransformChangedEventArgs { Rectangle = _rectangle });
        }

        private void OnTopLeftDragDelta(object sender, DragDeltaEventArgs e)
        {
            double x = (double) _rectangle.GetValue(Canvas.LeftProperty);
            _rectangle.SetValue(Canvas.LeftProperty, x + e.HorizontalChange);
            double y = (double) _rectangle.GetValue(Canvas.TopProperty);
            _rectangle.SetValue(Canvas.TopProperty, y + e.VerticalChange);
            double width = (double) _rectangle.GetValue(Canvas.WidthProperty);
            _rectangle.SetValue(Canvas.WidthProperty, width - e.HorizontalChange);
            double height = (double) _rectangle.GetValue(Canvas.HeightProperty);
            _rectangle.SetValue(Canvas.HeightProperty, height - e.VerticalChange);
            OnTransformChanged?.Invoke(this, new TransformChangedEventArgs { Rectangle = _rectangle });
        }
        
        private void OnBottomLeftDragDelta(object sender, DragDeltaEventArgs e)
        {
            double x = (double) _rectangle.GetValue(Canvas.LeftProperty);
            _rectangle.SetValue(Canvas.LeftProperty, x + e.HorizontalChange);
            double width = (double) _rectangle.GetValue(Canvas.WidthProperty);
            _rectangle.SetValue(Canvas.WidthProperty, width - e.HorizontalChange);
            double height = (double) _rectangle.GetValue(Canvas.HeightProperty);
            _rectangle.SetValue(Canvas.HeightProperty, height + e.VerticalChange);
            OnTransformChanged?.Invoke(this, new TransformChangedEventArgs { Rectangle = _rectangle });
        }

        private void OnMoveDragDelta(object sender, DragDeltaEventArgs e)
        {
            double x = (double) _rectangle.GetValue(Canvas.LeftProperty);
            _rectangle.SetValue(Canvas.LeftProperty, x + e.HorizontalChange);
            double y = (double) _rectangle.GetValue(Canvas.TopProperty);
            _rectangle.SetValue(Canvas.TopProperty, y + e.VerticalChange);
            OnTransformChanged?.Invoke(this, new TransformChangedEventArgs { Rectangle = _rectangle });
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _moveThumb?.Arrange(new Rect(_thickness.Left / 2, _thickness.Top / 2, finalSize.Width - (_thickness.Right), finalSize.Height -  _thickness.Bottom));
            _topLeftThumb?.Arrange(new Rect(0, 0, _thickness.Left, _thickness.Top));
            _bottomLeftThumb?.Arrange(new Rect(0, finalSize.Height - _thickness.Bottom, _thickness.Left, _thickness.Bottom));
            _topRightThumb?.Arrange(new Rect(finalSize.Width - _thickness.Right, 0, _thickness.Right, _thickness.Top));
            _bottomRightThumb?.Arrange(new Rect(finalSize.Width - _thickness.Right, finalSize.Height - _thickness.Bottom, _thickness.Right, _thickness.Bottom));
            return finalSize;
        }

        protected override int VisualChildrenCount => _visualCollection.Count;
        protected override Visual GetVisualChild(int index) => _visualCollection[index];
    }
}