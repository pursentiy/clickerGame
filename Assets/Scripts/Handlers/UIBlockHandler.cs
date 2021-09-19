using UnityEngine;
using UnityEngine.UI;

namespace Handlers
{
    public class UIBlockHandler : MonoBehaviour
    {
        [SerializeField] private Image _blockImage;

        public void BlockUserInput(bool value)
        {
            _blockImage.raycastTarget = value;
        }
    }
}
