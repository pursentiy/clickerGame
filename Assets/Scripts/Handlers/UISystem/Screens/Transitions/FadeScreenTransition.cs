using System;
using Extensions;
using UnityEngine;
using Zenject;

namespace Handlers.UISystem.Screens.Transitions
{
    public class FadeScreenTransition : UIScreenTimeBasedTransitionBase
    {
        [Inject] private ScreenTransitionParticlesHandler _particlesHandler;
        
        private const float OutDuration = 0.35f; // Время на исчезновение
        private const float InDuration = 0.35f;  // Время на появление
        private const float MiddlePause = 0.2f;  // Та самая пауза 0.2с
        private const float ParticlesDelayMainTransitionTime = 0.15f; 
        
        private const float MoveOffset = 120f;
        private const float ScaleEffect = 0.05f;

        private CanvasGroup _fromCanvas;
        private CanvasGroup _toCanvas;
        private RectTransform _fromRect;
        private RectTransform _toRect;
    
        private Vector2 _fromInitialPos;
        private Vector2 _toInitialPos;
        private bool _particlesPlayed;

        // Общее время теперь включает задержку
        public override float TransitionTime => ParticlesDelayMainTransitionTime + OutDuration + MiddlePause + InDuration;

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
            if (!_particlesPlayed)
            {
                _particlesHandler.PlayParticles();
                _particlesPlayed = true;
            }

            float currentTime = t * TransitionTime;

            // 1. Ожидание частиц
            if (currentTime < ParticlesDelayMainTransitionTime) return;

            // 2. Фаза ухода (Fade Out)
            float outStartTime = ParticlesDelayMainTransitionTime;
            if (currentTime >= outStartTime && currentTime < outStartTime + OutDuration)
            {
                float t_out = (currentTime - outStartTime) / OutDuration;
                float eased = EaseInQuart(t_out);

                _fromCanvas.alpha = 1f - t_out;
                var fromY = Mathf.Lerp(0, -MoveOffset * 0.5f, eased);
                _fromRect.anchoredPosition = _fromInitialPos + new Vector2(0, fromY);
                _fromRect.localScale = Vector3.one * (1f - ScaleEffect * eased);
                return;
            }

            // 3. Фаза паузы
            float pauseStartTime = outStartTime + OutDuration;
            if (currentTime >= pauseStartTime && currentTime < pauseStartTime + MiddlePause)
            {
                _fromCanvas.alpha = 0f; // Убеждаемся, что старый скрыт
                _toCanvas.alpha = 0f;   // Убеждаемся, что новый еще не виден
                return;
            }

            // 4. Фаза появления (Fade In)
            float inStartTime = pauseStartTime + MiddlePause;
            if (currentTime >= inStartTime)
            {
                float t_in = (currentTime - inStartTime) / InDuration;
                t_in = Mathf.Clamp01(t_in);
                float eased = EaseOutBack(t_in);

                _toCanvas.alpha = t_in;
                var toY = Mathf.Lerp(MoveOffset, 0, eased);
                _toRect.anchoredPosition = _toInitialPos + new Vector2(0, toY);
                _toRect.localScale = Vector3.one * ((1f - ScaleEffect) + ScaleEffect * eased);
            }
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