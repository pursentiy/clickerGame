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
        private const float InDuration = 0.55f; // Время на появление
        private const float MiddlePause = 0.2f; // Та самая пауза 0.2с
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
        public override float TransitionTime =>
            ParticlesDelayMainTransitionTime + OutDuration + MiddlePause + InDuration;

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

            // 2. Фаза ухода (Аналог Hide из FadeWidget)
            float outStartTime = ParticlesDelayMainTransitionTime;
            if (currentTime >= outStartTime && currentTime < outStartTime + OutDuration)
            {
                float t_out = Mathf.Clamp01((currentTime - outStartTime) / OutDuration);

                // Используем EaseInCubic как в Hide()
                float eased = t_out * t_out * t_out;

                _fromCanvas.alpha = 1f - t_out;
                // Уходит вниз на MoveOffset
                var fromY = Mathf.Lerp(0, -MoveOffset, eased);
                _fromRect.anchoredPosition = _fromInitialPos + new Vector2(0, fromY);
                return;
            }

            // 3. Фаза паузы
            float pauseStartTime = outStartTime + OutDuration;
            if (currentTime >= pauseStartTime && currentTime < pauseStartTime + MiddlePause)
            {
                _fromCanvas.alpha = 0f;
                _toCanvas.alpha = 0f;
                return;
            }

            // 4. Фаза "Шикарного" появления (Аналог Show из FadeWidget)
            float inStartTime = pauseStartTime + MiddlePause;
            if (currentTime >= inStartTime)
            {
                float t_in = Mathf.Clamp01((currentTime - inStartTime) / InDuration);

                // Математический эквивалент Ease.OutCubic: 1 - (1 - t)^3
                float t_inv = 1f - t_in;
                float moveEased = 1f - (t_inv * t_inv * t_inv);

                // Fade в оригинале был Linear
                float fadeEased = t_in;

                // 1. Прозрачность (Linear)
                _toCanvas.alpha = fadeEased;

                // 2. Движение СНИЗУ ВВЕРХ (от -MoveOffset к 0)
                // Используем OutCubic для того самого "шикарного" торможения
                var currentY = Mathf.Lerp(-MoveOffset, 0, moveEased);
                _toRect.anchoredPosition = _toInitialPos + new Vector2(0, currentY);

                // 3. Микро-скейл для дополнительной глубины (опционально)
                _toRect.localScale = Vector3.Lerp(Vector3.one * 0.98f, Vector3.one, moveEased);
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
    }
}