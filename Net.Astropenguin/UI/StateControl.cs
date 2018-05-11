using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace Net.Astropenguin.UI
{
	public enum ControlState { Closed, Active }

	[TemplateVisualState( Name = "Closed", GroupName = "ControlStates" )]
	[TemplateVisualState( Name = "Active", GroupName = "ControlStates" )]
	[ContentProperty( Name = "ControlContext" )]
	public class StateControl : Control
	{
		public static readonly DependencyProperty ControlContextProperty = DependencyProperty.Register( "ControlContext", typeof( object ), typeof( StateControl ), new PropertyMetadata( null ) );
		public static readonly DependencyProperty StateProperty = DependencyProperty.Register( "State", typeof( ControlState ), typeof( StateControl ), new PropertyMetadata( ControlState.Closed, StateChanged ) );

		public event TypedEventHandler<object, ControlState> OnStateChanged;

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
				case ControlState.Closed:
					VisualStateManager.GoToState( this, "Closed", useTransitions );
					break;

				case ControlState.Active:
					VisualStateManager.GoToState( this, "Active", useTransitions );
					break;
			}
		}

		public StateControl()
			: base()
		{
			DefaultStyleKey = typeof( StateControl );
		}

		private void OnClosed( object sender, object e )
		{
			OnStateChanged?.Invoke( this, ControlState.Closed );
		}

		private void OnActive( object sender, object e )
		{
			OnStateChanged?.Invoke( this, ControlState.Active );
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			VisualStateGroup VSG = ( VisualStateGroup ) GetTemplateChild( "ControlStates" );

			foreach( VisualTransition VST in VSG.Transitions )
			{
				switch( VST.GetValue( NameProperty ).ToString() )
				{
					case "ActiveToClosed":
						VST.Storyboard.Completed += OnClosed;
						break;
					case "ClosedToActive":
						VST.Storyboard.Completed += OnActive;
						break;
				}

			}

			UpdateVisualState( false );
		}
	}

}
