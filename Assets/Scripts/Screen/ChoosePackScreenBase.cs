using Handlers;
using Screen.SubElements;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Screen
{
    public class ChoosePackScreenBase : ScreenBase
    {
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private ProgressHandler _progressHandler;
        
        [SerializeField] private PackEnterWidgetHandler _packEnterWidgetPrefab;
        [SerializeField] private RectTransform _levelEnterPopupsParentTransform;
        [SerializeField] private HorizontalLayoutGroup _horizontalLayoutGroupPrefab;

        private HorizontalLayoutGroup _horizontalGroup;
        private void Start()
        {
            InitializeLevelsButton();
        }

        private void InitializeLevelsButton()
        {
            var packParams = _progressHandler.GetCurrentProgress();
            var index = 0;
            packParams.ForEach(levelParams =>
            {
                if(_horizontalGroup == null || index % 2 == 0)
                    _horizontalGroup = Instantiate(_horizontalLayoutGroupPrefab, _levelEnterPopupsParentTransform);
                
                var enterButton = Instantiate(_packEnterWidgetPrefab, _horizontalGroup.transform);
                enterButton.Initialize(levelParams.PackName, levelParams.PackImage, levelParams.PackNumber, levelParams.PackPlayable,
                    () =>
                    {
                        _progressHandler.CurrentPackNumber = levelParams.PackNumber;
                        _screenHandler.ShowChooseLevelScreen();
                    });
                index++;
            });
        }
    }
}