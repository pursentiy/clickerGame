using System;
using Extensions;
using UnityEngine;
using Zenject;

namespace Handlers.UISystem.Screens.Transitions
{
    public class FadeScreenTransition : UIScreenTimeBasedTransitionBase
    {
        [Inject] private ScreenTransitionParticlesHandler _particlesHandler;
        
        private const float Duration = 0.7f; 
        private const float MoveOffset = 120f;
        private const float ScaleEffect = 0.05f;
        
        // Поле задержки перед началом основной анимации
        private const float ParticlesDelayMainTransitionTime = 0.15f; 

        private CanvasGroup _fromCanvas;
        private CanvasGroup _toCanvas;
        private RectTransform _fromRect;
        private RectTransform _toRect;
    
        private Vector2 _fromInitialPos;
        private Vector2 _toInitialPos;
        private bool _particlesPlayed;

        // Общее время теперь включает задержку
        public override float TransitionTime => Duration + ParticlesDelayMainTransitionTime;

        public FadeScreenTransition(Type toScreenType, IScreenContext context)
            : base(toScreenType, context, false)
        {
            this.Inject();
        }

        public override void Prepare(UIScreenBase fromScreen, UIScreenBase toScreen, bool forward)
        {
            _fromCanvas = fromScreen.GetComponent<CanvasGroup>() ?? fromScreen.gameObject.AddComponent<CanvasGroup>();
            _toCanvas = toScreen.GetComponent<CanvasGroup>() ?? toScreen.gameObject.AddComponent<CanvasGroup>();
        
            _fromRect = fromScreen.GetComponent<RectTransform>();
            _toRect = toScreen.GetComponent<RectTransform>();

            _fromInitialPos = _fromRect.anchoredPosition;
            _toInitialPos = _toRect.anchoredPosition;

            _toCanvas.alpha = 0f;
            _toCanvas.blocksRaycasts = false;
            _fromCanvas.blocksRaycasts = false;
            
            toScreen.transform.SetAsLastSibling();
            _particlesPlayed = false;
        }

        public override void DoTransition(float t, bool forward)
        {
            // 1. Сначала запускаем частицы (сразу при старте)
            if (!_particlesPlayed)
            {
                _particlesHandler.PlayParticles();
                _particlesPlayed = true;
            }

            // 2. Рассчитываем локальное время для анимации (от 0 до 1 после задержки)
            // Общее время t здесь в секундах (если базовый класс передает нормализованное 0..1, 
            // то нужно предварительно умножить на TransitionTime)
            
            float currentTime = t * TransitionTime; // Текущее время в секундах
            
            if (currentTime < ParticlesDelayMainTransitionTime)
            {
                return;
            }

            // Нормализуем t_anim, чтобы она пробегала от 0 до 1 строго за время Duration
            float t_anim = (currentTime - ParticlesDelayMainTransitionTime) / Duration;
            t_anim = Mathf.Clamp01(t_anim);
            
            // Используем разные кривые для ухода и появления
            float outEased = EaseInQuart(t_anim);
            float inEased = EaseOutBack(t_anim);
            
            // 1. Старый экран плавно уходит вниз и затухает
            _fromCanvas.alpha = Mathf.Lerp(1f, 0f, t_anim * 1.5f);
            var fromY = Mathf.Lerp(0, -MoveOffset * 0.5f, outEased);
            _fromRect.anchoredPosition = _fromInitialPos + new Vector2(0, fromY);
            _fromRect.localScale = Vector3.one * (1f - ScaleEffect * outEased);

            // 2. Новый экран выезжает сверху
            _toCanvas.alpha = Mathf.Clamp01(t_anim * 2f);
            var toY = Mathf.Lerp(MoveOffset, 0, inEased);
            _toRect.anchoredPosition = _toInitialPos + new Vector2(0, toY);
            _toRect.localScale = Vector3.one * ((1f - ScaleEffect) + ScaleEffect * inEased);
        }

        public override void OnComplete(UIScreenBase fromScreen, UIScreenBase toScreen, bool forward)
        {
            SetFinalState(_fromCanvas, _fromRect, _fromInitialPos, 0f, false);
            SetFinalState(_toCanvas, _toRect, _toInitialPos, 1f, true);
            
            _fromRect.localScale = Vector3.one;
            _toRect.localScale = Vector3.one;
        }

        private void SetFinalState(CanvasGroup cg, RectTransform rt, Vector2 pos, float alpha, bool interactable)
        {
            cg.alpha = alpha;
            cg.blocksRaycasts = interactable;
            rt.anchoredPosition = pos;
        }

        private float EaseInQuart(float t) => t * t * t * t;

        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1;
            return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
        }
    }
}