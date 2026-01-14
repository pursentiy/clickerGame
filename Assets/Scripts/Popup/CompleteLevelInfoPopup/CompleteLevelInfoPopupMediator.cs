using System.Collections;
using Attributes;
using Common.Currency;
using DG.Tweening;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Popup.Common;
using RSG;
using Services;
using Services.FlyingRewardsAnimation;
using UnityEngine;
using Utilities.Disposable;
using Zenject;
using Random = UnityEngine.Random;

namespace Popup.CompleteLevelInfoPopup
{
    [AssetKey("UI Popups/CompleteLevelInfoPopupMediator")]
    public class CompleteLevelInfoPopupMediator : UIPopupBase<CompleteLevelInfoPopupView, CompleteLevelInfoPopupContext>
    {
        [Inject] private PlayerProgressService _playerProgressService;
        [Inject] private SoundHandler _soundHandler;
        [Inject] private PlayerCurrencyService _playerCurrencyService;
        [Inject] private FlyingUIRewardAnimationService _flyingUIRewardAnimationService;
        [Inject] private PlayerRepositoryService _playerRepositoryService;
        [Inject] private PlayerService _playerService;
        [Inject] private LocalizationService _localization;

        private Camera _textureCamera;
        private RenderTexture _renderTexture;
        private Coroutine _particlesCoroutine;
        private bool _currencyAcquired;

        public override IUIPopupAnimation Animation => new ScalePopupAnimation(View.MainTransform);

        public override void OnCreated()
        {
            base.OnCreated();
            
            View.StarsDisplayWidget.SetCurrency(_playerCurrencyService.Stars);
            
            AnimateTime(Context.TotalTime);
            AnimateStarsSequence(Context.TotalStars)
                .OnComplete(OnStarsAnimated)
                .KillWith(this);
            
            TryPlayFireworksParticles(Context.TotalStars);
            
            View.BackgronudButton.onClick.MapListenerWithSound(GoToLevelsMenuScreen).DisposeWith(this);
            View.GoToLevelsChooseScreenButton.onClick.MapListenerWithSound(GoToLevelsMenuScreen).DisposeWith(this);
            _playerCurrencyService.StarsChangedSignal.MapListener(OnStarsUpdated).DisposeWith(this);
        }

        private void OnStarsUpdated(Stars earnedStars)
        {
            //TODO REPLACE TO FlyingUIRewardAnimationService AUTOMATICALLY UPDATE
            View.StarsDisplayWidget.AddCurrency(earnedStars);
        }

        private void OnStarsAnimated()
        {
            StartStarsFloating(Context.TotalStars);
            TryAcquireEarnedStars(Context.EarnedStars);
        }
    
        private void AnimateTime(float finalTime)
        {
            if (View.TimeText == null) return;

            var format = _localization.GetCommonValue("result_time");
            View.TimeText.text = string.Format(format, 0f);
            float displayTime = 0;
    
            var seq = DOTween.Sequence().KillWith(this);
    
            seq.Append(DOTween.To(() => displayTime, x => displayTime = x, finalTime, 1.5f)
                .OnUpdate(() => 
                {
                    if (View.TimeText != null) // Дополнительная проверка внутри цикла
                        View.TimeText.text = string.Format(format, displayTime);
                })
                .SetEase(Ease.OutQuad));

            seq.Append(View.TimeText.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f));
        }
        
        private Sequence AnimateStarsSequence(Stars totalStarsForLevel)
        {
            foreach (var s in View.Stars) 
                s.transform.localScale = Vector3.zero;

            var seq = DOTween.Sequence().KillWith(this);

            // --- ПЕРВАЯ ЗВЕЗДА (Слева, индекс 0) ---
            if (View.Stars.Length > 0)
            {
                var star = View.Stars[0];
                var isStarEarned = totalStarsForLevel >= 1;
                star.material = isStarEarned ? View.DefaultStareMaterial : View.GrayScaleStarMaterial;

                if (isStarEarned)
                {
                    seq.Append(star.transform.DOScale(1f, 0.5f)
                        .SetEase(Ease.OutBack));
                }
                else
                {
                    seq.Append(star.transform.DOScale(0.8f, 0.3f).SetEase(Ease.OutQuad));
                }
            }

            // --- ВТОРАЯ ЗВЕЗДА (Справа, индекс 2) ---
            if (View.Stars.Length > 2)
            {
                var star = View.Stars[2];
                var isStarEarned = totalStarsForLevel >= 3;
                star.material = isStarEarned ? View.DefaultStareMaterial : View.GrayScaleStarMaterial;
        
                if (isStarEarned)
                {
                    seq.Append(star.transform.DOScale(1f, 0.5f)
                        .SetEase(Ease.OutBack, 3.0f)); 
                }
                else
                {
                    seq.Append(star.transform.DOScale(0.8f, 0.3f).SetEase(Ease.OutQuad));
                }
            }

            // --- ТРЕТЬЯ ЗВЕЗДА (Центр, индекс 1) ---
            if (View.Stars.Length > 1)
            {
                var star = View.Stars[1];
                var isStarEarned = totalStarsForLevel >= 2;
                star.material = isStarEarned ? View.DefaultStareMaterial : View.GrayScaleStarMaterial;
        
                if (isStarEarned)
                {
                    seq.Append(star.transform.DOScale(1.3f, 0.6f)
                        .SetEase(Ease.OutBack, 5.0f));
                    seq.Join(star.transform.DOPunchRotation(new Vector3(0, 0, 15), 0.6f, 10, 1));
                }
                else
                {
                    seq.Append(star.transform.DOScale(0.8f, 0.3f).SetEase(Ease.OutQuad));
                }
            }

            return seq;
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

        private void TryAcquireEarnedStars(Stars earnedStarsForLevel, bool fast = false)
        {
            if (_currencyAcquired || earnedStarsForLevel <= 0)
                return;

            _currencyAcquired = true;
            if (fast)
            {
                _playerCurrencyService.AddStars(earnedStarsForLevel);
                _playerRepositoryService.SavePlayerSnapshot(_playerService.ProfileSnapshot);
            }
            else
            {
                VisualizeStarsFlight(earnedStarsForLevel);
            }
        }
        
        private IPromise VisualizeStarsFlight(Stars earnedStars, bool updateProfileValues = true)
        {
            if (earnedStars.Value <= 0)
                return Promise.Resolved();
            
            var context = new FlyingUIRewardAnimationContext(
                new ICurrency[]{earnedStars}, 
                View.FlyingRewardsContainer, 
                new Vector3[] {View.StarsFlightStartPlace.position},
                new Vector3[] {View.StarsDisplayWidget.AnimationTarget.position},
                updateProfileValues: updateProfileValues);
            
            return _flyingUIRewardAnimationService.PlayAnimation(context).CancelWith(this);
        }

        private void GoToLevelsMenuScreen()
        {
            Context.GoToMenuAction?.SafeInvoke();
            Hide();
        }
        
        private void OnDestroy()
        {
            TryAcquireEarnedStars(Context.EarnedStars, true);
            
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