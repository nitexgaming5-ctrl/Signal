using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Signal.Game.Tests
{
    public sealed class WorkshopUpgradePanelTests
    {
        [Test]
        public void SupplierOfferOpensAndPurchaseRefreshesInstalledState()
        {
            var host = new GameObject("WorkshopUpgradePanelTestHost");
            WorkshopUpgradeDefinition definition = ScriptableObject.CreateInstance<WorkshopUpgradeDefinition>();
            EventSystem eventSystemBefore = EventSystem.current;
            WorkshopUpgradePanel panel = host.AddComponent<WorkshopUpgradePanel>();
            bool acquired = false;
            int purchaseCalls = 0;

            try
            {
                panel.Bind(definition, "Rhea",
                    () => acquired ? "Purchased and installed." : "Available now.",
                    () => acquired,
                    () => !acquired,
                    () =>
                    {
                        purchaseCalls++;
                        acquired = true;
                        return "Compact Sewing Machine installed.";
                    });

                panel.Open();

                Assert.That(panel.IsOpen, Is.True);
                Assert.That(host.transform.Find("WorkshopUpgradeCanvas"), Is.Not.Null);

                Button purchaseButton = null;
                foreach (Button button in host.GetComponentsInChildren<Button>(true))
                {
                    Text label = button.GetComponentInChildren<Text>();
                    if (label != null && label.text.StartsWith("BUY & INSTALL"))
                    {
                        purchaseButton = button;
                        break;
                    }
                }

                Assert.That(purchaseButton, Is.Not.Null);
                Assert.That(purchaseButton.interactable, Is.True);
                purchaseButton.onClick.Invoke();

                Assert.That(purchaseCalls, Is.EqualTo(1));
                Assert.That(acquired, Is.True);
                Assert.That(purchaseButton.interactable, Is.False);
                Assert.That(purchaseButton.GetComponentInChildren<Text>().text, Is.EqualTo("PURCHASED  /  INSTALLED"));

                panel.Close();
                Assert.That(panel.IsOpen, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(host);
                Object.DestroyImmediate(definition);
                if (eventSystemBefore == null && EventSystem.current != null)
                    Object.DestroyImmediate(EventSystem.current.gameObject);
            }
        }
    }
}
