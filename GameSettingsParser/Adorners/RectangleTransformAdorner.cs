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
        public class RectangleOperationEventArgs : EventArgs
        {
            public required Rectangle Rectangle { get; init; }
        }
        
        [Flags]
        enum DragDirection
        {
            Left = 1,
            Right = 2,
            Top = 4,
            Bottom = 8
        }
        
        public event EventHandler<RectangleOperationEventArgs>? OnTransformChanged;
        public event EventHandler<RectangleOperationEventArgs>? OnSelected;
        
        private readonly Rectangle _rectangle;
        private Thickness _thickness;
        private Point _minimumSize;
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
        
        public RectangleTransformAdorner(UIElement adornedElement, Thickness thickness, double minimumCentreSize = 4.0) : base(adornedElement)
        {
            if(adornedElement is not Rectangle rectangle)
                throw new ArgumentException("The adorned element must be a rectangle!");
            
            _rectangle = rectangle;
            _thickness = thickness;
            _minimumSize = new Point(_thickness.Left + _thickness.Right + minimumCentreSize, _thickness.Top + _thickness.Bottom + minimumCentreSize);

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
                Opacity = 0
            };
            _moveThumb.DragDelta += OnMoveDragDelta;
            _moveThumb.DragStarted += OnThumbMouseDown;
            _moveThumb.Cursor = Cursors.Cross;
            _visualCollection.Add(_moveThumb);
            
            if(_thickness is { Left: > 0, Top: > 0 })
            {
                _topLeftThumb = new Thumb
                {
                    Width = Math.Abs(_thickness.Left),
                    Height = Math.Abs(_thickness.Top),
                    Opacity = 0
                };
                _topLeftThumb.DragDelta += (sender, args) => OnResizeDragDelta(sender, args, DragDirection.Top | DragDirection.Left);
                _topLeftThumb.DragStarted += OnThumbMouseDown;
                _topLeftThumb.Cursor = Cursors.SizeNWSE;
                _visualCollection.Add(_topLeftThumb);
            }

            if (_thickness is { Left: > 0, Bottom: > 0 })
            {
                _bottomLeftThumb = new Thumb
                {
                    Width = Math.Abs(_thickness.Left),
                    Height = Math.Abs(_thickness.Bottom),
                    Opacity = 0
                };
                _bottomLeftThumb.DragDelta += (sender, args) => OnResizeDragDelta(sender, args, DragDirection.Bottom | DragDirection.Left);
                _bottomLeftThumb.DragStarted += OnThumbMouseDown;
                _bottomLeftThumb.Cursor = Cursors.SizeNESW;
                _visualCollection.Add(_bottomLeftThumb);
            }

            if (_thickness is { Right: > 0, Top: > 0 })
            {
                _topRightThumb = new Thumb
                {
                    Width = Math.Abs(_thickness.Right),
                    Height = Math.Abs(_thickness.Top),
                    Opacity = 0
                };
                _topRightThumb.DragDelta += (sender, args) => OnResizeDragDelta(sender, args, DragDirection.Top | DragDirection.Right);
                _topRightThumb.DragStarted += OnThumbMouseDown;
                _topRightThumb.Cursor = Cursors.SizeNESW;
                _visualCollection.Add(_topRightThumb);
            }

            if (_thickness is { Right: > 0, Bottom: > 0 })
            {
                _bottomRightThumb = new Thumb()
                {
                    Width = Math.Abs(_thickness.Right),
                    Height = Math.Abs(_thickness.Bottom),
                    Opacity = 0
                };
                _bottomRightThumb.DragDelta += (sender, args) => OnResizeDragDelta(sender, args, DragDirection.Bottom | DragDirection.Right);
                _bottomRightThumb.DragStarted += OnThumbMouseDown;
                _bottomRightThumb.Cursor = Cursors.SizeNWSE;
                _visualCollection.Add(_bottomRightThumb);
            }
            
            if(_thickness is { Left: > 0 })
            {
                _leftThumb = new Thumb
                {
                    Width = Math.Abs(_thickness.Left),
                    Height = Math.Abs(Height - _thickness.Top - _thickness.Bottom),
                    Opacity = 0
                };
                _leftThumb.DragDelta += (sender, args) => OnResizeDragDelta(sender, args, DragDirection.Left);
                _leftThumb.DragStarted += OnThumbMouseDown;
                _leftThumb.Cursor = Cursors.SizeWE;
                _visualCollection.Add(_leftThumb);
            }
            
            if(_thickness is { Right: > 0 })
            {
                _rightThumb = new Thumb
                {
                    Width = Math.Abs(_thickness.Right),
                    Height = Math.Abs(Height - _thickness.Top - _thickness.Bottom),
                    Opacity = 0
                };
                _rightThumb.DragDelta += (sender, args) => OnResizeDragDelta(sender, args, DragDirection.Right);
                _rightThumb.DragStarted += OnThumbMouseDown;
                _rightThumb.Cursor = Cursors.SizeWE;
                _visualCollection.Add(_rightThumb);
            }
            
            if(_thickness is { Top: > 0 })
            {
                _topThumb = new Thumb
                {
                    Width = Math.Abs(Width - _thickness.Left - _thickness.Right),
                    Height = Math.Abs(_thickness.Top),
                    Opacity = 0
                };
                _topThumb.DragDelta += (sender, args) => OnResizeDragDelta(sender, args, DragDirection.Top);
                _topThumb.DragStarted += OnThumbMouseDown;
                _topThumb.Cursor = Cursors.SizeNS;
                _visualCollection.Add(_topThumb);
            }
            
            if(_thickness is { Top: > 0 })
            {
                _bottomThumb = new Thumb
                {
                    Width = Math.Abs(Width - _thickness.Left - _thickness.Right),
                    Height = Math.Abs(_thickness.Bottom),
                    Opacity = 0
                };
                _bottomThumb.DragDelta += (sender, args) => OnResizeDragDelta(sender, args, DragDirection.Bottom);
                _bottomThumb.DragStarted += OnThumbMouseDown;
                _bottomThumb.Cursor = Cursors.SizeNS;
                _visualCollection.Add(_bottomThumb);
            }
        }

        private void OnThumbMouseDown(object sender, DragStartedEventArgs e)
        {
            OnSelected?.Invoke(this, new RectangleOperationEventArgs() { Rectangle = _rectangle });
        }

        private void OnResizeDragDelta(object sender, DragDeltaEventArgs e, DragDirection direction)
        {
            double width = (double) _rectangle.GetValue(Canvas.WidthProperty);
            double height = (double) _rectangle.GetValue(Canvas.HeightProperty);
            double minimumWidthAdjustment = (width - e.HorizontalChange) - (Math.Max(width - e.HorizontalChange, _minimumSize.Y));
            double minimumHeightAdjustment = (height - e.VerticalChange) - (Math.Max(height - e.VerticalChange, _minimumSize.Y));

            if (direction.HasFlag(DragDirection.Left))
            {
                double x = (double)_rectangle.GetValue(Canvas.LeftProperty);
                _rectangle.SetValue(Canvas.LeftProperty, x + e.HorizontalChange + minimumWidthAdjustment);
            }

            if (direction.HasFlag(DragDirection.Top))
            {
                double y = (double)_rectangle.GetValue(Canvas.TopProperty);
                _rectangle.SetValue(Canvas.TopProperty, y + e.VerticalChange + minimumHeightAdjustment);
            }

            if (direction.HasFlag(DragDirection.Left) || direction.HasFlag(DragDirection.Right))
            {
                double newWidth = direction.HasFlag(DragDirection.Right)
                    ? Math.Max(width + e.HorizontalChange, _minimumSize.X)
                    : width - (minimumWidthAdjustment + e.HorizontalChange);
                _rectangle.SetValue(Canvas.WidthProperty, newWidth);
            }

            if (direction.HasFlag(DragDirection.Top) || direction.HasFlag(DragDirection.Bottom))
            {
                double newHeight = direction.HasFlag(DragDirection.Bottom)
                    ? Math.Max(height + e.VerticalChange, _minimumSize.Y)
                    : height - (minimumHeightAdjustment + e.VerticalChange);
                _rectangle.SetValue(Canvas.HeightProperty, newHeight);
            }

            OnTransformChanged?.Invoke(this, new RectangleOperationEventArgs { Rectangle = _rectangle });
            e.Handled = true;
        }

        private void OnMoveDragDelta(object sender, DragDeltaEventArgs e)
        {
            double x = (double) _rectangle.GetValue(Canvas.LeftProperty);
            _rectangle.SetValue(Canvas.LeftProperty, x + e.HorizontalChange);
            double y = (double) _rectangle.GetValue(Canvas.TopProperty);
            _rectangle.SetValue(Canvas.TopProperty, y + e.VerticalChange);
            OnTransformChanged?.Invoke(this, new RectangleOperationEventArgs { Rectangle = _rectangle });
            e.Handled = true;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if(finalSize.Width < _thickness.Left + _thickness.Right || finalSize.Height < _thickness.Top + _thickness.Bottom)
                return finalSize;
            
            _moveThumb?.Arrange(new Rect(_thickness.Left / 2, _thickness.Top / 2, finalSize.Width - (_thickness.Right), finalSize.Height -  _thickness.Bottom));
            _topLeftThumb?.Arrange(new Rect(0, 0, _thickness.Left, _thickness.Top));
            _bottomLeftThumb?.Arrange(new Rect(0, finalSize.Height - _thickness.Bottom, _thickness.Left, _thickness.Bottom));
            _topRightThumb?.Arrange(new Rect(finalSize.Width - _thickness.Right, 0, _thickness.Right, _thickness.Top));
            _bottomRightThumb?.Arrange(new Rect(finalSize.Width - _thickness.Right, finalSize.Height - _thickness.Bottom, _thickness.Right, _thickness.Bottom));
            _leftThumb?.Arrange(new Rect(0, _thickness.Top / 2, _thickness.Left, finalSize.Height - (_thickness.Top + _thickness.Bottom)));
            _rightThumb?.Arrange(new Rect(finalSize.Width - _thickness.Right, _thickness.Top / 2, _thickness.Right, finalSize.Height - (_thickness.Top + _thickness.Bottom)));
            _topThumb?.Arrange(new Rect(_thickness.Left / 2, 0, finalSize.Width - (_thickness.Left + _thickness.Right), _thickness.Top));
            _bottomThumb?.Arrange(new Rect(_thickness.Left / 2, finalSize.Height - _thickness.Bottom, finalSize.Width - (_thickness.Left + _thickness.Right), _thickness.Bottom));
            return finalSize;
        }

        protected override int VisualChildrenCount => _visualCollection.Count;
        protected override Visual GetVisualChild(int index) => _visualCollection[index];

        public void Destroy()
        {
            _visualCollection.Clear();
            AdornerLayer.GetAdornerLayer(_rectangle)?.Remove(this);
        }
    }
}