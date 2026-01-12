using System.Collections.Generic;
using Attributes;
using Extensions;
using Handlers.UISystem;
using Popup.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;

namespace Popup.Universal
{
    [AssetKey("UI Popups/UniversalPopupMediator")]
     public class UniversalPopupMediator : UIPopupBase<UniversalPopupView, UniversalPopupContext>
     {
         public override IUIPopupAnimation Animation => new ScalePopupAnimation(View.MainGroup.GetComponent<RectTransform>());
         
         private readonly List<GameObject> _buttons = new List<GameObject>();
         
         public override void OnCreated()
         {
             base.OnCreated();

             if (Context.Message.IsNullOrEmpty())
             {
                 View.MessageBackgroundTransform.gameObject.SetActive(false);
             }
             else
             {
                 View.MessageBackgroundTransform.gameObject.SetActive(true);
                 View.MessageText.text = Context.Message;
                 View.MessageText.spriteAsset = Context.SpriteAsset;
             }
             
             var titleEmpty = Context.Title.IsNullOrEmpty();
             View.TitleBar.SetActive(!titleEmpty);

             if (Context.AllowBackgroundClose)
             {
                 View.BackgroundButton.onClick.MapListenerWithSound(Hide).DisposeWith(this);
             }
             
             View.CommonCrossButton.onClick.MapListenerWithSound(Hide).DisposeWith(this);

             if (!Context.Buttons.IsNullOrEmpty())
             {
                 Context.Buttons.Foreach(button =>
                 {
                     var buttonPrefab = View.ButtonStyles.GetValueOrDefault(button.Style, View.ButtonDefaultPrefab);
                     if (buttonPrefab == null)
                     {
                         buttonPrefab = View.ButtonDefaultPrefab;
                     }
                     var buttonObj = Instantiate(buttonPrefab.gameObject, View.ButtonsContainer);
                     _buttons.Add(buttonObj);
                     buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = button.Label;
                     buttonObj.GetComponent<Button>().onClick.MapListenerWithSound(() =>
                     {
                         Hide();
                         button.Callback?.Invoke();
                     }).DisposeWith(this);
                 });
             }

             View.Title.text = Context.Title;
         }
     }   
}
