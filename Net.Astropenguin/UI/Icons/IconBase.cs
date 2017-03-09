using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

using Net.Astropenguin.Logging;

namespace Net.Astropenguin.UI.Icons
{
	[TemplatePart( Name = StageName, Type = typeof( Canvas ) )]
	public abstract class IconBase : ContentControl
	{
		public static readonly string ID = typeof( IconBase ).Name;

		public static DependencyProperty AutoScaleProperty = DependencyProperty.Register( "AutoScale", typeof( bool ), typeof( IconBase ), new PropertyMetadata( false, AutoScaleChanged ) );

		public bool AutoScale
		{
			get { return ( bool ) GetValue( AutoScaleProperty ); }
			set { SetValue( AutoScaleProperty, value ); }
		}

		private static void AutoScaleChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			( d as IconBase ).ScaleStage();
		}

		private const string StageName = "Stage";

		public Canvas Stage { get; private set; }
		protected CompositeTransform StageTransform = new CompositeTransform();

		private Size StageDim;
		private Size SizeGiven;

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			Stage = GetTemplateChild( StageName ) as Canvas;
			if ( Stage == null ) return;

			StageTransform.CenterX = 0.5 * Stage.Width;
			StageTransform.CenterY = 0.5 * Stage.Height;
			Stage.RenderTransform = StageTransform;

			StageDim = new Size( Stage.Width, Stage.Height );
		}

		protected override Size MeasureOverride( Size availableSize )
		{
			// Scale up or down the icon if possible
			if( Stage != null && !( double.IsInfinity( availableSize.Width ) || double.IsInfinity( availableSize.Height ) ) )
			if ( !availableSize.Equals( StageDim ) )
			{
				ScaleStage( SizeGiven = availableSize );
			}

			return base.MeasureOverride( availableSize );
		}

		private void ScaleStage()
		{
			if ( SizeGiven == null || StageDim == null ) return;
			ScaleStage( SizeGiven );
		}

		virtual protected void ScaleStage( Size availableSize )
		{
			if ( !AutoScale ) return;
			double RScale = 1.0;

			if ( availableSize.Height < availableSize.Width )
			{
				RScale = availableSize.Height / StageDim.Height;
			}
			else
			{
				RScale = availableSize.Width / StageDim.Width;
			}

			if ( availableSize.Width < StageDim.Width )
			{
				StageTransform.TranslateX = 0.5 * ( RScale * StageDim.Width - StageDim.Width );
			}

			if ( availableSize.Height < StageDim.Height )
			{
				StageTransform.TranslateY = 0.5 * ( RScale * StageDim.Height - StageDim.Height );
			}

			StageTransform.ScaleX = StageTransform.ScaleY = RScale;
		}
	}
}
