using System.Collections;
using Attributes;
using DG.Tweening;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Popup.Common;
using Services;
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
        }

        private void OnStarsAnimated()
        {
            StartStarsFloating(Context.TotalStars);
            TryAcquireEarnedStars(Context.EarnedStars);
        }
    
        private void AnimateTime(float finalTime)
        {
            var format = _localization.GetCommonValue("result_time");
            View.TimeText.text = string.Format(format, 0f);
            float displayTime = 0;
            var seq = DOTween.Sequence();
    
            seq.Append(DOTween.To(() => displayTime, x => displayTime = x, finalTime, 1.5f)
                .OnUpdate(() => 
                {
                    View.TimeText.text = string.Format(format, displayTime);
                })
                .SetEase(Ease.OutQuad)).KillWith(this);
    
            seq.Append(View.TimeText.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f));
        }
        
        private Sequence AnimateStarsSequence(int totalStarsForLevel)
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
        
        private void StartStarsFloating(int totalStarsForLevel)
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
            // Небольшой разброс по времени и высоте, чтобы звезды летали вразнобой
            float duration = 1.5f + (index * 0.2f);
            float strength = 15f + (index * 5f); // Амплитуда движения вверх-вниз

            starTrm.DOLocalMoveY(starTrm.localPosition.y + strength, duration)
                .SetEase(Ease.InOutSine) // Плавный вход и выход
                .SetLoops(-1, LoopType.Yoyo); // -1 значит бесконечно, Yoyo - туда-обратно

            // Дополнительно можно добавить легкое покачивание
            starTrm.DORotate(new Vector3(0, 0, 5f), duration * 1.2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void TryAcquireEarnedStars(int earnedStarsForLevel, bool fast = false)
        {
            if (_currencyAcquired || earnedStarsForLevel <= 0)
                return;

            _currencyAcquired = true;
            _playerCurrencyService.AddStars(earnedStarsForLevel);
            _playerRepositoryService.SavePlayerSnapshot(_playerService.ProfileSnapshot);
            
            if (!fast)
                View.StarsDisplayWidget.AddCurrency(earnedStarsForLevel);
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
    }
}