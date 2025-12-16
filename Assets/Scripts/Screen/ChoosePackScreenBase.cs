using Handlers;
using Screen.SubElements;
using Services;
using Storage;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Screen
{
    public class ChoosePackScreenBase : ScreenBase
    {
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private PlayerProgressService _playerProgressService;
        [Inject] private FiguresStorageData _figuresStorageData;
        
        [SerializeField] private PackEnterWidgetHandler _packEnterWidgetPrefab;
        [SerializeField] private RectTransform _levelEnterPopupsParentTransform;
        [SerializeField] private HorizontalLayoutGroup _horizontalLayoutGroupPrefab;

        private HorizontalLayoutGroup _horizontalGroup;
        private void Start()
        {
            InitializePackButtons();
        }

        private void InitializePackButtons()
        {
            var currentPackParams = _playerProgressService.GetPackParams();
            var index = 0;
            currentPackParams.ForEach(packParams =>
            {
                if(_horizontalGroup == null || index % 2 == 0)
                    _horizontalGroup = Instantiate(_horizontalLayoutGroupPrefab, _levelEnterPopupsParentTransform);
                
                var enterButton = Instantiate(_packEnterWidgetPrefab, _horizontalGroup.transform);
                enterButton.Initialize(_figuresStorageData.GetPackParamsData(packParams.PackNumber).PackName, _figuresStorageData.GetPackParamsData(packParams.PackNumber).PackImage, packParams.PackNumber, _playerProgressService.IsPackAvailable(packParams.PackNumber),
                    () =>
                    {
                        _playerProgressService.CurrentPackNumber = packParams.PackNumber;
                        _screenHandler.ShowChooseLevelScreen();
                    });
                index++;
            });
        }
    }
}