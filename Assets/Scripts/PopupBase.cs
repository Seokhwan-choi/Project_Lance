using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
	class PopupBase : MonoBehaviour
	{
		protected bool mClosing;
		Tween mVisibleTweener;
		PopupManager mManager;
		CanvasGroup mCanvasGroup;
		Action mOnCloseAction;

		public bool IsVisible => mCanvasGroup.alpha >= 1f && mCanvasGroup.blocksRaycasts;
		public bool Closing => mClosing;
		public void SetUp(PopupManager manager)
		{
			mClosing = false;
			mVisibleTweener = null;
			mManager = manager;
			mCanvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();

			//Localize();
		}

		public virtual void Localize()
		{
			//gameObject.Localize();
		}

		public virtual void RefreshRedDots()
        {

        }

		public void HideCloseButton()
        {
			var closeButton = gameObject.FindComponent<Button>("Button_Close");

			closeButton.gameObject.SetActive(false);
		}

		public void SetUpCloseAction()
		{
			Button closeButton = gameObject.FindComponent<Button>("Button_Close");
			closeButton.SetButtonAction(() => Close());

			Button modalButton = gameObject.FindComponent<Button>("Button_Modal");
			modalButton.SetButtonAction(() => Close());
		}

		public void SetOnCloseAction(Action onCloseAction)
		{
			mOnCloseAction = onCloseAction;
		}

		public void SetTitleText(string title)
		{
			var textTitle = gameObject.FindComponent<TextMeshProUGUI>("Text_Title");

			textTitle.text = title;
		}

		public void SetAlpha(float alpha)
		{
			mCanvasGroup.alpha = alpha;
		}

		public virtual void OnBackButton(bool immediate = false, bool hideMotion = true)
		{
			Close(immediate, hideMotion);
		}

		public virtual void Close(bool immediate = false, bool hideMotion = true)
		{
			if (mClosing)
				return;

			mOnCloseAction?.Invoke();

			mManager.RemovePopup(this);

			if (immediate)
			{
				transform.DOKill();

				gameObject.Destroy();
			}
			else
			{
				if (hideMotion)
					PlayHideMotion();

				mClosing = true;

				SetVisible(false, 0.2f, () =>
				{
					transform.DOKill();

					gameObject.Destroy();
				});
			}
		}

		public void SetVisible(bool visible, float duration = 0, Action onFinish = null)
		{
			if (duration == 0 || this.isActiveAndEnabled == false)
			{
				mVisibleTweener?.Rewind();
				mVisibleTweener?.Kill();
				mVisibleTweener = null;

				mCanvasGroup.blocksRaycasts = !visible;
				mCanvasGroup.alpha = visible ? 1 : 0;

				onFinish?.Invoke();
			}
			else
			{
				mCanvasGroup.blocksRaycasts = true;

				float startValue = visible ? 0f : 1f;
				float endvalue = visible ? 1f : 0f;

				mVisibleTweener = DOTween.To(SetAlpha, startValue, endvalue, duration)
					.SetUpdate(isIndependentUpdate: true)
					.OnComplete(
					() =>
					{
						onFinish?.Invoke();

						mCanvasGroup.blocksRaycasts = visible;
					});

				mVisibleTweener.timeScale = 1f;
			}
		}

		public void PlayShowMotion()
		{
			transform.DOScale(Vector3.one, 0.5f)
				.SetEase(Ease.OutBack)
				.SetUpdate(isIndependentUpdate: true)
				.ChangeStartValue(Vector3.zero).timeScale = 1f;

			SetVisible(visible: true, 0.5f);

			SoundPlayer.PlayShowPopup();
		}

		public void PlayHideMotion()
		{
			transform.DOScale(Vector3.zero, 0.5f)
				.SetEase(Ease.InBack)
				.SetUpdate(isIndependentUpdate: true)
				.ChangeStartValue(Vector3.one).timeScale = 1f;

			SoundPlayer.PlayHidePopup();
		}

		public virtual void OnClose()
		{
			SoundPlayer.PlayUIButtonTouchSound();

			this.Close();
		}
	}
}