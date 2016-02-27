using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace Net.Astropenguin.UI
{
    public enum ControlState { Foreatii, Reovia, Seonium }

    [TemplateVisualState( Name = "Foreatii", GroupName = "ControlStates" )]
    [TemplateVisualState( Name = "Reovia", GroupName = "ControlStates" )]
    [TemplateVisualState( Name = "Seonium", GroupName = "ControlStates" )]
    [ContentProperty( Name = "ControlContext" )]
    public class StateControl : Control 
    {
        public static readonly DependencyProperty ControlContextProperty = DependencyProperty.Register( "ControlContext", typeof( object ), typeof( StateControl ), new PropertyMetadata( null ) );
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register( "State", typeof( ControlState ), typeof( StateControl ), new PropertyMetadata( ControlState.Foreatii, StateChanged ) );

        private static void StateChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            ( d as StateControl ).UpdateVisualState();
        }

        public ControlState State
        {
            get { return ( ControlState ) GetValue( StateProperty ); }
            set { SetValue( StateProperty, value ); }
        }

        public object ControlContext
        {
            get { return GetValue( ControlContextProperty ); }
            set { SetValue( ControlContextProperty, value ); }
        }

        private void UpdateVisualState( bool useTransitions = true )
        {
            switch ( State )
            {
                case ControlState.Foreatii:
                    VisualStateManager.GoToState( this, "Foreatii", useTransitions );
                    break;

                case ControlState.Reovia:
                    VisualStateManager.GoToState( this, "Reovia", useTransitions );
                    break;

                case ControlState.Seonium:
                    VisualStateManager.GoToState( this, "Seonium", useTransitions );
                    break;
            }
        }

        public StateControl()
            : base()
        {
            DefaultStyleKey = typeof( StateControl );
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateVisualState( false );
        }
    }

}
