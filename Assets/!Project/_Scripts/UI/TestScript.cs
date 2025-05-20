// TestLevelUpUI.cs
using UnityEngine;
using System.Collections.Generic; // List<T> i�in

public class TestLevelUpUI : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) // L tu�una bas�nca paneli g�ster
        {
            if (UIManager.Instance != null)
            {
                List<UIManager.SpellUpgradeChoice> testChoices = new List<UIManager.SpellUpgradeChoice>();

                testChoices.Add(new UIManager.SpellUpgradeChoice
                {
                    spellName = "Ate� Topu G��lendirmesi",
                    description = "Ate� topu hasar�n� %25 art�r�r.",
                    choiceIndex = 0 // Bu, sizin sisteminizde bir Spell ID veya Upgrade ID olabilir
                });

                testChoices.Add(new UIManager.SpellUpgradeChoice
                {
                    spellName = "H�zl� Ko�u",
                    description = "Hareket h�z�n� 5 saniyeli�ine art�r�r.",
                    choiceIndex = 1
                });

                testChoices.Add(new UIManager.SpellUpgradeChoice
                {
                    spellName = "Can Yenileme",
                    description = "An�nda 20 can yeniler.",
                    choiceIndex = 2
                });
                // �sterseniz daha az se�enek de g�nderebilirsiniz, UIManager kullan�lmayan slotlar� gizleyecektir.

                UIManager.Instance.ShowLevelUpPanel(testChoices);
            }
            else
            {
                Debug.LogError("UIManager.Instance is null!");
            }
        }
    }
}
