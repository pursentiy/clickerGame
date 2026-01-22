namespace Common.Widgets.ContainerScaler
{
    public interface IScalableWidget
    {
        void UpdateWidget(bool byForce = false);
        void AnimateWidget(bool enable);
    }
}