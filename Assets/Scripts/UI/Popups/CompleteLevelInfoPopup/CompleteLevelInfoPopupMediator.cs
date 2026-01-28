using System;
using System.Collections;
using Attributes;
using Common.Currency;
using DG.Tweening;
using Extensions;
using Handlers;
using Handlers.UISystem;
using RSG;
using Services;
using Services.FlyingRewardsAnimation;
using UI.Popups.CommonPopup;
using UnityEngine;
using Utilities.Disposable;
using Zenject;
using Random = UnityEngine.Random;

namespace UI.Popups.CompleteLevelInfoPopup
{
    [AssetKey("UI Popups/CompleteLevelInfoPopupMediator")]
    public class CompleteLevelInfoPopupMediator : UIPopupBase<CompleteLevelInfoPopupView, CompleteLevelInfoPopupContext>
    {
        [Inject] private readonly SoundHandler _soundHandler;
        [Inject] private readonly FlyingUIRewardAnimationService _flyingUIRewardAnimationService;
        [Inject] private readonly LocalizationService _localization;

        private Camera _textureCamera;
        private RenderTexture _renderTexture;
        private Coroutine _particlesCoroutine;
        private bool _currencyAcquired;

        public override IUIPopupAnimation Animation => new ScalePopupAnimation(View.MainTransform);

        public override void OnCreated()
        {
            base.OnCreated();
            
            var starsEarned = Math.Max(0, Context.CurrentStars - Context.InitialStars);
            var finalStarsCount = Context.PreRewardBalance + starsEarned;
            
            View.StarsDisplayWidget.SetCurrency(Context.PreRewardBalance);
            
            AnimateTime(Context.BeatTime);
            Promise.All(AnimateNewStarText(starsEarned, Context.LevelStatus), AnimateStarsSequence(Context.InitialStars, starsEarned))
                .Then(() =>
                {
                    TryPlayFireworksParticles(Context.CurrentStars);
                    StartStarsFloating(Context.CurrentStars);
                    return VisualizeStarsFlight(starsEarned);
                })
                .Then(() =>
                {
                    if (finalStarsCount > Context.PreRewardBalance)
                        View.StarsDisplayWidget.SetCurrency(finalStarsCount, true);
                })
                .CancelWith(this);
            
            View.BackgronudButton.onClick.MapListenerWithSound(ClosePopup).DisposeWith(this);
            View.GoToLevelsChooseScreenButton.onClick.MapListenerWithSound(ClosePopup).DisposeWith(this);
        }

        private void AnimateTime(float finalTime)
        {
            if (View.TimeText == null) return;

            var format = _localization.GetValue("result_time");
            View.TimeText.text = string.Format(format, 0f);
            float displayTime = 0;
    
            var seq = DOTween.Sequence().KillWith(this);
    
            seq.Append(DOTween.To(() => displayTime, x => displayTime = x, finalTime, 1.5f)
                .OnUpdate(() => 
                {
                    if (View.TimeText != null)
                        View.TimeText.text = string.Format(format, displayTime);
                })
                .SetEase(Ease.OutQuad));

            seq.Append(View.TimeText.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f));
        }
        
        private IPromise AnimateNewStarText(int earnedStars, CompletedLevelStatus status)
        {
            if (earnedStars <= 0 || status == CompletedLevelStatus.InitialCompletion)
            {
                View.NewStarTextCanvasGroup.alpha = 0;
                return Promise.Resolved();
            }

            var promise = new Promise();
            var textTransform = View.NewStarText.rectTransform;
            
            textTransform.DOKill();
            View.NewStarTextCanvasGroup.DOKill();
    
            View.NewStarText.text = $"+ {earnedStars}!";
            textTransform.localScale = Vector3.one * 0.5f;
            View.NewStarTextCanvasGroup.alpha = 0;

            var seq = DOTween.Sequence().KillWith(View.NewStarText.gameObject);
            
            seq.AppendInterval(0.3f);
            seq.Append(textTransform.DOScale(1.2f, 0.4f).SetEase(Ease.OutBack));
            seq.Join(View.NewStarTextCanvasGroup.DOFade(1f, 0.2f));
            seq.Append(textTransform.DOScale(1f, 0.2f).SetEase(Ease.InOutSine));
            seq.AppendInterval(1.2f);
            seq.Append(View.NewStarTextCanvasGroup.DOFade(0f, 0.3f));

            seq.OnComplete(() => promise.SafeResolve());

            return promise;
        }

