using UnityEngine;
using UnityEngine.EventSystems;

public class CompanyCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int companyIndex;
    public CompanySelect companySelect;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (companySelect != null)
            companySelect.ShowCompanyInfo(companyIndex);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (companySelect != null)
            companySelect.HideCompanyInfo();
    }
}
