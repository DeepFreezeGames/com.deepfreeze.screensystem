using Events.Runtime;
using UnityEngine;

namespace ScreenSystem.Runtime
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public class PopupCanvasController : MonoBehaviour
    {
        public Canvas Canvas { get; private set; }
        public CanvasGroup CanvasGroup { get; private set; }
        public Transform Transform { get; private set; }
        
        public PopupPriority Priority { get; private set; }

        private int _childCount = 0;
        
        private void Awake()
        {
            Canvas = GetComponent<Canvas>();
            CanvasGroup = GetComponent<CanvasGroup>();
            Transform = transform;
        }

        private void Start()
        {
            BindEvents();
        }

        public void Initialize(PopupPriority priority)
        {
            Priority = priority;
        }

        private void BindEvents()
        {
            EventManager.SubscribeEventListener<ScreenOpenedEvent>(OnScreenOpened);
            EventManager.SubscribeEventListener<ScreenClosedEvent>(OnScreenClosed);
        }

        private void OnScreenOpened(ScreenOpenedEvent screenOpenedEvent)
        {
            if (screenOpenedEvent.Screen.blocksPopups)
            {
                for (int i = 0; i < Transform.childCount; i++)
                {
                    Transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }

        private void OnScreenClosed(ScreenClosedEvent screenClosedEvent)
        {
            
        }
        
        public void LateUpdate()
        {
            if (_childCount != Transform.childCount)
            {
                _childCount = Transform.childCount;
                OnChildCountChanged();
            }
        }

        private void OnChildCountChanged()
        {
            
        }
    }
}