        private IPromise AnimateStarsSequence(int initialsStars, int earnedStars)
        {
            foreach (var s in View.Stars)
                s.transform.localScale = Vector3.zero;

            var seq = DOTween.Sequence().KillWith(this);
            int totalStarsAfterWin = initialsStars + earnedStars;

            if (initialsStars > 0)
            {
                seq.AppendInterval(0.5f);
            }

            int[] animationOrder = { 0, 2, 1 };

            for (int i = 0; i < animationOrder.Length; i++)
            {
                int starIndex = animationOrder[i];
                if (starIndex >= View.Stars.Length) continue;

                var starImage = View.Stars[starIndex].GetComponent<UnityEngine.UI.Image>();
                int starThreshold = (starIndex == 1) ? 2 : (starIndex == 0 ? 1 : 3);

                bool isOldStar = starThreshold <= initialsStars;
                bool isNewStar = starThreshold > initialsStars && starThreshold <= totalStarsAfterWin;
                
                if (isOldStar)
                {
                    starImage.color = View.AlreadyEarnedStarColor;
                    starImage.transform.localScale = Vector3.one * (starIndex == 1 ? 1.3f : 1f);
                }
                else if (isNewStar)
                {
                    float targetScale = (starIndex == 1 ? 1.3f : 1f);
                    float overshoot = (starIndex == 1 ? 5.0f : 3.0f);

                    seq.Append(starImage.transform.DOScale(targetScale, 0.5f).SetEase(Ease.OutBack, overshoot));

                    if (starIndex == 1)
                        seq.Join(starImage.transform.DOPunchRotation(new Vector3(0, 0, 15), 0.6f, 10, 1));

                    seq.AppendInterval(0.1f);
                }
                else
                {
                    starImage.material = View.GrayScaleStarMaterial;
                    seq.Append(starImage.transform.DOScale(0.7f, 0.2f).SetEase(Ease.OutQuad));
                }
            }

            return seq.AsPromise();
        }
        
        private void StartStarsFloating(Stars totalStarsForLevel)
        {
            for (var i = 0; i < View.Stars.Length; i++)
            {
                var earned = i switch
                {
                    0 => totalStarsForLevel >= 1,
                    1 => totalStarsForLevel >= 2,
                    2 => totalStarsForLevel >= 3,
                    _ => false
                };
                
                if (earned) 
                    StartFloating(View.Stars[i].transform, i);
            }
        }

        private void StartFloating(Transform starTrm, int index)
        {
            if (starTrm == null) 
                return;
            
            // Небольшой разброс по времени и высоте, чтобы звезды летали вразнобой
            float duration = 1.5f + (index * 0.2f);
            float strength = 15f + (index * 5f); // Амплитуда движения вверх-вниз

            starTrm.DOLocalMoveY(starTrm.localPosition.y + strength, duration)
                .SetEase(Ease.InOutSine) // Плавный вход и выход
                .SetLoops(-1, LoopType.Yoyo) // -1 значит бесконечно, Yoyo - туда-обратно
                .KillWith(this);

            // Дополнительно можно добавить легкое покачивание
            starTrm.DORotate(new Vector3(0, 0, 5f), duration * 1.2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .KillWith(this);
        }
        
        private IPromise VisualizeStarsFlight(Stars earnedStars, bool updateProfileValues = true)
        {
            if (earnedStars.Value <= 0)
                return Promise.Resolved();
            
            var context = new FlyingUIRewardAnimationContext(
                new ICurrency[]{earnedStars}, 
                View.FlyingRewardsContainer, 
                new Vector3[] {View.StarsFlightStartPlace.position},
                new Vector3[] {View.StarsDisplayWidget.AnimationTarget.position});
            
            return _flyingUIRewardAnimationService.PlayAnimation(context).CancelWith(this);
        }

        private void ClosePopup()
        {
            Hide();
        }
        
        private void OnDestroy()
        {
            if (_particlesCoroutine != null)
                StopCoroutine(_particlesCoroutine);
        }
        
        private void TryPlayFireworksParticles(int totalStars)
        {
            if (totalStars < 2) 
                return;
            
            var countToPlay = View.FireworksParticles.Length;

            if (totalStars == 2)
            {
                countToPlay = Mathf.CeilToInt(View.FireworksParticles.Length / 3f);
            }
            
            var fireworksParticlesArray = new int[View.FireworksParticles.Length];
            for (var i = 0; i < View.FireworksParticles.Length; i++)
            {
                fireworksParticlesArray[i] = i;
            }
            
            if (gameObject != null && gameObject.activeInHierarchy)
                _particlesCoroutine = StartCoroutine(PlayParticles(CollectionExtensions.ShuffleCopy(fireworksParticlesArray), countToPlay, totalStars));
        }

        private IEnumerator PlayParticles(int[] shuffledPositions, int countToPlay, int totalStars)
        {
            for (var i = 0; i < countToPlay; i++)
            {
                yield return new WaitForSeconds(0.1f);
                var index = shuffledPositions[i];
                View.FireworksParticles[index].Play();
            }
            yield return new WaitForSeconds(Random.Range(1.3f, 3f));
            TryPlayFireworksParticles(totalStars);
        }
        
        
        
#if UNITY_EDITOR
        public void PlayStarsAnimation(Stars earnedStarsForLevel, bool updateProfileValues)
        {
            VisualizeStarsFlight(earnedStarsForLevel, updateProfileValues);
        }
#endif
    }
}