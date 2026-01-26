using Common.Widgets.Animations;
using Installers;
using RSG;
using UnityEngine;

namespace UI.Screens
{
    public abstract class ScreenBase : InjectableMonoBehaviour
    {
        [SerializeField] private ScaleWidget _scaleWidget;
        [SerializeField] private FadeWidget _fadeWidget;
        [SerializeField] private ScreenAnimationType _screenAnimationType;

        protected override void Awake()
        {
            base.Awake();

            PrepareScreenForAnimation();
        }

        protected virtual void Start()
        {
            ShowScreen();
        }

        protected virtual void PrepareScreenForAnimation()
        {
            switch (_screenAnimationType)
            {
                case ScreenAnimationType.Fade:
                    _fadeWidget.ResetWidget();
                    break;
                case ScreenAnimationType.Scale:
                    _scaleWidget.ResetWidget();
                    break;
            }
        }

        public virtual IPromise HideScreen()
        {
            return _screenAnimationType switch
            {
                ScreenAnimationType.Fade => _fadeWidget.Hide(),
                ScreenAnimationType.Scale => _scaleWidget.Hide(),
                _ => Promise.Resolved()
            };
        }

        public virtual IPromise ShowScreen()
        {
            return _screenAnimationType switch
            {
                ScreenAnimationType.Fade => _fadeWidget.Show(),
                ScreenAnimationType.Scale => _scaleWidget.Show(),
                _ => Promise.Resolved()
            };
        }
    }

    public enum ScreenAnimationType
    {
        Fade,
        Scale
    }
}