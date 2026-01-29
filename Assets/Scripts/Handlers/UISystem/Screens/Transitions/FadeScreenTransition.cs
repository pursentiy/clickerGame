using System;
using UnityEngine;

namespace Handlers.UISystem.Screens.Transitions
{
    public class FadeScreenTransition : UIScreenTimeBasedTransitionBase
    {
        private const float Duration = 0.35f; // Увеличил чуть-чуть для плавности
        private const float MoveOffset = 40f;

        private CanvasGroup _fromCanvas;
        private CanvasGroup _toCanvas;
        private RectTransform _fromRect;
        private RectTransform _toRect;
    
        private Vector2 _fromInitialPos;
        private Vector2 _toInitialPos;

        public override float TransitionTime => Duration;

        public FadeScreenTransition(Type toScreenType, IScreenContext context) 
            : base(toScreenType, context, false) { }

        public override void Prepare(UIScreenBase fromScreen, UIScreenBase toScreen, bool forward)
        {
            _fromCanvas = fromScreen.GetComponent<CanvasGroup>() ?? fromScreen.gameObject.AddComponent<CanvasGroup>();
            _toCanvas = toScreen.GetComponent<CanvasGroup>() ?? toScreen.gameObject.AddComponent<CanvasGroup>();
        
            _fromRect = fromScreen.GetComponent<RectTransform>();
            _toRect = toScreen.GetComponent<RectTransform>();

            _fromInitialPos = _fromRect.anchoredPosition;
            _toInitialPos = _toRect.anchoredPosition;

            // Блокируем клики на старте
            _toCanvas.blocksRaycasts = false;
            _fromCanvas.blocksRaycasts = false;
        }

        public override void DoTransition(float t, bool forward)
        {
            // 1. Применяем Easing к входящему значению t (0..1)
            // Для появления лучше всего подходит EaseOutSine или EaseOutCubic
            float easedT = EaseOutSine(t);
        
            // 2. Рассчитываем прогресс для каждого экрана
            if (forward)
            {
                // Уходящий экран (просто затухает линейно или по Sine)
                _fromCanvas.alpha = 1f - t; 

                // Появляющийся экран (Fade + Move)
                _toCanvas.alpha = easedT;
                var yOffset = Mathf.Lerp(-MoveOffset, 0, easedT);
                _toRect.anchoredPosition = _toInitialPos + new Vector2(0, yOffset);
            }
            else
            {
                // Обратная анимация (возврат назад)
                _toCanvas.alpha = 1f - t;
            
                _fromCanvas.alpha = easedT;
                var yOffset = Mathf.Lerp(MoveOffset, 0, easedT); // При возврате можно ехать сверху вниз
                _fromRect.anchoredPosition = _fromInitialPos + new Vector2(0, yOffset);
            }
        }

        public override void OnComplete(UIScreenBase fromScreen, UIScreenBase toScreen, bool forward)
        {
            // Устанавливаем финальные состояния без погрешностей float
            if (forward)
            {
                SetFinalState(_fromCanvas, _fromRect, _fromInitialPos, 0f, false);
                SetFinalState(_toCanvas, _toRect, _toInitialPos, 1f, true);
            }
            else
            {
                SetFinalState(_toCanvas, _toRect, _toInitialPos, 0f, false);
                SetFinalState(_fromCanvas, _fromRect, _fromInitialPos, 1f, true);
            }
        }

        private void SetFinalState(CanvasGroup cg, RectTransform rt, Vector2 pos, float alpha, bool interactable)
        {
            cg.alpha = alpha;
            cg.blocksRaycasts = interactable;
            rt.anchoredPosition = pos;
        }

        // Вспомогательная функция EaseOutSine
        private float EaseOutSine(float t)
        {
            return Mathf.Sin(t * (Mathf.PI / 2f));
        }
    }
}