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

        private int _childCount = 0;

        private bool IsBlocked
        {
            get
            {
                switch (Priority)
                {
                    case PopupPriority.Medium:
                        return ScreenManager.BlockMediumPriorityPopups;
                    case PopupPriority.High:
                        return ScreenManager.BlockHighPriorityPopups;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
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
        }

        private void Start()
        {
            BindEvents();
            UpdatePopupSorting();
        }

        public void Initialize(PopupPriority priority)
        {
            Priority = priority;
        }

        private void BindEvents()
        {
            //Popup Events
            EventManager.SubscribeEventListener<PopupSpawnedEvent>(OnPopupOpened);
            EventManager.SubscribeEventListener<PopupClosedEvent>(OnPopupClosed);
        }

        private void OnPopupOpened(PopupSpawnedEvent popupSpawnedEvent)
        {
            if (popupSpawnedEvent.PopupCanvasController != this)
            {
                return;
            }
            
            Popups.Add(popupSpawnedEvent.Popup);
            UpdatePopupSorting();
        }

        private void OnPopupClosed(PopupClosedEvent popupClosedEvent)
        {
            if (popupClosedEvent.PopupCanvasController != this)
            {
                return;
            }

            Popups.Remove(popupClosedEvent.Popup);
            UpdatePopupSorting();
        }

        private void UpdatePopupSorting()
        {
            gameObject.SetActive(Popups.Count > 0 || IsBlocked);
            ActivePopup = Popups.Count > 0 ? Popups[0] : null;
            
            if (Popups.Count == 0)
            {
                return;
            }
            
            for (var i = 0; i < Popups.Count; i++)
            {
                Popups[i].gameObject.SetActive(i == 0);
            }
        }
    }
}