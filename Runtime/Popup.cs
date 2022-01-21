using System;
using System.Collections;
using Events.Runtime;
using UnityEngine;

namespace ScreenSystem.Runtime
{
    public abstract class Popup : MonoBehaviour
    {
        public bool animateIn = true;
        public bool animateOut = true;
        public float animateTime = 0.35f;
        public PopupPriority priority = PopupPriority.Medium;
        public PopupCanvasController PopupCanvasController { get; set; }
        
        public virtual void OnEnable()
        {
            if (animateIn)
            {
                StartCoroutine(Fade(true, InternalOpened));
            }
            else
            {
                InternalOpened();
            }
        }

        private void InternalOpened()
        {
            EventManager.TriggerEvent(new PopupOpenedEvent(this));
        }

        public virtual void Close()
        {
            if (animateOut)
            {
                StartCoroutine(Fade(false, InternalClose));
            }
            else
            {
                InternalClose();
            }
        }

        private IEnumerator Fade(bool fadeIn, Action onComplete = null)
        {
            var target = fadeIn ? 1f : 0f;
            var start = fadeIn ? 0f : 1f;
            var startTime = Time.time;
            while (Math.Abs(transform.localScale.x - target) > 0.01f)
            {
                var time = (Time.time - startTime) / animateTime;
                transform.localScale = Vector3.one * Mathf.SmoothStep(start, target, time);
                yield return null;
            }
            
            transform.localScale = Vector3.one * target;
            onComplete?.Invoke();
        }

        private void InternalClose()
        {
            var popup = this;
            Destroy(gameObject);
            EventManager.TriggerEvent(new PopupClosedEvent(popup, PopupCanvasController));
        }
    }
}
