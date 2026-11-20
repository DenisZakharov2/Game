using UnityEngine;
using UnityEngine.EventSystems;
using TimeMachine.Gameplay;

namespace TimeMachine.UI
{
    public class AssemblyPartDragItem : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private string partId;
        [SerializeField] private AssemblyWorkbenchPresenter presenter;

        public string PartId => partId;

        public void Configure(string id, AssemblyWorkbenchPresenter ownerPresenter)
        {
            partId = id;
            presenter = ownerPresenter;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (presenter != null)
            {
                presenter.SelectPartFromTray(partId);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (presenter != null)
            {
                presenter.BeginPartDrag(partId, eventData.position);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (presenter != null)
            {
                presenter.UpdatePartDrag(eventData.position);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (presenter != null)
            {
                presenter.EndPartDrag(partId, eventData.position);
            }
        }
    }
}
