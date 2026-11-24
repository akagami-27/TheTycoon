using System;

namespace TheTycoon.UI.Dashboard
{
    /// <summary>
    /// View abstraction for the corporate header top bar.
    /// Decouples presentation logic from Unity GameObject/MonoBehaviour specifics.
    /// </summary>
    public interface ITopBarView
    {
        void SetCompanyName(string companyName);
        void SetDate(string dateText);
        void SetReputation(int reputation);
        void SetAdvanceButtonInteractable(bool interactable);
        event Action OnAdvanceClicked;
    }
}
