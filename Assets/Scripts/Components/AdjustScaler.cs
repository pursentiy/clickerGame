using System.Collections;
using Extensions;
using UnityEngine;

namespace Components
{
    public class AdjustScaler : MonoBehaviour
    {
        [SerializeField] private RectTransform _targetTransform;

        [Tooltip(
            "Defines how target transform adjust in scaler transform.\n" +
            "Scaler type In = target transform fully contains in scaler transform\n" +
            "Scaler type Out = target transform fully hover scaler transform"
        )]
        [SerializeField]
        private AdjustScalerType _scalerType;

        private void OnEnable()
        {
            Adjust(false);
        }

        public void Adjust(bool instant)
        {
            if (instant)
            {
                AdjustScale();
            }
            else
            {
                StartCoroutine(AdjustCoroutine());
            }
        }

        private IEnumerator AdjustCoroutine()
        {
            yield return null;
            AdjustScale();
        }

        private void AdjustScale()
        {
            if (_scalerType == AdjustScalerType.In)
            {
                GetComponent<RectTransform>().AdjustScale(_targetTransform);
            }
            else
            {
                GetComponent<RectTransform>().AdjustScaleMax(_targetTransform);
            }
        }
    }

    public enum AdjustScalerType
    {
        In,
        Out
    }
}