using System;
using System.Collections.Generic;
using Events.Runtime;
using UnityEngine;

namespace ScreenSystem.Runtime
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public class PopupCanvasController : MonoBehaviour
    {
        public Canvas canvas;
        public CanvasGroup canvasGroup;
        public Transform popupContainer;

        public PopupPriority Priority { get; private set; }
        public List<Popup> Popups { get; private set; } = new List<Popup>();
        public Popup ActivePopup { get; private set; }

        private void Awake()
        {
            if (canvas == null)
            {
                canvas = GetComponent<Canvas>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (popupContainer == null)
            {
                popupContainer = transform;
            }
            
            UpdatePopupSorting(false);
        }

        public void Initialize(PopupPriority priority)
        {
            Priority = priority;
        }

        public void AddPopup(Popup popup)
        {
            Popups.Add(popup);
        }

        public void RemovePopup(Popup popup)
        {
            Popups.Remove(popup);
        }

        public void UpdatePopupSorting(bool active)
        {
            /*Popups.Clear();
            for (var i = 0; i < popupContainer.childCount; i++)
            {
                var popup = popupContainer.GetChild(i).GetComponent<Popup>();
                if (popup != null)
                {
                    Popups.Add(popup);
                }
            }*/
            
            gameObject.SetActive(Popups.Count > 0 && active);

            Debug.Log($"{Priority.ToString("G")} popup count: {Popups.Count.ToString()}");
            
            ActivePopup = Popups.Count > 0 ? Popups[0] : null;
            
            if (Popups.Count == 0)
            {
                gameObject.SetActive(false);
                return;
            }
            
            for (var i = 0; i < Popups.Count; i++)
            {
                Popups[i].gameObject.SetActive(i == 0);
            }
        }
    }
}