using System.Windows;
using System.Windows.Controls;

namespace WSM.SynData.Master
{
    public class Master : Control
    {
        static Master()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Master),
              new FrameworkPropertyMetadata(typeof(Master)));
        }

        public object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty ContentProperty =
          DependencyProperty.Register("Content", typeof(object),
          typeof(Master), new UIPropertyMetadata());
    }
}
