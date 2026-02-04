using System;

namespace Services.ScreenBlocker
{
    public interface IUIBlockRef : IDisposable
    {
        bool IsDisposed { get; }
    }
    
    public class UIBlockRef<T> : IUIBlockRef where T : UIBlockerComponentBase
    {
        public bool IsDisposed { get; private set; } = false;
        private readonly UIBlocker<T> _blocker;
        public UIBlockRef(UIBlocker<T> blocker)
        {
            _blocker = blocker;
            
#if UNITY_EDITOR
            GenerateId();
#endif
        }
        
        public void Dispose()
        {
            _blocker.Utilize(this);
            IsDisposed = true;
        }
        
#if UNITY_EDITOR
        private static int _index = 0;

        public int Id { get; private set; }
        
        private void GenerateId()
        {
            Id = _index;
            _index++;
        }
#endif
    }
}