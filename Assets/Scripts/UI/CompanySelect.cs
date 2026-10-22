using UnityEngine;
using UnityEngine.SceneManagement;

public class CompanySelect : MonoBehaviour
{
    public GameObject companyPanel;

    void Start()
    {
        if (GameSettings.OpenCompanySelectOnMenu)
        {
            GameSettings.OpenCompanySelectOnMenu = false;
            ShowCompanyPanel();
        }
    }

    public void ShowCompanyPanel()
    {
        if (companyPanel != null)
            companyPanel.SetActive(true);
    }

    public void HideCompanyPanel()
    {
        if (companyPanel != null)
            companyPanel.SetActive(false);
    }

    // 0=Freelancer, 1=UPS, 2=USPS, 3=FedEx
    public void ChooseCompany(int index)
    {
        GameSettings.SelectedCompany = (Company)Mathf.Clamp(index, 0, 3);
        SceneManager.LoadScene("level_2");
    }
}
