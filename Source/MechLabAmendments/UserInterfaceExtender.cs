using TMPro;
using UnityEngine;
using System.Collections.Generic;



namespace MechLabAmendments
{
    public class UserInterfaceExtender
    {
        private static UserInterfaceExtender instance;
        public static UserInterfaceExtender Instance
        {
            get
            {
                if (instance == null) instance = new UserInterfaceExtender();
                return instance;
            }
        }

        public void ExtendStockInfoPopup()
        {

            GameObject MechLabStockInfoPopup = GameObject.Find("uixPrfPanl_ML_stockconfigModalPopup(Clone)");
            GameObject MechLabStockInfoPopupBackGO = MechLabStockInfoPopup.FindRecursive("uixPrfBttn_SIM_backButton-MANAGED");
            GameObject MechLabStockInfoPopupMainLayout = MechLabStockInfoPopup.FindRecursive("mainLayout");
            GameObject MechLabStockInfoPopupButtonLayout = MechLabStockInfoPopupMainLayout.FindRecursive("buttonLayout");
            GameObject MechLabStockInfoPopupDoneGO = MechLabStockInfoPopupButtonLayout.FindRecursive("uixPrfBttn_BASE_button2-MANAGED");
            RectTransform MechLabStockInfoPopupDoneGOTransform = (RectTransform)MechLabStockInfoPopupDoneGO.transform;

            //MechLabStockInfoPopupBackButton.SetActive(true);
            GameObject MechLabStockInfoPopupApplyGO = GameObject.Instantiate(MechLabStockInfoPopupDoneGO);
            MechLabStockInfoPopupApplyGO.name = "button-APPLY-STOCK-LOADOUT";
            RectTransform MechLabStockInfoPopupApplyButtonTransform = (RectTransform)MechLabStockInfoPopupApplyGO.transform;

            MechLabStockInfoPopupApplyButtonTransform.SetParent(MechLabStockInfoPopupButtonLayout.transform, false);
            //MechLabStockInfoPopupApplyButtonTransform.anchoredPosition = new Vector2(50, MechLabStockInfoPopupDoneButtonTransform.sizeDelta.y);

            // Set the button text
            TextMeshProUGUI applyStockLoadoutText = MechLabStockInfoPopupApplyGO.FindRecursive("bttn2_Text").GetComponent<TextMeshProUGUI>();
            applyStockLoadoutText.SetText("Apply");

            // Set up click event
            /*
            HBSDOTweenToggle MechLabStockInfoPopupApplyButton = MechLabStockInfoPopupApplyGO.GetComponent<HBSDOTweenToggle>();
            UnityEvent OnClickEvent = new UnityEvent();
            MechLabStockInfoPopupApplyButton.OnClicked = OnClickEvent;
            OnClickEvent.RemoveAllListeners();
            OnClickEvent.AddListener(OnMechLabStockInfoPopupApplyButtonClicked);
            */

            //MechLabStockInfoPopupApplyGO.SetActive(true);

            // Disable button if reverting to stock isn't possible atm
            //MechLabStockInfoPopupApplyButton.SetState(ButtonState.Unavailable, true);



            //HBSButton MechLabStockInfoPopupApplyButtonBTN = MechLabStockInfoPopupApplyButton.GetComponent<HBSButton>();
            //HorizontalLayoutGroup MechLabStockInfoPopupButtonLayoutGroup = MechLabStockInfoPopupButtonLayout.GetComponent<HorizontalLayoutGroup>();


        }

        private void OnMechLabStockInfoPopupApplyButtonClicked()
        {
            Logger.LogLine("[MechLabStockInfoPopup_SetData_POSTFIX] OnMechLabStockInfoPopupApplyButtonClicked");
        }
    }

    public static class GameObjectExtensions
    {
        public static List<GameObject> FindAllContains(this GameObject go, string name)
        {
            List<GameObject> gameObjects = new List<GameObject>();

            foreach (Transform t in go.transform)
            {
                if (t.name.Contains(name))
                {
                    gameObjects.Add(t.gameObject);
                }
            }

            return gameObjects;
        }

        public static GameObject FindRecursive(this GameObject gameObject, string checkName)
        {
            foreach (Transform t in gameObject.transform)
            {
                if (t.name == checkName) return t.gameObject;

                GameObject possibleGameObject = FindRecursive(t.gameObject, checkName);
                if (possibleGameObject != null) return possibleGameObject;
            }

            return null;
        }
    }
}
